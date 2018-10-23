using Ashkatchap.Shared.Collections;

internal class ThreadSafeQueue<T> {
	private CircularBuffer<T> queue = new CircularBuffer<T>(32, 32);

	public bool Enqueue(T item) {
		lock (queue) {
			queue.Enqueue(item);
		}
		return true;
	}

	public bool Dequeue(out T item) {
		if (queue.Count > 0) {
			lock (queue) {
				if (queue.Count > 0) {
					item = queue.Dequeue();
					return true;
				}
			}
		}
		item = default(T);
		return false;
	}

	public int GetApproxLength() {
		return queue.Count;
	}
}
