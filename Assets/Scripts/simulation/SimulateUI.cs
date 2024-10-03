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
    [SerializeField] private Canvas _simulateCanvas;

    [SerializeField] private Button _simulateButton;
    [SerializeField] private Button _cancelButton;

    // Input fields for simulation
    private const string T_DEFAULT = "20";
    [SerializeField] private TMP_InputField _TInput;

    private const string STEPS_DEFAULT = "1000000";
    [SerializeField] private TMP_InputField _stepsInput;

    private const string SALT_DEFAULT = "1";
    [SerializeField] private TMP_InputField _saltInput;

    [SerializeField] private TMP_Dropdown _interactionTypeDropdown;

    private const string PRINT_CONF_INTERVAL_DEFAULT = "10000";
    [SerializeField] private TMP_InputField _printConfIntervalInput;

    private const string PRINT_ENERGY_INTERVAL_DEFAULT = "10000";
    [SerializeField] private TMP_InputField _printEnergyIntervalInput;

    [SerializeField] private TMP_Dropdown _thermostatDropdown;

    private const string DT_DEFAULT = "0.003";
    [SerializeField] private TMP_InputField _dtInput;

    private const string DIFF_COEFF_DEFAULT = "2.5";
    [SerializeField] private TMP_InputField _diffCoeffInput;

    private const string MAX_DENSITY_MULTIPLIER_DEFAULT = "10.0";
    [SerializeField] private TMP_InputField _maxDensityMultiplierInput;

    // TODO: Add compatability to toggle relax settings. Need to remove backbone force fields from the JSON.
    [SerializeField] private Toggle _relaxSettingsInput;

    private const string BACKBONE_FORCE_DEFAULT = "5.0";
    [SerializeField] private TMP_InputField _backboneForceInput;

    private const string BACKBONE_FORCE_FAR_DEFAULT = "10.0";
    [SerializeField] private TMP_InputField _backboneForceFarInput;

    /* Connect manager */
    [SerializeField] private GameObject _oxserveConnectionManager;
    private oxViewConnect _oxViewConnect;

    void Start()
    {
        _simulateCanvas.enabled = false;
        _oxViewConnect = _oxserveConnectionManager.GetComponent<oxViewConnect>();

        // Add button listeners
        _menuSimulateButton.onClick.AddListener(() => ShowSimulationUI());
        _simulateButton.onClick.AddListener(() => Simulate());
        _cancelButton.onClick.AddListener(() => Cancel());

        // Fill default values for input fields
        _TInput.text = T_DEFAULT;
        _stepsInput.text = STEPS_DEFAULT;
        _saltInput.text = SALT_DEFAULT;
        _printConfIntervalInput.text = PRINT_CONF_INTERVAL_DEFAULT;
        _printEnergyIntervalInput.text = PRINT_ENERGY_INTERVAL_DEFAULT;
        _dtInput.text = DT_DEFAULT;
        _diffCoeffInput.text = DIFF_COEFF_DEFAULT;
        _maxDensityMultiplierInput.text = MAX_DENSITY_MULTIPLIER_DEFAULT;
        _backboneForceInput.text = BACKBONE_FORCE_DEFAULT;
        _backboneForceFarInput.text = BACKBONE_FORCE_DEFAULT;

        // Add input field listeners
        _TInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _stepsInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _saltInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _printConfIntervalInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _printEnergyIntervalInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _dtInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _diffCoeffInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _maxDensityMultiplierInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _backboneForceInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _backboneForceFarInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
    }

    private void ShowSimulationUI()
    {
        _simulateCanvas.enabled = true;
        _menu.enabled = false;
    }

    private void Cancel()
    {
        _simulateCanvas.enabled = false;

        // TODO: break oxserve connection.
    }

    private JObject ParseSettings()
    {
        // UI copied form oxview, so other settings are hardcoded as they are in oxview.
        return new JObject(
            new JProperty("T", _TInput.text + "C"),
            new JProperty("steps", _stepsInput.text),
            new JProperty("salt_concentration", _saltInput.text),
            new JProperty("interaction_type", GetInteractionType()),
            new JProperty("print_conf_interval", _printConfIntervalInput.text),
            new JProperty("print_energy_every", _printEnergyIntervalInput.text),
            new JProperty("thermostat", GetThermostatSetting()),
            new JProperty("dt", _dtInput.text),
            new JProperty("diff_coeff", _diffCoeffInput.text),
            new JProperty("max_density_multiplier", _maxDensityMultiplierInput.text),
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
            new JProperty("max_backbone_force", _backboneForceInput.text),
            new JProperty("max_backbone_force_far", _backboneForceFarInput.text)
        );
    }

    private void Simulate()
    {
        _simulateCanvas.enabled = false;

        _oxViewConnect.Connect(ParseSettings());
    }

    private string GetThermostatSetting()
    {
        switch (_thermostatDropdown.options[_thermostatDropdown.value].text)
        {
            case "Brownian":
                return "brownian";
            case "Bussi-Donadio-Parrinello":
                return "bussi";
            case "Langevin":
                return "langevin";
            case "No":
                return "no";
            default:
                return "brownian";
        }
    }

    private string GetInteractionType()
    {
        switch (_interactionTypeDropdown.options[_interactionTypeDropdown.value].text)
        {
            case "DNA":
                return "DNA2";
            case "RNA":
                return "RNA2";
            case "DNA ANM":
                return "DNANM";
            case "RNA ANM":
                return "RNANM";
            default:
                return "DNA2";
        }
    }
}
