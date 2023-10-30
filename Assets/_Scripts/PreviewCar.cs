using System.Collections.Generic;
using UnityEngine;

public class PreviewCar : MonoBehaviour
{
    public GameObject parent;
    public List<GameObject> tires;
    public List<GameObject> spoilers;

    
    
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
    
    public void Activate()
    {
        parent.SetActive(true);
    }

    public void Deactivate()
    {
        parent.SetActive(false);
    }
}
