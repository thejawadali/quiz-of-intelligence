using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionsAnimations : MonoBehaviour
{
    public RectTransform question_bg_image;
    public RectTransform[] answers;
    public RectTransform avatar_container;
    public RectTransform time_slider;
    public RectTransform hintBtn;
    public RectTransform coins_container;
    public static bool gameStarted;
    public static QuestionsAnimations instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        gameStarted = false;
        // first get all components to their initial position
        AllComponentsAnimations_OUT(0);
        AllComponentsAnimations_IN(0.2f);
    }

    #region In game question text and options animations

    /// <summary>
    /// call it every time you get new question
    /// </summary>
    /// <param name="time"></param>
    public void AnimateQuestions_IN(float time)
    {
        Sequence mySeq = DOTween.Sequence();
        mySeq.Append(question_bg_image.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(1, time))
            .Append(answers[0].GetComponent<CanvasGroup>().DOFade(1, 0.1f))
            .Append(answers[1].GetComponent<CanvasGroup>().DOFade(1, 0.1f))
            .Append(answers[2].GetComponent<CanvasGroup>().DOFade(1, 0.1f))
            .Append(answers[3].GetComponent<CanvasGroup>().DOFade(1, 0.1f))
            .OnComplete(() => { gameStarted = true; });
    }

    /// <summary>
    /// call it after every question answered
    /// </summary>
    /// <param name="time"></param>
    public void AnimateQuestions_OUT(float time)
    {
        question_bg_image.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(0, time);
        foreach (var ops in answers)
        {
            var image = ops.transform.GetChild(1);
            image.gameObject.SetActive(false);
            image.GetComponent<Image>().color = Color.white;

            ops.GetComponent<CanvasGroup>().DOFade(0, time).OnComplete(() =>
            {
                QuestionGenerator.instance.GetNewQuestion();
            });
        }
    }

    #endregion


    #region Whole Scene components animtions

    /// <summary>
    /// call it when quiz starts
    /// </summary>
    /// <param name="time"></param>
    void AllComponentsAnimations_IN(float time)
    {
        avatar_container.DOAnchorPosX(33, time);
        time_slider.DOScale(Vector3.one, time);
        hintBtn.DOAnchorPosX(-199.36f, time);
        coins_container.DOAnchorPosY(15, time);
        question_bg_image.DOSizeDelta(new Vector2(0, 336), time).OnComplete(() =>
        {
            // animate questions in
            AnimateQuestions_IN(0.2f);
        });
    }

    /// <summary>
    /// call it in start to get components in initial state and in the end of quiz 
    /// </summary>
    /// <param name="time"></param>
    void AllComponentsAnimations_OUT(float time)
    {
        avatar_container.DOAnchorPosX(-180, time);
        time_slider.DOScale(Vector3.zero, time);
        hintBtn.DOAnchorPosX(-540, time);
        coins_container.DOAnchorPosY(-50, time);
        question_bg_image.DOSizeDelta(Vector2.zero, time);
        question_bg_image.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(0, time);
        foreach (var ops in answers)
        {
            ops.GetComponent<CanvasGroup>().DOFade(0, time);
        }
    }

    #endregion
}