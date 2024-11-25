using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Utils;
using static Geometry;

/// <summary>
/// Manages mappings between native nucleotides and their line numbers in oxDNA files.
/// Additionally, this provides updates to the nucleotides provided a .dat file for simulation. 
/// </summary>
public class OxDNAMapper
{
    private Dictionary<int, NucleotideComponent> _lineIndexToNucleotide;

    public OxDNAMapper()
    {
        _lineIndexToNucleotide = new Dictionary<int, NucleotideComponent>();
    }

    /// <summary>
    /// Adds a mapping from a line index of a dat file to a nucleotide.
    /// </summary>
    public void Add(int lineIndex, NucleotideComponent nucleotide)
    {
        _lineIndexToNucleotide.Add(lineIndex, nucleotide);
    }

    /// <summary>
    /// Updates the simulation to the positions given in the dat file.
    /// </summary>
    public void SimulationUpdate(string datFile)
    {
        StringReader datFileReader = new StringReader(datFile);

        // Read metadata - not needed
        datFileReader.ReadLine();
        datFileReader.ReadLine();
        datFileReader.ReadLine();

        int lineIndex = 0;

        string nextLine = datFileReader.ReadLine();
        while (nextLine != null)
        {
            // Extract updated positioning
            string[] updatedInfo = nextLine.Split(' ');
            Vector3 datFilePosition = new Vector3(float.Parse(updatedInfo[0]), float.Parse(updatedInfo[1]), float.Parse(updatedInfo[2]));
            Vector3 datFileA1 = new Vector3(float.Parse(updatedInfo[3]), float.Parse(updatedInfo[4]), float.Parse(updatedInfo[5]));

            UpdateNucleotidePosition(lineIndex, datFilePosition, datFileA1);

            lineIndex++;
            nextLine = datFileReader.ReadLine();
        }
    }

    /// <summary>
    /// Updates the associated nucleotide at the givne line index.
    /// </summary>
    private void UpdateNucleotidePosition(int lineIndex, Vector3 datFilePosition, Vector3 datFileA1)
    {
        _lineIndexToNucleotide.TryGetValue(lineIndex, out NucleotideComponent nucleotide);

        // Convert oxDNA position to native position.
        Vector3 position = (datFilePosition - 0.4f * datFileA1) / SCALE_FROM_NANOVR_TO_NM / (float)NM_TO_OX_UNITS;

        /* Instead of calling DrawPoint.SetNucleotide to update the nucleotides position, it will be directly done here.
         * This avoids calling GetComponent on the nucleotide - only the gameobject is needed to update the position.
         * 
         * Additionally, we are adding the saved position of the nucleotide because the simulation is centered at the origin.
         */
        nucleotide.transform.position = position; //+ nucleotide.Position;
    }

    /// <summary>
    /// Restores the nucleotides to their original editing position.
    /// </summary>
    public void RestoreNucleotidesToEdit()
    {
        foreach (NucleotideComponent nucleotideComponent in _lineIndexToNucleotide.Values)
        {
            nucleotideComponent.transform.position = nucleotideComponent.Position;
        }
    }
}
