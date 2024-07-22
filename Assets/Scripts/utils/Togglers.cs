/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GlobalVariables;

public class Togglers : MonoBehaviour
{
    /// <summary>
    /// Singleton which provides a single static instance for this class. Allows other
    /// scripts to access instance methods in this class.
    /// </summary>
    public static Togglers Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SelectToggled()
    {
        s_selectTogOn = true;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        s_loopoutOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void DrawToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = true;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        s_loopoutOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void EraseToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = true;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        s_loopoutOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void SplitToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = true;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        s_loopoutOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void MergeToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = true;
        s_insTogOn = false;
        s_delTogOn = false;
        s_loopoutOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void InsertionToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = true;
        s_delTogOn = false;
        s_loopoutOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void DeletionToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = true;
        s_loopoutOn = false;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void LoopOutToggled()
    {
        s_selectTogOn = false;
        s_drawTogOn = false;
        s_eraseTogOn = false;
        s_splitTogOn = false;
        s_mergeTogOn = false;
        s_insTogOn = false;
        s_delTogOn = false;
        s_loopoutOn = true;
        DrawNucleotideDynamic.ResetNucleotides();
        SelectStrand.Reset();
    }

    public void StencilsToggled()
    {
        s_hideStencils = !s_hideStencils;
    }

    public void NucleotideViewToggled()
    {
        s_strandView = false;
        s_helixView = false;
        s_nucleotideView = true;
        UpdateView();
    }

    public void StrandViewToggled()
    {
        s_strandView = true;
        s_helixView = false;
        s_nucleotideView = false;
        UpdateView();
    }

    public void HelixViewToggled()
    {
        s_strandView = false;
        s_helixView = true;
        s_nucleotideView = false;
        UpdateView();
    }

    /// <summary>
    /// Updates the view based on the values of the view togglers.
    /// Note: I had to add this because the togglers trigger whatever method you have in OnValueChanged
    /// whenever the toggle is turned on or off. This was breaking the domains because they would get
    /// turned on then off because one toggler would turn them on and the other would turn them off.
    /// This way, the view change only happens when one of them is turned on.
    /// </summary>
    private void UpdateView()
    {
        ToggleGroup toggleGroup = GetComponent<ToggleGroup>();

        // Only one toggle should be on
        Toggle activeToggle = toggleGroup.ActiveToggles().FirstOrDefault();
        
        // Magic strings unfortunately :(
        if (activeToggle.name == "NucleotideViewTog")
        {
            ViewingPerspective.ViewNucleotide();
        }
        else if (activeToggle.name == "StrandViewTog")
        {
            ViewingPerspective.ViewStrand();
        }
        else
        {
            // TODO: Helix view
        }
    }
}
