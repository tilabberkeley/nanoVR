using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static GlobalVariables;

public class CreateHelixTests
{
    [SetUp]
    public void SetUp()
    {
        GlobalVariables.s_numGrids = 1;
        GlobalVariables.s_gridDict.Clear();
        GlobalVariables.s_gridCopies.Clear();
        s_numHelices = 0;
        s_helixDict.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        GlobalVariables.s_numGrids = 1;
        GlobalVariables.s_gridDict.Clear();
        GlobalVariables.s_gridCopies.Clear();
        s_numHelices = 0;
        s_helixDict.Clear();
    }

    [Test]
    public void EmptyTest()
    {
        Assert.AreEqual(0, s_helixDict.Count);
    }

    [Test]
    public async void SingleHelixTest()
    {
        DNAGrid grid = DrawGrid.CreateGrid("1", "XY", new Vector3(0, 0, 0), "square");
        Helix helix = grid.AddHelix(0, grid.Grid2D[0, 0].Position, 64, "XY", grid.Grid2D[0, 0]);
        await helix.ExtendAsync(64);
        Assert.AreEqual(1, s_helixDict.Count);
        Assert.AreEqual(64, helix.Length);
        Assert.AreEqual(0, helix.Id);
        Assert.AreEqual("1", helix.GridId);
        Assert.AreEqual(grid.Grid2D[0, 0], helix.GridComponent);
    }

    [Test]
    public void HelixCommandTest()
    {
        DNAGrid grid = DrawGrid.CreateGrid("1", "XY", new Vector3(0, 0, 0), "square");
        ICommand command = new CreateHelixCommand(0, grid.Grid2D[0, 0].Position, 64, "XY", grid.Grid2D[0, 0], grid);
        s_helixDict.TryGetValue(0, out Helix helix);
        Assert.AreEqual(1, s_helixDict.Count);
        Assert.AreEqual(64, helix.Length);
        Assert.AreEqual(0, helix.Id);
        Assert.AreEqual("1", helix.GridId);
        Assert.AreEqual(grid.Grid2D[0, 0], helix.GridComponent);
        Assert.AreEqual(helix, grid.Grid2D[0, 0].Helix);

        // Undo command
        command.Undo();
        Assert.AreEqual(0, s_helixDict.Count);
        Assert.AreEqual(null, grid.Grid2D[0, 0].Helix);

        // Redo command
        command.Redo();
        s_helixDict.TryGetValue(0, out Helix helix2);
        Assert.AreEqual(1, s_helixDict.Count);
        Assert.AreEqual(64, helix2.Length);
        Assert.AreEqual(0, helix2.Id);
        Assert.AreEqual("1", helix2.GridId);
        Assert.AreEqual(grid.Grid2D[0, 0], helix2.GridComponent);
        Assert.AreEqual(helix2, grid.Grid2D[0, 0].Helix);
    }
}
