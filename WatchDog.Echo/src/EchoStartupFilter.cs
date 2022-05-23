using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WatchDog.Echo.src.Services;
using WatchDog.Echo.src.Utilities;

namespace WatchDog.Echo.src
{
    internal class EchoStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    //Registering an endpoint for non server application (worker service)
                    endpoints.MapGet("echo", async context =>
                    {
                        context.Response.ContentType = "text/html";
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        await context.Response.WriteAsync("Echo is listening");
                    });
                    endpoints.MapGrpcService<EchoServices>();
                });
                // Call the next configure method
                next(app);
            };
        }
    }
}
