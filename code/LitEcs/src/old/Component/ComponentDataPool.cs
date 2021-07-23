using System;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public interface IComponentDataPool
	{
		bool Has(int slot);
		void Remove(int slot);
	}

	public class ComponentDataPool<T> : IComponentDataPool where T:struct, IComponent
	{
	//	readonly Type _type;
	//	readonly EcsWorld _world;
	//	readonly int _id;
	//	readonly AutoResetHandler _autoReset;
		// 1-based index.
		T[] _denseItems;
		int[] _sparseItems;
		int _denseItemsCount;
		int[] _recycledItems;
		int _recycledItemsCount;

//#if ENABLE_IL2CPP && !UNITY_EDITOR
//        T _autoresetFakeInstance;
//#endif

		public ComponentDataPool(int denseCapacity, int sparseCapacity)
		{
			//_type = typeof(T);
			//_world = world;
			//_id = id;
			_denseItems = new T[denseCapacity + 1];
			_sparseItems = new int[sparseCapacity];
			_denseItemsCount = 1;
			_recycledItems = new int[512];
			_recycledItemsCount = 0;

			/*
			var isAutoReset = typeof(IEcsAutoReset<T>).IsAssignableFrom(_type);
#if DEBUG
            if (!isAutoReset && _type.GetInterface ("IEcsAutoReset`1") != null) {
                throw new Exception ($"IEcsAutoReset should have <{typeof (T).Name}> constraint for component \"{typeof (T).Name}\".");
            }
#endif
			if (isAutoReset)
			{
				var autoResetMethod = typeof(T).GetMethod(nameof(IEcsAutoReset<T>.AutoReset));
#if DEBUG
                if (autoResetMethod == null) {
                    throw new Exception (
                        $"IEcsAutoReset<{typeof (T).Name}> explicit implementation not supported, use implicit instead.");
                }
#endif
				_autoReset = (AutoResetHandler)Delegate.CreateDelegate(
					typeof(AutoResetHandler),
#if ENABLE_IL2CPP && !UNITY_EDITOR
                    _autoresetFakeInstance,
#else
					null,
#endif
					autoResetMethod);
			}
			*/
		}

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetId()
		{
			return _id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Type GetComponentType()
		{
			return _type;
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Resize(int capacity)
		{
			Array.Resize(ref _sparseItems, capacity);
		}

		/*
		object IEcsPool.GetRaw(int entity)
		{
			return _denseItems[_sparseItems[entity]];
		}

		public T[] GetRawDenseItems()
		{
			return _denseItems;
		}

		public int[] GetRawSparseItems()
		{
			return _sparseItems;
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(int entity)
		{
#if DEBUG
            //if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
            //if (_sparseItems[entity] > 0) { throw new Exception ("Already attached."); }
#endif

			if (_sparseItems[entity] > 0)
				return ref _denseItems[_sparseItems[entity]];

			int idx;
			if (_recycledItemsCount > 0)
			{
				idx = _recycledItems[--_recycledItemsCount];
			}
			else
			{
				idx = _denseItemsCount;

				if (_denseItemsCount == _denseItems.Length)
					Array.Resize(ref _denseItems, _denseItemsCount << 1);

				_denseItemsCount++;
				//_autoReset?.Invoke(ref _denseItems[idx]);
			}
			_sparseItems[entity] = idx;

			//_world.OnEntityChange(entity, _id, true);
			//_world.Entities[entity].ComponentsCount++;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            //_world.RaiseEntityChangeEvent (entity);
#endif
			return ref _denseItems[idx];
		}

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(int entity)
		{
#if DEBUG
            if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
            if (_sparseItems[entity] == 0) { throw new Exception ("Not attached."); }
#endif
			return ref _denseItems[_sparseItems[entity]];
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int entity)
		{
#if DEBUG
            //if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
			return _sparseItems[entity] > 0;
		}

		public void Remove(int entity)
		{
#if DEBUG
            //if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
			ref var sparseData = ref _sparseItems[entity];
			if (sparseData > 0)
			{
				//_world.OnEntityChange(entity, _id, false);
				if (_recycledItemsCount == _recycledItems.Length)
					Array.Resize(ref _recycledItems, _recycledItemsCount << 1);

				_recycledItems[_recycledItemsCount++] = sparseData;

				//if (_autoReset != null)
				//	_autoReset.Invoke(ref _denseItems[sparseData]);
				//else
					_denseItems[sparseData] = default;

				sparseData = 0;

				/*
				ref var entityData = ref _world.Entities[entity];
				entityData.ComponentsCount--;
#if DEBUG || LEOECSLITE_WORLD_EVENTS
                _world.RaiseEntityChangeEvent (entity);
#endif
				if (entityData.ComponentsCount == 0)
				{
					_world.DelEntity(entity);
				}
				*/
			}
		}

		public readonly StructDataPool<T> pool = new StructDataPool<T>();

		/*
		public readonly StructArray<int> remap;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int slot)
		{
			return remap[slot] > 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(int slot)
		{
			//remap.EnsureAccess(slot);

			ref var r = ref remap.Ref(slot);
			if (r == 0)
				r = pool.Alloc();

			return ref pool.items[r];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Remove(int slot)
		{
			ref var r = ref remap.Ref(slot);
			if (r == 0)
				return;

			pool.Release(r);
			r = 0;
		}

		public ComponentDataPool(int capacity = 0)
		{
			pool = new StructDataPool<T>(capacity);
			remap = new StructArray<int>(capacity);

			pool.Alloc();
		}
		*/
	}
}
