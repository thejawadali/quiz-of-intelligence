using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public TextMeshProUGUI total_coins_text;
    public TextMeshProUGUI total_points_text;

    #region MainMenu Stuff

    [Header("Main menu stuff")] public Button startQuiz_Button;
    public Button mulitPlayer_Button;

    #endregion

    #region Category Stuff

    [Header("Category window stuff")]

    #endregion

    public static Category category;

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
        total_coins_text.text = PlayerPrefs.GetInt("Total_Coins").ToString();
        total_points_text.text = PlayerPrefs.GetInt("Total_Points") + " pts";
        MainMenuButtonsListeners();
    }

    #region MainMenu Methods

    private void MainMenuButtonsListeners()
    {
        // start button clicked, show category selection window
        startQuiz_Button.onClick.AddListener(() =>
        {
            // make main menu items non-interactable and animate category selection window in 
            AnimationsManager.instance.mainMenu.interactable = false;
            AnimationsManager.instance.CategoryWindowAnimation_IN(0.1f);
        });

        // multi-player button clicked, if user logged in show create room screen else login screen
        mulitPlayer_Button.onClick.AddListener(() =>
        {
            // TODO: check if user's logged in or not
            AnimationsManager.instance.LoginScreenAnimations_IN(0.1f);
        });
    }

    #endregion

    #region Category Selection methods

    public void CategoryButtons(string cat)
    {
        switch (cat)
        {
            case "GENERAL_KNOWLEDGE":
                category = Category.GENERAL_KNOWLEDGE;
                break;
            case "GEOGRAPHY":
                category = Category.GEOGRAPHY;
                break;
            case "HISTORY":
                category = Category.HISTORY;
                break;
            case "SCIENCE":
                category = Category.SCIENCE;
                break;
            case "COMPUTER":
                category = Category.COMPUTER;
                break;
            case "MATHEMATICS":
                category = Category.MATHEMATICS;
                break;
            case "FILM":
                category = Category.FILM;
                break;
            case "MUSIC":
                category = Category.MUSIC;
                break;
            case "SPORTS":
                category = Category.SPORTS;
                break;
            default:
                category = Category.ALL;
                break;
        }

        SceneManager.LoadScene(1);
    }

    #endregion


    public void BackToMenu(bool isLoginScreen)
    {
        // make main menu interactable
        AnimationsManager.instance.mainMenu.interactable = true;

        // if login screen animate login screen out else category screen
        if (isLoginScreen)
        {
            AnimationsManager.instance.LoginScreenAnimations_OUT(0.1f);
        }
        else
        {
            AnimationsManager.instance.CategoryWindowAnimation_Out(0.1f);
        }
    }
}