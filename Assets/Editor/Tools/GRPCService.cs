
using UGrpc.Pipeline.GrpcPipe.V1;
using UnityEditor;
using UnityEngine;

namespace UGrpc
{
    [InitializeOnLoad]
    public class GRPCService : EditorWindow
    {

        static private UGrpcEditorService mGrpcService = new UGrpcEditorService();

        public static void Dispose()
        {
            mGrpcService.Dispose();
        }

        static GRPCService()
        {
            Application.wantsToQuit += WantsToQuit;

            //Auto start the grpcpipe command server if the auto-start mode
            // is turned on through registry key: grpcpipe_auto_start_server
            if (EditorPrefs.GetBool(UGrpcEditorService.PREFS_KEY_AUTO_START_SERVER))
            {
                mGrpcService.StartCommandServer(new UnittestPipeImpl(), mGrpcService.DefaultPort);
            }
        }
        static bool WantsToQuit()
        {
            // force shutdown the gRPC service when closing unity editor
            mGrpcService.Dispose();
            return true;
        }


        [MenuItem("Unitest/Pipeline/Start gRPC Server", priority = 1)]
        public static void StartCommandServer()
        {
            mGrpcService.StartCommandServer(new UnittestPipeImpl(), mGrpcService.DefaultPort);
        }

        [MenuItem("Unitest/Pipeline/Start gRPC Server", true)]
        static bool StartCommandServerValidator()
        {
            return !mGrpcService.IsRunning;
        }

        [MenuItem("Unitest/Pipeline/Stop gRPC Server", priority = 2)]
        public static void StopCommandServer()
        {
            if (mGrpcService.IsRunning)
            {
                Debug.Log("[Unitest] Stopped gRPC service");
                // release resources
                mGrpcService.Dispose();
            }
        }

        [MenuItem("Unitest/Pipeline/Stop gRPC Server", true)]
        static bool StopCommandServerValidator()
        {
            return mGrpcService.IsRunning;
        }

        [MenuItem("Unitest/Pipeline/AutoStart gRPC Server", priority = 13)]
        public static void TurnOnAutoStartCommandServer()
        {
            mGrpcService.IsRegistered = true;
        }

        [MenuItem("Unitest/Pipeline/AutoStart gRPC Server", true)]
        static bool TurnOnAutoStartCommandServerValidator()
        {
            return !mGrpcService.IsRegistered;
        }

        [MenuItem("Unitest/Pipeline/Cancel AutoStart gRPC Server", priority = 14)]
        public static void TurnOffAutoStartCommandServer()
        {
            mGrpcService.IsRegistered = false;
        }

        [MenuItem("Unitest/Pipeline/Cancel AutoStart gRPC Server", true)]
        static bool TurnOffAutoStartCommandServerValidator()
        {
            return mGrpcService.IsRegistered;
        }

    }

}
