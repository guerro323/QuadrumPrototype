using System.Runtime.InteropServices;
using QuadrumPrototype.Modules.RhythmEngine;
using revghost.flecs;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;

public unsafe partial struct RhythmCommandDescription : IComponent<RhythmEngineModule>
{
    private const int size = 16 * 8;
    
    private fixed byte buffer[size];

    public int Length;
    public Span<RhythmCommandAction> Buffer => MemoryMarshal.Cast<byte, RhythmCommandAction>(MemoryMarshal.CreateSpan(ref buffer[0], size))[..Length];
    public int Duration;

    public RhythmCommandDescription(ReadOnlySpan<RhythmCommandAction> span, int? duration = null)
    {
        Duration = duration is null or <= 0 ? 4 : duration.Value;
        
        Length = span.Length;
        span.CopyTo(Buffer);
    }
}