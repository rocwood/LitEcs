using System;
using System.Runtime.CompilerServices;

namespace SlimECS
{
	public struct Entity : IEquatable<Entity>
	{
		public readonly int id;

		public readonly int slot;
		public readonly Context context;

		public Entity(int id, int slot, Context context)
		{
			this.id = id;
			this.slot = slot;
			this.context = context;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)] 
		public override bool Equals(object obj) => obj is Entity entity && Equals(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Entity other) => context == other.context && id == other.id && slot == other.slot;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] 
		public static bool operator ==(Entity left, Entity right) => left.context == right.context && left.id == right.id && left.slot == right.slot;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] 
		public static bool operator !=(Entity left, Entity right) => left.context != right.context || left.id != right.id || left.slot != right.slot;

		// https://referencesource.microsoft.com/#System.Numerics/System/Numerics/HashCodeHelper.cs
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode() => ((id << 5) + id) ^ slot;
	}
}
