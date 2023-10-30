using System.Collections.Generic;
using UnityEngine;

public class PreviewController : SingletonNew<PreviewController>
{
    public GameObject camObj;
    public GameObject sahneObj;

    public List<PreviewCar> previewCars;
    public List<GameObject> previewLastik;
    public List<GameObject> previewMotor;
    public List<GameObject> previewKoltuk;
    public List<GameObject> previewRuzgarlik;


    private bool isActive = false;

    public void ActivatePreview()
    {
        camObj.SetActive(true);
        sahneObj.SetActive(true);
        isActive = true;
    }

    public void DeactivatePreview()
    {
        camObj.SetActive(false);
        sahneObj.SetActive(false);
        isActive = false;
    }

    public void SetPreviewOnlyCar(SavedCarProps savedCar)
    {
        SetPreview(false, savedCar, 0);
    }
    
    public void SetPreview(bool isCar, SavedCarProps savedCar, int tabIndex)
    {
        CloseAll();
        if (isCar || tabIndex == 0)
        {
            previewCars[savedCar.kaportaId].Activate();
            previewCars[savedCar.kaportaId].SetSpoilers(savedCar.ruzgarlikId);
            previewCars[savedCar.kaportaId].SetTires(savedCar.lastikId);
        }
        else if (tabIndex == 1)
        {
            previewLastik[savedCar.lastikId].SetActive(true);
        }
        else if (tabIndex == 2)
        {
            previewMotor[savedCar.motorId].SetActive(true);
        }
        else if (tabIndex == 3)
        {
            previewKoltuk[savedCar.koltukId].SetActive(true);
        }
        else if (tabIndex == 4)
        {
            previewRuzgarlik[savedCar.ruzgarlikId].SetActive(true);            
        }
        else
        {
            previewCars[savedCar.kaportaId].Activate();
            previewCars[savedCar.kaportaId].SetSpoilers(savedCar.ruzgarlikId);
            previewCars[savedCar.kaportaId].SetTires(savedCar.lastikId);
        }
    }

    public void CloseAll()
    {
        foreach (var previewCar in previewCars)
        {
            previewCar.Deactivate();
        }

        foreach (var o in previewLastik)
        {
            o.SetActive(false);
        }
        foreach (var o in previewMotor)
        {
            o.SetActive(false);
        }
        foreach (var o in previewKoltuk)
        {
            o.SetActive(false);
        }
        foreach (var o in previewRuzgarlik)
        {
            o.SetActive(false);
        }
    }
}
