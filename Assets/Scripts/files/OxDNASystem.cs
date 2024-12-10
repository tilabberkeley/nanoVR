using System;
using System.Collections.Generic;
using UnityEngine;
using static Geometry;
using static Utils;
using static GlobalVariables;
using System.Text;
using Oculus.Interaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System.Linq;

public class OxDNASystem
{
    private List<OxdnaStrand> _oxdnaStrands = new List<OxdnaStrand>();

    /// <summary>
    /// On construction, the entire scene will be converted to create an oxdna system - very expensive.
    /// </summary>
    public OxDNASystem()
    {
        ConvertToOxdnaSystem();
    }

    /// <summary>
    /// Converts the nanoVR scene into an oxDNA system.
    /// </summary>
    private void ConvertToOxdnaSystem()
    {
        Dictionary<int, List<int>> modMap = GetModMap();

        // For efficiency, calculate each helix's vector once
        var helixVectors = new Dictionary<int, (OxdnaVector, OxdnaVector, OxdnaVector)>();

        foreach (KeyValuePair<int, Helix> helixEntry in s_helixDict)
        {
            int helixId = helixEntry.Key;
            Helix helix = helixEntry.Value;
            var helixVectorsResult = GetHelixVectors(helix);
            helixVectors[helixId] = helixVectorsResult;
        }

        foreach (Strand strand in s_strandDict.Values)
        {
            var oxdnaStrand = new OxdnaStrand();
            oxdnaStrand.Color = strand.Color;
            oxdnaStrand.IsScaffold = strand.IsScaffold;

            foreach (GameObject nucleotide in strand.Nucleotides)
            {
                var nucleotideComponent = nucleotide.GetComponent<NucleotideComponent>();
                if (nucleotideComponent == null || nucleotideComponent.IsDeletion)
                {
                    continue; // Backbone or deletion
                }

                var helixId = nucleotideComponent.HelixId;
                var helix = s_helixDict[helixId];
                var originForwardNormal = helixVectors[helixId];
                var origin = originForwardNormal.Item1;
                var forward = originForwardNormal.Item2;
                var normal = originForwardNormal.Item3;
                var isNucleotideForward = nucleotideComponent.Direction == 0; // 5 to 3.

                if (!isNucleotideForward)
                {
                    normal = normal.Rotate(-MINOR_GROOVE_ANGLE, forward);
                }

                // oxDNA will rotate angles by +/- _GROOVE_GAMMA, so we first unrotate by that amount
                var grooveGammaCorrection = isNucleotideForward ? GROOVE_GAMME : -GROOVE_GAMME;
                normal = normal.Rotate(grooveGammaCorrection, forward);

                var mod = modMap[helixId][nucleotideComponent.Id];

                OxdnaVector cen; OxdnaVector norm; OxdnaVector forw;
                OxdnaNucleotide oxdnaNucleotide;

                var sequence = new string(nucleotideComponent.Sequence.Reverse().ToArray());
                var sequenceIndex = 0;

                // Handle insertion logic.
                if (nucleotideComponent.IsInsertion)
                {
                    var insertionLength = nucleotideComponent.Insertion;
                    for (int i = 0; i <= insertionLength; i++)
                    {
                        int idx = !isNucleotideForward ? (insertionLength - i) : i;

                        cen = origin + forward * (nucleotideComponent.Id + mod + idx - insertionLength) * RISE_PER_BASE_PAIR * NM_TO_OX_UNITS;
                        norm = normal.Rotate(STEP_ROTATION * (nucleotideComponent.Id + mod + idx - insertionLength), forward);
                        forw = isNucleotideForward ? -forward : forward;
                        oxdnaNucleotide = new OxdnaNucleotide(cen, norm, forw, nucleotideComponent, sequence[sequenceIndex].ToString());
                        oxdnaStrand.Nucleotides.Add(oxdnaNucleotide);
                        sequenceIndex++;
                    }
                    continue;
                }

                cen = origin + forward * (nucleotideComponent.Id + mod) * RISE_PER_BASE_PAIR * NM_TO_OX_UNITS;
                norm = normal.Rotate(STEP_ROTATION * (nucleotideComponent.Id + mod), forward);
                forw = isNucleotideForward ? -forward : forward;
                oxdnaNucleotide = new OxdnaNucleotide(cen, norm, forw, nucleotideComponent, sequence[sequenceIndex].ToString());
                oxdnaStrand.Nucleotides.Add(oxdnaNucleotide);
            }

            _oxdnaStrands.Add(oxdnaStrand);
        }
    }

    /// <summary>
    /// Generates the mod map. Which maps each helices index to a list that for each index determines how much a nucletide is
    /// adjusted based on the number of insertions and deletions on a helix.
    /// </summary>
    private Dictionary<int, List<int>> GetModMap()
    {
        // Initialize the modification map
        var modMap = new Dictionary<int, List<int>>();
        foreach (KeyValuePair<int, Helix> keyValuePair in s_helixDict)
        {
            modMap[keyValuePair.Key] = new List<int>(new int[keyValuePair.Value.Length]);
        }

        // Insert each insertion/deletion as a positive/negative number
        foreach (var strand in s_strandDict.Values)
        {
            foreach ((int, int, NucleotideComponent) insertion in strand.Insertions)
            {
                var nucleotide = insertion.Item3;
                var helixId = nucleotide.HelixId;
                var insertionLength = insertion.Item2;
                modMap[helixId][nucleotide.Id] = insertionLength;
            }
            foreach ((int, NucleotideComponent) deletion in strand.Deletions)
            {
                var nucleotide = deletion.Item2;
                var helixId = nucleotide.HelixId;
                modMap[helixId][nucleotide.Id] = -1;
            }
        }

        // Propagate the modifier so it stays consistent across domains
        foreach (KeyValuePair<int, Helix> keyValuePair in s_helixDict)
        {
            var helixId = keyValuePair.Key;
            var helix = keyValuePair.Value;

            for (int i = 1; i < helix.Length; i++)
            {
                modMap[helixId][i] += modMap[helixId][i - 1];
            }
        }

        return modMap;
    }

    /// <summary>
    /// Generates the helix vectors (origin, forward, and normal) for constructing calculating nucleotide positions.
    /// </summary>
    private (OxdnaVector origin, OxdnaVector forward, OxdnaVector normal) GetHelixVectors(Helix helix)
    {
        /*
        Returns a tuple (origin, forward, normal)
            origin  -- the starting point of the center of a helix, assumed to be at offset 0
            forward -- the direction in which the helix propagates
            normal  -- a direction perpendicular to forward representing the angle to the backbone at offset 0.
        */

        // Principal axes for computing rotation
        // See: https://en.wikipedia.org/wiki/Aircraft_principal_axes
        var yawAxis = new OxdnaVector(0, 1, 0);    // (0, 1, 0)
        var pitchAxis = new OxdnaVector(1, 0, 0); // (1, 0, 0)
        var rollAxis = new OxdnaVector(0, 0, 1); // (0, 0, 1)

        // Extract roll, pitch, and yaw from helix transform
        var roll = helix.GridComponent.transform.eulerAngles.z;
        var pitch = helix.GridComponent.transform.eulerAngles.x;
        var yaw = helix.GridComponent.transform.eulerAngles.y;

        // Apply rotations in the order: yaw -> pitch -> roll
        pitchAxis = pitchAxis.Rotate(-yaw, yawAxis);
        rollAxis = rollAxis.Rotate(-yaw, yawAxis);

        yawAxis = yawAxis.Rotate(pitch, pitchAxis);
        rollAxis = rollAxis.Rotate(pitch, pitchAxis);

        yawAxis = yawAxis.Rotate(-roll, rollAxis);

        // By convention, forward is the same as the roll axis, but negating to account for nanovr helix extension direction.
        // Normal is the negated yaw axis
        var forward = -rollAxis;
        var normal = -yawAxis;
        
        var helixNativePosition = helix._gridComponent.transform.position;

        // Convert position to oxDNA units.
        var origin = new OxdnaVector(helixNativePosition.x, helixNativePosition.y, helixNativePosition.z) * SCALE_FROM_NANOVR_TO_NM * NM_TO_OX_UNITS;

        return (origin, forward, normal);
    }

    /// <summary>
    /// Computes the bounding box to fit the origami(s) of this oxDNA system.
    /// </summary>
    /// <param name="cubic"></param>
    /// <returns></returns>
    private OxdnaVector ComputeBoundingBox(bool cubic = true)
    {
        OxdnaVector minVec = null;
        OxdnaVector maxVec = null;

        foreach (var oxdnaStrand in _oxdnaStrands)
        {
            foreach (var oxdnaNucleotide in oxdnaStrand.Nucleotides)
            {
                if (minVec == null)
                {
                    minVec = oxdnaNucleotide.Center;
                    maxVec = oxdnaNucleotide.Center;
                }
                else
                {
                    minVec = minVec.CoordMin(oxdnaNucleotide.Center);
                    maxVec = maxVec.CoordMax(oxdnaNucleotide.Center);
                }
            }
        }

        if (minVec != null && maxVec != null)
        {
            // 5 is arbitrarily chosen so that the box has a bit of wiggle room
            // 1.5 multiplier is to make all crossovers appear (advice from Oxdna authors)
            var box = (maxVec - minVec + new OxdnaVector(5, 5, 5)) * 1.5;
            if (cubic)
            {
                var maxSide = Math.Max(box.X, Math.Max(box.Y, box.Z));
                box = new OxdnaVector(maxSide, maxSide, maxSide);
            }
            return box;
        }
        return new OxdnaVector(1, 1, 1);
    }

    /// <summary>
    /// Generates the oxDNA files (.dat and .top) based on this oxDNA system.
    /// </summary>
    public (string topFile, string datFile, OxDNAMapper oxDNAMapper) OxDNAFiles()
    {
        StringBuilder topFileStringBuilder = new StringBuilder();
        StringBuilder datFileStringBuilder = new StringBuilder();

        OxdnaVector boundingBox = ComputeBoundingBox();

        // Write dat file metadata
        datFileStringBuilder.Append("t = 0" + Environment.NewLine);
        datFileStringBuilder.Append($"b = {boundingBox.X} {boundingBox.Y} {boundingBox.Z}" + Environment.NewLine);
        datFileStringBuilder.Append("E = 0 0 0" + Environment.NewLine);

        OxDNAMapper oxDNAMapper = new OxDNAMapper();

        int nucleotideCount = 0;
        int strandCount = 0;
        // Line number will increase as the nucleotides are parsed.
        int lineIndex = 0;

        foreach (OxdnaStrand oxdnaStrand in _oxdnaStrands)
        {
            strandCount++;

            int nucleotideIndex = 0;
            foreach (OxdnaNucleotide oxdnaNucleotide in oxdnaStrand.Nucleotides)
            {
                int n5 = nucleotideCount - 1;
                int n3 = nucleotideCount + 1;
                nucleotideCount++;

                if (nucleotideIndex == 0) n5 = -1;
                if (nucleotideIndex == oxdnaStrand.Nucleotides.Count - 1) n3 = -1;
                nucleotideIndex++;

                topFileStringBuilder.Append($"{strandCount} {oxdnaNucleotide.Base} {n3} {n5}" + Environment.NewLine);
                datFileStringBuilder.Append($"{oxdnaNucleotide.R.X:F16} {oxdnaNucleotide.R.Y:F16} {oxdnaNucleotide.R.Z:F16} " +
                                            $"{oxdnaNucleotide.B.X:F16} {oxdnaNucleotide.B.Y:F16} {oxdnaNucleotide.B.Z:F16} " +
                                            $"{oxdnaNucleotide.N.X:F16} {oxdnaNucleotide.N.Y:F16} {oxdnaNucleotide.N.Z:F16} " +
                                            $"{oxdnaNucleotide.V.X} {oxdnaNucleotide.V.Y} {oxdnaNucleotide.V.Z} " +
                                            $"{oxdnaNucleotide.L.X} {oxdnaNucleotide.L.Y} {oxdnaNucleotide.L.Z} " +
                                            Environment.NewLine); // F16's add precision to the file writes. Last two values should be zero.

                // Map native nucleotide to line number in .dat file.
                oxDNAMapper.Add(lineIndex, oxdnaNucleotide.Nucleotide);
                lineIndex++;
            }
        }

        // Add top file metadata
        topFileStringBuilder.Insert(0, $"{nucleotideCount} {strandCount}" + Environment.NewLine);

        return (topFileStringBuilder.ToString(), datFileStringBuilder.ToString(), oxDNAMapper);
    } 

    /// <summary>
    /// Generates the oxview file based on this oxDNA system.
    /// </summary>
    public string OxViewFile()
    {
        var oxViewFile = new OxViewFile
        {
            Date = DateTime.UtcNow.ToString("o")
        };

        var boundingBox = ComputeBoundingBox();
        var serializedBoundingBox = new List<double>
        {
            boundingBox.X,
            boundingBox.Y,
            boundingBox.Z
        };
        oxViewFile.Box = serializedBoundingBox;

        var oxViewSystem = new OxViewSystem { Id = 0 };
        oxViewFile.Systems.Add(oxViewSystem);

        int nucleotideId = 0; // Num nucleotides
        int strandId = 0; // Num strands

        // Using reference types as keys, but nucleotide components won't get destroyed when writing file.
        var nucleotideCompToOxviewIndex = new Dictionary<NucleotideComponent, int>();
        var oxviewIndexToNucleotideComp = new Dictionary<int, NucleotideComponent>();

        // Scaffold Strand Id
        int scaffholdStrandId = -1;

        foreach (var oxdnaStrand in _oxdnaStrands)
        {
            var oxViewStrand = new OxViewStrand
            {
                Id = ++strandId,
                Class = "NucleicAcidStrand",
                End5 = nucleotideId, // Start nucleotide index
            };

            if (oxdnaStrand.IsScaffold)
            {
                scaffholdStrandId = oxViewStrand.Id;
            }

            // Convert Unity Color to OxView format
            int strandColor = ((int)(oxdnaStrand.Color.r * 255) << 16) // Red shifted by 16 bits
                            | ((int)(oxdnaStrand.Color.g * 255) << 8)  // Green shifted by 8 bits
                            | (int)(oxdnaStrand.Color.b * 255);        // Blue component

            for (int i = oxdnaStrand.Nucleotides.Count - 1; i >= 0; i--)
            {
                var nucleotide = oxdnaStrand.Nucleotides[i];

                var oxViewMonomer = new OxViewMonomer
                {
                    Id = nucleotideId,
                    Type = nucleotide.Base,
                    Class = "DNA",
                    P = new List<double> { nucleotide.R.X, nucleotide.R.Y, nucleotide.R.Z },
                    A1 = new List<double> { nucleotide.B.X, nucleotide.B.Y, nucleotide.B.Z },
                    A3 = new List<double> { nucleotide.N.X, nucleotide.N.Y, nucleotide.N.Z },
                    Cluster = 1,
                    Color = strandColor,
                };

                // Link nucleotides
                if (oxViewStrand.Monomers.Count > 0)
                {
                    oxViewMonomer.N5 = nucleotideId - 1;
                    oxViewStrand.Monomers[oxViewStrand.Monomers.Count - 1].N3 = nucleotideId;
                }

                nucleotideCompToOxviewIndex.Add(nucleotide.Nucleotide, nucleotideId);
                oxviewIndexToNucleotideComp.Add(nucleotideId, nucleotide.Nucleotide);

                oxViewStrand.Monomers.Add(oxViewMonomer);
                nucleotideId++;
            }

            oxViewStrand.End3 = nucleotideId; // End nucleotide index
            oxViewSystem.Strands.Add(oxViewStrand);
        }

        // Assign the bp values of the monomers
        foreach (var oxViewStrand in oxViewSystem.Strands)
        {
            foreach (var oxViewMonomer in oxViewStrand.Monomers)
            {
                int id = oxViewMonomer.Id;

                var nucleotideComp = oxviewIndexToNucleotideComp[id];
                var nucleotideCompComplement = nucleotideComp.Complement.GetComponent<NucleotideComponent>();

                var complementId = nucleotideCompToOxviewIndex[nucleotideCompComplement];
                oxViewMonomer.Bp = complementId;
            }
        }

        return JsonConvert.SerializeObject(
            oxViewFile,
            new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        ProcessDictionaryKeys = true,
                        OverrideSpecifiedNames = true
                    }
                },
                NullValueHandling = NullValueHandling.Ignore, // Properties that are null won't get serialized
                Converters = new List<JsonConverter>
                {
                    new FloatPrecisionConverter()
                }
            }
        );
    }
}

class OxdnaVector
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public OxdnaVector(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double Dot(OxdnaVector other)
    {
        return X * other.X + Y * other.Y + Z * other.Z;
    }

    public OxdnaVector Cross(OxdnaVector other)
    {
        double xc = Y * other.Z - Z * other.Y;
        double yc = Z * other.X - X * other.Z;
        double zc = X * other.Y - Y * other.X;
        return new OxdnaVector(xc, yc, zc);
    }

    public double Length()
    {
        return Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    public OxdnaVector Normalize()
    {
        double len = Length();
        if (len == 0) throw new InvalidOperationException("Cannot normalize a zero-length vector.");
        return new OxdnaVector(X / len, Y / len, Z / len);
    }

    public OxdnaVector CoordMin(OxdnaVector other)
    {
        return new OxdnaVector(Math.Min(X, other.X), Math.Min(Y, other.Y), Math.Min(Z, other.Z));
    }

    public OxdnaVector CoordMax(OxdnaVector other)
    {
        return new OxdnaVector(Math.Max(X, other.X), Math.Max(Y, other.Y), Math.Max(Z, other.Z));
    }

    public static OxdnaVector operator +(OxdnaVector a, OxdnaVector b)
    {
        return new OxdnaVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static OxdnaVector operator -(OxdnaVector a, OxdnaVector b)
    {
        return new OxdnaVector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static OxdnaVector operator *(OxdnaVector vector, double scalar)
    {
        return new OxdnaVector(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    }

    public static OxdnaVector operator -(OxdnaVector vector)
    {
        return new OxdnaVector(-vector.X, -vector.Y, -vector.Z);
    }

    public OxdnaVector Rotate(double angle, OxdnaVector axis)
    {
        var u = axis.Normalize();
        double c = Math.Cos(angle * Math.PI / 180);
        double s = Math.Sin(angle * Math.PI / 180);

        var uCrossThis = u.Cross(this);
        return u * Dot(u) + (uCrossThis * c).Cross(u) - (uCrossThis * s);
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }
}

class OxdnaNucleotide
{
    private static readonly OxdnaVector OxdnaOrigin = new OxdnaVector(0, 0, 0);

    public OxdnaVector Center { get; }
    public OxdnaVector Normal { get; }
    public OxdnaVector Forward { get; }
    public string Base { get; }
    public NucleotideComponent Nucleotide { get; }

    // Velocity and angular velocity for oxDNA conf file
    public OxdnaVector V { get; }
    public OxdnaVector L { get; }

    public OxdnaNucleotide(OxdnaVector center, OxdnaVector normal, OxdnaVector forward, NucleotideComponent nucleotide, string @base)
    {
        Center = center;
        Normal = normal;
        Forward = forward;
        Nucleotide = nucleotide;
        Base = @base;
        V = OxdnaOrigin;
        L = OxdnaOrigin;
    }

    public OxdnaVector R => Center - B * BASE_DIST;

    public OxdnaVector B => -Normal;

    public OxdnaVector N => Forward;
}

class OxdnaStrand
{
    public List<OxdnaNucleotide> Nucleotides { get; private set; } = new List<OxdnaNucleotide>();
    public Color Color { get; set; }
    public bool IsScaffold { get; set; }

    public OxdnaStrand() { }
}

/// <summary>
/// Custom converter for higher precision when serializing floats or ooubles.
/// </summary>
public class FloatPrecisionConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(float) || objectType == typeof(double);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is float floatValue)
        {
            writer.WriteRawValue(floatValue.ToString("R", CultureInfo.InvariantCulture)); // Full precision
        }
        else if (value is double doubleValue)
        {
            writer.WriteRawValue(doubleValue.ToString("R", CultureInfo.InvariantCulture)); // Full precision
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException("This converter is for writing only.");
    }
}

