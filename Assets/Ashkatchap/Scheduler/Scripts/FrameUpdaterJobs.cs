using System;
using System.Threading;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		public static bool FORCE_SINGLE_THREAD = false;
		public static int NUM_THREADS = 8;
		private static readonly int ProcessorCount = Math.Min(Environment.ProcessorCount, 64);
		public delegate void Job(int index);


		static internal WorkerManager executor;
		
		public struct JobReference {
			private QueuedJob job;
			private int id;
			
			internal JobReference(QueuedJob job) {
				this.job = job;
				this.id = job != null ? job.GetId() : -1;
			}

			public void WaitForFinish() {
				Logger.WarnAssert(!IsMainThread(), "WaitForFinish can only be called from the main thread");
				if (!IsMainThread()) return;
				if (job == null) return;
				if (job.CheckId(id)) job.WaitForFinish();
			}

			public void Destroy() {
				Logger.WarnAssert(!IsMainThread(), "Destroy can only be called from the main thread");
				if (!IsMainThread()) return;
				if (job == null) return;
				if (job.CheckId(id)) job.Destroy();
			}
		}
		
		public JobReference QueueMultithreadJobInstance(Job job, ushort numberOfIterations, byte priority) {
			return executor.QueueMultithreadJobInstance(job, numberOfIterations, priority);
		}

		public static bool IsMainThread() {
			return Thread.CurrentThread == mainThread;
		}
	}
}
