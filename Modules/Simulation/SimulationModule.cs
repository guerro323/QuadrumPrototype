using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;

namespace QuadrumPrototype.Modules.Simulation;

public struct SimulationModule : IModule<MainModule>
{
    public static void Setup(World world)
    {
        world.Register<GameTime>();
        world.Register<SimulationPhase>();
        world.Register<TimeProgressSystem>();
    }
}