using UnityEngine;
using System;

public class NucleotideColliderComponent : MonoBehaviour
{
    // Which Helix this collider is associated with
    private Helix helixRef;
    private int nucleotideId;
    private int direction;  // 0 or 1

    // If you want to store the NucleotideData for convenience,
    // you can store it once in Setup():
    private NucleotideData data;

    public Helix HelixRef { get => helixRef; }
    public int NucleotideId { get => nucleotideId; }
    public int Direction { get => direction; }
    public NucleotideData Data { get => data; }

    /// <summary>
    /// Initialize the collider with references so we know
    /// which instance it corresponds to.
    /// </summary>
    public void Setup(Helix helix, int nucleotideId, int direction)
    {
        this.helixRef = helix;
        this.nucleotideId = nucleotideId;
        this.direction = direction;

        // Optionally fetch the data once for quick access later
        this.data = helix.GetNucleotideData(nucleotideId, direction);
    }
}
