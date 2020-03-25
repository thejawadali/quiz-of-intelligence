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

    MainMenuAnimation_IN(1f);

  }


  void Update()
  {
  }

  #region Main Menu animations

  public void MainMenuAnimation_IN(float time)
  {
    mainMenu.GetComponent<RectTransform>().DOAnchorPosX(0, time);
    mainMenu.DOFade(1, time).OnComplete(() =>
    {
      MainMenuAnimation_OUT(time);
    });
  }

  public void MainMenuAnimation_OUT(float time, Action onComplete = null)
  {
    mainMenu.GetComponent<RectTransform>().DOAnchorPosX(800, time);
    mainMenu.DOFade(0, time).OnComplete(() =>
    {
      CategoryWindowAnimation_IN(time);
    });
  }

  #endregion

  #region Login screen animations

  public void LoginScreenAnimations_IN(float time)
  {
    loginWindow.GetComponent<RectTransform>().DOAnchorPosX(0, time);
    loginWindow.DOFade(1, time).OnComplete(() =>
    {
      MainMenuAnimation_OUT(time);
    }); 
  }

  public void LoginScreenAnimations_OUT(float time)
  {
    loginWindow.GetComponent<RectTransform>().DOAnchorPosX(800, time);
    loginWindow.DOFade(0, time).OnComplete(() =>
    {
      CategoryWindowAnimation_IN(time);
    });
  }

  #endregion
  
  #region Category Menu Animations

  public void CategoryWindowAnimation_IN(float time)
  {

    categoryWindow.GetComponent<RectTransform>().DOAnchorPosX(0, time);
    categoryWindow.DOFade(1, time).OnComplete(() =>
    {
      CategoryWindowAnimation_Out(time);
    });
  }

  public void CategoryWindowAnimation_Out(float time)
  {

    categoryWindow.GetComponent<RectTransform>().DOAnchorPosX(800, time);
    categoryWindow.DOFade(0, time).OnComplete(() =>
    {
      MainMenuAnimation_IN(time);
    });
  }

  #endregion
}