using System;
using System.Threading;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		public static bool FORCE_SINGLE_THREAD = false;
		private static readonly int ProcessorCount = Math.Min(Environment.ProcessorCount, 64);
		public delegate void Job(int index);


		private WorkerManager executor;


		internal partial class QueuedJob {
			private Job job;
			private int index;
			private int length;
			private bool isFinished = true;
			internal long id;
			internal WorkerManager executor;
			
			internal void Init(WorkerManager executor, Job job, int length, long id, byte priority) {
				if (!this.isFinished) {
					Logger.Warn("Thread safety broken. Errors may appear, like executing the same job index more than one time.");
				}
				this.executor = executor;
				this.job = job;
				this.id = id;
				this.length = length;
				this.index = 0;
				this.priority = priority;
				this.isFinished = false;
				Thread.MemoryBarrier();
			}

			internal bool TryExecute() {
				if (!isFinished) {
					int indexToRun = Interlocked.Increment(ref index) - 1;
					// check to see if it repeats a number
					if (indexToRun < length) {
						job(indexToRun);
						/*
						try {
							//Logger.Trace("Thread <" + Thread.CurrentThread.Name + ">: Executing job " + id + " iteration " + indexToRun.ToString());

							job(indexToRun);
						} catch (Exception e) {
							Logger.Error(e.ToString());
						}
						*/
						return true;
					} else {
						FinishJob();
					}
				}
				return false;
			}

			public void WaitForFinish() {
				executor.RequestSetJobToAllThreads(this);
				while (true) if (!TryExecute()) break;
				FinishJob();
			}

			public void Destroy() {
				FinishJob();
			}

			public bool IsFinished() {
				return isFinished;
			}

			private void FinishJob() {
				isFinished = true;
				Interlocked.Exchange(ref index, length);
				Thread.MemoryBarrier();
			}

			public void ChangePriority(byte newPriority) {
				executor.RequestJobPriorityChange(this, newPriority);
			}
		}

		public struct JobReference {
			private QueuedJob job;
			private long id;

			internal JobReference(QueuedJob job) {
				this.job = job;
				this.id = job.id;
			}

			public void WaitForFinish() {
				if (job.id == id) job.WaitForFinish();
			}

			public void Destroy() {
				if (job.id == id) job.Destroy();
			}
		}
		
		public JobReference QueueMultithreadJobInstance(Job job, int numberOfIterations, byte priority) {
			return executor.RequestQueueMultithreadJobInstance(job, numberOfIterations, priority);
		}
	}
}
