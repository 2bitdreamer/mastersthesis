using UnityEngine;
using System.Collections;

public class Pan : MonoBehaviour
{
    public Vector3 ResetCamera;
    public Vector3 Origin;
    public Vector3 Diference;
    public bool Drag = false;

    void Start()
    {
        ResetCamera = Camera.main.transform.position;
    }
    
    void LateUpdate()
    {
        if (Input.GetMouseButton(2))
        {
            Diference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            if (Drag == false)
            {
                Drag = true;
                Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else
        {
            Drag = false;
        }
        if (Drag == true)
        {
            Camera.main.transform.position = Origin - Diference;
        }
        //RESET CAMERA TO STARTING POSITION WITH RIGHT CLICK
        if (Input.GetKeyUp(KeyCode.R))
        {
            Camera.main.transform.position = ResetCamera;
        }
    }
}
