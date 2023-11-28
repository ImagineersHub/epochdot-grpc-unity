using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using UnityEditor;
using UnityEngine;
namespace UGrpc.Pipeline.GrpcPipe.V1
{
    interface IUGrpcService
    {
        static void StartCommandServer(UGrpcPipeImpl impl) => Console.WriteLine("empty");
        static void Dispose() => Console.WriteLine("empty");
        static bool IsRunning
        {
            get;
        }
    }
    public class UGrpcService : IUGrpcService
    {
        private Server mGrpcServer;

        private const int DEFAULT_GRPC_PORT = 50061; // 50060 for runtime

        public virtual int DefaultPort
        {
            get
            {
                return DEFAULT_GRPC_PORT;
            }
        }

        public bool IsRunning
        {
            get
            {
                return mGrpcServer?.Services.Count() > 0;
            }
        }

        public void Dispose()
        {
            // implement the dispose form inherited class 
            StopCommandServer();
        }


        public void StartCommandServer(UGrpcPipeImpl impl, int startPort = -1, bool autoFindPort = true)
        {
            if (!IsRunning)
            {
                if (startPort == -1)
                {
                    startPort = DefaultPort;
                }

                if (autoFindPort)
                {
                    startPort = UGrpcPipeImpl.GetValidPort(startPort: startPort);
                }

                mGrpcServer = new Server
                {
                    Services = { UGrpcPipe.BindService(impl) },
                    Ports = { new ServerPort("0.0.0.0", startPort, ServerCredentials.Insecure) }
                };

                mGrpcServer.Start();

                Debug.Log($"gRPC service is running on port: {startPort}");
            }
        }

        public void StopCommandServer()
        {
            if (IsRunning)
            {
                Debug.Log("Stopped gRPC service");
                // release resources
                mGrpcServer.ShutdownAsync().Wait();
                mGrpcServer = null;
            }
        }
    }
}