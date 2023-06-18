using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation;
using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.RhythmEngine.Systems;

[In<RhythmEngineIsPlaying>]
[Write<RhythmEngineIsPlaying>]
[Phase(typeof(SimulationPhase))]
public partial struct ProcessSystem : ISystem<RhythmEngineModule>
{
    [Singleton] public GameTime Time;
    
    public RhythmEngineState State;
    public readonly RhythmEngineSettings Settings;
    public readonly RhythmEngineController Controller;
    
    public void Each()
    {
        if (Controller.StartTime != State.PreviousStartTime)
        {
            State.PreviousStartTime = Controller.StartTime;
            State.Elapsed = Time.Total - Controller.StartTime;
        }

        State.Elapsed += Time.Delta;

        if (State.Elapsed < TimeSpan.Zero)
        {
            Entity.Remove<RhythmEngineIsPlaying>();
        }
        
        var nextCurrentBeats = RhythmUtility.GetActivationBeat(State, Settings);
        if (State.CurrentBeat != nextCurrentBeats)
            State.NewBeatTick = (uint) Time.Frame;

        State.CurrentBeat = nextCurrentBeats;
    }
} 