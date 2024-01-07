using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeatFinder
{
    public class FirebaseSensor
    {
        private DatabaseReference reference_;
        public String RoomId;
        public String SensorId;
        public Vector3 SensorPosition;
        public Dictionary<string, int> Measurements;

        private SensorPanel _sensorPanel;
        
        public FirebaseSensor(String roomId, String sensorId, SensorPanel panel)
        {
            _sensorPanel = panel;
            SensorPosition = new Vector3(0, 0, 0);
            
            _sensorPanel.updateName(roomId, sensorId);
            Measurements = new Dictionary<string, int>
            {
                { "temperature", Int32.MinValue },
                { "humidity", Int32.MinValue },
                { "light", Int32.MinValue },
                { "noise", Int32.MinValue }

            };
            Debug.Log("Initializing sensor " + sensorId + " in room " + roomId);

            this.RoomId = SceneManager.GetActiveScene().name;
            this.SensorId = sensorId;
            this.reference_ = FirebaseDatabase.DefaultInstance
                .GetReference("rooms/" + roomId + "/sensors/" + sensorId);
            
            this.reference_.Child("position").GetValueAsync().ContinueWithOnMainThread(task =>
                {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error getting sensor position");
                    return;
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    SensorPosition = new Vector3(
                        float.Parse(snapshot.Child("x").Value.ToString()),
                        float.Parse(snapshot.Child("y").Value.ToString()),
                        float.Parse(snapshot.Child("z").Value.ToString())
                    );
                    // Debug.Log(snapshot.Child("x"));
                    // Debug.Log(snapshot.Child("y"));
                    // Debug.Log(snapshot.Child("z"));
                    this._sensorPanel.updateSensorPosition(SensorPosition);
                }
            });
            this.reference_.Child("measurements").ValueChanged += HandleSensorUpdate;
        }
        
        private void HandleSensorUpdate(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null) {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }
            
            foreach (var sensor in args.Snapshot.Children)
            {
                // Debug.Log(sensor.Value);
                // Debug.Log(sensor.Key);
                Measurements[sensor.Key] = Convert.ToInt32(sensor.Value);
            }

            this._sensorPanel.updateSensorValues(Measurements);
        }

    }
}