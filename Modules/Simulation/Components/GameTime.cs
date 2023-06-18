using PataNext.Game.Client.Core.Inputs;
using revghost.flecs;

namespace QuadrumPrototype.Modules.Simulation.Components;

public partial struct GameTime : IComponent
{
    public int Frame;
    public TimeSpan Total;
    public TimeSpan Delta;
    
    /// <summary>
    /// Provide a one length based range with the current frame
    /// </summary>
    public Range FrameRange => new Range(Frame, Frame);
}