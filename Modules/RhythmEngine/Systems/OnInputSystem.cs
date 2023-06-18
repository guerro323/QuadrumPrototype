using Godot;
using Quadrum.Game.Modules.Simulation.RhythmEngine;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation;
using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.RhythmEngine.Systems;

[Phase(typeof(SimulationPhase))]
public partial struct OnInputSystem : ISystem<RhythmEngineModule>
{
    [Singleton] private readonly GameTime _time;

    public RhythmEngineState State;
    public readonly RhythmEngineSettings Settings;

    public RhythmCommandProgression Progress;
    public readonly RhythmPredictedCommands Predicted;

    public readonly RhythmComboState ComboState;

    public RhythmCommandState CommandState;
    public RhythmEngineRecovery Recovery;

    private InputFilter _inputFilter;

    public void Each()
    {
        var flowBeat = RhythmUtility.GetFlowBeat(State, Settings);
        // Don't accept inputs when the rhythm engine hasn't yet started
        if (flowBeat < 0)
            return;

        GameRhythmInput input = default;
        foreach (var ent in _inputFilter)
            input = ent.RhythmInput;

        for (var i = 0; i < input.Actions.Length; i++)
        {
            ref readonly var action = ref input.Actions[i];
            // Console.WriteLine($"{action.InterFrame.Pressed} {_time.Frame}");
            if (!action.InterFrame.AnyUpdate(_time.FrameRange))
                continue;

            // If this is not the end of a slider or if it is but our command buffer is empty, skip it!
            if (action.InterFrame.IsReleased(_time.FrameRange) && (!action.IsSliding || Progress.Length == 0))
                continue;

            var cmdChainEndFlow = RhythmUtility.GetFlowBeat(
                CommandState.ChainEndTime,
                Settings.BeatInterval
            );
            var cmdEndFlow = RhythmUtility.GetFlowBeat(
                CommandState.EndTime,
                Settings.BeatInterval
            );

            // check for one beat space between inputs (should we just check for predicted commands? 'maybe' we would have a command with one beat space)
            var failFlag1 = Progress.Length > 0
                            && Predicted.Length == 0
                            && flowBeat > Progress.Buffer[^1].FlowBeat + 1
                            && cmdChainEndFlow > 0;
            // check if this is the first input and was started after the command input time
            var failFlag3 = flowBeat > cmdEndFlow
                            && Progress.Length == 0
                            && cmdEndFlow > 0;
            // check for inputs that were done after the current command chain
            var failFlag2 = flowBeat >= cmdChainEndFlow
                            && cmdChainEndFlow > 0;
            failFlag2 = false; // this flag is deactivated for delayed reborn ability
            var failFlag0 = cmdEndFlow > flowBeat;

            if (failFlag0 || failFlag1 || failFlag2 || failFlag3)
            {
                Recovery.RecoveryActivationBeat = flowBeat + 1;
                CommandState = default;
                continue;
            }

            var pressure = new FlowPressure(i + 1, State.Elapsed, Settings.BeatInterval)
            {
                IsSliderEnd = action.IsSliding
            };

            // TODO: add an event (eg: for increasing summon energy)
            if (ComboState.Count > 0) // No spamming to get score
            {
                Console.WriteLine(((int) (ComboState.Score * 4)) * 0.25f);
                var multiplier = 1.0f;
                multiplier = Mathf.Lerp(multiplier, 2f, ((int) (ComboState.Score * 4)) * 0.25f);

                if (ComboState.Score >= 1.0f)
                    multiplier += 0.5f;

                Entity.Get<PowerGaugeState>().Increase((int) ((1f - Math.Abs(pressure.Score)) * multiplier * 5));
            }

            Progress.Add(pressure);
            State.LastPressure = pressure;
        }
    }

    private partial struct InputFilter : IEntityFilter
    {
        public GameRhythmInput RhythmInput;
    }
}