using System;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FacebookAuthenticator : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;

    // public static FacebookAuthenticator instance = null;

    private void Awake()
    {
        // if (instance == null)
        // {
        //     instance = this;
        // }

        
#if !UNITY_EDITOR
        // initialize firebase and facebook sdk
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                user = auth.CurrentUser;
                // if facebook sdk in not initialized, initialize it

                if (!FB.IsInitialized)
                {
                    FB.Init(InitCallback, OnHideUnity);
                }
                else
                {
                    FB.ActivateApp();
                }
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies " + task);
            }
        });
#endif
    }

  


    void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            if (FB.IsLoggedIn)
            {
                // user is logged in
                // go to main menu
                AnimationsManager.instance.mainMenu.interactable = true;
                AnimationsManager.instance.logoutBtn.SetActive(true);
            }
            else
            {
                // user is not logged in
                AnimationsManager.instance.loginWindow.SetActive(true);
                AnimationsManager.instance.logoutBtn.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Fb can not initialized");
        }
    }

    void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }


    public void FB_Login()
    {
        var permissions = new List<string>() {"public_profile", "email"};
        FB.LogInWithReadPermissions(permissions, result =>
        {
            if (FB.IsLoggedIn)
            {
                var accessToken = AccessToken.CurrentAccessToken;
                SignInFirebase(accessToken);
                Debug.Log("UID: " + accessToken.UserId);
            }
            else
            {
                Debug.Log("User can not logged in");
            }
        });
    }

    void SignInFirebase(AccessToken accessToken)
    {
        Credential credential = FacebookAuthProvider.GetCredential(accessToken.TokenString);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }


            AnimationsManager.instance.loginWindow.SetActive(false);
            AnimationsManager.instance.mainMenu.interactable = true;
            AnimationsManager.instance.logoutBtn.SetActive(true);

            FirebaseUser newUser = task.Result;

            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
    }


    // TODO: delete this functionality in future
    public void FB_Logout()
    {
        if (FB.IsLoggedIn)
        {
            FB.LogOut();
        }
    }
}