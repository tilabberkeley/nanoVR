using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalVariables;

public class ClearScene : MonoBehaviour
{
    /// <summary>
    /// Clears entire Unity scene.
    /// Called by ClearScene button in hierarchy.
    /// </summary>
    public void ResetScene()
    {
        SceneManager.LoadScene("XR_Rig_Template");

        // Toggle bools for Draw panel
        s_selectTogOn = false;
        s_drawTogOn = true;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        s_loopoutOn = false;

        // adjust/add variables when add helix view.
        s_nucleotideView = true;
        s_hideStencils = false;
        s_visualMode = false;
        s_strandView = false;
        s_helixView = false;

        // Dictionaries and counts to keep track of Strand, Helix, and DNAGrid objects
        s_helixDict = new Dictionary<int, Helix>();
        s_strandDict = new Dictionary<int, Strand>();
        s_gridDict = new Dictionary<string, DNAGrid>();
        s_xoverSuggestions = new HashSet<XoverSuggestionComponent>();
        s_numStrands = 1;
        s_numHelices = 0;
        s_numGrids = 1;
        allGameObjects = new List<GameObject>();

        // OxView to keep track of all .oxview file imports
        s_oxView = new OxView();

        // Tracks how many copies of each gridName have been made
        s_gridCopies = new Dictionary<string, int>();

        // Dictionaries and counts to to keep track of alternate Strand, Helix, and DNAGrid objects when switching to visual mode
        s_visHelixDict = new Dictionary<int, Helix>();
        s_visStrandDict = new Dictionary<int, Strand>();
        s_visGridDict = new Dictionary<string, DNAGrid>();
        s_numVisStrands = 1;
        s_numVisHelices = 0;
        s_numVisGrids = 1;
        allVisGameObjects = new List<GameObject>();
    }
}
