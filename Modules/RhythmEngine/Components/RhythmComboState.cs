using revghost.flecs;

namespace QuadrumPrototype.Modules.RhythmEngine.Components;

public partial struct RhythmComboState : IComponent<RhythmEngineModule>
{
    public int Count;
    public float Score;
}