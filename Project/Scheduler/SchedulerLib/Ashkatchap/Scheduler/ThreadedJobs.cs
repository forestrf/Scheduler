using System;

namespace Ashkatchap.Scheduler {
	public static class ThreadedJobs {
		public static bool FORCE_SINGLE_THREAD = false;
		public static readonly int AVAILABLE_CORES = Environment.ProcessorCount;
		public static int DESIRED_NUM_CORES = AVAILABLE_CORES;
		public static int CORES_IN_USE {
			get { return Math.Max(1, Math.Min(DESIRED_NUM_CORES, AVAILABLE_CORES)); }
		}
		internal static FrameUpdater.WorkerManager executor;


		public static void MultithreadingStart() {
			if (null != executor) return;
			executor = new FrameUpdater.WorkerManager();
		}
		public static void MultithreadingEnd() {
			if (executor == null) return;
			executor.OnDestroy();
			executor = null;
		}

		public static bool QueueMultithreadJob(Action callback, out QueuedJob jobReference, Action<Action> OnFinished = null, Action<Exception> onException = null) {
			return executor.QueueMultithreadJob(callback, onException, out jobReference);
		}
	}
}
