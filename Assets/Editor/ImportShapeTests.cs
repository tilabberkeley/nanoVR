
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using static GlobalVariables;

/// <summary>
/// Tests the ImportShapes file.
/// </summary>
public class ImportShapeTests
{
    /// <summary>
    /// Tests import square button in scene.
    /// </summary>
    [Test]
    public async void ImportSquareTest()
    {
        await ImportShapes.Instance.ImportSquare();
        Assert.AreEqual(1, s_gridDict.Count);
        Assert.AreEqual(88, s_helixDict.Count);
        Assert.AreEqual(219, s_strandDict.Count);
    }

    /// <summary>
    /// Tets import triangle button in scene.
    /// </summary>
    [Test]
    public async void ImportTriangleTest()
    {
        await ImportShapes.Instance.ImportTriangle();
        Assert.AreEqual(1, s_gridDict.Count);
        Assert.AreEqual(40, s_helixDict.Count);
        Assert.AreEqual(206, s_strandDict.Count);
    }

    /// <summary>
    /// Tests import 6-helix honeycomb button in scene.
    /// </summary>
    [Test]
    public async void Import6HBTest()
    {
        await ImportShapes.Instance.Import6HB();
        Assert.AreEqual(1, s_gridDict.Count);
        Assert.AreEqual(6, s_helixDict.Count);
        Assert.AreEqual(25, s_strandDict.Count);
    }
}
