using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.Simulation;

[Phase(typeof(SimulationPhase))]
public partial struct TimeProgressSystem : ISystem<SimulationModule>
{
    [Singleton] private GameTime _gameTime;
    
    public void Each()
    {
        _gameTime.Delta = TimeSpan.FromSeconds(ProcessorContext.DeltaSystemTime);
        
        _gameTime.Frame += 1;
        _gameTime.Total += _gameTime.Delta;
    }
}