using System.Collections.Generic;

namespace Ashkatchap.Shared.Collections {
	/// <summary>
	/// Looks like a Dictionary, but it is an array. Used when the ammount of data is so small than iterating is faster
	/// than using a Dictionary.\n
	/// It is intended to be easily swapable with an actual Dictionary if the ammount of data changes in the future
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class PlainDictionary<TKey, TValue> {
		private struct Container {
			public TKey key;
			public TValue value;

			public Container(TKey key, TValue value) {
				this.key = key;
				this.value = value;
			}
		}

		private UnorderedList<Container> list;
		
		public PlainDictionary() : this(64, 64) { }
		public PlainDictionary(int initial_length) : this(initial_length, 64) { }
		public PlainDictionary(int initial_length, int step_increment) {
			this.list = new UnorderedList<Container>(initial_length, step_increment);
		}

		public int Count {
			get { return list.Size; }
		}

		public TValue this[TKey key] {
			get {
				for (int i = 0; i < list.Size; i++) {
					if (list.elements[i].key.Equals(key)) {
						return list.elements[i].value;
					}
				}
				return default(TValue);
			}
			set {
				for (int i = 0; i < list.Size; i++) {
					if (list.elements[i].key.Equals(key)) {
						list.elements[i].value = value;
						return;
					}
				}
				list.Add(new Container(key, value));
			}
		}

		public void Add(TKey key, TValue value) {
			this[key] = value;
		}

		public bool ContainsKey(TKey key) {
			for (int i = 0; i < list.Size; i++) {
				if (list.elements[i].key.Equals(key)) {
					return true;
				}
			}
			return false;
		}

		public bool Remove(TKey key) {
			for (int i = 0; i < list.Size; i++) {
				if (list.elements[i].key.Equals(key)) {
					list.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value) {
			for (int i = 0; i < list.Size; i++) {
				if (list.elements[i].key.Equals(key)) {
					value = list.elements[i].value;
					return true;
				}
			}
			value = default(TValue);
			return false;
		}

		public void Add(KeyValuePair<TKey, TValue> item) {
			Add(item.Key, item.Value);
		}

		public void Clear(bool onlyResetLength) {
			list.Clear(onlyResetLength);
		}
	}
}
