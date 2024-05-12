using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AssistantController : SingletonNew<AssistantController>
{
    public enum AssistantTabType
    {
        AfterRegister,
        AfterCreatingCar,
        CarName,
        CarSaveConfirmation,
        CarPartToggle,
    }
    
    
    public List<AssistantSingleTab> assistantTabs = new List<AssistantSingleTab>();
    public List<Button> closeButtons = new List<Button>();

    public bool SaveConfirmationBypass { get; set; } = false;
    
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
        foreach (var tab in assistantTabs)
        {
            tab.obj.SetActive(false);
        }
        onClose?.Invoke();
        onClose = null;
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

        if (tabType == AssistantTabType.AfterRegister)
        {
            if (PlayerPrefs.GetInt("a_successfulRegisterActivated", 0) == 1)
            {
                return false;
            }
            PlayerPrefs.SetInt("a_successfulRegisterActivated", 1);
        }
        else if (tabType == AssistantTabType.CarPartToggle)
        {
            if (PlayerPrefs.GetInt("a_carPartToggle", 0) == 1)
            {
                return false;
            }
            PlayerPrefs.SetInt("a_carPartToggle", 1);
        }
        else if (tabType == AssistantTabType.CarName || tabType == AssistantTabType.AfterCreatingCar ||
                 tabType == AssistantTabType.CarSaveConfirmation)
        {
            if (!AuthController.I.IsAssistant)
            {
                return false;
            }
        }

        assistantTabs[i].followingT.position = assistantTabs[i].followedT.position;
        assistantTabs[i].obj.SetActive(true);
        return true;
    }
}


[Serializable]
public class AssistantSingleTab
{
    public GameObject obj;
    public RectTransform followingT;
    public RectTransform followedT;
}