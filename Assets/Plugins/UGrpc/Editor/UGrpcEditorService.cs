#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class UGrpcEditorService : UGrpcService
    {
        private const int DEFAULT_EDITOR_GRPC_PORT = 50061;

        public override int DefaultPort
        {
            get
            {
                return DEFAULT_EDITOR_GRPC_PORT;
            }
        }
        public const string PREFS_KEY_AUTO_START_SERVER = "grpcpipe_auto_start_server";

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
    }
}
#endif