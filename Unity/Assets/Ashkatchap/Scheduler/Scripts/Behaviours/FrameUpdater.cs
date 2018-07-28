﻿using Ashkatchap.Scheduler;
using System;
using UnityEngine;

namespace Ashkatchap.UnityScheduler.Behaviours {
	public class FrameUpdater : MonoBehaviour {
		private FirstUpdaterBehaviour firstUpdater;
		private LastUpdaterBehaviour lastUpdater;

		private readonly Updater firstFixedUpdate = new Updater();
		private readonly Updater fixedUpdate = new Updater();
		private readonly Updater lastFixedUpdate = new Updater();

		private readonly Updater afterPhysicsExecuted = new Updater();

		private readonly Updater firstUpdate = new Updater();
		private readonly Updater update = new Updater();
		private readonly Updater lastUpdate = new Updater();

		private readonly Updater firstLateUpdate = new Updater();
		private readonly Updater lateUpdate = new Updater();
		private readonly Updater lastLateUpdate = new Updater();
		

		private void OnEnable() {
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(gameObject);

			firstUpdater = gameObject.AddComponent<FirstUpdaterBehaviour>();
			lastUpdater = gameObject.AddComponent<LastUpdaterBehaviour>();

			SetupUpdaters();
			Debug.Log("Updater GameObject created and Updater Behaviours configured");

			ThreadedJobs.MultithreadingStart();
			Debug.Log("Multithread Support started");
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
						afterPhysicsExecuted.Execute(OnException);
						afterFixedUpdateIsReady = false;
					}
					
					firstFixedUpdate.Execute(OnException);
					fixedUpdate.Execute(OnException);
				},
				() => {
					if (afterFixedUpdateIsReady) {
						afterPhysicsExecuted.Execute(OnException);
						afterFixedUpdateIsReady = false;
					}

					firstUpdate.Execute(OnException);
					update.Execute(OnException);
				},
				() => {
					firstLateUpdate.Execute(OnException);
					lateUpdate.Execute(OnException);
				});
			lastUpdater.SetQueues(
				() => {
					lastFixedUpdate.Execute(OnException);
					afterFixedUpdateIsReady = true;
				},
				() => {
					lastUpdate.Execute(OnException);
				},
				() => {
					lastLateUpdate.Execute(OnException);
				});
		}

		public UpdateReference AddUpdateCallback(Action method, QueueOrder queue, byte order = 127) {
			return new UpdateReference(queue, GetUpdaterList(queue).AddUpdateCallback(method, order));
		}

		public void RemoveUpdateCallback(UpdateReference reference) {
			var updater = GetUpdaterList(reference.queue);
			updater.RemoveUpdateCallback(reference.reference);
		}

		public void QueueCallback(QueueOrder queue, Action method) {
			GetUpdaterList(queue).QueueCallback(method);
			Debug.Log("Queued Update method");
		}

		internal void QueueCallback(QueueOrder queue, Action method, float secondsToWait) {
			GetUpdaterList(queue).QueueCallback(method, secondsToWait);
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

	public struct UpdateReference {
		public readonly QueueOrder queue;
		public readonly Scheduler.UpdateReference reference;

		public UpdateReference(QueueOrder queue, Scheduler.UpdateReference reference) {
			this.queue = queue;
			this.reference = reference;
		}
	}
}