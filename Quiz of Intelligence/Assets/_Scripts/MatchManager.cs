using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static List<string> questionIDs = new List<string>();

    public static string nameOther = "";
    Match myMatch = new Match();
    private DatabaseReference _reference;

    public static MatchManager instance = null;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        _reference = FirebaseDatabase.DefaultInstance.RootReference;

        
        // if you are receiver, start game directly
        if (ChallengeFriend.isComingFromInvitation)
        {
            QuestionIDs();
        }
    }


    public void QuestionIDs()
    {
        _reference.Child("Matches").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            // Debug.LogError("Entered Matches");
            if (task.IsCompleted)
            {
                foreach (var dataSnapshot in task.Result.Children.ToList())
                {
                    // Debug.LogError("Entered Matches foreach loop");
                    var token = dataSnapshot.Key;
                    // Debug.LogError("To: " + token);
                    // Debug.LogError("mk: " + ChallengeFriend.matchKey);

                    if (token == ChallengeFriend.matchKey)
                    {
                        // Debug.LogError("tokens are same");
                        myMatch = JsonUtility.FromJson<Match>(dataSnapshot.GetRawJsonValue());
                        questionIDs = myMatch.questionsIDs.ToList();
                        QuestionnaireManager.instance.GetQuestion();
                        ApplyUserNames();
                        // Debug.LogError("Got ques" + dataSnapshot.GetRawJsonValue());
                    }
                }
            }
        });
    }

    void ApplyUserNames()
    {
        _reference.Child("Users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            // if isComingFromInvitation, 
            
            if (task.IsCompleted)
            {
                if (ChallengeFriend.isComingFromInvitation)
                {
                    //i am receiver

                    // var nameMy = task.Result.Child(myMatch.receiver_id).Child("userName").GetValue(true)
                    //     .ToString();
                    nameOther = task.Result.Child(myMatch.sender_id).Child("userName").GetValue(true)
                        .ToString();
                }
                else
                {
                    // i m sender

                    nameOther = task.Result.Child(myMatch.receiver_id).Child("userName").GetValue(true)
                        .ToString();
                }

                MultiplayerSplash.instance.nameText_mine.text = FacebookAuthenticator.userName;
                MultiplayerSplash.instance.nameText_other.text = nameOther;
            }
        });
    }
}