using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UIManagerSmokeTests
{
    private GameObject uiManagerObj;
    private UIManager UIManager;
    private Settlement testSettlement;
    private Settlement enemySettlement;
    private Player testPlayer;
    private Army testArmy;

    [SetUp]
    public void SetUp()
    {
        UnitDatabase.UnitTypes.Clear(); 
        var spearmanType = new UnitType("Militia Spearman", 80, 10, 3, 2, 4);
        var archerType = new UnitType("Militia Archer", 70, 8, 2, 1, 4);
        var warriorType = new UnitType("Enemy Warrior", 100, 12, 5, 3, 3);
        UnitDatabase.UnitTypes.Add(spearmanType.Name, spearmanType);
        UnitDatabase.UnitTypes.Add(archerType.Name, archerType);
        UnitDatabase.UnitTypes.Add(warriorType.Name, warriorType);

        uiManagerObj = new GameObject("UIManager");
        UIManager = uiManagerObj.AddComponent<UIManager>();

        UIManager.endTurnButton = CreateButton("EndTurnButton");
        UIManager.turnText = CreateText("TurnText");
        UIManager.goldText = CreateText("GoldText");
        UIManager.terrainInfoText = CreateText("TerrainInfoText");

        UIManager.settlementPanel = CreatePanel("SettlementPanel");
        UIManager.nameText = CreateText("NameText");
        UIManager.populationText = CreateText("PopulationText");
        UIManager.incomeText = CreateText("IncomeText");
        UIManager.growthText = CreateText("GrowthText");
        UIManager.barracksLevelText = CreateText("BarracksLevelText");
        UIManager.archeryRangeLevelText = CreateText("ArcheryRangeLevelText");
        UIManager.stablesLevelText = CreateText("StablesLevelText");
        UIManager.farmLevelText = CreateText("FarmLevelText");
        UIManager.marketLevelText = CreateText("MarketLevelText");
        UIManager.constructionText = CreateText("ConstructionText");
        UIManager.barracksUpgradeButton = CreateButton("BarracksUpgradeButton");
        UIManager.mergeGarrisonButton = CreateButton("MergeGarrisonButton");
        UIManager.newArmyButton = CreateButton("NewArmyButton");

        UIManager.armyPanel = CreatePanel("ArmyPanel");
        UIManager.unitGrid = CreateGrid("UnitGrid");
        UIManager.armyTitleText = CreateText("ArmyTitleText");
        UIManager.unitCardPrefab = CreateUnitCardPrefab("UnitCardPrefab");
        UIManager.mergeButton = CreateButton("MergeButton");

        UIManager.garrisonPanel = CreatePanel("GarrisonPanel");
        UIManager.garrisonGrid = CreateGrid("GarrisonGrid");
        UIManager.garrisonUnitCardPrefab = CreateUnitCardPrefab("GarrisonUnitCardPrefab");
        UIManager.transferToArmyButton = CreateButton("TransferToArmyButton");

        UIManager.recruitmentPanel = CreatePanel("RecruitmentPanel");
        UIManager.recruitButton = CreateButton("RecruitButton");
        UIManager.barracksColumn = CreateGrid("BarracksColumn");
        UIManager.archeryRangeColumn = CreateGrid("ArcheryRangeColumn");
        UIManager.stablesColumn = CreateGrid("StablesColumn");
        UIManager.recruitmentQueueText = CreateText("RecruitmentQueueText");

        UIManager.enemySettlementPanel = CreatePanel("EnemySettlementPanel");
        UIManager.enemyGarrisonGrid = CreateGrid("EnemyGarrisonGrid");
        UIManager.enemyGarrisonUnitCardPrefab = CreateUnitCardPrefab("EnemyGarrisonUnitCardPrefab");

        testPlayer = new Player("Tester", true);

        testSettlement = new Settlement("UItown", "Player", new Vector2Int(2, 3));
        testSettlement.Garrison.AddUnit(new Unit(
            new UnitType("Militia Spearman", 80, 10, 3, 2, 4), 80));
        testSettlement.Garrison.AddUnit(new Unit(
            new UnitType("Militia Archer", 70, 8, 2, 1, 4), 70));

        enemySettlement = new Settlement("EnemyTown", "AI", new Vector2Int(5, 5));
        enemySettlement.Garrison.AddUnit(new Unit(
            new UnitType("Enemy Warrior", 100, 12, 5, 3, 3), 100));

        testArmy = new Army("Player");
        testArmy.AddUnit("Militia Spearman", 50);
        testArmy.AddUnit("Militia Archer", 40);
    }

    [UnityTest]
    public IEnumerator UIManager_Initializes_WithoutCrashing()
    {
        Assert.IsNotNull(UIManager.Instance);
        Assert.AreEqual(UIManager, UIManager.Instance);
        yield return null;
    }

    [UnityTest]
    public IEnumerator UpdateUI_DoesNotCrash_WhenTurnManagerNull()
    {
        Assert.DoesNotThrow(() => UIManager.UpdateUI());
        yield return null;

        Assert.IsTrue(UIManager.turnText.text.Contains("Turn"));
        Assert.IsTrue(UIManager.goldText.text.Contains("Gold"));
    }

    [UnityTest]
    public IEnumerator ShowSettlementInfo_DisplaysSettlementData()
    {
        UIManager.ShowSettlementInfo(testSettlement);
        yield return null;

        Assert.IsTrue(UIManager.settlementPanel.activeSelf, "Settlement panel should be visible");
        Assert.IsTrue(UIManager.nameText.text.Contains("UItown"), "Settlement name should be displayed");
        Assert.IsTrue(UIManager.populationText.text.Contains("Population"), "Population should be displayed");
        Assert.Greater(UIManager.garrisonGrid.childCount, 0, "Garrison units should be spawned");
    }

    [UnityTest]
    public IEnumerator ShowEnemySettlementInfo_DisplaysEnemyData()
    {
        UIManager.ShowEnemySettlementInfo(enemySettlement);
        yield return null;

        Assert.IsTrue(UIManager.enemySettlementPanel.activeSelf, "Enemy settlement panel should be visible");
        Assert.IsFalse(UIManager.settlementPanel.activeSelf, "Player settlement panel should be hidden");
        Assert.Greater(UIManager.enemyGarrisonGrid.childCount, 0, "Enemy garrison should be displayed");
    }

    [UnityTest]
    public IEnumerator ShowArmyPanel_DisplaysArmyUnits()
    {
        UIManager.ShowArmyPanel(testArmy);
        yield return null;

        Assert.IsTrue(UIManager.armyPanel.activeSelf, "Army panel should be visible");
        Assert.IsTrue(UIManager.armyTitleText.text.Contains("Player"), "Army title should show faction");
        Assert.AreEqual(15, UIManager.unitGrid.childCount, "Should have 15 slots");
    }

    [UnityTest]
    public IEnumerator ShowGarrison_PopulatesUnitCards()
    {
        UIManager.ShowGarrison(testSettlement);
        yield return null;

        Assert.IsTrue(UIManager.garrisonPanel.activeSelf, "Garrison panel should be visible");
        int expectedSlots = Garrison.MaxUnits;
        Assert.AreEqual(expectedSlots, UIManager.garrisonGrid.childCount,
            $"Should have {expectedSlots} slots in garrison grid");
    }

    [UnityTest]
    public IEnumerator HideSettlementInfo_HidesPanel()
    {
        UIManager.ShowSettlementInfo(testSettlement);
        yield return null;

        UIManager.HideSettlementInfo();
        yield return null;

        Assert.IsFalse(UIManager.settlementPanel.activeSelf, "Settlement panel should be hidden");
    }

    [UnityTest]
    public IEnumerator SetTerrainInfo_UpdatesText()
    {
        UIManager.SetTerrainInfo("Grassland");
        yield return null;

        Assert.IsTrue(UIManager.terrainInfoText.text.Contains("Grassland"),
            "Terrain info should display terrain name");
    }

    [UnityTest]
    public IEnumerator UpgradeButtons_StartConstruction()
    {
        UIManager.ShowSettlementInfo(testSettlement);
        yield return null;

        UIManager.UpgradeBarracks();
        yield return null;

        Assert.IsTrue(testSettlement.IsConstructing(), "Construction should be in progress");
        Assert.AreEqual("Barracks", testSettlement.BuildingInProgress);
        Assert.IsTrue(UIManager.constructionText.text.Contains("Barracks"),
            "Construction text should show building name");
    }

    [UnityTest]
    public IEnumerator MultipleUIOperations_WorkInSequence()
    {
        UIManager.UpdateUI();
        yield return null;
        UIManager.ShowSettlementInfo(testSettlement);
        yield return null;
        UIManager.ShowGarrison(testSettlement);
        yield return null;
        UIManager.ShowArmyPanel(testArmy);
        yield return null;
        UIManager.HideArmyPanel();
        yield return null;
        UIManager.ShowEnemySettlementInfo(enemySettlement);
        yield return null;
        UIManager.HideEnemySettlementInfo();
        yield return null;

        Assert.Pass("All UIMnager operations completed without problems");
    }

    [Test]
    public void OnEndTurn_DoesNotCrash_WhenCalled()
    {
        Assert.DoesNotThrow(() => UIManager.OnEndTurn());
    }

    private GameObject CreatePanel(string name)
    {
        var panel = new GameObject(name);
        panel.AddComponent<RectTransform>();
        panel.AddComponent<CanvasRenderer>();
        panel.SetActive(false);
        return panel;
    }

    private Transform CreateGrid(string name)
    {
        var grid = new GameObject(name);
        var rect = grid.AddComponent<RectTransform>();
        grid.AddComponent<GridLayoutGroup>();
        return rect;
    }

    private Button CreateButton(string name)
    {
        var buttonObj = new GameObject(name);
        buttonObj.AddComponent<RectTransform>();
        var button = buttonObj.AddComponent<Button>();
        buttonObj.AddComponent<Image>(); 
        return button;
    }

    private TMP_Text CreateText(string name)
    {
        var textObj = new GameObject(name);
        textObj.AddComponent<RectTransform>();
        return textObj.AddComponent<TextMeshProUGUI>();
    }

    private GameObject CreateUnitCardPrefab(string name)
    {
        var prefab = new GameObject(name);
        prefab.AddComponent<RectTransform>();
        prefab.AddComponent<Image>();
        prefab.AddComponent<Button>();

        var sizeTextObj = new GameObject("UnitSizeText");
        sizeTextObj.transform.SetParent(prefab.transform);
        sizeTextObj.AddComponent<RectTransform>();
        sizeTextObj.AddComponent<TextMeshProUGUI>();

        var nameTextObj = new GameObject("UnitNameText");
        nameTextObj.transform.SetParent(prefab.transform);
        nameTextObj.AddComponent<RectTransform>();
        nameTextObj.AddComponent<TextMeshProUGUI>();

        return prefab;
    }

    [TearDown]
    public void TearDown()
    {
        if (UIManager != null)
        {
            CleanupGrid(UIManager.garrisonGrid);
            CleanupGrid(UIManager.enemyGarrisonGrid);
            CleanupGrid(UIManager.unitGrid);
            CleanupGrid(UIManager.barracksColumn);
            CleanupGrid(UIManager.archeryRangeColumn);
            CleanupGrid(UIManager.stablesColumn);
        }

        if (uiManagerObj != null)
            Object.DestroyImmediate(uiManagerObj);

        typeof(UIManager)
            .GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            ?.SetValue(null, null);

        UnitDatabase.UnitTypes.Clear();
    }

    private void CleanupGrid(Transform grid)
    {
        if (grid == null) return;

        foreach (Transform child in grid)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }
}