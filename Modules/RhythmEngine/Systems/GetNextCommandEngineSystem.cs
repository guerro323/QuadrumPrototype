using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;
using revghost.Shared.Collections;

namespace QuadrumPrototype.Modules.RhythmEngine.Systems;

[In<RhythmEngineIsPlaying>]
[Phase(typeof(SimulationPhase))]
public partial struct GetNextCommandEngineSystem : ISystem<RhythmEngineModule>
{
    public readonly RhythmEngineState State;
    public readonly RhythmEngineSettings Settings;
    public RhythmCommandProgression Progression;

    public RhythmPredictedCommands Predicted;
    public RhythmExecutingCommand Executing;
    
    private Commands _commands;
    
    public void Each()
    {
        using var commandList = new ValueList<EntityId>(0);
        using var output = new ValueList<EntityId>(0);
        
        if (!State.CanRunCommands)
            return;
        
        foreach (var iterCommand in _commands)
        {
            commandList.Add(iterCommand.Id);
        }

        RhythmCommandUtility.GetCommand(
            ProcessorContext.World,
            commandList.Span, Progression.Buffer, output,
            false, Settings.BeatInterval
        );

        Predicted.Length = output.Count;
        output.Span.CopyTo(Predicted.Buffer);
        
        // No matching commands found, do a check to see if we have any predicted ones
        if (Predicted.Length == 0)
        {
            RhythmCommandUtility.GetCommand(
                ProcessorContext.World,
                commandList.Span, Progression.Buffer, output,
                true, Settings.BeatInterval
            );

            Predicted.Length += output.Count;
            output.Span.CopyTo(Predicted.Buffer);

            // early return, we didn't had a matched command anyway.
            return;
        }

        // this is so laggy clients don't have a weird things when their command has been on another beat on the server
        var targetBeat = Progression.Buffer[^1].FlowBeat + 1;
        
        Executing.Previous = Executing.CommandTarget;
        Executing.CommandTarget = output[0];
        Executing.ActivationBeatStart = targetBeat;

        var beatDuration = Executing.CommandTarget.WithWorld(ProcessorContext.World)
            .Get<RhythmCommandDescription>()
            .Duration;

        Executing.ActivationBeatEnd = targetBeat + beatDuration;
        Executing.WaitingForApply = true;

        var power = 0.0f;
        for (var i = 0; i != Progression.Length; i++)
        {
            // perfect
            if (Progression.Buffer[i].GetAbsoluteScore() <= 0.16f)
                power += 1.0f;
        }

        Executing.Power = power / Progression.Length;
        Progression.Length = 0;
        Console.WriteLine("execute");
    }

    [In<RhythmCommandDescription>]
    public partial struct Commands : IEntityFilter
    {
        
    }
} 