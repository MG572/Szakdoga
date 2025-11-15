using System.Collections.Generic;
using UnityEngine;

public class AIManager
{
    public static void TakeEconomyTurn(Player ai)
    {
        Debug.Log($"[AI] TakeEconomyTurn called for {ai.Name}");
        if (ai == null) return;

        foreach (Settlement settlement in ai.Settlements)
        {
            if (settlement == null) continue;

            if (!settlement.IsConstructing())
            {
                TryStartConstruction(ai, settlement);
            }

            TryRecruitUnit(ai, settlement);
            TrySpawnArmyFromGarrison(ai, settlement);
        }
    }

    private static void TryStartConstruction(Player ai, Settlement settlement)
    {
        if (settlement.IsConstructing()) return;

        // military strength comparison
        int aiStrength = CalculateTotalMilitaryStrength(ai);
        int playerStrength = CalculateTotalMilitaryStrength(TurnManager.Instance.CurrentPlayer);

        float strengthRatio = playerStrength > 0 ? (float)aiStrength / playerStrength : 2.0f;

        bool militaryAdvantage = strengthRatio > 1.2f; 
        bool militaryThreat = strengthRatio < 0.8f; 

        Debug.Log($"[AI] Strength ratio: {strengthRatio:F2} (AI: {aiStrength}, Player: {playerStrength})");

        if (settlement.FarmLevel < 1 && ai.Gold >= 400)
        {
            StartBuilding(ai, settlement, "Farm", 400, 2);
            return;
        }

        if (settlement.MarketLevel < 1 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Market", 600, 2);
            return;
        }

        if (militaryThreat)
        {
            BuildMilitaryFocused(ai, settlement);
        }
        else if (militaryAdvantage)
        {
            BuildEconomyFocused(ai, settlement);
        }
        else
        {
            BuildBalanced(ai, settlement);
        }
    }

    private static void BuildMilitaryFocused(Player ai, Settlement settlement)
    {
        Debug.Log($"AI Military threat detected, focusing on military buildings");

        if (settlement.BarracksLevel < 1 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Barracks", 600, 2);
            return;
        }

        if (settlement.ArcheryRangeLevel < 1 && ai.Gold >= 650)
        {
            StartBuilding(ai, settlement, "ArcheryRange", 650, 2);
            return;
        }

        if (settlement.StablesLevel < 2 && ai.Gold >= 700)
        {
            StartBuilding(ai, settlement, "Stables", 700, 2);
            return;
        }

        if (settlement.BarracksLevel < 3 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Barracks", 600, 3);
        }
        if (settlement.ArcheryRangeLevel < 3 && ai.Gold >= 650)
        {
            StartBuilding(ai, settlement, "Barracks", 650, 3);
        }
        if (settlement.StablesLevel < 3 && ai.Gold >= 700)
        {
            StartBuilding(ai, settlement, "Barracks", 700, 3);
        }
    }

    private static void BuildEconomyFocused(Player ai, Settlement settlement)
    {
        Debug.Log($"AI Military advantage, investing in economy");
        if (settlement.FarmLevel < 2 && ai.Gold >= 400)
        {
            StartBuilding(ai, settlement, "Farm", 400, 2);
            return;
        }
        if (settlement.MarketLevel < 2 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Market", 600, 2);
            return;
        }
        if (settlement.BarracksLevel < 2 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Barracks", 600, 2);
            return;
        }
        if (settlement.FarmLevel < 3 && ai.Gold >= 400)
        {
            StartBuilding(ai, settlement, "Farm", 400, 2);
        }
        if (settlement.MarketLevel < 3 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Market", 600, 2);
            return;
        }
    }

    private static void BuildBalanced(Player ai, Settlement settlement)
    {
        Debug.Log($"[AI] Balanced situation, mixed development");
        int totalLevels = settlement.BarracksLevel + settlement.ArcheryRangeLevel +
                         settlement.StablesLevel + settlement.FarmLevel + settlement.MarketLevel;

        bool buildEconomy = totalLevels % 2 == 0;

        if (buildEconomy)
        {
            if (settlement.FarmLevel < 2 && ai.Gold >= 400)
            {
                StartBuilding(ai, settlement, "Farm", 400, 2);
                return;
            }
            if (settlement.MarketLevel < 2 && ai.Gold >= 600)
            {
                StartBuilding(ai, settlement, "Market", 600, 2);
                return;
            }
            if (settlement.FarmLevel < 3 && ai.Gold >= 400)
            {
                StartBuilding(ai, settlement, "Farm", 400, 2);
                return;
            }
            if (settlement.MarketLevel < 3 && ai.Gold >= 600)
            {
                StartBuilding(ai, settlement, "Market", 600, 2);
                return;
            }
            if (settlement.BarracksLevel < 3 && ai.Gold >= 600)
            {
                StartBuilding(ai, settlement, "Barracks", 600, 2);
                return;
            }
            if (settlement.ArcheryRangeLevel < 3 && ai.Gold >= 650)
            {
                StartBuilding(ai, settlement, "ArcheryRange", 650, 2);
                return;
            }
            if (settlement.StablesLevel < 3 && ai.Gold >= 700)
            {
                StartBuilding(ai, settlement, "Stables", 700, 2);
            }
        }

        if (settlement.BarracksLevel < 2 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Barracks", 600, 2);
            return;
        }
        if (settlement.ArcheryRangeLevel < 2 && ai.Gold >= 650)
        {
            StartBuilding(ai, settlement, "ArcheryRange", 650, 2);
            return;
        }
        if (settlement.StablesLevel < 2 && ai.Gold >= 700)
        {
            StartBuilding(ai, settlement, "Stables", 700, 2);
        }
        if (settlement.BarracksLevel < 3 && ai.Gold >= 600)
        {
            StartBuilding(ai, settlement, "Barracks", 600, 2);
            return;
        }
        if (settlement.ArcheryRangeLevel < 3 && ai.Gold >= 650)
        {
            StartBuilding(ai, settlement, "ArcheryRange", 650, 2);
            return;
        }
        if (settlement.StablesLevel < 3 && ai.Gold >= 700)
        {
            StartBuilding(ai, settlement, "Stables", 700, 2);
        }
    }

    private static void StartBuilding(Player ai, Settlement settlement, string buildingName, int cost, int time)
    {
        if (!settlement.CanUpgradeBuilding(buildingName, ai.Gold))
        {
            return;
        }
        int actualCost = Settlement.BuildingCosts[buildingName];
        ai.SpendGold(actualCost);
        settlement.BuildingInProgress = buildingName;
        settlement.ConstructionTimeRemaining = time;
        Debug.Log($"[AI] {ai.Name} started building {buildingName} at {settlement.Name} (Cost: {cost})");
    }

    private static int CalculateTotalMilitaryStrength(Player player)
    {
        if (player == null) return 0;

        int totalStrength = 0;

        foreach (Army army in player.Armies)
        {
            if (army != null)
                totalStrength += army.TotalSoldiers();
        }

        foreach (Settlement settlement in player.Settlements)
        {
            if (settlement != null && settlement.Garrison != null)
                totalStrength += settlement.Garrison.TotalSoldiers();
        }

        return totalStrength;
    }

    private static void TryRecruitUnit(Player ai, Settlement settlement)
    {
        Debug.Log($"[AI] TryRecruitUnit called for {settlement.Name}, Gold: {ai.Gold}");
        if (settlement.RecruitmentQueue.Count >= settlement.MaxQueueSize)
        {
            Debug.Log($"AI {settlement.Name} recruitment queue is full");
            return;
        }
        UnitType unitToRecruit = DecideUnitToRecruit(ai, settlement);

        if (unitToRecruit == null)
        {
            Debug.Log($"AI {settlement.Name} no suitable unit to recruit");
            return;
        }

        if (settlement.CanRecruitUnit(unitToRecruit, ai.Gold))
        {
            int cost = UnitDatabase.GetRecruitmentCost(unitToRecruit);
            ai.SpendGold(cost);

            bool added = settlement.AddToRecruitmentQueue(unitToRecruit);
            if (added)
            {
                Debug.Log($"AI {ai.Name} queued {unitToRecruit.Name} at {settlement.Name} for {cost} gold");
            }
        }
        else
        {
            Debug.Log($"AI cannot afford to recruit {unitToRecruit.Name} at {settlement.Name}");
        }
    }

    private static UnitType DecideUnitToRecruit(Player ai, Settlement settlement)
    {
        int barracksUnits = 0;
        int archeryUnits = 0;
        int stablesUnits = 0;

        foreach (Unit unit in settlement.Garrison.Units)
        {
            string unitName = unit.Type.Name;
            if (UnitDatabase.GetUnitsForBuilding("Barracks", 3).Contains(unitName))
                barracksUnits++;
            else if (UnitDatabase.GetUnitsForBuilding("Archery Range", 3).Contains(unitName))
                archeryUnits++;
            else if (UnitDatabase.GetUnitsForBuilding("Stables", 3).Contains(unitName))
                stablesUnits++;
        }
        foreach (UnitType queuedUnit in settlement.RecruitmentQueue)
        {
            string unitName = queuedUnit.Name;

            if (UnitDatabase.GetUnitsForBuilding("Barracks", 3).Contains(unitName))
                barracksUnits++;
            else if (UnitDatabase.GetUnitsForBuilding("Archery Range", 3).Contains(unitName))
                archeryUnits++;
            else if (UnitDatabase.GetUnitsForBuilding("Stables", 3).Contains(unitName))
                stablesUnits++;
        }

        Debug.Log($"[AI] {settlement.Name} composition - Barracks: {barracksUnits}, Archery: {archeryUnits}, Stables: {stablesUnits}");

        
        int totalUnits = barracksUnits + archeryUnits + stablesUnits;

        if (totalUnits == 0)
        {
            return GetBestUnitFromBuilding(settlement, "Barracks");
        }

        float barracksRatio = totalUnits > 0 ? (float)barracksUnits / totalUnits : 0;
        float archeryRatio = totalUnits > 0 ? (float)archeryUnits / totalUnits : 0;
        float stablesRatio = totalUnits > 0 ? (float)stablesUnits / totalUnits : 0;

        // Target ratio: 8 barracks, 4 archery, 3 stables
        float barracksNeed = 0.53f - barracksRatio;
        float archeryNeed = 0.27f - archeryRatio;
        float stablesNeed = 0.20f - stablesRatio;

        Debug.Log($"[AI] Need scores - Barracks: {barracksNeed:F2}, Archery: {archeryNeed:F2}, Stables: {stablesNeed:F2}");

        if (barracksNeed >= archeryNeed && barracksNeed >= stablesNeed)
        {
            UnitType unit = GetBestUnitFromBuilding(settlement, "Barracks");
            if (unit != null) return unit;
        }

        if (archeryNeed >= stablesNeed)
        {
            UnitType unit = GetBestUnitFromBuilding(settlement, "Archery Range");
            if (unit != null) return unit;
        }
        UnitType stablesUnit = GetBestUnitFromBuilding(settlement, "Stables");
        if (stablesUnit != null) return stablesUnit;

        List<string> availableUnits = settlement.GetAvailableUnits();
        if (availableUnits.Count > 0)
        {
            string unitName = availableUnits[Random.Range(0, availableUnits.Count)];
            if (UnitDatabase.UnitTypes.TryGetValue(unitName, out UnitType fallbackUnit))
                return fallbackUnit;
        }

        return null;
    }

    private static UnitType GetBestUnitFromBuilding(Settlement settlement, string buildingType)
    {
        int buildingLevel = buildingType switch
        {
            "Barracks" => settlement.BarracksLevel,
            "Archery Range" => settlement.ArcheryRangeLevel,
            "Stables" => settlement.StablesLevel,
            _ => 0
        };

        if (buildingLevel == 0)
        {
            Debug.Log($"[AI] {settlement.Name} has no {buildingType}");
            return null;
        }

        List<string> availableUnits = UnitDatabase.GetUnitsForBuilding(buildingType, buildingLevel);

        if (availableUnits.Count == 0)
            return null;

        UnitType bestUnit = null;
        int highestCost = 0;

        foreach (string unitName in availableUnits)
        {
            if (UnitDatabase.UnitTypes.TryGetValue(unitName, out UnitType unitType))
            {
                int cost = UnitDatabase.GetRecruitmentCost(unitType);
                if (cost > highestCost)
                {
                    bestUnit = unitType;
                    highestCost = cost;
                }
            }
        }
        Debug.Log($"[AI] Best unit from {buildingType}: {bestUnit?.Name ?? "none"}");
        return bestUnit;
    }

    public static void TakeMilitaryTurn(Player ai)
    {
        if (ai == null) return;

        var armies = new List<Army>(ai.Armies);
        foreach (Army army in armies)
        {
            if (army == null || army.TotalSoldiers() == 0)
                continue;

            Tile fromTile = GridManager.Instance.GetTileOfArmy(army);
            if (fromTile == null)
            {
                Debug.LogWarning($"[AI] Could not find tile for army of {ai.Name}");
                continue;
            }

            HandleArmyBehavior(ai, army, fromTile);
        }
    }

    private static void HandleArmyBehavior(Player ai, Army army, Tile fromTile)
    {
        if (army.RemainingMovement <= 0)
        {
            return;
        }

        Tile targetTile = FindNearestEnemyTile(army, fromTile);

        if (targetTile == null)
        {
            Debug.Log($"[AI] {ai.Name} army found no targets.");
            return;
        }

        MoveArmyTowardTarget(ai, army, fromTile, targetTile);
    }

    private static void MoveArmyTowardTarget(Player ai, Army army, Tile fromTile, Tile targetTile)
    {
        Vector2Int current = fromTile.GridPosition;
        Vector2Int target = targetTile.GridPosition;

        int steps = Mathf.Max(0, army.RemainingMovement);
        Vector2Int pos = current;

        for (int i = 0; i < steps; i++)
        {
            // compute single-step direction (diagonal allowed)
            Vector2Int direction = new Vector2Int(
                Mathf.Clamp(target.x - pos.x, -1, 1),
                Mathf.Clamp(target.y - pos.y, -1, 1)
            );

            Vector2Int nextPos = pos + direction;

            Tile nextTile = GridManager.Instance.GetTileAt(nextPos.x, nextPos.y);
            if (nextTile == null) break;

            if (nextTile.HasArmy && nextTile.Army.Faction != army.Faction)
            {
                int myStrength = army.TotalSoldiers();
                int enemyStrength = nextTile.Army.TotalSoldiers();

                if (myStrength >= enemyStrength * 0.8f)
                {
                    Debug.Log($"AI attacks enemy army at {nextPos} (my {myStrength} vs enemy {enemyStrength})");
                    ClickManager.Instance.MoveArmyToTile(army, fromTile, nextTile);
                    return;
                }
                else
                {
                    Debug.Log($"AI will not attack stronger enemy ({myStrength} vs {enemyStrength}).");
                    return;
                }
            }

            if (nextTile.HasSettlement && nextTile.Settlement.Faction != army.Faction)
            {
                Debug.Log($"AI attacks settlement {nextTile.Settlement.Name} at {nextPos}");
                ClickManager.Instance.MoveArmyToTile(army, fromTile, nextTile);
                return;
            }

            if (!nextTile.HasArmy && (!nextTile.HasSettlement || nextTile.Settlement.Faction == army.Faction))
            {
                ClickManager.Instance.MoveArmyToTile(army, fromTile, nextTile);

                fromTile = nextTile;
                pos = nextPos;

                if (army.RemainingMovement <= 0)
                    return;

                continue;
            }
            return;
        }
    }

    private static Tile FindNearestEnemyTile(Army army, Tile fromTile)
    {
        Tile[,] tiles = GridManager.Instance.Tiles;
        Tile nearest = null;
        float minDist = float.MaxValue;

        if (tiles == null) return null;

        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = tiles[x, y];
                if (tile == null) continue;

                bool isEnemy = false;
                if (tile.HasArmy && tile.Army.Faction != army.Faction) isEnemy = true;
                if (tile.HasSettlement && tile.Settlement.Faction != army.Faction) isEnemy = true;

                if (!isEnemy) continue;

                float dist = Vector2Int.Distance(fromTile.GridPosition, tile.GridPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = tile;
                }
            }
        }
        return nearest;
    }
    private static void TrySpawnArmyFromGarrison(Player ai, Settlement settlement)
    {
        if (settlement.Garrison.UnitCount < 12)
        {
            Debug.Log($"AI {settlement.Name} garrison too small to spawn army ({settlement.Garrison.UnitCount}/12)");
            return;
        }
        if (IsEnemyNearby(settlement, 5, ai.Name))
        {
            Debug.Log($"AI {settlement.Name} has enemy nearby - keeping defensive garrison");
            return;
        }
        if (ai.Armies.Count >= ai.Settlements.Count)
        {
            Debug.Log($"AI at army limit");
            return;
        }
        List<Unit> unitsToMove = new List<Unit>();
        int unitsToTake = settlement.Garrison.UnitCount - 3;

        for (int i = 0; i < unitsToTake && i < settlement.Garrison.Units.Count; i++)
        {
            unitsToMove.Add(settlement.Garrison.Units[i]);
        }

        if (unitsToMove.Count > 0)
        {
            Army newArmy = ArmyService.CreateArmyFromGarrison(settlement, ai, unitsToMove);

            if (newArmy != null)
            {
                Debug.Log($"[AI] {ai.Name} spawned army from {settlement.Name} with {unitsToMove.Count} units");
            }
        }
    }

    private static bool IsEnemyNearby(Settlement settlement, int range, string faction)
    {
        Tile[,] tiles = GridManager.Instance.Tiles;
        Vector2Int pos = settlement.GridPosition;

        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        for (int x = Mathf.Max(0, pos.x - range); x <= Mathf.Min(width - 1, pos.x + range); x++)
        {
            for (int y = Mathf.Max(0, pos.y - range); y <= Mathf.Min(height - 1, pos.y + range); y++)
            {
                Tile tile = tiles[x, y];
                if (tile == null) continue;

                float distance = Vector2Int.Distance(pos, new Vector2Int(x, y));
                if (distance > range) continue;
                if (tile.HasArmy && tile.Army.Faction != faction)
                {
                    Debug.Log($"AI Found enemy army at distance {distance} from {settlement.Name}");
                    return true;
                }
            }
        }
        return false;
    }
}
