using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public static class ContextInfo
	{
		private static ComponentTypeInfo[] _componentInfoList;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ComponentTypeInfo[] GetComponentInfoList()
		{
			if (_componentInfoList == null)
				_componentInfoList = CollectComponents();

			return _componentInfoList;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIndexOf<T>() where T : struct, IComponent
		{
			return ComponentTypeInfo<T>.index;
		}

		private static ComponentTypeInfo[] CollectComponents()
		{
			var baseType = typeof(IComponent);

			var types = AppDomain.CurrentDomain.GetAssemblies()
				.Where(s => !s.FullName.StartsWith("System.") && !s.FullName.StartsWith("SlimECS."))
				.SelectMany(s => s.GetTypes())
				.Where(t => t.IsValueType && !t.IsPrimitive && t.IsPublic && baseType.IsAssignableFrom(t))
				.ToArray();

			Array.Sort(types, (x, y) => string.CompareOrdinal(x.FullName, y.FullName));

			var list = new ComponentTypeInfo[types.Length];
			for (int i = 0; i < types.Length; i++)
			{
				var t = types[i];
				var typeInfo = new ComponentTypeInfo(t, i, IsZeroSizeStruct(t));
				
				list[i] = typeInfo;

				var infoType = typeof(ComponentTypeInfo<>).MakeGenericType(t);

				var fieldIndex = infoType.GetField("index", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				fieldIndex.SetValue(null, i);

				var fieldZeroSize = infoType.GetField("zeroSize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				fieldZeroSize?.SetValue(null, typeInfo.zeroSize);
			}

			return list;
		}

		// https://stackoverflow.com/a/27851610
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsZeroSizeStruct(Type t)
		{
			return t.IsValueType && !t.IsPrimitive 
				&& t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.All(field => IsZeroSizeStruct(field.FieldType));
		}
	}
}
