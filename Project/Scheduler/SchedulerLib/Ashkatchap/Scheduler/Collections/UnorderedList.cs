using System;
using System.Text;

namespace Ashkatchap.Scheduler.Collections {
	/// <summary>
	/// Fast list that doesn't maintain the position of the objects.
	/// Useful when the index of an object in the list is not important after adding/deleting objects
	/// There are "Length" elements inside "elements" without spaces between them
	/// Not thread safe
	/// </summary>
	/// <typeparam name="T">Type of the objects of the list</typeparam>
	internal class UnorderedList<T> {
		public static readonly int DEFAULT_INITIAL_LENGTH = 64;
		public static readonly int DEFAULT_STEP_INCREMENT = 64;

		public int Count;
		int step_increment;
		public T[] elements;

		/// <summary>
		/// Create a new dynamic unordered Array. Its indices may and will vary after a RemoveAt call
		/// </summary>
		public UnorderedList() : this(DEFAULT_INITIAL_LENGTH) { }

		/// <summary>
		/// Create a new dynamic unordered Array. Its indices may and will vary after a RemoveAt call
		/// </summary>
		/// <param name="initial_length">Starting Array Size</param>
		public UnorderedList(int initial_length) : this(initial_length, DEFAULT_STEP_INCREMENT) { }

		/// <summary>
		/// Create a new dynamic unordered Array. Its indices may and will vary after a RemoveAt call
		/// </summary>
		/// <param name="initialLength">Starting Array Size</param>
		/// <param name="stepIncrement">Expand the array by this number of extra elements after a Add call when it is full</param>
		public UnorderedList(int initialLength, int stepIncrement) : this(new T[initialLength], 0, stepIncrement) { }

		public UnorderedList(T[] internalArray, int internalLength, int stepIncrement) {
			this.Count = internalLength;
			this.elements = internalArray;
			this.step_increment = stepIncrement;
		}

		public T this[int index] {
			get { return elements[index]; }
			set { elements[index] = value; }
		}

		/// <summary>
		/// Adds an Element to the Array
		/// </summary>
		/// <param name="element">Element to Add</param>
		/// <returns>true if sucess, false if can't increase the array elements</returns>
		public bool Add(T element) {
			if (Count == elements.Length) {
				if (step_increment == 0) return false;

				T[] newElements = new T[Count + step_increment];
				elements.CopyTo(newElements, 0);
				elements = newElements;
			}
			elements[Count++] = element;
			return true;
		}

		/// <summary>
		/// Adds an Element to the Array at the specified position
		/// </summary>
		/// <param name="element">Element to Add</param>
		/// <returns>true if sucess, false if can't increase the array elements</returns>
		public bool Insert(int index, T element) {
			if (RequestSpace(1 + (index - Count + 1 > 0 ? index - Count + 1 : 0))) {
				elements[Count++] = elements[index];
				elements[index] = element;
				return true;
			}
			else return false;
		}

		/// <summary>
		/// Adds an block of the element source Array to the Array
		/// </summary>
		/// <param name="element">Element array to Add from</param>
		public bool AddBlock(T[] element) {
			return AddBlock(element, 0, element.Length);
		}

		/// <summary>
		/// Adds an block of the element source Array to the Array
		/// </summary>
		/// <param name="element">Element array to Add from</param>
		/// <param name="offset">First element</param>
		public bool AddBlock(T[] element, int offset) {
			return AddBlock(element, offset, element.Length);
		}

		/// <summary>
		/// Adds an block of the element source Array to the Array
		/// </summary>
		/// <param name="element">Element array to Add from</param>
		/// <param name="offset">First element</param>
		/// <param name="length">Number of elements</param>
		public bool AddBlock(T[] element, int offset, int length) {
			if (RequestSpace(length)) {
				Array.Copy(element, offset, elements, Count, length);
				Count += length;
				return true;
			}
			else return false;
		}

		/// <summary>
		/// If the length of the underlying array (elements) is not enough to hold its currents elements plus
		/// the number of elements indicated by "length", it will resize the array.
		/// </summary>
		/// <param name="length">Number of elements than need to fit in the array</param>
		/// <returns>True if sucess, false if can't increase the array elements</returns>
		public bool RequestSpace(int length) {
			if (Count + length > elements.Length) {
				if (step_increment == 0) return false;

				int noCaben = Count + length - elements.Length;
				int extra = step_increment * ((int) (noCaben / step_increment) + 1);
				T[] newElements = new T[elements.Length + extra];
				elements.CopyTo(newElements, 0);
				elements = newElements;
			}
			return true;
		}

		/// <summary>
		/// If the length of the underlying array (elements) is not large enough it will be resized
		/// </summary>
		/// <param name="totalLength">Total number of elements than need to fit in the array</param>
		/// <returns>True if sucess, false if can't increase the array elements</returns>
		public bool EnsureCapacity(int totalLength) {
			if (totalLength > elements.Length) {
				return RequestSpace(totalLength - Count);
			}
			return true;
		}

		/// <summary>
		/// If the length of the underlying array (elements) is not large enough it will be resized
		/// </summary>
		/// <param name="totalLength">Total number of elements than need to fit in the array</param>
		/// <returns>True if sucess, false if can't increase the array elements</returns>
		public bool SetLength(int totalLength) {
			if (EnsureCapacity(totalLength)) {
				Count = totalLength;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Delete the element of the Array on index by replacing it with the last element of the array and then setting the last element of the array to null, in that order
		/// Do not call with an unused or out of bounds index
		/// </summary>
		/// <param name="index">Index of the element to delete. It must exists and be occupied</param>
		/// <returns>Index of the element that changed to fill the removed place. Same as index</returns>
		public bool RemoveAt(int index) {
			elements[index] = elements[--Count];
			elements[Count] = default(T);
			return true;
		}

		/// <summary>
		/// Slow. Uses Find internally
		/// Delete the element by replacing it with the last element of the array
		/// Do not call with an unused or out of bounds index
		/// </summary>
		/// <param name="element">Element to delete.
		/// <returns>Index of the element that changed to fill the removed place. Same as index</returns>
		public bool Remove(T element) {
			int toDelete = IndexOf(element);
			if (toDelete != -1) return RemoveAt(toDelete);
			return false;
		}

		/// <summary>
		/// Find an element.
		/// Do not call with an unused or out of bounds index
		/// </summary>
		/// <param name="element"></param>
		/// <returns>Integer with the index of the element in the array in this moment. -1 if it is not in the list. It may change after removing an element</returns>
		public int IndexOf(T element) {
			if (element == null) {
				for (int i = 0; i < Count; i++) {
					if (elements[i] == null) return i;
				}
			}
			else {
				for (int i = 0; i < Count; i++) {
					if (element.Equals(elements[i])) return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Return the element of the Array on index and remove it from the list
		/// Do not call with an unused or out of bounds index
		/// </summary>
		/// <param name="index">Index of the element to return. It must exists and be occupied</param>
		public T ExtractAt(int index) {
			T toReturn = elements[index];
			RemoveAt(index);
			return toReturn;
		}

		/// <summary>
		/// Return the last element of the Array and remove it from the list
		/// Do not call when Length == 0
		/// </summary>
		public T ExtractLast() {
			var toReturn = elements[--Count];
			elements[Count] = default(T);
			return toReturn;
		}

		/// <summary>
		/// Return the last element of the Array
		/// Do not call when Length == 0
		/// </summary>
		public T PeekLast() {
			return elements[Count - 1];
		}

		/// <summary>
		/// Reset the array
		/// </summary>
		/// <param name="onlyResetLength">Keep references and only reset the internal Length</param>
		public void Clear(bool onlyResetLength) {
			if (onlyResetLength) {
				Count = 0;
			}
			else {
				while (Count > 0) {
					ExtractLast();
				}
			}
		}

		public override string ToString() {
			StringBuilder s = new StringBuilder();
			s.Append("Size: " + Count + " | ");
			for (int i = 0; i < Count; i++) {
				s.Append(elements[i].ToString());
				s.Append("\n");
			}
			return s.ToString();
		}

		public void Clear() {
			Clear(false);
		}

		public bool Contains(T item) {
			return IndexOf(item) != -1;
		}
	}
}
