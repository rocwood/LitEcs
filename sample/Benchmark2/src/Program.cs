using System;
using System.Diagnostics;
using SlimECS;

namespace ECS.Benchmark
{
	public struct Position : IComponent //, IModifiable
	{
		public float x;
		public float y;
	}

	public struct Velocity : IComponent
	{
		public float x;
		public float y;
	}
	
	public class MovementSystem : SystemBase
	{
		private const float axisBound = 100;

		private EntityQueryAll<Position, Velocity> query;

		public override void Init()
		{
			context.GetQuery(out query);
		}

		public override void Execute()
		{
			var positionPool = context.GetComponentDataPool<Position>();
			var velocityPool = context.GetComponentDataPool<Velocity>();

			foreach (var e in query)
			{
				//ref var v = ref e.Ref<Velocity>();
				//ref var pos = ref e.Ref<Position>();

				ref var v = ref e.Ref<Velocity>(velocityPool);
				ref var pos = ref e.Ref<Position>(positionPool);

				pos.x += v.x;
				pos.y += v.y;

				// check bound, and reflect velocity
				if ((v.x < 0 && pos.x < -axisBound) ||
					(v.x > 0 && pos.x > axisBound))
					v.x = -v.x;
				if ((v.y < 0 && pos.y < -axisBound) ||
					(v.y > 0 && pos.y > axisBound))
					v.y = -v.y;
			}
		}
	}

	public class BenchmarkCase
	{
		private const float axisBound = 100;
		private const float maxAxisSpeed = 20;

		private const int initialEntityCount = 1000;
		private const int iterateCount = 10000;

		private Context context;
		private SystemManager systems;

		public void Init()
		{
			context = new Context("Default");

			systems = new SystemManager(context);
			systems.CollectAll();
			systems.Init();

			var random = new Random(1);

			for (int i = 0; i < initialEntityCount; i++)
			{
				var child = context.CreateEntity();

				float x = ((float)random.NextDouble() - 0.5f) * axisBound;
				float y = ((float)random.NextDouble() - 0.5f) * axisBound;
				child.Set(new Position { x = x, y = y });

				float vx = ((float)random.NextDouble() - 0.5f) * maxAxisSpeed;
				float vy = ((float)random.NextDouble() - 0.5f) * maxAxisSpeed;
				child.Set(new Velocity { x = vx, y = vy });

				if (i == 0)
					e0 = child;
			}

			context.WithAll<Position, Velocity>().Get();
			context.Poll();
		}

		public void Execute()
		{
			for (int i = 0; i < iterateCount; i++)
			{
				systems.Execute();
			}
		}

		private Entity e0;

		public void Cleanup()
		{
			ref var pos = ref e0.Ref<Position>();
			Console.WriteLine($"e0({pos.x},{pos.y})");

			systems = null;
			context = null;
		}
	}

	public class Program
	{
		static void Main(string[] args)
		{
			var sw = new Stopwatch();
			var benchmark = new BenchmarkCase();

			var mem0 = GC.GetTotalMemory(false);

			sw.Start();
			benchmark.Init();
			sw.Stop();
			var initTime = sw.ElapsedMilliseconds;

			var mem1 = GC.GetTotalMemory(false);

			sw.Restart();
			benchmark.Execute();
			sw.Stop();
			var execTime = sw.ElapsedMilliseconds;

			sw.Restart();
			benchmark.Cleanup();
			sw.Stop();
			var cleanupTime = sw.ElapsedMilliseconds;

			var mem2 = GC.GetTotalMemory(false);

			Console.WriteLine($"Init = {initTime}ms, {(mem1 - mem0) / 1024}KB\nExec = {execTime}ms, {(mem2 - mem1) / 1024}KB\nClean = {cleanupTime}");
		}
	}
}
