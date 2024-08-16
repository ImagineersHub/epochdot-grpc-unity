
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class SystemUtils
    {
        public static ProjectInfoResp GetProjectInfo()
        {
            return new ProjectInfoResp()
            {
                DataPath = Application.dataPath,
                ProjectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 6 /* Assets */),
                Platform = ProjectInfoResp.Types.PlatformCode.Unity,
                BuildVersion = Application.version,
                Status = new Status()
            };
        }

        public static GenericResp GetServiceStatus()
        {
            return new GenericResp()
            {
                Status = new Status()
                {
                    Code = Status.Types.StatusCode.Success,
                    Message = "SUCCESS"
                }
            };
        }
#if UNITY_EDITOR
        public static void QuitWithoutSaving()
        {
            EditorApplication.Exit(0);
        }
#endif
    }
}