using Godot;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype;
using QuadrumPrototype.Client;
using QuadrumPrototype.Client.Proxies;
using QuadrumPrototype.Modules;
using QuadrumPrototype.Modules.Proxy;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;
using revghost.v2;
using revghost.v2.Utility;

public partial struct MyPhase : IStaticEntity, IStaticEntitySetup
{
	public static void Setup(World world)
	{
		world.Scope.Value!.Add(flecs_hub.flecs.EcsPhase);
		world.Scope.Value!.DependsOn(world.GetStatic(flecs_hub.flecs.EcsOnUpdate));
	}
}

public partial class BonjourScript : Node
{
	private static ConcurrentQueue<(HostLogLevel level, string msg)> _log = new();
	public static GhostRunner Runner;

	static BonjourScript()
	{
		if (Engine.IsEditorHint())
			return;
		
		HostLogger.Output = (level, line, source, theme) =>
		{
			if (string.IsNullOrEmpty(source))
				source = "global";
			if (string.IsNullOrEmpty(theme))
				theme = ":";

			_log.Enqueue((level, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{level}|{source}|{theme}: {line}"));
		};

		Runner = GhostBuilder.Create()
			.WithoutDefaultModule()
			.WithModule<MainModule>()
			.WithTargetFps(240)
			.Startup(world =>
			{
				var sys = world.Register<MultipleCallSystem>();
				sys.Add(world.Get<MyPhase>());

				world.Register<ClientModule>();

				world.New("Rhythm Engine")
					.Set(new RhythmEngineController
					{
						State = RhythmEngineController.EState.Playing,
						StartTime = TimeSpan.FromSeconds(1)
					})
					.Set(new RhythmEngineSettings
					{
						BeatInterval = TimeSpan.FromMilliseconds(500),
						MaxBeats = 4
					})
					.Set(new RhythmEngineState())
					.Set(new RhythmEngineRecovery())
					.Set(new RhythmComboState())
					.Set(new RhythmCommandState())
					.Set(new RhythmExecutingCommand())
					.Set(new RhythmCommandProgression())
					.Set(new RhythmPredictedCommands())
					.Set(new PowerGaugeState
					{
						Level = 0,
						MaxLevel = 4,
						Tick = 0,
						MaxTick = 100
					});

				world.Get<GameTime>().Set(new GameTime()
				{
					Delta = TimeSpan.FromSeconds(0.01f)
				});

				world.New("MarchCommand")
					.Set(new RhythmCommandDescription(stackalloc[]
					{
						new RhythmCommandAction(0, (int) DefaultCommandKeys.Left),
						new RhythmCommandAction(1, (int) DefaultCommandKeys.Left),
						new RhythmCommandAction(2, (int) DefaultCommandKeys.Left),
						new RhythmCommandAction(3, (int) DefaultCommandKeys.Right),
					}));
				
				world.New("AttackCommand")
					.Set(new RhythmCommandDescription(stackalloc[]
					{
						new RhythmCommandAction(0, (int) DefaultCommandKeys.Right),
						new RhythmCommandAction(1, (int) DefaultCommandKeys.Right),
						new RhythmCommandAction(2, (int) DefaultCommandKeys.Left),
						new RhythmCommandAction(3, (int) DefaultCommandKeys.Right),
					}));
				
				world.New("SpecialCommand")
					.Set(new RhythmCommandDescription(stackalloc[]
					{
						new RhythmCommandAction(0, (int) DefaultCommandKeys.Right),
						new RhythmCommandAction(1, (int) DefaultCommandKeys.Down),
						new RhythmCommandAction(2, (int) DefaultCommandKeys.Down),
						new RhythmCommandAction(3, (int) DefaultCommandKeys.Right),
					}));
				
				world.New("ShortAttackCommand")
					.Set(new RhythmCommandDescription(stackalloc[]
					{
						RhythmCommandAction.WithSlider(0, 1, (int) DefaultCommandKeys.Right),
					}, 2));
				
				world.New("QuickDodgeCommand")
					.Set(new RhythmCommandDescription(stackalloc[]
					{
						RhythmCommandAction.WithSlider(0, 2, (int) DefaultCommandKeys.Right),
						RhythmCommandAction.With(1, (int) DefaultCommandKeys.Left),
					}, 3));
			})
			.Build();
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		for (var step = 0; step < 1; step++)
		{
			Runner.Progress();
			Runner.World.Get<MyPhase>().Run(0);
		}

		while (_log.TryDequeue(out var tuple))
		{
			switch (tuple.level)
			{
				case HostLogLevel.Info:
					GD.Print(tuple.msg);
					break;
				case HostLogLevel.Warn:
					GD.PushWarning(tuple.msg);
					break;
				case HostLogLevel.Error:
					GD.PushError(tuple.msg);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		// It's actually fine, 99.9% of the time in a frame we don't allocate GC.
		// But any garbage memory can make our application freeze for an instant so it's kinda bad.
		//
		// In the future there will be more managed memory which can make that line takes more time to execute.
		// So we will need to optimize these parts as much as possible! (and then we can remove that line)
		var sw = new Stopwatch();
		sw.Start();
		GC.Collect();
		sw.Stop();

		Console.WriteLine($"{sw.Elapsed.TotalMilliseconds:F4}ms");
	}
}
