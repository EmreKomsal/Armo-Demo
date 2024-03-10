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


    private void Start()
    {
        backButton.onClick.AddListener(Back);
        skipButton.onClick.AddListener(Skip);
    }

    public void Back()
    {
        GameManager.I.currentScreenType = StartScreenType.MainPanel;
        SceneManager.LoadScene(1);
    }

    public void Skip()
    {
        SceneManager.LoadScene(3);
    }
}
