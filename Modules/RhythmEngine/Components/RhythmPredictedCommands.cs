using System.Runtime.InteropServices;
using Quadrum.Game.Modules.Simulation.RhythmEngine;
using revghost.flecs;

namespace QuadrumPrototype.Modules.RhythmEngine.Components;

public unsafe partial struct RhythmPredictedCommands : IComponent<RhythmEngineModule>
{
    private const int size = sizeof(int) * 16;
    
    private fixed byte buffer[size];

    public int Length;
    public Span<EntityId> Buffer => MemoryMarshal.Cast<byte, EntityId>(MemoryMarshal.CreateSpan(ref buffer[0], size))[..Length];
}