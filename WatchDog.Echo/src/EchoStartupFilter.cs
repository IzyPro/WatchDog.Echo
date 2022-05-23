using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Net;
using WatchDog.Echo.src.Services;

namespace WatchDog.Echo.src
{
    internal class EchoStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseRouting();

                bool isIISExpress = String.Compare(Process.GetCurrentProcess().ProcessName, "iisexpress") == 0;
                if(isIISExpress)
                    app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

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
