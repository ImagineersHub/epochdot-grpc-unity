
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class LogBlocker : IDisposable
    {
        public LogBlocker()
        {
            /*
            Cysharp continuationQueue.RunCore can throw NullReferenceException when triggering the "EditorSceneManager.NewScene"
            static method. The exception was caused during the process of traversing the UniTask-runtime jobs. Temporarily disable
            the logger to ignore the unnecessary exception logs.
            */
            UnityEngine.Debug.unityLogger.logEnabled = false;
        }

        public void Dispose()
        {
            UnityEngine.Debug.unityLogger.logEnabled = true;
        }
    }
    public class SceneFeeder : IDisposable
    {
        public Scene Instance { get; set; }

        public string Target { get; set; }

        public SceneFeeder(string target, bool isForce)
        {
            using (var _ = new LogBlocker())
            {
                // try to open the existing scene
                var scene = EditorSceneManager.GetSceneByPath(target);
                if (scene.name != null && !isForce)
                {
                    Instance = EditorSceneManager.OpenScene(target, OpenSceneMode.Single);
                }
                else
                {
                    Instance = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }

                EditorSceneManager.SetActiveScene(Instance);
            }

            Target = target;
        }
        public SceneFeeder(string source, string target)
        {
            using (var _ = new LogBlocker())
            {
                Instance = EditorSceneManager.OpenScene(source, OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(Instance);
            }

            Target = target;
        }

        public void Dispose()
        {
            EditorSceneManager.SaveScene(Instance, Target);
        }

        public void DisableLog()
        {
            /*
            Cysharp continuationQueue.RunCore can throw NullReferenceException when triggering the "EditorSceneManager.NewScene"
            static method. The exception was caused during the process of traversing the UniTask-runtime jobs. Temporarily disable
            the logger to ignore the unnecessary exception logs.
            */
            UnityEngine.Debug.unityLogger.logEnabled = false;
        }

        public void EnableLog()
        {
            UnityEngine.Debug.unityLogger.logEnabled = true;
        }
    }
    public class SceneUtils
    {
        public static void CreateScene(string target, string[] assets, bool isForce)
        {
            using (var scene = new SceneFeeder(target, isForce))
            {
                foreach (var asset in assets)
                {
                    var assetInst = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                    if (assetInst != null)
                    {
                        PrefabUtility.InstantiatePrefab(assetInst);
                    }
                    else
                    {
                        Debug.Log($"Not found asset: {asset}");
                    }
                }
            }
        }
        public static void CreateScene(string source, string target, string[] assets)
        {
            using (var scene = new SceneFeeder(source, target))
            {
                foreach (var asset in assets)
                {
                    var assetInst = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
                    if (assetInst != null)
                    {
                        PrefabUtility.InstantiatePrefab(assetInst);
                    }
                    else
                    {
                        Debug.Log($"Not found asset: {asset}");
                    }
                }
            }
        }
        public static void SaveCurrentScene()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif