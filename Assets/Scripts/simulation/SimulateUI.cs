using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GlobalVariables;

public class SimulateUI : MonoBehaviour
{
    /* Main menu UI */
    [SerializeField] private Canvas _menu;

    [SerializeField] private Button _menuSimulateButton;

    /* Simulation UI */
    [SerializeField] private Canvas _simulatePanel;

    [SerializeField] private Button _OKButton;
    [SerializeField] private Button _cancelButton;

    // Input fields for simulation
    private const string T_DEFAULT = "20";
    [SerializeField] private TMP_InputField _TInput;
    private string _t;

    private const string STEPS_DEFAULT = "1000000";
    [SerializeField] private TMP_InputField _stepsInput;
    private string _steps;

    private const string SALT_DEFAULT = "1";
    [SerializeField] private TMP_InputField _saltInput;
    private string _salt;

    [SerializeField] private Dropdown _interactionTypeDropdown;
    private string _interactionType;

    private const string PRINT_CONF_INTERVAL = "10000";
    [SerializeField] private TMP_InputField _printConfIntervalInput;
    private string _printConfInterval;

    private const string PRINT_ENERGY_INTERVAL = "10000";
    [SerializeField] private TMP_InputField _printEnergyIntervalInput;
    private string _printEnergy;

    [SerializeField] private Dropdown _thermostatDropdown;
    private string _thermostat;

    private const string DT_DEFAULT = "0.003";
    [SerializeField] private TMP_InputField _dtInput;
    private string _dt;

    private const string DIFF_COEFF_DEFAULT = "2.5";
    [SerializeField] private TMP_InputField _diffCoeffInput;
    private string _diffCoeff;

    private const string MAX_DENSITY_MULTIPLIER_DEFAULT = "10.0";
    [SerializeField] private TMP_InputField _maxDensityMultiplierInput;
    private string _maxDensityMultiplier;

    [SerializeField] private Toggle _relaxSettingsInput;
    private string _relaxSettings;

    private const string BACKBONE_FORCE_DEFAULT = "5.0";
    [SerializeField] private TMP_InputField _backboneForceInput;
    private string _backboneForce;

    private const string BACKBONE_FORCE_FAR_DEFAULT = "10.0";
    [SerializeField] private TMP_InputField _backboneForceFarInput;
    private string _backboneForceFar;

    /* Connect manager */
    [SerializeField] private GameObject _oxserveConnectionManager;
    private oxViewConnect _oxViewConnect;

    // Start is called before the first frame update
    void Start()
    {
        _simulatePanel.enabled = false;
        _oxViewConnect = _oxserveConnectionManager.GetComponent<oxViewConnect>();

        // Add button listeners


        // Fill default values for input fields


        // Add input field listeners
    }

    private void ShowSimulationUI(bool on)
    {
        _simulatePanel.enabled = on;
        _menu.enabled = !on;
    }

    private JObject ParseSettings()
    {
        return new JObject(
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
        );
    }

    public void Simulate()
    {
        _oxViewConnect.Connect(ParseSettings());
    }
}
