using UnityEngine;

public class Director : MonoBehaviour
{
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

        DetectObjectWithRaycast();
    }

    public void DetectObjectWithRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {

            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.transform != null)
                {
                    //Our custom method. 
                    /*Debug.Log(raycastHit.transform.name);*/

                    CurrentClickedGameObject(raycastHit.transform.gameObject);
                }
            }
        }
    }

    public void CurrentClickedGameObject(GameObject gameObject)
    {
        Transform trans = gameObject.transform;
        Transform childTrans = trans.Find("InfoPanel");

/*        Debug.Log(childTrans.name);*/

        if (childTrans != null)
        {
            GameObject MenuPanel = childTrans.gameObject;
            MenuPanel.SetActive(!MenuPanel.activeSelf);

        }

        
    }
}
