using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GOList
{
    public List<GameObject> gameObjects;
}

public class PreviewCar : MonoBehaviour
{
    public GameObject parent;
    public List<Renderer> colorChangedRends;
    public List<GameObject> tires;
    public List<GameObject> spoilers;
    public List<GOList> stils;

    private MaterialPropertyBlock propBlock;

    public void SetTires(int index)
    {
        foreach (var tire in tires)
        {
            tire.SetActive(false);
        }
        tires[index].SetActive(true);
    }

    public void SetSpoilers(int index)
    {
        foreach (var spoiler in spoilers)
        {
            spoiler.SetActive(false);
        }
        spoilers[index].SetActive(true);
    }

    public void SetStil(int index)
    {
        foreach (var stil in stils)
        {
            foreach (var o in stil.gameObjects)
            {
                o.SetActive(false);
            }
        }

        foreach (var o in stils[index].gameObjects)
        {
            o.SetActive(true);
        }
    }

    public void SetColor(int index)
    {
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        var propName = GameManager.I.colorPropertyName;
        var matIndex = GameManager.I.colorMaterialIndex;

        foreach (var rend in colorChangedRends)
        {
            rend.GetPropertyBlock(propBlock, matIndex);
            propBlock.SetColor(propName, GameManager.I.colors[index]);
            rend.SetPropertyBlock(propBlock, matIndex);
        }
    }
    
    public void Activate()
    {
        parent.SetActive(true);
    }

    public void Deactivate()
    {
        parent.SetActive(false);
    }
}
