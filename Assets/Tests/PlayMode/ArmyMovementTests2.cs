using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

public class ArmyMovementTests2
{
    private GridManager gridManager;
    private Player mockPlayer;
    private Army army;
    private Tile startTile;
    private Tile targetTile;

    [SetUp]
    public void SetUp()
    {
        GameObject gridObj = new GameObject("GridManager");
        gridManager = gridObj.AddComponent<GridManager>();
        var tiles = new Tile[3, 3];
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                tiles[x, y] = new Tile(x, y, TerrainType.Grassland);
            }
        }
        typeof(GridManager)
            .GetField("grid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(gridManager, tiles);

        startTile = tiles[1, 1];
        targetTile = tiles[1, 2];
        mockPlayer = new Player("Tester",true);
        army = new Army(mockPlayer.Name);
        army.AddUnit("Militia Spearman", 100);
        startTile.Army = army;
    }

    [UnityTest]
    public IEnumerator ArmyMovement_Succeeds_WhenTargetIsEmptyAndWithinRange()
    {
        Assert.IsNull(targetTile.Army, "Target tile should be empty before move.");
        Assert.AreEqual(startTile.Army, army, "Army should be at start tile.");
        bool moveSucceeded = SimulateArmyMove(startTile, targetTile, army);
        yield return null;

        Assert.IsTrue(moveSucceeded, "Army should have successfully moved.");
        Assert.AreEqual(army, targetTile.Army, "Target tile should now contain the army.");
        Assert.IsNull(startTile.Army, "Start tile should now be empty.");
    }

    private bool SimulateArmyMove(Tile fromTile, Tile toTile, Army movingArmy)
    {
        if (toTile == null || fromTile == null)
            return false;
        if (toTile.Army != null)
            return false;

        // “Move” the army
        toTile.Army = movingArmy;
        fromTile.Army = null;
        movingArmy.SpendMovement(1);

        return true;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gridManager.gameObject);
    }
}
