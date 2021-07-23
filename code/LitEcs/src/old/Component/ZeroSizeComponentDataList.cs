#if false

namespace SlimECS
{
	internal class ZeroSizeComponentDataList<T> : IComponentDataList<T> where T : struct, IComponent
	{
		private StructArray<int> _data;

		public ZeroSizeComponentDataList()
		{
			_data = new StructArray<int>(0);
		}

		public bool Has(int entityId, int slot)
		{
			if (entityId <= 0 || slot < 0 || slot >= _data.Length)
				return false;

			return _data[slot] == entityId;
		}

		public bool Get(int entityId, int slot, out T value)
		{
			value = default;

			return Has(entityId, slot);
		}

		public T Get(int entityId, int slot)
		{
			return default;
		}

		public void Set(int entityId, int slot, T value)
		{
			if (entityId <= 0 || slot < 0)
				return;

			_data.EnsureAccess(slot);

			_data[slot] = entityId;
		}

		public ref T Ref(int entityId, int slot)
		{
			return ref EmptyComponentRef<T>.value;
		}

		public bool Remove(int entityId, int slot)
		{
			if (entityId <= 0 || slot < 0 || slot >= _data.Length)
				return false;

			// TODO: onRemove

			_data[slot] = 0;
			return true;
		}
	}
}

#endif
