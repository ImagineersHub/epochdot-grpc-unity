#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UGrpc.Pipeline.GrpcPipe.V1
{
    public class UnitTestUtils
    {
        public static float[] GetFloatArrayData()
        {
            return new float[3] { 1f, 2f, 3f };
        }
    }
}
#endif