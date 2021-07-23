using System;

namespace Unity.IL2CPP.CompilerServices
{
	enum Option
	{
		NullChecks = 1,
		ArrayBoundsChecks = 2
	}

	/// Unity IL2CPP performance optimization attribute.
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	class Il2CppSetOptionAttribute : Attribute
	{
		public Option Option { get; private set; }
		public object Value { get; private set; }

		public Il2CppSetOptionAttribute(Option option, object value) { Option = option; Value = value; }
	}
}
