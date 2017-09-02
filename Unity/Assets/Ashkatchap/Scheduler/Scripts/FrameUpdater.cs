using Ashkatchap.Updater.Behaviours;
using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace Ashkatchap.Updater {
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

			Logger.logger = new UnityLogger();

			SetupUpdaters();
			Logger.Debug("Updater GameObject created and Updater Behaviours configured");

			Scheduler.MultithreadingStart(firstUpdate);
			Logger.Debug("Multithread Support started");
		}

		private void OnDisable() {
			Scheduler.MultithreadingEnd();
			Destroy(firstUpdater);
			Destroy(lastUpdater);
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

		public UpdateReferenceQ AddUpdateCallback(Action method, QueueOrder queue, byte order = Scheduler.DEFAULT_PRIORITY) {
			return new UpdateReferenceQ(queue, GetUpdaterList(queue).AddUpdateCallback(method, order));
		}
		public void RemoveUpdateCallback(UpdateReferenceQ reference) {
			var updater = GetUpdaterList(reference.queue);
			updater.RemoveUpdateCallback(reference.reference);
		}

		public void QueueCallback(QueueOrder queue, Action method) {
			GetUpdaterList(queue).QueueCallback(method);
			Logger.Debug("Queued Update method");
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

	public struct UpdateReferenceQ {
		public readonly QueueOrder queue;
		public readonly UpdateReference reference;

		public UpdateReferenceQ(QueueOrder queue, UpdateReference reference) {
			this.queue = queue;
			this.reference = reference;
		}
	}
}
