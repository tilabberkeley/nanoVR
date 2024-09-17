using Newtonsoft.Json.Linq;
using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static GlobalVariables;
using static UnityEngine.EventSystems.EventTrigger;
using static Utils;

public class OxDNAMapping
{
    public GameObject Nucleotide { get; set; }
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 A1 { get; set; }
}

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

public class OxView
{
    List<GameObject> nucleotides;
    List<GameObject> backbones;
    private Dictionary<int, OxDNAMapping> lineIndexToOxDNAMapping;
    int nucleotideIndex;
    int backboneIndex;

    private StringBuilder _topFileStringBuilder;
    private string _topFile;
    public string TopFile { get => _topFile; }

    private StringBuilder _datFileStringBuilder;
    private string _datFile;
    public string DatFile { get => _datFile; }

    public OxView()
    {
        nucleotides = new List<GameObject>();
        backbones = new List<GameObject>();
        lineIndexToOxDNAMapping = new Dictionary<int, OxDNAMapping>();
        nucleotideIndex = 0;
        backboneIndex = 0;
        _topFileStringBuilder = new StringBuilder();
        _datFileStringBuilder = new StringBuilder();
    }

    public void BuildStrands(List<OxViewStrand> strands, List<double> box)
    {
        int numStrands = strands.Count;
        int numNucleotides = 0;

        foreach (OxViewStrand strand in strands)
        {
            numNucleotides += strand.Monomers.Count;
        }

        // Write top file metadata
        _topFileStringBuilder.Append($"{numNucleotides} {numStrands}" + Environment.NewLine);

        // Write dat file metadata
        _datFileStringBuilder.Append("t = 0" + Environment.NewLine);
        _datFileStringBuilder.Append("b = " + string.Join(" ", box) + Environment.NewLine);
        _datFileStringBuilder.Append("E = 0 0 0" + Environment.NewLine);

        // Line number will increase as the strands are parsed.
        int lineIndex = 0;

        int strandCounter = 1;
        foreach (OxViewStrand strand in strands)
        {
            List<OxViewMonomer> monomers = strand.Monomers;
            StringBuilder sequence = new StringBuilder();
            Color color = Color.white;

            int prime5 = -1;
            int prime3 = 1;

            // Iterate backwards as per observed in an oxDNA export sample
            for (int i = strand.Monomers.Count - 1; i >= 0; i--)
            {
                OxViewMonomer monomer = strand.Monomers[i];

                // Write file contents
                _topFileStringBuilder.Append($"{strandCounter} {monomer.Type} {prime5++} {(i != 0 ? prime3++ : -1)}" + Environment.NewLine);
                _datFileStringBuilder.Append($"{string.Join(" ", monomer.P)} {string.Join(" ", monomer.A1)} {string.Join(" ", monomer.A3)}" + " 0 0 0 0 0 0" + Environment.NewLine);

                string type = monomer.Type;
                sequence.Append(type);

                Vector3 position = new Vector3(monomer.P[0], monomer.P[1], monomer.P[2]);
                Vector3 a1Vec = new Vector3(monomer.A1[0], monomer.A1[1], monomer.A1[2]);

                OxDNAMapping mapping = new OxDNAMapping()
                {
                    Id = monomer.Id,
                    Position = position,
                    A1 = a1Vec,
                };
                lineIndexToOxDNAMapping.Add(lineIndex, mapping);

                // Convert base 10 color to hex.
                string hexColor = monomer.Color.ToString("X6");

                // Set strand color once
                if (color.Equals(Color.white))
                {
                    ColorUtility.TryParseHtmlString("#" + hexColor, out color);
                }

                lineIndex++;
            }

            // SetNucleotides(monomers.Count, positions, a1s, ids);
            // List<GameObject> nucleotides = oxView.GetSubstructure(monomers.Count);
            // Strand strand = CreateStrand(nucleotides, s_numStrands, color, true);
            // strand.SetSequence(sequence.ToString());
        }

        _topFile = _topFileStringBuilder.ToString();
        _datFile = _datFileStringBuilder.ToString();

        BuildOrigami();
    }

    private void SetNucleotidePosition(OxDNAMapping mapping)
    {
        Vector3 position = (mapping.Position - 0.4f * mapping.A1) / SCALE;
        DrawPoint.SetNucleotide(mapping.Nucleotide, position, mapping.Id, -1, -1, false, true);
    }

    private void BuildOrigami()
    {
        // Generate Nucleotides
        int numNucleotides = lineIndexToOxDNAMapping.Count;

        List<GameObject> newNucleotides = new List<GameObject>();

        if (ObjectPoolManager.Instance.CanGetNucleotides(numNucleotides))
        {
            newNucleotides.AddRange(ObjectPoolManager.Instance.GetNucleotides(numNucleotides));
        }

        int i = 0;
        foreach (KeyValuePair<int, OxDNAMapping> entry in lineIndexToOxDNAMapping)
        {
            entry.Value.Nucleotide = newNucleotides[i++];

            // Assign positions
            SetNucleotidePosition(entry.Value);
        }
    }

    public async Task BuildStrand(int length)
    {
        // Draw strand.
        // First check if ObjectPool has enough GameObjects to use.
        // If not, generate them async.

        // TODO: Testing this!! - 8/4/24 DY
        int count64 = length / 64;
        if (ObjectPoolManager.Instance.CanGetNucleotides(length) && ObjectPoolManager.Instance.CanGetBackbones(length - 1))
        {
            nucleotides.AddRange(ObjectPoolManager.Instance.GetNucleotides(length));
            backbones.AddRange(ObjectPoolManager.Instance.GetBackbones(length - 1));
        }
        else
        {
            await GenerateGameObjects(length, true);
        }
    }

    private async Task GenerateGameObjects(int length, bool hideGameObjects)
    {
        //int num64nt = length / 64;
        //length %= 64;
        int num32nt = length / 32;
        length %= 32;
        int num16nt = length / 16;
        length %= 16;
        int num8nt = length / 8;
        length %= 8;
        int num4nt = length / 4;
        length %= 4;
        int num2nt = length / 2;
        length %= 2;
        int num1nt = length / 1;

        //int numConnectingBackbones = num64nt + num32nt + num16nt + num8nt + num4nt + num2nt + num1nt - 1;
        int numConnectingBackbones = num32nt + num16nt + num8nt + num4nt + num2nt + num1nt - 1;


        /*for (int i = 0; i < num64nt; i++)
        {
            _nucleotidesA.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
            await Task.Yield();

            _nucleotidesB.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_64, hideGameObjects));
            await Task.Yield();

            _backbonesA.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
            await Task.Yield();

            _backbonesB.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_63, hideGameObjects));
            await Task.Yield();

        }*/
        //await Task.Yield();
        for (int i = 0; i < num32nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
            await Task.Yield();

            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_32, hideGameObjects));
            //await Task.Yield();

            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
            await Task.Yield();

            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_31, hideGameObjects));
            //await Task.Yield();

        }

        for (int i = 0; i < num16nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
            //await Task.Yield();

            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_16, hideGameObjects));
            //await Task.Yield();

            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects));
            //await Task.Yield();

            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_15, hideGameObjects));
            //await Task.Yield();
        }


        for (int i = 0; i < num8nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_8, hideGameObjects));
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_7, hideGameObjects));
        }
        await Task.Yield();

        for (int i = 0; i < num4nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_4, hideGameObjects));
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_3, hideGameObjects));
        }

        for (int i = 0; i < num2nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_2, hideGameObjects));
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
        }

        for (int i = 0; i < num1nt; i++)
        {
            nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
            //nucleotides.AddRange(DrawPoint.MakeNucleotides(NucleotideSize.LENGTH_1, hideGameObjects));
        }

        for (int i = 0; i < numConnectingBackbones; i++)
        {
            backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
            //backbones.AddRange(DrawPoint.MakeBackbones(BackboneSize.LENGTH_1, hideGameObjects));
        }
        await Task.Yield();
    }
   
    public List<GameObject> GetSubstructure(int length)
    {
        List<GameObject> temp = new List<GameObject>();
        for (int i = 0; i < length - 1; i++)
        {
            temp.Add(nucleotides[i + nucleotideIndex]);
            temp.Add(backbones[i + backboneIndex]);
        }
        temp.Add(nucleotides[nucleotideIndex + length - 1]);

        // Update indices after GetSubstructure which is last method called.
        nucleotideIndex += length;
        backboneIndex += length - 1;
        return temp;
    }

    public void SetNucleotides(int length, List<Vector3> positions, List<Vector3> a1s, List<int> ids)
    {
        for (int i = 0; i < length; i++)
        {
            Vector3 position = (positions[i] - 0.4f * a1s[i]) / SCALE;
            DrawPoint.SetNucleotide(nucleotides[i + nucleotideIndex], position, ids[i], -1, -1, false, true) ;
            if (i > 0)
            {
                DrawPoint.SetBackbone(backbones[i + backboneIndex - 1], -1, -1, -1, nucleotides[i + nucleotideIndex - 1].transform.position, nucleotides[i + nucleotideIndex].transform.position, false, true);
            }
        }
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
            lineIndexToOxDNAMapping.TryGetValue(lineIndex++, out OxDNAMapping mapping);

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
}
