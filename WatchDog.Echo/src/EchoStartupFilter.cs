using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using WatchDog.Echo.src.Models;
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
                    if(Protocol.ProtocolType == Enums.ProtocolEnum.gRPC)
                        endpoints.MapGrpcService<EchoServices>();
                    else
                    {
                        endpoints.MapGet(Constants.RestEndpoint, async context =>
                        {
                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            var response = new EchoHTTPResponse
                            {
                                Message = "Echo Successful",
                                StatusCode = (int)HttpStatusCode.OK,
                            };
                            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                        });
                    }
                });
                // Call the next configure method
                next(app);
            };
        }
    }
}
