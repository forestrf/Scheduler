using System;

namespace Ashkatchap.Scheduler {
	public delegate void Job(int index);

	public static class Scheduler {
		public const byte DEFAULT_PRIORITY = 127;
		public static bool FORCE_SINGLE_THREAD = false;
		public static readonly int AVAILABLE_CORES = Environment.ProcessorCount;
		public static int DESIRED_NUM_CORES = AVAILABLE_CORES;
		public static int CORES_IN_USE {
			get { return UnityEngine.Mathf.Clamp(DESIRED_NUM_CORES, 1, AVAILABLE_CORES); }
		}
		internal static FrameUpdater.WorkerManager executor;
		private static Updater updater;


		public static void MultithreadingStart(Updater updater) {
			if (null != executor) return;
			Scheduler.updater = updater;
			executor = new FrameUpdater.WorkerManager(updater);
		}
		public static void MultithreadingEnd() {
			if (executor == null) return;
			executor.OnDestroy();
			executor = null;
		}

		public static bool QueueMultithreadJob(Job callback, ushort numberOfIterations, out JobReference jobReference, byte priority = DEFAULT_PRIORITY, Action<Job> OnFinished = null, Action<Exception> onException = null) {
			return executor.QueueMultithreadJobInstance(callback, numberOfIterations, priority, onException, out jobReference);
		}

		public static bool InMainThread() {
			return updater.InMainThread();
		}
	}
}
