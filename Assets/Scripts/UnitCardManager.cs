using UnityEngine;

public class UnitCardManager : MonoBehaviour
{
    private Unit linkedUnit;

    public void Initialize(Unit unit)
    {
        linkedUnit = unit;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (linkedUnit != null)
            {
                ClickManager.Instance.TryDisbandUnit(linkedUnit);
            }
        }
    }
}