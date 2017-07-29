using System;
using UnityEngine;

namespace Ashkatchap.Updater {
	public static class UpdaterAPI {
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

		public static UpdateReferenceQ AddUpdateCallback(Action method, QueueOrder queue, byte order = 127) {
			return Instance.AddUpdateCallback(method, queue, order);
		}
		public static void RemoveUpdateCallback(UpdateReferenceQ reference) {
			Instance.RemoveUpdateCallback(reference);
		}

		public static void QueueCallback(QueueOrder queue, Action method) {
			Instance.QueueCallback(queue, method);
		}

		public static JobReference QueueMultithreadJob(Job callback, ushort numberOfIterations, byte priority = 127) {
			return Scheduler.QueueMultithreadJob(callback, numberOfIterations, priority);
		}
	}
}
