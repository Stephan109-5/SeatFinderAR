using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SeatFinder;

public class SensorPanel : MonoBehaviour
{
    public GameObject lightValue;
    public GameObject temperatureValue;
    public GameObject humidityValue;
    public GameObject noiseValue;
    public GameObject panelTitle;

    private String _sensorName = "";
    private Dictionary<string, TextMeshProUGUI> measurementToTextObj;
    
    private Camera _mainCamera;

    public FirebaseSensor Sensor;

    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        measurementToTextObj = new Dictionary<string, TextMeshProUGUI>
        {
            { "temperature", temperatureValue.GetComponent<TextMeshProUGUI>() },
            { "humidity", humidityValue.GetComponent<TextMeshProUGUI>() },
            { "light", lightValue.GetComponent<TextMeshProUGUI>() },
            { "noise", noiseValue.GetComponent<TextMeshProUGUI>() }
        };
        
        _mainCamera = Camera.main;

        foreach (var entry in measurementToTextObj)
        {
            entry.Value.text = "";
        }
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

    private void Update()
    {
        transform.forward = _mainCamera.transform.forward;
    }

    public void updateSensorPosition(Vector3 position)
    {
        this.transform.position = position;
    }
    
    public void updateName(String roomId, String sensorId)
    {
        this._sensorName = "Sensor #" + sensorId;
        this.panelTitle.GetComponent<TextMeshProUGUI>().text = this._sensorName;
    }
}