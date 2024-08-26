
using UnityEngine;
using UnityEditor;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class AssetUtils
    {

        public static bool AssetExists(string path)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            return asset != null;
        }
    }
}