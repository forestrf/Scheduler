using Ashkatchap.Scheduler.Logging;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ashkatchap.Scheduler.Collections {
	/// <summary>
	/// Implementation of the Disruptor pattern
	/// </summary>
	/// <typeparam name="T">the type of item to be stored</typeparam>
	internal class ThreadSafeRingBuffer_MultiProducer_SingleConsumer<T> where T : class {
		private readonly T[] _entries;
		private int _consumerCursor = 0;
		private Volatile.PaddedVolatileInt _producerCursor = new Volatile.PaddedVolatileInt();
		
		public ThreadSafeRingBuffer_MultiProducer_SingleConsumer(ushort capacity) {
			_entries = new T[capacity];
		}
		
		/// <summary>
		/// Thread safe Enqueue from any thread
		/// </summary>
		public void Enqueue(T obj) {
			if (obj == null) {
				Logger.Warn("Trying to Enqueue null. It is not supported because internally null represents a free place in the buffer for this algorithm.");
				return;
			}

			int indexToWriteOn;
			do {
				Thread.MemoryBarrier(); // obtain a fresh _producerCursor
				indexToWriteOn = _producerCursor.value;
			} while (ReferenceEquals(null, Interlocked.CompareExchange(ref _entries[indexToWriteOn], obj, null)));
			_producerCursor.value = indexToWriteOn == _entries.Length - 1 ? 0 : indexToWriteOn + 1;
			Thread.MemoryBarrier(); // _producerCursor must be written eventually now
		}
		
		/// <summary>
		/// Thread unsafe. It must always be called from only one thread
		/// </summary>
		/// <param name="item">The dequeued item if success, null otherwise</param>
		/// <returns>Whether it was possible to dequeue an item, only possible if there are queued items</returns>
		public bool TryDequeue(out T item) {
			Thread.MemoryBarrier();
			item = _entries[_consumerCursor];
			if (item != null) {
				_entries[_consumerCursor] = null;
				Thread.MemoryBarrier();
				_consumerCursor = _consumerCursor == _entries.Length - 1 ? 0 : _consumerCursor + 1;
				return true;
			}
			return false;
		}
	}

	internal static class Volatile {
		private const int CacheLineSize = 64;
		private const int IntSize = 4;

		[StructLayout(LayoutKind.Explicit, Size = CacheLineSize * 2 - IntSize)]
		public struct PaddedInt {
			[FieldOffset(CacheLineSize - IntSize)]
			public int value;

			public PaddedInt(int value) {
				this.value = value;
			}

			public override string ToString() {
				return value.ToString();
			}
		}

		[StructLayout(LayoutKind.Explicit, Size = CacheLineSize * 2 - IntSize)]
		public struct PaddedVolatileInt {
			[FieldOffset(CacheLineSize - IntSize)]
			public volatile int value;

			public PaddedVolatileInt(int value) {
				this.value = value;
			}

			public override string ToString() {
				return value.ToString();
			}
		}
	}
}
