using System;

namespace SlimECS
{
	public class SystemException : Exception
	{
		public SystemException(string message) : base(message) { }
		public SystemException(string message, Exception innerException) : base(message, innerException) { }
	}
}
