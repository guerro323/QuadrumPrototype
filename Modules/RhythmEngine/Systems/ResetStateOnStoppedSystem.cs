using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.RhythmEngine.Systems;

[None<RhythmEngineIsPlaying>]
[None<RhythmEngineIsPaused>]
[Phase(typeof(SimulationPhase))]
public partial struct ResetStateOnStoppedSystem : ISystem<RhythmEngineModule>
{
    public RhythmComboState Combo;
    public RhythmEngineRecovery Recovery;
    
    public void Each()
    {
        Combo = default;
        Recovery = default;
    }
} 