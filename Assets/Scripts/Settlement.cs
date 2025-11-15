using System.Collections.Generic;
using UnityEngine;

public class Settlement
{
    public string Name;
    public string Faction;

    public int GoldIncome;
    public int Population;

    public int BarracksLevel;
    public int ArcheryRangeLevel;
    public int StablesLevel;
    public int FarmLevel;
    public int MarketLevel;

    public string BuildingInProgress;
    public int ConstructionTimeRemaining;

    public Garrison Garrison;

    public Vector2Int GridPosition;
    public List<UnitType> RecruitmentQueue { get; private set; } = new List<UnitType>();
    public int MaxQueueSize = 1;

    public const int MaxBuildingLevel = 3;
    public Tile Tile { get; set; }

    public static readonly Dictionary<string, int> BuildingCosts = new Dictionary<string, int>
    {
        { "Barracks", 600 },
        { "ArcheryRange", 650 },
        { "Stables", 700 },
        { "Farm", 400 },
        { "Market", 600 }
    };

    public Settlement(string name, string faction, Vector2Int gridPosition)
    {
        Name = name;
        Faction = faction;
        GridPosition = gridPosition;
        Population = 100;
        BarracksLevel = 1;
        ArcheryRangeLevel = 1;
        StablesLevel = 1;
        FarmLevel = 0;
        MarketLevel = 0;
        BuildingInProgress = null;
        ConstructionTimeRemaining = 0;
        Garrison = new Garrison();
    }

    public int Income()
    {
        GoldIncome = Population*10 + (MarketLevel * 500) + (FarmLevel * 200);
        return GoldIncome;
    }

    public void Grow()
    {
        Population += 30 + (FarmLevel * 50);
    }

    public int GetBuildingLevel(string buildingName)
    {
        switch (buildingName)
        {
            case "Barracks": return BarracksLevel;
            case "ArcheryRange": return ArcheryRangeLevel;
            case "Stables": return StablesLevel;
            case "Farm": return FarmLevel;
            case "Market": return MarketLevel;
            default: return 0;
        }
    }

    public bool CanUpgradeBuilding(string buildingName, int playerGold)
    {
        int currentLevel = GetBuildingLevel(buildingName);

        if (currentLevel >= MaxBuildingLevel)
        {
            Debug.LogWarning($"[Settlement] {Name}: {buildingName} already at max level ({MaxBuildingLevel})");
            return false;
        }

        if (!BuildingCosts.ContainsKey(buildingName))
        {
            Debug.LogError($"[Settlement] Unknown building: {buildingName}");
            return false;
        }

        int cost = BuildingCosts[buildingName];
        if (playerGold < cost)
        {
            Debug.LogWarning($"[Settlement] {Name}: Not enough gold to upgrade {buildingName} (Need: {cost}, Have: {playerGold})");
            return false;
        }

        if (IsConstructing())
        {
            Debug.LogWarning($"[Settlement] {Name}: Already constructing {BuildingInProgress}");
            return false;
        }

        return true;
    }

    public bool CanRecruitUnit(UnitType unitType, int playerGold)
    {
        if (RecruitmentQueue.Count >= MaxQueueSize)
        {
            Debug.LogWarning($"[Settlement: {Name}] Recruitment queue full");
            return false;
        }

        int futureGarrisonSize = Garrison.UnitCount + RecruitmentQueue.Count + 1;
        if (futureGarrisonSize > Garrison.MaxUnits)
        {
            Debug.LogWarning($"[Settlement: {Name}] Garrison will be full when recruitment completes");
            return false;
        }

        int cost = UnitDatabase.GetRecruitmentCost(unitType);
        if (playerGold < cost)
        {
            Debug.LogWarning($"[Settlement: {Name}] Not enough gold to recruit {unitType.Name} (Need: {cost}, Have: {playerGold})");
            return false;
        }

        return true;
    }

    public void AdvanceConstruction() 
    {
        if (!string.IsNullOrEmpty(BuildingInProgress) && ConstructionTimeRemaining>0)
        {
            ConstructionTimeRemaining--;
            if (ConstructionTimeRemaining ==0)
            {
                switch (BuildingInProgress)
                {
                    case "Barracks":
                        BarracksLevel++;
                        UnityEngine.Debug.Log($"[Settlement] {Name} completed Barracks construction. Level: {BarracksLevel}");
                        break;
                    case "ArcheryRange":
                        ArcheryRangeLevel++;
                        UnityEngine.Debug.Log($"[Settlement] {Name} completed Archery Range construction. Level: {ArcheryRangeLevel}");
                        break;
                    case "Stables":
                        StablesLevel++;
                        UnityEngine.Debug.Log($"[Settlement] {Name} completed Stables construction. Level: {StablesLevel}");
                        break;
                    case "Farm":
                        FarmLevel++;
                        UnityEngine.Debug.Log($"[Settlement] {Name} completed Farm construction. Level: {FarmLevel}");
                        break;
                    case "Market":
                        MarketLevel++;
                        UnityEngine.Debug.Log($"[Settlement] {Name} completed Market construction. Level: {MarketLevel}");
                        break;
                }
                UnityEngine.Debug.Log($"[Settlement] {Name} construction completed. BuildingInProgress set to null.");
                BuildingInProgress = null;
            }
        }
    }

    public bool IsConstructing()
    {
        return !string.IsNullOrEmpty(BuildingInProgress);
    }

    public List<string> GetAvailableUnits()
    {
        List<string> availableUnits = new List<string>();

        availableUnits.AddRange(UnitDatabase.GetUnitsForBuilding("Barracks", BarracksLevel));
        availableUnits.AddRange(UnitDatabase.GetUnitsForBuilding("Archery Range", ArcheryRangeLevel));
        availableUnits.AddRange(UnitDatabase.GetUnitsForBuilding("Stables", StablesLevel));

        return availableUnits;
    }

    public Unit TrainUnit(string unitTypeName, int size)
    {
        if (!UnitDatabase.UnitTypes.ContainsKey(unitTypeName))
        {
            UnityEngine.Debug.LogError($"Unit type '{unitTypeName}' not found in UnitDatabase.");
            return null;
        }

        UnitType type = UnitDatabase.UnitTypes[unitTypeName];
        return new Unit(type, size);
    }

    public bool AddToRecruitmentQueue(UnitType unitType)
    {
        if (RecruitmentQueue.Count >= MaxQueueSize)
        {
            Debug.LogWarning($"[Settlement: {Name}] Recruitment queue full!");
            return false;
        }

        RecruitmentQueue.Add(unitType);
        Debug.Log($"[Settlement: {Name}] Added {unitType.Name} to recruitment queue.");
        return true;
    }

    public void ProcessRecruitmentQueue()
    {
        if (Garrison == null)
        {
            Debug.LogWarning($"[Settlement: {Name}] No garrison to recruit into.");
            return;
        }

        foreach (var unitType in RecruitmentQueue)
        {
            if (Garrison.IsFull)
            {
                Debug.LogWarning($"[Settlement: {Name}] Garrison full! Cannot recruit more units.");
                break;
            }

            Garrison.AddUnit(new Unit(unitType, unitType.DefaultSize));
        }

        RecruitmentQueue.Clear();
        Debug.Log($"[Settlement: {Name}] Recruitment queue processed.");
    }
}