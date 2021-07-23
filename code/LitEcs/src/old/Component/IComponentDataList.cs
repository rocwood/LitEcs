#if false

namespace SlimECS
{
	internal interface IComponentDataList
	{
		bool Has(int entityId, int slot);
		bool Remove(int entityId, int slot);
	}

	internal interface IComponentDataList<T> : IComponentDataList
	{
		/*
		void Set(int entityId, int slot, T value);

		bool Get(int entityId, int slot, out T value);
		T Get(int entityId, int slot);
		ref T Ref(int entityId, int slot);
		*/
	}
}

#endif
