using UnityEngine;

public class NucleotideColliderComponent : MonoBehaviour
{
    // Which Helix this collider is associated with
    [HideInInspector] public Helix HelixRef;
    [HideInInspector] public int NucleotideID;
    [HideInInspector] public int Direction;  // 0 or 1

    // If you want to store the NucleotideData for convenience,
    // you can store it once in Setup():
    [HideInInspector] public NucleotideData Data;

    /// <summary>
    /// Initialize the collider with references so we know
    /// which instance it corresponds to.
    /// </summary>
    public void Setup(Helix helix, int nucleotideID, int direction)
    {
        HelixRef = helix;
        NucleotideID = nucleotideID;
        Direction = direction;

        // Optionally fetch the data once for quick access later
        Data = helix.GetNucleotideData(nucleotideID, direction);
    }
}
