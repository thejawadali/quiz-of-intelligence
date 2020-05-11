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

    [Header("Result screen data for Single Player")] [SerializeField]
    private TextMeshProUGUI totalPointsText;

    [SerializeField] private TextMeshProUGUI timeTakenToSolveQuiz_text;
    [SerializeField] private TextMeshProUGUI wrongAnswersText;
    [SerializeField] private TextMeshProUGUI correctAnswersText;

    [Header("Result screen data for Multiplayer")] [SerializeField]
    private TextMeshProUGUI totalPointsText_other;

    [SerializeField] private TextMeshProUGUI timeTakenToSolveQuiz_text_other;
    [SerializeField] private TextMeshProUGUI correctAnswersText_other;
    [SerializeField] private TextMeshProUGUI wrongAnswersText_other;
    [SerializeField] private TextMeshProUGUI nameMy;
    [SerializeField] private TextMeshProUGUI nameother;
    [SerializeField] private GameObject myTag;
    [SerializeField] private GameObject otherTag;
    public TextMeshProUGUI resultText;

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
        myTag.SetActive(false);
        otherTag.SetActive(false);
    }

    public void GameOver()
    {
        IncrementDifficulty();

        timeTakenToSolveQuiz_text.text = QuestionnaireManager.timeTakenToSolveQuiz.ToString("0.00") + " secs";
        correctAnswersText.text = correctAnswers.ToString();
        wrongAnswersText.text = wrongAnswers.ToString();
        totalPointsText.text = QuestionnaireManager.totalPoints.ToString();


        if (!FacebookAuthenticator.isSinglePlayer)
        {
            var correctAnswers_other = 2;
            nameMy.text = FacebookAuthenticator.userName;
            nameother.text = "Other Man";
            timeTakenToSolveQuiz_text_other.text = 12.2f + "secs";
            correctAnswersText_other.text = correctAnswers_other.ToString();
            wrongAnswersText_other.text = 5.ToString();
            totalPointsText_other.text = 2.ToString();
            CheckWinner(correctAnswers, correctAnswers_other);
        }

        GameSceneAnimations.instance.ResultScreenAnimations_IN(0.2f);
        PlayerPrefs.Save();

        // save total points on firebase
    }

    void CheckWinner(int myPts, int otherPts)
    {
        if (myPts > otherPts)
        {
            // i m winner
            myTag.SetActive(true);
            resultText.text = "You wonnn!";
        }
        else if (myPts < otherPts)
        {
            // other is winnner
            resultText.text = "You Lose!";
            otherTag.SetActive(true);
        }
        else
        {
            resultText.text = "Match Tied!";
            // tied
        }
    }

    void IncrementDifficulty()
    {
        if (FacebookAuthenticator.isSinglePlayer)
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
    }


    public void PlayAgainButton()
    {
        GameSceneAnimations.instance.ResultScreenAnimations_OUT(0.2f, () => { SceneManager.LoadScene(1); });
    }

    public void HomeButton(bool resultScreen)
    {
        if (resultScreen)
        {
            GameSceneAnimations.instance.ResultScreenAnimations_OUT(0.2f, () => { SceneManager.LoadScene(0); });
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}