
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class MaterialUtils
    {
        public static void UpdateTextures(string source, string diffuse, string channel)
        {
            var materialAsset = AssetDatabase.LoadAssetAtPath(source, typeof(Material)) as Material;
            var diffuseTexture = AssetDatabase.LoadAssetAtPath(diffuse, typeof(Texture2D)) as Texture2D;
            var channelTexture = AssetDatabase.LoadAssetAtPath(channel, typeof(Texture2D)) as Texture2D;

            materialAsset.mainTexture = diffuseTexture;
            materialAsset.SetTexture("_ChannelMap", channelTexture);

        }
    }
}