using QuadrumPrototype.Modules.RhythmEngine;
using QuadrumPrototype.Modules.Simulation;
using revghost.flecs;

namespace QuadrumPrototype.Modules;

public struct MainModule : IModule
{
    public static void Setup(World world)
    {
        world.Register<SimulationModule>();
        world.Register<RhythmEngineModule>();
    }
}