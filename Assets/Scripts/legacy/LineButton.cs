using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineButton : MonoBehaviour
{
    public Button lineBtn;

    public void selectLineBtn()
    {
        lineBtn.Select();
    }

    public void disselectOtherBtns()
    {

    }
}
