using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using static Utils;
using static Geometry;

public class OxDNAMapping
{
    public NucleotideData Nucleotide { get; set; }
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 A1 { get; set; }
    public int StrandId { get; set; }
    public string Base { get; set; }
    public Color Color { get; set; }
    private int prevBackId = -1;
    private int nextBackId = -1;
    private int prevNuclId = -1;
    private int nextNuclId = -1;
    public int PrevBackId {
        get => prevBackId;
        set => prevBackId = value;
    }

    public int NextBackId
    {
        get => nextBackId;
        set => nextBackId = value;
    }
    public int PrevNuclId
    {
        get => prevNuclId;
        set => prevNuclId = value;
    }
    public int NextNuclId
    {
        get => nextNuclId;
        set => nextNuclId = value;
    }
}

public class StrandInfoMapping
{
    public List<NucleotideData> Nucleotides { get; set; }
    public Color Color { get; set; }
}

public class OxView
{
    private int oxViewId;
    public int OxViewId { get => oxViewId; }

    private Dictionary<int, OxDNAMapping> _lineIndexToOxDNAMapping;
    private Dictionary<int, StrandInfoMapping> _strandIdToStrandInfo;
    private int _numStrands;

    private StringBuilder _topFileStringBuilder;
    private string _topFile;
    public string TopFile { get => _topFile; }

    private StringBuilder _datFileStringBuilder;
    private string _datFile;
    public string DatFile { get => _datFile; }

    private List<NucleotideData> nucleotides;
    private List<Vector3> nucleotidePositions;
    private List<Matrix4x4> nuclMatrices;
    private List<Matrix4x4> backboneMatrices;
    private List<Color> nucleotideColors;
    private List<Color> backboneColors;

    public List<Matrix4x4> Nucleotides { get => nuclMatrices; }
    public List<Matrix4x4> Backbones { get => backboneMatrices; }
    public List<Color> NucleotideColors { get => nucleotideColors; }
    public List<Color> BackboneColors { get => backboneColors; }
    public Dictionary<int, StrandInfoMapping> StrandIdToStrandInfo { get => _strandIdToStrandInfo; }

    public OxView(int oxViewId)
    {
        this.oxViewId = oxViewId;
        _lineIndexToOxDNAMapping = new Dictionary<int, OxDNAMapping>();
        _strandIdToStrandInfo = new Dictionary<int, StrandInfoMapping>();
        _topFileStringBuilder = new StringBuilder();
        _datFileStringBuilder = new StringBuilder();
        nucleotides = new List<NucleotideData>();
        nucleotidePositions = new List<Vector3>();
        nuclMatrices = new List<Matrix4x4>();
        backboneMatrices = new List<Matrix4x4>();
        nucleotideColors = new List<Color>();
        backboneColors = new List<Color>();
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
                _topFileStringBuilder.AppendLine($"{strandCounter} {monomer.Type} {prime5} {prime3}");
                _datFileStringBuilder.AppendLine($"{string.Join(" ", monomer.P)} {string.Join(" ", monomer.A1)} {string.Join(" ", monomer.A3)} 0 0 0 0 0 0");

                globalNucleotideIndex++;

                Vector3 position = new Vector3((float)monomer.P[0], (float)monomer.P[1], (float)monomer.P[2]);
                Vector3 a1Vec = new Vector3((float)monomer.A1[0], (float)monomer.A1[1], (float)monomer.A1[2]);

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
                Nucleotides = new List<NucleotideData>()
            };
            _strandIdToStrandInfo.Add(strandId, strandInfoMapping);
        }

        // Generate Nucleotides
        //int numNucleotides = _lineIndexToOxDNAMapping.Count;

        /*List<NucleotideData> newNucleotides = new List<NucleotideData>();

        if (ObjectPoolManager.Instance.CanGetNucleotides(numNucleotides))
        {
            newNucleotides.AddRange(ObjectPoolManager.Instance.GetNucleotides(numNucleotides));
        }
        else
        {
            // TODO: Generate new nucleotides if object pool manager is empty
            throw new NotImplementedException("Object pool manager empty");
        }*/

        foreach (KeyValuePair<int, OxDNAMapping> entry in _lineIndexToOxDNAMapping)
        {
            //NucleotideData newNucleotide = newNucleotides[i++];

            Vector3 pos = (entry.Value.Position - 0.4f * entry.Value.A1) / SCALE_FROM_NANOVR_TO_NM;
            nucleotidePositions.Add(pos);
            Matrix4x4 nuclMatrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(NUCL_RAD, NUCL_RAD, NUCL_RAD));
            NucleotideData newNucleotide = new NucleotideData(entry.Key, -1, -1, inExtension: false)
            {
                SavedPosition = nuclMatrix.GetColumn(3),
                OxViewId = oxViewId,
                Color = entry.Value.Color,
            };

            nuclMatrices.Add(nuclMatrix);
            nucleotides.Add(newNucleotide);
            nucleotideColors.Add(entry.Value.Color);

            entry.Value.Nucleotide = newNucleotide;

            // Add nucleotide to appropiate strand list
            _strandIdToStrandInfo.TryGetValue(entry.Value.StrandId, out StrandInfoMapping strandInfoMapping);
            strandInfoMapping.Nucleotides.Add(newNucleotide);
            strandInfoMapping.Color = entry.Value.Color; // This is repetive, potential refactor
        }

        BuildStrands();
    }

    private void BuildStrands()
    {
        int globalBackboneIndex = 0;

        foreach (KeyValuePair<int, StrandInfoMapping> entry in _strandIdToStrandInfo)
        {
            List<NucleotideData> nucleotides = entry.Value.Nucleotides;
            int strandSize = nucleotides.Count;

            //NucleotideData firstNucleotide = nucleotides[0];
            //NucleotideData secondNucleotide;
            for (int i = 1; i < strandSize; i++)
            {
                // NOTE: Edited DY 6/1/25 seems like this was not being used in simulations

                /*GameObject backbone = backbones[i - 1];
                secondNucleotide = nucleotides[i];
                DrawPoint.SetBackbone(backbone, i - 1, -1, -1, firstNucleotide.transform.position, secondNucleotide.transform.position, false, true);

                // Assing first and second nucleotides to backbones for simulation.
                BackBoneComponent backboneComponent = backbone.GetComponent<BackBoneComponent>();
                backboneComponent.FirstNucleotide = firstNucleotide;
                backboneComponent.SecondNucleotide = secondNucleotide;

                // Move to next nucleotide
                firstNucleotide = secondNucleotide;*/

                NucleotideData prev = nucleotides[i - 1];
                NucleotideData curr = nucleotides[i];

                if (_lineIndexToOxDNAMapping.TryGetValue(prev.Id, out var prevMapping) &&
                _lineIndexToOxDNAMapping.TryGetValue(curr.Id, out var currMapping))
                {
                    // Set references to shared backbone index
                    prevMapping.NextBackId = globalBackboneIndex;
                    currMapping.PrevBackId = globalBackboneIndex;

                    prevMapping.NextNuclId = curr.Id;
                    currMapping.PrevNuclId = prev.Id;

                    // Compute and store backbone matrix
                    Vector3 prevPos = prev.GetPosition();
                    Vector3 currPos = curr.GetPosition();
                    Matrix4x4 backboneMatrix = GetBackboneMatrix(prevPos, currPos, fiveToThree: true);
                    backboneMatrices.Add(backboneMatrix);
                    backboneColors.Add(entry.Value.Color);

                    globalBackboneIndex++;
                }
            }

            // Create strand
            // CreateStrand(nucleotides, entry.Key, entry.Value.Color, true);

            // TODO: set sequence
        }
    }

    public void SimulationUpdate(string datFile)
    {
        StringReader datFileReader = new StringReader(datFile);
        //Debug.Log($"datFile: {datFile}");
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

            UpdateNucleotidePosition(mapping);

            nextLine = datFileReader.ReadLine();
        }
    }

    private void UpdateNucleotidePosition(OxDNAMapping mapping)
    {
        // r center of mass to backbone repulsion site.
        Vector3 position = (mapping.Position - 0.4f * mapping.A1) / SCALE_FROM_NANOVR_TO_NM / (float)NM_TO_OX_UNITS;
        NucleotideData nucleotide = mapping.Nucleotide;
        //nucleotide.UpdatePosition(position + nucleotide.GetPosition());
        Matrix4x4 mat = nuclMatrices[nucleotide.Id];
        mat.SetColumn(3, new Vector4(position.x, position.y, position.z, 1f));
        nuclMatrices[nucleotide.Id] = mat;

        // Update backbone connecting to previous nucleotide
        if (mapping.PrevBackId >= 0 && _lineIndexToOxDNAMapping.TryGetValue(mapping.PrevNuclId, out var prevMapping))
        {
            Vector3 prevPos = (prevMapping.Position - 0.4f * prevMapping.A1) / SCALE_FROM_NANOVR_TO_NM / (float)NM_TO_OX_UNITS;
            Matrix4x4 backboneMatrix = GetBackboneMatrix(prevPos, position, fiveToThree: true);
            backboneMatrices[mapping.PrevBackId] = backboneMatrix;
        }

        // Update backbone connecting to next nucleotide
        if (mapping.NextBackId >= 0 && _lineIndexToOxDNAMapping.TryGetValue(mapping.NextNuclId, out var nextMapping))
        {
            Vector3 nextPos = (nextMapping.Position - 0.4f * nextMapping.A1) / SCALE_FROM_NANOVR_TO_NM / (float)NM_TO_OX_UNITS;
            Matrix4x4 backboneMatrix = GetBackboneMatrix(position, nextPos, fiveToThree: true);
            backboneMatrices[mapping.NextBackId] = backboneMatrix;
        }
    }

    public void Clear()
    {
        _lineIndexToOxDNAMapping.Clear();
        _strandIdToStrandInfo.Clear();
        _topFileStringBuilder.Clear();
        _datFileStringBuilder.Clear();
        nucleotides.Clear();
        nucleotidePositions.Clear();
        nuclMatrices.Clear();
        backboneMatrices.Clear();
        nucleotideColors.Clear();
        backboneColors.Clear();
    }

    public void RestoreNucleotides()
    {
        foreach (OxDNAMapping ox in _lineIndexToOxDNAMapping.Values)
        {
            NucleotideData nd = ox.Nucleotide;
            nd.UpdatePosition(nd.SavedPosition);
        }
    }
}
