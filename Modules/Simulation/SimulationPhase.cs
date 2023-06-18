using flecs_hub;
using revghost.flecs;

namespace QuadrumPrototype.Modules.Simulation;

public struct SimulationPhase : IStaticEntity, IStaticEntitySetup
{
    public static void Setup(World world)
    {
        var scope = world.Scope.Value!;

        scope.Add(flecs.EcsPhase);
        unsafe
        {
            // flecs.ecs_set_interval(world.Handle, scope.Id, 0.01f);
        }
    }
}