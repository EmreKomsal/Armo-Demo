using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PropertiesTableTabType
{
    Kaporta = 0,
    Lastik = 1,
    Motor = 2,
    Koltuk = 3,
    Ruzgarlik = 4,
}

public class PropertiesTableElement : MonoBehaviour
{
    public Button selectButton;
    public Image icon;
    public TMP_Text text;
    public GameObject selectedBG;
    public GameObject nonSelectedBG;
    public PropertiesTableTabType tabType;
    [ReadOnly] public int value;
    public string selectedInfo;


    private void Start()
    {
        selectButton.onClick.AddListener(SelectButtonPressed);
    }


    public void SelectButtonPressed()
    {
        UIControl.I.GetTab(tabType).SelectElement(this);
    }
    

    public void Select()
    {
        selectedBG.SetActive(true);
        nonSelectedBG.SetActive(false);
        var c = UIControl.I.GetTab(tabType).color;
        icon.color = c;
        text.color = c;
    }

    public void Deselect()
    {
        selectedBG.SetActive(false);
        nonSelectedBG.SetActive(true);
        icon.color = Color.black;
        text.color = Color.black;
    }
}
