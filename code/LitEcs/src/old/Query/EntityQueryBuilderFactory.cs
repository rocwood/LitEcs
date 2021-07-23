﻿using System.Runtime.CompilerServices;

namespace SlimECS
{
	public static class EntityQueryBuilderFactory
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAllBuilder<T1> WithAll<T1>(this Context c) where T1:struct,IComponent 
			=> new EntityQueryAllBuilder<T1>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAllBuilder<T1,T2> WithAll<T1,T2>(this Context c) where T1:struct,IComponent where T2:struct,IComponent 
			=> new EntityQueryAllBuilder<T1,T2>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAllBuilder<T1,T2,T3> WithAll<T1,T2,T3>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent 
			=> new EntityQueryAllBuilder<T1,T2,T3>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAllBuilder<T1,T2,T3,T4> WithAll<T1,T2,T3,T4>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent where T4:struct,IComponent 
			=> new EntityQueryAllBuilder<T1,T2,T3,T4>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAllBuilder<T1,T2,T3,T4,T5> WithAll<T1,T2,T3,T4,T5>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent where T4:struct,IComponent where T5:struct,IComponent 
			=> new EntityQueryAllBuilder<T1,T2,T3,T4,T5>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAllBuilder<T1,T2,T3,T4,T5,T6> WithAll<T1,T2,T3,T4,T5,T6>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent where T4:struct,IComponent where T5:struct,IComponent where T6:struct,IComponent 
			=> new EntityQueryAllBuilder<T1,T2,T3,T4,T5,T6>(c);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAnyBuilder<T1> WithAny<T1>(this Context c) where T1:struct,IComponent 
			=> new EntityQueryAnyBuilder<T1>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAnyBuilder<T1,T2> WithAny<T1,T2>(this Context c) where T1:struct,IComponent where T2:struct,IComponent 
			=> new EntityQueryAnyBuilder<T1,T2>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAnyBuilder<T1,T2,T3> WithAny<T1,T2,T3>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent 
			=> new EntityQueryAnyBuilder<T1,T2,T3>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAnyBuilder<T1,T2,T3,T4> WithAny<T1,T2,T3,T4>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent where T4:struct,IComponent 
			=> new EntityQueryAnyBuilder<T1,T2,T3,T4>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAnyBuilder<T1,T2,T3,T4,T5> WithAny<T1,T2,T3,T4,T5>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent where T4:struct,IComponent where T5:struct,IComponent 
			=> new EntityQueryAnyBuilder<T1,T2,T3,T4,T5>(c);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static EntityQueryAnyBuilder<T1,T2,T3,T4,T5,T6> WithAny<T1,T2,T3,T4,T5,T6>(this Context c) where T1:struct,IComponent where T2:struct,IComponent where T3:struct,IComponent where T4:struct,IComponent where T5:struct,IComponent where T6:struct,IComponent 
			=> new EntityQueryAnyBuilder<T1,T2,T3,T4,T5,T6>(c);

	}
}
