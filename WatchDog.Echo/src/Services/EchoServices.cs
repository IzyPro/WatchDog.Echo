using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WatchDog.Echo.src.Services
{
    public class EchoServices : EchoRPCService.EchoRPCServiceBase
    {
        public override Task<EchoResponse> SendEcho(EchoRequest request, ServerCallContext context)
        {
            var htppContext = context.GetHttpContext();
            var callerHost = htppContext.Request.IsHttps ? $"https://{context.Host}" : $"http://{context.Host}";
            return Task.FromResult(new EchoResponse
            {
                Message = "Success",
                StatusCode = (int)StatusCode.OK,
                IsReverb = request.IsReverb,
                CallerHost = callerHost
            });
        }

        public override Task<EchoResponse> ReverbEcho(Empty request, ServerCallContext context)
        {
            var htppContext = context.GetHttpContext();
            var callerHost = htppContext.Request.IsHttps ? $"https://{context.Host}" : $"http://{context.Host}";
            return Task.FromResult(new EchoResponse
            {
                Message = "Success",
                StatusCode = (int)StatusCode.OK,
                IsReverb = false,
                CallerHost = callerHost
            });
        }

    }
}
