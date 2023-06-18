using Godot;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Modules.RhythmEngine;
using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Client;

public partial struct UpdateGameRhythmInputSystem : ISystem
{
    [Singleton] private GameTime _gameTime;

    public GameRhythmInput RhythmInput;
    
    public void Each()
    {
        for (var i = 0; i < RhythmInput.Actions.Length; i++)
        {
            var name = (DefaultCommandKeys) (i + 1) switch
            {
                DefaultCommandKeys.Up => "r_up",
                DefaultCommandKeys.Down => "r_down",
                DefaultCommandKeys.Left => "r_left",
                DefaultCommandKeys.Right => "r_right"
            };
            
            var input = (active: Input.IsActionPressed(name), down: Input.IsActionJustPressed(name), up: Input.IsActionJustReleased(name));

            ref var action = ref RhythmInput.Actions[i];

            if (input.active)
                action.ActiveTime += _gameTime.Delta;
            else
                action.ActiveTime = default;
            
            action.IsSliding = (action.IsSliding && input.up) || (input.active && action.ActiveTime > TimeSpan.FromMilliseconds(300));

            if (input.down)
                action.InterFrame.Pressed = _gameTime.Frame;

            if (input.up)
                action.InterFrame.Released = _gameTime.Frame;
        }
    }
}