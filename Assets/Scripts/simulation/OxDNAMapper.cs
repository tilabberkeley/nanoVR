using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Utils;

/// <summary>
/// Manages mappings between native nucleotides and their line numbers in oxDNA files.
/// Additionally, this provides updates to the nucleotides provided a .dat file for simulation. 
/// </summary>
public class OxDNAMapper
{
    private Dictionary<int, GameObject> _lineIndexToNucleotide;

    public OxDNAMapper()
    {
        _lineIndexToNucleotide = new Dictionary<int, GameObject>();
    }

    public void Add(int lineIndex, GameObject nucleotide)
    {
        _lineIndexToNucleotide.Add(lineIndex, nucleotide);
    }
    
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

    private void UpdateNucleotidePosition(int lineIndex, Vector3 datFilePosition, Vector3 datFileA1)
    {
        _lineIndexToNucleotide.TryGetValue(lineIndex, out GameObject nucleotide);

        // Convert oxDNA position to native position.
        Vector3 position = (datFilePosition - 0.4f * datFileA1) / SCALE;

        /* Instead of calling DrawPoint.SetNucleotide to update the nucleotides position, it will be directly done here.
         * This avoids calling GetComponent on the nucleotide - only the gameobject is needed to update the position.
         * If the a1 value needs to be updated in the future, then GetComponent will need to be called.
         */
        nucleotide.transform.position = position;
    }
}
