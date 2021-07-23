using System;

namespace SlimECS
{
	public class InvalidEntityException : Exception
	{
		public InvalidEntityException(Entity e)
			: base($"Invalid entitiy : id={e.id}, slot={e.slot}, context={e.context?.Name}")
		{
		}
	}
}
