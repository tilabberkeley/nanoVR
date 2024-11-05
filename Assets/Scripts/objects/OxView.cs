using Newtonsoft.Json.Linq;
using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UIElements;
using static GlobalVariables;
using static UnityEngine.EventSystems.EventTrigger;
using static Utils;

public class OxViewStrand
{
    public int Id { get; set; }
    public List<OxViewMonomer> Monomers { get; set; } = new List<OxViewMonomer>();
    public int End3 { get; set; }
    public int End5 { get; set; }
    public string Class { get; set; } = "";
}

public class OxViewMonomer
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Class { get; set; } = "";
    public List<float> P { get; set; } = new List<float>();
    public List<float> A1 { get; set; } = new List<float>();
    public List<float> A3 { get; set; } = new List<float>();
    public int? N3 { get; set; }
    public int? N5 { get; set; }
    public int Cluster { get; set; }
    public int Color { get; set; }
}

public class OxDNAMapping
{
    public GameObject Nucleotide { get; set; }
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 A1 { get; set; }
    public int StrandId { get; set; }
    public string Base { get; set; }
    public Color Color { get; set; }
}

public class StrandInfoMapping
{
    public List<GameObject> Nucleotides { get; set; }
    public Color Color { get; set; }
}

public class OxView
{
    private Dictionary<int, OxDNAMapping> _lineIndexToOxDNAMapping;
    private Dictionary<int, StrandInfoMapping> _strandIdToStrandInfo;
    private int _numStrands;

    private StringBuilder _topFileStringBuilder;
    private string _topFile;
    public string TopFile { get => _topFile; }

    private StringBuilder _datFileStringBuilder;
    private string _datFile;
    public string DatFile { get => _datFile; }

    public OxView()
    {
        _lineIndexToOxDNAMapping = new Dictionary<int, OxDNAMapping>();
        _strandIdToStrandInfo = new Dictionary<int, StrandInfoMapping>();
        _topFileStringBuilder = new StringBuilder();
        _datFileStringBuilder = new StringBuilder();
    }

    public void BuildStrands(List<OxViewStrand> strands, List<double> box)
    {
        _numStrands = strands.Count;
        int numNucleotides = 0;

        foreach (OxViewStrand strand in strands)
        {
            numNucleotides += strand.Monomers.Count;
        }

        // Write top file metadata
        _topFileStringBuilder.Append($"{numNucleotides} {_numStrands}" + Environment.NewLine);

        // Write dat file metadata
        _datFileStringBuilder.Append("t = 0" + Environment.NewLine);
        _datFileStringBuilder.Append("b = " + string.Join(" ", box) + Environment.NewLine);
        _datFileStringBuilder.Append("E = 0 0 0" + Environment.NewLine);

        // Line number will increase as the strands are parsed.
        int lineIndex = 0;

        int strandCounter = 1;
        int globalNucleotideIndex = -1;
        foreach (OxViewStrand strand in strands)
        {
            List<OxViewMonomer> monomers = strand.Monomers;
            Color color = Color.white;

            // Iterate backwards as seen in an oxDNA export sample
            for (int i = strand.Monomers.Count - 1; i >= 0; i--)
            {
                int prime5 = globalNucleotideIndex;
                int prime3 = globalNucleotideIndex + 2;

                // Beginning and end of strand edge cases
                if (i == strand.Monomers.Count - 1)
                {
                    prime5 = -1;
                }
                else if (i == 0)
                {
                    prime3 = -1;
                }

                OxViewMonomer monomer = strand.Monomers[i];

                // Write file contents
                _topFileStringBuilder.Append($"{strandCounter} {monomer.Type} {prime5} {prime3}" + Environment.NewLine);
                _datFileStringBuilder.Append($"{string.Join(" ", monomer.P)} {string.Join(" ", monomer.A1)} {string.Join(" ", monomer.A3)}" + " 0 0 0 0 0 0" + Environment.NewLine);

                globalNucleotideIndex++;

                Vector3 position = new Vector3(monomer.P[0], monomer.P[1], monomer.P[2]);
                Vector3 a1Vec = new Vector3(monomer.A1[0], monomer.A1[1], monomer.A1[2]);

                // Convert base 10 color to hex.
                string hexColor = monomer.Color.ToString("X6");

                // Set strand color once
                if (color.Equals(Color.white))
                {
                    ColorUtility.TryParseHtmlString("#" + hexColor, out color);
                }

                OxDNAMapping mapping = new OxDNAMapping()
                {
                    Id = monomer.Id,
                    Position = position,
                    A1 = a1Vec,
                    StrandId = strandCounter - 1, // Substract 1 for 0 index.
                    Base = monomer.Type,
                    Color = color
                };
                _lineIndexToOxDNAMapping.Add(lineIndex, mapping);

                lineIndex++;
            }

            strandCounter++;

            // SetNucleotides(monomers.Count, positions, a1s, ids);
            // List<GameObject> nucleotides = oxView.GetSubstructure(monomers.Count);
            // Strand strand = CreateStrand(nucleotides, s_numStrands, color, true);
            // strand.SetSequence(sequence.ToString());
        }

        _topFile = _topFileStringBuilder.ToString();
        _datFile = _datFileStringBuilder.ToString();

        BuildOrigami();
    }

    private void BuildOrigami()
    {
        // Initialize strand info mappings for strand dictionary
        for (int strandId = 0; strandId < _numStrands; strandId++)
        {
            StrandInfoMapping strandInfoMapping = new StrandInfoMapping
            {
                Nucleotides = new List<GameObject>()
            };
            _strandIdToStrandInfo.Add(strandId, strandInfoMapping);
        }

        // Generate Nucleotides
        int numNucleotides = _lineIndexToOxDNAMapping.Count;

        List<GameObject> newNucleotides = new List<GameObject>();

        if (ObjectPoolManager.Instance.CanGetNucleotides(numNucleotides))
        {
            newNucleotides.AddRange(ObjectPoolManager.Instance.GetNucleotides(numNucleotides));
        }
        else
        {
            // TODO: Generate new nucleotides if object pool manager is empty
            throw new NotImplementedException("Object pool manager empty");
        }

        int i = 0;
        foreach (KeyValuePair<int, OxDNAMapping> entry in _lineIndexToOxDNAMapping)
        {
            GameObject newNucleotide = newNucleotides[i++];
            entry.Value.Nucleotide = newNucleotide;

            // Add nucleotide to appropiate strand list
            _strandIdToStrandInfo.TryGetValue(entry.Value.StrandId, out StrandInfoMapping strandInfoMapping);
            strandInfoMapping.Nucleotides.Add(newNucleotide);
            strandInfoMapping.Color = entry.Value.Color; // This is repetive, potential refactor

            // Assign positions
            SetNucleotidePosition(entry.Value);
        }

        BuildStrands();
    }

    private void BuildStrands()
    {
        foreach (KeyValuePair<int, StrandInfoMapping> entry in _strandIdToStrandInfo)
        {
            List<GameObject> nucleotides = entry.Value.Nucleotides;
            int strandSize = nucleotides.Count;

            // Create backbones
            List<GameObject> backbones = new List<GameObject>();

            if (ObjectPoolManager.Instance.CanGetBackbones(strandSize - 1))
            {
                backbones.AddRange(ObjectPoolManager.Instance.GetBackbones(strandSize - 1));
            } 
            else
            {
                // TODO: Generate new backbones if object pool manager is empty
                throw new NotImplementedException("Object pool manager empty");
            }

            if (strandSize < 1)
            {
                // TODO
                throw new NotImplementedException("Strand size too small for oxview");
            }

            GameObject firstNucleotide = nucleotides[0];
            GameObject secondNucleotide;
            for (int i = 1; i < strandSize; i++)
            {
                GameObject backbone = backbones[i - 1];
                secondNucleotide = nucleotides[i];
                DrawPoint.SetBackbone(backbone, i - 1, -1, -1, firstNucleotide.transform.position, secondNucleotide.transform.position, false, true);

                // Assing first and second nucleotides to backbones for simulation.
                BackBoneComponent backboneComponent = backbone.GetComponent<BackBoneComponent>();
                backboneComponent.FirstNucleotide = firstNucleotide;
                backboneComponent.SecondNucleotide = secondNucleotide;

                // Move to next nucleotide
                firstNucleotide = secondNucleotide;
            }

            // Create strand
            CreateStrand(nucleotides, entry.Key, entry.Value.Color, true);

            // TODO: set sequence
        }
    }

    // TODO: Add generation of gameobjects when pool is emtpy.
    //private async Task GenerateGameObjects(int length, bool hideGameObjects)
    //{
    //    //int num64nt = length / 64;
    //    //length %= 64;
    //    int num32nt = length / 32;
    //    length %= 32;
    //    int num16nt = length / 16;
    //    length %= 16;
    //    int num8nt = length / 8;
    //    length %= 8;
    //    int num4nt = length / 4;
    //    length %= 4;
    //    int num2nt = length / 2;
    //    length %= 2;
    //    int num1nt = length / 1;

    //    //int numConnectingBackbones = num64nt + num32nt + num16nt + num8nt + num4nt + num2nt + num1nt - 1;
    //    int numConnectingBackbones = num32nt + num16nt + num8nt + num4nt + num2nt + num1nt - 1;


    //    /*for (int i = 0; i < num64nt; i++)
    //    {
    //        _nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
    //        await Task.Yield();

    //        _nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
    //        await Task.Yield();

    //        _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
    //        await Task.Yield();

    //        _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
    //        await Task.Yield();

    //    }*/
    //    //await Task.Yield();
    //    for (int i = 0; i < num32nt; i++)
    //    {
    //        nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
    //        await Task.Yield();

    //        //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
    //        //await Task.Yield();

    //        backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
    //        await Task.Yield();

    //        //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
    //        //await Task.Yield();

    //    }

    //    for (int i = 0; i < num16nt; i++)
    //    {
    //        nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
    //        //await Task.Yield();

    //        //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
    //        //await Task.Yield();

    //        backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects));
    //        //await Task.Yield();

    //        //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects));
    //        //await Task.Yield();
    //    }


    //    for (int i = 0; i < num8nt; i++)
    //    {
    //        nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
    //        //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
    //        backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
    //        //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
    //    }
    //    await Task.Yield();

    //    for (int i = 0; i < num4nt; i++)
    //    {
    //        nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
    //        //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
    //        backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
    //        //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
    //    }

    //    for (int i = 0; i < num2nt; i++)
    //    {
    //        nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
    //        //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
    //        backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
    //        //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
    //    }

    //    for (int i = 0; i < num1nt; i++)
    //    {
    //        nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
    //        //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
    //    }

    //    for (int i = 0; i < numConnectingBackbones; i++)
    //    {
    //        backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
    //        //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
    //    }
    //    await Task.Yield();
    //}

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
            _lineIndexToOxDNAMapping.TryGetValue(lineIndex++, out OxDNAMapping mapping);

            // Extract updated positioning
            string[] updatedInfo = nextLine.Split(' ');
            Vector3 position = new Vector3(float.Parse(updatedInfo[0]), float.Parse(updatedInfo[1]), float.Parse(updatedInfo[2]));
            Vector3 a1 = new Vector3(float.Parse(updatedInfo[3]), float.Parse(updatedInfo[4]), float.Parse(updatedInfo[5]));

            mapping.Position = position;
            mapping.A1 = a1;

            SetNucleotidePosition(mapping);

            nextLine = datFileReader.ReadLine();
        }
    }

    private void SetNucleotidePosition(OxDNAMapping mapping)
    {
        Vector3 position = (mapping.Position - 0.4f * mapping.A1) / SCALE;
        DrawPoint.SetNucleotide(mapping.Nucleotide, Vector3.zero, Vector3.zero, position, mapping.Id, -1, -1, false, true);
    }
}
