using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen : MonoBehaviour
{
    public static int wrongAnswers;
    public static int correctAnswers;
    public static int cur_coins_count;


    private int points;
    public static int totalCoins;

    #region Result screen data

    [Header("Result screen data")] [SerializeField]
    private TextMeshProUGUI cur_points_text;

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
        total_coins_text.text = totalCoins.ToString();
        wrongAnswers = 0;
        correctAnswers = 0;
        cur_coins_count = 0;
    }

    public void GameOver()
    {
        GameSceneAnimations.instance.ResultScreenAnimations_IN(0.2f);
        correctAnswersText.text = correctAnswers.ToString();
        wrongAnswersText.text = wrongAnswers.ToString();
        cur_coins_count_text.text = "+" + cur_coins_count;
        cur_points_text.text = QuestionnaireManager.cur_points.ToString();

        totalCoins += cur_coins_count;
        // TODO: increase total coins text incrementally with animation
        total_coins_text.text = totalCoins.ToString();
        PlayerPrefs.SetInt("Total_Coins", totalCoins);
        PlayerPrefs.Save();
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