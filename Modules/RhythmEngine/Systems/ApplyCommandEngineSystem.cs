using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.RhythmEngine.Systems;

[In<RhythmEngineIsPlaying>]
[Phase(typeof(SimulationPhase))]
public partial struct ApplyCommandEngineSystem : ISystem<RhythmEngineModule>
{
    public readonly RhythmEngineState State;
    public readonly RhythmEngineSettings Settings;
    public RhythmCommandProgression Progression;

    public RhythmPredictedCommands Predicted;
    public RhythmExecutingCommand Executing;
    public RhythmCommandState CommandState;
    public RhythmComboState ComboState;
    public RhythmEngineRecovery Recovery;

    public void Each()
    {
        if (!State.CanRunCommands)
            return;

        const int mercy = 1; // increase it by 1 on a server
        const int cmdMercy = 0; // increase it by 3 on a server

        var rhythmActiveAtFlowBeat = Executing.ActivationBeatStart;

        var checkStopBeat = Math.Max(State.LastPressure.FlowBeat + mercy,
            RhythmUtility.GetFlowBeat(new TimeSpan(CommandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                Settings.BeatInterval) + cmdMercy);
        if (true) // todo: !isServer && simulateTagFromEntity.Exists(entity)
            checkStopBeat = Math.Max(checkStopBeat,
                RhythmUtility.GetFlowBeat(
                    new TimeSpan(CommandState.EndTimeMs * TimeSpan.TicksPerMillisecond),
                    Settings.BeatInterval));

        var flowBeat = RhythmUtility.GetFlowBeat(State, Settings);
        var activationBeat = RhythmUtility.GetActivationBeat(State, Settings);
        if (Recovery.IsRecovery(flowBeat)
            || rhythmActiveAtFlowBeat < flowBeat && checkStopBeat < activationBeat
            || Executing.CommandTarget.Equals(default) && Predicted.Length != 0 &&
            rhythmActiveAtFlowBeat < State.LastPressure.FlowBeat
            || Predicted.Length == 0)
        {
            CommandState.Reset();
            ComboState = default;
            Executing = default;
        }

        if (Executing.CommandTarget.Equals(default) || Recovery.IsRecovery(flowBeat))
        {
            CommandState.Reset();
            ComboState = default;
            Executing = default;
            return;
        }

        if (!Executing.WaitingForApply)
            return;
        Executing.WaitingForApply = false;

        var beatDuration = Executing.CommandTarget.WithWorld(ProcessorContext.World)
            .Get<RhythmCommandDescription>()
            .Duration;
        
        /*foreach (var element in targetResourceBuffer.Span)
            beatDuration = Math.Max(beatDuration, (int) Math.Ceiling(element.Value.Beat.Target + 1 + element.Value.Beat.Offset + element.Value.Beat.SliderLength));*/

        // if (!isServer && settings.UseClientSimulation && simulateTagFromEntity.Exists(entity))
        if (true)
        {
            Console.WriteLine("APPLY");
            
            CommandState.ChainEndTimeMs = (int) ((rhythmActiveAtFlowBeat + beatDuration + 4) *
                                                        (Settings.BeatInterval.Ticks /
                                                         TimeSpan.TicksPerMillisecond));
            CommandState.StartTimeMs = (int) (Executing.ActivationBeatStart *
                                                     (Settings.BeatInterval.Ticks /
                                                      TimeSpan.TicksPerMillisecond));
            CommandState.EndTimeMs = (int) (Executing.ActivationBeatEnd *
                                                   (Settings.BeatInterval.Ticks /
                                                    TimeSpan.TicksPerMillisecond));

            // var wasFever = ComboSettings.CanEnterFever(ComboState);

            ComboState.Count++;
            ComboState.Score += (float) (Executing.Power - 0.5) * 0.5f;
            if (ComboState.Score < 0)
                ComboState.Score = 0;
            if (ComboState.Score > 1)
                ComboState.Score = 1;

            // We have a little bonus when doing a perfect command
            /*if (executing.IsPerfect
                && wasFever
                && HasComponent(entity, AsComponentType<RhythmSummonEnergy>()))
                GetComponentData<RhythmSummonEnergy>(entity).Value += 20;*/
        }
    }
} 