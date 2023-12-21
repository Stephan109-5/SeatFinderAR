using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartDirector : MonoBehaviour
{
    private string scene_name;
    public TMP_Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
        scene_name = dropdown.options[dropdown.value].text;
    }

    // Update is called once per frame
/*    void Update()
    {
        
    }*/

    public void OnClickStart()
    {
        SceneManager.LoadScene(scene_name);
    }

    public void DropdownItemSelection(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;
        scene_name = dropdown.options[index].text;
    }
}
