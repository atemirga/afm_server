using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebSocketManager
{
    public static class WebSocketManagerExtensions
    {
        public static IServiceCollection AddWebSocketManager(this IServiceCollection services, Assembly assembly = null)
        {
            return services.AddWebSocketManager<CommonWebSocketConnection>();
        }

        public static IServiceCollection AddWebSocketManager<TConnection>(this IServiceCollection services, Assembly assembly = null) where TConnection:WebSocketConnection
        {
            services.AddTransient<WebSocketConnectionManager>();

            Assembly ass = assembly ?? Assembly.GetEntryAssembly();

            foreach (var type in ass.ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketHandler))
                {
                    services.AddSingleton(type);
                }
            }

            return services;
        }

        public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app,
                                                              PathString path,
                                                              WebSocketHandler handler,
                                                              string authScheme)
        {
            if (System.Reflection.Assembly.GetEntryAssembly().GetName().Name == "ef")
            {
                return app;
            }

            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketManagerMiddleware>(handler, authScheme));
        }
    }
}