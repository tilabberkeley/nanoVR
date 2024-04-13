/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using UnityEngine.UI;

public class ConsoleToText : MonoBehaviour
{
    public Text debugText;
    string output = "";
    string stack = "";

    private void Start()
    {
        Application.logMessageReceived += HandleLog;
        //Debug.Log("Log enabled");
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString + "\n" + output;
        stack = stackTrace;
    }

    private void OnGUI()
    {
        debugText.text = output + "\n";
    }

    public void ClearLog()
    {
        output = "";
    }
}
