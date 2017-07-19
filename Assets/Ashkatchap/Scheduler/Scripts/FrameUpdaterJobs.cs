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
			private int index = 0;
			private int length = 0;
			private int id;
			internal WorkerManager executor;
			
			internal void Init(WorkerManager executor, Job job, int length, int id, byte priority) {
				if (!IsFinished()) {
					//Logger.Warn("Thread safety broken. Errors may appear, like executing the same job index more than one time.");
				}
				this.executor = executor;
				this.job = job;
				this.id = id;
				this.length = length;
				this.priority = priority;
				Thread.MemoryBarrier();
				this.index = -1;
				Thread.MemoryBarrier();
			}

			internal bool TryExecute() {
				if (index < length) {
					int indexToRun = Interlocked.Increment(ref index) - 1;
					// check to see if it repeats a number
					if (indexToRun < length) {
						try {
							//Logger.Trace("Thread <" + Thread.CurrentThread.Name + ">: Executing job " + id + " iteration " + indexToRun.ToString());

							job(indexToRun);
						} catch (Exception e) {
							Logger.Error(e.ToString());
						}
						
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
				return index >= length;
			}

			private void FinishJob() {
				Interlocked.Exchange(ref index, length);
			}

			public void ChangePriority(byte newPriority) {
				executor.RequestJobPriorityChange(this, newPriority);
			}

			public bool CheckId(int id) {
				return this.id == id;
			}
			public int GetId() {
				return this.id;
			}
		}

		public struct JobReference {
			private QueuedJob job;
			private int id;

			internal JobReference(QueuedJob job) {
				this.job = job;
				this.id = job.GetId();
			}

			public void WaitForFinish() {
				if (job.CheckId(id)) job.WaitForFinish();
			}

			public void Destroy() {
				if (job.CheckId(id)) job.Destroy();
			}
		}
		
		public JobReference QueueMultithreadJobInstance(Job job, int numberOfIterations, byte priority) {
			return executor.RequestQueueMultithreadJobInstance(job, numberOfIterations, priority);
		}
	}
}
