/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Imports PDB files to support space-filling protein models.
/// </summary>
public class PDBImport : MonoBehaviour
{
    // Ångström to Unity coordinate scale
    private static float scaleFactor = 1 / Utils.ATOM_SCALE / Utils.SCALE_FROM_NANOVR_TO_NM;

    // Atom radii (van der Waals radii in Ångströms)
    private static readonly Dictionary<string, float> AtomRadii = new Dictionary<string, float>
    {
        { "H", 1.1f },  // Hydrogen
        { "C", 1.7f },  // Carbon
        { "N", 1.55f }, // Nitrogen
        { "O", 1.52f }, // Oxygen
        { "S", 1.8f },  // Sulfur
        { "P", 1.8f },  // Phosphorus
        { "Unknown", 1.5f },
    };

    // Atom colors (CPK color scheme)
    private static readonly Dictionary<string, Color> AtomColors = new Dictionary<string, Color>
    {
        { "H", Color.white },
        { "C", Color.gray },
        { "N", Color.blue },
        { "O", Color.red },
        { "S", Color.yellow },
        { "P", new Color(1.0f, 0.647f, 0.0f) }, // Orange
        { "Unknown", Color.magenta },
    };

    /// <summary>
    /// Parses through PDB file and generates space-filling model.
    /// </summary>
    /// <param name="filePath">Path of PDB file</param>
    public static void ParseAndVisualizePDB(string filePath)
    {
        List<GameObject> atoms = new List<GameObject>();
        string proteinName = ExtractProteinName(filePath);
        Vector3 positionOffset = Vector3.zero;

        using (StreamReader reader = new StreamReader(filePath))
        {
            GameObject parent = new GameObject();
            Transform transform = parent.transform;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Only process ATOM and HETATM lines
                if (line.StartsWith("ATOM") || line.StartsWith("HETATM"))
                {
                    string atomType = line.Substring(76, 2).Trim(); // Atom name (columns 77-78)
                    if (!AtomRadii.ContainsKey(atomType))
                    {
                        Debug.LogWarning($"Unknown atom type: {atomType}. Using default values.");
                        atomType = "Unknown";
                    };

                    // Parse atom position (columns 31-54)
                    float x = float.Parse(line.Substring(30, 8)) * scaleFactor;
                    float y = float.Parse(line.Substring(38, 8)) * scaleFactor;
                    float z = float.Parse(line.Substring(46, 8)) * scaleFactor;
                    Vector3 atomPosition = new Vector3(x, y, z);
                    GameObject atom;


                    // Positions protein around user
                    if (positionOffset == Vector3.zero)
                    {
                        Vector3 startPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.5f;
                        positionOffset = startPosition - atomPosition;
                        atom = CreateAtomSphere(startPosition, atomType, transform, proteinName);
                    }
                    else
                    {
                        atom = CreateAtomSphere(atomPosition + positionOffset, atomType, transform, proteinName);
                    }

                    atoms.Add(atom);
                }
            }
            StaticBatchingUtility.Combine(atoms.ToArray(), parent);
            GlobalVariables.proteins.Add(parent);
        }
    }

    private static string ExtractProteinName(string filePath)
    {
        string proteinName = "Unknown Protein";

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Look for "COMPND" record with "MOLECULE:"
                if (line.StartsWith("COMPND") && line.Contains("MOLECULE:"))
                {
                    int startIndex = line.IndexOf("MOLECULE:") + "MOLECULE:".Length;
                    proteinName = line.Substring(startIndex).Trim().Trim(';');
                    return proteinName;
                }

                // Fall back to the "HEADER" record
                if (line.StartsWith("HEADER"))
                {
                    proteinName = line.Substring(10, 40).Trim();
                    return proteinName;
                }
            }
        }

        return proteinName;
    }

    /// <summary>
    /// Creates sphere GameObject to represent an atom.
    /// </summary>
    /// <param name="position">3D position of atom</param>
    /// <param name="atomType">Periodic element</param>
    /// <param name="parent">Empty GameObject to group all atoms of a protein together</param>
    private static GameObject CreateAtomSphere(Vector3 position, string atomType, Transform parent, string proteinName)
    {
        // Create a sphere
        GameObject sphere = Instantiate(GlobalVariables.Atom,
                    position,
                    Quaternion.identity);
        sphere.transform.localScale = AtomRadii[atomType] * scaleFactor * Vector3.one;
        sphere.name = "Protein: " + proteinName;

        // Set atom color
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (renderer != null && AtomColors.ContainsKey(atomType))
        {
            renderer.material.color = AtomColors[atomType];
        }

        sphere.transform.parent = parent;
        return sphere;
    }
}

