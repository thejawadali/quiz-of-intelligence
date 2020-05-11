using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class QuestionnaireManager : MonoBehaviour
{
    List<string> alreadyAskedQuestion_id = new List<string>();
    [SerializeField] private TextMeshProUGUI cur_points_text;


    [Space(12)]

    #region Texts quiz screen

    [SerializeField]
    private TextMeshProUGUI hint_coin_text;

    [Header("Questions and options texts")] [SerializeField]
    private TextMeshProUGUI questionsCounter_GameScene;


    [SerializeField] private TextMeshProUGUI questionText;

    [SerializeField] private CanvasGroup[] options;

    [Space(12)] [Header("timer components/items")] [SerializeField]
    private TextMeshProUGUI timer_text;

    [SerializeField] private Image timer_slider;
    [SerializeField] private TextMeshProUGUI playerName_text;

    #endregion

    [SerializeField] public TextMeshProUGUI total_coins_text;

    // [Space(12)]
    /// <summary>
    /// Total question in a questionnaire 
    /// </summary>
    [SerializeField] [Header("Total question in a questionnaire")]
    public static int totalQuestions = 7;


    /// <summary>
    /// Max time to solve a questions 
    /// </summary>
    [SerializeField] [Header("Max time to solve a questions")]
    private float maxValue = 5f;

    [SerializeField] [Header("Min coins to take hint")]
    private int minCoinsForHint = 5;

    private float minValue = 0f;

    // Create a property to handle the slider's value
    private float currentValue = 0f;
    private int currentQuestion;
    public static int totalPoints;

    public static int difficulty;

    public static float timeTakenToSolveQuiz = 0;

    private Questions questionObject;
    private Question ques;
    private string json;

    #region Hint data

    /// <summary>
    /// list of all wrong options index
    /// </summary>
    private List<int> wrongOptionsListForHint = new List<int>();

    /// <summary>
    /// list of wrongAnswersListForHint that have been used
    /// </summary>
    private List<int> usedHintNo = new List<int>();

    List<string> questionIDs = new List<string>();

    /// <summary>
    /// random button from wrongAnswersListForHint to fade
    /// </summary>
    int randomHintButton;

    #endregion

    private int totalCoins;


    private bool hintTaken = false;

    public static QuestionnaireManager instance = null;

    private void Awake()
    {
        currentQuestion = 0;
        // singleton
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // getting difficulty
        difficulty = PlayerPrefs.GetInt("DIFFICULTY", 1);
        totalCoins = PlayerPrefs.GetInt("Total_Coins");
        total_coins_text.text = totalCoins.ToString();

        // setting user name in game scene
        playerName_text.text = FacebookAuthenticator.userName;
        // playerName_text.text = "You"


        alreadyAskedQuestion_id.Clear();
        hint_coin_text.text = minCoinsForHint.ToString();
        if (totalCoins >= minCoinsForHint)
        {
            // hint button is intractable
            GameSceneAnimations.instance.hintBtn.GetComponent<CanvasGroup>().interactable = true;
            GameSceneAnimations.instance.hintBtn.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            // hint button is not intractable
            GameSceneAnimations.instance.hintBtn.GetComponent<CanvasGroup>().interactable = false;
            GameSceneAnimations.instance.hintBtn.GetComponent<CanvasGroup>().alpha = 0.5f;
        }

        // set timer_text = maxValue 
        timer_text.text = maxValue.ToString("f0");

        // get json file
        var path = Path.Combine(Application.persistentDataPath, "Questions.json");
        json = File.ReadAllText(path);
        questionObject = JsonUtility.FromJson<Questions>(json);


        if (FacebookAuthenticator.isSinglePlayer)
        {
            for (int i = 0; i < totalQuestions; i++)
            {
                questionIDs.Add(GenerateQuestionId());
            }

            GetQuestion();
        }
    }


    private void FixedUpdate()
    {
        // start timer only when question is fully loaded
        if (GameSceneAnimations.gameStarted)
        {
            // timer starts
            CurrentValue += Time.deltaTime;
            // check if user can not answer in given time, its wrong answer man
            if (CurrentValue >= maxValue)
            {
                AnswerResponse(false);
            }
        }
    }

    /// <summary>
    /// to get questions
    /// </summary>
    public void GetQuestion()
    {
        try
        {
            ques = GetQuestionByID(currentQuestion);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        // question's text
        questionText.text = ques.question + " " + ques.difficulty + " " + ques.category;
        // questionText.text = ques.question;
        // shuffle answers list
        ques.answers = ques.answers.Shuffle().ToList();
        // answers text
        for (var i = 0; i < ques.answers.Count; i++)
        {
            options[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ques.answers[i].answerText;
            // populate wrong answer
            if (!ques.answers[i].isCorrect)
            {
                if (wrongOptionsListForHint.Count <= 3)
                {
                    wrongOptionsListForHint.Add(i);
                }
            }

            // its temp
            if (ques.answers[i].isCorrect)
            {
                options[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text += "\t c";
            }
        }

        questionsCounter_GameScene.text = (currentQuestion + 1) + "/" + totalQuestions;
    }

    Question GetQuestionByID(int _cur)
    {
        List<string> quesList = new List<string>();
        if (FacebookAuthenticator.isSinglePlayer)
        {
            quesList = questionIDs;
        }
        else
        {
            quesList = MatchManager.questionIDs;
            // Debug.LogError("Counter: " + quesList.Count);
        }

        Question question = new Question();
        var questions = questionObject.questions.Where(ques => ques._id == quesList[_cur]).ToList();
        question = questions[0];
        return question;
    }

    public string GenerateQuestionId()
    {
        Category cat = UiManager.category;
        int difficulty = QuestionnaireManager.difficulty;
        Question newQuestion = new Question();
        // getting question of certain category and difficulty level
        var questions = questionObject.questions
            .Where(ques => ques.category == cat.ToString() && ques.difficulty == difficulty).ToList();

        // get question randomly from any category
        if (cat == Category.ALL)
        {
            questions = questionObject.questions.Where(ques => ques.difficulty == difficulty).ToList();
        }


        // getting random but unique questions from 'questions'
        var remainingQuestions = questions.SkipWhile(cq => alreadyAskedQuestion_id
            .Contains(cq._id)).ToList();
        if (remainingQuestions.Count > 0)
        {
            newQuestion = remainingQuestions[Random.Range(0, remainingQuestions.Count)];
            alreadyAskedQuestion_id.Add(newQuestion._id);
        }

        return newQuestion._id;
    }

    /// <summary>
    ///  to get new question
    /// </summary>
    public void GetNewQuestion()
    {
        Reset();
        GetQuestion();
        GameSceneAnimations.instance.AnimateQuestions_IN(0.3f);
    }

    private void Reset()
    {
        hintTaken = false;
        CurrentValue = 0;
        wrongOptionsListForHint.Clear();
        usedHintNo.Clear();
        // make all option Buttons intractable
        foreach (var op in options)
        {
            op.interactable = true;
        }
    }


    /// <summary>
    /// Get time since questions loaded
    /// </summary>
    public float CurrentValue
    {
        get { return currentValue; }
        set
        {
            // Ensure the passed value falls within min/max range
            currentValue = Mathf.Clamp(value, minValue, maxValue);

            // Calculate the current fill percentage and display it
            float fillPercentage = currentValue / maxValue;
            timer_slider.fillAmount = fillPercentage;

            float countDown = maxValue - currentValue;
            timer_text.text = countDown.ToString("f0");
        }
    }


    /// <summary>
    /// Called on response of every question
    /// </summary>
    /// <param name="isCorrect">true if answer is correct</param>
    void AnswerResponse(bool isCorrect)
    {
        GameSceneAnimations.gameStarted = false;
        currentQuestion++;
        // go to next question with some delay
        StartCoroutine(WaitAndAnimateQuestionsOUT());


        if (isCorrect)
        {
            // correct answer
            ResultScreen.correctAnswers++;
            // reward coins only if user did not take hint
            if (!hintTaken)
            {
                totalCoins += CoinsReward(CurrentValue);
                StartCoroutine(TextUpdater(totalCoins - CoinsReward(CurrentValue), totalCoins, total_coins_text));
                PlayerPrefs.SetInt("Total_Coins", totalCoins);
                PlayerPrefs.Save();
            }

            timeTakenToSolveQuiz += CurrentValue;

            totalPoints += 10;
            cur_points_text.text = totalPoints.ToString();
        }
        else
        {
            // wrong answer
            ResultScreen.wrongAnswers++;

            // if user gave wrong answer, show him the correct answer
            for (int i = 0; i < ques.answers.Count; i++)
            {
                if (ques.answers[i].isCorrect)
                {
                    var a = options[i].transform.GetChild(1);
                    a.gameObject.SetActive(true);
                    a.GetComponent<Image>().color = Color.green;
                }
            }
        }
    }


    /// <summary>
    /// Show green or red sprite, to show user answer response.
    /// Overloading only to show correct/wrong answer sprite, only called if user answered on time 
    /// </summary>
    /// <param name="isCorrect">true if user answered correct</param>
    /// <param name="index">index of option</param>
    void AnswerResponse(bool isCorrect, int index)
    {
        // give user response about his answer
        var image = options[index].transform.GetChild(1);
        image.gameObject.SetActive(true);

        if (isCorrect)
        {
            // its correct answer man
            image.GetComponent<Image>().color = Color.green;

            AnswerResponse(true);
        }
        else
        {
            // its wrong answer man
            image.GetComponent<Image>().color = Color.red;
            AnswerResponse(false);
        }
    }


    /// <summary>
    /// To reward user with coins based on time he answered the question
    /// </summary>
    /// <param name="time">Time taken to answer</param>
    /// <returns></returns>
    int CoinsReward(float time)
    {
        if (time > 0 && time <= 5)
        {
            // reward is 4 coins
            return 4;
        }

        if (time > 5 && time <= 10)
        {
            // reward is 3 coins
            return 3;
        }

        if (time > 10 && time <= 15)
        {
            // reward is 2 coins
            return 2;
        }

        if (time > 15 && time <= 20)
        {
            // reward is 1 coin
            return 1;
        }
        else
        {
            return 0;
        }
    }


    IEnumerator WaitAndAnimateQuestionsOUT()
    {
        yield return new WaitForSeconds(0.5f);
        if (currentQuestion >= totalQuestions)
        {
            // show result screen
            ShowResultScreen();
        }
        else
        {
            // continue with next question
            GameSceneAnimations.instance.AnimateQuestions_OUT(0.2f);
        }
    }


    /// <summary>
    /// Game over, lets show result screen
    /// </summary>
    void ShowResultScreen()
    {
        GameSceneAnimations.instance.AllComponentsAnimations_OUT(0.2f,
            () => { ResultScreen.instance.GameOver(); });
    }


    public void HintButton_QS()
    {
        if (!hintTaken && GameSceneAnimations.gameStarted)
        {
            if (totalCoins >= minCoinsForHint)
            {
                // can take hint
                hintTaken = true;

                // fade 2 wrong options
                for (int i = 0; i < 2; i++)
                {
                    while (usedHintNo.Contains(randomHintButton))
                    {
                        randomHintButton = Random.Range(0, wrongOptionsListForHint.Count);
                    }

                    usedHintNo.Add(randomHintButton);
                    options[wrongOptionsListForHint[randomHintButton]].interactable = false;
                }

                totalCoins -= minCoinsForHint;
                total_coins_text.text = totalCoins.ToString();
                PingRect(total_coins_text.GetComponent<RectTransform>());
                PlayerPrefs.SetInt("Total_Coins", totalCoins);
                PlayerPrefs.Save();
                if (totalCoins < minCoinsForHint)
                {
                    GameSceneAnimations.instance.hintBtn.GetComponent<CanvasGroup>().interactable = false;
                    GameSceneAnimations.instance.hintBtn.GetComponent<CanvasGroup>().alpha = 0.5f;
                }
            }
        }
    }

    void PingRect(RectTransform rect)
    {
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(rect.DOScale(new Vector2(0.9f, 0.9f), 0.1f))
            .Append(rect.DOScale(new Vector2(1.1f, 1.1f), 0.1f))
            .Append(rect.DOScale(Vector3.one, 0.1f));
    }

    // working on coins deduction for hint

    public void AnswerButtonListeners(int index)
    {
        if (GameSceneAnimations.gameStarted)
        {
            Question questionObj = ques;
            switch (index)
            {
                case 1:
                    if (questionObj.answers[0].isCorrect)
                    {
                        AnswerResponse(true, 0);
                    }
                    else
                    {
                        AnswerResponse(false, 0);
                    }

                    break;
                case 2:
                    if (questionObj.answers[1].isCorrect)
                    {
                        AnswerResponse(true, 1);
                    }
                    else
                    {
                        AnswerResponse(false, 1);
                    }

                    break;
                case 3:
                    if (questionObj.answers[2].isCorrect)
                    {
                        AnswerResponse(true, 2);
                    }
                    else
                    {
                        AnswerResponse(false, 2);
                    }

                    break;
                case 4:
                    if (questionObj.answers[3].isCorrect)
                    {
                        AnswerResponse(true, 3);
                    }
                    else
                    {
                        AnswerResponse(false, 3);
                    }

                    break;
            }
        }
    }


    IEnumerator TextUpdater(int lastCoins, int newCoins, TextMeshProUGUI text)
    {
        while (lastCoins < newCoins)
        {
            lastCoins++;
            text.text = lastCoins.ToString();
            yield return new WaitForSeconds(0.2f);
        }

        PingRect(text.GetComponent<RectTransform>());
    }
}