using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace LitEcs
{
	class ComponentInfo
	{
		public readonly Type type;
		public readonly int index;
		public readonly bool zeroSize;

		internal ComponentInfo(Type type, int index, bool zeroSize)
		{
			this.type = type;
			this.index = index;
			this.zeroSize = zeroSize;
		}


		private static ComponentInfo[] _componentInfoList;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ComponentInfo[] GetComponentList()
		{
			if (_componentInfoList == null)
				_componentInfoList = CollectComponents();

			return _componentInfoList;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetComponentCount()
		{
			return GetComponentList().Length;
		}

		private static ComponentInfo[] CollectComponents()
		{
			var baseType = typeof(IComponent);

			var types = AppDomain.CurrentDomain.GetAssemblies()
				.Where(s => !s.FullName.StartsWith("System.") && !s.FullName.StartsWith("LitEcs."))
				.SelectMany(s => s.GetTypes())
				.Where(t => t.IsValueType && !t.IsPrimitive && t.IsPublic && baseType.IsAssignableFrom(t))
				.ToArray();

			Array.Sort(types, (x, y) => string.CompareOrdinal(x.FullName, y.FullName));

			var list = new ComponentInfo[types.Length];
			for (int i = 0; i < types.Length; i++)
			{
				var t = types[i];
				var typeInfo = new ComponentInfo(t, i, IsZeroSizeStruct(t));

				list[i] = typeInfo;

				var infoType = typeof(ComponentInfo<>).MakeGenericType(t);

				var fieldIndex = infoType.GetField("index", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				fieldIndex.SetValue(null, i);

				var fieldZeroSize = infoType.GetField("zeroSize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				fieldZeroSize?.SetValue(null, typeInfo.zeroSize);
			}

			return list;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsZeroSizeStruct(Type t)
		{
			// https://stackoverflow.com/a/27851610
			return t.IsValueType && !t.IsPrimitive
				&& t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.All(field => IsZeroSizeStruct(field.FieldType));
		}
	}

	class ComponentInfo<T> where T : struct, IComponent
	{
		public static int index = -1;
		public static bool zeroSize = false;
	}
}
