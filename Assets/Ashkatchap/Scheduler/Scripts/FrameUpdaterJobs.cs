using System;
using System.Threading;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		public static bool FORCE_SINGLE_THREAD = false;
		private static readonly int ProcessorCount = Math.Min(Environment.ProcessorCount, 64);
		public delegate void Job(int index);


		static internal WorkerManager executor;
		
		public struct JobReference {
			private QueuedJob job;
			private int id;

			internal JobReference(QueuedJob job) {
				this.job = job;
				this.id = job.GetId();
			}

			public void WaitForFinish() {
				if (Thread.CurrentThread != mainThread) {
					Logger.Warn("WaitForFinish can only be called from the main thread");
					return;
				}
				if (job.CheckId(id)) job.WaitForFinish();
			}

			public void Destroy() {
				if (Thread.CurrentThread != mainThread) {
					Logger.Warn("WaitForFinish can only be called from the main thread");
					return;
				}
				if (job.CheckId(id)) job.Destroy();
			}
		}
		
		public JobReference QueueMultithreadJobInstance(Job job, ushort numberOfIterations, byte priority) {
			return executor.QueueMultithreadJobInstance(job, numberOfIterations, priority);
		}
	}
}
