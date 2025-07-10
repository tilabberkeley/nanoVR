/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using UnityEngine;
using static Highlight;

/// <summary>
/// Add insertion to selected nucleotide.
/// </summary>
public class DrawInsertion : MonoBehaviour
{
    /// <summary>
    /// Command method to create insertion.
    /// </summary>
    /// <param name="go">Gameobject nucleotide of insertion.</param>
    /// <param name="length">Length of insertion.</param>
    public static void DoInsertion(NucleotideData nd, int length)
    {
        ICommand command = new InsertionCommand(nd, length);
        CommandManager.AddCommand(command);
    }

    /// <summary>
    /// Actual method that creates insertion.
    /// </summary>
    public static void Insertion(NucleotideData nd, int length)
    {
        if (nd.IsDeletion)
        {
            Debug.Log("Cannot draw insertion over deletion.");
            return;
        }
        if (!nd.IsSelected())
        {
            Debug.Log("Cannot draw insertion on unbound nucleotide.");
            return;
        }

        Strand strand = nd.GetStrand();

        if (nd.IsInsertion)
        {
            nd.Insertion = 0;
            UnhighlightInsertion(nd);
            nd.GetDomain().Insertions.Remove(nd.Id);
        }
        else
        {
            nd.Insertion = length;
            HighlightInsertion(nd);
            nd.GetDomain().Insertions.Add(nd.Id, length);
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
