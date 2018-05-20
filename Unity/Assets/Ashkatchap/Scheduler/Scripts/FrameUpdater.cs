using Ashkatchap.Scheduler;
using Ashkatchap.UnityScheduler.Behaviours;
using System;
using UnityEngine;
using UnityEngine.Profiling;

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


	public partial class FrameUpdater : MonoBehaviour {
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
