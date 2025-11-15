using UnityEngine;
using System.Collections.Generic;

public static class ArmyService
{
    public static Army CreateArmyFromGarrison(Settlement settlement, Player player, List<Unit> unitsToMove)
    {
        if (settlement == null || player == null || unitsToMove == null || unitsToMove.Count == 0)
        {
            Debug.LogWarning("ArmyService Invalid parameters for army creation");
            return null;
        }
        if (player.Armies.Count >= player.Settlements.Count)
        {
            Debug.LogWarning($"ArmyService {player.Name} cannot create new army: army limit reached");
            return null;
        }
        Tile spawnTile = FindAdjacentSpawnTile(settlement.Tile);
        if (spawnTile == null)
        {
            Debug.LogWarning($"ArmyService No valid spawn tile near {settlement.Name}");
            return null;
        }
        Army newArmy = new Army(player.Name);

        foreach (Unit unit in unitsToMove)
        {
            newArmy.AddUnit(unit.Type.Name, unit.Size);
            settlement.Garrison.RemoveUnit(unit);
        }
        player.AddArmy(newArmy);
        spawnTile.Army = newArmy;

        SpawnArmyGameObject(newArmy, spawnTile);

        Debug.Log($"ArmyService Created army for {player.Name} at {spawnTile.GridPosition} with {unitsToMove.Count} units");
        return newArmy;
    }

    private static Tile FindAdjacentSpawnTile(Tile settlementTile)
    {
        if (settlementTile == null) return null;

        Vector2Int pos = settlementTile.GridPosition;
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // East
            new Vector2Int(-1, 0),  // West
            new Vector2Int(0, 1),   // North
            new Vector2Int(0, -1),  // South
            new Vector2Int(1, 1),   // NE
            new Vector2Int(-1, 1),  // NW
            new Vector2Int(1, -1),  // SE
            new Vector2Int(-1, -1)  // SW
        };
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = pos + dir;
            Tile tile = GridManager.Instance.GetTileAt(checkPos.x, checkPos.y);

            if (tile != null && !tile.HasArmy && !tile.HasSettlement)
            {
                return tile;
            }
        }
        return null;
    }

    private static void SpawnArmyGameObject(Army army, Tile tile)
    {
        Vector2Int pos = tile.GridPosition;
        GameObject armyObj = Object.Instantiate(
            GridManager.Instance.armyPrefab,
            new Vector3(pos.x, pos.y , 0),
            Quaternion.identity
        );

        armyObj.transform.localScale = Vector3.one * 0.2f;
        armyObj.tag = "Army";
        tile.ArmyObject = armyObj;

        ClickableObject clickable = armyObj.GetComponent<ClickableObject>();
        if (clickable != null)
        {
            clickable.linkedArmy = army;
            clickable.linkedTile = tile;
        }
    }
}