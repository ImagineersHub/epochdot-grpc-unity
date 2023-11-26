using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public static class AppSceneUtils
{
    public static string[] FetchSceneHierarchy()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        List<string> hierarchy = new List<string>();
        foreach (GameObject obj in rootObjects)
        {
            hierarchy.Add(obj.name);
            Debug.Log(obj.name);
        }

        return hierarchy.ToArray();
    }
}