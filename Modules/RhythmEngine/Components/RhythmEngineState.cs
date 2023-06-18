using Quadrum.Game.Modules.Simulation.RhythmEngine;
using revghost.flecs;

namespace QuadrumPrototype.Modules.RhythmEngine.Components;

public partial struct RhythmEngineState : IComponent<RhythmEngineModule>
{
    public FlowPressure LastPressure;
    
    public TimeSpan Elapsed;
    public TimeSpan PreviousStartTime;

    public int CurrentBeat;
    public uint NewBeatTick;

    public bool CanRunCommands => Elapsed > TimeSpan.Zero;
}