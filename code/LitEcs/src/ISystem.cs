namespace LitEcs
{
	public interface ISystem 
	{
	}

	public interface IInitSystem : ISystem
	{
		void Init(World world);
	}

	public interface IExecuteSystem : ISystem
	{
		void Execute(World world);
	}

	public interface IDestroySystem : ISystem
	{
		void Destroy(World world);
	}
}
