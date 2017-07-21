using Ashkatchap.Shared.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Profiling;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		public class WorkerManager {
			internal readonly JobArray[] jobsToDo; // Only "thread" writes, others only read
			private readonly Worker[] workers;

			private UnorderedList<QueuedJob> pool = new UnorderedList<QueuedJob>();
			
			internal byte lastActionStamp = 0;

			
			public WorkerManager(FrameUpdater updater, int numWorkers) {
				jobsToDo = new JobArray[256];
				for (int i = 0; i < jobsToDo.Length; i++) jobsToDo[i] = new JobArray();

				Thread.MemoryBarrier();

				workers = new Worker[numWorkers];
				for (int i = 0; i < workers.Length; i++) workers[i] = new Worker(this, i);
				Logger.Info("Spawned workers: " + workers.Length);

				updater.AddRecurrentUpdateCallbackInstance(Cleaner, QueueOrder.PreUpdate, 0);
				updater.AddRecurrentUpdateCallbackInstance(Cleaner, QueueOrder.Update, 0);
				updater.AddRecurrentUpdateCallbackInstance(Cleaner, QueueOrder.PostUpdate, 0);
				updater.AddRecurrentUpdateCallbackInstance(Cleaner, QueueOrder.PreLateUpdate, 0);
				updater.AddRecurrentUpdateCallbackInstance(Cleaner, QueueOrder.LateUpdate, 0);
				updater.AddRecurrentUpdateCallbackInstance(Cleaner, QueueOrder.PostLateUpdate, 0);
			}

			internal void OnDestroy() {
				for (int i = 0; i < workers.Length; i++) workers[i].OnDestroy();
			}

			
			private void SignalWorkers() {
				for (int i = 0; i < workers.Length; i++) workers[i].waiter.Set();
			}

			
			
			public JobReference QueueMultithreadJobInstance(Job job, ushort numberOfIterations, byte priority) {
				if (Thread.CurrentThread != mainThread) {
					Logger.Warn("WaitForFinish can only be called from the main thread");
					return default(JobReference);
				}
				var queuedJob = pool.Count > 0 ? pool.ExtractLast() : new QueuedJob();
				Profiler.BeginSample("b");
				queuedJob.Init(job, numberOfIterations, priority);
				Profiler.EndSample();

				Profiler.BeginSample("c");
				if (workers.Length == 0) {
					// If single thread, do the job already and return
					queuedJob.WaitForFinish();
				} else {
					jobsToDo[queuedJob.wantedPriority].AddAuto(queuedJob.wantedPriority, queuedJob);
					lastActionStamp++;
					Thread.MemoryBarrier();
					SignalWorkers();
				}
				Profiler.EndSample();
				return new JobReference(queuedJob);
			}
			
			internal void SetJobToAllThreads(QueuedJob queuedJob) {
				JobPriorityChange(queuedJob, 0);
			}

			internal void JobPriorityChange(QueuedJob queuedJob, byte newPriority) {
				if (Thread.CurrentThread != mainThread) {
					Logger.Warn("WaitForFinish can only be called from the main thread");
					return;
				}
				queuedJob.wantedPriority = newPriority;
				Thread.MemoryBarrier();

				jobsToDo[queuedJob.wantedPriority].AddAuto(queuedJob.wantedPriority, queuedJob);
				lastActionStamp++;
				Thread.MemoryBarrier();

				SignalWorkers();
			}



			private void Cleaner() {
				Profiler.BeginSample("Cleaner");
				// Search and Pool finished jobs
				for (int p = 0; p < jobsToDo.Length; p++) {
					for (int i = jobsToDo[p].count - 1; i >= 0; i--) {
						var job = jobsToDo[p].array[i];
						if (job.IsFinished()) {
							jobsToDo[p].RemoveAtAuto(p, i);
							pool.Add(job);
						}
					}
				}
				Profiler.EndSample();
			}
		}

		/// <summary>
		/// Not thread safe
		/// </summary>
		internal class JobArray {
			const int INITIAL_SIZE = 64;
			const int INCREMENT = 64;

			public QueuedJob[] array;
			public int count;

			public JobArray() {
				array = new QueuedJob[INITIAL_SIZE];
				count = 0;
			}

			private void IncreaseArray() {
				QueuedJob[] newArray = new QueuedJob[array.Length + INCREMENT];
				Logger.Trace("Array of multithreaded jobs increased from " + array.Length + " to " + newArray.Length);
				array.CopyTo(newArray, 0);
				array = newArray;
			}

			public void AddAuto(int queue, QueuedJob queuedJob) {
				if (queue == 0) {
					InsertDesplacingOlder(0, queuedJob);
				} else {
					Add(queuedJob);
				}
			}
			private void InsertDesplacingOlder(int index, QueuedJob queuedJob) {
				if (count == array.Length) IncreaseArray();

				for (int i = count - 1; i >= index; i--) array[i + 1] = array[i];
				array[index] = queuedJob;
				count++;
			}
			private void Add(QueuedJob queuedJob) {
				if (count == array.Length) IncreaseArray();
				array[count] = queuedJob;
				count++;
			}

			public void RemoveAuto(int queue, QueuedJob queuedJob) {
				if (queue == 0) {
					RemoveDisplacingOlder(queuedJob);
				} else {
					Remove(queuedJob);
				}
			}
			private void RemoveDisplacingOlder(QueuedJob queuedJob) {
				for (int i = 0; i < count; i++) {
					if (array[i] == queuedJob) {
						RemoveAtDisplacingOlder(i);
						return;
					}
				}
			}
			private void Remove(QueuedJob queuedJob) {
				for (int i = 0; i < count; i++) {
					if (array[i] == queuedJob) {
						RemoveAt(i);
						return;
					}
				}
			}

			public void RemoveAtAuto(int queue, int index) {
				if (queue == 0) {
					RemoveAtDisplacingOlder(index);
				} else {
					RemoveAt(index);
				}
			}
			private void RemoveAtDisplacingOlder(int index) {
				for (int i = index; i < count - 1; i++) array[i] = array[i + 1];
				array[count - 1] = null;
				count--;
			}
			private void RemoveAt(int index) {
				array[index] = array[count - 1];
				array[count - 1] = null;
				count--;
			}
		}
	}
}
