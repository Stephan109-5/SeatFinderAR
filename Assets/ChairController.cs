using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ChairController : MonoBehaviour
{
    GameObject MenuPanel;
/*    bool isShow;*/
/*    private Camera camera;*/

    void Start()
    {
/*        this.isShow = false;*/

        MenuPanel = GameObject.Find("InfoPanel");
        MenuPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
/*        DetectObjectWithRaycast();*/

        /*        if (Input.GetMouseButtonDown(0))
                {
                    this.isShow = !this.isShow;
                    MenuPanel.SetActive(isShow);

                }*/
    }

/*    public void DetectObjectWithRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform != null)
                {
                    //Our custom method. 
                    Debug.Log(ray);
                    CurrentClickedGameObject(raycastHit.transform.gameObject);
                }
            }
        }
    }

    public void CurrentClickedGameObject(GameObject gameObject)
    {
        this.isShow = !this.isShow;
        MenuPanel.SetActive(isShow);
    }*/
}
