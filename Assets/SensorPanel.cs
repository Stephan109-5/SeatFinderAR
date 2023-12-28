using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions; // for ContinueWithOnMainThread
using System.Threading;
using SeatFinder;

public class SensorPanel : MonoBehaviour
{
    public GameObject lightValue;
    public GameObject temperatureValue;
    public GameObject humidityValue;
    public GameObject noiseValue;

    private Dictionary<string, TextMeshProUGUI> measurementToTextObj;

    public FirebaseSensor Sensor;

    // Start is called before the first frame update
    void Start()
    {
        measurementToTextObj = new Dictionary<string, TextMeshProUGUI>
        {
            { "temperature", temperatureValue.GetComponent<TextMeshProUGUI>() },
            { "humidity", humidityValue.GetComponent<TextMeshProUGUI>() },
            { "light", lightValue.GetComponent<TextMeshProUGUI>() },
            { "noise", noiseValue.GetComponent<TextMeshProUGUI>() }
        };
    }

    public void updateSensorValues(Dictionary<string, int> measurements)
    {
        foreach (var measurement in measurements)
        {
            if (measurementToTextObj.ContainsKey(measurement.Key) && measurement.Value != Int32.MinValue)
            {
                measurementToTextObj[measurement.Key].text = measurement.Value.ToString();
            }
        }
    }
    
    public void updateSensorPosition(Vector3 position)
    {
        this.transform.position = position;
    }
}