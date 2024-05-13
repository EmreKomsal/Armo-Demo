using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AssistantController : SingletonNew<AssistantController>
{
    public enum AssistantTabType
    {
        DuringRegister,
        MenuOverlay0,
        MenuOverlay1,
        MenuOverlay2,
        MenuAfterCreateCar,
        NewCarOverlay0,
        NewCarOverlay1,
        NewCarOverlay2,
        NewCarOverlay3,
        NewCarOverlay4,
        NewCarOverlay5,
        NewCarOverlay6,
        NewCarOverlay7,
    }
    
    
    public List<AssistantSingleTab> assistantTabs = new List<AssistantSingleTab>();
    public List<Button> closeButtons = new List<Button>();

    public bool SaveConfirmationBypass { get; set; } = false;


    private bool canPress = true;
    public void SomethingPressed()
    {
        canPress = false;
        foreach (var closeButton in closeButtons)
        {
            closeButton.interactable = false;
        }


        UtilityRoutines.I.DelayedCall(0.02f, delegate
        {
            canPress = true;
            foreach (var closeButton in closeButtons)
            {
                closeButton.interactable = true;
            }
        });
    }
    
    private void Start()
    {
        foreach (var tab in assistantTabs)
        {
            tab.obj.SetActive(false);
        }
        foreach (var button in closeButtons)
        {
            button.onClick.AddListener(CloseAssistantTabs);
        }
    }

    public void CloseAssistantTabs()
    {
        if (!canPress)
        {
            return;
        }
        SomethingPressed();
        foreach (var tab in assistantTabs)
        {
            tab.obj.SetActive(false);
        }

        var buff = onClose;
        buff?.Invoke();
        // onClose = null;
    }

    private UnityAction onClose;
    
    public bool OpenAssistantTab(AssistantTabType tabType, UnityAction newOnClose = null)
    {
        foreach (var tab in assistantTabs)
        {
            tab.obj.SetActive(false);
        }
        var i = (int)tabType;
        onClose = newOnClose;
        if (i < 0 || i >= assistantTabs.Count)
        {
            return false;
        }

        if (tabType == AssistantTabType.MenuOverlay0 || tabType == AssistantTabType.MenuOverlay1 || tabType == AssistantTabType.MenuOverlay2)
        {
            if (PlayerPrefs.GetInt("a_successfulRegisterActivated", 0) == 1)
            {
                onClose = null;
                return false;
            }
        }
        else if (tabType == AssistantTabType.MenuAfterCreateCar)
        {
            if (PlayerPrefs.GetInt("a_successfulRegisterCreatedCar", 0) == 1)
            {
                onClose = null;

                return false;
            }
        }
        else if (tabType == AssistantTabType.NewCarOverlay0 || tabType == AssistantTabType.NewCarOverlay1 ||
                 tabType == AssistantTabType.NewCarOverlay2 || tabType == AssistantTabType.NewCarOverlay3 ||
                 tabType == AssistantTabType.NewCarOverlay4 || tabType == AssistantTabType.NewCarOverlay5 ||
                 tabType == AssistantTabType.NewCarOverlay6)
        {
            if (!AuthController.I.IsAssistant)
            {
                onClose = null;
                return false;
            }
            // if (PlayerPrefs.GetInt("a_carPartToggle", 0) == 1)
            // {
            //     return false;
            // }
            // PlayerPrefs.SetInt("a_carPartToggle", 1);
        }
        else if (tabType == AssistantTabType.NewCarOverlay7)
        {
            if (!AuthController.I.IsAssistant || TabChangeComplete)
            {
                return false;
            }
        }
        // else if (tabType == AssistantTabType.CarName || tabType == AssistantTabType.AfterCreatingCar ||
        //          tabType == AssistantTabType.CarSaveConfirmation)
        // {
        //     if (!AuthController.I.IsAssistant)
        //     {
        //         return false;
        //     }
        // }

        if (assistantTabs[i].followingT != null && assistantTabs[i].followedT != null)
        {
            assistantTabs[i].followingT.position = assistantTabs[i].followedT.position;
        }

        if (assistantTabs[i].obj != null)
        {
            assistantTabs[i].obj.SetActive(true);
        }
        return true;
    }

    public bool TabChangeComplete { get; set; } = false;


    public void MenuOverlay0End()
    {
        OpenAssistantTab(AssistantTabType.MenuOverlay1, MenuOverlay1End);
    }

    public void MenuOverlay1End()
    {
        OpenAssistantTab(AssistantTabType.MenuOverlay2, MenuOverlay2End);
    }

    public void MenuOverlay2End()
    {
        PlayerPrefs.SetInt("a_successfulRegisterActivated", 1);
        UIControl.I.NewCarPanel();
    }
    
    public void MenuAfterCreateCarEnd()
    {
        UIControl.I.PlayPanel();
        PlayerPrefs.SetInt("a_successfulRegisterCreatedCar", 1);
    }

    public void NewCarOverlay0End()
    {
        UIControl.I.onNameChangeClose = delegate
        {
            OpenAssistantTab(AssistantTabType.NewCarOverlay1, NewCarOverlay1End);
        };
        UIControl.I.NewCarNameChangeOpen();
        
    }
    
    public void NewCarOverlay1End()
    {
        if (PlayerPrefs.GetInt("a_carPartToggle", 0) == 1)
        {
            OpenAssistantTab(AssistantTabType.NewCarOverlay4, NewCarOverlay4End);
        }
        else
        {
            OpenAssistantTab(AssistantTabType.NewCarOverlay2, NewCarOverlay2End);
        }
    }
    
    public void NewCarOverlay2End()
    {
        OpenAssistantTab(AssistantTabType.NewCarOverlay3, NewCarOverlay3End);

    }
    
    public void NewCarOverlay3End()
    {
        PlayerPrefs.SetInt("a_carPartToggle", 1);
        OpenAssistantTab(AssistantTabType.NewCarOverlay4, NewCarOverlay4End);
    }
    
    public void NewCarOverlay4End()
    {
        OpenAssistantTab(AssistantTabType.NewCarOverlay5, NewCarOverlay5End);
    }
    
    public void NewCarOverlay5End()
    {
        OpenAssistantTab(AssistantTabType.NewCarOverlay6, NewCarOverlay6End);
    }
    
    public void NewCarOverlay6End()
    {
        
    }

    public void NewCarOverlay7End()
    {
        TabChangeComplete = true;
    }
    
    
}


[Serializable]
public class AssistantSingleTab
{
    public GameObject obj;
    public RectTransform followingT;
    public RectTransform followedT;
}