using QuadrumPrototype.Modules.RhythmEngine;
using revghost.flecs;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineController : IComponent<RhythmEngineModule>
{
    public enum EState
    {
        Stopped = 0,
        Paused = 1,
        Playing = 2
    }

    public EState State;
    public TimeSpan StartTime;
}