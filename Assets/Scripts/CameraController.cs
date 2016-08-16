using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private Vector3 m_resetCamera;
    private Vector3 m_origin;
    private Vector3 m_diference;
    private bool m_drag = false;
    private float m_defaultScale = 5f;
    private float m_gridSize;

    public float m_speed = 5;
    public float m_scrollSpeed = 150;

    void Start()
    {
        m_resetCamera = Camera.main.transform.position;
        HexGrid hexGrid = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
        m_gridSize = (float)(Mathf.Max(hexGrid.gridHeightInHexes, hexGrid.gridWidthInHexes)) * hexGrid.TILE_SIZE;
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
