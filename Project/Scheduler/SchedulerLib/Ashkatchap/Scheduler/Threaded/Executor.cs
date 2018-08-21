using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	internal partial class FrameUpdater {
		public class WorkerManager : IDisposable {
			internal volatile byte lastActionStamp = 0;
			internal readonly ThreadSafeQueue<int> jobsToDo;
			private readonly Worker[] workers;
			internal readonly Thread jobDistributor;
			internal AutoResetEvent waiter = new AutoResetEvent(true);
			internal readonly int jobsForWorkersLengthMask = (1 << 15) - 1;
			internal readonly Job[] jobsForWorkers = new Job[1 << 15];

			public WorkerManager() {
				jobDistributor = new Thread(JobDistributor);
				jobsToDo = new ThreadSafeQueue<int>();
				for (int i = 0; i < jobsForWorkers.Length; i++) {
					jobsForWorkers[i] = new Job();
				}

				int numWorkers = Math.Max(1, ThreadedJobs.AVAILABLE_CORES);
				workers = new Worker[numWorkers];
				for (int i = 0; i < workers.Length; i++) workers[i] = new Worker(this, i);
				jobDistributor.Name = "Job distributor";
				jobDistributor.Start();
			}

			internal void OnDestroy() {
				for (int i = 0; i < workers.Length; i++) workers[i].OnDestroy();
			}

			private void SignalWorkers() {
				int l = ThreadedJobs.CORES_IN_USE - 1;
				for (int i = 0; i < l; i++) workers[workers.Length - 1 - i].waiter.Set();
			}

			private void JobDistributor() {
				while (true) {
					if (!waiter.WaitOne(3000)) {
						//Console.WriteLine("hmmm");
					}
					// There are queued jobs to distribute, don't end the loop until they are distributed
					int job;
					while (jobsToDo.Dequeue(out job)) {
						while (!DistributeJob(job, jobsForWorkers[job].jobId)) {
							waiter.WaitOne(1);
						}
					}

					// try reorganizing jobs if needed?
					// ---
				}
			}

			private int maxNumberOfQueuedJobsInWorker = 10; // should be tuned
			private bool DistributeJob(int jobArrayIndex, int jobId) {
				for (int i = 0; i < Math.Min(workers.Length, ThreadedJobs.CORES_IN_USE); i++) {
					if (workers[i].jobsToDo.GetApproxLength() < maxNumberOfQueuedJobsInWorker) {
						workers[i].jobsToDo.Enqueue(jobArrayIndex);
						workers[i].waiter.Set();
						return true;
					}
				}
				return false;
			}

			int nextJobIndex = 0;
			public QueuedJob QueueMultithreadJob(Action action, Action<Exception> onException) {
				// Not very well done
				int nextIndex = 0;
				while (0 == nextIndex || 0 != jobsForWorkers[nextIndex].jobId)
					nextIndex = Interlocked.Increment(ref nextJobIndex) & jobsForWorkersLengthMask;

				jobsForWorkers[nextIndex].Set(action, onException);

				if (ThreadedJobs.FORCE_SINGLE_THREAD || !jobsToDo.Enqueue(nextIndex)) {
					jobsForWorkers[nextIndex].Execute();
				}
				else {
					lastActionStamp++;
					waiter.Set();
				}
				return new QueuedJob(jobsForWorkers[nextIndex]);
			}

			public void Dispose() {
				waiter.Close();
			}
		}
	}
}
