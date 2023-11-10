using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions; // for ContinueWithOnMainThread
using System.Threading;

public class TextController : MonoBehaviour
{
    public TextMeshPro text;
    private GameObject parentObj;
    private float timer;
    private string seatID;
    private string seatOccupant;
    private DatabaseReference reference;
    public GameObject userName;
    public TextMeshProUGUI buttonText;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        parentObj = this.transform.parent.transform.parent.gameObject;
        seatID = parentObj.name;
        text.SetText(seatID + "<br>Occupant:<br>Fetching...");
        GetSeatOccupant();
    }

    // Update is called once per frame
    void Update()
    {

        if (timer < 2)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0;
            GetSeatOccupant();
        }
    }

    private void GetSeatOccupant()
    {
        this.reference = FirebaseDatabase.DefaultInstance.GetReference("seats/" + seatID);

        this.reference
              .GetValueAsync().ContinueWithOnMainThread(task =>
              {
                  if (task.IsFaulted)
                  {
                      // Handle the error...
                      Debug.Log("Fault");
                  }
                  else if (task.IsCompleted)
                  {
                      DataSnapshot snapshot = task.Result;
                      seatOccupant = (string) snapshot.Child("Occupant").GetValue(true);

                      text.SetText(seatID + "<br>Occupant:<br>" + seatOccupant);

                      PlayerDirector name = (PlayerDirector)userName.GetComponent("PlayerDirector");
                      if (name.username != "" && seatOccupant == name.username)
                      {
                          buttonText.SetText("Unbook");
                      }
                      else
                      {
                          buttonText.SetText("Book");
                      }

                  }
              });
    }

    //Function for Book/Reserve button
    public void OnClick()
    {
        PlayerDirector name = (PlayerDirector)userName.GetComponent("PlayerDirector");
        reference = FirebaseDatabase.DefaultInstance.GetReference("seats/" + seatID);
        /*Debug.Log(name.username);*/

        if (name.username != null || name.username != "") {

            if (seatOccupant == name.username && buttonText.text == "Unbook")
            {
                reference.Child("Occupant").SetValueAsync("");
                buttonText.SetText("Book");
            }
            else if (seatOccupant == "")
            {
                reference.Child("Occupant").SetValueAsync(name.username);
            }
            
            GetSeatOccupant();
        }
        
            
    }
}
