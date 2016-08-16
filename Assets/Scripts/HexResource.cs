using UnityEngine;
using System.Collections;

public class HexResource : MonoBehaviour {
    public int m_pointsToOwn;
    public int m_pointsTowardsOwning = 0;
    public int m_owningTeam = -1;
    public int m_teamWorkingTowardsOwning = -1;
    public int m_owningBonus;
    public int m_operatingBonus;
    public Color m_baseColor;
    public HexTile m_tile;

    Vector3[] m_vertices;
    int[] m_indices;
    Color[] m_colors;
    Mesh m_mesh;

    // Use this for initialization
    void Start () {
        //m_textMesh = GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void Initialize(Vector3 hexCenter, Color color, Shape shape, int pointsToOwn, int owningBonus, int operatingBonus, HexTile tile)
    {
        m_pointsToOwn = pointsToOwn;
        m_owningBonus = owningBonus;
        m_operatingBonus = operatingBonus;
        m_tile = tile;
        m_baseColor = color;

        m_mesh = new Mesh();
        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = m_mesh;

        SolveForDrawProperties(hexCenter, color, shape);

        m_mesh.vertices = m_vertices;
        m_mesh.triangles = m_indices;
        m_mesh.colors = m_colors;

        m_mesh.RecalculateBounds();
    }

    public void SolveForDrawProperties(Vector3 hexCenter, Color color, Shape shape)
    {
        HexGrid hexGrid = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
        float hexSize = hexGrid.TILE_SIZE;

        float halfSize = hexSize / 2f;
        switch (shape)
        {
            case Shape.SQUARE:
                m_vertices = new Vector3[4];
                m_vertices[0] = new Vector3(hexCenter.x - halfSize, hexCenter.y - halfSize, 0);
                m_vertices[1] = new Vector3(hexCenter.x - halfSize, hexCenter.y + halfSize, 0);
                m_vertices[2] = new Vector3(hexCenter.x + halfSize, hexCenter.y + halfSize, 0);
                m_vertices[3] = new Vector3(hexCenter.x + halfSize, hexCenter.y - halfSize, 0);

                m_colors = new Color[4];
                for(int i = 0; i < 4; i++)
                    m_colors[i] = color;

                m_indices = new int[6] {
                    0, 1, 2,
                    2, 3, 0 };
                break;


            case Shape.TRIANGLE:
                m_vertices = new Vector3[3];
                m_vertices[0] = new Vector3(hexCenter.x - halfSize, hexCenter.y - halfSize, 0);
                m_vertices[1] = new Vector3(hexCenter.x, hexCenter.y + halfSize, 0);
                m_vertices[2] = new Vector3(hexCenter.x + halfSize, hexCenter.y - halfSize, 0);

                m_colors = new Color[3];
                for (int i = 0; i < 3; i++)
                    m_colors[i] = color;

                m_indices = new int[3] {
                    0, 1, 2};
                break;
        }

    }

    public void Recolor(Color c)
    {
        for (int i = 0; i < 4; i++)
            m_colors[i] = c;
        m_mesh.colors = m_colors;
        m_mesh.RecalculateBounds();
    }
}
