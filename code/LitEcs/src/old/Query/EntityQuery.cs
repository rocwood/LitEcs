using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public abstract class EntityQuery
	{
		internal EntitySet _entities = new EntitySet();

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _entities.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity GetAt(int index) => _entities.GetAt(index);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref Entity RefAt(int index) => ref _entities._items[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntitySet.Enumerator GetEnumerator() => _entities.GetEnumerator();

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void HandleEntity(Entity e)
		{
			if (e.id <= 0)
				return;

			if (_context.IsMatch(e, _indices, _matchAny))
				_entities.Add(e);
			else
				_entities.Remove(e);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void HandleAddEntity(Entity e) => _entities.Add(e);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void HandleRemoveEntity(Entity e) => _entities.Remove(e);
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void make(params int[] indices) => this._indices = indices;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static int inc<T>() where T : struct, IComponent => ComponentTypeInfo<T>.index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static int outc<T>() where T : struct, IComponent => -ComponentTypeInfo<T>.index-1;

		protected EntityQuery(Context c, bool matchAny)
		{
			_context = c;
			_matchAny = matchAny;
		}

		protected readonly Context _context;
		internal readonly bool _matchAny;
		internal int[] _indices;
	}
}
