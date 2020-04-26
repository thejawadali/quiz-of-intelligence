using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;

public class UserUtils : MonoBehaviour
{
  private FirebaseAuth auth;
  private FirebaseUser user;
  private DatabaseReference reference;


  public static string UID;
  public static string userName;
  private int totalPoints;
  // private DateTime lastPingTime;


  public static UserUtils instance = null;

  private void Awake()
  {
    if (instance == null)
    {
      instance = this;
    }
    DontDestroyOnLoad(gameObject);

    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
    {
      var dependencyStatus = task.Result;
      if (dependencyStatus == DependencyStatus.Available)
      {
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;

        // Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://fir-and-unity-testing.firebaseio.com/");
        // Get the root reference location of the database.
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        reference.Child("Users").Child(user.UserId).Child("userName").SetValueAsync(user.DisplayName);
        UID = user.UserId;
        UserStatus(true);
        FirebaseDatabase.DefaultInstance.GetReference("Users").ValueChanged += HandleValueChanged;
      }
    });
  }



  /// <summary>
  /// Check user's status, whether if he is online or offline
  /// </summary>
  /// <param name="isOnline"></param>
  public void UserStatus(bool isOnline)
  {
    reference.Child("Users").Child(UID).Child("availableForChallenge").SetValueAsync(isOnline);
  }

  private void OnApplicationFocus(bool focusStatus)
  {
    UserStatus(focusStatus);
  }

  private void OnApplicationQuit()
  {
    UserStatus(false);
  }


  public void UpdateUserProgress(int totalPoints, string timeTaken)
  {
    if (user != null)
    {
      reference.Child("Users").Child(UID).Child("totalPoints").SetValueAsync(totalPoints);
      reference.Child("Users").Child(UID).Child("timeTaken").SetValueAsync(timeTaken);
    }
  }


  // public string GetUserName(bool isRS)
  // {
  //   if (isRS)

  //     return userName;
  //   else
  //     return user.DisplayName;
  // }

  // public int GetTotalPoints()
  // {
  //   return totalPoints;
  // }


  void HandleValueChanged(object sender, ValueChangedEventArgs args)
  {
    if (args.DatabaseError != null)
    {
      Debug.LogError(args.DatabaseError.Message);
      return;
    }
    userName = args.Snapshot.Child(UID).Child("userName").GetValue(true).ToString();
    // lastPingTime = (DateTime)args.Snapshot.Child(UID).Child("lastPing").GetValue(true);
    // totalPoints = (int)args.Snapshot.Child(UID).Child("totalPoints").GetValue(true);
  }
}