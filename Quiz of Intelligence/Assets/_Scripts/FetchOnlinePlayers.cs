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
    [SerializeField] private GameObject loadingText;
    [SerializeField] private GameObject fetchedPlayerPanel;
    List<string> onlinePlayerNames = new List<string>();
    [SerializeField] private GameObject[] onlinePlayersTickets;
    private DatabaseReference reference;


    private bool dataFetched = false;


    // Start is called before the first frame update
    void Start()
    {
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
        StartCoroutine(Wait());
        reference.Child("Users").GetValueAsync().ContinueWith((task) =>
        {
            var dataSnapshots = task.Result;

            foreach (var dataSnapShot in dataSnapshots.Children.ToList())
            {
                if (dataSnapShot.Key != FacebookAuthenticator.UID)
                {
                    var players = dataSnapshots.Child(dataSnapShot.Key).Child("userName").GetValue(true);
                    onlinePlayerNames.Add(players.ToString());
                    // Debug.LogError("Name: " + players);
                }
            }
            dataFetched = true;    
        });
    }

    IEnumerator Wait()
    {
        yield return new WaitUntil(() => { return dataFetched; });
        // Debug.LogError("VAR" + onlinePlayerNames.Count);
        for (int i = 0; i < onlinePlayerNames.Count; i++)
        {
            loadingText.SetActive(false);
            fetchedPlayerPanel.SetActive(true);
            onlinePlayersTickets[i].SetActive(true);
            onlinePlayersTickets[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = onlinePlayerNames[i];
        }
    }
}