using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ResourceType
{
    MINE,
    FACTORY
}

public class ResourceData
{
    public int m_pointsToOwn;
    public int m_operatingBonus;
    public int m_owningBonus;
    public bool m_allowsConstruction;
    public Color m_resourceColor;
    public Shape m_resourceShape;
}


public class HexResource : MonoBehaviour {
    public static Dictionary<ResourceType, ResourceData> s_resourceMap;
    public ResourceType m_resourceType;

    public int m_pointsToOwn; // How many points a team needs to own this resource
    public int m_pointsTowardsOwning = 0; // How many points a team currently has towards owning this resource

    public int m_owningTeam = -1; // Who owns this resource
    public int m_teamWorkingTowardsOwning = -1; //Who is currently working towards owning this resource
    public int m_owningBonus; //Gold income granted for owning this resource
    public int m_operatingBonus; // Gold granted for operating this resource

    public Color m_baseColor; //Color of resource
    public HexTile m_tile;

    Vector3[] m_vertices;
    int[] m_indices;
    Color[] m_colors;
    Mesh m_mesh;

    // Use this for initialization
    void Awake () {
        //m_textMesh = GetComponent<TextMesh>();

    }
	
	// Update is called once per frame
	void Update () {
	}

    public void Initialize(HexTile tile, ResourceType type)
    {
        ResourceData rd = s_resourceMap[type];
        m_resourceType = type;

        m_pointsToOwn = rd.m_pointsToOwn;
        m_owningBonus = rd.m_owningBonus;
        m_operatingBonus = rd.m_operatingBonus;
        m_tile = tile;
        m_baseColor = rd.m_resourceColor;

        m_mesh = new Mesh();
        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = m_mesh;

        SolveForDrawProperties(tile.m_worldCenterPos, m_baseColor, rd.m_resourceShape);

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
        Shape shape = s_resourceMap[m_resourceType].m_resourceShape;
        int numVerts = 4;
        if (shape == Shape.TRIANGLE)
            numVerts = 3;
        for (int i = 0; i < numVerts; i++)
            m_colors[i] = c;
        m_mesh.colors = m_colors;
        m_mesh.RecalculateBounds();
    }
}
