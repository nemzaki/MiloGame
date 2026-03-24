using Quantum.Collections;

namespace Quantum
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;

	public static unsafe partial class IListExtensions
	{
		// PUBLIC METHODS

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Find<T>(this IList<T> list, Predicate<T> condition)
		{
			int count = list.Count;

			for (int i = 0; i < count; i++)
			{
				if (condition.Invoke(list[i]) == true)
					return list[i];
			}

			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<T>(this IList<T> list, Predicate<T> condition)
		{
			int count = list.Count;

			for (int i = 0; i < count; i++)
			{
				if (condition.Invoke(list[i]) == true)
					return i;
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this IList<T> list, T item)
		{
			return list.Contains(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ContainsAll<T>(this IList<T> list, IList<T> other)
		{
			for (int i = 0; i < other.Count; i++)
			{
				if (list.Contains(other[i]) == false)
					return false;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddUnique<T>(this IList<T> list, T item)
		{
			if (list.Contains(item) == false)
			{
				list.Add(item);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Swap<T>(this IList<T> list, int indexA, int indexB)
		{
			T value = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SafeCount<T>(this IList<T> list)
		{
			return list != null ? list.Count : 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T SafeGet<T>(this IList<T> list, int index)
		{
			if (list == null)
				return default;

			if (index < 0 || index >= list.Count)
				return default;

			return list[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T LastOrDefault<T>(this IList<T> list)
		{
			int count = list.SafeCount();

			if (count == 0)
				return default(T);

			return list[count - 1];
		}

		// O(1)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveBySwap<T>(this IList<T> list, int index)
		{
			list[index] = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
		}

		// O(n)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveBySwap<T>(this IList<T> list, T item)
		{
			int index = list.IndexOf(item);
			if (index >= 0)
			{
				RemoveBySwap(list, index);
			}
		}

		// O(n)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void RemoveBySwap<T>(this List<T> list, Predicate<T> predicate)
		{
			int index = list.FindIndex(predicate);
			if (index >= 0)
			{
				RemoveBySwap(list, index);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Shuffle<T>(this IList<T> list, Frame frame)
		{
			int n = list.Count - 1;

			while (n > 0)
			{
				int k = frame.RNG->Next(0, n);

				T value = list[k];

				list[k] = list[n];
				list[n] = value;

				--n;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Shuffle<T>(this QList<T> list, Frame frame) where T : unmanaged
		{
			int n = list.Count - 1;

			while (n > 0)
			{
				int k = frame.RNG->Next(0, n);

				T value = list[k];

				list[k] = list[n];
				list[n] = value;

				--n;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Shuffle<T>(this QList<T> list, Frame frame, int count) where T : unmanaged
		{
			int n = Math.Min(count - 1, list.Count - 1);

			while (n > 0)
			{
				int k = frame.RNG->Next(0, n);

				T value = list[k];

				list[k] = list[n];
				list[n] = value;

				--n;
			}
		}
	}
}
