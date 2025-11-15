using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Garrison
{
    public const int MaxUnits = 15;
    private List<Unit> units = new List<Unit>();
    public List<Unit> Units => units;

    public void AddUnit(Unit unit)
    {
        if (units.Count >= MaxUnits)
        {
            UnityEngine.Debug.LogWarning("[Garrison] Cannot add more units, garrison is full!");
            return;
        }
        units.Add(unit);
        UnityEngine.Debug.Log($"[Garrison] Added {unit.Type.Name} ({unit.Size}) to the garrison.");
    }

    public void RemoveUnit(Unit unit)
    {
        if (units.Remove(unit))
        {
            UnityEngine.Debug.Log($"[Garrison] Removed {unit.Type.Name} from the garrison.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("[Garrison] Attempted to remove a unit that is not in the garrison!");
        }
    }

    public void ClearGarrison()
    {
        units.Clear();
        UnityEngine.Debug.Log("[Garrison] Garrison cleared.");
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

    public int UnitCount => units.Count;

    public bool IsFull => units.Count >= MaxUnits;

    public void MergeUnits()
    {
        Dictionary<string, int> unitTotals = new Dictionary<string, int>();

        foreach (var unit in units)
        {
            string name = unit.Type.Name;
            if (!unitTotals.ContainsKey(name))
                unitTotals[name] = 0;

            unitTotals[name] += unit.Size;
        }
        units.Clear();

        foreach (var kvp in unitTotals)
        {
            string unitName = kvp.Key;
            int totalSoldiers = kvp.Value;

            if (!UnitDatabase.UnitTypes.TryGetValue(unitName, out UnitType type))
            {
                UnityEngine.Debug.LogError($"[Garrison.MergeUnits] Unit type '{unitName}' not found in UnitDatabase!");
                continue;
            }

            int fullUnitSize = type.DefaultSize;
            int fullUnits = totalSoldiers / fullUnitSize;
            int remainder = totalSoldiers % fullUnitSize;

            for (int i = 0; i < fullUnits; i++)
            {
                if (units.Count >= MaxUnits) return;
                units.Add(new Unit(type, fullUnitSize));
            }

            if (remainder > 0 && units.Count < MaxUnits)
            {
                units.Add(new Unit(type, remainder));
            }
        }
        UnityEngine.Debug.Log($"[Garrison] Units merged. Now contains {units.Count} stacks.");
    }

}