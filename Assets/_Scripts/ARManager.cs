using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Firestore;
using TMPro;

[Serializable]
public class MenuTableController
{
    public TMP_Text groundTypeText;
    public List<Row> rows;
    public GameObject failObj;
    public Row failRow;

    public Color playerPlaceBgColor;
    public Color playerStatBgColor;
    public Color npcPlaceBgColor;
    public Color npcStatBgColor;


    public void SetMenu(SavedCarProps currentProps, ResizeOnDetection currentRoad)
    {
        var firstRowListOpenCount = 1;
        var didFail = false;
        groundTypeText.SetText(currentRoad.roadName);
        if (!GameManager.I.bestScoresHolder.groundToBestCars.ContainsKey(currentRoad.groundType) ||
            GameManager.I.bestScoresHolder.groundToBestCars[currentRoad.groundType].Count == 0)
        {
            foreach (var row in rows)
            {
                row.SetActive(false);
            }

            rows[0].SetActive(true);
            rows[0].SetRecord(1, true, currentProps,
                PartEffectController.I.GetSpeed(currentProps, currentRoad.groundType));
            failObj.SetActive(false);
        }
        else
        {
            var playerSpeed = PartEffectController.I.GetSpeed(currentProps, currentRoad.groundType);
            var count = 0;
            var playerPlacement = -1;
            for (int i = 0; i < GameManager.I.bestScoresHolder.groundToBestCars[currentRoad.groundType].Count; i++)
            {
                if (playerSpeed >= GameManager.I.bestScoresHolder.groundToBestCars[currentRoad.groundType][i].speed)
                {
                    playerPlacement = i;
                    break;
                }

                count++;

                if (count >= 9)
                {
                    break;
                }
            }

            foreach (var row in rows)
            {
                row.SetActive(false);
            }

            if (playerPlacement < 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    var score = GameManager.I.bestScoresHolder.groundToBestCars[currentRoad.groundType][i];
                    rows[i].SetActive(true);
                    rows[i].SetRecord(i + 1, false, score.carProps, score.speed);
                }

                failObj.SetActive(true);
                failRow.SetActive(true);
                failRow.SetRecord(-1, true, currentProps,
                    PartEffectController.I.GetSpeed(currentProps, currentRoad.groundType));
            }
            else
            {
                failObj.SetActive(false);
                for (int i = 0; i < playerPlacement; i++)
                {
                    var score = GameManager.I.bestScoresHolder.groundToBestCars[currentRoad.groundType][i];
                    rows[i].SetActive(true);
                    rows[i].SetRecord(i + 1, false, score.carProps, score.speed);
                }

                rows[playerPlacement].SetActive(true);
                rows[playerPlacement].SetRecord(playerPlacement+1, true, currentProps,
                    PartEffectController.I.GetSpeed(currentProps, currentRoad.groundType));
                for (int i = playerPlacement + 1; i < 10; i++)
                {
                    var score = GameManager.I.bestScoresHolder.groundToBestCars[currentRoad.groundType][i - 1];
                    rows[i].SetActive(true);
                    rows[i].SetRecord(i + 1, false, score.carProps, score.speed);
                }
            }
        }
    }
}

public class ARManager : SingletonNew<ARManager>
{
    [Header("Info Components")]
    public GameObject InfoMenu;
    // public GameObject arInfoMenu;
    // public GameObject arDangerInfoMenu;
    public Button infoBackButton;
    // public Button anladimButton;

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
    public TMP_Text end_agirlikText;
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

    public MenuTableController menuTableController;
    
    void Start()
    {
        Debug.Log("ARManager Start method called."); // This should appear in the console when the scene starts
        InfoMenu.SetActive(true);
        // arInfoMenu.SetActive(false);
        // arDangerInfoMenu.SetActive(true);
        // DidSkipDangerInfo = false;
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
        
        infoBackButton.onClick.AddListener(ARM_Home);
        // anladimButton.onClick.AddListener(SkipDangerInfo);
}

    // public void LateUpdate()
    // {
    //     if (RaceMenu.activeSelf)
    //     {
    //         ChangeSpeedText();
    //         ChangeTimerText();
    //         CheckFinish();
    //     }
    // }

    #region RoadResize


    [Header("PistSize Varibeles")]
    public ResizeOnDetection currentRoad = null;
    public float initialSize = 1.0f; // The initial size of the road object
    public float maxSize = 5.0f; // The maximum size the road can be resized to
    public float minSize = 0.5f; // The minimum size the road can be resized to


    // public bool DidSkipDangerInfo { get; private set; } = false;
    
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

    // public void SkipDangerInfo()
    // {
    //     arDangerInfoMenu.SetActive(false);
    //     arInfoMenu.SetActive(true);
    //     DidSkipDangerInfo = true;
    // }

    public void ARM_MakeRoadLonger()
    {
        currentRoad.MakeRoadLonger();
        pistSize.text = currentRoad.lastSize.ToString("F1");
    }

    public void ARM_MakeRoadShorter()
    {
        currentRoad.MakeRoadShorter();
        pistSize.text = currentRoad.lastSize.ToString("F1");
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
        GameManager.I.currentScreenType = StartScreenType.MainPanel;
        SceneManager.LoadScene(1);
    }


    public void ARM_Back() 
    {
        StartMenu.SetActive(false);
        PistMenu.SetActive(true);
    }

    #endregion

    #region RaceMenu

    // private void ChangeSpeedText()
    // {
    //     speedText.text = currentRoad.UpdateSpeed();
    // }
    //
    // private void ChangeTimerText()
    // {
    //     timerText.text = currentRoad.UpdateTimer();
    // }

    public void Finish()
    {
        RaceMenu.SetActive(false);
        EndMenu.SetActive(true);
        end_agirlikText.text = Mathf.FloorToInt(PartEffectController.I.GetMass(GameManager.I.lastCarProps)).ToString() + " Kg";
        end_speedText.text = speedText.text;
        end_timeText.text = timerText.text;
            
            
        menu_speedText.text = speedText.text;
        menu_timeText.text = timerText.text;
        menu_frictionText.text = end_agirlikText.text;
    }
    
    // private void CheckFinish() 
    // {
    //     if (currentRoad.GetFinish())
    //     {
    //         time = currentRoad.GetTimer();
    //         RaceMenu.SetActive(false);
    //         EndMenu.SetActive(true);
    //         end_agirlikText.text = Mathf.FloorToInt(PartEffectController.I.GetMass(GameManager.I.lastCarProps)).ToString() + " Kg";
    //         end_speedText.text = speedText.text;
    //         end_timeText.text = timerText.text;
    //         
    //         
    //         menu_speedText.text = speedText.text;
    //         menu_timeText.text = timerText.text;
    //         menu_frictionText.text = end_agirlikText.text;
    //     }
    // }

    #endregion

    #region EndRegion
    private bool waitBgActive = false;
    public GameObject waitBg;
    public Transform waitBgRotatingTransform;
    public float waitBgRotateSpeed = 360f;
    public void SetWaitBG(bool to)
    {
        waitBg.SetActive(to);
        waitBgRotatingTransform.localRotation = Quaternion.identity;
        waitBgActive = to;
    }

    private void Update()
    {
        if (waitBgActive)
        {
            waitBgRotatingTransform.localRotation =
                Quaternion.AngleAxis(waitBgRotateSpeed * Time.deltaTime, Vector3.forward) *
                waitBgRotatingTransform.localRotation;
        }
    }

    private void ARM_LoadMenu() 
    {
        SetWaitBG(true);
        AuthController.I.SendResult(GameManager.I.lastCarProps, currentRoad.groundType,
            currentRoad.GetTimeMoving(), currentRoad.GetProjSpeed(),
            PartEffectController.I.GetMass(GameManager.I.lastCarProps), LoadMenu);
        
    }

    public void LoadMenu(Task<DocumentReference> task)
    {
        if (task.IsCanceled)
        {
            SetWaitBG(false);
            EndMenu.SetActive(true);
            LastMenu.SetActive(false);
            return;
        }

        if (task.IsFaulted)
        {
            SetWaitBG(false);
            EndMenu.SetActive(true);
            LastMenu.SetActive(false);
            return;
        }

        if (task.IsCompleted)
        {
            SetWaitBG(false);
            EndMenu.SetActive(false);
            LastMenu.SetActive(true);
            
            menuTableController.SetMenu(GameManager.I.lastCarProps, currentRoad);
            GameManager.I.bestScoresHolder.AddToDictionary(currentRoad.groundType, new SavedCarProps(GameManager.I.lastCarProps));
            return;
        }
    }
    
    #endregion

    #region MenuRegion
    private void ARM_LoadCustom() 
    {
        GameManager.I.currentScreenType = StartScreenType.EditCarPanel;
        SceneManager.LoadScene(1);
    }

    private void ARM_NewCar() 
    {
        GameManager.I.currentScreenType = StartScreenType.NewCarPanel;
        SceneManager.LoadScene(1);
    }

    private void ARM_ReloadScene()
    {
        SceneManager.LoadScene(2);
    }
    #endregion
}
