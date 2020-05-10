using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using Firebase.Auth;
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
    public static string invitationKey = null;


    public static ChallengeFriend instance = null;
    public static bool responseReceived = false;
    string token = null;
    bool isRequested = true;
    private bool waitingForResponse = false;

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

        var key = _reference.Child("Invitations").Push();
        key.SetRawJsonValueAsync(JsonUtility.ToJson(inv));
        invitationKey = key.Key;

        FetchOnlinePlayers.instance.waitingPanel.SetActive(true);
        waitingForResponse = true;
        StartCoroutine(WaitForResponse());
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

    IEnumerator WaitForResponse()
    {
        while (waitingForResponse)
        {
            yield return new WaitForSeconds(2);
            // Debug.LogError("Key: " + invitationKey);
            _reference.Child("Invitations").Child(invitationKey).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.LogError(task.Result.GetValue(true));

                    if (!task.Result.Exists)
                    {
                        Debug.LogError("invitation cancelled/accepted");

                        _reference.Child("Matches").GetValueAsync().ContinueWithOnMainThread(task1 =>
                        {
                            if (task1.IsCompleted)
                            {
                                var allNone = false;
                                foreach (var dataSnapShot in task1.Result.Children.ToList())
                                {
                                    var match = JsonUtility.FromJson<Match>(dataSnapShot.GetRawJsonValue());
                                    if (match.sender_id == FacebookAuthenticator.UID)
                                    {
                                        allNone = true;
                                        // accepted
                                        Debug.LogError("Request Accepted");
                                        waitingForResponse = false;
                                        
                                        // start quiz

                                        FetchOnlinePlayers.instance.waitingPanel.SetActive(false);
                                    }
                                }

                                if (!allNone)
                                {
                                    // hamari koi request match mein nai hai
                                    Debug.LogError("Request Rejected");
                                    waitingForResponse = false;
                                    FetchOnlinePlayers.instance.waitingPanel.SetActive(false);
                                }
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("abhi invitation jaari hai");
                    }
                }
            });
        }
    }

    IEnumerator CheckForRequest()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            if (isRequested)
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
                                isRequested = false;
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
        invitationPanel.transform.GetChild(0).gameObject.SetActive(false);

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
                        
                        // start quiz
                        
                        isRequested = true;
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
        if (!isRequested)
        {
            MyMsg.instance.Message("Lanat e", 2);
            // Debug.LogError("sUser rejected request = " + token);
            isRequested = true;
            _reference.Child("Invitations").Child(token).RemoveValueAsync();
            invitationPanel.transform.GetChild(0).gameObject.SetActive(false);
            FetchOnlinePlayers.instance.waitingPanel.SetActive(false);
        }
    }

    IEnumerator HideRequestPopUp()
    {
        yield return new WaitForSeconds(5);
        RejectRequest();
    }
}