
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class PrefabFeeder : IDisposable
    {
        public GameObject Instance { get; set; }

        public string Target { get; set; }
        public string Source { get; set; }

        public PrefabFeeder(string target)
        {
            Instance = new GameObject();
            Target = target;
        }
        public PrefabFeeder(string source, string target)
        {
            var sourcePrefab = AssetDatabase.LoadAssetAtPath(source, typeof(GameObject)) as GameObject;
            Instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            Target = target;
            Source = source;
        }

        public void Dispose()
        {
            PrefabUtility.SaveAsPrefabAsset(Instance, Target);
            GameObject.DestroyImmediate(Instance, true);
        }
    }
    public class PrefabUtils
    {

        public static GenericResp CreateModelAsset(string source, string target, bool disableLighting = true, string material = null)
        {
            using (var sourceInst = new PrefabFeeder(source, target))
            {
                var meshRenderer = sourceInst.Instance.GetComponent<MeshRenderer>();

                if (disableLighting)
                {
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    meshRenderer.receiveShadows = false;

                    meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    meshRenderer.allowOcclusionWhenDynamic = false;
                    meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                }

                // reset scale
                sourceInst.Instance.transform.localScale = new Vector3(1, 1, 1);

                if (material != null)
                {
                    var materialAsset = AssetDatabase.LoadAssetAtPath(material, typeof(Material)) as Material;
                    meshRenderer.material = materialAsset;
                }
            }


            return new GenericResp
            {
                Status = new Status { Code = Status.Types.StatusCode.Success, Message = $"Created prefab: {target}" }
            };
        }

        public static void Merge(string[] assets, string target)
        {
            if (assets.Length > 1)
            {
                using (var parentPrefab = new PrefabFeeder(target))
                {
                    foreach (var asset in assets)
                    {
                        var childPrefab = AssetDatabase.LoadAssetAtPath(asset, typeof(GameObject)) as GameObject;
                        var sourceInst = PrefabUtility.InstantiatePrefab(childPrefab) as GameObject;
                        sourceInst.transform.parent = parentPrefab.Instance.transform;
                    }
                }
            }
            else
            {
                AssetDatabase.CopyAsset(assets[0], target);
            }
        }

        private static Type ParseType(string path, bool reportError = true)
        {
            var componentName = path;

            var compType = Type.GetType(path);

            if (compType == null && reportError) throw new Exception($"Not found component: {componentName}");

            return compType;
        }

        private static Component ParseComponentInstance(GameObject obj, string path, bool reportError = true)
        {
            Component compInst;
            var compChain = path.Split("/");

            var compType = ParseType(compChain[^1]);

            if (compChain.Length > 1)
            {
                var childPath = string.Join("/", compChain[0..^1]);

                var childTrans = obj.transform.Find(childPath);

                if (childTrans == null) throw new Exception($"Not found child chain: {childPath}");

                compInst = childTrans.gameObject.GetComponent(compType);
            }
            else
            {
                compInst = obj.GetComponent(compType);
            }

            if (reportError && compInst == null) throw new Exception($"Not found the specified component: {path}, asset: {AssetDatabase.GetAssetPath(obj)}");

            return compInst;
        }

        public static void AddComponent(string source, string componentPath, bool isCreate = true)
        {
            // componentPath represent the full path chain including the nested children path and component name
            // e.g., Collision/UnityEngine.MeshCollider
            var compChain = componentPath.Split("/");

            var compType = ParseType(compChain[^1]);

            GameObject gameObject;

            using (var sourceInst = new PrefabFeeder(source, source))
            {
                if (compChain.Length > 1)
                {
                    var childPath = string.Join("/", compChain[0..^1]);

                    var childTrans = sourceInst.Instance.transform.Find(childPath);

                    if (childTrans == null && !isCreate)
                    {
                        throw new Exception($"Not found child chain: {childPath}");
                    }
                    else
                    {
                        // create sub children objects
                        childTrans = sourceInst.Instance.transform;
                        // create child transform
                        foreach (var childName in childPath.Split("/"))
                        {
                            var childObj = new GameObject(name: childName);
                            childObj.transform.parent = childTrans;
                            childTrans = childObj.transform;
                        }
                    }
                    gameObject = childTrans.gameObject;
                }
                else
                {
                    gameObject = sourceInst.Instance;
                }

                if (gameObject.GetComponent(compType) == null)
                {
                    gameObject.AddComponent(compType);
                }
            }
        }

        public static void SetValue(string source, string componentPath, string propertyName, object value)
        {

            using (var sourceInst = new PrefabFeeder(source, source))
            {
                Component compInst = ParseComponentInstance(sourceInst.Instance, componentPath);
                var propertyInfo = compInst.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
                propertyInfo.SetValue(compInst, Convert.ChangeType(value, propertyInfo.PropertyType));
            }
        }

        public static void SetReferenceValue(string source, string sourceComponentPath, string sorucePropertyName, string target,
                                             string targetComponentPath, string targetPropertyName)
        {
            // it's aimed to set value by passing a reference link
            // e.g., set mesh link to the MeshFilter from a specific fbx prefab
            var sourceCompChain = sourceComponentPath.Split("/");
            var targetCompChain = targetComponentPath.Split("/");

            var sourceInst = AssetDatabase.LoadAssetAtPath(source, typeof(GameObject)) as GameObject;
            var sourceCompInst = ParseComponentInstance(sourceInst, sourceComponentPath);
            var sourcePropInfo = sourceCompInst.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(sorucePropertyName, StringComparison.OrdinalIgnoreCase));
            var sourceValue = sourcePropInfo.GetValue(sourceCompInst);

            using (var targetInst = new PrefabFeeder(target, target))
            {
                var targetCompInst = ParseComponentInstance(targetInst.Instance, targetComponentPath);
                var targetPropInfo = targetCompInst.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(targetPropertyName, StringComparison.OrdinalIgnoreCase));
                if (targetPropInfo.PropertyType != sourcePropInfo.PropertyType) throw new Exception("Source and Target property types are not same!");
                targetPropInfo.SetValue(targetCompInst, sourceValue);
            }
        }
    }
}