using UnityEngine;
using UnityEngine.EventSystems;

public class UnitCardClickHandlerUI : MonoBehaviour, IPointerClickHandler
{
    private Unit linkedUnit;
    private Settlement linkedSettlement;

    public void Initialize(Unit unit, Settlement settlement = null)
    {
        linkedUnit = unit;
        linkedSettlement = settlement;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && linkedUnit != null)
        {
            if (linkedSettlement!=null && linkedSettlement.Faction != "Player")
            {
                return;
            }

            if (linkedSettlement != null)
            {
                if (linkedSettlement.Garrison.Units.Contains(linkedUnit))
                {
                    linkedSettlement.Garrison.RemoveUnit(linkedUnit);
                    UIManager.Instance.RefreshSettlementPanel(linkedSettlement);
                }
            }
            else
            {
                ClickManager.Instance.TryDisbandUnit(linkedUnit);
            }
        }
    }
}