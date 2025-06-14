using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class OpenFile : MonoBehaviour
{
    string path;
    public GameObject ChooseSeqTog;
    public Text FileName;
    public Button GenerateBtn;

    public void OpenFileExplorer() {
        changeToggleToTrue();
        // path = EditorUtility.OpenFilePanel("Show all text files (.txt) ", "", "txt");
        // GlobalVariables.sequence = getText();
        
    }

    public void changeToggleToTrue() {
        ChooseSeqTog.GetComponent<Toggle>().isOn = true;
    }

    public void turnOnGenerateBtn() {
        GenerateBtn.interactable = true;
    }

    public void turnOffGenerateBtn() {
        GenerateBtn.interactable = false;
    }

    public string getText() {
        if (path != null && path.Length != 0) {
            turnOnGenerateBtn();
            // WWW www = new WWW("file:///" + path);
            FileName.text = Path.GetFileName(path);
            return ""; // www.text ### commented out because getting compiler warnings. Revist script later.
        }
        else {
            turnOffGenerateBtn();
            FileName.text = "No file chosen";
            return null;
        }
    }
   
}
