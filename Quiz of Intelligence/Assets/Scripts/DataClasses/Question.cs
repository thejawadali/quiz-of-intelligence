using System;

[Serializable]
public class Question
{
  public Category category;
  public Difficulty difficulty;
  public string question_text;
  public string correct_answer;
  public string[] incorrect_answer;


}
