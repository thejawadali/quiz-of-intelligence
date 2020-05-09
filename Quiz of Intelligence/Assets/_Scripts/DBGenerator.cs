using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DBGenerator : MonoBehaviour
{
    private bool isDownloading = true;
    public GameObject progressGroup;
    public Image progressBar;

    private void Start()
    {
        if (File.Exists(Application.persistentDataPath + Path.DirectorySeparatorChar + "Questions.json"))
        {
            // db exists, continue
            DBGenerated();
        }
        else
        {
            CreateDB();
        }
    }

    /// <summary>
    /// To create questions DB
    /// </summary>
    void CreateDB()
    {
        progressGroup.SetActive(true);
        StartCoroutine(GetRequest("https://config.nitroxis.com/questions",
            webreq => { StartCoroutine(ProgressCoroutine(webreq)); }, (isSuccessful, response) =>
            {
                isDownloading = false;
                Debug.Log(1);
                if (isSuccessful)
                {
                    try
                    {
                        Debug.Log(2);
                        File.WriteAllText(
                            Application.persistentDataPath + Path.DirectorySeparatorChar + "Questions.json",
                            response);
                        DBGenerated();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(3);
                        Debug.Log(e.Message);
                    }
                }
                else
                {
                    Debug.Log(4);
                    // MyMsg.showError("Check your internet connection and try again");
                    return;
                }
            }));
    }


    
    /// <summary>
    /// DB has already created
    /// </summary>
    void DBGenerated()
    {
        progressGroup.SetActive(false);
        FacebookAuthenticator.instance.InitializeFirebase();
    }


    IEnumerator ProgressCoroutine(UnityWebRequest req)
    {
        while (isDownloading)
        {
            progressBar.fillAmount = req.downloadProgress;
            yield return new WaitForSeconds(0.1f);
        }
    }


    public static IEnumerator GetRequest(string uri, Action<UnityWebRequest> progress = null,
        Action<bool, string> sendBackResponse = null)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            progress(webRequest);
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                sendBackResponse(false, "Network error");
            }
            else
            {
                sendBackResponse(true, webRequest.downloadHandler.text);
            }
        }
    }
}