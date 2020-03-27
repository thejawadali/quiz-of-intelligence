using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationsManager : MonoBehaviour
{
  public CanvasGroup mainMenu;
  public CanvasGroup categoryWindow;
  public CanvasGroup loginWindow;

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
    mainMenu.interactable = true;
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

  #region Login screen animations

  public void LoginScreenAnimations_IN(float time)
  {
    loginWindow.GetComponent<RectTransform>().DOAnchorPosX(0, time);
    loginWindow.DOFade(1, time);
    //   .OnComplete(() =>
    // {
    // }); 
  }

  public void LoginScreenAnimations_OUT(float time)
  {
    loginWindow.GetComponent<RectTransform>().DOAnchorPosX(800, time);
    loginWindow.DOFade(0, time);
    //   .OnComplete(() =>
    // {
    // });
  }

  #endregion
  
}