using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusCheck : MonoBehaviour
{
    private TextMeshProUGUI statusText;
    private Firebase.FirebaseApp app;
    // Start is called before the first frame update
    void Awake()
    {
        statusText = GameObject.Find("Status").GetComponent<TextMeshProUGUI>();
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                statusText.text = "Firebase: Online";
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
                statusText.text = "Firebase: Error";
            }
        });

    }

}
