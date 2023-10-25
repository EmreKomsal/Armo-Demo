using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    string carsPath = "Cars";
    public int carID = 0;
    public int wheelID = 0;
    public int spoilerID = 0;
    public GameObject carPrefab;

    void Start()
    {
        // Register the OnSceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Called when the scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene has ID 1
        if (scene.buildIndex == 1)
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

        // Instantiate each prefab as a child of the parentObject
        foreach (GameObject prefab in prefabs)
        {
            if (i == carID)
            {
                carPrefab = Instantiate(prefab, parentObject);
                carPrefab.SetActive(false);
                // After instantiating the car prefab
                CarData carDataComponent = carPrefab.GetComponent<CarData>();
                if (carDataComponent != null)
                {
                    carDataComponent.SetPartsByID(wheelID, spoilerID);
                }
            }
            i++;
        }
    }

    public void SetCarID(int newID, int new_wheelID, int new_spoilerID)
    {
        carID = newID;
        spoilerID = new_spoilerID;
        wheelID = new_wheelID;
    }

    public void LoadCarPrefab(Transform parent)
    {
        if (carPrefab != null)
        {
            carPrefab.transform.SetParent(parent, false);
            carPrefab.SetActive(true);
        }

    }


}
