using System;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public class StructArray<T> where T : struct
	{
		private const int DefaultCapacity = 256;
		private const int MaxCapacity = 0x7FEFFFFF;

		private T[] _items;

		public StructArray(int capacity = 0)
		{
			if (capacity < 0)
				capacity = 0;
			else if ((uint)capacity > MaxCapacity)
				capacity = MaxCapacity;

			_items = new T[capacity];
		}

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _items.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Ref(int index) => ref _items[index];

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _items[index];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _items[index] = value;
		}

		public void EnsureAccess(int index)
		{
			int size = _items.Length;
			if (index < size)
				return;

			while (index >= size)
			{
				if (size <= 0)
					size = DefaultCapacity;
				else
					size *= 2;

				if ((uint)size > MaxCapacity)
				{
					size = MaxCapacity;
					break;
				}
			}

			if (size > _items.Length)
				Array.Resize(ref _items, size);
		}
	}
}
