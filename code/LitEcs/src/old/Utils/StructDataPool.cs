using System;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public class StructDataPool<T> where T : struct
	{
		public const int DefaultCapacity = 16;

		public T[] items;
		private int _itemsCount;

		private int[] _reservedIndex;
		private int _reservedCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StructDataPool(int capacity = 0)
		{
			if (capacity <= 0)
				capacity = DefaultCapacity;

			items = new T[capacity];
			_reservedIndex = new int[capacity];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Alloc()
		{
			if (_reservedCount > 0)
				return _reservedIndex[--_reservedCount];

			int id = _itemsCount;

			if (_itemsCount >= items.Length)
				Array.Resize(ref items, _itemsCount << 1);

			_itemsCount++;

			return id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Release(int idx)
		{
			if (_reservedCount >= _reservedIndex.Length)
				Array.Resize(ref _reservedIndex, _reservedCount << 1);

			_reservedIndex[_reservedCount++] = idx;
		}
	}
}
