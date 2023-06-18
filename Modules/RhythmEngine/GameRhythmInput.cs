using System.Runtime.InteropServices;
using PataNext.Game.Client.Core.Inputs;
using revghost.flecs;

namespace QuadrumPrototype.Modules.RhythmEngine;

public partial struct GameRhythmInput : IComponent<RhythmEngineModule>
{
    public struct RhythmAction
    {
        public InterFramePressAction InterFrame;
        // Is the button currently being held?
        public TimeSpan ActiveTime;
        // For sliders
        public bool IsSliding;
    }

    // left
    private RhythmAction action0;
    // right
    private RhythmAction action1;
    // down
    private RhythmAction action2;
    // up
    private RhythmAction action3;

    /// <summary>
    /// Get the 4 <see cref="RhythmAction"/>
    /// </summary>
    /// <remarks>
    /// [0] is Left
    /// [1] is Right
    /// [2] is Down
    /// [3] is Up
    /// </remarks>
    public Span<RhythmAction> Actions => MemoryMarshal.CreateSpan(ref action0, 4);
}