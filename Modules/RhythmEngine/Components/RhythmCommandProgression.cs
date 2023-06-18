using System.Runtime.InteropServices;
using Quadrum.Game.Modules.Simulation.RhythmEngine;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using revghost.flecs;
using revghost.v2.Utility;

namespace QuadrumPrototype.Modules.RhythmEngine.Components;

public unsafe partial struct RhythmCommandProgression : IComponent<RhythmEngineModule>
{
    private const int capacity = 16;
    private const int size = capacity * 32;
    
    private fixed byte buffer[size];

    public int Length;
    public Span<FlowPressure> Buffer => MemoryMarshal.Cast<byte, FlowPressure>(MemoryMarshal.CreateSpan(ref buffer[0], size))[..Length];

    public void Add(FlowPressure pressure)
    {
        if (Length + 1 >= capacity)
            RemoveAt(0);
        else
            Length += 1;

        Console.WriteLine($"Length={Length} ({MemoryMarshal.Cast<byte, FlowPressure>(MemoryMarshal.CreateSpan(ref buffer[0], size)).Length})");
        Buffer[^1] = pressure;
    }

    public void RemoveAt(int index)
    {
        if (Buffer.RemoveAt(index))
        {
            Length--;
            Console.WriteLine($"removed {Length}");
        }
    }
}