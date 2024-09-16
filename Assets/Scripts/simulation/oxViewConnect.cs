using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using WebSocketSharp;
using static GlobalVariables;

public class oxViewConnect : MonoBehaviour
{
    [SerializeField]
    private string _connectionURL = "wss://nanobase.org:8989/";
    private WebSocket _ws;
    private int messageCount;

    public void Connect()
    {
        _ws = new WebSocket(_connectionURL);
        _ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected");
        };
        _ws.OnOpen += SendOrigami;
        _ws.OnMessage += (sender, e) =>
        {
            Debug.Log(e.Data.Substring(0, 20));
        };
        _ws.OnError += (sender, e) =>
        {
            Debug.Log(e.Message);
        };
        Debug.Log("Connecting");
        _ws.Connect();
    }

    IEnumerator SendMessageEverySecond()
    {
        while (true)
        {
            _ws.Send(BitConverter.GetBytes(messageCount++));
            yield return new WaitForSeconds(1);
        }
    }

    private void SendOrigami(object sender, EventArgs e)
    {
        string datFile = oxView.DatFile;
        string topFile = oxView.TopFile;

        // Hard code settings for now.
        JObject initialMessage = new JObject(
            new JProperty("top_file", topFile),
            new JProperty("dat_file", datFile),
            new JProperty("settings", new JObject(
                new JProperty("T", "20C"),
                new JProperty("steps", "1000000"),
                new JProperty("salt_concentration", "1"),
                new JProperty("interaction_type", "DNA2"),
                new JProperty("print_conf_interval", "10000"),
                new JProperty("print_energy_every", "10000"),
                new JProperty("thermostat", "brownian"),
                new JProperty("dt", "0.003"),
                new JProperty("diff_coeff", "2.5"),
                new JProperty("max_density_multiplier", "10"),
                new JProperty("sim_type", "MD"),
                new JProperty("T_units", "C"),
                new JProperty("backend", "CUDA"),
                new JProperty("backend_precision", "mixed"),
                new JProperty("time_scale", "linear"),
                new JProperty("verlet_skin", 0.5),
                new JProperty("use_average_seq", 0),
                new JProperty("refresh_vel", 1),
                new JProperty("CUDA_list", "verlet"),
                new JProperty("restart_step_counter", 1),
                new JProperty("newtonian_steps", 103),
                new JProperty("CUDA_sort_every", 0),
                new JProperty("use_edge", 1),
                new JProperty("edge_n_forces", 1),
                new JProperty("cells_auto_optimisation", "true"),
                new JProperty("reset_com_momentum", "true"),
                new JProperty("max_backbone_force", "5"),
                new JProperty("max_backbone_force_far", "10")
            ))
        );

        _ws.Send(initialMessage.ToString());
    }
}
