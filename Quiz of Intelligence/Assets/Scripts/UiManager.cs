using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    #region MainMenu Stuff

    [Header("Main menu stuff")]
    public Button startQuiz_Button;
    public Button mulitPlayer_Button;

    #endregion

    #region Category Stuff

    [Header("Category window stuff")] public GameObject categoryPanel;

    #endregion

    public static UiManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        MainMenuButtonsListeners();
        // AnimationsManager.instance
        categoryPanel.SetActive(false);
    }

    #region MainMenu Methods

    private void MainMenuButtonsListeners()
    {
        startQuiz_Button.onClick.AddListener(() =>
        {
            // go to category screen 
            // animate main menu stuff out
            AnimationsManager.instance.MainMenuAnimation_OUT(0.1f, () =>
            {
                // animate in category screen
                AnimationsManager.instance.CategoryWindowAnimation_IN(0.1f);
            });
            categoryPanel.SetActive(true);
            Debug.Log("Start single player game");
        });

        mulitPlayer_Button.onClick.AddListener(() => { Debug.Log("Start multiplayer game"); });
    }

    #endregion
}