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

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI[] answerTexts;

    #endregion

    public static int currentQuestion;
    private Questions ques;
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
        ques = JsonUtility.FromJson<Questions>(json);
        // get question when quiz starts
        GetQuestion();
    }


    /// <summary>
    /// to get questions
    /// </summary>
    void GetQuestion()
    {
        var q = ques.questions[currentQuestion];

        // question's text
        questionText.text = q.question;
        // shuffle answers list
        q.answers = q.answers.Shuffle().ToList();
        // answers text
        for (var i = 0; i < q.answers.Count; i++)
        {
            answerTexts[i].text = q.answers[i].answerText;

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
    }

    /// <summary>
    /// Answer's response
    /// </summary>
    /// <param name="isCorrect">true for correct answer</param>
    /// <param name="index">index of option</param>
    void AnswerResponse(bool isCorrect, int index)
    {
        QuestionsAnimations.gameStarted = false;
        currentQuestion++;
        // go to next question with some delay
        StartCoroutine(WaitAndAnimateQuestionsOUT());

        // give user response about his answer
        var image = answerTexts[index].transform.parent.GetChild(1);
        image.gameObject.SetActive(true);

        if (isCorrect)
        {
            // its correct answer man
            image.GetComponent<Image>().color = Color.green;
        }
        else
        {
            // its wrong answer man
            image.GetComponent<Image>().color = Color.red;
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
            Question questionObj = ques.questions[currentQuestion];
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