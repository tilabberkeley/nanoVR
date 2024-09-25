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

    public async void ImportSquare()
    {
        await ImportShape(GlobalVariables.SQUARE_SC);
    }
    public async void ImportTriangle()
    {
        await ImportShape(GlobalVariables.TRIANGLE_SC);
    }
    public async void Import6HB()
    {
        await ImportShape(GlobalVariables.SIXHB_SC);
    }


    private async Task ImportShape(string fileContent)
    {
        await FileImport.Instance.ParseSC(fileContent);
    }
}
