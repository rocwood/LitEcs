using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace LitEcs
{
	public interface IComponentPool
	{
		void Resize(int capacity);
		bool Has(int entity);
		void Remove(int entity);
		object GetRaw(int entity);

		int GetId();
		Type GetComponentType();
	}

#if ENABLE_IL2CPP
	[Il2CppSetOption (Option.NullChecks, false)]
	[Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
	public sealed class ComponentPool<T> : IComponentPool where T : struct, IComponent
	{
		readonly World _world;

		readonly Type _type;
		readonly int _id;

		// 1-based index.
		T[] _denseItems;
		int[] _sparseItems;
		int _denseItemsCount;
		int[] _recycledItems;
		int _recycledItemsCount;

		delegate void AutoResetHandler(ref T component);
		readonly AutoResetHandler _autoReset;
		readonly T _autoResetFakeInstance;

		internal ComponentPool(World world, int id, int denseCapacity, int sparseCapacity)
		{
			_type = typeof(T);
			_world = world;
			_id = id;
			_denseItems = new T[denseCapacity + 1];
			_sparseItems = new int[sparseCapacity];
			_denseItemsCount = 1;
			_recycledItems = new int[512];
			_recycledItemsCount = 0;

			var isAutoReset = typeof(IAutoReset).IsAssignableFrom(_type);
			var autoResetMethod = typeof(T).GetMethod("AutoReset", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
			var autoResetHandler = (autoResetMethod != null)
				? (AutoResetHandler)Delegate.CreateDelegate(typeof(AutoResetHandler), _autoResetFakeInstance, autoResetMethod)
				: null;

			if ((isAutoReset && autoResetHandler == null) || (!isAutoReset && autoResetHandler != null))
				throw new Exception($"{typeof(T).Name} implement IAutoReset should declare 'public static AutoReset(ref {typeof(T).Name})' explicit implementation.");

			_autoReset = autoResetHandler;
		}

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

		void IComponentPool.Resize(int capacity)
		{
			Array.Resize(ref _sparseItems, capacity);
		}

		object IComponentPool.GetRaw(int entity)
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

		public ref T Add(int entity)
		{
#if DEBUG
			if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif

			int idx = _sparseItems[entity];
			if (idx == 0)
			{
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
					_autoReset?.Invoke(ref _denseItems[idx]);
				}

				_sparseItems[entity] = idx;

				_world.OnEntityChange(entity, _id, true);
				_world.Entities[entity].ComponentsCount++;

#if DEBUG || LEOECSLITE_WORLD_EVENTS
				_world.RaiseEntityChangeEvent (entity);
#endif
			}

			return ref _denseItems[idx];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get(int entity)
		{
#if DEBUG
			if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
			if (_sparseItems[entity] == 0) { throw new Exception ("Not attached."); }
#endif
			return ref _denseItems[_sparseItems[entity]];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has(int entity)
		{
#if DEBUG
			if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
			return _sparseItems[entity] > 0;
		}

		public void Remove(int entity)
		{
#if DEBUG
			if (!_world.IsEntityAliveInternal (entity)) { throw new Exception ("Cant touch destroyed entity."); }
#endif
			ref var sparseData = ref _sparseItems[entity];
			if (sparseData == 0)
				return;

			_world.OnEntityChange(entity, _id, false);

			if (_recycledItemsCount == _recycledItems.Length)
				Array.Resize(ref _recycledItems, _recycledItemsCount << 1);

			_recycledItems[_recycledItemsCount++] = sparseData;

			if (_autoReset != null)
				_autoReset.Invoke(ref _denseItems[sparseData]);
			else
				_denseItems[sparseData] = default;

			sparseData = 0;

			ref var entityData = ref _world.Entities[entity];
			entityData.ComponentsCount--;

#if DEBUG || LEOECSLITE_WORLD_EVENTS
			_world.RaiseEntityChangeEvent (entity);
#endif
			//if (entityData.ComponentsCount == 0)
			//	_world.DelEntity(entity);
		}

	}
}
