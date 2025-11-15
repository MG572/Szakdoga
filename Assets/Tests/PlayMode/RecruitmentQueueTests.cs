using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class RecruitmentQueueTests
{
    private Settlement testSettlement;
    private UnitType spearmanType;

    [SetUp]
    public void SetUp()
    {
        spearmanType = new UnitType(
            "Militia Spearman",
            defaultSize: 100,
            damage: 10,
            meleeArmor: 5,
            rangedArmor: 2,
            speed: 3
        );
        if (!UnitDatabase.UnitTypes.ContainsKey(spearmanType.Name))
            UnitDatabase.UnitTypes.Add(spearmanType.Name, spearmanType);

        testSettlement = new Settlement("Testville", "Player", new Vector2Int(0, 0));
    }

    [UnityTest]
    public IEnumerator RecruitmentQueue_AddsUnitToGarrison_OnProcess()
    {
        bool added = testSettlement.AddToRecruitmentQueue(spearmanType);
        Assert.IsTrue(added, "Unit should be added to recruitment queue.");
        int initialCount = testSettlement.Garrison.UnitCount;
        testSettlement.ProcessRecruitmentQueue();
        yield return null;
        Assert.AreEqual(initialCount + 1, testSettlement.Garrison.UnitCount,
            "Garrison should contain one more unit after processing queue.");
        Assert.AreEqual(0, testSettlement.RecruitmentQueue.Count,
            "Recruitment queue should be cleared after processing.");

        Unit recruited = testSettlement.Garrison.Units[0];
        Assert.AreEqual(spearmanType.Name, recruited.Type.Name, "Recruited unit type should match queued type.");
        Assert.AreEqual(spearmanType.DefaultSize, recruited.Size, "Recruited unit size should match type default.");
    }

    [TearDown]
    public void TearDown()
    {
        UnitDatabase.UnitTypes.Clear();
    }
}
