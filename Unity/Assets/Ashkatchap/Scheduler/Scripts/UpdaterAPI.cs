using Ashkatchap.Scheduler;
using System;
using UnityEngine;

namespace Ashkatchap.UnityScheduler {
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
				Debug.Log("Updater created");
			}
		}

		public static UpdateReference AddUpdateCallback(Action method, QueueOrder queue, byte order = 127) {
			return Instance.AddUpdateCallback(method, queue, order);
		}
		public static void RemoveUpdateCallback(UpdateReference reference) {
			Instance.RemoveUpdateCallback(reference);
		}

		public static void QueueCallback(QueueOrder queue, Action method) {
			Instance.QueueCallback(queue, method);
		}

		public static bool QueueMultithreadJob(Action callback, out QueuedJob jobReference) {
			return ThreadedJobs.QueueMultithreadJob(callback, out jobReference);
		}
	}
}
