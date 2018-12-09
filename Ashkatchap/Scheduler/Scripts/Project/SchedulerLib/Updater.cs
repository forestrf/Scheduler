using Ashkatchap.Scheduler.Collections;
using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	public class Updater {
		private static readonly ITimer defaultTimer = new Timer();

		public int timesUpdated { get; private set; }

		private readonly ThreadSafeQueue<TimedAction> queuedUpdateCallbacks = new ThreadSafeQueue<TimedAction>();
		private readonly UnorderedList<TimedAction> actionsStillWaiting = new UnorderedList<TimedAction>();
		private readonly UnorderedList<ActionWrapped>[] recurrentCallbacks = new UnorderedList<ActionWrapped>[256];
		private readonly UnorderedList<UpdateReference>[] delayedRemoves = new UnorderedList<UpdateReference>[256];
		private readonly Thread mainThread;
		private readonly Action<Exception> onException;
		private int nextRecurrentId;

		/// <summary>
		/// Create a new Updater. The current thread will be considered the main thread.
		/// </summary>
		public Updater(Action<Exception> onException, int initialSize = 16, int stepIncrement = 16) {
			this.onException = onException;
			mainThread = Thread.CurrentThread;
			for (int i = 0; i < recurrentCallbacks.Length; i++) {
				recurrentCallbacks[i] = new UnorderedList<ActionWrapped>(initialSize, stepIncrement);
				delayedRemoves[i] = new UnorderedList<UpdateReference>(initialSize, stepIncrement);
			}
		}

		public bool NowInMainThread() {
			return mainThread == Thread.CurrentThread;
		}

		/// <param name="onException">To avoid generating garbage use a static method or a cached delegate</param>
		public void Execute() {
			defaultTimer.UpdateCurrentTime();

			timesUpdated++;
			for (int i = 0; i < recurrentCallbacks.Length; i++) {
				var queue = recurrentCallbacks[i];

				// Execute Delayed Deletes
				var delayedQueue = delayedRemoves[i];
				if (delayedQueue.Count > 0) {
					lock (delayedQueue) {
						while (delayedQueue.Count > 0) {
							var reference = delayedQueue.ExtractLast();
							for (int j = 0; j < queue.Count; j++) {
								if (queue.elements[j].id == reference.id) {
									queue.RemoveAt(j);
									break;
								}
							}
						}
					}
				}

				for (int j = 0; j < queue.Count; j++) {
					try {
						queue.elements[j].action();
					}
					catch (Exception e) {
						Console.WriteLine(e);
						if (null != onException) onException.Invoke(e);
					}
				}
			}


			for (int i = 0; i < actionsStillWaiting.Count;) {
				if (actionsStillWaiting[i].ItsTime()) {
					try {
						actionsStillWaiting[i].action.Invoke();
					}
					catch (Exception e) {
						Console.WriteLine(e);
						if (null != onException) onException.Invoke(e);
					}
					finally {
						actionsStillWaiting.RemoveAt(i);
					}
				}
				else {
					i++;
				}
			}

			TimedAction timedAction;
			while (queuedUpdateCallbacks.Dequeue(out timedAction)) {
				if (timedAction.ItsTime()) {
					try {
						timedAction.action();
					}
					catch (Exception e) {
						Console.WriteLine(e);
						if (null != onException) onException.Invoke(e);
					}
				}
				else actionsStillWaiting.Add(timedAction);
			}
		}

		public UpdateReference AddUpdateCallback(Action method, byte order = 127) {
			var aw = new ActionWrapped(nextRecurrentId++, method);
			recurrentCallbacks[order].Add(aw);
			return new UpdateReference(aw.id, order);
		}
		public void RemoveUpdateCallback(UpdateReference reference) {
			var delayedQueue = delayedRemoves[reference.order];
			lock (delayedQueue) {
				delayedRemoves[reference.order].Add(reference);
			}
		}

		public void QueueCallback(Action method) {
			QueueCallback(defaultTimer, method, 0);
		}
		public void QueueCallback(Action method, float secondsToWait) {
			QueueCallback(defaultTimer, method, secondsToWait);
		}
		public void QueueCallback(ITimer timer, Action method, float secondsToWait) {
			queuedUpdateCallbacks.Enqueue(new TimedAction(timer, method, secondsToWait));
		}

		private struct TimedAction : IEquatable<TimedAction> {
			public Action action;
			public float secondsToWait;
			private double timestampStart;
			private ITimer timer;

			public TimedAction(ITimer timer, Action action, float secondsToWait) {
				this.timer = timer;
				this.action = action;
				this.secondsToWait = secondsToWait;
				this.timestampStart = timer.GetCurrentTime();
			}

			public bool Equals(TimedAction other) {
				return action == other.action && secondsToWait == other.secondsToWait && timestampStart == other.timestampStart;
			}

			public bool ItsTime() {
				return timer.GetCurrentTime() >= timestampStart + secondsToWait;
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

		public void Clear() {
			actionsStillWaiting.Clear();
			TimedAction ignore;
			while (queuedUpdateCallbacks.Dequeue(out ignore)) ;
			foreach (var elem in recurrentCallbacks) elem.Clear();
			foreach (var elem in delayedRemoves) elem.Clear();
			timesUpdated = 0;
		}
	}
}
