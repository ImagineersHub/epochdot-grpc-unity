using UnityEditor;
using UnityEngine;

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
                Status = new Status()
            };
        }
    }
}