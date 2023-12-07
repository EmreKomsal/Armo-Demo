using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIControl : SingletonNew<UIControl>
{
    [Header("Panel Parents")]
    public GameObject welcomePanelParent;
    public GameObject mainPanelParent;
    public GameObject garagePanelParent;
    public GameObject playPanelParent;
    public GameObject newCarPanelParent;
    public GameObject noNetworkPanel;
    public GameObject tutorialPanel;
    public GameObject loginScreen;
    public GameObject registerScreen;
    public GameObject successfulRegisterScreen;
    public GameObject profilePanel;

    public GameObject waitBg;
    public Transform waitBgRotatingTransform;
    public float waitBgRotateSpeed = 360f;
    
    [Header("Welcome Panel")]
    public Button welcomeContinueButton;

    [Header("Main Panel")]
    public Button mainGarageButton;
    public Button mainNewCarButton;
    public Button mainPlayButton;
    public Button mainExitButton;
    public Button mainProfileButton;
    public TMP_Text mainProfileNameText;


    [Header("Garage Panel")]
    public GameObject garageCarExistParent;
    public GameObject garageNoCarExistParent;
    public TMP_Text garageCarNameText;
    public Button garagePrevCarButton;
    public Button garageNextCarButton;
    public Button garageBackButton;
    public Button garageBackNoCarsExistButton;
    public Button garageNewCarNoCarsExistButton;
    public Button garageDeleteButton;
    public Button garageEditButton;
    public Button garagePlayButton;
    public Image garageStatsAccelerationFill;
    public Image garageStatsSpeedFill;
    public Image garageStatsWeightFill;
    public float garageStatsLerpSpeed = 5f;
    private float targetSpeedFill = 0;
    
    [Header("Play Panel")]
    public GameObject playCarExistParent;
    public GameObject playNoCarExistParent;
    public Button playPrevCarButton;
    public Button playNextCarButton;
    public Button playBackButton;
    public Button playPlayButton;
    public Button playNewCarButton;
    public Button playBackButtonNoCarExist;
    public Button playNewCarButtonNoCarExist;


    [Header("New Car Panel")]
    public GameObject newCarNameChangePopUpParent;
    public Button newCarNameChangeButton;
    public Button newCarSaveButton;
    public Button newCarCancelButton;
    public GameObject newCarTogglePartParent;
    public Button newCarTogglePartButton;
    public GameObject newCarToggleCarParent;
    public Button newCarToggleCarButton;
    public Button newCarNameChangePopUpSaveButton;
    public Button newCarNameChangePopUpCancelButton;
    public Button newCarNameChangePopUpSaveButton2;
    public TMP_Text newCarNameText;
    public TMP_InputField newCarNameChangePopUpInputField;



    [Header("Tutorial Panel")]
    public List<GameObject> tanitimPages;
    public List<GameObject> tanitimPageIndicators;
    public GameObject beginTanitimParent;
    public GameObject betweenTanitimParent;
    public GameObject endTanitimParent;
    public Button beginTanitimButton;
    public Button betweenTanitimButtonNext;
    public Button betweenTanitimButtonPrev;
    public Button endTanitimButton;
    private bool didTanitimComplete = false;
    private int tanitimStage = 0;


    [Header("Login Screen")]
    public TMP_InputField loginEmailInputField;
    public TMP_InputField loginPasswordInputField;
    public Button loginButton;
    public Button switchToRegisterButton;

    [Header("Register Screen")]
    public TMP_InputField registerNameInputField;
    public TMP_InputField registerEmailInputField;
    public TMP_InputField registerPasswordInputField;
    public TMP_InputField registerRefInputField;
    public Button switchToLoginButton;
    public Button registerButton;

    [Header("Successful Register Screen")]
    public Button successfulRegisterOkButton;

    [Header("Profile Panel")]
    public Button profileMainMenuButton;
    public Button profileLogoutButton;
    public TMP_Text profileNameText;
    public TMP_Text profileMailText;
    public TMP_Text profileUidText;
    
    public List<PropertiesTableTab> tableTabs;
    private Dictionary<PropertiesTableTabType, PropertiesTableTab> tabTypesToTabs =
        new Dictionary<PropertiesTableTabType, PropertiesTableTab>();


    private int garageCarIndex = 0;
    private int playCarIndex = 0;
    private SavedCarProps editedCar = new SavedCarProps();
    private bool isEditing = false;
    private string newCarNameBuffer = "New Car";
    private bool isToggleCar = true;
    private int lastTabId = 0;
    private bool isGarage = false;
    private float garageSpeed = 0f;
    private bool internetReachable = true;
    private bool waitBgActive = false;


    public void SetWaitBG(bool to)
    {
        waitBg.SetActive(to);
        waitBgRotatingTransform.localRotation = Quaternion.identity;
        waitBgActive = to;
    }
    
    private void Start()
    {
        foreach (var tableTab in tableTabs)
        {
            tabTypesToTabs[tableTab.tabType] = tableTab;
        }
        
        welcomeContinueButton.onClick.AddListener(WelcomeContinue);
        
        mainGarageButton.onClick.AddListener(MainGarage);
        mainNewCarButton.onClick.AddListener(MainNewCar);
        mainPlayButton.onClick.AddListener(MainPlay);
        mainExitButton.onClick.AddListener(QuitApplication);
        
        
        garagePrevCarButton.onClick.AddListener(GaragePreviousCar);
        garageNextCarButton.onClick.AddListener(GarageNextCar);
        garageBackButton.onClick.AddListener(GarageBack);
        garageBackNoCarsExistButton.onClick.AddListener(GarageBack);
        garageNewCarNoCarsExistButton.onClick.AddListener(GarageNewCar);
        garageDeleteButton.onClick.AddListener(GarageDelete);
        garageEditButton.onClick.AddListener(GarageEdit);
        garagePlayButton.onClick.AddListener(GaragePlay);
        
        playPrevCarButton.onClick.AddListener(PlayPreviousCar);
        playNextCarButton.onClick.AddListener(PlayNextCar);
        playBackButton.onClick.AddListener(PlayBack);
        playBackButtonNoCarExist.onClick.AddListener(PlayBack);
        playNewCarButton.onClick.AddListener(PlayNewCar);
        playNewCarButtonNoCarExist.onClick.AddListener(PlayNewCar);
        playPlayButton.onClick.AddListener(PlayPlay);
        
        
        newCarNameChangeButton.onClick.AddListener(NewCarNameChangeOpen);
        newCarNameChangePopUpSaveButton.onClick.AddListener(NewCarNameChangeCloseSave);
        newCarNameChangePopUpSaveButton2.onClick.AddListener(NewCarNameChangeCloseSave);
        newCarNameChangePopUpCancelButton.onClick.AddListener(NewCarNameChangeClose);
        newCarCancelButton.onClick.AddListener(NewCarBack);
        newCarSaveButton.onClick.AddListener(NewCarSave);
        newCarTogglePartButton.onClick.AddListener(NewCarToggleCar);
        newCarToggleCarButton.onClick.AddListener(NewCarTogglePart);
        
        beginTanitimButton.onClick.AddListener(NextTanitimPanel);
        betweenTanitimButtonNext.onClick.AddListener(NextTanitimPanel);
        betweenTanitimButtonPrev.onClick.AddListener(PreviousTanitimPanel);
        endTanitimButton.onClick.AddListener(EndTanitim);

        loginButton.onClick.AddListener(Login);
        
        InitAllTabs();

        CheckInternet();
        if (internetReachable)
        {
            InitScreen();            
        }
        
    }

    public void InitScreen()
    {
        didTanitimComplete = PlayerPrefs.GetInt("didTanitimComplete", 0) == 1;

        if (!didTanitimComplete)
        {
            TutorialStart();
            return;
        }

        if (AuthController.I.auth.CurrentUser != null)
        {
            if (GameManager.I.currentScreenType == StartScreenType.MainPanel)
            {
                MainPanel();
            }
            else if (GameManager.I.currentScreenType == StartScreenType.NewCarPanel)
            {
                NewCarPanel();
            }
            else if (GameManager.I.currentScreenType == StartScreenType.EditCarPanel)
            {
                NewCarPanel(GameManager.I.lastCarProps.saveId);
            }
            else
            {
                WelcomePanel();            
            }
        }
        else
        {
            LoginScreen();
        }
    }

    public void LoginScreen()
    {
        CloseAllParents();
        loginScreen.SetActive(true);
        loginEmailInputField.text = "";
        loginPasswordInputField.text = "";
    }

    public void Login()
    {
        SetWaitBG(true);
        AuthController.I.LoginAttempt(loginEmailInputField.text, loginPasswordInputField.text, HandleLogin);
    }

    public void HandleLogin(Task<Firebase.Auth.AuthResult> task)
    {
        if (task.IsCompleted)
        {
            if (task.Result.User != null && task.Result.User.IsValid())
            {
                MainPanel();
            }
            else
            {
                LoginScreen();
            }
            SetWaitBG(false);
        }
    }
    
    public void RegisterScreen()
    {
        CloseAllParents();
        registerScreen.SetActive(true);
        registerNameInputField.text = "";
        registerEmailInputField.text = "";
        registerPasswordInputField.text = "";
        registerRefInputField.text = "";
    }
    
    public void TutorialStart()
    {
        CloseAllParents();
        tutorialPanel.SetActive(true);
        SetTutorialScreen();
    }

    public void SetTutorialScreen()
    {
        if (tanitimStage == 0)
        {
            beginTanitimParent.SetActive(true);
            betweenTanitimParent.SetActive(false);
            endTanitimParent.SetActive(false);
        }
        else if (tanitimStage == 4)
        {
            beginTanitimParent.SetActive(false);
            betweenTanitimParent.SetActive(false);
            endTanitimParent.SetActive(true);
        }
        else
        {
            beginTanitimParent.SetActive(false);
            betweenTanitimParent.SetActive(true);
            endTanitimParent.SetActive(false);
        }

        foreach (var tanitimPage in tanitimPages)
        {
            tanitimPage.SetActive(false);
        }

        foreach (var tanitimPageIndicator in tanitimPageIndicators)
        {
            tanitimPageIndicator.SetActive(false);
        }
        
        tanitimPages[tanitimStage].SetActive(true);
        tanitimPageIndicators[tanitimStage].SetActive(true);
    }

    public void NextTanitimPanel()
    {
        tanitimStage = Mathf.Clamp(tanitimStage + 1, 0, 4);
        SetTutorialScreen();
    }

    public void PreviousTanitimPanel()
    {
        tanitimStage = Mathf.Clamp(tanitimStage - 1, 0, 4);
        SetTutorialScreen();
    }

    public void EndTanitim()
    {
        didTanitimComplete = true;
        PlayerPrefs.SetInt("didTanitimComplete", 1);
        InitScreen();
    }
    
    public void NoInternetPanel()
    {
        CloseAllParents();
        noNetworkPanel.SetActive(true);
        SetWaitBG(false);
    }

    public void CheckInternet()
    {
        if (internetReachable && Application.internetReachability == NetworkReachability.NotReachable)
        {
            internetReachable = false;
            NoInternetPanel();
        }
        else if (!internetReachable && Application.internetReachability != NetworkReachability.NotReachable)
        {
            internetReachable = true;
            InitScreen();
        }
    }
    
    public void Update()
    {
        CheckInternet();
        if (waitBgActive)
        {
            waitBgRotatingTransform.localRotation =
                Quaternion.AngleAxis(waitBgRotateSpeed * Time.deltaTime, Vector3.forward) *
                waitBgRotatingTransform.localRotation;
        }
        
        if (isGarage)
        {
            garageStatsSpeedFill.fillAmount = Mathf.Lerp(garageStatsSpeedFill.fillAmount, targetSpeedFill,
                garageStatsLerpSpeed * Time.deltaTime);
        }
    }


    public void WelcomePanel()
    {
        isGarage = false;
        CloseAllParents();
        welcomePanelParent.SetActive(true);
        PreviewController.I.DeactivatePreview();
    }

    public void WelcomeContinue()
    {
        MainPanel();
    }

    public void MainGarage()
    {
        GaragePanel();
    }
    
    public void MainNewCar()
    {
        NewCarPanel();
    }
    
    public void MainPlay()
    {
        PlayPanel();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }


    public void GarageBack()
    {
        MainPanel();
    }
    
    public void GarageNewCar()
    {
        NewCarPanel();
    }
    
    public void GarageDelete()
    {
        SaveCarController.I.DeleteCar(garageCarIndex);
        if (SaveCarController.I.GetCarCount() == 0)
        {
            garageNoCarExistParent.SetActive(true);
            garageCarExistParent.SetActive(false);
        }
        else
        {
            if (garageCarIndex >= SaveCarController.I.GetCarCount())
            {
                garageCarIndex = SaveCarController.I.GetCarCount() - 1;
            }
            garageNoCarExistParent.SetActive(false);
            garageCarExistParent.SetActive(true);
            SetGaragePreviewCar(garageCarIndex);
        }
    }

    public void GarageEdit()
    {
        NewCarPanel(garageCarIndex);
    }
    
    
    public void GaragePlay()
    {
        GameManager.I.SetCarProps(SaveCarController.I.GetCarProps(garageCarIndex));
        SceneManager.LoadScene(2);
    }

    public void PlayBack()
    {
        MainPanel();
    }
    
    public void PlayNewCar()
    {
        NewCarPanel();
    }

    public void PlayPlay()
    {
        GameManager.I.SetCarProps(SaveCarController.I.GetCarProps(playCarIndex));
        SceneManager.LoadScene(2);
    }


    public void NewCarBack()
    {
        MainPanel();
    }

    public void NewCarSave()
    {
        if (isEditing)
        {
            SaveCarController.I.EditCar(editedCar.saveId, editedCar);
        }
        else
        {
            SaveCarController.I.SaveCar(editedCar);
        }
        MainPanel();
    }
    
    public void NewCarNameChangeOpen()
    {
        newCarNameChangePopUpParent.SetActive(true);
        newCarNameChangePopUpInputField.text = "";
    }
    
    public void NewCarNameChangeClose()
    {
        newCarNameChangePopUpParent.SetActive(false);
        newCarNameText.text = newCarNameBuffer;
    }
    
    public void NewCarNameChangeCloseSave()
    {
        newCarNameBuffer = newCarNameChangePopUpInputField.text;
        newCarNameChangePopUpParent.SetActive(false);
        newCarNameText.text = newCarNameBuffer;
        editedCar.name = newCarNameBuffer;
    }
    
    
    
    
    public void MainPanel()
    {
        isGarage = false;
        CloseAllParents();
        mainPanelParent.SetActive(true);
        PreviewController.I.DeactivatePreview();
    }
    
    public void GaragePanel()
    {
        CloseAllParents();
        PreviewController.I.ActivatePreview();
        garagePanelParent.SetActive(true);
        garageCarIndex = 0;
        if (SaveCarController.I.GetCarCount() == 0)
        {
            garageNoCarExistParent.SetActive(true);
            garageCarExistParent.SetActive(false);
        }
        else
        {
            isGarage = true;
            garageNoCarExistParent.SetActive(false);
            garageCarExistParent.SetActive(true);
            SetGaragePreviewCar(garageCarIndex);
            garageSpeed = PartEffectController.I.GetSpeed(SaveCarController.I.GetCarProps(garageCarIndex));
        
            garageStatsSpeedFill.fillAmount = (garageSpeed - PartEffectController.I.GetMinMaxSpeed().x) /
                                              (PartEffectController.I.GetMinMaxSpeed().y -
                                               PartEffectController.I.GetMinMaxSpeed().x);
            targetSpeedFill = garageStatsSpeedFill.fillAmount;
        }

        
    }

    public void SetGaragePreviewCar(int wantedIndex)
    {
        var carProps = SaveCarController.I.GetCarProps(wantedIndex);
        garageCarNameText.text = carProps.name;
        PreviewController.I.SetPreviewOnlyCar(carProps);
    }

    public void GarageNextCar()
    {
        if (SaveCarController.I.GetCarCount() == 0)
        {
            return;
        }
        garageCarIndex = (garageCarIndex + 1) % SaveCarController.I.GetCarCount();
        SetGaragePreviewCar(garageCarIndex);
        garageSpeed = PartEffectController.I.GetSpeed(SaveCarController.I.GetCarProps(garageCarIndex));
        targetSpeedFill = (garageSpeed - PartEffectController.I.GetMinMaxSpeed().x) /
                          (PartEffectController.I.GetMinMaxSpeed().y -
                           PartEffectController.I.GetMinMaxSpeed().x);
    }

    public void GaragePreviousCar()
    {
        if (SaveCarController.I.GetCarCount() == 0)
        {
            return;
        }

        garageCarIndex = (garageCarIndex == 0 ? (SaveCarController.I.GetCarCount() - 1) : (garageCarIndex - 1));
        SetGaragePreviewCar(garageCarIndex);
        garageSpeed = PartEffectController.I.GetSpeed(SaveCarController.I.GetCarProps(garageCarIndex));
        targetSpeedFill = (garageSpeed - PartEffectController.I.GetMinMaxSpeed().x) /
                          (PartEffectController.I.GetMinMaxSpeed().y -
                           PartEffectController.I.GetMinMaxSpeed().x);
    }
    
    public void PlayPanel()
    {
        isGarage = false;
        CloseAllParents();
        playPanelParent.SetActive(true);
        PreviewController.I.ActivatePreview();
        playCarIndex = 0;
        if (SaveCarController.I.GetCarCount() == 0)
        {
            playNoCarExistParent.SetActive(true);
            playCarExistParent.SetActive(false);
        }
        else
        {
            playNoCarExistParent.SetActive(false);
            playCarExistParent.SetActive(true);
            SetPlayPreviewCar(playCarIndex);
        }
    }
    
    public void SetPlayPreviewCar(int wantedIndex)
    {
        PreviewController.I.SetPreviewOnlyCar(SaveCarController.I.GetCarProps(wantedIndex));
    }
    
    public void PlayNextCar()
    {
        if (SaveCarController.I.GetCarCount() == 0)
        {
            return;
        }
        playCarIndex = (playCarIndex + 1) % SaveCarController.I.GetCarCount();
        SetPlayPreviewCar(playCarIndex);
    }

    public void PlayPreviousCar()
    {
        if (SaveCarController.I.GetCarCount() == 0)
        {
            return;
        }

        playCarIndex = (playCarIndex == 0 ? (SaveCarController.I.GetCarCount() - 1) : (playCarIndex - 1));
        SetPlayPreviewCar(playCarIndex);
    }
    
    public void NewCarPanel(int editIndex = -1)
    {
        isGarage = false;
        CloseAllParents();
        PreviewController.I.ActivatePreview();
        newCarPanelParent.SetActive(true);
        newCarNameBuffer = "Yeni Araç";
        newCarNameText.text = newCarNameBuffer;
        isToggleCar = true;
        if (editIndex < 0 || editIndex >= SaveCarController.I.GetCarCount())
        {
            editedCar = new SavedCarProps();
            isEditing = false;
        }
        else
        {
            editedCar = SaveCarController.I.GetCarProps(editIndex);
            isEditing = true;
        }
        InitAllTabs(editedCar);
        CloseAllTabs();
        tableTabs[0].Select();
        NewCarToggleCar();
    }

    public void SetLastTab(int id)
    {
        lastTabId = id;
    }
    
    public void NewCarToggleCar()
    {
        isToggleCar = true;
        newCarToggleCarParent.SetActive(true);
        newCarTogglePartParent.SetActive(false);
        SetNewCarPreviewCar();
    }

    public void NewCarTogglePart()
    {
        isToggleCar = false;
        newCarToggleCarParent.SetActive(false);
        newCarTogglePartParent.SetActive(true);
        SetNewCarPreviewCar();
    }
    
    public void SetNewCarPreviewCar()
    {
        PreviewController.I.SetPreview(isToggleCar, editedCar, lastTabId);
    }
    
    public void CloseAllParents()
    {
        welcomePanelParent.SetActive(false);
        mainPanelParent.SetActive(false);
        garagePanelParent.SetActive(false);
        playPanelParent.SetActive(false);
        newCarPanelParent.SetActive(false);
        noNetworkPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        loginScreen.SetActive(false);
        registerScreen.SetActive(false);
        successfulRegisterScreen.SetActive(false);
        profilePanel.SetActive(false);
    }
    
    public PropertiesTableTab GetTab(PropertiesTableTabType wantedType)
    {
        if (!tabTypesToTabs.ContainsKey(wantedType))
        {
            return null;
        }
        return tabTypesToTabs[wantedType];
    }

    public void InitAllTabs()
    {
        foreach (var tableTab in tableTabs)
        {
            tableTab.Init();
        }
    }

    public void SetTabValue(PropertiesTableTabType wantedType, int newValue)
    {
        switch (wantedType)
        {
            case PropertiesTableTabType.Kaporta:
                editedCar.kaportaId = newValue;
                break;
            case PropertiesTableTabType.Lastik:
                editedCar.lastikId = newValue;
                break;
            case PropertiesTableTabType.Motor:
                editedCar.motorId = newValue;
                break;
            case PropertiesTableTabType.Koltuk:
                editedCar.koltukId = newValue;
                break;
            case PropertiesTableTabType.Ruzgarlik:
                editedCar.ruzgarlikId = newValue;
                break;
            default:
                break;
        }
        SetNewCarPreviewCar();
    }
    
    public void InitAllTabs(SavedCarProps savedProps)
    {
        foreach (var tableTab in tableTabs)
        {
            switch (tableTab.tabType)
            {
                case PropertiesTableTabType.Kaporta:
                    tableTab.Init(savedProps.kaportaId);
                    break;
                case PropertiesTableTabType.Lastik:
                    tableTab.Init(savedProps.lastikId);
                    break;
                case PropertiesTableTabType.Motor:
                    tableTab.Init(savedProps.motorId);
                    break;
                case PropertiesTableTabType.Koltuk:
                    tableTab.Init(savedProps.koltukId);
                    break;
                case PropertiesTableTabType.Ruzgarlik:
                    tableTab.Init(savedProps.ruzgarlikId);
                    break;
                default:
                    tableTab.Init();
                    break;
            }
        }
    }
    
    public void CloseAllTabs()
    {
        foreach (var tableTab in tableTabs)
        {
            tableTab.Deselect();
        }
    }
}
