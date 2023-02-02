using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
    public GameObject MenuCanvas;
    
    public void MenuOpener() {
        openMenu();
        // resetMenu();  
    }

    void openMenu() {
        if (MenuCanvas != null) {  
            bool isActive = MenuCanvas.activeSelf;  
            MenuCanvas.SetActive(!isActive);  
        }  
    }

 
}
