using UnityEngine;
namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class UGrpcRuntimeService : UGrpcService
    {
        private const int DEFAULT_RUNTIME_GRPC_PORT = 50060;
        public override int DefaultPort
        {
            get
            {
                return DEFAULT_RUNTIME_GRPC_PORT;
            }
        }
    }
}