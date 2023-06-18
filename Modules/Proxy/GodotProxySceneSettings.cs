using Godot;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.Proxy;

[ManagedField<string>("ScenePath")]
public partial struct GodotProxySceneSettings<T0> : IComponent
{
    public bool Duplicate;
}

[ManagedField<Node>("Node")]
public partial struct GodotProxy<T> : IComponent {}

public partial struct GodotProxySystem<T0> : ISystem
    where T0 : IProxyFilter
{
    [Singleton] public ProxyQueue<T0> Queue;
    [Singleton] public GodotProxySceneSettings<T0> GodotProxy;

    public void Each()
    {
        var world = ProcessorContext.World;
        
        Resource? res = null;
        foreach (var target in Queue.Add)
        {
            if (res == null)
            {
                res = ResourceLoader.LoadThreadedGet(GodotProxy.ScenePath);
            }

            if (GodotProxy.Duplicate)
                res = res.Duplicate();

            if (res is PackedScene ps)
            {
                var ent = target.WithWorld(world);
                ent.Set(new GodotProxy<T0>
                {
                    Node = ps.Instantiate()
                });
            }
        }
        
        Queue.Add.Clear();
    }
}

public static class GodotProxyExt
{
    public static void RegisterGodotProxy<T>(this World world, string scene)
        where T : IProxyFilter
    {
        ResourceLoader.LoadThreadedRequest(scene);

        world.Register<T>();
        world.Register<ProxyDescription<T>>();
        world.Register<GodotProxySystem<T>>();
        world.Register<GodotProxy<T>>();
        world.Register<GodotProxySceneSettings<T>>().Set(new GodotProxySceneSettings<T>
        {
            ScenePath = scene
        });
    }
}