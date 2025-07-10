/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu>
 */
using static GlobalVariables;

/// <summary>
/// Merges two strands into one strand.
/// </summary>
public class DrawMerge
{
    public static int IsValid(NucleotideData nd)
    {
        // Checks GameObject is part of strand.
        if (!nd.IsSelected())
        {
            return -1;
        }

        // Check GameObject is a strand head or a strand tail.
        Strand strand = nd.GetStrand();
        if (strand.GetHead() != nd && strand.GetTail() != nd)
        {
            return -1;
        }

        // Check GameObject's neighbor is selected.
        Helix helix = nd.GetHelix();
        int direction = nd.Direction;
        NucleotideData headNeighbor = helix.GetHeadNeighbor(nd, direction);
        NucleotideData tailNeighbor = helix.GetTailNeighbor(nd, direction);
        if (strand.GetHead() == nd && headNeighbor != null && headNeighbor.IsSelected())
        {
            return 0;
        }
        else if (strand.GetTail() == nd && tailNeighbor != null && tailNeighbor.IsSelected())
        {
            return 1;
        }
        return -1;
    }

    public static void DoMergeStrand(NucleotideData nd)
    {
        int valid = IsValid(nd);
        if (valid == -1)
        {
            return;
        }
        s_helixDict.TryGetValue(nd.HelixId, out Helix helix);
        int direction = nd.Direction;
        NucleotideData neighbor = null;

        if (valid == 0)
        {
            neighbor = helix.GetHeadNeighbor(nd, direction);
        }
        else if (valid == 1)
        {
            neighbor = helix.GetTailNeighbor(nd, direction);
        }

        ICommand command = new MergeCommand(nd, neighbor);
        CommandManager.AddCommand(command);
    }

    public static void MergeStrand(NucleotideData nd)
    {
        int valid = IsValid(nd);
        if (valid == -1)
        {
            return;
        }

        Helix helix = nd.GetHelix();
        int direction = nd.Direction;
        
        if (valid == 0)
        {
            NucleotideData neighbor = helix.GetHeadNeighbor(nd, direction);
            MergeStrand(nd, neighbor, isHead: true); 
        }
        else if (valid == 1)
        {
            NucleotideData neighbor = helix.GetTailNeighbor(nd, direction);
            MergeStrand(nd, neighbor, isHead: false);
        }
    }

    public static void MergeStrand(NucleotideData nd1, NucleotideData nd2, bool isHead)
    {
        Strand s1 = nd1.GetStrand();
        Strand s2 = nd2.GetStrand();
        //bool circularStrand = firstNtc.StrandId == secondNtc.StrandId;

        Domain d1 = nd1.GetDomain();
        Domain d2 = nd2.GetDomain();

        d1.Merge(d2);

        if (isHead)
        {
            s1.AddToHead(s2.Domains.GetRange(0, s2.Domains.Count - 1));
        }
        else
        {
            s1.AddToTail(s2.Domains.GetRange(1, s2.Domains.Count - 1));
        }
        SelectStrand.RemoveStrand(s2.Id);
        s1.SetDomainsRevamp();
    }
}
