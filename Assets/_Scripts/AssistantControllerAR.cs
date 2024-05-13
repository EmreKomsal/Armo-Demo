
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AssistantControllerAR : SingletonNew<AssistantControllerAR>
{
    public enum AssistantTabType
    {
        ARPistOverlay,
        ARYarisaBaslaGhostOverlay0,
        ARYarisaBaslaGhostOverlay1,
        ARYarisSonuMoreThan5Seconds,
        ARSonEkranMoreThan5SecondsForce,
        ARSonEkranMoreThan5SecondsNoForce,
        ARSonEkranLessThan5SecondsForce,
        ARSonEkranLessThan5SecondsNoForce,
    }
    
    
    public List<AssistantSingleTab> assistantTabs = new List<AssistantSingleTab>();
    public List<Button> closeButtons = new List<Button>();

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

        if (!AuthController.I.IsAssistant)
        {
            return false;
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

}
