using System;
using System.Threading;

namespace Ashkatchap.Scheduler.Collections {
	internal class RingBuffer_SingleProducer_SingleConsumerStruct<T> where T : struct, IEquatable<T> {
		private readonly T[] _entries;
		private readonly int lengthMask;
		private int _consumerCursor = 0;
		private int _producerCursor = 0;
		private Thread consumer;
		private Thread producer;

		public RingBuffer_SingleProducer_SingleConsumerStruct(ushort powerOfTwoForCapacity, Thread producer, Thread consumer) {
			_entries = new T[1 << powerOfTwoForCapacity];
			lengthMask = (1 << powerOfTwoForCapacity) - 1;
			this.consumer = consumer;
			this.producer = producer;
		}

		/// <summary>
		/// Thread unsafe. It must always be called from only one thread
		/// </summary>
		public bool Enqueue(T obj) {
			if (default(T).Equals(obj)) return false; // Null not allowed
			if (Thread.CurrentThread != producer) {
				Console.WriteLine("Not allowed");
				return false;
			}

			int indexToWriteOn = _producerCursor;
			int nextIndex = (indexToWriteOn + 1) & lengthMask;
			_entries[indexToWriteOn] = obj;
			_producerCursor = nextIndex;

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
				Console.WriteLine("Not allowed");
				return false;
			}

			item = _entries[_consumerCursor];
			if (!default(T).Equals(item)) {
				_entries[_consumerCursor] = default(T);
				_consumerCursor = _consumerCursor == _entries.Length - 1 ? 0 : _consumerCursor + 1;
				return true;
			}
			return false;
		}

		internal int GetApproxLength() {
			int indexToWriteOn = _producerCursor;
			int offset = (indexToWriteOn - _consumerCursor) & lengthMask;
			return offset;
		}
	}
}
