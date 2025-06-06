using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

public class OxViewConnect : MonoBehaviour
{
    [SerializeField]
    private const string _connectionURL = "wss://nanobase.org:8989/";

    private WebSocket _ws;
    private JObject _settings;
    private SynchronizationContext _unityContext;
    private OxDNAMapper _oxDNAMapper;

    private void Awake()
    {
        // Store the main thread's synchronization context for use later
        _unityContext = SynchronizationContext.Current;
    }

    public void Connect(JObject settings)
    {
        _settings = settings;

        Utils.ShowHideAllHelix(show: false);

        _ws = new WebSocket(_connectionURL, "json")
        {
            Origin = "https://sulcgroup.github.io"
        };

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
        OxView oxView = GlobalVariables.s_oxViewDict.Values.First();

        _unityContext.Post(_ =>
        {
            oxView.RestoreNucleotides();
            oxView.Clear();
            GlobalVariables.s_oxViewDict.Remove(oxView.OxViewId);
            Utils.ShowHideAllHelix(show: true);
        }, null);
    }

    private void SendOrigami(object sender, EventArgs e)
    {
        if (_settings == null)
        {
            throw new ArgumentException("Missing simulation settings");
        }
        if (GlobalVariables.s_oxViewDict.Count != 1)
        {
            throw new Exception("Need to simulate one structure at a time.");
        }

        // Get file contents and mappings
        OxDNASystem oxDNAsystem = new OxDNASystem();
        var fileResults = oxDNAsystem.OxDNAFiles();
        _oxDNAMapper = fileResults.oxDNAMapper;
        //_oxDNAMapper.SaveNucleotidePositions();

        OxView oxView = GlobalVariables.s_oxViewDict.Values.First();

        JObject initialMessage = new JObject(
            //new JProperty("dat_file", fileResults.datFile),
            new JProperty("dat_file", oxView.DatFile),
            new JProperty("settings", _settings),
            //new JProperty("top_file", fileResults.topFile)
            new JProperty("top_file", oxView.TopFile)
        );

        string message = initialMessage.ToString();
        //Debug.Log("Final WebSocket message:\n" + initialMessage.ToString(Newtonsoft.Json.Formatting.Indented));
        _ws.Send(message);
        Debug.Log("Origami sent!");
    }

    private void SimulationUpdate(object sender, MessageEventArgs e)
    {
        Debug.Log("Received message");
        JObject message = JObject.Parse(e.Data);

        string datFile = message["dat_file"].ToString();
        OxView oxView = GlobalVariables.s_oxViewDict.Values.First();

        // Use the synchronization context to ensure the SimulationUpdate runs on the main Unity thread
        // This is to ensure that unity API calls are not done outside of unity's sync context.
        Debug.Log("Updating simulation");
        _unityContext.Post(_ =>
        {
            oxView.SimulationUpdate(datFile);
        }, null);
        Debug.Log("Simulation updated!");
    }
}
