using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using UGrpc.Pipeline.GrpcPipe.V1;
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
        public const string PREFS_KEY_AUTO_START_SERVER = "grpcpipe_auto_start_server";

        private Server mGrpcServer;

        private const int DEFAULT_PORT = 50061;

        public virtual int DefaultPort
        {
            get
            {
                return DEFAULT_PORT;
            }
        }

        public bool IsRegistered
        {
            get
            {
                return EditorPrefs.GetBool(PREFS_KEY_AUTO_START_SERVER, false);
            }
            set
            {
                EditorPrefs.SetBool(PREFS_KEY_AUTO_START_SERVER, value);
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
            if (IsRunning)
            {
                mGrpcServer.ShutdownAsync().Wait();
                mGrpcServer = null;
            }

        }


        public void StartCommandServer(UGrpcPipeImpl impl, int startPort)
        {
            if (!IsRunning)
            {
                var gRPCPort = UGrpcPipeImpl.GetValidPort(startPort: startPort);
                mGrpcServer = new Server
                {
                    Services = { UGrpcPipe.BindService(impl) },
                    Ports = { new ServerPort("0.0.0.0", gRPCPort, ServerCredentials.Insecure) }
                };

                mGrpcServer.Start();

                Debug.Log($"[SynPusher] gRPC service is running on port: {gRPCPort}");
            }
        }
    }
}