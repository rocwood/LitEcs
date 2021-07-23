namespace SlimECS
{
	public abstract class SystemBase
	{
		public Context context { get; internal set; }

		public virtual void Init() {}
		public abstract void Execute();
	}
}
