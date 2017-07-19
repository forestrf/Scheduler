using System;
using UnityEngine;

namespace Ashkatchap.Updater {
	public static class Scheduler {
		static FrameUpdater Instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnRuntimeMethodLoad() {
			if (Instance == null) {
#if UNITY_EDITOR
				if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
					return;
#endif
				Instance = new GameObject("Updater").AddComponent<FrameUpdater>();
				Logger.Info("Updater created");
			}
		}

		public static FrameUpdater.RecurrentReference AddRecurrentUpdateCallback(Action method, QueueOrder queue, byte order = 127) {
			return Instance.AddRecurrentUpdateCallbackInstance(method, queue, order);
		}
		public static void RemoveRecurrentUpdateCallback(FrameUpdater.RecurrentReference reference) {
			Instance.RemoveRecurrentUpdateCallbackInstance(reference);
		}

		public static void QueueUpdateCallback(QueueOrder queue, Action method) {
			Instance.QueueUpdateCallbackInstance(queue, method);
		}

		public static FrameUpdater.JobReference QueueMultithreadJob(FrameUpdater.Job callback, int numberOfIterations, byte priority = 127) {
			return Instance.QueueMultithreadJobInstance(callback, numberOfIterations, priority);
		}
	}
}
