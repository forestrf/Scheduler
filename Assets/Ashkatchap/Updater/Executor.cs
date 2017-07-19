using DisruptorUnity3d;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		public class WorkerManager {
			private readonly FrameUpdater updater;
			private readonly AutoResetEvent waiter = new AutoResetEvent(false);
			private readonly Thread thread;

			internal readonly JobArray[] jobsToDo; // Only "thread" writes, others only read
			private readonly Worker[] workers;

			private readonly RingBuffer<QueuedJob> jobPool = new RingBuffer<QueuedJob>(16384);
			private readonly RingBuffer<KeyValuePair<QueuedJob, byte>> queuedJobsWantingToChangePriority = new RingBuffer<KeyValuePair<QueuedJob, byte>>(8192);
			private readonly RingBuffer<QueuedJob> queuedJobsToShedule = new RingBuffer<QueuedJob>(8192);
			private readonly Volatile.PaddedLong queuedJobIdCounter = new Volatile.PaddedLong();


			internal byte currentPriority = 0;
			internal byte currentPriorityStamp = 0;



			public WorkerManager(FrameUpdater updater, int numWorkers) {
				Logger.Info("Current Executor Version: 0.19");
				this.updater = updater;
				jobsToDo = new JobArray[256];
				for (int i = 0; i < jobsToDo.Length; i++) jobsToDo[i] = new JobArray();

				Thread.MemoryBarrier();

				workers = new Worker[numWorkers];
				for (int i = 0; i < workers.Length; i++) workers[i] = new Worker(this, i);
				Logger.Info("Spawned workers: " + workers.Length);

				thread = new Thread(()=> { SecureLaunchThread(ThreadMethod); });
				thread.Name = "Worker Feeder";
				thread.Priority = ThreadPriority.AboveNormal;
				thread.IsBackground = true;
				thread.Start();
			}

			internal void OnDestroy() {
				for (int i = 0; i < workers.Length; i++) workers[i].OnDestroy();
			}


			internal void SignalExecutor() {
				waiter.Set();
			}
			private void SignalWorkers() {
				for (int i = 0; i < workers.Length; i++) workers[i].waiter.Set();
			}



			// This can be called from any thread!!!!
			public JobReference RequestQueueMultithreadJobInstance(Job job, int numberOfIterations, byte priority) {
				QueuedJob queuedJob;
				UnityEngine.Profiling.Profiler.BeginSample("a");
				if (!jobPool.TryDequeue(out queuedJob)) {
					UnityEngine.Profiling.Profiler.BeginSample("a2");
					queuedJob = new QueuedJob();
					UnityEngine.Profiling.Profiler.EndSample();
				}
				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("b");
				queuedJob.Init(this, job, numberOfIterations, queuedJobIdCounter.AtomicAddAndGet(1), priority);
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("c");
				if (workers.Length == 0) {
				// If single thread, do the job already and return
					queuedJob.WaitForFinish();
				} else {
					int counterWaiting = 0;
					while (!queuedJobsToShedule.TryEnqueue(queuedJob)) {
						if (counterWaiting++ < 10) {
							Thread.SpinWait(100);
						} else {
							queuedJob.WaitForFinish();
							break;
						}
					}
					SignalExecutor();
				}
				UnityEngine.Profiling.Profiler.EndSample();
				return new JobReference(queuedJob);
			}
			
			internal void RequestSetJobToAllThreads(QueuedJob queuedJob) {
				RequestJobPriorityChange(queuedJob, 0);
			}

			internal void RequestJobPriorityChange(QueuedJob queuedJob, byte newPriority) {
				queuedJobsWantingToChangePriority.TryEnqueue(new KeyValuePair<QueuedJob, byte>(queuedJob, newPriority));
				SignalExecutor();
			}



			private void ThreadMethod() {
				while (true) {
					bool somethingAdded = false;
					bool somethingDeleted = false;
					int lowestPriority = int.MaxValue;

					// Search for finished jobs, to remove/pool them
					for (int p = 0; p < jobsToDo.Length; p++) {
						for (int i = jobsToDo[p].count - 1; i >= 0; i--) {
							if (jobsToDo[p].array[i].IsFinished()) {
								var jobToQueue = jobsToDo[p].array[i];
								jobsToDo[p].RemoveAtAuto(p, i);
								jobToQueue.insideJobsToDo = false;
								jobPool.TryEnqueue(jobToQueue);

								somethingDeleted = true;
							} else {
								i++;
							}
						}
					}

					// Add Max Priority jobs to the priority 0. They may not be in jobsToDo (if they are not finished)
					KeyValuePair<QueuedJob, byte> toUpdate;
					while (queuedJobsWantingToChangePriority.TryDequeue(out toUpdate)) {
						if (!toUpdate.Key.IsFinished()) {
							if (toUpdate.Key.insideJobsToDo) {
								jobsToDo[toUpdate.Key.priority].RemoveAuto(toUpdate.Key.priority, toUpdate.Key);
							} else {
								somethingAdded = true;
								lowestPriority = Math.Min(lowestPriority, toUpdate.Value);
							}
							jobsToDo[toUpdate.Value].AddAuto(toUpdate.Value, toUpdate.Key);
							toUpdate.Key.insideJobsToDo = true;
						}
					}

					// Add normal jobs from the queue (if they are not finished)
					QueuedJob toAdd;
					while (queuedJobsToShedule.TryDequeue(out toAdd)) {
						if (!toAdd.IsFinished()) {
							if (!toAdd.insideJobsToDo) {
								jobsToDo[toAdd.priority].AddAuto(toAdd.priority, toAdd);
								toAdd.insideJobsToDo = true;
								somethingAdded = true;
								lowestPriority = Math.Min(lowestPriority, toAdd.priority);
							}
						}
					}

					// write changes to ram if needed
					if (somethingAdded) {
						currentPriority = (byte) lowestPriority;
						currentPriorityStamp++;
						Thread.MemoryBarrier();
						SignalWorkers();
					} else if (somethingDeleted) {
						Thread.MemoryBarrier();
					}

					// Queues processed, wait until we have more work to do
					waiter.WaitOne();
				}
			}
		}

		internal partial class QueuedJob {
			// Variables only used by Executor. They will be readed and written ONLY by the executor
			internal bool insideJobsToDo = false;
			internal byte priority;

		}

		internal static void SecureLaunchThread(Action action) {
			try {
				action();
			}
			catch(Exception e) {
				Logger.Error(e.ToString());
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
