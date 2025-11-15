using NUnit.Framework;
using UnityEngine;

public class SettlementBuildingTests
{
    private Settlement settlement;

    [SetUp]
    public void SetUp()
    {
        settlement = new Settlement("Testville", "Tester", new Vector2Int(0, 0));
    }
    [Test]
    public void FarmUpgrade_IncreasesLevelAndResetsConstruction()
    {
        int initialFarmLevel = settlement.FarmLevel;
        settlement.BuildingInProgress = "Farm";
        settlement.ConstructionTimeRemaining = 1;
        settlement.AdvanceConstruction();
        Assert.AreEqual(initialFarmLevel + 1, settlement.FarmLevel, "Farm should be upgraded by one level.");
        Assert.IsNull(settlement.BuildingInProgress, "Construction should be cleared after upgrade.");
    }
    [Test]
    public void BarracksUpgrade_CompletesAfterConstructionTime()
    {
        int initialLevel = settlement.BarracksLevel;
        settlement.BuildingInProgress = "Barracks";
        settlement.ConstructionTimeRemaining = 2;
        settlement.AdvanceConstruction();
        Assert.AreEqual(initialLevel, settlement.BarracksLevel, "Barracks should not upgrade mid-construction.");
        Assert.AreEqual(1, settlement.ConstructionTimeRemaining, "Construction time should decrease.");
        settlement.AdvanceConstruction();
        Assert.AreEqual(initialLevel + 1, settlement.BarracksLevel, "Barracks level should have increased by 1.");
        Assert.IsNull(settlement.BuildingInProgress, "BuildingInProgress should be null after completion.");
    }
    [Test]
    public void AdvanceConstruction_DoesNothing_WhenNoBuildingInProgress()
    {
        settlement.BuildingInProgress = null;
        settlement.ConstructionTimeRemaining = 0;
        int beforeBarracks = settlement.BarracksLevel;
        settlement.AdvanceConstruction();
        Assert.AreEqual(beforeBarracks, settlement.BarracksLevel, "Building level should remain unchanged.");
    }
}
