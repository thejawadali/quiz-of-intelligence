using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionGenerator : MonoBehaviour
{
    #region Texts

    [Header("Questions and options texts")]
    public TextMeshProUGUI questionText;

    public TextMeshProUGUI[] answerTexts;

    [Space(12)] [Header("timer components/items")]
    public TextMeshProUGUI timer_text;

    public Image timer_slider;

    #endregion

    // Create a property to handle the slider's value
    private float currentValue = 0f;

    /// <summary>
    /// Max time to solve a questions 
    /// </summary>
    [SerializeField] [Header("Max time to solve a questions")]
    private float maxValue = 5f;

    private float minValue = 0f;

    public static int currentQuestion;
    private Questions questionObject;
    private Question ques;
    private string json;

    private List<int> wrongAnswersListForHint = new List<int>();

    public static QuestionGenerator instance = null;

    private void Awake()
    {
        // singleton
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        // get json file
        json = File.ReadAllText(Application.dataPath + "/output.json");
        questionObject = JsonUtility.FromJson<Questions>(json);
        // get question when quiz starts
        GetQuestion();
    }

    private void FixedUpdate()
    {
        // start timer only when question is fully loaded
        if (QuestionsAnimations.gameStarted)
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
    void GetQuestion()
    {
        ques = questionObject.questions[currentQuestion];

        // question's text
        questionText.text = ques.question;
        // shuffle answers list
        ques.answers = ques.answers.Shuffle().ToList();
        // answers text
        for (var i = 0; i < ques.answers.Count; i++)
        {
            answerTexts[i].text = ques.answers[i].answerText;

            // populate wrong answer
            // if (!q.answers[i].isCorrect)
            // {
            //     if (wrongAnswersListForHint.Count <= 3)
            //     {
            //         wrongAnswersListForHint.Add(i);
            //     }
            // }
        }
    }

    /// <summary>
    ///  to get new question
    /// </summary>
    public void GetNewQuestion()
    {
        GetQuestion();
        QuestionsAnimations.instance.AnimateQuestions_IN(0.2f);
        Reset();
    }

    private void Reset()
    {
        CurrentValue = 0;
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
        QuestionsAnimations.gameStarted = false;
        currentQuestion++;
        // go to next question with some delay
        StartCoroutine(WaitAndAnimateQuestionsOUT());


        if (isCorrect)
        {
            // correct answer
        }
        else
        {
            // wrong answer

            // if user gave wrong answer, show him the correct answer
            for (int i = 0; i < ques.answers.Count; i++)
            {
                if (ques.answers[i].isCorrect)
                {
                    var a = answerTexts[i].transform.parent.GetChild(1);
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
        var image = answerTexts[index].transform.parent.GetChild(1);
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


    IEnumerator WaitAndAnimateQuestionsOUT()
    {
        yield return new WaitForSeconds(0.5f);
        QuestionsAnimations.instance.AnimateQuestions_OUT(0.2f);
    }


    public void AnswerButtonListeners(int index)
    {
        if (QuestionsAnimations.gameStarted)
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
}