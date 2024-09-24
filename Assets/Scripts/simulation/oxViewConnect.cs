using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using WebSocketSharp;
using static GlobalVariables;

public class oxViewConnect : MonoBehaviour
{
    [SerializeField]
    private const string _connectionURL = "wss://nanobase.org:8989/";
    private WebSocket _ws;
    private JObject _settings;
    private SynchronizationContext _unityContext;

    private void Awake()
    {
        // Store the main thread's synchronization context for use later
        _unityContext = SynchronizationContext.Current;
    }

    public void Connect(JObject settings)
    {
        _settings = settings;

        _ws = new WebSocket(_connectionURL);
        _ws.OnOpen += SendOrigami;
        _ws.OnOpen += (sender, e) =>
        {
            _unityContext.Post(_ =>
            {
                Debug.Log("Connected");
            }, null);
        };
        _ws.OnError += (sender, e) =>
        {
            _unityContext.Post(_ =>
            {
                Debug.Log("Connection Error " + e.Message);
            }, null);
            
        };
        _ws.OnClose += (sender, e) =>
        {
            _unityContext.Post(_ =>
            {
                Debug.Log("Reason " + e.Reason);
                Debug.Log("Error code " + e.Code);
            }, null);
        };
        _ws.OnMessage += SimulationUpdate;

        _ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        _ws.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        _ws.Connect();
    }

    private void SendOrigami(object sender, EventArgs e)
    {
        if (_settings == null)
        {
            throw new ArgumentException("Missing simulation settings");
        }

        string datFile = oxView.DatFile;
        string topFile = oxView.TopFile;

        // Hard code settings for now.
        JObject initialMessage = new JObject(
            new JProperty("top_file", topFile),
            new JProperty("dat_file", datFile),
            new JProperty("settings", _settings)
        );

        string message = initialMessage.ToString();

        _ws.Send(message);
    }

    private void SimulationUpdate(object sender, MessageEventArgs e)
    {
        _unityContext.Post(_ =>
        {
            Debug.Log(e.Data.Substring(0, 50));
        }, null);

        JObject message = JObject.Parse(e.Data);

        string datFile = message["dat_file"].ToString();

        // Use the synchronization context to ensure the SimulationUpdate runs on the main Unity thread
        // This is to ensure that unity API calls are not done outside of unity's sync context.
        _unityContext.Post(_ =>
        {
            oxView.SimulationUpdate(datFile);
        }, null);
    }
}
