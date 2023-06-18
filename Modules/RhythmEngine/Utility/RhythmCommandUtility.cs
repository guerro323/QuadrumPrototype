using Quadrum.Game.Modules.Simulation.RhythmEngine.Commands;
using revghost.flecs;

namespace Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;

public static class RhythmCommandUtility
{
    private static ReadOnlySpan<ComputedSliderFlowPressure> computeFlowPressures(
        Span<ComputedSliderFlowPressure> array, ReadOnlySpan<FlowPressure> executingCommand)
    {
        var resultCount = 0;
        for (var exec = 0; exec != executingCommand.Length; exec++)
        {
            var pressure = executingCommand[exec];
            if (!pressure.IsSliderEnd)
            {
                array[resultCount].Start = pressure;
                // Search for a slider end of the same key
                var tempExec = exec + 1;
                for (; tempExec < executingCommand.Length; tempExec++)
                    if (executingCommand[tempExec].KeyId == pressure.KeyId && executingCommand[tempExec].IsSliderEnd)
                    {
                        array[resultCount].End = executingCommand[tempExec];
                        break;
                    }

                /*// If we still have another pressure and the next pressure is a slider
                // Then 
                if (exec + 1 < executingCommand.Count && executingCommand[exec + 1].IsSliderEnd)
                {
                    array[resultCount].End = executingCommand[exec + 1];
                    exec++;
                }*/

                resultCount++;
            }
        }

        return array.Slice(0, resultCount);
    }

    public static bool CanBePredicted(ReadOnlySpan<RhythmCommandAction> commandTarget,
        ReadOnlySpan<ComputedSliderFlowPressure> executingCommand,
        TimeSpan beatInterval)
    {
        if (executingCommand.Length == 0)
            return true;

        if (executingCommand.Length > commandTarget.Length)
            return false;

        var startSpan = executingCommand[0].Start.FlowBeat * beatInterval;
        for (var i = 0; i != Math.Min(executingCommand.Length, commandTarget.Length); i++)
        {
            var action = commandTarget[i];
            var pressure = executingCommand[i];

            if (action.Key != pressure.Start.KeyId)
                return false;
            if (!action.Beat.IsPredictionValid(executingCommand[i], startSpan, beatInterval))
                return false;
        }

        return true;
    }

    public static bool CanBePredicted<TCommandList>(ReadOnlySpan<RhythmCommandAction> commandTarget,
        ReadOnlySpan<FlowPressure> executingCommand,
        TimeSpan beatInterval)
    {
        var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Length],
            executingCommand);
        return CanBePredicted(commandTarget, computedSpan, beatInterval);
    }

    public static bool SameAsSequence(ReadOnlySpan<RhythmCommandAction> commandTarget,
        ReadOnlySpan<ComputedSliderFlowPressure> executingCommand,
        TimeSpan beatInterval)
    {
        if (executingCommand.Length != commandTarget.Length)
            return false;

        //Console.WriteLine("begin");

        var startSpan = executingCommand[0].Start.FlowBeat * beatInterval;
        for (var i = 0; i != commandTarget.Length; i++)
        {
            var action = commandTarget[i];
            var pressure = executingCommand[i];

            //Console.WriteLine($"{i} - {action.Key}, {pressure.Start.KeyId}");

            if (action.Key != pressure.Start.KeyId)
                return false;

            if (!action.Beat.IsValid(executingCommand[i], startSpan, beatInterval))
                //Console.WriteLine("start=" + action.Beat.IsStartValid(executingCommand[i].Start.Time, startSpan, beatInterval));
                //Console.WriteLine($"slider={action.Beat.IsSliderValid(executingCommand[i].End.Time, startSpan, beatInterval)} {executingCommand[i].End.Time} {startSpan} ({executingCommand[i].IsSlider})");
                return false;
        }

        return true;
    }

    public static bool SameAsSequence<TCommandList>(ReadOnlySpan<RhythmCommandAction> commandTarget,
        ReadOnlySpan<FlowPressure> executingCommand,
        TimeSpan beatInterval)
    {
        var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Length],
            executingCommand);
        return SameAsSequence(commandTarget, computedSpan, beatInterval);
    }

    public static void GetCommand<TOutputEntityList>(World world, ReadOnlySpan<EntityId> entities,
        ReadOnlySpan<FlowPressure> executingCommand, in TOutputEntityList commandsOutput,
        bool isPredicted, TimeSpan beatInterval)
        where TOutputEntityList : IList<EntityId>
    {
        var computedSpan = computeFlowPressures(stackalloc ComputedSliderFlowPressure[executingCommand.Length],
            executingCommand);

        foreach (ref readonly var entity in entities)
        {
            var actionBuffer = entity.WithWorld(world).Get<RhythmCommandDescription>().Buffer;
            if (!isPredicted && SameAsSequence(actionBuffer, computedSpan, beatInterval))
            {
                commandsOutput.Add(entity);
                return;
            }

            if (isPredicted && CanBePredicted(actionBuffer, computedSpan, beatInterval))
                commandsOutput.Add(entity);
        }
    }
}