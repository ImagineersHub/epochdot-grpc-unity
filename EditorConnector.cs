using System.Threading.Tasks;
using Epochdot.Greet.V1;
using Grpc.Core;
using UnityEditor;
using UnityEngine;
public class EditorConnector : EditorWindow
{
    private static Server _grpcServer;
    private const int PORT = 50053;

    class GreeterImpl : Greeter.GreeterBase
    {
        private int _counter = 0;
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Debug.Log($"Received Message: {request.Name} - {_counter++}");
            return Task.FromResult(new HelloReply { Message = $"Hello {request.Name}, This is Unity" });
        }
    }

    [MenuItem("Connect/Start Sync Service")]
    public static void StartSyncService()
    {
        if (_grpcServer == null)
        {
            _grpcServer = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("localhost", PORT, ServerCredentials.Insecure) }
            };
            _grpcServer.Start();

            Debug.Log("Started Sync Service");

            Debug.Log(_grpcServer);
        }
    }

    [MenuItem("Connect/Stop Sync Service")]
    public static void StopSyncService()
    {
        Debug.Log("Stopped Sync Service");
        if (_grpcServer != null)
        {
            _grpcServer.ShutdownAsync().Wait();
            _grpcServer = null;
        }
    }
}
