using System.Runtime.CompilerServices;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.RhythmEngine.Systems;
using QuadrumPrototype.Modules.Simulation;
using revghost.flecs;

namespace QuadrumPrototype.Modules.RhythmEngine;

public partial struct RhythmEngineModule : IModule<SimulationModule>
{
    public static void Setup(World world)
    {
        var desc = new RhythmCommandDescription();
        Console.WriteLine($"length = {desc.Buffer.Length} ({Unsafe.SizeOf<RhythmCommandAction>()})");

        world.Register<GameRhythmInput>();
        
        world.Register<RhythmEngineIsPlaying>();
        world.Register<RhythmEngineIsPaused>();

        world.Register<RhythmCommandDescription>();

        world.Register<ApplyTagsSystem>();
        world.Register<ProcessSystem>();
        world.Register<ResetStateOnStoppedSystem>();
        world.Register<OnInputSystem>();
        world.Register<GetNextCommandEngineSystem>();
        world.Register<ResizeCommandStateBufferSystem>();
        world.Register<ApplyCommandEngineSystem>();
    }
}