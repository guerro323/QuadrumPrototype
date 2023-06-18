using QuadrumPrototype.Modules.RhythmEngine;
using revghost.flecs;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Components;

public partial struct RhythmEngineSettings : IComponent<RhythmEngineModule>
{
    public TimeSpan BeatInterval;
    public int MaxBeats;
}