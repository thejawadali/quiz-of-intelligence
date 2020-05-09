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

    void Start()
    {
        total_coins_text.text = PlayerPrefs.GetInt("Total_Coins").ToString();
    }


    public void SelectCategory(string cat)
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

            
        // UserUtils.instance.UserStatus(false);
        if (FacebookAuthenticator.isSinglePlayer)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            SceneManager.LoadScene(2);
        }
        // FacebookAuthenticator.instance.UserAvailabilityStatus(false);
    }


    #region GameScene button listeners

    public void PlayGame(bool isSinglePlayer)
    {
        FacebookAuthenticator.isSinglePlayer = isSinglePlayer;
        // make main menu items non-intractable and animate category selection window in 
        AnimationsManager.instance.mainMenu.interactable = false;
        AnimationsManager.instance.CategoryWindowAnimation_IN(0.1f);
    }

    #endregion

    public void BackToMenu()
    {
        // make main menu intractable
        AnimationsManager.instance.mainMenu.interactable = true;
        AnimationsManager.instance.CategoryWindowAnimation_Out(0.1f);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}