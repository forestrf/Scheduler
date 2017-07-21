using System.Threading;

namespace Ashkatchap.Shared.Collections {
	/// <summary>
	/// Implementation of the Disruptor pattern
	/// </summary>
	/// <typeparam name="T">the type of item to be stored</typeparam>
	public class ThreadSafeIntRingBuffer_MultiProducer_SingleConsumer {
		private readonly int[] _entries; // saved ints are stored with a positive offset of 1
		private int _consumerCursor = 0;
		private Volatile.PaddedInt _producerCursor = new Volatile.PaddedInt();
		
		public ThreadSafeIntRingBuffer_MultiProducer_SingleConsumer(ushort capacity) {
			_entries = new int[capacity];
		}
		
		public void Enqueue(int obj) {
			obj++;
			if (obj == 0) {
				Logger.Warn("Trying to Enqueue null. It is not supported because internally null represents a free place in the buffer for this algorithm.");
				return;
			}

			int indexToWriteOn;
			do {
				Thread.MemoryBarrier(); // obtain a fresh _producerCursor
				indexToWriteOn = _producerCursor.value;
			} while (0 == Interlocked.CompareExchange(ref _entries[indexToWriteOn], obj, 0));
			_producerCursor.value = _producerCursor.value == _entries.Length - 1 ? 0 : _producerCursor.value + 1;
			Thread.MemoryBarrier(); // _producerCursor must be written eventually now
		}
		
		public bool TryDequeue(out int item) {
			Thread.MemoryBarrier();
			item = _entries[_consumerCursor];
			if (item != 0) {
				_entries[_consumerCursor] = 0;
				Thread.MemoryBarrier();
				_consumerCursor = _consumerCursor == _entries.Length - 1 ? 0 : _consumerCursor + 1;

				item--;
				return true;
			} else {
				return false;
			}
		}
	}
}
