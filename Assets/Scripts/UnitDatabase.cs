using System.Collections.Generic;

public static class UnitDatabase
{
    public static Dictionary<string, UnitType> UnitTypes = new Dictionary<string, UnitType>
    {
        //size, damage, melee armor, ranged armor, speed
        // Barracks
        { "Militia Spearman", new UnitType("Militia Spearman", 120, 5, 2, 1, 3) },
        { "Pikeman", new UnitType("Pikeman", 150, 8, 4, 2, 2) },
        { "Armored Spearman", new UnitType("Armored Spearman", 120, 9, 5, 3, 3) },
        { "Man-at-Arms", new UnitType("Man-at-Arms", 120, 10, 6, 3, 3) },
        { "Foot Knight", new UnitType("Foot Knight", 100, 16, 10, 8, 2) },
        { "Halberdier", new UnitType("Halberdier", 100, 14, 10, 8, 2) },

        // Archery Range
        { "Militia Archer", new UnitType("Militia Archer", 80, 4, 1, 3, 4) },
        { "Skirmisher", new UnitType("Skirmisher", 80, 3, 1, 2, 5) },
        { "Archer", new UnitType("Archer", 80, 6, 2, 3, 4) },
        { "Crossbowman", new UnitType("Crossbowman", 70, 10, 2, 4, 3) },
        { "Longbowman", new UnitType("Longbowman", 70, 12, 3, 5, 4) },
        { "Heavy Crossbowman", new UnitType("Heavy Crossbowman", 60, 14, 4, 6, 3) },

        // Stables
        { "Light Cavalry", new UnitType("Light Cavalry", 80, 7, 3, 2, 8) },
        { "Horse Archer", new UnitType("Horse Archer", 80, 6, 2, 4, 8) },
        { "Lancer", new UnitType("Lancer", 70, 15, 5, 4, 7) },
        { "Knight", new UnitType("Knight", 60, 20, 10, 8, 6) },
        { "Paladin", new UnitType("Paladin", 60, 27, 12, 10, 6) },
        { "Veteran Horse Archer", new UnitType("Veteran Horse Archer", 60, 10, 4, 8, 9) }
    };

    public static int GetRecruitmentCost(UnitType unitType)
    {
        int baseCost = (unitType.DefaultSize * 2) +
                       (unitType.Damage * 10) +
                       (unitType.MeleeArmor * 5) +
                       (unitType.RangedArmor * 5);

        return baseCost;
    }

    public static List<string> GetUnitsForBuilding(string buildingType, int level)
    {
        if (buildingType == "Barracks")
        {
            return level switch
            {
                1 => new List<string> { "Militia Spearman" },
                2 => new List<string> { "Militia Spearman", "Pikeman", "Armored Spearman", "Man-at-Arms" },
                3 => new List<string> { "Militia Spearman", "Pikeman", "Armored Spearman", "Man-at-Arms", "Foot Knight", "Halberdier" },
                _ => new List<string>()
            };
        }
        else if (buildingType == "Archery Range")
        {
            return level switch
            {
                1 => new List<string> { "Militia Archer", "Skirmisher" },
                2 => new List<string> { "Militia Archer", "Skirmisher", "Archer", "Crossbowman" },
                3 => new List<string> { "Militia Archer", "Skirmisher", "Archer", "Crossbowman", "Longbowman", "Heavy Crossbowman" },
                _ => new List<string>()
            };
        }
        else if (buildingType == "Stables")
        {
            return level switch
            {
                1 => new List<string> { "Light Cavalry" },
                2 => new List<string> { "Light Cavalry", "Horse Archer", "Lancer", "Knight" },
                3 => new List<string> { "Light Cavalry", "Horse Archer", "Lancer", "Knight", "Paladin", "Veteran Horse Archer" },
                _ => new List<string>()
            };
        }
        return new List<string>();
    }
}