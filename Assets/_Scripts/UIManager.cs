using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Game Manager")]

    public GameManager gameManager;

    [Header("Panels")]
    public GameObject panelWelcome;
    public GameObject panelMain;
    public GameObject panelRegistered;
    public GameObject panelPlay;

    [Header("Welcome Panel")]
    public Button btnNext;

    [Header("Main Panel")]
    public Button btnRegCar;
    public Button btnPlayPanel;
    public Button btnNewCar;
    public Button btnExit;
    public Button btnSet;

    [Header("Registered Car Panel")]
    public Button btnNextCar;
    public Button btnPrevCar;
    public Button btnBack;
    public Button btnDel;
    public Button btnEdit;
    public Button btnARPreview;
    public GameObject imgCarRegistered;
    public Image imgStats;

    [Header("Play Panel")]
    public Button btnBackMain;
    public GameObject imgCarPlay;
    public Button btnPlay;

    [Header("Lists")]
    public List<GameObject> cars = new List<GameObject>();
    public int carID = 0;
    public int wheelID = 0;
    public int spoilerID = 0;
    public GameObject car;

    [Header("Car Data")]
    public CarData carData;

    [Header("Registered Car Panel Wheels and Spoilers")]
    public Button btnNextWheel;
    public Button btnPrevWheel;
    public Button btnNextSpoiler;
    public Button btnPrevSpoiler;

    string carsPath = "Cars";

    public enum PanelType
    {
        Main,
        Play,
        Registered,
        // Add more panel types here if needed
    }

    // Start is called before the first frame update
    void Start()
    {
        SetWelcomePanel();
        SetMainPanel();
        SetRegisteredPanel();
        SetPlayPanel();
        LoadPrefabs(imgCarRegistered);
        LoadPrefabs(imgCarPlay);



    }

    #region Event Sets

    // Set up the Welcome panel
    void SetWelcomePanel()
    {
        panelWelcome.SetActive(true);
        btnNext.onClick.AddListener(() => SetPanel(panelWelcome, panelMain, PanelType.Main));
    }

    // Set up the Main panel
    void SetMainPanel()
    {
        btnRegCar.onClick.AddListener(() => SetPanel(panelMain, panelRegistered, PanelType.Registered));
        btnPlayPanel.onClick.AddListener(() => SetPanel(panelMain, panelPlay, PanelType.Play));
        btnNewCar.onClick.AddListener(() => SetPanel(panelMain, panelRegistered, PanelType.Registered));
        btnExit.onClick.AddListener(ExitApp);
    }

    // Set up the Registered Car panel
    void SetRegisteredPanel()
    {
        btnARPreview.onClick.AddListener(() => SetPanel(panelRegistered, panelPlay, PanelType.Play));
        btnBack.onClick.AddListener(() => SetPanel(panelRegistered, panelMain, PanelType.Main));
    }

    // Set up the Play panel
    void SetPlayPanel()
    {
        btnPlay.onClick.AddListener(() => StartAR());
        btnBackMain.onClick.AddListener(() => SetPanel(panelPlay, panelMain, PanelType.Play));
    }

    #endregion

    #region Panel Handling

    // Switch between panels and perform specific actions based on the target panel type
    void SetPanel(GameObject prev, GameObject next, PanelType panelType)
    {
        prev.SetActive(false);
        next.SetActive(true);

        switch (panelType)
        {
            case PanelType.Play:
                if (next == panelPlay)
                {
                    // Perform specific actions when switching to the Play panel
                    Debug.Log("Switching to Play Panel");
                    // Add your Play panel specific actions here
                    LoadToList(imgCarPlay.transform);
                    SpawnCar();
                }
                else if (prev == panelPlay)
                {
                    // Perform specific actions when leaving the Play panel
                    Debug.Log("Leaving Play Panel");
                    // Add your Play panel exit actions here
                }
                break;

            case PanelType.Registered:
                if (next == panelRegistered)
                {
                    // Perform specific actions when switching to the Registered panel
                    Debug.Log("Switching to Registered Panel");
                    // Add your Registered panel specific actions here
                    LoadToList(imgCarRegistered.transform);
                    SpawnCar();
                }
                else if (prev == panelRegistered)
                {
                    // Perform specific actions when leaving the Registered panel
                    Debug.Log("Leaving Registered Panel");
                    // Add your Registered panel exit actions here
                }
                break;

            // Add more cases for other panel types if needed

            default:
                break;
        }
    }

    #endregion

    #region Car Handling

    // Navigate to the previous car in the list
    public void SetPrevCar()
    {
        if (cars.Count == 0) return;

        // Decrement the carID
        carID--;

        // If the carID becomes less than zero, wrap around to the last car in the list
        if (carID < 0)
        {
            carID = cars.Count - 1;
        }

        SpawnCar();
    }

    public void SetNextCar()
    {
        if (cars.Count == 0) return;

        // Increment the carID
        carID++;

        // If the carID becomes greater than the last index, wrap around to the first car in the list
        if (carID >= cars.Count)
        {
            carID = 0;
        }

        SpawnCar();
    }


    // Activate the selected car and deactivate the others
    public void SpawnCar()
    {
        car.SetActive(false);
        car = cars[carID];
        car.SetActive(true);

        // Update the carData reference to the CarData component of the new car
        carData = car.GetComponent<CarData>();
        if (carData == null)
        {
            Debug.LogWarning("The activated car does not have a CarData component!");
        }

        // Add listeners for the wheel and spoiler changer buttons
        btnNextWheel.onClick.AddListener(carData.SetNextWheel);
        btnPrevWheel.onClick.AddListener(carData.SetPrevWheel);
        btnNextSpoiler.onClick.AddListener(carData.SetNextSpoiler);
        btnPrevSpoiler.onClick.AddListener(carData.SetPrevSpoiler);
    }

    #endregion

    #region Resource Loading

    // Load prefabs from the "Cars" folder and instantiate them as child objects of the provided parent object
    private void LoadPrefabs(GameObject parentObject)
    {
        // Load all prefabs from the specified folder path
        GameObject[] prefabs = Resources.LoadAll<GameObject>(carsPath);

        if (prefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs found in the folder: " + carsPath);
            return;
        }

        // Instantiate each prefab as a child of the parentObject
        foreach (GameObject prefab in prefabs)
        {
            var temps = Instantiate(prefab, parentObject.transform);
            temps.SetActive(false);
        }
    }

    // Load instantiated cars into the list for further manipulation
    private void LoadToList(Transform parent)
    {
        if (cars.Count != 0)
        {
            cars.Clear();
        }

        foreach (Transform child in parent)
        {
            Debug.Log(child.name);
            // Add each child object to the list
            cars.Add(child.gameObject);
        }

        car = cars[0];
    }

    #endregion

    // Quit the application
    void ExitApp()
    {
        Application.Quit();
    }

    // Load the AR scene (Scene 1)
    void StartAR()
    {
        wheelID = carData.activeWheelID;
        spoilerID = carData.activeSpoilerID;
        // gameManager.SetCarID(carID, wheelID, spoilerID);
        SceneManager.LoadScene(1);
    }
}
