using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class ConsoleToText : MonoBehaviour
{
    [Tooltip("UI Text component to display debug messages.")]
    public TextMeshProUGUI debugText;

    [Tooltip("Time interval (in seconds) between UI updates.")]
    public float updateInterval = 0.5f;

    [Tooltip("Maximum number of log entries to retain.")]
    public int maxLogCount = 500;

    private Queue<string> logQueue = new Queue<string>();
    private string latestStackTrace = "";
    private float timeSinceLastUpdate = 0f;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    // This method captures log messages as they come in.
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Add new log entry.
        logQueue.Enqueue(logString);

        // Ensure we don't retain more than maxLogCount entries.
        if (logQueue.Count > maxLogCount)
        {
            logQueue.Dequeue();
        }

        // Always update the latest stack trace.
        latestStackTrace = stackTrace;
    }

    private void Update()
    {
        // Update on a fixed interval to reduce performance overhead.
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdateDebugText();
            timeSinceLastUpdate = 0f;
        }
    }

    // This method rebuilds the text string and assigns it to the UI element.
    private void UpdateDebugText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Stack:");
        sb.AppendLine(latestStackTrace);
        sb.AppendLine("====================================");
        sb.AppendLine("Logs:");

        // Append each log entry
        foreach (string log in logQueue)
        {
            sb.AppendLine(log);
        }

        debugText.text = sb.ToString();
    }

    /// <summary>
    /// Clears the current logs.
    /// </summary>
    public void ClearLog()
    {
        logQueue.Clear();
        latestStackTrace = "";
        debugText.text = "";
    }
}
