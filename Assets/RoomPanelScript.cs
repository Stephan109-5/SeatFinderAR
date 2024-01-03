using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class RoomPanelScript : MonoBehaviour
{
    private string RoomName;
    private int SeatOccupancy;
    private TextMeshProUGUI RoomNameLabel;

    // Start is called before the first frame update
    void Start()
    {
        RoomName = SceneManager.GetActiveScene().name;
        RoomNameLabel = GameObject.Find("RoomName").GetComponent<TextMeshProUGUI>();
        RoomNameLabel.text = RoomName;
        /*GetRoomOccupancy();*/
        
        FirebaseDatabase.DefaultInstance.GetReference("rooms/" + RoomName + "/seats").ValueChanged += GetRoomOccupancy;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GetRoomOccupancy(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        DataSnapshot snapshot = args.Snapshot;
        // Do something with snapshot...

        Dictionary<string, object> data = (Dictionary<string, object>)snapshot.Value;
        int occupants = 0;
        int totalSeats = 0;
        foreach (var seats in data)
        {
            /*Debug.Log($"{seats.Key}: {seats.Value}");
            Debug.Log($"{seat_data["occupant"]}");*/
            Dictionary<string, object> seat_data = (Dictionary<string, object>)seats.Value;
                    
            totalSeats++;

            if (!string.IsNullOrEmpty((string) seat_data["occupant"]))
            {
                occupants++;
            }

        }

        Debug.Log("Number of seat available: " +  occupants + "/" + totalSeats);

    }
}
