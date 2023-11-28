#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UGrpc.Pipeline.GrpcPipe.V1;

using UnityEngine;

using UnityEditor;

namespace UGrpc
{
    public class UGrpcEditorPipeImpl : UGrpcPipeImpl
    {
        private Dictionary<string, System.Type> editorPipeAssembles;

        internal override Type defaultModule
        {
            get
            {
                return typeof(EditorWindow);
            }
        }

        public override Dictionary<string, System.Type> AssemblesMappings
        {
            get
            {
                if (editorPipeAssembles == null)
                {
                    editorPipeAssembles = new Dictionary<string, Type>(){
                        {"UGrpc.SystemUtils",typeof(SystemUtils)},
                        {"UGrpc.SceneUtils",typeof(SceneUtils)},
                        {"UGrpc.PrefabUtils",typeof(PrefabUtils)},
                        {"UGrpc.MaterialUtils",typeof(MaterialUtils)},
                        {"UGrpc.UnitTestUtils",typeof(UnitTestUtils)},
                        {"UnityEngine.Application",typeof(Application)},
                        {"UnityEditor.AssetDatabase",typeof(UnityEditor.AssetDatabase)},
                        {"UnityEditor.SceneManagement.EditorSceneManager",typeof(UnityEditor.SceneManagement.EditorSceneManager)}
                    };
                    editorPipeAssembles = editorPipeAssembles.Concat(base.mAssembles).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return editorPipeAssembles;
            }
        }

    }
}
#endif