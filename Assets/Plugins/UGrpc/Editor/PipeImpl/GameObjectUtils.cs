
using UnityEngine;
using UnityEditor;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class GameObjectUtils
    {
        public static bool AssetExists(string path)
        {
            return AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)) != null;
        }

        public static void SetLayerRecursively(GameObject obj, string layerName)
        {
            var layerID = LayerMask.NameToLayer(layerName);
            if (layerID < 0)
            {
                throw new System.Exception($"Not found layer: {layerName}");
            }
            SetLayerRecursively(obj, layerID);
        }

        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            // Set the layer of the object and its children to the new layer.
            if (newLayer < 0) newLayer = 0;
            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public static void SetGameObjectStatic(GameObject obj, bool isStatic = true)
        {
            obj.isStatic = true;
            // Recursively set the isStatic flag for all children gameobjects
            foreach (Transform child in obj.GetComponentInChildren<Transform>())
            {
                child.gameObject.isStatic = isStatic;
            }
        }
    }
}