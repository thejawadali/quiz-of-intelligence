using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using UnityEngine;

public class FetchOnlinePlayers : MonoBehaviour
{
  private FirebaseAuth auth;
  private FirebaseUser user;
  private DatabaseReference reference;
  private string UID;
  // Start is called before the first frame update
  void Start()
  {
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
    // UserStatus(true);
    FirebaseDatabase.DefaultInstance.GetReference("Users").ValueChanged += HandleValueChanged;
  }
});
  }

  // Update is called once per frame
  void Update()
  {

  }

  void GetOnlinePlayers(ValueChangedEventArgs args)
  {
    // var a = args.Snapshot.GetValue(true).where
  }
  void HandleValueChanged(object sender, ValueChangedEventArgs args)
  {
    if (args.DatabaseError != null)
    {
      Debug.LogError(args.DatabaseError.Message);
      return;
    }
  }
}
