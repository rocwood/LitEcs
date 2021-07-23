#if false

using System;
using System.Collections.Generic;

namespace SlimECS
{
	class EntityDataList
	{
		private const int DefaultCapacity = 256;
		private const int MaxCapacity = 0x7FEFFFFF;

		struct EntityData
		{
			public int id;
			public bool destroy;
			public bool changed;
			public string name;
			public int[] components;
		}

		private StructDataPool<EntityData> _entities;

		private int _lastId = 0;
		private int _entityCount = 0;

		private bool _hasToDestroy = false;
		private bool _hasChanged = false;

		private readonly Dictionary<int, int> _destroyMap = new Dictionary<int, int>();
		private readonly Dictionary<int, int> _changedMap = new Dictionary<int, int>();

		public EntityDataList(int capacity)
		{
			if (capacity <= 0)
				capacity = DefaultCapacity;

			_entities = new EntityData[capacity];

			_freeSlotHead = 0;

			InitFreeSlotLinks(0, capacity);
		}

		public int Count => _entityCount;

		/*
		internal int Capacity => _entities.Length;
		internal Entity GetAt(int slot)
		{
			int id = _entities[slot];

			return new Entity(id, slot);
		}
		internal Entity GetAt(int slot, out byte flags)
		{
			int id = _entities[slot];
			flags = _flags[slot];

			return new Entity(id, slot);
		}
		*/

		public Entity Create(string name)
		{
			var id = ++_lastId;


			int slot = _entities.Alloc();

			ref var d = ref _entities.items[slot];

			d.id = id;
			d.name = name;
			d.changed = true;
			d.destroy = false;
			//d.components = new int[];

			_entityCount++;

			_changedMap[id] = slot;
			_hasChanged = true;

			return new Entity(id, slot);
		}

		public void SetName(Entity e, string name)
		{
			if (e.id <= 0)
				return;

			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id || d.destroy)
				return;

			d.name = name;
		}

		public bool IsActive(Entity e)
		{
			if (e.id <= 0)
				return false;

			ref var d = ref _entities.items[e.slot];
			return d.id == e.id && !d.destroy;
		}

		internal void SetChanged(Entity e)
		{
			if (e.id <= 0)
				return;

			ref var d = ref _entities.items[e.slot];
			if (d.id != e.id)
				return;

			d.changed = true;

			_changedMap[e.id] = e.slot;
			_hasChanged = true;
		}

		public void Destroy(Entity e)
		{
			if (e.id <= 0 || e.slot < 0)
				return;

			ref var d = ref _entities[e.slot];
			d.destroy = true;
			d.changed = true;

			_changedMap[e.id] = e.slot;
			_destroyMap[e.id] = e.slot;

			_hasToDestroy = true;
			_hasChanged = true;
		}

		internal void CollectDestroyEntities()
		{
			if (!_hasToDestroy)
				return;

			foreach (var kv in _destroyMap)
			{
				int slot = kv.Value;

				ref var e = ref _entities[slot];
				e.id = -_freeSlotHead;
				e.destroy = false;
				e.changed = false;
				e.name = null;

				_freeSlotHead = slot;
				_entityCount--;
			}

			/*
			int length = _entities.Length;
			for (int slot = 0; slot < length; slot++)
			{
				ref var e = ref _entities[slot];
				if (e.destroy && e.id > 0)
				{
					e.id = -_freeSlotHead;
					e.destroy = false;
					e.changed = false;
					e.name = null;

					_freeSlotHead = slot;
					_entityCount--;
				}
			}
			*/

			_destroyMap.Clear();
			_hasToDestroy = false;
		}

		internal void ForEach(Action<Entity, bool /*Active*/, bool/*Changed*/> process)
		{
			int length = _entities.Length;
			for (int slot = 0; slot < length; slot++)
			{
				ref var e = ref _entities[slot];
				if (e.id > 0)
					process(new Entity(e.id, slot), !e.destroy, e.changed);
			}
		}

		internal void ForEachActive(Action<Entity> process)
		{
			int length = _entities.Length;
			for (int slot = 0; slot < length; slot++)
			{
				ref var e = ref _entities[slot];
				if (e.id > 0 && !e.destroy)
					process(new Entity(e.id, slot));
			}
		}

		internal void ForEachChanged(Action<Entity> process)
		{
			if (!_hasChanged)
				return;

			foreach (var kv in _changedMap)
				process(new Entity(kv.Key, kv.Value));
			
			/*
			int length = _entities.Length;
			for (int slot = 0; slot < length; slot++)
			{
				ref var e = ref _entities[slot];
				if (e.changed && e.id > 0)
					process(new Entity(e.id, slot));
			}
			*/
		}

		internal void ResetChanged()
		{
			if (!_hasChanged)
				return;

			foreach (var kv in _changedMap)
				_entities[kv.Value].changed = false;

			/*
			int length = _entities.Length;
			for (int slot = 0; slot < length; slot++)
			{
				_entities[slot].changed = false;
			}
			*/

			_changedMap.Clear();
			_hasChanged = false;
		}
	}
}

#endif
