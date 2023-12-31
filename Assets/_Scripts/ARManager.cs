using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ARManager : MonoBehaviour
{
    [Header("Info Components")]
    public GameObject InfoMenu;

    [Header("PistSize Components")]
    public GameObject PistMenu;
    public Button ilerleBtn;
    public Button pist_bigger;
    public Button pist_smaller;
    public TMP_Text pistSize;


    [Header("Start Components")]
    public GameObject StartMenu;
    public Button startBtn;
    public Button homeBtn;
    public Button backBtn;

    [Header("Race Components")]
    public GameObject RaceMenu;
    public TMP_Text speedText;
    public TMP_Text timerText;
    public float time = 0;

    [Header("End Components")]
    public GameObject EndMenu;
    public TMP_Text end_speedText;
    public TMP_Text end_timeText;
    public TMP_Text end_frictionText;
    public Button end_NextBtn;

    [Header("End Components")]
    public GameObject LastMenu;
    public TMP_Text menu_speedText;
    public TMP_Text menu_timeText;
    public TMP_Text menu_frictionText;
    public Button menu_homeBtn;
    public Button menu_customBtn;
    public Button menu_newcarBtn;
    public Button menu_restartBtn;


    void Start()
    {
        Debug.Log("ARManager Start method called."); // This should appear in the console when the scene starts

        // Continue with button assignments and log each step
        if (ilerleBtn != null)
        {
            Debug.Log("ilerleBtn is not null, adding listener.");
            ilerleBtn.onClick.AddListener(ARM_SpawnCar);
        }
        else
        {
            Debug.Log("ilerleBtn is null!");
        }

        if (pist_bigger != null)
            pist_bigger.onClick.AddListener(ARM_MakeRoadLonger);

        if (pist_smaller != null)
            pist_smaller.onClick.AddListener(ARM_MakeRoadShorter);

        if (startBtn != null)
            startBtn.onClick.AddListener(ARM_Start);

        if (homeBtn != null)
            homeBtn.onClick.AddListener(ARM_Home);

        if (backBtn != null)
            backBtn.onClick.AddListener(ARM_Back);

        if (end_NextBtn != null)
        {
            end_NextBtn.onClick.AddListener(ARM_LoadMenu);
        }

        if (menu_homeBtn != null)
        {
            menu_homeBtn.onClick.AddListener(ARM_Home);
        }

        if (menu_customBtn != null)
        {
            menu_customBtn.onClick.AddListener(ARM_LoadCustom);
        }

        if (menu_newcarBtn != null)
        {
            menu_newcarBtn.onClick.AddListener(ARM_NewCar);
        }

        if (menu_restartBtn != null)
        {
            menu_restartBtn.onClick.AddListener(ARM_ReloadScene);
        }
}

    public void LateUpdate()
    {
        if (RaceMenu.activeSelf)
        {
            ChangeSpeedText();
            ChangeTimerText();
            CheckFinish();
        }
    }

    #region RoadResize

    [Header("PistSize Varibeles")]
    public ResizeOnDetection currentRoad = null;
    public float initialSize = 1.0f; // The initial size of the road object
    public float maxSize = 5.0f; // The maximum size the road can be resized to
    public float minSize = 0.5f; // The minimum size the road can be resized to


    public void SetCurrentRoad(ResizeOnDetection road)
    {
        ResizeOnDetection temp_road = road;
        if (temp_road != null)
        {
            currentRoad = temp_road;
            pistSize.text = currentRoad.gameObject.transform.localScale.x.ToString();
            PistMenu.SetActive(true);
            InfoMenu.SetActive(false);
            return;
        }
        return;
    }

    public void ARM_MakeRoadLonger()
    {
        pistSize.text = currentRoad.MakeRoadLonger().ToString();
    }

    public void ARM_MakeRoadShorter()
    {
        pistSize.text = currentRoad.MakeRoadShorter().ToString();
    }

    public void ARM_SpawnCar()
    {
        Debug.Log("SpawnCar");
        currentRoad.SpawnCar();
        PistMenu.SetActive(false);
        StartMenu.SetActive(true);
    }
    #endregion

    #region StartRegion

    public void ARM_Start()
    {
        currentRoad.StartCar();
        StartMenu.SetActive(false);
        RaceMenu.SetActive(true);
    }

    public void ARM_Home()
    {
        Debug.Log("ARM_Home called.");
        GameManager.I.currentScreenType = StartScreenType.MainPanel;
        SceneManager.LoadScene(0);
    }


    public void ARM_Back() 
    {
        StartMenu.SetActive(false);
        PistMenu.SetActive(true);
    }

    #endregion

    #region RaceMenu

    private void ChangeSpeedText()
    {
        speedText.text = currentRoad.UpdateSpeed();
    }

    private void ChangeTimerText()
    {
        timerText.text = currentRoad.UpdateTimer();
    }

    private void CheckFinish() 
    {
        if (currentRoad.GetFinish())
        {
            time = currentRoad.GetTimer();
            RaceMenu.SetActive(false);
            EndMenu.SetActive(true);
            end_frictionText.text = currentRoad.UpdateFriction();
            end_speedText.text = speedText.text;
            end_timeText.text = timerText.text;
            
            
            menu_speedText.text = speedText.text;
            menu_timeText.text = timerText.text;
            menu_frictionText.text = end_frictionText.text;
        }
    }

    #endregion

    #region EndRegion

    private void ARM_LoadMenu() 
    {
        Debug.Log("ARM_Load called.");
        EndMenu.SetActive(false);
        LastMenu.SetActive(true);
    }

    #endregion

    #region MenuRegion
    private void ARM_LoadCustom() 
    {
        GameManager.I.currentScreenType = StartScreenType.EditCarPanel;
        SceneManager.LoadScene(0);
    }

    private void ARM_NewCar() 
    {
        GameManager.I.currentScreenType = StartScreenType.NewCarPanel;
        SceneManager.LoadScene(0);
    }

    private void ARM_ReloadScene()
    {
        SceneManager.LoadScene(1);
    }
    #endregion
}
