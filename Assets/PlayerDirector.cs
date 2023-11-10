using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDirector : MonoBehaviour
{
    // Start is called before the first frame update
    public string username;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetName(string n) {
        username = n;
        /*Debug.Log(username);*/
    }

    public string GetName() { 
        return username;
    }
}
