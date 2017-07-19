using System;
using System.Collections;
using System.Collections.Generic;

public static class ArrayUtility {
	public static void Add<T, T2>(ref T[] array, T2 item) where T2 : T {
		if (array == null) array = new T[1];
		else Array.Resize(ref array, array.Length + 1);
		array[array.Length - 1] = item;
	}
	public static T[] Add<T, T2>(this T[] array, T2 item) where T2 : T {
		if (array == null) array = new T[1];
		else Array.Resize(ref array, array.Length + 1);
		array[array.Length - 1] = item;
		return array;
	}

	public static bool ArrayEquals<T>(this T[] lhs, T[] rhs) {
		if (lhs.Length != rhs.Length) {
			return false;
		}
		for (int i = 0; i < lhs.Length; i++) {
			if (!lhs[i].Equals(rhs[i])) {
				return false;
			}
		}
		return true;
	}

	public static void AddRange<T>(ref T[] array, T[] items) {
		if (array == null) array = new T[items.Length];
		int num = array.Length;
		Array.Resize(ref array, array.Length + items.Length);
		for (int i = 0; i < items.Length; i++) {
			array[num + i] = items[i];
		}
	}

	public static void Insert<T>(ref T[] array, int index, T item) {
		if (array == null) array = new T[1];
		ArrayList arrayList = new ArrayList();
		arrayList.AddRange(array);
		arrayList.Insert(index, item);
		array = (arrayList.ToArray(typeof(T)) as T[]);
	}

	public static void Remove<T>(ref T[] array, T item) {
		if (array == null) array = new T[0];
		List<T> list = new List<T>(array);
		list.Remove(item);
		array = list.ToArray();
	}

	public static void RemoveAnyOrder<T>(ref T[] array, object item, bool onlyFirstOccurrence = true) {
		if (array == null) array = new T[0];
		int l = array.Length;
		for (int i = 0; i < l;) {
			if (array[i].Equals(item)) {
				array[i] = array[l - 1];
				l--;
				if (onlyFirstOccurrence) break;
			} else {
				i++;
			}
		}
		var newArr = new T[l];
		Array.Copy(array, 0, newArr, 0, l);
		array = newArr;
	}

	public static List<T> FindAll<T>(this T[] array, Predicate<T> match) {
		if (array == null) return new List<T>();
		List<T> list = new List<T>(array);
		return list.FindAll(match);
	}

	public static T Find<T>(this T[] array, Predicate<T> match) {
		if (array == null) return default(T);
		List<T> list = new List<T>(array);
		return list.Find(match);
	}

	public static int FindIndex<T>(this T[] array, Predicate<T> match) {
		if (array == null) return -1;
		List<T> list = new List<T>(array);
		return list.FindIndex(match);
	}

	public static int IndexOf<T>(this T[] array, T value) {
		if (array == null) return -1;
		List<T> list = new List<T>(array);
		return list.IndexOf(value);
	}

	public static int LastIndexOf<T>(this T[] array, T value) {
		if (array == null) return -1;
		List<T> list = new List<T>(array);
		return list.LastIndexOf(value);
	}

	public static void RemoveAt<T>(ref T[] array, int index) {
		if (array == null) array = new T[0];
		List<T> list = new List<T>(array);
		list.RemoveAt(index);
		array = list.ToArray();
	}
	public static T[] RemoveAt<T>(this T[] array, int index) {
		if (array == null) array = new T[0];
		List<T> list = new List<T>(array);
		list.RemoveAt(index);
		array = list.ToArray();
		return array;
	}

	public static bool Contains<T>(this T[] array, T item) {
		if (array == null) return false;
		List<T> list = new List<T>(array);
		return list.Contains(item);
	}

	public static void Clear<T>(ref T[] array) {
		if (array == null) array = new T[0];
		Array.Clear(array, 0, array.Length);
		Array.Resize(ref array, 0);
	}

	public static void Swap<T>(this T[] array, int i, int j) {
		T t = array[i];
		array[i] = array[j];
		array[j] = t;
	}

	public static void Swap<T>(this IList<T> array, int i, int j) {
		T t = array[i];
		array[i] = array[j];
		array[j] = t;
	}

	public static T[] GetCopy<T>(this T[] array) {
		T[] copy = new T[array.Length];
		array.CopyTo(copy, 0);
		return copy;
	}
}
