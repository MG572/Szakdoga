using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    public void ResolveBattle(Army attacker, Army defender, Tile fromTile, Tile toTile)
    {
        if (attacker == null || defender == null || fromTile == null || toTile == null)
        {
            Debug.LogError("[BattleManager] ResolveBattle called with null args.");
            return;
        }

        Debug.Log($"[BattleManager] Battle: {attacker.Faction} vs {defender.Faction}");

        float attackerPower = CalculateArmyPower(attacker);
        float defenderPower = CalculateArmyPower(defender);

        float terrainMultiplier = TerrainDefenceModifier(toTile.Terrain);
        defenderPower *= terrainMultiplier;

        Debug.Log($"[BattleManager] Attacker={attackerPower}, Defender={defenderPower} (terrain x{terrainMultiplier})");

        bool attackerWins = attackerPower >= defenderPower;

        if (attackerWins)
        {
            Debug.Log("[BattleManager] Attacker wins!");
            float attackerLossRatio = Mathf.Clamp01((defenderPower / attackerPower) * 0.6f);
            ApplyBattleLosses(attacker, attackerLossRatio);
            attacker.Units.RemoveAll(u => u.Size <= 0);

            ApplyBattleLosses(defender, 1.0f);
            if (toTile.ArmyObject != null)
                Object.Destroy(toTile.ArmyObject);
            TurnManager.Instance.GetPlayerByName(defender.Faction)?.Armies.Remove(defender);

            toTile.Army = attacker;
            toTile.ArmyObject = fromTile.ArmyObject;
            fromTile.Army = null;
            fromTile.ArmyObject = null;

            UpdateArmyObject(toTile);
        }
        else
        {
            Debug.Log("[BattleManager] Defender wins!");
            float defenderLossRatio = Mathf.Clamp01((attackerPower / defenderPower) * 0.4f);
            ApplyBattleLosses(defender, defenderLossRatio);
            defender.Units.RemoveAll(u => u.Size <= 0);

            if (fromTile.ArmyObject != null)
                Object.Destroy(fromTile.ArmyObject);
            TurnManager.Instance.GetPlayerByName(attacker.Faction)?.Armies.Remove(attacker);

            fromTile.Army = null;
            fromTile.ArmyObject = null;

            UpdateArmyObject(toTile);
        }

        Debug.Log("[BattleManager] Battle resolved.");
        UIManager.Instance?.HideArmyPanel();
        ClickManager.Instance?.ClearArmySelection();
        UIManager.Instance?.UpdateUI();
    }

    private float CalculateArmyPower(Army army)
    {
        float total = 0;
        foreach (var unit in army.Units)
        {
            total += (unit.Type.Damage + unit.Type.MeleeArmor + unit.Type.Speed) * unit.Size;
        }
        return total;
    }

    private void ApplyBattleLosses(Army army, float lossRatio)
    {
        foreach (var unit in army.Units)
        {
            int losses = Mathf.RoundToInt(unit.Size * lossRatio);
            unit.Size -= losses;
            if (unit.Size < 0) unit.Size = 0;
        }

        Debug.Log($"[BattleManager] {army.Faction} lost {(int)(lossRatio * 100)}% of its troops.");
    }

    private void UpdateArmyObject(Tile tile)
    {
        if (tile.ArmyObject == null) return;
        tile.ArmyObject.transform.position = new Vector3(tile.GridPosition.x, tile.GridPosition.y, 0);
        var clickable = tile.ArmyObject.GetComponent<ClickableObject>();
        if (clickable != null)
            clickable.linkedTile = tile;
    }

    public void ResolveSiege(Army attacker, Settlement settlement, Tile fromTile, Tile settlementTile)
    {
        if (attacker == null || settlement == null || fromTile == null || settlementTile == null)
        {
            Debug.LogError("[BattleManager] ResolveSiege called with null arguments.");
            return;
        }

        Debug.Log($"[BattleManager] Siege begins: {attacker.Faction} attacks {settlement.Name} ({settlement.Faction})");

        float attackerPower = CalculateArmyPower(attacker);
        float defenderPower = CalculateArmyPowerFromGarrison(settlement.Garrison);

        const float fortificationBonus = 1.75f;
        defenderPower *= fortificationBonus;

        Debug.Log($"[BattleManager] Attacker Power: {attackerPower}, Defender Power: {defenderPower} (x{fortificationBonus} fortification bonus)");

        bool attackerWins = attackerPower >= defenderPower;

        if (attackerWins)
        {
            Debug.Log($"[BattleManager] {attacker.Faction} has conquered {settlement.Name}!");

            float attackerLossRatio = Mathf.Clamp01((defenderPower / attackerPower) * 0.7f);
            float defenderLossRatio = Mathf.Clamp01((attackerPower / defenderPower) * 0.9f);

            ApplyBattleLosses(attacker, attackerLossRatio);
            ApplyBattleLossesToGarrison(settlement.Garrison, defenderLossRatio);

            attacker.Units.RemoveAll(u => u.Size <= 0);

            Player oldOwner = TurnManager.Instance.GetPlayerByName(settlement.Faction);
            Player newOwner = TurnManager.Instance.GetPlayerByName(attacker.Faction);

            if (oldOwner != null)
            {
                oldOwner.RemoveSettlement(settlement);
                Debug.Log($"[BattleManager] Removed {settlement.Name} from {oldOwner.Name}");
            }

            if (newOwner != null)
            {
                newOwner.AddSettlement(settlement);
                Debug.Log($"[BattleManager] Added {settlement.Name} to {newOwner.Name}");
            }

            settlement.Faction = attacker.Faction;
            settlement.Garrison.Units.Clear();
            settlement.Garrison.Units.AddRange(attacker.Units);

            TurnManager.Instance.GetPlayerByName(attacker.Faction)?.Armies.Remove(attacker);

            if (fromTile.ArmyObject != null)
                Object.Destroy(fromTile.ArmyObject);

            fromTile.Army = null;
            fromTile.ArmyObject = null;
            Debug.Log($"[BattleManager] {settlement.Name} now garrisoned by {attacker.Faction} ");
        }
        else
        {
            Debug.Log($"[BattleManager] {settlement.Faction} successfully defended {settlement.Name}!");
            float attackerLossRatio = Mathf.Clamp01((defenderPower / attackerPower) * 0.8f);
            float defenderLossRatio = Mathf.Clamp01((attackerPower / defenderPower) * 0.5f);

            ApplyBattleLosses(attacker, attackerLossRatio);
            ApplyBattleLossesToGarrison(settlement.Garrison, defenderLossRatio);
            if (fromTile.ArmyObject != null)
                Object.Destroy(fromTile.ArmyObject);

            TurnManager.Instance.GetPlayerByName(attacker.Faction)?.Armies.Remove(attacker);

            fromTile.Army = null;
            fromTile.ArmyObject = null;
            Debug.Log($"[BattleManager] Attacker destroyed. {settlement.Name} remains under {settlement.Faction} control.");
        }

        ClickManager.Instance?.ClearArmySelection();
        UIManager.Instance?.HideArmyPanel();
        UIManager.Instance?.UpdateUI();
    }

    private float CalculateArmyPowerFromGarrison(Garrison garrison)
    {
        if (garrison == null || garrison.Units == null) return 0;

        float total = 0;
        foreach (var unit in garrison.Units)
        {
            total += (unit.Type.Damage + unit.Type.MeleeArmor + unit.Type.Speed) * unit.Size;
        }
        return total;
    }

    private void ApplyBattleLossesToGarrison(Garrison garrison, float lossRatio)
    {
        foreach (var unit in garrison.Units)
        {
            int losses = Mathf.RoundToInt(unit.Size * lossRatio);
            unit.Size -= losses;
            if (unit.Size < 0) unit.Size = 0;
        }
        Debug.Log($"[BattleManager] Garrison lost {(int)(lossRatio * 100)}% of its troops.");
    }


    float TerrainDefenceModifier(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Woodland: return 1.05f;
            case TerrainType.Hills: return 1.15f;
            case TerrainType.Mountains: return 1.25f;
            case TerrainType.Snow: return 1.0f;
            case TerrainType.HighMountains: return 1.3f;
            case TerrainType.Water: return 1.0f;
            case TerrainType.Desert: return 1.0f;
            case TerrainType.Grassland:
            default:
                return 1.0f;
        }
    }

    public void EnterFriendlySettlement(Army army, Settlement settlement, Tile fromTile, Tile settlementTile)
    {
        if (army == null || settlement == null || fromTile == null || settlementTile == null)
        {
            Debug.LogError("[BattleManager] EnterFriendlySettlement called with null arguments.");
            return;
        }
        Debug.Log($"[BattleManager] {army.Faction} army entering friendly settlement {settlement.Name}.");
        Garrison garrison = settlement.Garrison;
        garrison.MergeUnits();
        army.MergeUnits();
        List<Unit> unitsToTransfer = new List<Unit>(army.Units);

        foreach (var unit in unitsToTransfer)
        {
            if (garrison.Units.Count < Garrison.MaxUnits)
            {
                garrison.Units.Add(unit);
            }
            else
            {
                Debug.LogWarning("[BattleManager] Garrison is full. Some units remain in the field.");
                break;
            }
        }

        foreach (var unit in unitsToTransfer)
        {
            if (garrison.Units.Contains(unit))
                army.Units.Remove(unit);
        }
        garrison.MergeUnits();
        army.MergeUnits();

        if (army.Units.Count == 0)
        {
            Debug.Log($"[BattleManager] {army.Faction} army fully absorbed into {settlement.Name}'s garrison.");
            TurnManager.Instance.GetPlayerByName(army.Faction)?.Armies.Remove(army);

            if (fromTile.ArmyObject != null)
                Object.Destroy(fromTile.ArmyObject);

            fromTile.Army = null;
            fromTile.ArmyObject = null;
        }
        else
        {
            Debug.Log($"[BattleManager] {army.Faction} army partially merged; {army.Units.Count} stacks remain outside {settlement.Name}.");
        }
        ClickManager.Instance?.ClearArmySelection();
        UIManager.Instance?.HideArmyPanel();
        UIManager.Instance?.UpdateUI();

        Debug.Log($"[BattleManager] {settlement.Name} garrison now has {garrison.UnitCount} unit stacks ({garrison.TotalSoldiers()} soldiers).");
    }

}
