using UnityEngine;
using UnityEngine.UI;

public class ComputeModeSwitcher : MonoBehaviour
{
    public Toggle cpuToggle;
    public Toggle gpuToggle;

    public GameObject cpuOnlyPanel;
    public GameObject gpuOnlyPanel;

    void Start()
    {
        cpuToggle.onValueChanged.AddListener(OnCPUToggle);
        gpuToggle.onValueChanged.AddListener(OnGPUToggle);

        // Initialize correct panel on startup
        if (cpuToggle.isOn)
        {
            OnCPUToggle(true);
        }
        else
        {
            OnGPUToggle(true);
        }
    }

    void OnCPUToggle(bool isOn)
    {
        if (isOn)
        {
            cpuOnlyPanel.SetActive(true);
            gpuOnlyPanel.SetActive(false);
        }
    }

    void OnGPUToggle(bool isOn)
    {
        if (isOn)
        {
            gpuOnlyPanel.SetActive(true);
            cpuOnlyPanel.SetActive(false);
        }
    }
}
