#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class MaterialUtils
    {
        public static void UpdateTextures(string source, string shaderName, string diffuse, string channel, string normal = null, string emission = null)
        {
            var materialAsset = AssetDatabase.LoadAssetAtPath<Material>(source);
            if (materialAsset == null)
            {
                Debug.LogError($"Material not found at path: {source}");
                return;
            }

            // Set the shader
            var shader = Shader.Find(shaderName);
            if (shader != null)
            {
                materialAsset.shader = shader;
            }
            else
            {
                Debug.LogError($"Shader '{shaderName}' not found. Make sure it's included in your project.");
                return;
            }

            // Set main texture (albedo)
            var diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(diffuse);
            if (diffuseTexture != null)
            {
                materialAsset.SetTexture("_MainTex", diffuseTexture);
            }
            else
            {
                Debug.LogWarning($"Diffuse texture not found at path: {diffuse}");
            }

            // Set channel map
            var channelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(channel);
            if (channelTexture != null)
            {
                materialAsset.SetTexture("_ChannelMap", channelTexture);
                materialAsset.EnableKeyword("_CHANNEL_MAP");
            }
            else
            {
                Debug.LogWarning($"Channel map texture not found at path: {channel}");
            }

            // Set normal map if provided
            if (!string.IsNullOrEmpty(normal))
            {
                var normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(normal);
                if (normalTexture != null)
                {
                    materialAsset.SetTexture("_NormalMap", normalTexture);
                    materialAsset.EnableKeyword("_NORMAL_MAP");
                }
                else
                {
                    Debug.LogWarning($"Normal map texture not found at path: {normal}");
                }
            }

            // Set emission map if provided
            if (!string.IsNullOrEmpty(emission))
            {
                var emissionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(emission);
                if (emissionTexture != null)
                {
                    materialAsset.SetTexture("_EmissiveMap", emissionTexture);
                    materialAsset.EnableKeyword("_EMISSION");
                }
                else
                {
                    Debug.LogWarning($"Emission map texture not found at path: {emission}");
                }
            }

            // Force material to update
            EditorUtility.SetDirty(materialAsset);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif