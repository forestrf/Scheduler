using System.Runtime.InteropServices;
using System.Threading;

namespace Ashkatchap.Scheduler.Collections {
	/// <summary>
	/// Implementation of the Disruptor pattern
	/// </summary>
	/// <typeparam name="T">the type of item to be stored</typeparam>
	internal class ThreadSafeRingBuffer_MultiProducer_SingleConsumer<T> where T : class {
		private readonly T[] _entries;
		private readonly int lengthMask;
		private int _consumerCursor = 0;
		private Volatile.PaddedVolatileInt _producerCursor = new Volatile.PaddedVolatileInt();
		private Thread consumer;

		public ThreadSafeRingBuffer_MultiProducer_SingleConsumer(ushort powerOfTwoForCapacity, Thread consumer) {
			_entries = new T[1 << powerOfTwoForCapacity];
			lengthMask = (1 << powerOfTwoForCapacity) - 1;
			this.consumer = consumer;
		}
		
		/// <summary>
		/// Thread safe Enqueue from any thread
		/// </summary>
		public bool Enqueue(T obj) {
			if (null == obj) return false; // Null not allowed

			int indexToWriteOn;
			int nextIndex;
			do {
				Thread.MemoryBarrier(); // obtain a fresh _producerCursor
				indexToWriteOn = _producerCursor.value;
				nextIndex = (indexToWriteOn + 1) & lengthMask;
				int offset = (indexToWriteOn - _consumerCursor) & lengthMask;
				if (offset < 0) return false; // No free space
				Thread.MemoryBarrier(); // obtain a fresh _producerCursor
			} while (indexToWriteOn != Interlocked.CompareExchange(ref _producerCursor.value, nextIndex, indexToWriteOn));

			// We have an index to write
			_entries[indexToWriteOn] = obj;
			Thread.MemoryBarrier(); // _producerCursor must be written eventually now
			return true;
		}
		
		/// <summary>
		/// Thread unsafe. It must always be called from only one thread
		/// </summary>
		/// <param name="item">The dequeued item if success, null otherwise</param>
		/// <returns>Whether it was possible to dequeue an item, only possible if there are queued items</returns>
		public bool TryDequeue(out T item) {
			if (Thread.CurrentThread != consumer) {
				item = default(T);
				System.Console.WriteLine("Not allowed");
				return false;
			}

			item = _entries[_consumerCursor];
			if (null != item) {
				_entries[_consumerCursor] = null;
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
			public int value;

			public PaddedVolatileInt(int value) {
				this.value = value;
			}

			public override string ToString() {
				return value.ToString();
			}
		}
	}
}
