
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickElement : MonoBehaviour
{
    [ReadOnly] public int index = 0;
    public Button notPickedButton;
    public Button pickedButton;

    public void Pick()
    {
        UIControl.I.PickColor(index);
    }
    
    public void ToPicked()
    {
        notPickedButton.gameObject.SetActive(true);
        pickedButton.gameObject.SetActive(false);   
    }

    public void ToNotPicked()
    {
        notPickedButton.gameObject.SetActive(true);
        pickedButton.gameObject.SetActive(false);
    }
}
