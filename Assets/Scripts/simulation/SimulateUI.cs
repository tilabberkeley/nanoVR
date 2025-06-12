using System.Linq;
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
    [SerializeField] private TextMeshProUGUI _simulateButtonText;
    [SerializeField] private Button _cancelButton;

    /* Shared input fields between compute modes */
    private const string T_DEFAULT_CPU = "30";
    private const string T_DEFAULT_GPU = "20";
    [SerializeField] private TMP_InputField _TInput;
    private string _TCPU = T_DEFAULT_CPU;
    private string _TGPU = T_DEFAULT_GPU;

    private const string STEPS_DEFAULT_CPU = "100000";
    private const string STEPS_DEFAULT_GPU = "1000000";
    [SerializeField] private TMP_InputField _stepsInput;
    private string _stepsCPU = STEPS_DEFAULT_CPU;
    private string _stepsGPU = STEPS_DEFAULT_GPU;

    private const string SALT_DEFAULT = "1";
    [SerializeField] private TMP_InputField _saltInput;
    private string _saltCPU = SALT_DEFAULT;
    private string _saltGPU = SALT_DEFAULT;

    private const int INTERACTION_TYPE_DEFAULT = 0; // Associated with DNA
    [SerializeField] private TMP_Dropdown _interactionTypeDropdown;
    private int _interactionTypeCPU = INTERACTION_TYPE_DEFAULT;
    private int _interactionTypeGPU = INTERACTION_TYPE_DEFAULT;

    private const string PRINT_CONF_INTERVAL_DEFAULT_CPU = "1000";
    private const string PRINT_CONF_INTERVAL_DEFAULT_GPU = "10000";
    [SerializeField] private TMP_InputField _printConfIntervalInput;
    private string _printConfIntervalCPU = PRINT_CONF_INTERVAL_DEFAULT_CPU;
    private string _printConfIntervalGPU = PRINT_CONF_INTERVAL_DEFAULT_GPU;

    private const string PRINT_ENERGY_INTERVAL_DEFAULT_CPU = "1000";
    private const string PRINT_ENERGY_INTERVAL_DEFAULT_GPU = "10000";
    [SerializeField] private TMP_InputField _printEnergyIntervalInput;
    private string _printEnergyIntervalCPU = PRINT_ENERGY_INTERVAL_DEFAULT_CPU;
    private string _printEnergyIntervalGPU = PRINT_ENERGY_INTERVAL_DEFAULT_GPU;

    /* Compute mode */
    [SerializeField] private Toggle _cpuToggle;
    [SerializeField] private Toggle _gpuToggle;

    [SerializeField] private GameObject _cpuOnlyPanel;
    [SerializeField] private GameObject _gpuOnlyPanel;

    /* Input fields for CPU compute mode */
    private const string DELTA_TRANSLATION_DEFAULT = "0.22";
    [SerializeField] private TMP_InputField _deltaTranslationInput;

    private const string DELTA_ROTATION_DEFAULT = "0.22";
    [SerializeField] private TMP_InputField _deltaRotationInput;

    /* Input fields for GPU compute mode */
    [SerializeField] private TMP_Dropdown _thermostatDropdown;

    private const string DT_DEFAULT = "0.003";
    [SerializeField] private TMP_InputField _dtInput;

    private const string DIFF_COEFF_DEFAULT = "2.5";
    [SerializeField] private TMP_InputField _diffCoeffInput;

    private const string MAX_DENSITY_MULTIPLIER_DEFAULT = "10.0";
    [SerializeField] private TMP_InputField _maxDensityMultiplierInput;

    /* Relax settings - shared between compute modes */
    // TODO: Add compatability to toggle relax settings. Need to remove backbone force fields from the JSON.
    [SerializeField] private Toggle _relaxSettingsInput;

    private const string BACKBONE_FORCE_DEFAULT = "5.0";
    [SerializeField] private TMP_InputField _backboneForceInput;
    private string _backboneForceCPU = BACKBONE_FORCE_DEFAULT;
    private string _backboneForceGPU = BACKBONE_FORCE_DEFAULT;

    private const string BACKBONE_FORCE_FAR_DEFAULT = "10.0";
    [SerializeField] private TMP_InputField _backboneForceFarInput;
    private string _backboneForceFarCPU = BACKBONE_FORCE_FAR_DEFAULT;
    private string _backboneForceFarGPU = BACKBONE_FORCE_FAR_DEFAULT;

    /* Connect manager */
    [SerializeField] private GameObject _oxserveConnectionManager;
    private oxViewConnect _oxViewConnect;

    /* Current oxview structure for simulation */
    private OxView _oxView;

    /* Components for managing views */
    [SerializeField] private Togglers _togglers;
    [SerializeField] private ViewChanger _viewChanger;

    /* Text to update for simulation summary messages */
    [SerializeField] private TextMeshProUGUI _simulationSummaryMessage;

    void Start()
    {
        _simulateCanvas.enabled = false;
        _oxViewConnect = _oxserveConnectionManager.GetComponent<oxViewConnect>();

        /* Add button listeners */
        _menuSimulateButton.onClick.AddListener(() => ShowSimulationUI());
        _simulateButton.onClick.AddListener(() => ToggleSimulate());
        _cancelButton.onClick.AddListener(() => Cancel());

        _backboneForceInput.text = _backboneForceCPU;
        _backboneForceFarInput.text = _backboneForceFarCPU;

        /* Fill in default values for non-shared inputs between compute modes */
        // CPU inputs
        _deltaTranslationInput.text = DELTA_TRANSLATION_DEFAULT;
        _deltaRotationInput.text = DELTA_ROTATION_DEFAULT;

        // GPU inputs
        _dtInput.text = DT_DEFAULT;
        _diffCoeffInput.text = DIFF_COEFF_DEFAULT;
        _maxDensityMultiplierInput.text = MAX_DENSITY_MULTIPLIER_DEFAULT;

        /* Add input field listeners */
        _TInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _stepsInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _saltInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _printConfIntervalInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _printEnergyIntervalInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });

        _deltaTranslationInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _deltaRotationInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });

        _dtInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _diffCoeffInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _maxDensityMultiplierInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });

        _backboneForceInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });
        _backboneForceFarInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); });

        /* Set up compute mode toggles */
        _cpuToggle.onValueChanged.AddListener(OnCPUToggle);
        _gpuToggle.onValueChanged.AddListener(OnGPUToggle);

        // Initialize correct panel on startup
        if (_cpuToggle.isOn)
        {
            OnCPUToggle(true);
        }
        else
        {
            OnGPUToggle(true);
        }
    }

    private void ShowSimulationUI()
    {
        _simulateCanvas.enabled = true;
        _menu.enabled = false;
    }

    /// <summary>
    /// Parsing settings based on the given compute mode.
    /// </summary>
    private JObject ParseSettings(bool useCPU)
    {
        var settings = new JObject(
            new JProperty("T", _TInput.text + "C"),
            new JProperty("steps", _stepsInput.text),
            new JProperty("salt_concentration", _saltInput.text),
            new JProperty("interaction_type", GetInteractionType()),
            new JProperty("print_conf_interval", _printConfIntervalInput.text),
            new JProperty("print_energy_every", _printEnergyIntervalInput.text),
            new JProperty("sim_type", useCPU ? "MC" : "MD"),
            new JProperty("backend", useCPU ? "CPU" : "CUDA"),
            new JProperty("backend_precision", useCPU ? "double" : "mixed"),
            new JProperty("time_scale", "linear"),
            new JProperty("verlet_skin", useCPU ? 1 : 0.5),
            new JProperty("use_average_seq", 0),
            new JProperty("restart_step_counter", 1),
            new JProperty("max_backbone_force", _backboneForceInput.text),
            new JProperty("max_backbone_force_far", _backboneForceFarInput.text)
        );

        if (useCPU)
        {
            settings["delta_translation"] = _deltaTranslationInput.text;
            settings["delta_rotation"] = _deltaRotationInput.text;
            settings["ensemble"] = "NVT";
        }
        else
        {
            settings["thermostat"] = GetThermostatSetting();
            settings["dt"] = _dtInput.text;
            settings["diff_coeff"] = _diffCoeffInput.text;
            settings["max_density_multiplier"] = _maxDensityMultiplierInput.text;
            settings["T_units"] = "C";
            settings["refresh_vel"] = 1;
            settings["CUDA_list"] = "verlet";
            settings["newtonian_steps"] = 103;
            settings["CUDA_sort_every"] = 0;
            settings["use_edge"] = 1;
            settings["edge_n_forces"] = 1;
            settings["cells_auto_optimisation"] = "true";
            settings["reset_com_momentum"] = "true";
        }

        return settings;
    }

    private void Cancel()
    {
        s_simulating = false;
        s_simulationRunning = false;

        _simulateCanvas.enabled = false;

        _togglers.StencilsToggled();
        _viewChanger.ChangeStencilsView();

        _oxViewConnect.Disconnect();
    }

    private void ToggleSimulate()
    {
        s_simulationRunning = !s_simulationRunning;

        if (s_simulationRunning)
        {
            if (!s_simulating)
            {
                // First time pressing simulate
                // Before connecting, export the current structure to oxview and reimport it.
                OxDNASystem oxDNASystem = new OxDNASystem();
                var oxViewFile = oxDNASystem.OxViewFile();

                FileImport.OxViewImport(oxViewFile);
                _oxView = s_oxViewDict.Values.First();

                _oxViewConnect.Connect(ParseSettings(_cpuToggle.isOn), _oxView);

                // We are now in simulation mode.
                s_simulating = true;

                _togglers.StencilsToggled();
                _viewChanger.ChangeStencilsView();
            }
            else
            {
                // Redoing the simulation, use the same oxview structure
                _oxViewConnect.Connect(ParseSettings(_cpuToggle.isOn), _oxView);
            }
        }
        else
        {
            _oxViewConnect.AbortCurrentSimulation();
        }

        _simulateCanvas.enabled = false;

        SetSimulateButton(s_simulationRunning);
    }

    private void SetSimulateButton(bool simulating)
    {
        if (_menuSimulateButton != null && _simulateButtonText != null)
        {
            if (simulating)
            {
                _simulateButtonText.text = "Abort";
            }
            else
            {
                _simulateButtonText.text = "Simulate";
            }
        }
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

    void OnCPUToggle(bool isOn)
    {
        if (isOn)
        {
            // Store GPU values (UI → *_GPU)
            // Null check is for first situation where input fields are empty by default.
            _TGPU = string.IsNullOrWhiteSpace(_TInput.text) ? T_DEFAULT_GPU : _TInput.text;
            _stepsGPU = string.IsNullOrWhiteSpace(_stepsInput.text) ? STEPS_DEFAULT_GPU : _stepsInput.text;
            _saltGPU = string.IsNullOrWhiteSpace(_saltInput.text) ? SALT_DEFAULT : _saltInput.text;
            _interactionTypeGPU = _interactionTypeDropdown.value;
            _printConfIntervalGPU = string.IsNullOrWhiteSpace(_printConfIntervalInput.text) ? PRINT_CONF_INTERVAL_DEFAULT_GPU : _printConfIntervalInput.text;
            _printEnergyIntervalGPU = string.IsNullOrWhiteSpace(_printEnergyIntervalInput.text) ? PRINT_ENERGY_INTERVAL_DEFAULT_GPU : _printEnergyIntervalInput.text;

            _backboneForceGPU = _backboneForceInput.text;
            _backboneForceFarGPU = _backboneForceFarInput.text;

            // Restore CPU values (saved earlier in *_CPU → UI)
            _TInput.text = _TCPU;
            _stepsInput.text = _stepsCPU;
            _saltInput.text = _saltCPU;
            _interactionTypeDropdown.value = _interactionTypeCPU;
            _printConfIntervalInput.text = _printConfIntervalCPU;
            _printEnergyIntervalInput.text = _printEnergyIntervalCPU;

            _backboneForceInput.text = _backboneForceCPU;
            _backboneForceFarInput.text = _backboneForceFarCPU;

            _cpuOnlyPanel.SetActive(true);
            _gpuOnlyPanel.SetActive(false);
        }
    }

    void OnGPUToggle(bool isOn)
    {
        if (isOn)
        {
            // Store CPU values (current UI → *_CPU)
            _TCPU = _TInput.text;
            _stepsCPU = _stepsInput.text;
            _saltCPU = _saltInput.text;
            _interactionTypeCPU = _interactionTypeDropdown.value;
            _printConfIntervalCPU = _printConfIntervalInput.text;
            _printEnergyIntervalCPU = _printEnergyIntervalInput.text;

            _backboneForceCPU = _backboneForceInput.text;
            _backboneForceFarCPU = _backboneForceFarInput.text;

            // Restore GPU values (saved earlier in *_GPU → UI)
            _TInput.text = _TGPU;
            _stepsInput.text = _stepsGPU;
            _saltInput.text = _saltGPU;
            _interactionTypeDropdown.value = _interactionTypeGPU;
            _printConfIntervalInput.text = _printConfIntervalGPU;
            _printEnergyIntervalInput.text = _printEnergyIntervalGPU;

            _backboneForceInput.text = _backboneForceGPU;
            _backboneForceFarInput.text = _backboneForceFarGPU;

            _gpuOnlyPanel.SetActive(true);
            _cpuOnlyPanel.SetActive(false);
        }
    }

    public void SetSimulationSummaryMessage(string simulationSummaryMessage)
    {
        _simulationSummaryMessage.text = simulationSummaryMessage;
    }
}
