using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArmyPanelUI : MonoBehaviour
{
    public GameObject unitCardPrefab;
    public Transform unitGrid;
    public TMP_Text titleText;

    public void ShowArmy(Army army)
    {
        titleText.text = army.Faction + " Army";
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
            nameText.text = unit.Type.ToString();

            UnitCardClickHandlerUI handler = card.AddComponent<UnitCardClickHandlerUI>();
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
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}
