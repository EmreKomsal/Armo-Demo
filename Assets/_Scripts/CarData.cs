using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CarData : MonoBehaviour
{

    [ReadOnly] public bool isGhost = false;
    public GameObject crownObject;

    public GameObject activeSpoiler;
    public GameObject activeWheel;
    public List<GameObject> activeStil;

    public int activeWheelID;
    public int activeSpoilerID;
    public int activeRenkID;
    public int activeStilID;

    public List<GameObject> spoilerList;
    public List<GameObject> wheelList;
    public List<GOList> stilList;
    public List<Renderer> colorChangingRends;

    public float speedMlp = 1f;

    private MaterialPropertyBlock propBlock;

    public void SetNextWheel()
    {
        // If there are no wheels in the list, return
        if (wheelList.Count == 0) return;

        // Increment the activeWheelID to switch to the next wheel
        activeWheelID++;

        // If the activeWheelID goes beyond the last index, reset it to the first wheel in the list
        if (activeWheelID >= wheelList.Count)
        {
            activeWheelID = 0;
        }

        // Spawn the selected wheel
        SpawnWheel();
    }

    // Function to set the previous wheel in the list
    public void SetPrevWheel()
    {
        // If there are no wheels in the list, return
        if (wheelList.Count == 0) return;

        // Decrement the activeWheelID to switch to the previous wheel
        activeWheelID--;

        // If the activeWheelID goes below 0, set it to the last wheel in the list
        if (activeWheelID < 0)
        {
            activeWheelID = wheelList.Count - 1;
        }

        // Spawn the selected wheel
        SpawnWheel();
    }

    // Function to activate the currently selected wheel and deactivate others
    public void SpawnWheel()
    {
        // Deactivate the current active wheel
        activeWheel.SetActive(false);

        // Set the active wheel to the selected one from the list
        activeWheel = wheelList[activeWheelID];

        // Activate the selected wheel
        activeWheel.SetActive(true);
    }
 
    // Function to set the next spoiler in the list
    public void SetNextSpoiler()
    {
        // If there are no spoilers in the list, return
        if (spoilerList.Count == 0) return;

        // Increment the activeSpoilerID to switch to the next spoiler
        activeSpoilerID++;

        // If the activeSpoilerID goes beyond the last index, reset it to the first spoiler in the list
        if (activeSpoilerID >= spoilerList.Count)
        {
            activeSpoilerID = 0;
        }

        // Spawn the selected spoiler
        SpawnSpoiler();
    }

    // Function to set the previous spoiler in the list
    public void SetPrevSpoiler()
    {
        // If there are no spoilers in the list, return
        if (spoilerList.Count == 0) return;

        // Decrement the activeSpoilerID to switch to the previous spoiler
        activeSpoilerID--;

        // If the activeSpoilerID goes below 0, set it to the last spoiler in the list
        if (activeSpoilerID < 0)
        {
            activeSpoilerID = spoilerList.Count - 1;
        }

        // Spawn the selected spoiler
        SpawnSpoiler();
    }

    // Function to activate the currently selected spoiler and deactivate others
    public void SpawnSpoiler()
    {
        // Deactivate the current active spoiler
        activeSpoiler.SetActive(false);

        // Set the active spoiler to the selected one from the list
        activeSpoiler = spoilerList[activeSpoilerID];

        // Activate the selected spoiler
        activeSpoiler.SetActive(true);
    }


    // Function to set wheel and spoiler based on given IDs
    public void SetPartsByID(int wheelID, int spoilerID, int renkId, int stilId, bool setGhost)
    {
        isGhost = setGhost;
        crownObject.SetActive(isGhost);
        // Set wheel by ID
        if (wheelList.Count > 0 && wheelID >= 0 && wheelID < wheelList.Count)
        {
            activeWheel.SetActive(false);
            activeWheel = wheelList[wheelID];
            activeWheel.SetActive(true);
            activeWheelID = wheelID; // Update the activeWheelID
        }

        // Set spoiler by ID
        if (spoilerList.Count > 0 && spoilerID >= 0 && spoilerID < spoilerList.Count)
        {
            activeSpoiler.SetActive(false);
            activeSpoiler = spoilerList[spoilerID];
            activeSpoiler.SetActive(true);
            activeSpoilerID = spoilerID; // Update the activeSpoilerID
        }
        
        if (stilList.Count > 0 && stilId >= 0 && stilId < stilList.Count)
        {
            foreach (var o in activeStil)
            {
                o.SetActive(false);
            }
            activeStil = stilList[stilId].gameObjects;
            foreach (var o in activeStil)
            {
                o.SetActive(true);
            }
            activeStilID = stilId; // Update the activeSpoilerID
        }
        
        if (propBlock == null)
        {
            propBlock = new MaterialPropertyBlock();
        }

        var propName = GameManager.I.colorPropertyName;
        var matIndex = GameManager.I.colorMaterialIndex;
        var c = GameManager.I.colors[renkId];
        if (isGhost)
        {
            c.a = GameManager.I.ghostColorAlpha;
            var otherRends = GetComponentsInChildren<Renderer>(true);
            foreach (var rend in otherRends)
            {
                rend.GetPropertyBlock(propBlock, matIndex);
                var otherColor = propBlock.GetColor(propName);
                otherColor.a = GameManager.I.ghostColorAlpha;
                propBlock.SetColor(propName, otherColor);
                rend.SetPropertyBlock(propBlock, matIndex);
            }
        }
        else
        {
            foreach (var rend in colorChangingRends)
            {
                rend.GetPropertyBlock(propBlock, matIndex);
                propBlock.SetColor(propName, c);
                rend.SetPropertyBlock(propBlock, matIndex);
            }
        }
    }
}