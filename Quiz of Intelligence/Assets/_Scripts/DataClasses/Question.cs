using System;
using System.Collections.Generic;

[Serializable]
public class Questions
{
  public Question[] questions;
}
[Serializable]
public class Question
{
  public string _id;
  public string question;

  public string category;
  public int difficulty;
  public List<Answers> answers;
}

[Serializable]
public class Answers
{
  public string answerText;
  public bool isCorrect;
}
