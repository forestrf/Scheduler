using System;
using System.Threading;

namespace Ashkatchap.Scheduler.Collections {
	internal class ThreadSafeRingBuffer_MultiProducer_SingleConsumerInt {
		private readonly int[] _entries;
		private readonly int lengthMask;
		private int _consumerCursor = 0;
		private Volatile.PaddedVolatileInt _producerCursor = new Volatile.PaddedVolatileInt();
		private Thread consumer;

		public ThreadSafeRingBuffer_MultiProducer_SingleConsumerInt(ushort powerOfTwoForCapacity, Thread consumer) {
			_entries = new int[1 << powerOfTwoForCapacity];
			lengthMask = (1 << powerOfTwoForCapacity) - 1;
			this.consumer = consumer;
		}
		
		/// <summary>
		/// Thread safe Enqueue from any thread
		/// </summary>
		public bool Enqueue(int obj) {
			if (0 == obj) return false; // Null not allowed

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
		public bool TryDequeue(out int item) {
			if (Thread.CurrentThread != consumer) {
				item = 0;
				Console.WriteLine("Not allowed");
				return false;
			}

			item = _entries[_consumerCursor];
			if (0 != item) {
				_entries[_consumerCursor] = 0;
				_consumerCursor = _consumerCursor == _entries.Length - 1 ? 0 : _consumerCursor + 1;
				return true;
			}
			return false;
		}

		internal int GetApproxLength() {
			int indexToWriteOn = _producerCursor.value;
			int offset = (indexToWriteOn - _consumerCursor) & lengthMask;
			return offset;
		}
	}
}
