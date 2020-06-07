using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreen : MonoBehaviour
{
    public static int wrongAnswers;
    public static int correctAnswers;

    public GameObject waitingPanel_gameOver;

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


    private DatabaseReference _reference;


    public static ResultScreen instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        wrongAnswers = 0;
        correctAnswers = 0;
        if (!FacebookAuthenticator.isSinglePlayer)
        {
            _reference = FirebaseDatabase.DefaultInstance.RootReference;
            waitingPanel_gameOver.SetActive(false);

            myTag.SetActive(false);
            otherTag.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (!FacebookAuthenticator.isSinglePlayer)
        {
            nameMy.text = FacebookAuthenticator.userName;
            SendStatsToServer();

            waiting = true;
            waitingPanel_gameOver.SetActive(true);
            StartCoroutine(WaitForOpponentToFinish());
        }
        else
        {
            GameSceneAnimations.instance.ResultScreenAnimations_IN(0.2f);
        }

        IncrementDifficulty();

        timeTakenToSolveQuiz_text.text = QuestionnaireManager.timeTakenToSolveQuiz.ToString("0.00") + " secs";
        correctAnswersText.text = correctAnswers.ToString();
        wrongAnswersText.text = wrongAnswers.ToString();
        totalPointsText.text = QuestionnaireManager.totalPoints.ToString();


        PlayerPrefs.Save();
    }

    private bool waiting = false;

    IEnumerator WaitForOpponentToFinish()
    {
        while (waiting)
        {
            yield return new WaitForSeconds(3);
            _reference.Child("Matches").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    foreach (var dataSnapshot in task.Result.Children.ToList())
                    {
                        var match = JsonUtility.FromJson<Match>(dataSnapshot.GetRawJsonValue());
                        if (ChallengeFriend.isComingFromInvitation)
                        {
                            // receiver   
                            if (match.sender_completed)
                            {
                                waiting = false;
                                Debug.LogError("He also completed");
                                GetOpponentsScores();
                                // show result screen
                            }
                        }
                        else
                        {
                            // sender
                            if (match.receiver_completed)
                            {
                                // show result screen
                                waiting = false;
                                GetOpponentsScores();
                                // Debug.LogError("He also completed");
                            }
                        }
                    }
                }
            });
        }
    }


    void SendStatsToServer()
    {
        var db = _reference.Child("Matches").Child(ChallengeFriend.matchKey);
        if (ChallengeFriend.isComingFromInvitation)
        {
            //  receiver

            db.Child("receiver_completed").SetValueAsync(true);
            db.Child("receiver_pts").SetValueAsync(QuestionnaireManager.totalPoints);
            db.Child("receiver_time").SetValueAsync(QuestionnaireManager.timeTakenToSolveQuiz);
            db.Child("receiver_cAns").SetValueAsync(correctAnswers);
            db.Child("receiver_wAns").SetValueAsync(wrongAnswers);
        }
        else
        {
            // sender

            db.Child("sender_completed").SetValueAsync(true);
            db.Child("sender_pts").SetValueAsync(QuestionnaireManager.totalPoints);
            db.Child("sender_time").SetValueAsync(QuestionnaireManager.timeTakenToSolveQuiz);
            db.Child("sender_cAns").SetValueAsync(correctAnswers);
            db.Child("sender_wAns").SetValueAsync(wrongAnswers);
        }
    }


    void GetOpponentsScores()
    {
        Match myMatch = new Match();
        var time = 0.0f;
        var pts = 0;
        var cAns = 0;
        var wAns = 0;

        _reference.Child("Matches").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                foreach (var dataSnapshot in task.Result.Children.ToList())
                {
                    var token = dataSnapshot.Key;
                    if (token == ChallengeFriend.matchKey)
                    {
                        myMatch = JsonUtility.FromJson<Match>(dataSnapshot.GetRawJsonValue());
                        if (ChallengeFriend.isComingFromInvitation)
                        {
                            // i m receiver, you should get sender
                            time = myMatch.sender_time;
                            pts = myMatch.sender_pts;
                            cAns = myMatch.sender_cAns;
                            wAns = myMatch.sender_wAns;
                        }
                        else
                        {
                            // i m sender, you should get receiver
                            time = myMatch.receiver_time;
                            pts = myMatch.receiver_pts;
                            cAns = myMatch.receiver_cAns;
                            wAns = myMatch.receiver_wAns;
                        }

                        CheckWinner(QuestionnaireManager.totalPoints, pts);
                        timeTakenToSolveQuiz_text_other.text = time.ToString("0.00") + " secs";
                        totalPointsText_other.text = pts.ToString();
                        correctAnswersText_other.text = cAns.ToString();
                        wrongAnswersText_other.text = wAns.ToString();
                        nameother.text = MatchManager.nameOther;
                        GameSceneAnimations.instance.ResultScreenAnimations_IN(0.2f);
                        waitingPanel_gameOver.SetActive(false);
                        _reference.Child("Matches").Child(token).RemoveValueAsync();
                        _reference.Child("Matches").RemoveValueAsync();
                    }
                }
            }
        });
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
            GameSceneAnimations.instance.ResultScreenAnimations_OUT(0.2f, () =>
            {
                // Destroy(GameObject.FindGameObjectWithTag("Uni"));
                ChallengeFriend.isComingFromInvitation = false;
                SceneManager.LoadScene(0); 
                
            });
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}