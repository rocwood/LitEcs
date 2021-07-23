using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace LitEcs
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public sealed class World
	{
		internal EntityData[] Entities;
		int _entitiesCount;
		int _uniqueId;

		int[] _recycledEntities;
		int _recycledEntitiesCount;
		
		readonly IComponentPool[] _pools;
		//int _poolsCount;
		readonly int _poolDenseSize;

		//readonly Dictionary<Type, IComponentPool> _poolHashes;
		readonly Dictionary<int, Filter> _hashedFilters;

		readonly List<Filter> _allFilters;
		List<Filter>[] _filtersByIncludedComponents;
		List<Filter>[] _filtersByExcludedComponents;

		bool _destroyed;

		public World(in Config cfg = default)
		{
			// entities.
			var capacity = cfg.Entities > 0 ? cfg.Entities : Config.EntitiesDefault;
			Entities = new EntityData[capacity + 1];
			
			capacity = cfg.RecycledEntities > 0 ? cfg.RecycledEntities : Config.RecycledEntitiesDefault;
			_recycledEntities = new int[capacity];
			_entitiesCount = 1;
			_recycledEntitiesCount = 0;
			_uniqueId = 0;

			// pools.
			capacity = ComponentInfo.GetComponentCount();
			_pools = new IComponentPool[capacity];
			//_poolHashes = new Dictionary<Type, IComponentPool>(capacity);
			_filtersByIncludedComponents = new List<Filter>[capacity];
			_filtersByExcludedComponents = new List<Filter>[capacity];
			//_poolsCount = 0;

			// filters.
			capacity = cfg.Filters > 0 ? cfg.Filters : Config.FiltersDefault;
			_hashedFilters = new Dictionary<int, Filter>(capacity);
			_allFilters = new List<Filter>(capacity);
			_poolDenseSize = cfg.PoolDenseSize > 0 ? cfg.PoolDenseSize : Config.PoolDenseSizeDefault;

#if DEBUG || LEOECSLITE_WORLD_EVENTS
            _eventListeners = new List<IEcsWorldEventListener> (4);
#endif
			_destroyed = false;
		}

		public void Destroy()
		{
/*
#if DEBUG
            if (CheckForLeakedEntities ()) { throw new Exception ($"Empty entity detected before EcsWorld.Destroy()."); }
#endif
*/
			_destroyed = true;

			for (var i = _entitiesCount - 1; i >= 0; i--)
			{
				ref var entityData = ref Entities[i];

				if (entityData.ComponentsCount > 0)
					RemoveEntity(i);
			}

			//_pools = Array.Empty<IComponentPool>();
			//_poolHashes.Clear();

			_hashedFilters.Clear();
			_allFilters.Clear();
			_filtersByIncludedComponents = Array.Empty<List<Filter>>();
			_filtersByExcludedComponents = Array.Empty<List<Filter>>();

#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (var ii = _eventListeners.Count - 1; ii >= 0; ii--) {
                _eventListeners[ii].OnWorldDestroyed (this);
            }
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAlive()
		{
			return !_destroyed;
		}

		public int CreateEntity()
		{
			_uniqueId++;

			int entity;

			if (_recycledEntitiesCount > 0)
			{
				entity = _recycledEntities[--_recycledEntitiesCount];

				//ref var entityData = ref Entities[entity];
				//entityData.Gen = (short)-entityData.Gen;
				Entities[entity].UniqueId = _uniqueId;
			}
			else
			{
				// new entity.
				if (_entitiesCount == Entities.Length)
				{
					// resize entities and component pools.
					var newSize = _entitiesCount << 1;
					Array.Resize(ref Entities, newSize);

					for (int i = 0, iMax = _pools.Length; i < iMax; i++)
						_pools[i].Resize(newSize);

					for (int i = 0, iMax = _allFilters.Count; i < iMax; i++)
						_allFilters[i].ResizeSparseIndex(newSize);

#if DEBUG || LEOECSLITE_WORLD_EVENTS
                    for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++)
                        _eventListeners[ii].OnWorldResized (newSize);
#endif
				}

				entity = _entitiesCount++;

				//ref var entityData = ref Entities[entity];
				//entityData.Gen = 1;
				Entities[entity].UniqueId = _uniqueId;
			}
#if DEBUG
            _leakedEntities.Add (entity);
#endif
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnEntityCreated (entity);
            }
#endif

			return entity;
		}

		public void RemoveEntity(int entity)
		{
#if DEBUG
            if (entity <= 0 || entity >= _entitiesCount)
                throw new Exception("Cant touch destroyed entity.");
#endif

			ref var entityData = ref Entities[entity];
			//if (entityData.Gen < 0)
			if (entityData.UniqueId <= 0)
				return;

			// kill components.
			if (entityData.ComponentsCount > 0)
			{
				int i = 0;
				int poolsCount = _pools.Length;

				while (entityData.ComponentsCount > 0 && i < poolsCount)
				{
					for (; i < poolsCount; i++)
					{
						var pool = _pools[i];
						if (pool != null && pool.Has(entity))
						{
							pool.Remove(entity);
							i++;
							break;
						}
					}
				}
#if DEBUG
                if (entityData.ComponentsCount != 0) 
					throw new Exception ($"Invalid components count on entity {entity} => {entityData.ComponentsCount}.");
#endif
				return;
			}

			//entityData.Gen = (short)(entityData.Gen == short.MaxValue ? -1 : -(entityData.Gen + 1));
			entityData.UniqueId = -entityData.UniqueId;

			if (_recycledEntitiesCount == _recycledEntities.Length)
				Array.Resize(ref _recycledEntities, _recycledEntitiesCount << 1);

			_recycledEntities[_recycledEntitiesCount++] = entity;

#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnEntityDestroyed(entity);
            }
#endif
		}

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetComponentsCount(int entity)
		{
			return Entities[entity].ComponentsCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short GetEntityGen(int entity)
		{
			return Entities[entity].Gen;
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetUniqueId(int entity)
		{
			return Entities[entity].UniqueId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAllocatedEntitiesCount()
		{
			return _entitiesCount;
		}

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetWorldSize()
		{
			return Entities.Length;
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentPool<T> GetPool<T>() where T : struct, IComponent
		{
			var componentIndex = ComponentInfo<T>.index;

			var pool = (ComponentPool<T>)_pools[componentIndex];
			if (pool == null)
				_pools[componentIndex] = pool = new ComponentPool<T>(this, componentIndex, _poolDenseSize, Entities.Length);

			return pool;
		}

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IComponentPool GetPoolById(int typeId)
		{
			return typeId >= 0 && typeId < _poolsCount ? _pools[typeId] : null;
		}
		*/

		public int GetAllEntities(ref int[] entities)
		{
			var count = _entitiesCount - _recycledEntitiesCount;
			if (entities == null || entities.Length < count)
			{
				entities = new int[count];
			}
			var id = 0;
			for (int i = 0, iMax = _entitiesCount; i < iMax; i++)
			{
				ref var entityData = ref Entities[i];

				// should we skip empty entities here?
				if (entityData.UniqueId > 0 /*entityData.ComponentsCount >= 0*/)
				{
					entities[id++] = i;
				}
			}
			return count;
		}

		/*
		public int GetComponents(int entity, ref object[] list)
		{
			var itemsCount = Entities[entity].ComponentsCount;
			if (itemsCount == 0) { return 0; }
			if (list == null || list.Length < itemsCount)
			{
				list = new object[_pools.Length];
			}
			for (int i = 0, j = 0, iMax = _poolsCount; i < iMax; i++)
			{
				if (_pools[i].Has(entity))
				{
					list[j++] = _pools[i].GetRaw(entity);
				}
			}
			return itemsCount;
		}

		public int GetComponentTypes(int entity, ref Type[] list)
		{
			var itemsCount = Entities[entity].ComponentsCount;
			if (itemsCount == 0) { return 0; }
			if (list == null || list.Length < itemsCount)
			{
				list = new Type[_pools.Length];
			}
			for (int i = 0, j = 0, iMax = _poolsCount; i < iMax; i++)
			{
				if (_pools[i].Has(entity))
				{
					list[j++] = _pools[i].GetComponentType();
				}
			}
			return itemsCount;
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool IsEntityAliveInternal(int entity)
		{
			return entity >= 0 && entity < _entitiesCount && Entities[entity].UniqueId > 0; //Entities[entity].Gen > 0;
		}

		/*
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EcsFilter.Mask Filter<T>() where T : struct
		{
			return EcsFilter.Mask.New(this).Inc<T>();
		}

		internal (EcsFilter, bool) GetFilterInternal(EcsFilter.Mask mask, int capacity = 512)
		{
			var hash = mask.Hash;
			var exists = _hashedFilters.TryGetValue(hash, out var filter);
			if (exists) { return (filter, false); }
			filter = new EcsFilter(this, mask, capacity, Entities.Length);
			_hashedFilters[hash] = filter;
			_allFilters.Add(filter);
			// add to component dictionaries for fast compatibility scan.
			for (int i = 0, iMax = mask.IncludeCount; i < iMax; i++)
			{
				var list = _filtersByIncludedComponents[mask.Include[i]];
				if (list == null)
				{
					list = new List<EcsFilter>(8);
					_filtersByIncludedComponents[mask.Include[i]] = list;
				}
				list.Add(filter);
			}
			for (int i = 0, iMax = mask.ExcludeCount; i < iMax; i++)
			{
				var list = _filtersByExcludedComponents[mask.Exclude[i]];
				if (list == null)
				{
					list = new List<EcsFilter>(8);
					_filtersByExcludedComponents[mask.Exclude[i]] = list;
				}
				list.Add(filter);
			}
			// scan exist entities for compatibility with new filter.
			for (int i = 0, iMax = _entitiesCount; i < iMax; i++)
			{
				ref var entityData = ref Entities[i];
				if (entityData.ComponentsCount > 0 && IsMaskCompatible(mask, i))
				{
					filter.AddEntity(i);
				}
			}
#if DEBUG || LEOECSLITE_WORLD_EVENTS
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnFilterCreated (filter);
            }
#endif
			return (filter, true);
		}

		internal void OnEntityChange(int entity, int componentType, bool added)
		{
			var includeList = _filtersByIncludedComponents[componentType];
			var excludeList = _filtersByExcludedComponents[componentType];
			if (added)
			{
				// add component.
				if (includeList != null)
				{
					foreach (var filter in includeList)
					{
						if (IsMaskCompatible(filter.GetMask(), entity))
						{
#if DEBUG
                            if (filter.SparseEntities[entity] > 0) { throw new Exception ("Entity already in filter."); }
#endif
							filter.AddEntity(entity);
						}
					}
				}
				if (excludeList != null)
				{
					foreach (var filter in excludeList)
					{
						if (IsMaskCompatibleWithout(filter.GetMask(), entity, componentType))
						{
#if DEBUG
                            if (filter.SparseEntities[entity] == 0) { throw new Exception ("Entity not in filter."); }
#endif
							filter.RemoveEntity(entity);
						}
					}
				}
			}
			else
			{
				// remove component.
				if (includeList != null)
				{
					foreach (var filter in includeList)
					{
						if (IsMaskCompatible(filter.GetMask(), entity))
						{
#if DEBUG
                            if (filter.SparseEntities[entity] == 0) { throw new Exception ("Entity not in filter."); }
#endif
							filter.RemoveEntity(entity);
						}
					}
				}
				if (excludeList != null)
				{
					foreach (var filter in excludeList)
					{
						if (IsMaskCompatibleWithout(filter.GetMask(), entity, componentType))
						{
#if DEBUG
                            if (filter.SparseEntities[entity] > 0) { throw new Exception ("Entity already in filter."); }
#endif
							filter.AddEntity(entity);
						}
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IsMaskCompatible(EcsFilter.Mask filterMask, int entity)
		{
			for (int i = 0, iMax = filterMask.IncludeCount; i < iMax; i++)
			{
				if (!_pools[filterMask.Include[i]].Has(entity))
				{
					return false;
				}
			}
			for (int i = 0, iMax = filterMask.ExcludeCount; i < iMax; i++)
			{
				if (_pools[filterMask.Exclude[i]].Has(entity))
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool IsMaskCompatibleWithout(EcsFilter.Mask filterMask, int entity, int componentId)
		{
			for (int i = 0, iMax = filterMask.IncludeCount; i < iMax; i++)
			{
				var typeId = filterMask.Include[i];
				if (typeId == componentId || !_pools[typeId].Has(entity))
				{
					return false;
				}
			}
			for (int i = 0, iMax = filterMask.ExcludeCount; i < iMax; i++)
			{
				var typeId = filterMask.Exclude[i];
				if (typeId != componentId && _pools[typeId].Has(entity))
				{
					return false;
				}
			}
			return true;
		}
		*/

		public struct Config
		{
			public int Entities;
			public int RecycledEntities;
			public int Pools;
			public int Filters;
			public int PoolDenseSize;

			internal const int EntitiesDefault = 512;
			internal const int RecycledEntitiesDefault = 512;
			internal const int PoolsDefault = 512;
			internal const int FiltersDefault = 512;
			internal const int PoolDenseSizeDefault = 512;
		}

		internal struct EntityData
		{
			public int UniqueId;
			//public short Gen;
			public short ComponentsCount;
		}


#if DEBUG || LEOECSLITE_WORLD_EVENTS
        List<IEcsWorldEventListener> _eventListeners;

        public void AddEventListener (IEcsWorldEventListener listener) {
#if DEBUG
            if (listener == null) { throw new Exception ("Listener is null."); }
#endif
            _eventListeners.Add (listener);
        }

        public void RemoveEventListener (IEcsWorldEventListener listener) {
#if DEBUG
            if (listener == null) { throw new Exception ("Listener is null."); }
#endif
            _eventListeners.Remove (listener);
        }

        public void RaiseEntityChangeEvent (int entity) {
            for (int ii = 0, iMax = _eventListeners.Count; ii < iMax; ii++) {
                _eventListeners[ii].OnEntityChanged (entity);
            }
        }
#endif

/*
#if DEBUG
        readonly List<int> _leakedEntities = new List<int> (512);

        internal bool CheckForLeakedEntities () {
            if (_leakedEntities.Count > 0) {
                for (int i = 0, iMax = _leakedEntities.Count; i < iMax; i++) {
                    ref var entityData = ref Entities[_leakedEntities[i]];
                    if (entityData.Gen > 0 && entityData.ComponentsCount == 0) {
                        return true;
                    }
                }
                _leakedEntities.Clear ();
            }
            return false;
        }
#endif
*/
	}

#if DEBUG || LEOECSLITE_WORLD_EVENTS
    public interface IEcsWorldEventListener {
        void OnEntityCreated (int entity);
        void OnEntityChanged (int entity);
        void OnEntityDestroyed (int entity);
        void OnFilterCreated (EcsFilter filter);
        void OnWorldResized (int newSize);
        void OnWorldDestroyed (EcsWorld world);
    }
#endif
}
