using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TinyWebServerLib.Attributes;

namespace TinyLogger
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProxiedTransient<TImplementation>(this IServiceCollection services)
        where TImplementation : class
        {
            services.AddTransient<TImplementation>();
            services.AddTransient(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<TImplementation>>();
                var proxyGenerator = new ProxyGenerator();
                var interceptor = new LoggerInterceptor(logger);
                var actual = sp.GetRequiredService<TImplementation>();
                return proxyGenerator.CreateClassProxyWithTarget(actual, interceptor);
            });
            return services;
        }

        public static IServiceCollection AddProxiedTransient<TInterface, TImplementation>(this IServiceCollection services)
        where TImplementation : class, TInterface
        where TInterface : class
        {
            services.AddTransient<TImplementation>();
            services.AddTransient(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<TImplementation>>();
                var proxyGenerator = new ProxyGenerator();
                var interceptor = new LoggerInterceptor(logger);
                var actual = sp.GetRequiredService<TImplementation>();
                return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(actual, interceptor);
            });
            return services;
        }

        public static IServiceCollection AddProxiedScoped<T>(this IServiceCollection services) where T : class
        {
            services.AddScoped<T>();
            services.AddScoped(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<T>>();
                var proxyGenerator = new ProxyGenerator();
                var interceptor = new LoggerInterceptor(logger);

                // Use ActivatorUtilities to create the instance directly,
                // avoiding circular dependency with GetServices
                var actual = ActivatorUtilities.CreateInstance<T>(sp);
                return proxyGenerator.CreateClassProxyWithTarget(actual, interceptor);
            });
            return services;
        }

        public static IServiceCollection AddProxiedControllers(this IServiceCollection services, Assembly assembly)
        {
            var controllerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ApiControllerAttribute>() != null);

            foreach (var type in controllerTypes)
            {
                // Register a factory that creates the proxy.
                services.AddScoped(type, provider =>
                {
                    // Manually create an instance of the real controller,
                    // resolving its dependencies from the container.
                    // This avoids the circular dependency.
                    var actualController = ActivatorUtilities.CreateInstance(provider, type);

                    var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(type);
                    var proxyGenerator = new ProxyGenerator();
                    var interceptor = new LoggerInterceptor(logger);

                    // Create a proxy that wraps the actual controller instance.
                    return proxyGenerator.CreateClassProxyWithTarget(type, actualController, interceptor);
                });
            }
            return services;
        }
    }
}
