using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {
    public TileCoord m_position;
    public int m_movement;
    public char m_displayCharacter;
    public Font m_font;
    public Material m_material;
    public TextMesh m_textMesh;

    private HexGrid m_hexGridRef;


    public void Initialize(TileCoord pos, int moves, char displayChar)
    {
        m_position = pos;
        m_movement = moves;
        m_displayCharacter = displayChar;
        m_hexGridRef = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();

        m_textMesh.characterSize = 1f;
        m_textMesh.font = m_font;
        m_textMesh.text = m_displayCharacter.ToString();
        m_textMesh.color = Color.green;

        transform.position = m_hexGridRef.m_grid[m_position.x, m_position.y].m_worldCenterPos;
        transform.position += new Vector3(-m_textMesh.characterSize / 2f, m_textMesh.characterSize / 2f, 0f);

        // MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        //meshRenderer.material = m_material;
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () { 
	}
}
