using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UGrpc.Pipeline.GrpcPipe.V1;

namespace UGrpc.Runtime
{
    public class UGrpcRuntime : MonoBehaviour
    {

        public UGrpcRuntimeService Service { get; private set; }

        public UGrpcPipeImpl PipeImpl { get; private set; }

        void Awake()
        {
            Service = new UGrpcRuntimeService();
            PipeImpl = new UGrpcPipeImpl();
        }

        void Start()
        {
            Service.StartCommandServer(PipeImpl);
        }

        void OnDestroy()
        {
            Service.StopCommandServer();
        }
    }

}
