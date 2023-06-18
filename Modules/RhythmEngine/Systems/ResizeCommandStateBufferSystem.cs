using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.RhythmEngine.Systems;

[Phase(typeof(SimulationPhase))]
public partial struct ResizeCommandStateBufferSystem : ISystem<RhythmEngineModule>
{
    public RhythmCommandProgression Progression;
    public readonly RhythmEngineState State;
    public readonly RhythmEngineSettings Settings;
    public readonly RhythmEngineRecovery Recovery;
    
    public void Each()
    {
        var flowBeat = RhythmUtility.GetFlowBeat(State, Settings);
        var mercy = 0; // When on authoritative server, increase it by one
        
        for (var i = 0; i < Progression.Length; i++)
        {
            var pressure = Progression.Buffer[i];
            if (flowBeat >= pressure.FlowBeat + mercy + Settings.MaxBeats
                || Recovery.IsRecovery(flowBeat))
            {
                Progression.RemoveAt(i--);
            }
        }
    }
} 