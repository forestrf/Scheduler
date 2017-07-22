using System.Threading;

namespace Ashkatchap.Shared.Collections {
	/// <summary>
	/// Implementation of the Disruptor pattern
	/// </summary>
	/// <typeparam name="T">the type of item to be stored</typeparam>
	public class ThreadSafeObjectPool<T> where T : class, new() {
		private readonly T[] _entries;
		
		public ThreadSafeObjectPool(ushort capacity) {
			_entries = new T[capacity];
		}
		
		public void Recycle(T obj) {
			for (int i = 0; i < _entries.Length; i++) {
				if (ReferenceEquals(_entries[i], null)) {
					if (ReferenceEquals(null, Interlocked.CompareExchange(ref _entries[i], obj, null))) {
						return;
					}
				}
			}
		}
		
		public T Get() {
			for (int i = 0; i < _entries.Length; i++) {
				if (!ReferenceEquals(_entries[i], null)) {
					T item = _entries[i];
					if (ReferenceEquals(item, Interlocked.CompareExchange(ref _entries[i], null, item))) {
						return item;
					}
				}
			}
			return new T();
		}
	}
}
