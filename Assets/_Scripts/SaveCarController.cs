
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class SavedCarProps
{
    public int saveId = 0;
    public string name = "Yeni Araç";
    public int kaportaId = 0;
    public int lastikId = 0;
    public int motorId = 0;
    public int koltukId = 0;
    public int ruzgarlikId = 0;

    public SavedCarProps()
    {
        
    }
    public SavedCarProps(SavedCarProps props)
    {
        name = props.name;
        kaportaId = props.kaportaId;
        lastikId = props.lastikId;
        motorId = props.motorId;
        koltukId = props.koltukId;
        ruzgarlikId = props.ruzgarlikId;
    }
}

public class SaveCarController : SingletonNew<SaveCarController>
{

    private int carCount;

    private List<SavedCarProps> savedCars = new List<SavedCarProps>();

    public int GetCarCount()
    {
        return carCount;
    }
    
    private void Start()
    {
        carCount = PlayerPrefs.GetInt("SavedCarCount", 0);
        savedCars.Clear();
        for (int j = 0; j < carCount; j++)
        {
            savedCars.Add(new SavedCarProps
            {
                saveId = j,
                name = PlayerPrefs.GetString("savedCar_" + j + "_name", "New Car"),
                kaportaId = PlayerPrefs.GetInt("savedCar_" + j + "_kaportaId", 0),
                lastikId = PlayerPrefs.GetInt("savedCar_" + j + "_lastikId", 0),
                motorId = PlayerPrefs.GetInt("savedCar_" + j + "_motorId", 0),
                koltukId = PlayerPrefs.GetInt("savedCar_" + j + "_koltukId", 0),
                ruzgarlikId = PlayerPrefs.GetInt("savedCar_" + j + "_ruzgarlikId", 0),
            });
        }
    }

    public SavedCarProps GetCarProps(int wantedIndex)
    {
        if (wantedIndex < 0 || wantedIndex >= savedCars.Count)
        {
            return null;
        }
        return new SavedCarProps(savedCars[wantedIndex]);
    }

    public void EditCar(int wantedIndex, SavedCarProps newProps)
    {
        if (wantedIndex < 0 || wantedIndex >= savedCars.Count)
        {
            return;
        }

        savedCars[wantedIndex] = newProps;
        UpdatePrefs();
    }
    
    
    public void SaveCar(SavedCarProps newProps)
    {
        newProps.saveId = carCount;
        savedCars.Add(newProps);
        carCount++;
        PlayerPrefs.SetInt("SavedCarCount", carCount);
        UpdatePrefs();
    }


    public void DeleteCar(SavedCarProps deleted)
    {
        savedCars.Remove(deleted);
        for (int j = 0; j < savedCars.Count; j++)
        {
            savedCars[j].saveId = j;
        }
        carCount = savedCars.Count;
        PlayerPrefs.SetInt("SavedCarCount", carCount);
        UpdatePrefs();
    }
    
    public void DeleteCar(int deleted)
    {
        if (deleted < 0 || deleted >= savedCars.Count)
        {
            return;
        }
        
        savedCars.RemoveAt(deleted);
        for (int j = 0; j < savedCars.Count; j++)
        {
            savedCars[j].saveId = j;
        }
        carCount = savedCars.Count;
        PlayerPrefs.SetInt("SavedCarCount", carCount);
        UpdatePrefs();
    }
    
    public void UpdatePrefs()
    {
        for (int j = 0; j < savedCars.Count; j++)
        {
            var props = savedCars[j];
            PlayerPrefs.SetString("savedCar_" + j + "_name", props.name);
            PlayerPrefs.SetInt("savedCar_" + j + "_kaportaId", props.kaportaId);
            PlayerPrefs.SetInt("savedCar_" + j + "_lastikId", props.lastikId);
            PlayerPrefs.SetInt("savedCar_" + j + "_motorId", props.motorId);
            PlayerPrefs.SetInt("savedCar_" + j + "_koltukId", props.koltukId);
            PlayerPrefs.SetInt("savedCar_" + j + "_ruzgarlikId", props.ruzgarlikId);
        }
    }
}
