using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public class EntitySet
	{
		private const int DefaultCapacity = 16;

		private readonly Dictionary<int, int> _lookup;
		public Entity[] _items;
		private int _count;

		public EntitySet(int capacity = 0)
		{
			if (capacity <= 0)
				capacity = DefaultCapacity;

			_items = new Entity[capacity];
			_lookup = new Dictionary<int, int>(capacity);
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity GetAt(int index)
		{
			return _items[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(Entity e)
		{
			int id = e.id;
			if (id <= 0)
				return;

			if (_lookup.TryGetValue(id, out var index))
			{
				_items[index] = e;
			}
			else
			{
				if (_count >= _items.Length)
					EnsureLength(_count + 1);

				_items[_count] = e;
				_lookup[id] = _count;

				_count++;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(Entity e)
		{
			if (_count <= 0)
				return;

			int id = e.id;
			if (id <= 0)
				return;

			if (!_lookup.TryGetValue(id, out var index))
				return;

			_lookup.Remove(id);

			int last = _count - 1;
			if (index < last)
			{
				ref var el = ref _items[last];
				
				_items[index] = el;
				_lookup[el.id] = index;

				el = default;
			}

			_count--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureLength(int minSize)
		{
			int size = _items.Length;
			if (size >= minSize)
				return;

			if (size <= 0)
				size = DefaultCapacity;

			while (size < minSize)
				size *= 2;

			if (size > _items.Length)
				Array.Resize(ref _items, size);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public struct Enumerator //: IEnumerator<int>
		{
			//private readonly EntitySet _container;
			private Entity[] _items;
			private int _index;
			private int _count;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Enumerator(EntitySet container)
			{
				//_container = container;

				_items = container._items;
				_count = container._count;
				_index = -1;
			}

			public int Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => _items[_index].slot;
			}

			/*
			object IEnumerator.Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => this.Current;
			}
			*/

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				return ++_index < _count;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Reset()
			{
				_index = -1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Dispose()
			{
			}
		}
	}
}
