using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WatchDog.Echo.src.Utilities;

namespace WatchDog.Echo.src.Services
{
    public class EchoServices : EchoRPCService.EchoRPCServiceBase
    {
        public override Task<EchoResponse> SendEcho(EchoRequest request, ServerCallContext context)
        {
            return Task.FromResult(new EchoResponse
            {
                Message = "Echo Successful",
                StatusCode = (int)StatusCode.OK,
                IsReverb = request.IsReverb,
            });
        }

        public override Task<EchoResponse> ReverbEcho(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new EchoResponse
            {
                Message = "Reverb Successful",
                StatusCode = (int)StatusCode.OK,
                IsReverb = false,
            });
        }

    }
}
