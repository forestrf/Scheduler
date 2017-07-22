using Ashkatchap.Shared.Collections;
using System;
using System.Threading;
using UnityEngine.Profiling;

namespace Ashkatchap.Updater {
	public class Updater {
		private readonly ThreadSafeRingBuffer_MultiProducer_SingleConsumer<Action> queuedUpdateCallbacks = new ThreadSafeRingBuffer_MultiProducer_SingleConsumer<Action>(256);
		private readonly UnorderedList<ActionWrapped>[] recurrentCallbacks = new UnorderedList<ActionWrapped>[256];
		private readonly Thread mainThread;
		private int nextRecurrentId;

		public Updater() {
			mainThread = Thread.CurrentThread;
			for (int i = 0; i < recurrentCallbacks.Length; i++) {
				recurrentCallbacks[i] = new UnorderedList<ActionWrapped>(16, 16);
			}
		}


		public bool InMainThread() {
			return mainThread == Thread.CurrentThread;
		}
		
		public void Execute() {
			Profiler.BeginSample("Queue Iterate");
			for (int i = 0; i < recurrentCallbacks.Length; i++) {
				var queue = recurrentCallbacks[i];
				for (int j = 0; j < queue.Size; j++) {
					try {
						queue.elements[j].action();
					}
					catch (Exception e) {
						Logger.Error(e.ToString());
					}
				}
			}
			Profiler.EndSample();

			Profiler.BeginSample("One Time Callbacks");
			Action action;
			while (queuedUpdateCallbacks.TryDequeue(out action)) action();
			Profiler.EndSample();
		}

		

		public UpdateReference AddUpdateCallback(Action method, byte order = 127) {
			var aw = new ActionWrapped(nextRecurrentId++, method);
			recurrentCallbacks[order].Add(aw);
			return new UpdateReference(aw.id, order);
		}
		public void RemoveUpdateCallback(UpdateReference reference) {
			var list = recurrentCallbacks[reference.order];
			for (int i = 0; i < list.Size; i++) {
				if (list.elements[i].id == reference.id) {
					list.RemoveAt(i);
					return;
				}
			}
		}
		
		public void QueueCallback(Action method) {
			queuedUpdateCallbacks.Enqueue(method);
			Logger.Debug("Queued Update Callback");
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
