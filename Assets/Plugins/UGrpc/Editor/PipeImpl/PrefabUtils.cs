#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class PrefabFeeder : IDisposable
    {
        public GameObject Instance { get; set; }


        public string Target { get; set; }
        public string Source { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsUnpack { get; set; }
        public bool IsDestroy { get; set; }
        public bool IsStatic { get; set; }

        public UnityEngine.Object SourcePrefab { get; set; }
        public PrefabFeeder(string target, bool isReadOnly = false, bool isDestroy = true, bool isStatic = false)
        {
            // keep a copy in self property
            // it will perform a skip when dispose the object
            IsReadOnly = isReadOnly;

            IsDestroy = isDestroy;

            IsStatic = isStatic;



            if (IsReadOnly)
            {
                SourcePrefab = AssetDatabase.LoadAssetAtPath(target, typeof(GameObject)) as GameObject;
                Instance = PrefabUtility.InstantiatePrefab(SourcePrefab) as GameObject;
            }
            else
            {
                var assetName = Path.GetFileNameWithoutExtension(target);
                Instance = new GameObject(assetName);
                Target = target;
            }
        }

        public PrefabFeeder(string source, string target, bool isUnpack = false, bool isDestroy = true, bool isStatic = false)
        {
            // unpack the prefab asset before saving a new version (break connection from the original asset)
            IsUnpack = isUnpack;

            IsDestroy = isDestroy;

            IsStatic = isStatic;

            SourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(source);
            if (SourcePrefab == null)
            {
                var assetName = Path.GetFileNameWithoutExtension(source);
                Instance = new GameObject(assetName);
            }
            else
            {
                Instance = PrefabUtility.InstantiatePrefab(SourcePrefab) as GameObject;
            }

            Target = target;
            Source = source;

        }

        public void AddChild(Transform child)
        {
            child.parent = Instance.transform;
        }

        public void SetGameObjectStatic(GameObject obj)
        {
            obj.isStatic = true;

            // Recursively set the isStatic flag for all child GameObjects
            foreach (Transform child in obj.GetComponentsInChildren<Transform>())
            {
                child.gameObject.isStatic = true;
            }
        }

        public void Dispose()
        {
            if (!IsReadOnly)
            {
                if (IsUnpack)
                {
                    PrefabUtility.UnpackPrefabInstance(Instance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                }

                if (IsStatic)
                {
                    SetGameObjectStatic(Instance);
                }

                PrefabUtility.SaveAsPrefabAsset(Instance, Target);

            }

            if (IsDestroy)
                GameObject.DestroyImmediate(Instance, true);
        }
    }
    public class PrefabUtils
    {
        public static GenericResp CreatePrefab(string source, string target, bool isUnpack = false, bool isStatic = false)
        {
            using (var sourceInst = new PrefabFeeder(source, target, isUnpack, isStatic))
            {
                return new GenericResp
                {
                    Status = new Status { Code = Status.Types.StatusCode.Success, Message = $"Created prefab: {target}" }
                };
            }
        }
        public static GenericResp CreateModelAsset(string source, string target, bool disableLighting = true, string material = null)
        {
            using (var sourceInst = new PrefabFeeder(source, target))
            {
                var meshRenderer = sourceInst.Instance.GetComponent<MeshRenderer>();

                if (meshRenderer!=null && disableLighting)
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

                if (material != String.Empty && material != null)
                {
                    var materialAsset = AssetDatabase.LoadAssetAtPath(material, typeof(Material)) as Material;
                    meshRenderer.sharedMaterial = materialAsset;
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

        public static Type ParseType(string componentName, bool reportError = true)
        {
            Debug.Log($"Parse component: {componentName}");
            var compType = Type.GetType(componentName);
            if (compType == null && reportError) throw new Exception($"Not found component: {componentName}");
            return compType;
        }

        public static UnityEngine.Component ParseComponentInstance(GameObject obj, string path, bool reportError = true)
        {
            UnityEngine.Component compInst;
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

        private static GameObject FetchObjectByPath(GameObject root, string childPath, bool isCreate)
        {
            Transform childTrans = root.transform.Find(childPath);

            if (childTrans == null && isCreate)
            {
                if (!isCreate) throw new Exception($"Not found child chain: {childPath}");

                // create sub children objects
                childTrans = root.transform;
                // create child transform
                foreach (var childName in childPath.Split("/"))
                {
                    var childObj = new GameObject(name: childName);
                    childObj.transform.parent = childTrans;
                    childTrans = childObj.transform;
                }
            }

            return childTrans.gameObject;
        }

        public static void ChangeActivate(string source, string path, bool isActive)
        {
            using (var sourceInst = new PrefabFeeder(source, source))
            {
                var compChain = path.Split("/");

                var compType = ParseType(compChain[^1], reportError: false);

                if (compType == null)
                {
                    if (compChain[^1].Contains(","))
                        throw new Exception("Not found specified component");

                    var gameObject = FetchObjectByPath(sourceInst.Instance, path, false);
                    gameObject.SetActive(isActive);
                }
                else
                {
                    var childPath = string.Join("/", compChain[0..^1]);
                    var gameObject = FetchObjectByPath(sourceInst.Instance, childPath, false);
                    var compInst = gameObject.GetComponent(compType);
                    PropertyInfo enabledProperty = compType.GetProperty("enabled");
                    if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool))
                    {
                        enabledProperty.SetValue(compInst, isActive);
                    }
                }
            }
        }

        public static void AddComponent(string source, string componentPath, bool isCreate = true)
        {
            // componentPath represent the full path chain including the nested children path and component name
            // e.g., Collision/UnityEngine.MeshCollider, UnityEngine
            // e.g., <child name>/UnityEngine.MeshRenderer, UnityEngine
            var compChain = componentPath.Split("/");

            var compType = ParseType(compChain[^1]);

            GameObject gameObject;

            using (var sourceInst = new PrefabFeeder(source, source))
            {
                if (compChain.Length > 1)
                {
                    var childPath = string.Join("/", compChain[0..^1]);

                    // var childTrans = sourceInst.Instance.transform.Find(childPath);

                    // if (childTrans == null && !isCreate)
                    // {
                    //     throw new Exception($"Not found child chain: {childPath}");
                    // }
                    // else
                    // {
                    //     // create sub children objects
                    //     childTrans = sourceInst.Instance.transform;
                    //     // create child transform
                    //     foreach (var childName in childPath.Split("/"))
                    //     {
                    //         var childObj = new GameObject(name: childName);
                    //         childObj.transform.parent = childTrans;
                    //         childTrans = childObj.transform;
                    //     }
                    // }
                    gameObject = FetchObjectByPath(sourceInst.Instance, childPath, isCreate);
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
                UnityEngine.Component compInst = ParseComponentInstance(sourceInst.Instance, componentPath);
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

        private static int GetTotalMaterialCount(GameObject obj)
        {
            int materialCount = 0;

            // Get the material count of the current object
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Material[] materials = meshRenderer.sharedMaterials;
                materialCount += materials.Length;
            }

            // Recur for all children of the current object
            foreach (Transform child in obj.transform)
            {
                materialCount += GetTotalMaterialCount(child.gameObject);
            }

            return materialCount;
        }

        public class CTransform
        {
            public Vector3 scale;
            public Vector3 translate;
            public Vector3 rotate;


            public static explicit operator CTransform(string jsonString)
            {
                return JsonConvert.DeserializeObject<CTransform>(jsonString);
            }

        }
        public static void SetGIModeForMeshRenderer(MeshRenderer meshRenderer, int mode)
        {
            if (meshRenderer != null)
            {
                if (mode == 0)
                {
                    // No GI
                    meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    meshRenderer.receiveGI = ReceiveGI.LightProbes;
                }
                else if (mode == 1)
                {
                    // Use Lightmap
                    meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    meshRenderer.receiveGI = ReceiveGI.Lightmaps;
                }
                else if (mode == 2)
                {
                    // Use Light Probe
                    meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                    meshRenderer.receiveGI = ReceiveGI.LightProbes;
                }
                else
                {
                    Debug.LogError("Invalid mode: " + mode);
                }
            }
            else
            {
                Debug.LogError("No MeshRenderer attached to this GameObject.");
            }
        }


        public static void CreatePrefabVariant(string source,
                                                string target,
                                                bool unpack,
                                                bool isStatic,
                                                string paramStr,
                                                string[] materialAssetPaths,
                                                int giMode,
                                                bool castShadow,
                                                string layer)
        {
            //giMode: 0-> No GI, 1-> Lightmap, 2-> Light Probe
            using (var sourceInst = new PrefabFeeder(source: source, target: target, isUnpack: unpack, isStatic: isStatic))
            {
                var param = (CTransform)paramStr ?? throw new Exception("Found Invalid parameter string. failed to cast string to CTransform");
                var renderers = sourceInst.Instance.GetComponentsInChildren<MeshRenderer>();

                foreach (var meshRenderer in renderers)
                {
                    if (!castShadow) meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    SetGIModeForMeshRenderer(meshRenderer, giMode);
                }

                // update transform
                sourceInst.Instance.transform.localPosition = param.translate;
                sourceInst.Instance.transform.localRotation = Quaternion.Euler(param.rotate.x, param.rotate.y, param.rotate.z);
                sourceInst.Instance.transform.localScale = param.scale;

                if (materialAssetPaths.Length > 0) ApplyMaterials(sourceInst.Instance, materialAssetPaths);

                if (layer != null) GameObjectUtils.SetLayerRecursively(sourceInst.Instance, layer);
            }
        }

        static void ApplyMaterials(GameObject target, string[] materialAssetPaths)
        {
            var totalMaterialNumbers = GetTotalMaterialCount(target);
            if (materialAssetPaths.Length != 0 && totalMaterialNumbers != materialAssetPaths.Length) throw new Exception(
                "The specified material list don't match the total materials of the gameobject renderers" +
                $"{totalMaterialNumbers} : {materialAssetPaths.Length}");

            // resolve the material path to object
            List<Material> materialList = new();
            foreach (var materialPath in materialAssetPaths)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath) ?? throw new Exception($"Not found material{materialPath}");
                materialList.Add(mat);
            }

            var renderers = target.GetComponentsInChildren<MeshRenderer>();
            var matIndex = 0;
            foreach (var meshRenderer in renderers)
            {
                // skip the material assignment if the specified material list was empty
                if (materialList.Count > 0)
                {
                    var matLength = meshRenderer.sharedMaterials.Count();
                    meshRenderer.sharedMaterials = materialList.Skip(matIndex).Take(matLength).ToArray();
                    matIndex += matLength;
                }
            }
        }

        public static void SetActive(string source, string[] children, bool isActive)
        {
            using (var sourceInst = new PrefabFeeder(source))
            {
                if (children?.Length == 0)
                {
                    sourceInst.Instance.SetActive(isActive);
                }
                else
                {
                    foreach (var childPath in children)
                    {
                        var childTrans = sourceInst.Instance.transform.Find(childPath);
                        childTrans.gameObject.SetActive(isActive);
                    }
                }
            }
        }

        public static void Trim(string source, string[] children)
        {
            using (var sourceInst = new PrefabFeeder(source))
            {
                foreach (var childPath in children)
                {
                    var element = sourceInst.Instance.transform.Find(childPath);
                    if (element != null)
                    {
                        GameObject.DestroyImmediate(element.gameObject, true);
                        Debug.Log($"Removed child object: {childPath}");
                    }
                    else
                    {
                        Debug.LogWarning($"Not found child object: {childPath}");
                    }
                }
            }
        }
    }
}
#endif