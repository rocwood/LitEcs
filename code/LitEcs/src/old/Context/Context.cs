using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public partial class Context
	{
		public static int DefaultEntityCapacity = 256;
		public static int DefaultComponentPoolCapacity = 128;

		private readonly string _contextName;

		private readonly IComponentDataPool[] _componentPools;
		internal readonly StructDataPool<EntityData> _entities;

		private int _lastId;
		private int _entityCount;

		private bool _hasDestroy = false;
		private bool _hasChanged = false;

		private readonly Dictionary<int, int> _destroyMap = new Dictionary<int, int>();
		private readonly Dictionary<int, int> _changedMap = new Dictionary<int, int>();

		internal struct EntityData
		{
			public int id;
			public int[] components;
			public bool destroy;
			public bool changed;
			public string name;
		}

		public Context(string contextName = null)
		{
			_contextName = contextName;

			_entities = new StructDataPool<EntityData>(DefaultEntityCapacity);

			var componentInfoList = ContextInfo.GetComponentInfoList();
			int count = componentInfoList.Length;

			_componentPools = new IComponentDataPool[count];
			for (int i = 0; i < count; i++)
				_componentPools[i] = CreateComponentDataPool(componentInfoList[i]);

			_queryParams = new object[] { this };
		}

		public int Count => _entityCount;
		public string Name => _contextName;

		public Entity CreateEntity(string name = null)
		{
			var id = ++_lastId;
			int slot = _entities.Alloc();

			ref var d = ref _entities.items[slot];
			{
				d.id = id;
				d.name = name;
				d.changed = true;
				d.destroy = false;
				
				if (d.components == null)
				{
					d.components = new int[_componentPools.Length];
					for (int i = 0; i < _componentPools.Length; i++)
						d.components[i] = -1;
				}
			}

			_entityCount++;

			MarkChanged(id, slot);

			return new Entity(id, slot, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void MarkChanged(int id, int slot)
		{
			_changedMap[id] = slot;
			_hasChanged = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void MarkDestory(int id, int slot)
		{
			MarkChanged(id, slot);
			
			_destroyMap[id] = slot;
			_hasDestroy = true;
		}

		/*
		public void Destroy(Entity e)
		{
			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id || d.destroy)
				return;

			d.destroy = true;
			d.changed = true;

			_changedMap[e.id] = e.slot;
			_destroyMap[e.id] = e.slot;

			_hasToDestroy = true;
			_hasChanged = true;
		}

		public void SetName(Entity e, string name)
		{
			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id)
				return;

			d.name = name;
		}

		public bool IsActive(Entity e)
		{
			ref var d = ref _entities.items[e.slot];
			return d.id == e.id && !d.destroy;
		}
		*/

		public void Poll()
		{
			HandleGroupChanges();

			CollectDestroyEntities();
		}

		internal void CollectDestroyEntities()
		{
			if (!_hasDestroy)
				return;

			foreach (var kv in _destroyMap)
			{
				int slot = kv.Value;

				ref var e = ref _entities.items[slot];
				e.id = 0;
				e.destroy = false;
				e.changed = false;
				e.name = null;

				// TODO
				//components
				
				_entities.Release(slot);
				_entityCount--;
			}

			_destroyMap.Clear();
			_hasDestroy = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentDataPool<T> GetComponentDataPool<T>() where T : struct, IComponent
		{
			int componentIndex = ComponentTypeInfo<T>.index;
			return (ComponentDataPool<T>)_componentPools[componentIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComponentDataPool<T> GetComponentDataPool<T>(int componentIndex) where T : struct, IComponent
		{
			return (ComponentDataPool<T>)_componentPools[componentIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IComponentDataPool CreateComponentDataPool(ComponentTypeInfo info)
		{
			if (info == null || info.type == null)
				return null;

			var cType = /*info.zeroSize
				? typeof(ZeroSizeComponentDataList<>).MakeGenericType(info.type) // TODO: check il2cpp failure
				: */typeof(ComponentDataPool<>).MakeGenericType(info.type);

			if (cType == null)
				return null;

			return (IComponentDataPool)Activator.CreateInstance(cType, DefaultComponentPoolCapacity);
		}
	}
}
