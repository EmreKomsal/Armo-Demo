using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StartScreenType
{
    WelcomePanel,
    MainPanel,
    NewCarPanel,
    EditCarPanel,
}

public class GameManager : SingletonNew<GameManager>
{
    string carsPath = "Cars";
    public GameObject carPrefab;
    public SavedCarProps lastCarProps;

    public BestScoresHolder bestScoresHolder;
    public int groundCount = 4;
    [Range(0f, 1f)] public float ghostColorAlpha = 0.5f;
    
    public StartScreenType currentScreenType = StartScreenType.WelcomePanel;
    public List<Color> colors;
    public string colorPropertyName = "_Color";
    public int colorMaterialIndex = 0;
    
    public 

    void Start()
    {
        // Register the OnSceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Called when the scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene has ID 1
        if (scene.buildIndex == 2)
        {
            // Load the prefab in the scene with ID 1
            LoadPrefab(transform); // Assuming the parentObject is this GameManager's GameObject
        }
    }

    public void LoadPrefab(Transform parentObject)
    {
        // Load all prefabs from the specified folder path
        GameObject[] prefabs = Resources.LoadAll<GameObject>(carsPath);

        if (prefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs found in the folder: " + carsPath);
            return;
        }

        int i = 0;
        
        // Debug.Log(lastCarProps.kaportaId);
        // Instantiate each prefab as a child of the parentObject
        foreach (GameObject prefab in prefabs)
        {
            if (i == lastCarProps.kaportaId)
            {
                carPrefab = Instantiate(prefab, parentObject);
                carPrefab.SetActive(false);
                // After instantiating the car prefab
                CarData carDataComponent = carPrefab.GetComponent<CarData>();
                if (carDataComponent != null)
                {
                    carDataComponent.SetPartsByID(lastCarProps.lastikId, lastCarProps.ruzgarlikId, lastCarProps.renkId,
                        lastCarProps.stilId, false);
                }
            }
            i++;
        }
        
        foreach (var ground in bestScoresHolder.groundToBestCars.Keys)
        {
            var bestCar = bestScoresHolder.GetBestCar(ground);
            if (bestCar == null)
            {
                continue;
            }
            
            var spawnedCar = Instantiate(prefabs[bestCar.kaportaId], parentObject);

            spawnedCar.SetActive(false);
            // After instantiating the car prefab
            CarData carData = spawnedCar.GetComponent<CarData>();
            if (carData != null)
            {
                carData.SetPartsByID(bestCar.lastikId, bestCar.ruzgarlikId, bestCar.renkId,
                    bestCar.stilId, true);
            }
            
            bestScoresHolder.groundToCarPrefabs[ground] = spawnedCar;
        }
    }
    

    public void SetCarProps(SavedCarProps newCarProps)
    {
        lastCarProps = newCarProps;
    }

    public bool LoadGhostCar(PartEffectController.GroundType wantedType, Transform parent, bool active)
    {
        var p = bestScoresHolder.GetBestCarObject(wantedType);
        if (p != null)
        {
            p.transform.SetParent(parent, false);
            p.SetActive(active);
            return true;
        }

        return false;
    }
    
    public void LoadCarPrefab(Transform parent, bool active)
    {
        if (carPrefab != null)
        {
            carPrefab.transform.SetParent(parent, false);
            carPrefab.SetActive(active);
        }

    }
  

}
