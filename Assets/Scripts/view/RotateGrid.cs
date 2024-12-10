/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rotates Gizmos.
/// </summary>
public class RotateGrid : MonoBehaviour
{
    private List<DNAGrid> grids = new List<DNAGrid>();
    [SerializeField] private TMP_InputField pitchInput;
    [SerializeField] private TMP_InputField rollInput;
    [SerializeField] private TMP_InputField yawInput;
    [SerializeField] private Canvas rotateMenu;
    [SerializeField] private Canvas menu;

    [SerializeField] private Button _OKButton;
    [SerializeField] private Button _cancelButton;

    private void Awake()
    {
        //pitchInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.DecimalPad); });
        //rollInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.DecimalPad); });
        //yawInput.onSelect.AddListener(delegate { TouchScreenKeyboard.Open("", TouchScreenKeyboardType.DecimalPad); });

        rotateMenu.enabled = false;
        _OKButton.onClick.AddListener(() => Cancel());
        _OKButton.onClick.AddListener(() => RotateGrids());
        _cancelButton.onClick.AddListener(() => Cancel());
    }

    public void ShowMenu() 
    {
        rotateMenu.enabled = true;
        menu.enabled = false;
    }

    /// <summary>
    /// Called by RotateGrids button in scene.
    /// </summary>
    public void RotateGrids()
    {
        Debug.Log("RotateGrids OK clicked");
        grids = SelectGrid.Grids;
        //float pitch = GetInput(pitchInput);
        //float roll = GetInput(rollInput);
        //float yaw = GetInput(yawInput);
        float pitch = 90f;
        float roll = 0f;
        float yaw = 0f; // TESTING PURPOSES
        Debug.Log($"Rotating {grids.Count} grids");
        foreach (DNAGrid grid in grids)
        {
            grid.Rotate(pitch, roll, yaw);
        }
    }

    public void Cancel()
    {
        Debug.Log("RotateGrids Cancel clicked");

        rotateMenu.enabled = false;
    }

    private float GetInput(TMP_InputField inputField)
    {
        float length = float.Parse(inputField.text);
        // Clears input field.
        inputField.Select();
        inputField.text = "";
        return length;
    }
}
