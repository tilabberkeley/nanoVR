/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using UnityEngine;
using TMPro;

public class ConsoleToText : MonoBehaviour
{
    public TextMeshProUGUI debugText;
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
        debugText.text = "Stack: " + "\n" + stack + "\n"
                       + "============================================" + "\n"
                       + "Log: " + "\n" + output;
    }

    public void ClearLog()
    {
        output = "";
    }
}
