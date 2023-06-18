using QuadrumPrototype.Modules.RhythmEngine;
using revghost.flecs;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineRecovery : IComponent<RhythmEngineModule>
{
    public int RecoveryActivationBeat;

    public readonly bool IsRecovery(int activationBeat)
    {
        return RecoveryActivationBeat > activationBeat;
    }
}