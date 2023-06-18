using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.RhythmEngine.Systems;

[Write<RhythmEngineIsPlaying>]
[Write<RhythmEngineIsPaused>]
[Phase(typeof(SimulationPhase))]
public partial struct ApplyTagsSystem : ISystem<RhythmEngineModule>
{
    public RhythmEngineController Controller;
    
    public void Each()
    {
        switch (Controller.State)
        {
            case RhythmEngineController.EState.Playing:
                Entity.Add<RhythmEngineIsPlaying>();
                Entity.Remove<RhythmEngineIsPaused>();
                break;
            case RhythmEngineController.EState.Paused:
                Entity.Remove<RhythmEngineIsPlaying>();
                Entity.Add<RhythmEngineIsPaused>();
                break;
            case RhythmEngineController.EState.Stopped:
                Entity.Remove<RhythmEngineIsPlaying>();
                Entity.Remove<RhythmEngineIsPaused>();
                break;
            default:
                throw new ArgumentOutOfRangeException($"{Entity} ---> {Controller.State}");
        }
    }
}