using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelSequence : MonoBehaviour
{
    public GameObject Tog7249;
    public GameObject Tog8064;
    public TextAsset dna7249;
    public TextAsset dna8064;
    public Button GenerateBtn;

    public void Tog7249Sequence() {
        GlobalVariables.s_sequence = dna7249.text;
        turnOnGenerateBtn();
    }

    public void Tog8064Sequence() {
        GlobalVariables.s_sequence = dna8064.text;
        turnOnGenerateBtn();
    }

    public void turnOnGenerateBtn() {
        GenerateBtn.interactable = true;
    }
}
