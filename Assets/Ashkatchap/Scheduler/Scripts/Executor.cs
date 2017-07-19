using Ashkatchap.Shared.Collections;
using System;
using System.Threading;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		public class WorkerManager {
			private readonly AutoResetEvent waiter = new AutoResetEvent(false);
			private readonly Thread thread;

			internal readonly JobArray[] jobsToDo; // Only "thread" writes, others only read
			private readonly Worker[] workers;

			private readonly ThreadSafeObjectPool<QueuedJob> jobPool = new ThreadSafeObjectPool<QueuedJob>(16384);
			private readonly ThreadSafeRingBuffer_MultiProducer_SingleConsumer<QueuedJob> queuedJobsWantingToChangePriority = new ThreadSafeRingBuffer_MultiProducer_SingleConsumer<QueuedJob>(8192);
			private readonly ThreadSafeRingBuffer_MultiProducer_SingleConsumer<QueuedJob> queuedJobsToShedule = new ThreadSafeRingBuffer_MultiProducer_SingleConsumer<QueuedJob>(8192);
			private readonly Volatile.PaddedInt queuedJobIdCounter = new Volatile.PaddedInt();


			internal byte currentPriority = 0;
			internal byte currentPriorityStamp = 0;



			public WorkerManager(int numWorkers) {
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
				thread.Interrupt();
				thread.Abort();
			}


			internal void SignalExecutor() {
				waiter.Set();
			}
			private void SignalWorkers() {
				for (int i = 0; i < workers.Length; i++) workers[i].waiter.Set();
			}



			// This can be called from any thread!!!!
			public JobReference RequestQueueMultithreadJobInstance(Job job, int numberOfIterations, byte priority) {
				UnityEngine.Profiling.Profiler.BeginSample("a");
				QueuedJob queuedJob = jobPool.Get();
				UnityEngine.Profiling.Profiler.EndSample();
				UnityEngine.Profiling.Profiler.BeginSample("b");
				queuedJob.Init(this, job, numberOfIterations, queuedJobIdCounter.InterlockedIncrement(), priority);
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("c");
				if (workers.Length == 0) {
					// If single thread, do the job already and return
					queuedJob.WaitForFinish();
				} else {
					queuedJobsToShedule.Enqueue(queuedJob);
					SignalExecutor();
				}
				UnityEngine.Profiling.Profiler.EndSample();
				return new JobReference(queuedJob);
			}
			
			internal void RequestSetJobToAllThreads(QueuedJob queuedJob) {
				RequestJobPriorityChange(queuedJob, 0);
			}

			internal void RequestJobPriorityChange(QueuedJob queuedJob, byte newPriority) {
				queuedJob.wantedPriority = newPriority;
				Thread.MemoryBarrier();
				queuedJobsWantingToChangePriority.Enqueue(queuedJob);
				SignalExecutor();
			}



			private void ThreadMethod() {
				while (true) {
					bool somethingAdded = false;
					bool somethingDeleted = false;
					int lowestPriority = int.MaxValue;

					// Search and Pool finished jobs
					for (int p = 0; p < jobsToDo.Length; p++) {
						for (int i = jobsToDo[p].count - 1; i >= 0; i--) {
							if (jobsToDo[p].array[i].IsFinished()) {
								var jobToRecycle = jobsToDo[p].array[i];
								jobsToDo[p].RemoveAtAuto(p, i);
								RecycleJobSingleThread(jobToRecycle);

								somethingDeleted = true;
							}
						}
					}

					// Add Max Priority jobs to the priority 0. They may not be in jobsToDo (if they are not finished)
					QueuedJob toUpdate;
					while (queuedJobsWantingToChangePriority.TryDequeue(out toUpdate)) {
						if (!toUpdate.IsFinished()) {
							if (toUpdate.insideJobsToDo) {
								jobsToDo[toUpdate.priority].RemoveAuto(toUpdate.priority, toUpdate);
							} else {
								somethingAdded = true;
								lowestPriority = Math.Min(lowestPriority, toUpdate.wantedPriority);
							}
							jobsToDo[toUpdate.wantedPriority].AddAuto(toUpdate.wantedPriority, toUpdate);
							toUpdate.insideJobsToDo = true;
						}
					}

					// Add normal jobs from the queue (if they are not finished)
					QueuedJob toAdd;
					while (queuedJobsToShedule.TryDequeue(out toAdd)) {
						if (!toAdd.insideJobsToDo) {
							if (!toAdd.IsFinished()) {
								jobsToDo[toAdd.priority].AddAuto(toAdd.priority, toAdd);
								toAdd.insideJobsToDo = true;
								somethingAdded = true;
								lowestPriority = Math.Min(lowestPriority, toAdd.priority);
							} else {
								RecycleJobSingleThread(toAdd);
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

			private void RecycleJobSingleThread(QueuedJob job) {
				job.insideJobsToDo = false;
				jobPool.Recycle(job);
			}
		}

		internal partial class QueuedJob {
			// Variables only used by Executor. They will be readed and written ONLY by the executor
			internal bool insideJobsToDo = false;
			internal byte priority;
			internal byte wantedPriority;

		}

		internal static void SecureLaunchThread(Action action, string name) {
			try {
				action();
			}
			catch(Exception e) {
				if (e.GetType() != typeof(ThreadInterruptedException) && e.GetType() != typeof(ThreadAbortException))
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
