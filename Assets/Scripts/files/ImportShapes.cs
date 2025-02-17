/*
 * nanoVR, a VR application for DNA nanostructures.
 * author: David Yang <davidmyang@berkeley.edu> and Oliver Petrick <odpetrick@berkeley.edu>
 */
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Imports pre-defined shapes from scadnano files
/// </summary>
public class ImportShapes : MonoBehaviour
{
    public static ImportShapes Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void ImportSquare()
    {
        ImportShape(GlobalVariables.SQUARE_SC);
    }
    public void ImportTriangle()
    {
        ImportShape(GlobalVariables.TRIANGLE_SC);
    }
    public void Import6HB()
    {
        ImportShape(GlobalVariables.SIXHB_SC);
    }


    private void ImportShape(string fileContent)
    {
        FileImport.Instance.ParseSC(fileContent);
    }
}
