using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen : MonoBehaviour
{
    public static int wrongAnswers;
    public static int correctAnswers;
    public static int cur_coins_count;


    private int totalPoints;
    public static int totalCoins;

    #region Result screen data

    [Header("Result screen data")] [SerializeField]
    private TextMeshProUGUI cur_points_text;

    [SerializeField] public TextMeshProUGUI outlined_coinText_forAnimation;
    [SerializeField] private TextMeshProUGUI cur_coins_count_text;
    [SerializeField] private TextMeshProUGUI wrongAnswersText;
    [SerializeField] private TextMeshProUGUI correctAnswersText;
    [SerializeField] public TextMeshProUGUI total_coins_text;

    #endregion


    public static ResultScreen instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        totalCoins = PlayerPrefs.GetInt("Total_Coins");
        totalPoints = PlayerPrefs.GetInt("Total_Points");
        total_coins_text.text = totalCoins.ToString();
        wrongAnswers = 0;
        correctAnswers = 0;
        cur_coins_count = 0;
    }

    public void GameOver()
    {
        if (correctAnswers >= 3)
        {
            if (QuestionnaireManager.difficulty <= 3)
            {
                PlayerPrefs.SetInt("DIFFICULTY", 2); //QuestionnaireManager.difficulty++);
                PlayerPrefs.Save();
            }
        }

        correctAnswersText.text = correctAnswers.ToString();
        wrongAnswersText.text = wrongAnswers.ToString();
        cur_coins_count_text.text = "+" + cur_coins_count;
        cur_points_text.text = QuestionnaireManager.cur_points.ToString();
        totalPoints += QuestionnaireManager.cur_points;

        totalCoins += cur_coins_count;

        GameSceneAnimations.instance.ResultScreenAnimations_IN(0.2f, () =>
        {
            //  increase total coins text incrementally with animation
            AnimateCoinsText(cur_coins_count, () =>
            {
                // total_coins_text.text = totalCoins.ToString();
                StartCoroutine(TextUpdater(totalCoins - cur_coins_count, totalCoins, total_coins_text));
                CoinsTextDefualtPos();
            });
        });
        PlayerPrefs.SetInt("Total_Coins", totalCoins);
        PlayerPrefs.SetInt("Total_Points", totalPoints);
        PlayerPrefs.Save();
    }

    void AnimateCoinsText(int coinsCount, Action onComplete = null)
    {
        float time = 0.3f;
        outlined_coinText_forAnimation.text = "+" + coinsCount;
        outlined_coinText_forAnimation.gameObject.SetActive(true);
        var rect = outlined_coinText_forAnimation.GetComponent<RectTransform>();
        rect.DOScale(new Vector2(2, 2), time * 2).OnComplete(() =>
        {
            rect.DOScale(Vector3.zero, time);
            rect.DOAnchorPos(new Vector2(-280, -770), time);
            outlined_coinText_forAnimation.DOFade(0.5f, time * 1.5f).OnComplete(() => { onComplete(); });
        });
    }

    void CoinsTextDefualtPos()
    {
        outlined_coinText_forAnimation.gameObject.SetActive(false);
        outlined_coinText_forAnimation.text = "";
        outlined_coinText_forAnimation.GetComponent<RectTransform>().DOScale(Vector3.one, 0);
        outlined_coinText_forAnimation.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-75f, 0), 0);
        outlined_coinText_forAnimation.DOFade(1, 0);
    }


    IEnumerator TextUpdater(int lastCoins, int newCoins, TextMeshProUGUI text)
    {
        while (lastCoins < newCoins)
        {
            lastCoins++;
            text.text = lastCoins.ToString();
            yield return new WaitForSeconds(0.2f);
        }

        QuestionnaireManager.instance.PingRect(text.GetComponent<RectTransform>());
    }


    public void PlayAgainButton()
    {
        GameSceneAnimations.instance.ResultScreenAnimations_OUT(0.2f, () => { SceneManager.LoadScene(1); });
    }

    public void HomeButton()
    {
        GameSceneAnimations.instance.ResultScreenAnimations_OUT(0.2f, () => { SceneManager.LoadScene(0); });
    }
}