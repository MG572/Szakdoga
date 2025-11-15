using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Button endTurnButton;
    public TMP_Text turnText;
    public TMP_Text goldText;
    public TMP_Text terrainInfoText;

    [Header("Settlement Panel")]
    public GameObject settlementPanel;
    public TMP_Text nameText;
    public TMP_Text populationText;
    public TMP_Text incomeText;
    public TMP_Text growthText;
    public TMP_Text barracksLevelText;
    public TMP_Text archeryRangeLevelText;
    public TMP_Text stablesLevelText;
    public TMP_Text farmLevelText;
    public TMP_Text marketLevelText;
    public Button mergeGarrisonButton;

    public Button barracksUpgradeButton;

    private Settlement selectedSettlement;

    private Army currentArmy;

    public TMP_Text constructionText;

    [Header("Army Panel")]
    public GameObject armyPanel;
    public Transform unitGrid;
    public TMP_Text armyTitleText;
    public GameObject unitCardPrefab;
    public Button mergeButton;

    [Header("Garrison Panel")]
    public GameObject garrisonPanel;
    public Transform garrisonGrid;
    public GameObject garrisonUnitCardPrefab;

    [Header("Recruitment Panel")]
    public GameObject recruitmentPanel;
    public Button recruitButton;
    public Transform barracksColumn;
    public Transform archeryRangeColumn;
    public Transform stablesColumn;
    public TMP_Text recruitmentQueueText;

    private List<Unit> selectedUnits = new List<Unit>();

    [Header("Garrison Transfer")]
    public Button transferToArmyButton;

    [Header("New Army")]
    public Button newArmyButton;

    [Header("Enemy Settlement")]
    public GameObject enemySettlementPanel;
    public Transform enemyGarrisonGrid;
    public GameObject enemyGarrisonUnitCardPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            UnityEngine.Debug.LogWarning("[UIManager] Duplicate instance found. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UnityEngine.Debug.Log("[UIManager] Awake()");
    }

    void Start()
    {
        UnityEngine.Debug.Log("[UIManager] Start()");

        if (endTurnButton == null) UnityEngine.Debug.LogError("endTurnButton is not assigned in the Inspector!");
        if (turnText == null) UnityEngine.Debug.LogError("turnText is not assigned in the Inspector!");
        if (goldText == null) UnityEngine.Debug.LogError("goldText is not assigned in the Inspector!");

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }

        barracksUpgradeButton.onClick.AddListener(UpgradeBarracks);

        if (settlementPanel != null)
        {
            settlementPanel.SetActive(false);
        }

        if (transferToArmyButton != null)
            transferToArmyButton.onClick.AddListener(OnTransferToArmyButtonClicked);

        if (mergeButton != null)
            mergeButton.onClick.AddListener(OnMergeArmyButtonClicked);

        if (mergeGarrisonButton != null)
            mergeGarrisonButton.onClick.AddListener(OnMergeGarrisonButtonClicked);

        UpdateUI();
    }

    void OnEndTurnButtonClicked()
    {
        UnityEngine.Debug.Log("[UIManager] End Turn Button Clicked");

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.EndTurn();
            UnityEngine.Debug.Log("Turn ended successfully");
        }
        else
        {
            UnityEngine.Debug.LogError("Can't end turn: TurnManager instance is null!");
        }
    }

    public void OnEndTurn()
    {
        UnityEngine.Debug.Log("[UIManager] OnEndTurn() called");
        UpdateUI();
    }

    public void UpdateUI()
    {
        UnityEngine.Debug.Log("[UIManager] UpdateUI() called");

        if (TurnManager.Instance == null)
        {
            UnityEngine.Debug.LogWarning("Update UI: TurnManager.Instance is null - using default values");

            // bakcup values
            if (turnText != null) turnText.text = "Turn: 1";
            if (goldText != null) goldText.text = "Gold: 0";
            return;
        }

        if (TurnManager.Instance.CurrentPlayer == null)
        {
            UnityEngine.Debug.LogError("Update UI: TurnManager.CurrentPlayer is null");
            return;
        }

        var currentPlayer = TurnManager.Instance.CurrentPlayer;

        if (turnText != null)
            turnText.text = $"Turn: {TurnManager.Instance.TurnNumber}";

        if (goldText != null)
            goldText.text = $"Gold: {currentPlayer.Gold}";

        UnityEngine.Debug.Log($"[UIManager] UI Updated - Turn: {TurnManager.Instance.TurnNumber}, Gold: {currentPlayer.Gold}");
    }

    public void SetTerrainInfo(string terrainName)
    {
        if (terrainInfoText != null)
            terrainInfoText.text = $"Terrain: {terrainName}";
    }

    public void ShowSettlementInfo(Settlement settlement)
    {
        HideEnemySettlementInfo();
        if (settlement.Faction=="Player")
        {
            selectedSettlement = settlement;
            if (settlement == null)
            {
                UnityEngine.Debug.LogWarning("[UIManager] Attempted to show info for null settlement");
                return;
            }

            UnityEngine.Debug.Log($"[UIManager] ShowSettlementInfo() called on: {settlement.Name}");

            if (settlementPanel != null)
                settlementPanel.SetActive(true);

            int income = settlement.Income();

            if (nameText != null) nameText.text = $"{settlement.Name}";
            if (populationText != null) populationText.text = $"Population: {settlement.Population}";
            if (incomeText != null) incomeText.text = $"Income: {income}";
            int estimatedGrowth = 30 + (settlement.FarmLevel * 50);
            if (growthText != null) growthText.text = $"Growth: +{estimatedGrowth}";
            if (barracksLevelText != null) barracksLevelText.text = $"Barracks: {settlement.BarracksLevel}";
            if (archeryRangeLevelText != null) archeryRangeLevelText.text = $"Archery Range: {settlement.ArcheryRangeLevel}";
            if (stablesLevelText != null) stablesLevelText.text = $"Stables: {settlement.StablesLevel}";
            if (farmLevelText != null) farmLevelText.text = $"Farm: {settlement.FarmLevel}";
            if (marketLevelText != null) marketLevelText.text = $"Market: {settlement.MarketLevel}";

            if (settlement.IsConstructing())
            {
                constructionText.text = $"Construction in progress: {settlement.BuildingInProgress} ({settlement.ConstructionTimeRemaining} turns remaining)";
            }
            else
            {
                constructionText.text = "";
            }

            newArmyButton.onClick.RemoveAllListeners();
            newArmyButton.onClick.AddListener(() =>
            {
                OnNewArmyButtonClicked(settlement, TurnManager.Instance.CurrentPlayer);
            });
            UpdateRecruitmentQueueText();
            ShowGarrison(settlement);
        }
        
    }

    public void ShowEnemySettlementInfo(Settlement enemySettlement)
    {
        HideSettlementInfo();
        if (enemySettlement.Faction=="AI")
        {
            selectedSettlement =enemySettlement;
            if (enemySettlement == null)
            {
                UnityEngine.Debug.LogWarning("[UIManager] Attempted to show info for null settlement");
                return;
            }

            if (enemySettlementPanel != null)
                enemySettlementPanel.SetActive(true);

            ShowEnemyGarrison(enemySettlement);
        }
    }

    public void HideSettlementInfo()
    {
        if (settlementPanel != null)
            settlementPanel.SetActive(false);
    }

    public void HideEnemySettlementInfo()
    {
        if (enemySettlementPanel != null)
            enemySettlementPanel.SetActive(false);
    }

    public void ExitSettlmentInfo()
    {
        HideSettlementInfo();
        UnityEngine.Debug.Log("[UIManager] Exit Settlement Info");
    }
    public void UpgradeBarracks()
    {
        UnityEngine.Debug.Log("[UIManager] UpgradeBarracks called");

        if (selectedSettlement == null)
        {
            UnityEngine.Debug.LogError("[UIManager] selectedSettlement is NULL!");
            return;
        }

        UnityEngine.Debug.Log($"[UIManager] selectedSettlement: {selectedSettlement.Name}");

        Player currentPlayer = TurnManager.Instance.CurrentPlayer;

        if (currentPlayer == null)
        {
            UnityEngine.Debug.LogError("[UIManager] currentPlayer is NULL!");
            return;
        }

        UnityEngine.Debug.Log($"[UIManager] currentPlayer: {currentPlayer.Name}, Gold: {currentPlayer.Gold}");

        bool canUpgrade = selectedSettlement.CanUpgradeBuilding("Barracks", currentPlayer.Gold);
        UnityEngine.Debug.Log($"[UIManager] CanUpgradeBuilding returned: {canUpgrade}");

        if (canUpgrade)
        {
            int cost = Settlement.BuildingCosts["Barracks"];
            UnityEngine.Debug.Log($"[UIManager] About to spend {cost} gold");
            currentPlayer.SpendGold(cost);
            UnityEngine.Debug.Log($"[UIManager] SpendGold called, player now has {currentPlayer.Gold} gold");

            selectedSettlement.BuildingInProgress = "Barracks";
            selectedSettlement.ConstructionTimeRemaining = 2;

            ShowSettlementInfo(selectedSettlement);
            UpdateUI();
        }
    }
    public void UpgradeArcheryRange()
    {
        if (selectedSettlement == null) return;

        Player currentPlayer = TurnManager.Instance.CurrentPlayer;
        if (currentPlayer == null) return;

        if (selectedSettlement.CanUpgradeBuilding("ArcheryRange", currentPlayer.Gold))
        {
            int cost = Settlement.BuildingCosts["ArcheryRange"];
            currentPlayer.SpendGold(cost);

            selectedSettlement.BuildingInProgress = "ArcheryRange";
            selectedSettlement.ConstructionTimeRemaining = 2;

            ShowSettlementInfo(selectedSettlement);
            UpdateUI();
        }
    }

    public void UpgradeStables()
    {
        if (selectedSettlement == null) return;

        Player currentPlayer = TurnManager.Instance.CurrentPlayer;
        if (currentPlayer == null) return;

        if (selectedSettlement.CanUpgradeBuilding("Stables", currentPlayer.Gold))
        {
            int cost = Settlement.BuildingCosts["Stables"];
            currentPlayer.SpendGold(cost);

            selectedSettlement.BuildingInProgress = "Stables";
            selectedSettlement.ConstructionTimeRemaining = 2;

            ShowSettlementInfo(selectedSettlement);
            UpdateUI();
        }
    }

    public void UpgradeFarm()
    {
        if (selectedSettlement == null) return;

        Player currentPlayer = TurnManager.Instance.CurrentPlayer;
        if (currentPlayer == null) return;

        if (selectedSettlement.CanUpgradeBuilding("Farm", currentPlayer.Gold))
        {
            int cost = Settlement.BuildingCosts["Farm"];
            currentPlayer.SpendGold(cost);

            selectedSettlement.BuildingInProgress = "Farm";
            selectedSettlement.ConstructionTimeRemaining = 2;

            ShowSettlementInfo(selectedSettlement);
            UpdateUI();
        }
    }

    public void UpgradeMarket()
    {
        if (selectedSettlement == null) return;

        Player currentPlayer = TurnManager.Instance.CurrentPlayer;
        if (currentPlayer == null) return;

        if (selectedSettlement.CanUpgradeBuilding("Market", currentPlayer.Gold))
        {
            int cost = Settlement.BuildingCosts["Market"];
            currentPlayer.SpendGold(cost);

            selectedSettlement.BuildingInProgress = "Market";
            selectedSettlement.ConstructionTimeRemaining = 2;

            ShowSettlementInfo(selectedSettlement);
            UpdateUI();
        }
    }

    public void ShowArmyPanel(Army army)
    {
        currentArmy = army;
        if (army == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] Tried to show an army panel for a null army");
            return;
        }
        if (armyTitleText != null)
            armyTitleText.text = $"{army.Faction} Army";

        foreach (Transform child in unitGrid)
        {
            Destroy(child.gameObject);
        }
        foreach (Unit unit in army.Units)
        {
            GameObject card = Instantiate(unitCardPrefab, unitGrid);
            TMP_Text sizeText = card.transform.Find("UnitSizeText").GetComponent<TMP_Text>();
            TMP_Text nameText = card.transform.Find("UnitNameText").GetComponent<TMP_Text>();

            sizeText.text = unit.Size.ToString();
            nameText.text = unit.Type.Name;

            UnitCardClickHandlerUI handler = card.AddComponent<UnitCardClickHandlerUI>();
            handler.Initialize(unit);
        }
        int emptySlots = 15 - army.Units.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject card = Instantiate(unitCardPrefab, unitGrid);
            TMP_Text sizeText = card.transform.Find("UnitSizeText").GetComponent<TMP_Text>();
            TMP_Text nameText = card.transform.Find("UnitNameText").GetComponent<TMP_Text>();

            sizeText.text = "-";
            nameText.text = "Empty Slot";
        }

        if (armyPanel != null)
            armyPanel.SetActive(true);

        if (army.Faction!="Player")
        {
            mergeButton.gameObject.SetActive(false);
        }
        if (army.Faction=="Player")
        {
            mergeButton.gameObject.SetActive(true);
        }

        UnityEngine.Debug.Log($"[UIManager] Showing army panel for {army.Faction}");
    }

    public void HideArmyPanel()
    {
        if (armyPanel != null)
            armyPanel.SetActive(false);
    }

    public void ShowGarrison(Settlement settlement)
    {
        if (settlement == null || settlement.Garrison == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] Tried to show a garrison panel for a null settlement");
            return;
        }
        foreach (Transform child in garrisonGrid)
        {
            Destroy(child.gameObject);
        }
        foreach (Unit unit in settlement.Garrison.Units)
        {
            GameObject card = Instantiate(garrisonUnitCardPrefab, garrisonGrid);
            TMP_Text sizeText = card.transform.Find("UnitSizeText").GetComponent<TMP_Text>();
            TMP_Text nameText = card.transform.Find("UnitNameText").GetComponent<TMP_Text>();
            Button button = card.GetComponent<Button>();

            sizeText.text = unit.Size.ToString();
            nameText.text = unit.Type.Name;

            UnitCardClickHandlerUI handler = card.AddComponent<UnitCardClickHandlerUI>();
            handler.Initialize(unit, settlement);

            button.onClick.AddListener(() => OnGarrisonUnitClicked(unit, card));
        }
        int emptySlots = Garrison.MaxUnits - settlement.Garrison.UnitCount;
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject card = Instantiate(garrisonUnitCardPrefab, garrisonGrid);
            TMP_Text sizeText = card.transform.Find("UnitSizeText").GetComponent<TMP_Text>();
            TMP_Text nameText = card.transform.Find("UnitNameText").GetComponent<TMP_Text>();

            sizeText.text = "-";
            nameText.text = "Empty Slot";
        }

        garrisonPanel.SetActive(true);
        UnityEngine.Debug.Log($"[UIManager] Showing garrison panel for {settlement.Name}");
    }

    public void ShowEnemyGarrison(Settlement settlement)
    {
        UnityEngine.Debug.Log($"[UIManager] AAAAA ShowEnemyGarrison for {settlement.Name}");
        UnityEngine.Debug.Log($"enemyGarrisonGrid assigned? {enemyGarrisonGrid != null}");
        UnityEngine.Debug.Log($"Prefab assigned? {(enemyGarrisonUnitCardPrefab != null ? "Yes" : "No")}");
        UnityEngine.Debug.Log($"Garrison units count: {settlement.Garrison.Units.Count}");
        if (settlement == null || settlement.Garrison == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] Tried to show an enemy garrison for a null settlement");
            return;
        }

        if (enemyGarrisonGrid == null)
        {
            UnityEngine.Debug.LogError("[UIManager] enemyGarrisonGrid not assigned in Inspector!");
            return;
        }
        foreach (Transform child in enemyGarrisonGrid)
            Destroy(child.gameObject);

        foreach (Unit unit in settlement.Garrison.Units)
        {
            GameObject card = Instantiate(
                enemyGarrisonUnitCardPrefab != null ? enemyGarrisonUnitCardPrefab : garrisonUnitCardPrefab,
                enemyGarrisonGrid
            );

            TMP_Text sizeText = card.transform.Find("UnitSizeText")?.GetComponent<TMP_Text>();
            TMP_Text nameText = card.transform.Find("UnitNameText")?.GetComponent<TMP_Text>();
            Button button = card.GetComponent<Button>();

            if (sizeText != null) sizeText.text = unit.Size.ToString();
            if (nameText != null) nameText.text = unit.Type.Name;

            if (button != null)
            {
                button.interactable = false;
                button.onClick.RemoveAllListeners();
            }
            var handler = card.GetComponent<UnitCardClickHandlerUI>();
            if (handler != null)
                Destroy(handler);
        }
        int emptySlots = Garrison.MaxUnits - settlement.Garrison.UnitCount;
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject card = Instantiate(
                enemyGarrisonUnitCardPrefab != null ? enemyGarrisonUnitCardPrefab : garrisonUnitCardPrefab,
                enemyGarrisonGrid
            );

            TMP_Text sizeText = card.transform.Find("UnitSizeText")?.GetComponent<TMP_Text>();
            TMP_Text nameText = card.transform.Find("UnitNameText")?.GetComponent<TMP_Text>();
            Button button = card.GetComponent<Button>();

            if (sizeText != null) sizeText.text = "-";
            if (nameText != null) nameText.text = "Empty Slot";

            if (button != null)
            {
                button.interactable = false;
                button.onClick.RemoveAllListeners();
            }

            var handler = card.GetComponent<UnitCardClickHandlerUI>();
            if (handler != null)
                Destroy(handler);
        }
        if (enemySettlementPanel != null) enemySettlementPanel.SetActive(true);
        if (settlementPanel != null) settlementPanel.SetActive(false);

        UnityEngine.Debug.Log($"[UIManager] Showing ENEMY garrison for {settlement.Name}");
    }

    public void ToggleRecruitmentPanel()
    {
        if (recruitmentPanel == null || selectedSettlement == null) return;

        bool isActive = !recruitmentPanel.activeSelf;
        recruitmentPanel.SetActive(isActive);
        if (isActive) PopulateRecruitmentPanel();
    }

    public void PopulateRecruitmentPanel()
    {
        if (selectedSettlement == null) return;

        PopulateBuildingUnits(barracksColumn, "Barracks", selectedSettlement.BarracksLevel);
        PopulateBuildingUnits(archeryRangeColumn, "Archery Range", selectedSettlement.ArcheryRangeLevel);
        PopulateBuildingUnits(stablesColumn, "Stables", selectedSettlement.StablesLevel);
    }

    private void PopulateBuildingUnits(Transform column, string buildingType, int level)
    {
        foreach (Transform child in column)
        {
            Destroy(child.gameObject);
        }
        List<string> unitNames = UnitDatabase.GetUnitsForBuilding(buildingType, level);

        foreach (string unitName in unitNames)
        {
            UnitType unitType = UnitDatabase.UnitTypes[unitName];
            int cost = UnitDatabase.GetRecruitmentCost(unitType);
            GameObject card = Instantiate(unitCardPrefab, column);
            TMP_Text nameText = card.GetComponentInChildren<TMP_Text>();
            TMP_Text sizeText = card.GetComponentsInChildren<TMP_Text>()[1];
            nameText.text = $"{unitType.Name} ({cost}g)"; 
            sizeText.text = unitType.DefaultSize.ToString();

            Button button = card.GetComponent<Button>();
            button.onClick.AddListener(() => AddToRecruitmentQueue(unitType));
        }
    }

    public void AddToRecruitmentQueue(UnitType unitType)
    {
        if (selectedSettlement == null) return;

        Player currentPlayer = TurnManager.Instance.CurrentPlayer;
        if (currentPlayer == null) return;

        if (selectedSettlement.CanRecruitUnit(unitType, currentPlayer.Gold))
        {
            int cost = UnitDatabase.GetRecruitmentCost(unitType);
            currentPlayer.SpendGold(cost);

            bool added = selectedSettlement.AddToRecruitmentQueue(unitType);
            if (added)
            {
                UpdateRecruitmentQueueText();
                UpdateUI();
                UnityEngine.Debug.Log($"[UIManager] Recruited {unitType.Name} for {cost} gold");
            }
        }
    }


    private void UpdateRecruitmentQueueText()
    {
        if (selectedSettlement == null) return;

        recruitmentQueueText.text = "Recruitment Queue:\n";
        foreach (UnitType unit in selectedSettlement.RecruitmentQueue)
        {
            recruitmentQueueText.text += $"- {unit.Name} ({unit.DefaultSize} units)\n";
        }
    }
    private void OnGarrisonUnitClicked(Unit unit, GameObject card)
    {
        if (selectedUnits.Contains(unit))
        {
            selectedUnits.Remove(unit);
            SetCardHighlight(card, false);
        }
        else
        {
            selectedUnits.Add(unit);
            SetCardHighlight(card, true);
        }

        UnityEngine.Debug.Log($"Selected units: {selectedUnits.Count}");
    }
    private void SetCardHighlight(GameObject card, bool selected)
    {
        Image background = card.GetComponent<Image>();
        if (background != null)
        {
            background.color = selected ? Color.yellow : Color.white;
        }
    }


    private void OnTransferToArmyButtonClicked()
    {
        if (selectedSettlement == null || selectedSettlement.Garrison == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] No selected settlement or garrison.");
            return;
        }
        Army nearbyArmy = FindNearbyArmyForTransfer();
        if (nearbyArmy == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] No nearby army found.");
            return;
        }

        foreach (Unit unit in selectedUnits)
        {
            selectedSettlement.Garrison.RemoveUnit(unit);

            nearbyArmy.AddUnit(unit.Type.Name, unit.Size);
            UnityEngine.Debug.Log($"[Transfer] {unit.Type.Name} ({unit.Size}) moved to nearby army.");
        }

        selectedUnits.Clear();
        RefreshGarrisonUI();
    }
    private Army FindNearbyArmyForTransfer()
    {
        if (selectedSettlement == null)
            return null;

        Vector2Int pos = selectedSettlement.GridPosition;
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(1, 0),  // East
        new Vector2Int(-1, 0), // West
        new Vector2Int(0, 1),  // North
        new Vector2Int(0, -1), // South
        new Vector2Int(1, 1),  // NE (optional)
        new Vector2Int(-1, 1), // NW
        new Vector2Int(1, -1), // SE
        new Vector2Int(-1, -1) // SW
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = pos + dir;
            Tile tile = GridManager.Instance.GetTileAt(checkPos.x, checkPos.y);

            if (tile != null && tile.Army != null)
            {
                UnityEngine.Debug.Log($"[FindNearbyArmy] Found army at ({checkPos.x},{checkPos.y})");
                return tile.Army;
            }
        }

        UnityEngine.Debug.LogWarning("[FindNearbyArmy] No army found near settlement.");
        return null;
    }


    public void RefreshGarrisonUI()
    {
        if (selectedSettlement != null)
        {
            ShowGarrison(selectedSettlement); 
        }
    }
    public void RefreshArmyPanel(Army army)
    {
        ShowArmyPanel(army);
    }

    public void RefreshSettlementPanel(Settlement settlement)
    {
        ShowSettlementInfo(settlement); 
    }

    public void OnNewArmyButtonClicked(Settlement settlement, Player player)
    {
        if (selectedUnits.Count == 0)
        {
            UnityEngine.Debug.LogWarning("[UIManager] Cannot create new army: no units selected.");
            return;
        }
        Army newArmy = ArmyService.CreateArmyFromGarrison(settlement, player, new List<Unit>(selectedUnits));

        if (newArmy != null)
        {
            selectedUnits.Clear();
            RefreshSettlementPanel(settlement);
        }
    }

    private void OnMergeArmyButtonClicked()
    {
        if (currentArmy == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] No army selected to merge.");
            return;
        }
        currentArmy.MergeUnits();
        RefreshArmyPanel(currentArmy);
        UnityEngine.Debug.Log("[UIManager] Merged army units successfully.");
    }

    private void OnMergeGarrisonButtonClicked()
    {
        if (selectedSettlement == null)
        {
            UnityEngine.Debug.LogWarning("[UIManager] No settlement selected to merge garrison units.");
            return;
        }
        selectedSettlement.Garrison.MergeUnits();
        RefreshSettlementPanel(selectedSettlement);
        UnityEngine.Debug.Log("[UIManager] Garrison units merged successfully.");
    }
}