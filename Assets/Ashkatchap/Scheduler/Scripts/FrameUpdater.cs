using Ashkatchap.Shared;
using Ashkatchap.Shared.Collections;
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
		/// Execute before any Update script. It is intended to only read information before any script can makes changes to the scene, but not making changes is not enforced
		/// </summary>
		PreUpdate = 0,
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

		private readonly UpdaterList firstUpdate = new UpdaterList();
		private readonly UpdaterList update = new UpdaterList();
		private readonly UpdaterList lastUpdate = new UpdaterList();
		private readonly UpdaterList firstLateUpdate = new UpdaterList();
		private readonly UpdaterList lateUpdate = new UpdaterList();
		private readonly UpdaterList lastLateUpdate = new UpdaterList();
		
		private int nextRecurrentId;

		private void OnEnable() {
			//gameObject.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(gameObject);

			firstUpdater = gameObject.AddComponent<FirstUpdaterBehaviour>();
			lastUpdater = gameObject.AddComponent<LastUpdaterBehaviour>();

			SetUpUpdaters();

			Logger.Debug("Updater GameObject created and Updater Behaviours configured");

			executor = new WorkerManager(ProcessorCount - 1);
			Logger.Info("Executor created");
		}

		private void OnDisable() {
			executor.OnDestroy();
			Destroy(firstUpdater);
			Destroy(lastUpdater);
		}

		private void SetUpUpdaters() {
			firstUpdater.SetQueues(
				() => {
					LoopUpdate(QueueOrder.PreUpdate, firstUpdate);
					LoopUpdate(QueueOrder.Update, update);
				},
				() => {
					LoopUpdate(QueueOrder.PostUpdate, lastUpdate);
				});
			lastUpdater.SetQueues(
				() => {
					LoopUpdate(QueueOrder.PreLateUpdate, firstLateUpdate);
					LoopUpdate(QueueOrder.LateUpdate, lateUpdate);
				},
				() => {
					LoopUpdate(QueueOrder.PostLateUpdate, lastLateUpdate);
				});
		}

		public struct RecurrentReference {
			internal readonly long id;
			internal readonly QueueOrder queue;
			internal readonly byte order;

			public RecurrentReference(long id, QueueOrder queue, byte order) {
				this.id = id;
				this.queue = queue;
				this.order = order;
			}
		}

		
		public RecurrentReference AddRecurrentUpdateCallbackInstance(Action method, QueueOrder queue, byte order = 127) {
			var updater = GetUpdaterList(queue);
			ActionWrapped aw = new ActionWrapped(nextRecurrentId++, method);
			updater.AddDelayed(aw, order);
			return new RecurrentReference(aw.id, queue, order);
		}
		public void RemoveRecurrentUpdateCallbackInstance(RecurrentReference reference) {
			var updater = GetUpdaterList(reference.queue);
			updater.RemoveDelayed(reference);
		}

		public void QueueUpdateCallbackInstance(QueueOrder queue, Action method) {
			GetUpdaterList(queue).queuedUpdateCallbacks.Enqueue(method);
			Logger.Debug("Queued Update method");
		}
		
		private UpdaterList GetUpdaterList(QueueOrder queue) {
			switch (queue) {
				default:
				case QueueOrder.PreUpdate: return firstUpdate;
				case QueueOrder.Update: return update;
				case QueueOrder.PostUpdate: return lastUpdate;
				case QueueOrder.PreLateUpdate: return firstLateUpdate;
				case QueueOrder.LateUpdate: return lateUpdate;
				case QueueOrder.PostLateUpdate: return lastLateUpdate;
			}
		}
		
		private static void LoopUpdate(QueueOrder queueOrder, UpdaterList updater) {
			Profiler.BeginSample("Queue ExecutePendingChanges");
			updater.ExecuteDelayedActions();
			Profiler.EndSample();

			Profiler.BeginSample("Queue Iterate");
			for (int i = 0; i < updater.recurrentCallbacks.Length; i++) {
				var queue = updater.recurrentCallbacks[i];
				for (int j = 0; j < queue.Size; j++) {
					queue.elements[j].action();
					/*
					try {
						queue.elements[j].action();
					}
					catch (Exception e) {
						Logger.Error(e.ToString());
					}
					*/
				}
			}
			Profiler.EndSample();

			Profiler.BeginSample("One Time Callbacks");
			Action action;
			while (updater.queuedUpdateCallbacks.TryDequeue(out action)) action();
			Profiler.EndSample();
		}

		private class UpdaterList {
			internal readonly UnorderedList<ActionWrapped>[] recurrentCallbacks = new UnorderedList<ActionWrapped>[256];
			internal readonly ThreadSafeRingBuffer_MultiProducer_SingleConsumer<Action> queuedUpdateCallbacks = new ThreadSafeRingBuffer_MultiProducer_SingleConsumer<Action>(256); // Can change in any Thread

			private object pendingListsLocker = new object();
			private readonly UnorderedList<DelayedAdd> pendingAdds = new UnorderedList<DelayedAdd>();
			private readonly UnorderedList<DelayedRemove> pendingRemoves = new UnorderedList<DelayedRemove>();
			
			public UpdaterList() {
				for (int i = 0; i < recurrentCallbacks.Length; i++) {
					recurrentCallbacks[i] = new UnorderedList<ActionWrapped>(16, 16);
				}
			}

			public void AddDelayed(ActionWrapped aw, byte order) {
				lock (pendingListsLocker) {
					pendingAdds.Add(new DelayedAdd(aw, order));
				}
			}
			public void RemoveDelayed(RecurrentReference reference) {
				lock (pendingListsLocker) {
					pendingRemoves.Add(new DelayedRemove(reference));
				}
			}

			public void ExecuteDelayedActions() {
				lock (pendingListsLocker) {
					for (int i = 0; i < pendingAdds.Size; i++) pendingAdds.elements[i].Execute(this);
					pendingAdds.Clear(true);
					for (int i = 0; i < pendingRemoves.Size; i++) pendingRemoves.elements[i].Execute(this);
					pendingRemoves.Clear(true);
				}
			}

			private struct DelayedAdd {
				private byte order;
				private bool done;
				private ActionWrapped actionToAdd;

				public DelayedAdd(ActionWrapped actionToAdd, byte order) {
					this.actionToAdd = actionToAdd;
					this.order = order;
					done = false;
				}

				public void Execute(UpdaterList updater) {
					if (done) return;

					updater.recurrentCallbacks[order].Add(actionToAdd);
					Logger.Trace("Added update method");
					
					done = true;
				}
			}
			private struct DelayedRemove {
				private bool done;
				private RecurrentReference actionToRemove;
				
				public DelayedRemove(RecurrentReference actionToRemove) {
					this.actionToRemove = actionToRemove;
					done = false;
				}

				public void Execute(UpdaterList updater) {
					if (done) return;

					var list = updater.recurrentCallbacks[actionToRemove.order];
					for (int i = 0; i < list.Size; i++) {
						if (list.elements[i].id == actionToRemove.id) {
							list.RemoveAt(i);
							Logger.Trace("Removed update method");
							break;
						}
					}
					
					done = true;
				}
			}
		}

		private struct ActionWrapped {
			public Action action;
			public long id;

			public ActionWrapped(long id, Action action) {
				this.id = id;
				this.action = action;
			}
		}
	}
}
