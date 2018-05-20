using Ashkatchap.Scheduler.Collections;
using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	public class Updater {
		private readonly ThreadSafeRingBuffer_MultiProducer_SingleConsumer<Action> queuedUpdateCallbacks;
		private readonly UnorderedList<ActionWrapped>[] recurrentCallbacks = new UnorderedList<ActionWrapped>[256];
		private readonly UnorderedList<UpdateReference>[] delayedRemoves = new UnorderedList<UpdateReference>[256];
		private readonly Thread mainThread;
		private int nextRecurrentId;
		
		public Updater(int initialSize = 16, int stepIncrement = 16) {
			mainThread = Thread.CurrentThread;
			for (int i = 0; i < recurrentCallbacks.Length; i++) {
				recurrentCallbacks[i] = new UnorderedList<ActionWrapped>(initialSize, stepIncrement);
				delayedRemoves[i] = new UnorderedList<UpdateReference>(initialSize, stepIncrement);
			}
			queuedUpdateCallbacks = new ThreadSafeRingBuffer_MultiProducer_SingleConsumer<Action>(9, mainThread);
		}


		public bool InMainThread() {
			return mainThread == Thread.CurrentThread;
		}
		
		/// <param name="onException">To avoid generating garbage use a static method or a cached delegate</param>
		public void Execute(Action<Exception> onException) {
			for (int i = 0; i < recurrentCallbacks.Length; i++) {
				var queue = recurrentCallbacks[i];

				// Execute Delayed Deletes
				var delayedQueue = delayedRemoves[i];
				if (delayedQueue.Size > 0) {
					lock (delayedQueue) {
						while (delayedQueue.Size > 0) {
							var reference = delayedQueue.ExtractLast();
							for (int j = 0; j < queue.Size; j++) {
								if (queue.elements[j].id == reference.id) {
									queue.RemoveAt(j);
									break;
								}
							}
						}
					}
				}

				for (int j = 0; j < queue.Size; j++) {
					try {
						queue.elements[j].action();
					}
					catch (Exception e) {
						Console.WriteLine(e);
						if (null != onException) onException.Invoke(e);
					}
				}
			}
			
			Action action;
			while (queuedUpdateCallbacks.TryDequeue(out action)) action();
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
			queuedUpdateCallbacks.Enqueue(method);
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
