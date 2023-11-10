using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions; // for ContinueWithOnMainThread
using System.Threading;

public class BookingBtnController : MonoBehaviour
{
    private GameObject parentObj;
    private string seatID;
    private string seatOccupant;
    private DatabaseReference reference;
    // Start is called before the first frame update
    void Start()
    {
        

    }
    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        parentObj = this.transform.parent.transform.parent.transform.parent.gameObject;
        seatID = parentObj.name;
        this.reference = FirebaseDatabase.DefaultInstance.GetReference("seats/" + seatID);

        reference
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
                      seatOccupant = (string)snapshot.Child("Occupant").GetValue(true);

                      Debug.Log(seatID + "<br>Occupant: " + seatOccupant);

                  }
              });
    }
}
