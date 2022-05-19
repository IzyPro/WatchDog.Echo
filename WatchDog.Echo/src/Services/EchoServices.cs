using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WatchDog.Echo.src.Services
{
    public class EchoServices : EchoRPCService.EchoRPCServiceBase
    {
        public override Task<EchoResponse> SendEcho(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new EchoResponse
            {
                Message = "Success",
                StatusCode = (int)StatusCode.OK,
            });
        }
    }
}
