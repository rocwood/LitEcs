using System.Runtime.CompilerServices;

namespace SlimECS
{
	public static class EntityOp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Destroy(this Entity e)
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			d.destroy = true;
			d.changed = true;

			e.context.MarkDestory(e.id, e.slot);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetName(this Entity e, string name)
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			d.name = name;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty(this Entity e)
		{
			return e.context == null || e.id <= 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsActive(this Entity e)
		{
			ref var d = ref e.context._entities.items[e.slot];
			return d.id == e.id && !d.destroy;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this Entity e) where T : struct, IComponent
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			int componentIndex = ComponentTypeInfo<T>.index;
			return d.components[componentIndex] >= 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Get<T>(this Entity e, out T value) where T : struct, IComponent
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			int componentIndex = ComponentTypeInfo<T>.index;
			int index = d.components[componentIndex];
			if (index < 0)
			{
				value = default;
				return false;
			}

			var c = e.context.GetComponentDataPool<T>(componentIndex);
			value = c.pool.items[index];

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Get<T>(this Entity e) where T : struct, IComponent
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			int componentIndex = ComponentTypeInfo<T>.index;
			int index = d.components[componentIndex];
			if (index < 0)
				return default;

			var c = e.context.GetComponentDataPool<T>(componentIndex);
			return c.pool.items[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Set<T>(this Entity e, T value) where T : struct, IComponent
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			int componentIndex = ComponentTypeInfo<T>.index;
			var c = e.context.GetComponentDataPool<T>(componentIndex);

			int index = d.components[componentIndex];
			if (index < 0)
			{
				d.components[componentIndex] = index = c.pool.Alloc();
				e.context.MarkChanged(e.id, e.slot);
			}

			c.pool.items[index] = value;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Ref<T>(this Entity e, ComponentDataPool<T> c = null) where T : struct, IComponent
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			int componentIndex = ComponentTypeInfo<T>.index;
			if (c == null)
				c = e.context.GetComponentDataPool<T>(componentIndex);

			int index = d.components[componentIndex];
			if (index < 0)
			{
				d.components[componentIndex] = index = c.pool.Alloc();
				e.context.MarkChanged(e.id, e.slot);
			}

			return ref c.pool.items[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Remove<T>(this Entity e) where T : struct, IComponent
		{
			ref var d = ref e.context._entities.items[e.slot];
#if DEBUG
			if (d.id != e.id)
				throw new InvalidEntityException(e);
#endif

			int componentIndex = ComponentTypeInfo<T>.index;
			int index = d.components[componentIndex];
			if (index < 0)
				return false;

			var c = e.context.GetComponentDataPool<T>(componentIndex);
			c.pool.Release(index);
			d.components[componentIndex] = -1;

			e.context.MarkChanged(e.id, e.slot);

			return true;
		}
	}
}

