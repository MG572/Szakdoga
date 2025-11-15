using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string Name { get; private set; }
    public bool isHuman { get; private set; }
    public int Gold { get; set; }
    public List<Settlement> Settlements { get; private set; }
    public List<Army> Armies { get; private set; }

    public Player(string name, bool isHuman)
    {
        Name = name;
        this.isHuman = isHuman;
        Gold = 300;
        Settlements = new List<Settlement>();
        Armies = new List<Army>();
        UnityEngine.Debug.Log($"Player created: {Name}, Human: {isHuman}, Starting Gold: {Gold}");
    }

    public void BeginTurn()
    {
    UnityEngine.Debug.Log($"[Player] {Name} begins turn {TurnManager.Instance.TurnNumber}");
    
    int turnIncome=0;
    foreach (Settlement s in Settlements)
    {
        turnIncome += s.Income();
    }
    Gold += turnIncome;
    UnityEngine.Debug.Log($"[Player] {Name} Received gold and now has Gold: {Gold}, Turn Income: {turnIncome}");


    foreach (Settlement s in Settlements)
        {
            s.AdvanceConstruction();
            s.Grow();
        }
        Tile[,] tiles = GridManager.Instance.Tiles;

        int width = tiles.GetLength(0);
        int height = tiles.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = tiles[x, y];
                if (tile != null && tile.Army != null && tile.Army.Faction == Name)
                {
                    tile.Army.ResetMovement();
                }
            }
        }

        if (!isHuman)
        {
            PerformAITurn();
        }
    }

    public void EndTurn()
    {
        UnityEngine.Debug.Log($"[Player] {Name} ends turn {TurnManager.Instance.TurnNumber}");
    }
    private void PerformAITurn()
    {
        UnityEngine.Debug.Log($"[Player] {Name} is performing AI turn actions");
        
        AIManager.TakeEconomyTurn(this);
        AIManager.TakeMilitaryTurn(this);

        EndTurn();
    }

    public void AddSettlement(Settlement settlement)
    {
        if (settlement != null)
        {
            Settlements.Add(settlement);
            UnityEngine.Debug.Log($"[Player] {Name} added settlement: {settlement.Name}");
        }
    }
    public void RemoveSettlement(Settlement settlement)
    {
        if (settlement != null && Settlements.Contains(settlement))
        {
            Settlements.Remove(settlement);
            UnityEngine.Debug.Log($"[Player] {Name} removed settlement: {settlement.Name}");
        }
    }
    public void AddArmy(Army army)
    {
        if (army != null)
        {
            Armies.Add(army);
            UnityEngine.Debug.Log($"[Player] {Name} added army");
        }
    }
    public void SpendGold(int amount)
    {
        if (amount <= Gold)
        {
            Gold -= amount;
            UnityEngine.Debug.Log($"[Player] {Name} spent {amount} gold. Remaining Gold: {Gold}");

        }
        else
        {
            UnityEngine.Debug.LogWarning($"[Player] {Name} tried to spend {amount} gold but only has {Gold} gold.");
        }
    }

    public void TrainUnitAtSettlement(Settlement settlement, string unitTypeName, int size)
    {
        Unit newUnit = settlement.TrainUnit(unitTypeName, size);
        if (newUnit != null)
        {
            Army firstArmy = Armies.Count > 0 ? Armies[0] : null;

            if (firstArmy != null)
            {
                firstArmy.AddUnit(unitTypeName, size);
                UnityEngine.Debug.Log($"Trained {size} {unitTypeName} at {settlement.Name} and added to the first army.");
            }
            else
            {
                UnityEngine.Debug.LogWarning("No army found to add the new unit to.");
            }
        }
    }
}