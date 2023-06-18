using Godot;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Modules.Proxy;
using QuadrumPrototype.Modules.RhythmEngine;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Client.Proxies;

public partial struct RhythmEngineInterfaceProxy : IProxyFilter
{
    public RhythmEngineSettings Settings;
    public RhythmEngineState State;
    
    public partial struct OnProxyAdded : IEntityObserver<OnSet>
    {
        public GodotProxy<RhythmEngineInterfaceProxy> Proxy;
        
        public void Each()
        {
            ((SceneTree) Engine.GetMainLoop()).CurrentScene.AddChild(Proxy.Node);
        }
    }

    public partial struct OnProxyUpdate : ISystem
    {
        [Singleton] private GameTime _gameTime;
        
        public readonly RhythmEngineState State;
        public readonly RhythmEngineSettings Settings;
        public readonly RhythmExecutingCommand ExecutingCommand;

        public readonly PowerGaugeState PowerState;
        public readonly RhythmComboState ComboState;

        public readonly GodotProxy<RhythmEngineInterfaceProxy> Proxy;

        private InputFilter _filter;
        
        public void Each()
        {
            var powerLabel = Proxy.Node.GetNode("%PowerLabel");
            /*powerLabel.Set("power_level", State.CurrentBeat);
            powerLabel.Set("power_progress",
                TimeSpan.FromTicks(State.Elapsed.Ticks % Settings.BeatInterval.Ticks).TotalSeconds * 2);*/
            {
                var prevLevel = powerLabel.Get("power_level").AsInt32();
                var prevProgress = powerLabel.Get("power_progress").AsDouble();

                var target = ((float) PowerState.Tick) / PowerState.MaxTick;
                
                var lerped = Mathf.Lerp(prevProgress, target, ProcessorContext.DeltaSystemTime);
                lerped = Mathf.MoveToward(lerped, target, ProcessorContext.DeltaSystemTime * 0.5f);

                if (prevLevel != PowerState.Level)
                    lerped = target;
                
                powerLabel.Set("power_level", PowerState.Level);
                powerLabel.Set("power_progress", lerped);
                powerLabel.Set("power_max_level", PowerState.MaxLevel);
            }

            var comboLabel = (Label) Proxy.Node.GetNode("%ComboLabel");
            comboLabel.Text = ComboState.Count.ToString();

            var feverGauge = Proxy.Node.GetNode("%FeverGauge");
            {
                // Lerp
                var prevProgress = feverGauge.Get("progress").AsDouble();
                var lerped = Mathf.Lerp(prevProgress, ComboState.Score, ProcessorContext.DeltaSystemTime * 2f);
                lerped = Mathf.MoveToward(lerped, ComboState.Score, ProcessorContext.DeltaSystemTime);
                feverGauge.Set("progress", lerped);
            }

            var eoc = ExecutingCommand.ActivationBeatEnd - State.CurrentBeat;
            Proxy.Node.CallDeferred("play_metronome", State.CurrentBeat, eoc);

            foreach (var ent in _filter)
            {
                var input = ent.Input;
                for (var i = 0; i < input.Actions.Length; i++)
                {
                    var action = input.Actions[i];
                    if (!action.InterFrame.IsPressed((_gameTime with {Frame = _gameTime.Frame - 1}).FrameRange))
                        continue;
                    
                    var score = Math.Abs(RhythmUtility.GetScore(State, Settings));

                    Proxy.Node.CallDeferred("play_drum", i, score);
                }
            }
        }

        private partial struct InputFilter : IEntityFilter
        {
            public GameRhythmInput Input;
        }
    }
}