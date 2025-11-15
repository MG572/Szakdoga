using System.Collections.Generic;
using UnityEngine;

public class Army
{
    public string Faction;
    public List<Unit> Units;
    public const int MaxUnits = 15;
    public int MaxMovement = 5;
    public int RemainingMovement { get; private set; }

    public Army(string faction)
    {
        Faction = faction;
        Units = new List<Unit>();
        RemainingMovement = MaxMovement;
    }
    public void AddUnit(string unitTypeName, int size)
    {
        if (Units.Count >= MaxUnits)
        {
            UnityEngine.Debug.LogWarning("Cannot add more units: army is full.");
            return;
        }

        if (!UnitDatabase.UnitTypes.TryGetValue(unitTypeName, out UnitType type))
        {
            UnityEngine.Debug.LogError($"Unit type '{unitTypeName}' not found in UnitDatabase.");
            return;
        }

        Units.Add(new Unit(type, size));
    }

    public int TotalSoldiers()
    {
        int total = 0;
        foreach (var unit in Units)
        {
            total += unit.Size;
        }
        return total;
    }
    public void ResetMovement()
    {
        RemainingMovement = MaxMovement;
    }

    public void SpendMovement(int cost)
    {
        RemainingMovement = Mathf.Max(RemainingMovement - cost, 0);
    }

    public bool CanMoveTo(int cost)
    {
        return RemainingMovement >= cost;
    }

    public bool RemoveUnit(Unit unit)
    {
        bool removed = Units.Remove(unit);
        if (removed)
        {
            UnityEngine.Debug.Log($"[Army] Disbanded {unit.Type.Name}.");

            if (Units.Count == 0)
            {
                UnityEngine.Debug.Log("[Army] No units left. Army will be disbanded.");
                return true; 
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("[Army] Tried to disband a unit that doesn't exist in this army.");
        }

        return false;
    }
    public void MergeUnits()
    {
        Dictionary<string, int> unitTotals = new Dictionary<string, int>();

        foreach (var unit in Units)
        {
            string name = unit.Type.Name;
            if (!unitTotals.ContainsKey(name))
                unitTotals[name] = 0;

            unitTotals[name] += unit.Size;
        }

        Units.Clear();

        foreach (var kvp in unitTotals)
        {
            string unitName = kvp.Key;
            int totalSoldiers = kvp.Value;

            if (!UnitDatabase.UnitTypes.TryGetValue(unitName, out UnitType type))
            {
                Debug.LogError($"[Army.MergeUnits] Could not find unit type {unitName} in UnitDatabase!");
                continue;
            }

            int fullUnitSize = type.DefaultSize;
            int fullUnits = totalSoldiers / fullUnitSize;
            int remainder = totalSoldiers % fullUnitSize;

            for (int i = 0; i < fullUnits; i++)
            {
                if (Units.Count >= MaxUnits) return;
                Units.Add(new Unit(type, fullUnitSize));
            }
            if (remainder > 0 && Units.Count < MaxUnits)
            {
                Units.Add(new Unit(type, remainder));
            }
        }

        Debug.Log($"[Army] Units merged. Now contains {Units.Count} stacks.");
    }

}