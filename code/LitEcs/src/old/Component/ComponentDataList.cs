#if false

using System.Runtime.CompilerServices;

namespace SlimECS
{
	public class ComponentDataList<T> : IComponentDataList<T> where T : struct, IComponent
	{
		struct ComponentData
		{
			public int entityId;
			public T component;
		}

		private StructArray<ComponentData> _data;

		public ComponentDataList()
		{
			_data = new StructArray<ComponentData>(0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int entityId, int slot)
		{
			if (entityId <= 0 || slot < 0)
				return false;

			return _data.Ref(slot).entityId == entityId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Get(int entityId, int slot, out T value)
		{
			if (entityId <= 0 || slot < 0)
			{
				value = default;
				return false;
			}

			ref var d = ref _data.Ref(slot);
			if (d.entityId != entityId)
			{
				value = default;
				return false;
			}

			value = d.component;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get(int entityId, int slot)
		{
			if (entityId <= 0 || slot < 0)
				return default;

			ref var d = ref _data.Ref(slot);
			if (d.entityId != entityId)
				return default;

			return d.component;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(int entityId, int slot, T value)
		{
			if (entityId <= 0 || slot < 0)
				return;

			_data.EnsureAccess(slot);

			ref var d = ref _data.Ref(slot);
			d.component = value;
			d.entityId = entityId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Ref(int entityId, int slot)
		{
			//if (entityId <= 0 || slot < 0)
			//	return ref EmptyComponentRef<T>.value;

			ref var d = ref _data.Ref(slot);
			//if (d.entityId != entityId)
			//	return ref EmptyComponentRef<T>.value;

			return ref d.component;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(int entityId, int slot)
		{
			if (!Has(entityId, slot))
				return false;

			// TODO: onRemove

			_data[slot] = default;
			return true;
		}
	}
}

#endif
