using Godot;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Components;
using Quadrum.Game.Modules.Simulation.RhythmEngine.Utility;
using QuadrumPrototype.Client.Proxies;
using QuadrumPrototype.Modules;
using QuadrumPrototype.Modules.Proxy;
using QuadrumPrototype.Modules.RhythmEngine;
using QuadrumPrototype.Modules.RhythmEngine.Components;
using QuadrumPrototype.Modules.RhythmEngine.Systems;
using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Client;

public struct ClientModule : IModule<MainModule>
{
    public static void Setup(World world)
    {
        /*world.Register<ProxyDescription<RhythmEngineProxy>>();
        world.Register<RhythmEngineProxyManage>();*/

        world.RegisterGodotProxy<RhythmEngineInterfaceProxy>("res://scenes/rhythm_engine_interface.tscn");
        world.Register<RhythmEngineInterfaceProxy.OnProxyAdded>();
        world.Register<RhythmEngineInterfaceProxy.OnProxyUpdate>();

        world.Get<OnInputSystem>().DependsOn(
            world.Register<UpdateGameRhythmInputSystem>()
        );

        using (world.BeginScope(default))
        {
            world.New("Player")
                .Add<GameRhythmInput>();
        }
    }
}