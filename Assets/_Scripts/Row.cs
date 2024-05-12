using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Row : MonoBehaviour
{
    public GameObject obj;
    public Image placementBg;
    public TMP_Text placementText;
    public Image hizBg;
    public TMP_Text hizText;
    public Image sureBg;
    public TMP_Text sureText;
    public Image agirlikBg;
    public TMP_Text agirlikText;

    public bool IsPlayer { get; private set; } = false;

    public void SetActive(bool to)
    {
        obj.SetActive(to);
    }

    public void SetRecord(int placement, bool isPlayer, SavedCarProps props, float speed)
    {
        if (isPlayer)
        {
            placementBg.color = ARManager.I.menuTableController.playerPlaceBgColor;
            hizBg.color = ARManager.I.menuTableController.playerStatBgColor;
            sureBg.color = ARManager.I.menuTableController.playerStatBgColor;
            agirlikBg.color = ARManager.I.menuTableController.playerStatBgColor;
        }
        else
        {
            placementBg.color = ARManager.I.menuTableController.npcPlaceBgColor;
            hizBg.color = ARManager.I.menuTableController.npcStatBgColor;
            sureBg.color = ARManager.I.menuTableController.npcStatBgColor;
            agirlikBg.color = ARManager.I.menuTableController.npcStatBgColor;
        }

        placementText.SetText(placement.ToString());
        var projSpeed = PartEffectController.I.GetProjectedSpeed(speed);
        hizText.SetText((projSpeed.ToString("F1") + " km/sa"));

        agirlikText.SetText(Mathf.FloorToInt(PartEffectController.I.GetMass(props)).ToString() + " Kg");

        var elapsed = PartEffectController.I.GetDuration(speed);
        sureText.SetText((elapsed.ToString("F") + " s"));
    }
}
