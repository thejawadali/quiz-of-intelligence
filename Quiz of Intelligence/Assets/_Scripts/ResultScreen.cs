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
    private int points;
    private int cur_coins_count;

    #region Result screen data

    [Header("Result screen data")] [SerializeField]
    private TextMeshProUGUI points_text;

    [SerializeField] private TextMeshProUGUI cur_coins_count_text;
    [SerializeField] private TextMeshProUGUI wrongAnswersText;
    [SerializeField] private TextMeshProUGUI correctAnswersText;

    #endregion

    public static ResultScreen instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        wrongAnswers = 0;
        correctAnswers = 0;
        cur_coins_count = 0;
    }

    public void GameOver()
    {
        GameSceneAnimations.instance.ResultScreenAnimations_IN(0.2f);
        correctAnswersText.text = correctAnswers.ToString();
        wrongAnswersText.text = wrongAnswers.ToString();
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