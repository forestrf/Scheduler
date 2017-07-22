using System;
using UnityEngine;

namespace Ashkatchap.Shared.Collections {
	public class CircularBuffer<T> {
		const int DEFAULT_CAPACITY = 16;
		const int DEFAULT_INCREMENT = 16;

		/// <summary>
		/// Don't touch this unless you know what you are doing
		/// </summary>
		public T[] array; // from left to right, oldest to newest
		int increment;

		/// <summary>
		/// Don't touch this unless you know what you are doing
		/// </summary>
		public int tailIndex = 0;

		/// <summary>
		/// Don't touch this unless you know what you are doing
		/// </summary>
		public int headIndex = 0;

		/// <summary>
		/// Don't touch this unless you know what you are doing
		/// </summary>
		public int Length = 0;
		

		/// <summary>
		/// Don't use negative numbers. Supports cyclic indexing
		/// </summary>
		public T this[int index] {
			get { return array[(tailIndex + index) % array.Length]; }
			set { array[(tailIndex + index) % array.Length] = value; }
		}

		/// <param name="index">>=0</param>
		public int GetInternalArrayIndex(int index) {
			int i = tailIndex + index;
			return i >= array.Length ? i % array.Length : i;
		}

		public CircularBuffer(int initialCapacity, int increment) {
			array = new T[initialCapacity > 0 ? initialCapacity : DEFAULT_CAPACITY];
			this.increment = increment >= 0 ? increment : DEFAULT_INCREMENT;
			NormalEnumerator = new NormalEnumeratorImp(this);
			ReversedEnumerator = new ReversedEnumeratorImp(this);
		}
		public CircularBuffer() : this(DEFAULT_CAPACITY, DEFAULT_INCREMENT) { }


		public void Enqueue(T element) {
			if (Length == array.Length) {
				if (increment > 0) {
					// Need a bigger array
					var tmp = new T[array.Length + increment];
					Array.Copy(array, 0, tmp, 0, headIndex);
					int newStartIndex = tmp.Length - (array.Length - tailIndex);
					Array.Copy(array, tailIndex, tmp, newStartIndex, array.Length - tailIndex);
					tailIndex = newStartIndex;
					array = tmp;
				} else {
					Dequeue();
				}
			}

			array[headIndex] = element;
			headIndex = (headIndex + 1) % array.Length;
			Length++;
		}
		public T ExtractLastQueued() {
			if (Length > 0) {
				Length--;
				headIndex = Mod(headIndex - 1, array.Length);
				var toReturn = array[headIndex];
				array[headIndex] = default(T);
				return toReturn;
			} else {
				Debug.LogError("Trying to ExtractLastQueued without elements in the collection");
				return default(T);
			}
		}
		public T PeekLastQueued() {
			if (Length > 0) {
				int index = Mod(headIndex - 1, array.Length);
				return array[index];
			} else {
				Debug.LogError("Trying to ExtractLastQueued without elements in the collection");
				return default(T);
			}
		}
		public T Dequeue() {
			if (Length > 0) {
				Length--;
				var toReturn = array[tailIndex];
				array[tailIndex] = default(T);
				tailIndex = (tailIndex + 1) % array.Length;
				return toReturn;
			} else {
				Debug.LogError("Trying to Dequeue without elements in the collection");
				return default(T);
			}
		}
		public T PeekDequeue() {
			if (Length > 0) {
				return array[tailIndex];
			} else {
				Debug.LogError("Trying to PeekDequeue without elements in the collection");
				return default(T);
			}
		}

		/// <param name="fastMode">Only change the length without cleaning the inside of the internal array</param>
		public void Clear(bool fastMode = true) {
			if (fastMode) {
				tailIndex = 0;
				headIndex = 0;
				Length = 0;
			} else {
				while (Length > 0) {
					Dequeue();
				}
				tailIndex = 0;
				headIndex = 0;
			}
		}

		/// <summary>
		/// Use only if x may be negative, otherwise use %
		/// </summary>
		static int Mod(int x, int m) {
			return (x % m + m) % m;
		}


		public NormalEnumeratorImp NormalEnumerator;
		public ReversedEnumeratorImp ReversedEnumerator;


		public class NormalEnumeratorImp {
			CircularBuffer<T> queue;
			public NormalEnumeratorImp(CircularBuffer<T> queue) {
				this.queue = queue;
			}
			public Enumerator GetEnumerator() {
				return new Enumerator(queue);
			}

			public struct Enumerator {
				private readonly CircularBuffer<T> queue;
				private int index;

				public Enumerator(CircularBuffer<T> queue) {
					this.queue = queue;
					index = -1;
				}

				public T Current {
					get { return queue[index]; }
				}

				public bool MoveNext() {
					index++;
					return index < queue.Length;
				}

				public void Reset() {
					index = -1;
				}
			}
		}
		public class ReversedEnumeratorImp {
			CircularBuffer<T> queue;
			public ReversedEnumeratorImp(CircularBuffer<T> queue) {
				this.queue = queue;
			}

			public Enumerator GetEnumerator() {
				return new Enumerator(queue);
			}

			public struct Enumerator {
				private readonly CircularBuffer<T> queue;
				private int index;

				public Enumerator(CircularBuffer<T> queue) {
					this.queue = queue;
					index = queue.Length;
				}

				public T Current {
					get { return queue[index]; }
				}

				public bool MoveNext() {
					index--;
					return index >= 0;
				}

				public void Reset() {
					index = queue.Length;
				}
			}
		}
	}
}
