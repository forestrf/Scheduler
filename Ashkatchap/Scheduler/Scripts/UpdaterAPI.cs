using Ashkatchap.Scheduler;
using System;
using UnityEngine;

namespace Ashkatchap.UnityScheduler {
	/// <summary>
	/// Executing ordered
	/// </summary>
	public enum QueueOrder {
		/// <summary>
		/// Execute before any FixedUpdate script. It is intended to only read information before any script can makes changes to the scene, but not making changes is not enforced
		/// </summary>
		PreFixedUpdate,
		/// <summary>
		/// FixedUpdate
		/// </summary>
		FixedUpdate,
		/// <summary>
		/// Execute after any FixedUpdate script
		/// </summary>
		PostFixedUpdate,

		/// <summary>
		/// Execute after a physics step is executed (FixedUpdate, Physics execution and then this)
		/// It will be executed in FixedUpdate or Update given the nature of how FixedUpdate works
		/// </summary>
		AfterFixedUpdate,

		/// <summary>
		/// Execute before any Update script. It is intended to only read information before any script can makes changes to the scene, but not making changes is not enforced
		/// </summary>
		PreUpdate,
		/// <summary>
		/// Update
		/// </summary>
		Update,
		/// <summary>
		/// Execute after any Update script
		/// </summary>
		PostUpdate,

		/// <summary>
		/// Execute before any LateUpdate script. It is intended to only read information after the animations are updated, but not making changes is not enforced
		/// </summary>
		PreLateUpdate,
		/// <summary>
		/// LateUpdate
		/// </summary>
		LateUpdate,
		/// <summary>
		/// Execute after any LateUpdate script
		/// </summary>
		PostLateUpdate
	};

	public static class UpdaterAPI {
		private static Behaviours.FrameUpdater Instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnRuntimeMethodLoad() {
			if (Instance == null) {
#if UNITY_EDITOR
				if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
					return;
#endif
				Instance = new GameObject("Updater").AddComponent<Behaviours.FrameUpdater>();
				Debug.Log("Updater created");
			}
		}

		public static FrameUpdateReference AddUpdateCallback(Action method, QueueOrder queue, byte order = 127) {
			return Instance.AddUpdateCallback(method, queue, order);
		}
		public static void RemoveUpdateCallback(FrameUpdateReference reference) {
			Instance.RemoveUpdateCallback(reference);
		}

		public static void QueueCallback(QueueOrder queue, Action method) {
			Instance.QueueCallback(queue, method);
		}
		/// <param name="scaledTime">Be affected by Time.timeScale</param>
		public static void QueueCallback(QueueOrder queue, Action method, float secondsToWait, bool scaledTime = true) {
			Instance.QueueCallback(queue, method, secondsToWait, scaledTime);
		}
	}

	public struct FrameUpdateReference {
		public readonly QueueOrder queue;
		public readonly UpdateReference reference;

		public FrameUpdateReference(QueueOrder queue, UpdateReference reference) {
			this.queue = queue;
			this.reference = reference;
		}

		public void RemoveUpdateCallback() {
			UpdaterAPI.RemoveUpdateCallback(this);
		}
	}
}
