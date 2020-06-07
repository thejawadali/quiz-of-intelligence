using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FetchOnlinePlayers : MonoBehaviour
{
    public GameObject waitingPanel;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject fetchedPlayerPanel;
    List<string> onlinePlayerNames = new List<string>();

    List<string> onlinePlayersIDs = new List<string>();

    // [SerializeField] private GameObject[] onlinePlayersTickets;
    [SerializeField] private GameObject ticketsParent;

    private DatabaseReference reference;


    private bool dataFetched = false;

    public static FetchOnlinePlayers instance = null;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        waitingPanel.SetActive(false);
        loadingText.SetActive(true);
        fetchedPlayerPanel.SetActive(false);
        if (FacebookAuthenticator.firebaseInitialized)
        {
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            GetOnlinePlayersList();
        }
    }

    public void GetOnlinePlayersList()
    {
        StartCoroutine(WaitUntilAllPlayersAreFetched());
        reference.Child("Users").GetValueAsync().ContinueWith((task) =>
        {
            var dataSnapshots = task.Result;

            foreach (var dataSnapShot in dataSnapshots.Children.ToList())
            {
                if (dataSnapShot.Key == FacebookAuthenticator.UID) continue;
                var players = dataSnapshots.Child(dataSnapShot.Key).Child("userName").GetValue(true);
                onlinePlayerNames.Add(players.ToString());
                onlinePlayersIDs.Add(dataSnapshots.Child(dataSnapShot.Key).Key);
                // Debug.LogError("Name: " + players);
            }

            dataFetched = true;
        });
    }

    IEnumerator WaitUntilAllPlayersAreFetched()
    {
        yield return new WaitUntil(() => { return dataFetched; });
        if (onlinePlayerNames.Count < 1)
        {
            // no player is online
            Debug.LogError("No Player is onl");
            loadingText.GetComponent<TextMeshProUGUI>().text = "Can't find any opponent";
        }
        // Debug.LogError("VAR" + onlinePlayerNames.Count);
        for (int i = 0; i < onlinePlayerNames.Count; i++)
        {
            loadingText.SetActive(false);
            fetchedPlayerPanel.SetActive(true);
            // ticketsParent.transform.GetChild(i).gameObject
            ticketsParent.transform.GetChild(i).gameObject.SetActive(true);
            ticketsParent.transform.GetChild(i).gameObject.transform.GetChild(1).gameObject.name = onlinePlayersIDs[i];
            // Debug.LogError("Name: " + onlinePlayersIDs[i]);
            ticketsParent.transform.GetChild(i).gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                onlinePlayerNames[i];
        }
    }
}