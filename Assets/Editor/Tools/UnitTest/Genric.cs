using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UGrpc.Pipeline.GrpcPipe.V1;

public class Generic
{
    // A Test behaves as an ordinary method
    [Test]
    public void GenericTestCase()
    {
        // Use the Assert class to test conditions

        // PrefabUtils.ChangeActivate(
        //     source: "Assets/Content/Test.prefab",
        //     path: "default/UnityEngine.MeshRenderer, UnityEngine",
        //     isActive: true

        // );

        PrefabUtils.AddComponent(
            source: "Assets/Content/Test.prefab",
            componentPath: "default/UnityEngine.MeshCollider, UnityEngine",
            isCreate: true
        );
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GenericWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
