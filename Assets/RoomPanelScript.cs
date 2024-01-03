using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
