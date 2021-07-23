using System;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	static class ArrayHelper
	{
		private const int DefaultCapacity = 16;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void EnsureLength<T>(ref T[] array, int minSize)
		{
			int size = array.Length;
			if (size >= minSize)
				return;

			if (size <= 0)
				size = DefaultCapacity;

			while (size < minSize)
				size *= 2;

			if (size > array.Length)
				Array.Resize(ref array, size);
		}
	}
}
