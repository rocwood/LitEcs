#if false

using System.Runtime.CompilerServices;

namespace SlimECS
{
	public partial class Context
	{
#if false
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasComponent<T>(Entity e) where T : struct, IComponent => Has<T>(e);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetComponent<T>(Entity e, out T value) where T : struct, IComponent => Get(e, out value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetComponent<T>(Entity e) where T : struct, IComponent => Get<T>(e);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetComponent<T>(Entity e, T value) where T : struct, IComponent => Set(e, value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveComponent<T>(Entity e) where T : struct, IComponent => Remove<T>(e);
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has<T>(Entity e) where T : struct, IComponent
		{
			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id)
				return false;

			int componentIndex = ComponentTypeInfo<T>.index;
			return d.components[componentIndex] >= 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Get<T>(Entity e, out T value) where T : struct, IComponent
		{
			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id)
			{
				value = default;
				return false;
			}

			int componentIndex = ComponentTypeInfo<T>.index;
			int index = d.components[componentIndex];
			if (index < 0)
			{
				value = default;
				return false;
			}

			var c = GetComponentDataPool<T>();
			value = c.pool.items[index];

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get<T>(Entity e) where T : struct, IComponent
		{
			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id)
				return default;

			int componentIndex = ComponentTypeInfo<T>.index;
			int index = d.components[componentIndex];
			if (index < 0)
				return default;

			var c = GetComponentDataPool<T>();
			return c.pool.items[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Set<T>(Entity e, T value) where T : struct, IComponent
		{
			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id)
				return default;

			var c = GetComponentDataPool<T>();
			if (c == null)
				return false;

			c.Set(e.id, e.slot, value);

			_entities.SetChanged(e);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Ref<T>(Entity e) where T : struct, IComponent
		{
			//if (e.id <= 0)
			//	return ref EmptyComponentRef<T>.value;

			var c = GetComponentDataPool<T>();
			//if (c == null)
			//	return ref EmptyComponentRef<T>.value;

			return ref c.Ref(e.id, e.slot);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<T>(Entity e) where T : struct, IComponent
		{
			if (e.id <= 0)
				return false;

			var c = GetComponentDataPool<T>();
			if (c == null)
				return false;

			if (!c.Remove(e.id, e.slot))
				return false;

			_entities.SetChanged(e);
			return true;
		}
	}
}

#endif
