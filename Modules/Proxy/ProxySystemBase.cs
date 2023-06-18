using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using flecs_hub;
using QuadrumPrototype.Client.Proxies;
using QuadrumPrototype.Modules.Simulation.Components;
using revghost.flecs;
using revghost.flecs.Utilities.Generator;

namespace QuadrumPrototype.Modules.Proxy;

public interface IProxyFilter : IEntityFilter, IStaticEntity {}

[ManagedField<HashSet<EntityId>>("Add")]
[ManagedField<HashSet<EntityId>>("Remove")]
public unsafe partial struct ProxyQueue<T0> : IComponent, IStaticEntityParent<T0>
    where T0 : IProxyFilter
{
    public void ClearAll()
    {
        Add.Clear();
        Remove.Clear();
    }
}

public unsafe partial struct ProxyDescription<T0> : IStaticEntity, IStaticEntitySetup
    where T0 : IProxyFilter
{
    static void IStaticEntitySetup.Setup(World world)
    {
        var selfEnt = world.Scope.Value!;
        var filter = T0.GetFilter();
        
        world.Register<ProxyQueue<T0>>().Set(new ProxyQueue<T0>
        {
            Add = new HashSet<EntityId>(),
            Remove = new HashSet<EntityId>()
        });

        Console.WriteLine($"{world.Get<ProxyQueue<T0>>().Get<ProxyQueue<T0>>().Add}");
        
        Console.WriteLine("ok");
        {
            var onAdd = world.New("OnAdd");
            using var cpy = DisposableArray<flecs.ecs_term_t>.Create(filter.terms_buffer, filter.terms_buffer_count, 0);
            foreach (ref var elem in cpy.Span[..(cpy.Length - 1)])
            {
                elem.inout = flecs.ecs_inout_kind_t.EcsInOutNone;
            }

            ProcessorUtility.SetupObserverManaged(world.Handle, onAdd, new flecs.ecs_filter_desc_t
                {
                    terms_buffer = (flecs.ecs_term_t*) Unsafe.AsPointer(ref MemoryMarshal.GetReference(cpy.Span)),
                    terms_buffer_count = cpy.Length
                }, null, typeof(MonitorFacade),
                (delegate*unmanaged<flecs.ecs_iter_t*, void>) Marshal.GetFunctionPointerForDelegate(
                    MonitorFacade.EachUnmanaged
                )
            );
        }
    }

    /*[ManagedField<HashSet<EntityId>>("Set")]
    public partial struct ManagedData : IComponent {}*/
    
    private partial struct MonitorFacade : IEntityObserver<OnMonitor>
    {
        public void Each()
        {
            Console.WriteLine("rhythm engine proxy!!!");

            var world = ProcessorContext.World;
            // ugly, but we can't use [Singleton] fields here :(
            var queue = world.Get<ProxyQueue<T0>>().Get<ProxyQueue<T0>>();

            if (ProcessorContext.TargetEvent.Is<OnAdd>())
            {
                Console.WriteLine("added!");
                // Entity.Add(StaticEntity<ProxyDescription<T0>>.Id);

                queue.Add.Add(Id);
            }
            else if (ProcessorContext.TargetEvent.Is<OnRemove>())
            {
                Console.WriteLine("removed!");

                queue.Remove.Remove(Id);
            }
        }
    }
}