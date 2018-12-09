using Ashkatchap.Scheduler;
using System;
using UnityEngine;

namespace Ashkatchap.UnityScheduler.Behaviours {
	public class FrameUpdater : MonoBehaviour {
		private static TimerScaled UnityTimerScaled;
		private static TimerUnscaled UnityTimerUnscaled;

		private FirstUpdaterBehaviour firstUpdater;
		private LastUpdaterBehaviour lastUpdater;

		private readonly Updater firstFixedUpdate = new Updater(OnException);
		private readonly Updater fixedUpdate = new Updater(OnException);
		private readonly Updater lastFixedUpdate = new Updater(OnException);

		private readonly Updater afterPhysicsExecuted = new Updater(OnException);

		private readonly Updater firstUpdate = new Updater(OnException);
		private readonly Updater update = new Updater(OnException);
		private readonly Updater lastUpdate = new Updater(OnException);

		private readonly Updater firstLateUpdate = new Updater(OnException);
		private readonly Updater lateUpdate = new Updater(OnException);
		private readonly Updater lastLateUpdate = new Updater(OnException);

		private void Awake() {
			if (null == UnityTimerScaled) {
				UnityTimerScaled = new TimerScaled();
				UnityTimerUnscaled = new TimerUnscaled();
			}
		}

		private void OnEnable() {
			//gameObject.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(gameObject);

			firstUpdater = gameObject.AddComponent<FirstUpdaterBehaviour>();
			lastUpdater = gameObject.AddComponent<LastUpdaterBehaviour>();

			SetupUpdaters();
			Debug.Log("Updater GameObject created and Updater Behaviours configured");

#if !UNITY_WEBGL || (!FORCE_WEBGL && UNITY_EDITOR)
			ThreadedJobs.MultithreadingStart();
			Debug.Log("Multithread Support started");
#endif
		}

		private void OnDisable() {
			ThreadedJobs.MultithreadingEnd();
			Destroy(firstUpdater);
			Destroy(lastUpdater);
		}

		private static void OnException(Exception e) {
			Debug.Log(e);
		}

		private void SetupUpdaters() {
			bool afterFixedUpdateIsReady = false;
			firstUpdater.SetQueues(
				() => {
					if (afterFixedUpdateIsReady) {
						afterPhysicsExecuted.Execute();
						afterFixedUpdateIsReady = false;
					}

					firstFixedUpdate.Execute();
					fixedUpdate.Execute();
				},
				() => {
					if (afterFixedUpdateIsReady) {
						afterPhysicsExecuted.Execute();
						afterFixedUpdateIsReady = false;
					}

					UnityTimerScaled.UpdateCurrentTime();
					UnityTimerUnscaled.UpdateCurrentTime();

					firstUpdate.Execute();
					update.Execute();
				},
				() => {
					firstLateUpdate.Execute();
					lateUpdate.Execute();
				});
			lastUpdater.SetQueues(
				() => {
					lastFixedUpdate.Execute();
					afterFixedUpdateIsReady = true;
				},
				() => {
					lastUpdate.Execute();
				},
				() => {
					lastLateUpdate.Execute();
				});
		}

		public FrameUpdateReference AddUpdateCallback(Action method, QueueOrder queue, byte order = 127) {
			return new FrameUpdateReference(queue, GetUpdaterList(queue).AddUpdateCallback(method, order));
		}

		public void RemoveUpdateCallback(FrameUpdateReference reference) {
			var updater = GetUpdaterList(reference.queue);
			updater.RemoveUpdateCallback(reference.reference);
		}

		public void QueueCallback(QueueOrder queue, Action method) {
			GetUpdaterList(queue).QueueCallback(method);
			Debug.Log("Queued Update method");
		}

		internal void QueueCallback(QueueOrder queue, Action method, float secondsToWait, bool scaledTime = true) {
			GetUpdaterList(queue).QueueCallback(scaledTime ? (ITimer) UnityTimerScaled : UnityTimerUnscaled, method, secondsToWait);
		}

		private Updater GetUpdaterList(QueueOrder queue) {
			switch (queue) {
				default:
				case QueueOrder.PreFixedUpdate: return firstFixedUpdate;
				case QueueOrder.FixedUpdate: return fixedUpdate;
				case QueueOrder.PostFixedUpdate: return lastFixedUpdate;

				case QueueOrder.AfterFixedUpdate: return afterPhysicsExecuted;

				case QueueOrder.PreUpdate: return firstUpdate;
				case QueueOrder.Update: return update;
				case QueueOrder.PostUpdate: return lastUpdate;

				case QueueOrder.PreLateUpdate: return firstLateUpdate;
				case QueueOrder.LateUpdate: return lateUpdate;
				case QueueOrder.PostLateUpdate: return lastLateUpdate;
			}
		}
	}
}
