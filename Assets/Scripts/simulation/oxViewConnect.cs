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
    private OxView _oxView;

    private void Awake()
    {
        // Store the main thread's synchronization context for use later
        _unityContext = SynchronizationContext.Current;
    }

    public void Connect(JObject settings, OxView oxview)
    {
        _settings = settings;
        _oxView = oxview;

        if (_ws != null && _ws.IsAlive)
        {
            // Connection is still alive, just send settings.
            // Only way to get to this path if a previous simulation was aborted.
            SendOrigami();
            return;
        }

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

    /// <summary>
    /// Closes the websocket connection with oxview and restores scene back to edit mode
    /// if the connection is open.
    /// </summary>
    public void Disconnect()
    {
        if (_ws != null && _ws.IsAlive)
        {
            _ws.Close();

            // Use the synchronization context to ensure the Disconnect runs on the main Unity thread
            // This is to ensure that unity API calls are not done outside of unity's sync context.
            _unityContext.Post(_ =>
            {
                OxView oxView = GlobalVariables.s_oxViewDict.Values.First();
                oxView.RestoreNucleotides();
                oxView.Clear();
                GlobalVariables.s_oxViewDict.Remove(oxView.OxViewId);
                Utils.ShowHideAllHelix(show: true);
            }, null);
        }
    }

    /// <summary>
    /// Tells oxview to stop sending the simulation updates.
    /// </summary>
    public void AbortCurrentSimulation()
    {
        // This tells nanobase to stop sending the simulation.
        // The connection still remains up.
        
        _ws.Send("abort");
    }

    private void SendOrigami()
    {
        if (_settings == null)
        {
            throw new ArgumentException("Missing simulation settings");
        }
        if (GlobalVariables.s_oxViewDict.Count != 1)
        {
            throw new Exception("Need to simulate one structure at a time.");
        }

        JObject initialMessage = new JObject(
            //new JProperty("dat_file", fileResults.datFile),
            new JProperty("dat_file", _oxView.DatFile),
            new JProperty("settings", _settings),
            //new JProperty("top_file", fileResults.topFile)
            new JProperty("top_file", _oxView.TopFile)
        );

        string message = initialMessage.ToString();
        _ws.Send(message);
    }

    private void SendOrigami(object sender, EventArgs e)
    {
        SendOrigami();
    }

    private void SimulationUpdate(object sender, MessageEventArgs e)
    {
        JObject message = JObject.Parse(e.Data);

        string datFile = message["dat_file"].ToString();

        // Use the synchronization context to ensure the SimulationUpdate runs on the main Unity thread
        // This is to ensure that unity API calls are not done outside of unity's sync context.
        _unityContext.Post(_ =>
        {
            if (!GlobalVariables.s_simulationRunning)
            {
                // Don't do a simulation update if we aborted the simulation
                return;
            }

            OxView oxView = GlobalVariables.s_oxViewDict.Values.First();
            oxView.SimulationUpdate(datFile);
            Debug.Log("Simulation updated!");
        }, null);
    }
}
