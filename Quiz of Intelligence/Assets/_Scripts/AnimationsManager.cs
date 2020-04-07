using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Facebook.Unity;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
    public CanvasGroup mainMenu;
    public CanvasGroup categoryWindow;
    public GameObject loginWindow;
    public GameObject logoutBtn;

    public static AnimationsManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        loginWindow.SetActive(false);


        // if user's not logged in show him log in screen else go to main menu
        // if (FB.IsLoggedIn)
        // {
        //     Debug.Log("user is logged in");
        //     // go to main menu
        //     mainMenu.interactable = true;
        //     logoutBtn.SetActive(true);
        // }
        // else
        // {
        //     Debug.Log("user is not logged in");
        //     loginWindow.SetActive(true);
        //     logoutBtn.SetActive(false);
        // }
    }


    #region Category Menu Animations

    public void CategoryWindowAnimation_IN(float time)
    {
        // categoryWindow.interactable = true;
        categoryWindow.GetComponent<RectTransform>().DOAnchorPosX(0, time);
        categoryWindow.DOFade(1, time);
    }

    public void CategoryWindowAnimation_Out(float time)
    {
        categoryWindow.GetComponent<RectTransform>().DOAnchorPosX(800, time);
        categoryWindow.DOFade(0, time);
        //   .OnComplete(() =>
        // {
        //   MainMenuAnimation_IN(time);
        // });
    }

    #endregion
}