using Ashkatchap.Scheduler.Collections;
using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	internal partial class FrameUpdater {
		public class WorkerManager {
			internal volatile byte lastActionStamp = 0;
			internal readonly ThreadSafeRingBuffer_MultiProducer_SingleConsumerInt jobsToDo;
			private readonly Worker[] workers;
			internal readonly Thread jobDistributor;
			internal AutoResetEvent waiter = new AutoResetEvent(true);
			internal readonly int jobsForWorkersLengthMask = (1 << 15) - 1;
			internal readonly Job[] jobsForWorkers = new Job[1 << 15];
			
			public WorkerManager() {
				jobDistributor = new Thread(JobDistributor);
				jobsToDo = new ThreadSafeRingBuffer_MultiProducer_SingleConsumerInt(15, jobDistributor);
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
						Console.WriteLine("hmmm");
					}
					// There are queued jobs to distribute, don't end the loop until they are distributed
					int job;
					while (jobsToDo.TryDequeue(out job)) {
						while (!DistributeJob(job, jobsForWorkers[job].jobId)) {
							waiter.WaitOne(1);
						}
					}
					
					// try reorganizing jobs if needed
					// ---
				}
			}
			
			private bool DistributeJob(int jobArrayIndex, int jobId) {
				for (int i = 0; i < Math.Min(workers.Length, ThreadedJobs.DESIRED_NUM_CORES); i++) {
					if (workers[i].jobsToDo.GetApproxLength() < 10) {
						workers[i].jobsToDo.Enqueue(jobArrayIndex);
						workers[i].waiter.Set();
						return true;
					}
				}
				return false;
			}

			int nextJobIndex = 0;
			public QueuedJob QueueMultithreadJob(Action action, Action<Exception> onException) {
				int nextIndex = 0;
				while (0 == nextIndex)
					nextIndex = Interlocked.Increment(ref nextJobIndex) & jobsForWorkersLengthMask;
				
				jobsForWorkers[nextIndex].Set(action, onException, nextIndex);

				if (ThreadedJobs.FORCE_SINGLE_THREAD || !jobsToDo.Enqueue(nextIndex)) {
					jobsForWorkers[nextIndex].Execute();
				} else {
					lastActionStamp++;
					waiter.Set();
				}
				return new QueuedJob(jobsForWorkers[nextIndex].jobId, nextIndex, jobsForWorkers);
			}
		}
	}
}
