using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen : MonoBehaviour
{
    public static int wrongAnswers;
    public static int correctAnswers;


    #region Result screen data

    [Header("Result screen data")] [SerializeField]
    private TextMeshProUGUI totalPointsText;

    [SerializeField] private TextMeshProUGUI timeTakenToSolveQuiz_text;
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
    }

    public void GameOver()
    {
        IncrementDifficulty();

        timeTakenToSolveQuiz_text.text = QuestionnaireManager.timeTakenToSolveQuiz.ToString("0.00") + " secs";
        correctAnswersText.text = correctAnswers.ToString();
        wrongAnswersText.text = wrongAnswers.ToString();
        totalPointsText.text = QuestionnaireManager.totalPoints.ToString();


        GameSceneAnimations.instance.ResultScreenAnimations_IN(0.2f);
        PlayerPrefs.Save();

        // save total points on firebase
        // UserUtils.instance.UpdateUserProgress(QuestionnaireManager.totalPoints, QuestionnaireManager.timeTakenToSolveQuiz.ToString("0.00"));
        //
        // UserUtils.instance.UserStatus(true);
    }

    void IncrementDifficulty()
    {
        // check if all answers are correct and user solve quiz with in required time
        var requiredTime = (float) (QuestionnaireManager.totalQuestions * 20) / 2;
        if (correctAnswers >= QuestionnaireManager.totalQuestions &&
            QuestionnaireManager.timeTakenToSolveQuiz <= requiredTime)
        {
            if (QuestionnaireManager.difficulty < 3)
            {
                QuestionnaireManager.difficulty++;
                PlayerPrefs.SetInt("DIFFICULTY", QuestionnaireManager.difficulty);
                PlayerPrefs.Save();
            }
        }
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