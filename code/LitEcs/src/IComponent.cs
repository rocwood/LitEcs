namespace LitEcs
{
	/// <summary>
	/// Base interface for all data components
	/// </summary>
	public interface IComponent
	{
	}

	/// <summary>
	/// Component with AutoReset should declare explicit implementation.
	///		public static void AutoReset(ref T component);
	/// </summary>
	public interface IAutoReset
	{
	}
}
