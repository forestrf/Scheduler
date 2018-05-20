using Ashkatchap.Scheduler.Collections;
using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	internal partial class FrameUpdater {
		public class WorkerManager {
			internal volatile byte lastActionStamp = 0;
			internal readonly ThreadSafeRingBuffer_MultiProducer_SingleConsumer<QueuedJob> jobsToDo;
			private readonly Worker[] workers;
			private readonly Thread jobDistributor;
			internal AutoResetEvent waiter = new AutoResetEvent(true);
			
			public WorkerManager() {
				jobDistributor = new Thread(JobDistributor);
				jobsToDo = new ThreadSafeRingBuffer_MultiProducer_SingleConsumer<QueuedJob>(15, jobDistributor);

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
					QueuedJob job;
					while (jobsToDo.TryDequeue(out job)) {
						while (!DistributeJob(job)) {
							Thread.Sleep(0);
						}
					}
					
					// try reorganizing jobs if needed
					// ---
				}
			}

			private bool DistributeJob(QueuedJob job) {
				for (int i = 0; i < workers.Length; i++) {
					if (null == workers[i].pendingJob) {
						Interlocked.Exchange(ref workers[i].pendingJob, job);
						workers[i].waiter.Set();
						return true;
					}
				}
				return false;
			}

			public bool QueueMultithreadJob(Action job, Action<Exception> onException, out QueuedJob queuedJob) {
				queuedJob = new QueuedJob(job, onException);

				if (ThreadedJobs.FORCE_SINGLE_THREAD) {
					queuedJob.Execute();
					return true;
				} else {
					queuedJob = new QueuedJob(job, onException);
					jobsToDo.Enqueue(queuedJob);
					lastActionStamp++;
					waiter.Set();
					return true;
				}
			}
		}
	}
}
