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
using UnityEngine.SceneManagement;
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

    public int receiver_pts;
    public int sender_pts;

    public bool sender_completed;
    public bool receiver_completed;

    public int sender_cAns;
    public int receiver_cAns;

    public int sender_wAns;
    public int receiver_wAns;

    public float sender_time;
    public float receiver_time;

    public string[] questionsIDs;

    
}

public class ChallengeFriend : MonoBehaviour
{
    private DatabaseReference _reference;
    public static string invitationKey = null;
    public static string matchKey = null;

    public static bool responseReceived = false;
    string token = null;
    bool requestRespond = true;
    public static bool isComingFromInvitation = false;

    private bool waitingForResponse = false;

    // public static bool comingFromAcception = false;
    public static ChallengeFriend instance = null;

    public void Start()
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
        // Debug.LogError(recID);
        var inv = new Invitation()
        {
            receiver = recID,
            sender = FacebookAuthenticator.UID
        };
        // Debug.LogError(JsonUtility.ToJson(inv));

        var key = _reference.Child("Invitations").Push();
        key.SetRawJsonValueAsync(JsonUtility.ToJson(inv));
        invitationKey = key.Key;
        // show waiting panel to user who requests friend
        FetchOnlinePlayers.instance.waitingPanel.SetActive(true);
        // and start waiting for response
        waitingForResponse = true;
        StartCoroutine(WaitForResponse());
        StartCoroutine(RequestTimeOut());
    }


    IEnumerator RequestTimeOut()
    {
        yield return new WaitForSeconds(60);
        CancelRequest();
    }
    
    public void CancelRequest()
    {
        waitingForResponse = false;
        _reference.Child("Invitations").Child(token).RemoveValueAsync();
        
    }

  
    IEnumerator WaitForResponse()
    {
        while (waitingForResponse)
        {
            yield return new WaitForSeconds(2);
            // Debug.LogError("waiting for response = " + invitationKey);

            _reference.Child("Invitations").Child(invitationKey).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    if (!task.Result.Exists)
                    {
                        // it means invitation has been deleted, now look for matches

                        // Debug.LogError("invitation cancelled/accepted");

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
                                        // accepted
                                        matchKey = dataSnapShot.Key;

                                        allNone = true;
                                        MyMsg.instance.Message("Challenge Accepted", 1);
                                        waitingForResponse = false;


                                        // start quiz
                                        FacebookAuthenticator.isSinglePlayer = false;
                                        FetchOnlinePlayers.instance.waitingPanel.SetActive(false);
                                        MultiplayerSplash.instance.AnimateSplash();
                                        MatchManager.instance.QuestionIDs();
                                    }
                                }

                                if (!allNone)
                                {
                                    // request rejected
                                    MyMsg.instance.Message("Challenge Rejected", 1);
                                    waitingForResponse = false;
                                    FetchOnlinePlayers.instance.waitingPanel.SetActive(false);
                                }
                            }
                        });
                    }
                    else
                    {
                        // Debug.LogError("abhi invitation jaari hai");
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
            if (requestRespond)
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
                                requestRespond = false;
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
        _reference.Child("Users").GetValueAsync().ContinueWithOnMainThread((task) =>
        {
            if (task.IsCompleted)
            {
                var challengerName = task.Result.Child(inv.sender).Child("userName").GetValue(true).ToString();

                invitationPanel = GameObject.FindGameObjectWithTag("Canvas");

                invitationPanel.transform.GetChild(0).GetChild(1).GetChild(0)
                    .GetComponent<TextMeshProUGUI>().text = challengerName;

                invitationPanel.transform.GetChild(0).gameObject.SetActive(true);
                StartCoroutine(HideRequestPopUp());
            }
        });
    }


    private GameObject invitationPanel;

    public void AcceptRequest()
    {
        _reference.Child("Invitations").GetValueAsync().ContinueWithOnMainThread((task) =>
        {
            if (task.IsCompleted)
            {
                foreach (var dataSnapShot in task.Result.Children.ToList())
                {
                    // invitation key
                    var _token = dataSnapShot.Key;
                    var invitation = JsonUtility.FromJson<Invitation>(dataSnapShot.GetRawJsonValue());
                    if (_token == token)
                    {
                        CreateMatch(invitation.receiver, invitation.sender);

                        // start quiz
                        FacebookAuthenticator.isSinglePlayer = false;
                        invitationPanel.transform.GetChild(0).gameObject.SetActive(false);
                        SceneManager.LoadScene(2);
                        isComingFromInvitation = true;
                        requestRespond = true;
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
        if (!requestRespond)
        {
            Debug.LogError("Called reject method");
            requestRespond = true;
            invitationPanel.transform.GetChild(0).gameObject.SetActive(false);
            _reference.Child("Invitations").Child(token).RemoveValueAsync();
        }
    }

    // start timer as soon as receiver gets invitation and hide request pop up after 10 secs
    IEnumerator HideRequestPopUp()
    {
        yield return new WaitForSeconds(10);
        RejectRequest();
    }
    
    
    List<string> CreateQuestionList()
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
                questionsIDs = CreateQuestionList().ToArray()
            };

            var key = _reference.Child("Matches").Push();
            key.SetRawJsonValueAsync(JsonUtility.ToJson(match));
            matchKey = key.Key;
            // _reference.Child("Matches").Push().SetRawJsonValueAsync(JsonUtility.ToJson(match));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    
}