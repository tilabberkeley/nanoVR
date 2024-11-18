using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class OxViewConnect : MonoBehaviour
{
    [SerializeField]
    private const string _connectionURL = "wss://nanobase.org:8989/";

    [SerializeField]
    private GameObject _fileExportObject;
    private FileExport _fileExport;

    private WebSocket _ws;
    private JObject _settings;
    private SynchronizationContext _unityContext;
    private OxDNAMapper _oxDNAMapper;

    private void Awake()
    {
        // Store the main thread's synchronization context for use later
        _unityContext = SynchronizationContext.Current;
        _fileExport = _fileExportObject.GetComponent<FileExport>();
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
                Debug.Log("oxDNA Connected");
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
                Debug.Log("oxDNA Disconnected");
                Debug.Log("Reason " + e.Reason);
                Debug.Log("Error code " + e.Code);
            }, null);
        };
        _ws.OnMessage += SimulationUpdate;

        _ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        _ws.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        _ws.Connect();
    }

    public void Disconnect()
    {
        _ws.Close();

        // Use the synchronization context to ensure the SimulationUpdate runs on the main Unity thread
        // This is to ensure that unity API calls are not done outside of unity's sync context.
        _unityContext.Post(_ =>
        {
            _oxDNAMapper.RestoreNucleotidesToEdit();
        }, null);
    }

    private void SendOrigami(object sender, EventArgs e)
    {
        if (_settings == null)
        {
            throw new ArgumentException("Missing simulation settings");
        }

        // Get file contents and mappings
        _fileExport.GenerateOxDNAFiles(out string topFile, out string datFile, out _oxDNAMapper);

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
        JObject message = JObject.Parse(e.Data);

        string datFile = message["dat_file"].ToString();

        // Use the synchronization context to ensure the SimulationUpdate runs on the main Unity thread
        // This is to ensure that unity API calls are not done outside of unity's sync context.
        _unityContext.Post(_ =>
        {
            _oxDNAMapper.SimulationUpdate(datFile);
        }, null);
    }
}
