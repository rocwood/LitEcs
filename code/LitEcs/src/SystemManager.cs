using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace LitEcs
{
	[Il2CppSetOption (Option.NullChecks, false)]
	[Il2CppSetOption (Option.ArrayBoundsChecks, false)]
	public sealed class SystemManager
	{
		readonly World _world;
		
		readonly List<ISystem> _allSystems;
		readonly List<IExecuteSystem> _executeSystems;

		public SystemManager(World world)
		{
			_world = world;
			
			_allSystems = new List<ISystem>(128);
			_executeSystems = new List<IExecuteSystem>(128);
		}

		/*
		public int GetAllSystems(ref ISystem[] list)
		{
			var itemsCount = _allSystems.Count;
			if (itemsCount == 0) { return 0; }
			if (list == null || list.Length < itemsCount)
			{
				list = new ISystem[_allSystems.Capacity];
			}
			for (int i = 0, iMax = itemsCount; i < iMax; i++)
			{
				list[i] = _allSystems[i];
			}
			return itemsCount;
		}

		public int GetRunSystems(ref IExecuteSystem[] list)
		{
			var itemsCount = _runSystemsCount;
			if (itemsCount == 0) { return 0; }
			if (list == null || list.Length < itemsCount)
			{
				list = new IExecuteSystem[_runSystems.Length];
			}
			for (int i = 0, iMax = itemsCount; i < iMax; i++)
			{
				list[i] = _runSystems[i];
			}
			return itemsCount;
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public World GetWorld()
		{
			return _world;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SystemManager Add(ISystem system)
		{
			_allSystems.Add(system);

			if (system is IExecuteSystem executeSystem)
				_executeSystems.Add(executeSystem);

			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Init()
		{
			for (var i = 0; i < _allSystems.Count; ++i)
			{
				if (_allSystems[i] is IInitSystem initSystem)
					initSystem.Init(_world);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Execute()
		{
			for (int i = 0; i < _executeSystems.Count; ++i)
				_executeSystems[i].Execute(_world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Destroy()
		{
			for (var i = _allSystems.Count - 1; i >= 0; i--)
			{
				if (_allSystems[i] is IDestroySystem destroySystem)
					destroySystem.Destroy(_world);
			}

			_allSystems.Clear();
			_executeSystems.Clear();
		}
	}
}
