using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenLog : MonoBehaviour
{
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();

    void Start()
    {
        Debug.Log("Started up logging.");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 0, Screen.width - 20, Screen.height));
        GUI.skin.label.fontSize = 24;
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
        GUILayout.EndArea();
    }
}