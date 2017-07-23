using System;

namespace Ashkatchap.Updater {
	public delegate void Job(int index);

	public static class Scheduler {
		public static bool FORCE_SINGLE_THREAD = false;
		public static readonly int ProcessorCount = Math.Min(Environment.ProcessorCount, 64); // I don't remember about this limit
		public static int DESIRED_NUM_THREADS = ProcessorCount;
		internal static FrameUpdater.WorkerManager executor;
		private static Updater updater;


		public static void MultithreadingStart(Updater updater) {
			if (executor != null) {
				Logger.Warn("Multithreading support is already enabled");
				return;
			}
			Scheduler.updater = updater;
			executor = new FrameUpdater.WorkerManager(updater);
		}
		public static void MultithreadingEnd() {
			if (executor == null) {
				Logger.Warn("Multithreading support is already isabled");
				return;
			}
			executor.OnDestroy();
			executor = null;
		}

		public static JobReference QueueMultithreadJob(Job callback, ushort numberOfIterations, byte priority = 127) {
			return executor.QueueMultithreadJobInstance(callback, numberOfIterations, priority);
		}

		public static bool InMainThread() {
			return updater.InMainThread();
		}
	}
}
