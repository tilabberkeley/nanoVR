using System.Collections.Generic;

/// <summary>
/// Object to represent oxview json file format. Serialized this object to get the oxview file format.
/// </summary>
public class OxViewFile
{
    public List<double> Box { get; set; } = new List<double>();
    public string Date { get; set; } = "";
    public List<OxViewSystem> Systems { get; set; } = new List<OxViewSystem>();
    public List<int> Forces { get; set; } = new List<int>();
    public List<int> Selections { get; set; } = new List<int>();

}

public class OxViewSystem
{
    public int Id { get; set; }
    public List<OxViewStrand> Strands { get; set; } = new List<OxViewStrand>();
}

public class OxViewStrand
{
    public int Id { get; set; }
    public int End3 { get; set; }
    public int End5 { get; set; }
    public string Class { get; set; } = "";
    public List<OxViewMonomer> Monomers { get; set; } = new List<OxViewMonomer>();
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
    public int? Bp { get; set; }
    public int Cluster { get; set; }
    public int Color { get; set; }
}
