using System;
using System.Collections;
using UnityEngine;
using WebSocketSharp;

public class oxViewConnect : MonoBehaviour
{
    WebSocket ws;
    int messageCount;

    public void Connect()
    {
        ws = new WebSocket("ws://localhost:8080");
        ws.Connect();
        StartCoroutine(SendMessageEverySecond());
    }

    IEnumerator SendMessageEverySecond()
    {
        while (true)
        {
            ws.Send(BitConverter.GetBytes(messageCount++));
            yield return new WaitForSeconds(1);
        }
    }
}
