using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class UGrpcService : IDisposable
    {
        private Server mGrpcServer;
        private const int DEFAULT_GRPC_PORT = 50061; // 50060 for runtime
        private const int SHUTDOWN_TIMEOUT_MS = 5000; // 5 seconds timeout for shutdown

        public virtual int DefaultPort => DEFAULT_GRPC_PORT;

        public bool IsRunning => mGrpcServer != null && mGrpcServer.Ports.Count() > 0;

        private bool disposedValue;

        public int CurrentPort => IsRunning ? mGrpcServer.Ports.First().BoundPort : -1;

        public void StartCommandServer(UGrpcPipeImpl impl, int startPort = -1, bool autoFindPort = true)
        {
            if (IsRunning)
            {
                Debug.LogWarning("gRPC service is already running.");
                return;
            }

            try
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
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start gRPC service: {ex.Message}");
                mGrpcServer = null;
            }
        }

        public void StopCommandServer()
        {
            if (!IsRunning)
            {
                Debug.LogWarning("gRPC service is not running.");
                return;
            }

            try
            {
                Debug.Log("Stopping gRPC service...");
                var shutdownTask = mGrpcServer.ShutdownAsync();
                if (Task.WaitAny(new[] { shutdownTask }, SHUTDOWN_TIMEOUT_MS) == -1)
                {
                    Debug.LogWarning("gRPC service shutdown timed out. Forcing shutdown.");
                    mGrpcServer.KillAsync().Wait();
                }
                Debug.Log("gRPC service stopped successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while stopping gRPC service: {ex.Message}");
            }
            finally
            {
                mGrpcServer = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopCommandServer();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}