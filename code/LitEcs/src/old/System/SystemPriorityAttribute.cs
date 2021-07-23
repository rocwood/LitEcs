using System;

namespace SlimECS
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SystemPriorityAttribute : Attribute
	{
		public readonly int priority;

		public SystemPriorityAttribute(int priority)
		{
			this.priority = priority;
		}
	}
}
