
using UnityEngine;
using UnityEngine.UI;

public class ConsoleToText : MonoBehaviour
{
    public Text debugText;
    string output = "";
    string stack = "";

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        Debug.Log("Log enabled");
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        ClearLog();
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString + "\n" + output;
        stack = stackTrace;
    }

    private void OnGUI()
    {
        debugText.text = output;
    }

    public void ClearLog()
    {
        output = "";
    }
}
