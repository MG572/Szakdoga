using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

public class ArmyMovementTests
{
    private GridManager gridManager;
    private ClickManager clickManager;
    private Player mockPlayer;
    private Army army;
    private Tile startTile;
    private Tile targetTile;

    [SetUp]
    public void SetUp()
    {
        var gridObj = new GameObject("GridManager");
        gridManager = gridObj.AddComponent<GridManager>();
        gridManager.armyPrefab = new GameObject("ArmyPrefab");
        gridManager.settlementPrefab = new GameObject("SettlementPrefab");
        typeof(GridManager)
            .GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
            ?.SetValue(null, gridManager);

        var tiles = new Tile[3, 3];
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                tiles[x, y] = new Tile(x, y, TerrainType.Grassland);

        typeof(GridManager)
            .GetField("grid", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(gridManager, tiles);

        startTile = tiles[1, 1];
        targetTile = tiles[1, 2];
        mockPlayer = new Player("Tester", true);
        army = new Army(mockPlayer.Name);
        army.AddUnit("Militia Spearman", 100);
        startTile.Army = army;
        startTile.ArmyObject = new GameObject("ArmyObject");
        startTile.ArmyObject.AddComponent<ClickableObject>().linkedTile = startTile;
        var clickObj = new GameObject("ClickManager");
        clickObj.SetActive(false);
        clickManager = clickObj.AddComponent<ClickManager>();
        clickManager.highlightPrefab = new GameObject("HighlightPrefab");

        typeof(ClickManager)
            .GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
            ?.SetValue(null, clickManager);
        clickObj.SetActive(true);
    }

    [UnityTest]
    public IEnumerator MoveArmyToTile_TransfersArmyCorrectly()
    {
        Assert.AreEqual(army, startTile.Army);
        Assert.IsNull(targetTile.Army);

        clickManager.MoveArmyToTile(army, startTile, targetTile);
        yield return null;

        Assert.AreEqual(army, targetTile.Army);
        Assert.IsNull(startTile.Army);

        var clickable = targetTile.ArmyObject.GetComponent<ClickableObject>();
        Assert.AreEqual(targetTile, clickable.linkedTile);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gridManager.gameObject);
        Object.DestroyImmediate(clickManager.gameObject);
    }
}
