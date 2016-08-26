using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
   public Vector3 m_resetCamera;
   public Vector3 m_origin;
   public Vector3 m_diference;
   public bool m_drag = false;
   public float m_defaultScale = 5f;
   public float m_gridSize;

    public float m_speed;
    public float m_scrollSpeed = 1500;

    void Start()
    {
        HexGrid hexGrid = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
        m_gridSize = (float)(Mathf.Max(hexGrid.gridHeightInHexes, hexGrid.gridWidthInHexes)) * hexGrid.TILE_SIZE;
        m_speed = Screen.width * .07f;
        //m_defaultScale = (hexGrid.m_hexHeight * hexGrid.gridHeightInHexes) / 2f;
    }

    void LateUpdate()
    {
        if (Input.GetMouseButton(2))
        {
            m_diference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            if (m_drag == false)
            {
                m_drag = true;
                m_origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else
        {
            m_drag = false;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(new Vector3(-m_speed * Time.smoothDeltaTime, 0, 0));
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(new Vector3(m_speed * Time.smoothDeltaTime, 0, 0));
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(new Vector3(0, m_speed * Time.smoothDeltaTime, 0));
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(new Vector3(0, -m_speed * Time.smoothDeltaTime, 0));
        }

        if (m_drag == true)
        {
            Camera.main.transform.position = m_origin - m_diference;
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            Camera.main.transform.position = m_resetCamera;
            Camera.main.orthographicSize = m_defaultScale;
        }

        float d = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(d) <= .01f)
            d = 0f;

        if(d != 0f)
        {
            Camera.main.orthographicSize -= (d * m_scrollSpeed * Time.smoothDeltaTime);
            if (Camera.main.orthographicSize < 1f)
                Camera.main.orthographicSize = 1f;
            if(Camera.main.orthographicSize > m_gridSize)
                Camera.main.orthographicSize = m_gridSize;
        }
    }
}
