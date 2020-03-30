using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneAnimations : MonoBehaviour
{
    public GameObject quizScreen;
    public GameObject resultScreen;

    #region Quiz screen items

    [Header("Quiz screen items")] [SerializeField]
    private RectTransform question_bg_image;

    [SerializeField] private RectTransform[] answers;
    [SerializeField] private RectTransform avatar_container;
    [SerializeField] private RectTransform time_slider;
    [SerializeField] public RectTransform hintBtn;

    #endregion

    [Space(12)]

    #region Result screen items

    [Header("Result screen items")]
    [SerializeField]
    private RectTransform heading;

    [SerializeField] private RectTransform statsWindow;

    [SerializeField] private RectTransform playAgainButton;

    [SerializeField] private RectTransform homeButton;

    #endregion

    [Space(12)] [Header("Coins counter")] [SerializeField]
    private RectTransform coins_container;


    public static bool gameStarted;

    public static GameSceneAnimations instance = null;

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
        resultScreen.SetActive(false);
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
        foreach (var ops in answers)
        {
            var image = ops.transform.GetChild(1);
            image.gameObject.SetActive(false);
            image.GetComponent<Image>().color = Color.white;

            ops.GetComponent<CanvasGroup>().DOFade(0, time);
        }

        question_bg_image.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(0, time)
            .OnComplete(() => { QuestionnaireManager.instance.GetNewQuestion(); });
    }

    #endregion


    #region Whole Quiz Screen components animtions

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
    public void AllComponentsAnimations_OUT(float time, Action onComplete = null)
    {
        question_bg_image.transform.GetChild(0).GetComponent<CanvasGroup>().DOFade(0, time);
        foreach (var ops in answers)
        {
            ops.GetComponent<CanvasGroup>().DOFade(0, time);
        }

        question_bg_image.DOSizeDelta(Vector2.zero, time).OnComplete(() =>
        {
            avatar_container.DOAnchorPosX(-180, time);
            time_slider.DOScale(Vector3.zero, time);
            coins_container.DOAnchorPosY(-50, time);
            hintBtn.DOAnchorPosX(-540, time)
                .OnComplete(() => { onComplete(); });
        });
    }

    #endregion

    #region GameOver Screen

    public void ResultScreenAnimations_IN(float time, Action onComplete = null)
    {
        resultScreen.SetActive(true);
        quizScreen.SetActive(false);

        coins_container.DOAnchorPosY(15, time);
        heading.DOAnchorPosY(-48f, time);
        heading.GetComponent<TextMeshProUGUI>().DOFade(1, time);
        statsWindow.DOScale(Vector3.one, time);
        statsWindow.GetComponent<CanvasGroup>().DOFade(1, time);
        playAgainButton.DOAnchorPosX(0, time);
        homeButton.DOAnchorPosX(0, time).OnComplete(() => { onComplete(); });
    }

    public void ResultScreenAnimations_OUT(float time, Action onComplete)
    {
        heading.DOAnchorPosY(250f, time);
        coins_container.DOAnchorPosY(-50, time);
        heading.GetComponent<TextMeshProUGUI>().DOFade(0, time);
        statsWindow.DOScale(Vector3.zero, time);
        statsWindow.GetComponent<CanvasGroup>().DOFade(0, time);
        playAgainButton.DOAnchorPosX(-700, time);
        homeButton.DOAnchorPosX(700, time).OnComplete(() => { onComplete(); });
    }

    #endregion
}