using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PropertiesTableTab
{
    public Button tabHeaderButton;
    public GameObject tabHeaderSelectedBG;
    public GameObject tabHeaderNonSelectedBG;
    public GameObject tabParent;
    public PropertiesTableTabType tabType;
    public Color color;
    public TMP_Text infoText;
    public List<PropertiesTableElement> elements;

    [ReadOnly] public int value;

    public void HeaderButtonPressed()
    {
        UIControl.I.CloseAllTabs();
        Select();
    }

    public void Select()
    {
        tabParent.SetActive(true);
        tabHeaderSelectedBG.SetActive(true);
        tabHeaderNonSelectedBG.SetActive(false);
        UIControl.I.SetLastTab((int)tabType);
    }

    public void Deselect()
    {
        tabParent.SetActive(false);
        tabHeaderSelectedBG.SetActive(false);
        tabHeaderNonSelectedBG.SetActive(true);
    }
    
    public void Init(int newValue = 0)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].value = i;
        }
        value = newValue;
        SelectElement(elements[value]);
        tabHeaderButton.onClick.AddListener(HeaderButtonPressed);
    }
    
    public void SelectElement(PropertiesTableElement selectedElement)
    {
        DeselectAll();
        selectedElement.Select();
        infoText.text = selectedElement.selectedInfo;
        value = selectedElement.value;
        UIControl.I.SetTabValue(tabType, value);
    }

    private void DeselectAll()
    {
        foreach (var element in elements)
        {
            element.Deselect();
        }
    }
}
