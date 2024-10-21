using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToSimulation : MonoBehaviour
{
    public void GoToSimulationHandle()
    {
        SceneManager.LoadScene("Simulation");
    }
}
