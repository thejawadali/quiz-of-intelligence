using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facebook.Unity;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FacebookAuthenticator : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference reference;

    public static bool isSinglePlayer;

    public static string UID;
    public static string userName;


    public static bool firebaseInitialized = false;

    public static FacebookAuthenticator instance = null;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
        // InitializeFirebase();
    }

    #region Initialize fireabase and facebook

    public void InitializeFirebase()
    {
        // initialize firebase and facebook sdk
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                firebaseInitialized = true;

                // if facebook sdk in not initialized, initialize it
#if UNITY_EDITOR
                HandleDB();
#else
                if (!FB.IsInitialized)
                {
                    FB.Init(InitCallback, OnHideUnity);
                }
                else
                {
                    FB.ActivateApp();
                }
#endif
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies " + task);
            }
        });
    }

    void HandleDB()
    {
        if (firebaseInitialized)
        {
            // Set up the Editor before calling into the realtime database.
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://fir-and-unity-testing.firebaseio.com/");
            // Get the root reference location of the database.
            reference = FirebaseDatabase.DefaultInstance.RootReference;
#if UNITY_EDITOR

            reference.Child("Users").Child("001").Child("userName").SetValueAsync("Unity");
            UID = "001";
#else
            reference.Child("Users").Child(user.UserId).Child("userName").SetValueAsync(user.DisplayName);
            UID = user.UserId;
            userName = user.DisplayName;
#endif
            FirebaseDatabase.DefaultInstance.GetReference("Users").ValueChanged += HandleValueChanged;
        }
    }


    void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            if (FB.IsLoggedIn)
            {
                // user has logged in
                // go to main menu
                user = auth.CurrentUser;
                HandleDB();
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

    #endregion


    #region Login

    public void FB_Login()
    {
        var permissions = new List<string>() {"public_profile"};
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

    #endregion

    // TODO: delete this functionality in future
    public void FB_Logout()
    {
        if (FB.IsLoggedIn)
        {
            FB.LogOut();
        }
    }


    void HandleValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        // userName = args.Snapshot.Child(UID).Child("userName").GetValue(true).ToString();
    }
}