using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using UnityEditor;
using UnityEngine;
namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class UGrpcPipeImpl : UGrpcPipe.UGrpcPipeBase
    {
        [Serializable]
        private struct CommandParserParam
        {
            public string method;

            public string type;

            public bool isMethod;

            public string[] parameters;
        }

        [Serializable]
        private class CommandParserPayload
        {
            public string data;
        }

        protected Dictionary<string, System.Type> mAssembles = new Dictionary<string, System.Type>()
        {
            {"UGrpc.SystemUtils",typeof(SystemUtils)},
            {"UGrpc.SceneUtils",typeof(SceneUtils)},
            {"UGrpc.PrefabUtils",typeof(PrefabUtils)},
            {"UGrpc.MaterialUtils",typeof(MaterialUtils)},
            {"UGrpc.UnitTestUtils",typeof(UnitTestUtils)},
            {"UnityEngine.Application",typeof(Application)},
            {"UnityEditor.AssetDatabase",typeof(UnityEditor.AssetDatabase)},
            {"UnityEditor.SceneManagement.EditorSceneManager",typeof(UnityEditor.SceneManagement.EditorSceneManager)}
        };

        public virtual Dictionary<string, System.Type> AssemblesMappings
        {
            get
            {
                return mAssembles;
            }
        }

        public static int GetValidPort(int startPort, int validRangeLength = 20)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            List<int> usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();
            return Enumerable.Range(startPort, validRangeLength).Where(port => !usedPorts.Contains(port)).FirstOrDefault();
        }

        private object[] ResolveCommandParameters(string[] parameters, MethodInfo method)
        {
            /* It's aimed to convert the request method name chain and payload string into the commandParserParam
            e.g., TODO
            */

            // declare an empty list for storing the resolved parameter object
            List<object> exportParams = new List<object>();

            var paramInfo = method.GetParameters();

            var methodParams = parameters.Zip(paramInfo, (v, p) => new { value = v, paramInfo = p });

            foreach (var paramItem in methodParams)
            {
                // Convert param type
                // TODO: reimplement the array parameter parser
                if (paramItem.paramInfo.ParameterType == typeof(System.String[]))
                {
                    exportParams.Add(Convert.ChangeType(paramItem.value.Split("%@%"), paramItem.paramInfo.ParameterType));
                }
                else if (paramItem.paramInfo.ParameterType.BaseType == typeof(System.Enum))
                {
                    object enumValue;
                    if (!Enum.TryParse(paramItem.paramInfo.ParameterType, paramItem.value, true, out enumValue))
                        throw new Exception($"Failed to parse enum value: {paramItem.value}");

                    exportParams.Add(enumValue);
                }
                else
                {
                    exportParams.Add(Convert.ChangeType(paramItem.value, paramItem.paramInfo.ParameterType));
                }

            }
            return exportParams.ToArray();
        }

        private async Task<object> CommandParserAsync(CommandParserParam cmdParam)
        {
            // Switch to main thread to allow asset manipulation through AssetDatabase
            await UniTask.SwitchToMainThread();

            // Parse the module type from the module name (e.g., UnityEditor.AssetDatabase)
            var module = AssemblesMappings.GetValueOrDefault(cmdParam.type, typeof(EditorWindow));
            if (module == null)
            {
                throw new Exception($"Not found the specified module: {cmdParam.type}");
            }
            // declare an empty payload object for storing the response payload data
            object payload = null;

            System.Type returnType = null;

            if (cmdParam.isMethod)
            {
                // parse the module method by the method name (e.g., MoveAsset / Refresh)
                var moduleMethods = module.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.Name.Equals(cmdParam.method, StringComparison.OrdinalIgnoreCase));

                MethodInfo moduleMethod = null;
                foreach (var loopItem in moduleMethods)
                {
                    if (loopItem.GetParameters().Length == cmdParam.parameters.Length)
                    {
                        // return the first matched method
                        moduleMethod = loopItem;
                        break;
                    }
                }

                if (moduleMethod == null)
                {
                    throw new Exception($"Not found matched method: {cmdParam.method}. It may be caused by unmatched method name, or unmatched parameter info");
                }

                // retrieve the method return type
                returnType = moduleMethod.ReturnType;

                // invoke static method by passing the specific parameters
                payload = moduleMethod.Invoke(null, ResolveCommandParameters(cmdParam.parameters, moduleMethod));
            }
            else
            {
                var propertyInfo = module.GetProperties().FirstOrDefault(x => x.Name.Equals(cmdParam.method, StringComparison.OrdinalIgnoreCase));

                returnType = propertyInfo.PropertyType;

                payload = propertyInfo.GetValue(null, null);
            }

            // convert the return payload by casting with invoking method return type
            return (payload == null) ? "" : Convert.ChangeType(payload, returnType);

        }

        public override Task<GenericResp> CommandParser(CommandParserReq request, Grpc.Core.ServerCallContext context)
        {
            var response = new GenericResp();
            var cmdParam = JsonUtility.FromJson<CommandParserParam>(request.Payload);
            try
            {
                var payload = CommandParserAsync(cmdParam).Result;
                // Debug.Log(payload.GetType());
                // Debug.Log(JsonUtility.ToJson(payload));
                if (payload is IMessage)
                {
                    response.Payload = Google.Protobuf.WellKnownTypes.Any.Pack(payload as IMessage);
                }
                else if (payload is string[])
                {
                    var payload_list = payload as string[];
                    var listValue = new Google.Protobuf.WellKnownTypes.ListValue();
                    listValue.Values.AddRange(payload_list.Select(s => new Google.Protobuf.WellKnownTypes.Value { StringValue = s }));
                    response.Payload = Google.Protobuf.WellKnownTypes.Any.Pack(listValue);
                }
                else if (payload is float[])
                {
                    FloatArrayRep floatArrayResp = new FloatArrayRep
                    {
                        Values = { payload as float[] }
                    };
                    response.Payload = Google.Protobuf.WellKnownTypes.Any.Pack(floatArrayResp);
                }
                else
                {
                    response.Payload = Google.Protobuf.WellKnownTypes.Any.Pack(
                        new Google.Protobuf.WellKnownTypes.StringValue { Value = payload.ToString() }
                    );

                }

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                response.Status = new Status { Code = Status.Types.StatusCode.Error, Message = ex.ToString() };
            }

            return System.Threading.Tasks.Task.FromResult(response);
        }
    }
}