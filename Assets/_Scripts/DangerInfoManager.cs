using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DangerInfoManager : MonoBehaviour
{
    public Button backButton;
    public Button skipButton;

    public GameObject waitBg;
    public Transform waitBgRotatingTransform;
    public float waitBgRotateSpeed = 360f;
    private bool waitBgActive = false;

    public void SetWaitBG(bool to)
    {
        waitBg.SetActive(to);
        waitBgRotatingTransform.localRotation = Quaternion.identity;
        waitBgActive = to;
    }
    
    public void Update()
    {
        if (waitBgActive)
        {
            waitBgRotatingTransform.localRotation =
                Quaternion.AngleAxis(waitBgRotateSpeed * Time.deltaTime, Vector3.forward) *
                waitBgRotatingTransform.localRotation;
        }
    }
    
    private void Start()
    {
        SetWaitBG(false);
        backButton.onClick.AddListener(Back);
        skipButton.onClick.AddListener(Skip);
    }

    public void Back()
    {
        SetWaitBG(true);
        GameManager.I.currentScreenType = StartScreenType.MainPanel;

        UtilityRoutines.I.DelayedCall(0.5f, delegate
        {
            SessionLogger.I.StopRecording();
            SceneManager.LoadScene(1);
        });

    }

    public void Skip()
    {
        SetWaitBG(true);

        UtilityRoutines.I.DelayedCall(0.5f, delegate
        {
            SessionLogger.I.StopRecording();
            SceneManager.LoadScene(3);
        });
    }
}
