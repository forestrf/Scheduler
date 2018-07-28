using Ashkatchap.Scheduler;
using Ashkatchap.UnityScheduler.Behaviours;
using System;
using System.Collections.Generic;
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

		public static Behaviours.UpdateReference AddUpdateCallback(Action method, QueueOrder queue, byte order = 127) {
			return Instance.AddUpdateCallback(method, queue, order);
		}
		public static void RemoveUpdateCallback(Behaviours.UpdateReference reference) {
			Instance.RemoveUpdateCallback(reference);
		}

		public static void QueueCallback(QueueOrder queue, Action method) {
			Instance.QueueCallback(queue, method);
		}
		public static void QueueCallback(QueueOrder queue, Action method, float secondsToWait) {
			Instance.QueueCallback(queue, method, secondsToWait);
		}

		public static void QueueCallback(QueueOrder queue, IEnumerator<WaitOrder> method, byte order = 127) {
			if (!method.MoveNext()) return;
			Behaviours.UpdateReference reference = default(Behaviours.UpdateReference);
			int frameCount = 0;
			long timestamp = TimeCounter.GetTimestamp();

			Action OnUpdate = () => {
				bool executeAgain = false;
				switch (method.Current.type) {
					case WaitOrder.Type.SkipFrames:
						frameCount++;
						if (frameCount > method.Current.frames) executeAgain = true;
						break;
					case WaitOrder.Type.Seconds:
						double secondsPassed = TimeCounter.ElapsedSeconds(timestamp, TimeCounter.GetTimestamp());
						if (secondsPassed > method.Current.seconds) executeAgain = true;
						break;
					default:
						executeAgain = true;
						break;
				}
				if (executeAgain) {
					if (method.MoveNext()) {
						frameCount = 0;
						timestamp = TimeCounter.GetTimestamp();
					} else {
						Instance.RemoveUpdateCallback(reference);
					}
				}
			};
			reference = Instance.AddUpdateCallback(OnUpdate, queue, order);
		}
	}
}
