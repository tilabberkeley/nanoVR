using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class CreateGridTest
{
    [SetUp]
    public void SetUp()
    {
        GlobalVariables.s_numGrids = 1;
        GlobalVariables.s_gridDict.Clear();
        GlobalVariables.s_gridCopies.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        GlobalVariables.s_numGrids = 1;
        GlobalVariables.s_gridDict.Clear();
        GlobalVariables.s_gridCopies.Clear();
    }

    /// <summary>
    /// Tests no Grids in scene.
    /// </summary>
    [Test]
    public void EmptyTest()
    {
        Assert.AreEqual(1, GlobalVariables.s_numGrids);
        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_gridCopies.Count);
        Assert.AreEqual(0, GlobalVariables.s_gridDict.Count);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);
    }

    /// <summary>
    /// Tests creating one grid and its properties.
    /// </summary>
    [Test]
    public void CreateOneGridTest()
    {
        DNAGrid grid = DrawGrid.CreateGrid("1", "XY", new Vector3(0, 0, 0), "square");
        Assert.AreEqual(2, GlobalVariables.s_numGrids);
        Assert.AreEqual(new Vector3(0, 0, 0), grid.Position);
        Assert.AreEqual("1", grid.Id);
        Assert.AreEqual("square", grid.Type);
        Assert.AreEqual("XY", grid.Plane);
        Assert.AreEqual(5, grid.Width);
        Assert.AreEqual(5, grid.Length);
        Assert.AreEqual(grid, GlobalVariables.s_gridDict["1"]);
        Assert.AreEqual(0, GlobalVariables.s_gridCopies["1"]);
        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);
    }

    /// <summary>
    /// Tests creating multiple grids and their properties.
    /// </summary>
    [Test]
    public void CreateMultipleGridsTest()
    {
        for (int i = 1; i <= 10; i++)
        {
            DNAGrid grid = DrawGrid.CreateGrid(i.ToString(), "YZ", new Vector3(0, 0, 0), "honeycomb");
            Assert.AreEqual(i + 1, GlobalVariables.s_numGrids);
            Assert.AreEqual(new Vector3(0, 0, 0), grid.Position);
            Assert.AreEqual(i.ToString(), grid.Id);
            Assert.AreEqual("honeycomb", grid.Type);
            Assert.AreEqual("YZ", grid.Plane);
            Assert.AreEqual(5, grid.Width);
            Assert.AreEqual(5, grid.Length);
            Assert.AreEqual(grid, GlobalVariables.s_gridDict[i.ToString()]);
            Assert.AreEqual(0, GlobalVariables.s_gridCopies[i.ToString()]);
        }

        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);
    }

    /// <summary>
    /// Tests clicking NewGridButton in scene creates a new Grid.
    /// </summary>
    [Test]
    public void NewGridButtonTest()
    {
        GameObject newGridBtn = GameObject.Find("GridBut");
        newGridBtn.GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(2, GlobalVariables.s_numGrids);
        Assert.IsTrue(GlobalVariables.s_gridDict.ContainsKey("1"));
        Assert.AreEqual(0, GlobalVariables.s_gridCopies["1"]);
        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);
    }

    /// <summary>
    /// Tests create Grid command and its undo/redo actions.
    /// </summary>
    [Test]
    public void CreateGridCommandTest()
    {
        ICommand command = new CreateGridCommand("1", "XY", new Vector3(0, 0, 0), "square");
        CommandManager.AddCommand(command);
        Assert.AreEqual(2, GlobalVariables.s_numGrids);
        Assert.IsTrue(GlobalVariables.s_gridDict.ContainsKey("1"));
        Assert.AreEqual(0, GlobalVariables.s_gridCopies["1"]);
        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);

        // Undo create Grid and check
        command.Undo();
        Assert.AreEqual(2, GlobalVariables.s_numGrids);
        Assert.AreEqual(0, GlobalVariables.s_gridDict.Count);
        Assert.AreEqual(0, GlobalVariables.s_gridCopies.Count);
        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);

        // Redo create Grid and check
        command.Redo();
        Assert.AreEqual(3, GlobalVariables.s_numGrids);
        Assert.IsTrue(GlobalVariables.s_gridDict.ContainsKey("1"));
        Assert.AreEqual(0, GlobalVariables.s_gridCopies["1"]);
        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);
    }

    /// <summary>
    /// Tests delete Grid command and its undo/redo actions.
    /// </summary>
    [Test]
    public void DeleteGridCommandTest()
    {

    }

    /// <summary>
    /// Tests delete Grid when empty Helix exists works.
    /// </summary>
    [Test]
    public void DeleteGridWithEmptyHelixTest()
    {
        DNAGrid grid = DrawGrid.CreateGrid("1", "XY", new Vector3(0, 0, 0), "square");
        grid.AddHelix(0, grid.Grid2D[0, 0].Position, 64, "XY", grid.Grid2D[0, 0]);
        grid.DeleteGrid();
        Assert.AreEqual(2, GlobalVariables.s_numGrids);
        Assert.AreEqual(0, GlobalVariables.s_gridDict.Count);
        Assert.AreEqual(0, GlobalVariables.s_gridCopies.Count);
        Assert.AreEqual(1, GlobalVariables.s_numVisGrids);
        Assert.AreEqual(0, GlobalVariables.s_visGridDict.Count);
    }

    /// <summary>
    /// Tests delete Grid when non-empty Helix exists doesn't work.
    /// </summary>
    [Test]
    public void DeleteGridWithNonEmptyHelixTest()
    {
        
    }
}
