using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class Invitation
{
    public string sender;
    public string receiver;
}

[Serializable]
public class Match
{
    public string sender_id;
    public string receiver_id;

    public string receiver_pts;
    public string sender_pts;

    public string[] questionsIDs;
}

public class ChallengeFriend : MonoBehaviour
{
    private DatabaseReference _reference;


    public static ChallengeFriend instance = null;
    public string token;


    public static bool isAvailable = true;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }


        StartCoroutine(CheckForRequest());
        StartCoroutine(WaitFirebaseToLoad());
    }

    IEnumerator WaitFirebaseToLoad()
    {
        yield return new WaitUntil(() => { return FacebookAuthenticator.firebaseInitialized; });
        _reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void InviteFriend(string recID)
    {
        Debug.LogError(recID);
        var inv = new Invitation()
        {
            receiver = recID,
            sender = FacebookAuthenticator.UID
        };
        Debug.LogError(JsonUtility.ToJson(inv));

        _reference.Child("Invitations").Push().SetRawJsonValueAsync(JsonUtility.ToJson(inv));

        FetchOnlinePlayers.instance.waitingPanel.SetActive(true);
    }

    List<string> GreatQuestions()
    {
        var path = Path.Combine(Application.persistentDataPath, "Questions.json");
        var json = File.ReadAllText(path);
        var questionObject = JsonUtility.FromJson<Questions>(json);
        var questionsIds = questionObject.questions.ToList().ConvertAll(q => q._id).Shuffle().Take(7);
        return questionsIds.ToList();

        // questions.ConvertAll()
    }


    void CreateMatch(string rec, string send)
    {
        try
        {
            
            Match match = new Match()
            {
                sender_id = send,
                receiver_id = rec,
                questionsIDs = GreatQuestions().ToArray()
            };

            Debug.LogError("JSON: " + JsonUtility.ToJson(match));
            _reference.Child("Matches").Push().SetRawJsonValueAsync(JsonUtility.ToJson(match));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    IEnumerator CheckForRequest()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            if (isAvailable)
            {
                _reference.Child("Invitations").GetValueAsync().ContinueWith((task) =>
                {
                    if (task.IsCompleted)
                    {
                        foreach (var dataSnapShot in task.Result.Children.ToList())
                        {
                            token = dataSnapShot.Key;
                            var invitation = JsonUtility.FromJson<Invitation>(dataSnapShot.GetRawJsonValue());
                            if (invitation.receiver == FacebookAuthenticator.UID)
                            {
                                // Debug.LogError("snap: " + dataSnapShot.GetRawJsonValue());
                                isAvailable = false;
                                // this request is for me
                                // Debug.LogError("I was challenged");
                                InvitationReceived(invitation);
                            }
                        }
                    }

                    else
                    {
                        Debug.LogError("Request can not completed");
                    }
                });
            }
        }
    }

    private void InvitationReceived(Invitation inv)
    {
        _reference.Child("Users").GetValueAsync().ContinueWith((task) =>
        {
            if (task.IsCompleted)
            {
                var challengerName = task.Result.Child(inv.sender).Child("userName").GetValue(true).ToString();

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    invitationPanel = GameObject.FindGameObjectWithTag("Canvas");

                    invitationPanel.transform.GetChild(0).GetChild(1).GetChild(0)
                        .GetComponent<TextMeshProUGUI>().text = challengerName;

                    invitationPanel.transform.GetChild(0).gameObject.SetActive(true);
                    StartCoroutine(HideRequestPopUp());
                });
            }
        });
    }

    private GameObject invitationPanel;

    public void AcceptRequest()
    {
        isAvailable = true;
        invitationPanel.transform.GetChild(0).gameObject.SetActive(false);
        MyMsg.instance.Message("Dur fate mu", 2);

        _reference.Child("Invitations").GetValueAsync().ContinueWithOnMainThread((task) =>
        {
            if (task.IsCompleted)
            {
                foreach (var dataSnapShot in task.Result.Children.ToList())
                {
                    var _token = dataSnapShot.Key;
                    var invitation = JsonUtility.FromJson<Invitation>(dataSnapShot.GetRawJsonValue());
                    if (_token == token)
                    {
                        CreateMatch(invitation.receiver, invitation.sender);
                        _reference.Child("Invitations").Child(token).RemoveValueAsync();
                    }
                }
            }

            else
            {
                Debug.LogError("Request can not completed");
            }
        });

        // FetchOnlinePlayers.instance.waitingPanel.SetActive(false);
        
    }

    public void RejectRequest()
    {
        if (isAvailable) return;
        MyMsg.instance.Message("Lanat e", 2);
        // Debug.LogError("User rejected request = " + token);
        _reference.Child("Invitations").Child(token).RemoveValueAsync();
        invitationPanel.transform.GetChild(0).gameObject.SetActive(false);
        // FetchOnlinePlayers.instance.waitingPanel.SetActive(false);
    }

    IEnumerator HideRequestPopUp()
    {
        yield return new WaitForSeconds(5);
        RejectRequest();
    }
}