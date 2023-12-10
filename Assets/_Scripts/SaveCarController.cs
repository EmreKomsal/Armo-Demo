
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Sirenix.OdinInspector;
using UnityEngine;


public class SavedCarProps
{
    public string docPath;
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
        docPath = props.docPath;
        saveId = props.saveId;
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
        if (FindObjectsOfType<SaveCarController>().Length > 1)
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        // carCount = PlayerPrefs.GetInt("SavedCarCount", 0);
        savedCars.Clear();
        // for (int j = 0; j < carCount; j++)
        // {
        //     savedCars.Add(new SavedCarProps
        //     {
        //         saveId = j,
        //         name = PlayerPrefs.GetString("savedCar_" + j + "_name", "New Car"),
        //         kaportaId = PlayerPrefs.GetInt("savedCar_" + j + "_kaportaId", 0),
        //         lastikId = PlayerPrefs.GetInt("savedCar_" + j + "_lastikId", 0),
        //         motorId = PlayerPrefs.GetInt("savedCar_" + j + "_motorId", 0),
        //         koltukId = PlayerPrefs.GetInt("savedCar_" + j + "_koltukId", 0),
        //         ruzgarlikId = PlayerPrefs.GetInt("savedCar_" + j + "_ruzgarlikId", 0),
        //     });
        // }
    }

    public void SetCarCount(int to)
    {
        carCount = to;
    }
    
    public void AddCar(SavedCarProps newProps)
    {
        newProps.saveId = savedCars.Count;
        savedCars.Add(newProps);
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

        UIControl.I.SetWaitBG(true);
        
        AuthController.I.EditCar(savedCars[wantedIndex].docPath, newProps, task =>
        {
            if (task.IsCanceled)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsFaulted)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsCompleted)
            {
                savedCars[wantedIndex] = newProps;
                UpdatePrefs();
                UIControl.I.SetWaitBG(false);
                UIControl.I.MainPanel();
            }
        });
    }
    
    
    public void SaveCar(SavedCarProps newProps)
    {
        newProps.saveId = carCount;
        UIControl.I.SetWaitBG(true);
        AuthController.I.AddCar(newProps, task =>
        {
            if (task.IsCanceled)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsFaulted)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsCompleted)
            {
                newProps.docPath = task.Result.Id;
                savedCars.Add(newProps);
                carCount++;
                PlayerPrefs.SetInt("SavedCarCount", carCount);
                UpdatePrefs();
                UIControl.I.SetWaitBG(false);
                UIControl.I.MainPanel();
            }
        });
    }



    public void DeleteCar(SavedCarProps deleted)
    {
        UIControl.I.SetWaitBG(true);
        AuthController.I.RemoveCar(deleted.docPath, task =>
        {
            if (task.IsCanceled)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsFaulted)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsCompleted)
            {
                savedCars.Remove(deleted);
                for (int j = 0; j < savedCars.Count; j++)
                {
                    savedCars[j].saveId = j;
                }
                carCount = savedCars.Count;
                PlayerPrefs.SetInt("SavedCarCount", carCount);
                UpdatePrefs();
                UIControl.I.SetWaitBG(false);
                UIControl.I.DeleteComplete();
            }
        });
        
        
    }
    
    public void DeleteCar(int deleted)
    {
        if (deleted < 0 || deleted >= savedCars.Count)
        {
            return;
        }

        var newRef = savedCars[deleted].docPath;
        UIControl.I.SetWaitBG(true);

        AuthController.I.RemoveCar(newRef, task =>
        {
            if (task.IsCanceled)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsFaulted)
            {
                UIControl.I.SetWaitBG(false);
                return;
            }

            if (task.IsCompleted)
            {
                savedCars.RemoveAt(deleted);
                for (int j = 0; j < savedCars.Count; j++)
                {
                    savedCars[j].saveId = j;
                }
                carCount = savedCars.Count;
                PlayerPrefs.SetInt("SavedCarCount", carCount);
                UpdatePrefs();
                UIControl.I.SetWaitBG(false);
                UIControl.I.DeleteComplete();
            }
        });
        
        
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
