namespace BasicDependencyInjectionLibrary.Decorators;

public static class DecoratorChainExtensions
{
    // C# 14 extension block (receiver is IServiceCollection)
    extension(IServiceCollection services)
    {
        public IServiceCollection ReplaceSingleton<TService, TImpl>()
            where TService : class
            where TImpl : class, TService
            => ReplaceBase<TService, TImpl>(services, ServiceLifetime.Singleton);

        public IServiceCollection ReplaceScoped<TService, TImpl>()
            where TService : class
            where TImpl : class, TService
            => ReplaceBase<TService, TImpl>(services, ServiceLifetime.Scoped);

        public IServiceCollection ReplaceTransient<TService, TImpl>()
            where TService : class
            where TImpl : class, TService
            => ReplaceBase<TService, TImpl>(services, ServiceLifetime.Transient);

        // FIRST added runs FIRST
        public IServiceCollection Decorate<TService, TDecorator>()
            where TService : class
            where TDecorator : class, TService
        {
            var registry = DecoratorChainRegistry.GetOrAdd(services);
            var chain = registry.GetOrAddChain(typeof(TService));

            chain.Decorators.Add(typeof(TDecorator));

            EnsureChainRegistrations<TService>(services, chain);
            return services;
        }
    }

    private static IServiceCollection ReplaceBase<TService, TImpl>(IServiceCollection services, ServiceLifetime lifetime)
        where TService : class
        where TImpl : class, TService
    {
        var registry = DecoratorChainRegistry.GetOrAdd(services);
        var chain = registry.GetOrAddChain(typeof(TService));

        chain.Lifetime = lifetime;
        chain.BaseImplementationType = typeof(TImpl);

        // Register/replace the INNER base holder (correctly typed)
        services.Replace(ServiceDescriptor.Describe(
            typeof(IInner<TService>),
            sp =>
            {
                var baseImpl = (TService)ActivatorUtilities.CreateInstance(sp, typeof(TImpl));
                return new Inner<TService>(baseImpl);
            },
            lifetime));

        EnsureChainRegistrations<TService>(services, chain);
        return services;
    }

    private static void EnsureChainRegistrations<TService>(
        IServiceCollection services,
        DecoratorChainInfo chain)
        where TService : class
    {
        if (chain.BaseImplementationType is null)
            throw new InvalidOperationException(
                $"Base not set for {typeof(TService).FullName}. Call ReplaceSingleton/Scoped/Transient first.");

        // Ensure inner exists (ReplaceBase sets it; this is just safety)
        services.TryAdd(ServiceDescriptor.Describe(
            typeof(IInner<TService>),
            sp =>
            {
                // This path only happens if somebody called Decorate before Replace*
                // but we already throw above, so it’s mostly a safeguard.
                throw new InvalidOperationException($"Inner base not configured for {typeof(TService).FullName}.");
            },
            chain.Lifetime));

        // Replace the service with a factory that builds the decorator chain.
        // To make FIRST decorator added run FIRST, build from LAST -> FIRST so FIRST-added becomes outermost.
        services.Replace(ServiceDescriptor.Describe(
            typeof(TService),
            sp =>
            {
                var inner = sp.GetRequiredService<IInner<TService>>().Value;
                var current = inner;

                for (int i = chain.Decorators.Count - 1; i >= 0; i--)
                {
                    var decoratorType = chain.Decorators[i];
                    current = (TService)ActivatorUtilities.CreateInstance(sp, decoratorType, current);
                }

                return current;
            },
            chain.Lifetime));
    }

    // =====================================================
    // Helper types
    // =====================================================

    internal sealed class DecoratorChainRegistry
    {
        private readonly Dictionary<Type, DecoratorChainInfo> _chains = new();

        public DecoratorChainInfo GetOrAddChain(Type serviceType)
        {
            if (!_chains.TryGetValue(serviceType, out var info))
            {
                info = new DecoratorChainInfo();
                _chains[serviceType] = info;
            }
            return info;
        }

        public static DecoratorChainRegistry GetOrAdd(IServiceCollection services)
        {
            for (int i = services.Count - 1; i >= 0; i--)
            {
                var d = services[i];
                if (d.ServiceType == typeof(DecoratorChainRegistry) &&
                    d.ImplementationInstance is DecoratorChainRegistry reg)
                {
                    return reg;
                }
            }

            var created = new DecoratorChainRegistry();
            services.AddSingleton(created);
            return created;
        }
    }

    internal sealed class DecoratorChainInfo
    {
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
        public Type? BaseImplementationType { get; set; }
        public List<Type> Decorators { get; } = new();
    }

    internal interface IInner<TService> where TService : class
    {
        TService Value { get; }
    }

    internal sealed class Inner<TService>(TService value) : IInner<TService> where TService : class
    {
        public TService Value { get; } = value;
    }
}