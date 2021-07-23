using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public partial class Context
	{
		private readonly Dictionary<Type, EntityQuery> _queryMap = new Dictionary<Type, EntityQuery>();
		private readonly List<EntityQuery> _queries = new List<EntityQuery>();

		private readonly object[] _queryParams;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal EntityQuery GetQuery(Type queryType)
		{
			if (!_queryMap.TryGetValue(queryType, out var query))
			{
				if (!typeof(EntityQuery).IsAssignableFrom(queryType))
					return null;

				query = (EntityQuery)Activator.CreateInstance(queryType, BindingFlags.NonPublic|BindingFlags.Instance, null, _queryParams, CultureInfo.InvariantCulture);
				if (query == null)
					return null;

				//_entities.ForEachActive(e => query.HandleEntity(e));

				var items = _entities.items;

				for (int slot = 0; slot < items.Length; slot++)
				{
					ref var d = ref items[slot];
					if (d.id > 0 && !d.destroy)
					{
						if (IsMatch(ref d, query._indices, query._matchAny))
							query._entities.Add(new Entity(d.id, slot, this));
					}
				}

				_queryMap.Add(queryType, query);
				_queries.Add(query);
			}

			return query;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void GetQuery<T>(out T query) where T : EntityQuery
		{
			query = (T)GetQuery(typeof(T));
		}

		private void HandleGroupChanges()
		{
			if (!_hasChanged)
				return;

			foreach (var kv in _changedMap)
			{
				ref var d = ref _entities.items[kv.Value];
				var e = new Entity(d.id, kv.Value, this);

				for (int i = 0; i < _queries.Count; i++)
				{
					var query = _queries[i];

					if (IsMatch(ref d, query._indices, query._matchAny))
						query._entities.Add(e);
					else
						query._entities.Remove(e);
				}
			}

			_changedMap.Clear();
			_hasChanged = false;
		}

		/*
		private Action<Entity> _handleGroupChangedFunc;

		private void HandleGroupChangesImpl(Entity e)
		{
			for (int i = 0; i < _queries.Count; i++)
				_queries[i].HandleEntity(e);
		}
		*/

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsMatch(ref EntityData d, int[] indices, bool matchAny)
		{
			if (d.destroy || indices == null)
				return false;

			return matchAny ? IsMatchAny(ref d, indices) : IsMatchAll(ref d, indices);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsMatchAll(ref EntityData d, int[] indices)
		{
			for (int i = 0; i < indices.Length; i++)
			{
				var index = indices[i];
				if (index >= 0) // include
				{
					if (d.components[index] < 0)
						return false;
				}
				else  // exclude
				{
					if (d.components[-index - 1] >= 0)
						return false;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsMatchAny(ref EntityData d, int[] indices)
		{
			bool hasAny = false;

			for (int i = 0; i < indices.Length; i++)
			{
				var index = indices[i];
				if (index >= 0) // include
				{
					if (!hasAny)
					{
						if (d.components[index] >= 0)
							hasAny = true;
					}
				}
				else // exclude
				{
					if (d.components[-index - 1] >= 0)
						return false;
				}
			}

			return hasAny;
		}
	}
}
