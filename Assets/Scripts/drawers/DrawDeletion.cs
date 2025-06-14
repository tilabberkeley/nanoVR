/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Highlight;

/// <summary>
/// Add deletion to selected nucleotide.
/// </summary>
public class DrawDeletion
{
    /// <summary>
    /// Command method to create deletion.
    /// </summary>
    public static void DoDeletion(NucleotideData nd)
    {
        ICommand command = new DeletionCommand(nd);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Actual method that creates deletion.
    /// </summary>
    /// <param name="go">Gameobject nucleotide of deletion.</param>
    public static void Deletion(GameObject go)
    {
        var ntc = go.GetComponent<NucleotideComponent>();
        if (ntc.IsInsertion)
        {
            Debug.Log("Cannot draw deletion over insertion.");
            return;
        }
        if (!ntc.Selected)
        {
            Debug.Log("Cannot draw insertion on unbound nucleotide.");
            return;
        }

        Strand strand = Utils.GetStrand(go);

        if (ntc.IsDeletion)
        {
            ntc.IsDeletion = false;
            UnhighlightDeletion(go);
        }
        else
        {
            ntc.IsDeletion = true;
            HighlightDeletion(go);
        }

        // Update strand DNA sequence
        if (strand != null)
        {
            string sequence = strand.Sequence;
            strand.Sequence = sequence;
            Utils.CheckMismatch(strand);
        }
    }

    public static void Deletion(NucleotideData nd)
    {
        if (nd.IsInsertion)
        {
            Debug.Log("Cannot draw deletion over insertion.");
            return;
        }
        if (!nd.IsSelected())
        {
            Debug.Log("Cannot draw insertion on unbound nucleotide.");
            return;
        }

        Strand strand = nd.GetStrand();

        if (nd.IsDeletion)
        {
            nd.IsDeletion = false;
            UnhighlightDeletion(nd);
            nd.GetDomain().Deletions.Remove(nd.Id);
        }
        else
        {
            nd.IsDeletion = true;
            HighlightDeletion(nd);
            nd.GetDomain().Deletions.Add(nd.Id);
        }

        // Update strand DNA sequence
        if (strand != null)
        {
            string sequence = strand.Sequence;
            if (sequence.Length > 0)
            {
                strand.SetSequenceRevamp(sequence);
                Utils.CheckMismatch(strand);
            }
        }
    }
}
