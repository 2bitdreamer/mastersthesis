using UnityEngine;
using System.Collections.Generic;
using Vectrosity;

public class HexGrid : MonoBehaviour
{
    public int xDimensions = 5;
    public VectorLine m_outline;
    public int yDimensions = 5;

    public HexTile[,] m_grid;
    public float tileSize = 10f;

    Mesh m_hexMesh;

    Vector3[] m_vertices;
    int[] m_indices;
    Color[] m_colors;
    List<Vertex> highlightVertices = new List<Vertex>();

    private float m_width;
    private float m_height;
    private Dictionary<TileType, TileDefinition> m_tileDefinitions;

    private const int VERTICES_PER_HEX = 7;
    private const int INDICES_PER_HEX = 18;

    private static readonly Vector2[] rotate_lookup = new Vector2[VERTICES_PER_HEX] {
      TransformForAngle(-30), TransformForAngle(-90), TransformForAngle(-150),
      TransformForAngle(-210), TransformForAngle(-270), TransformForAngle(-330),
      new Vector2(0,0)
    };

    private static readonly int[] indices_lookup = new int[INDICES_PER_HEX] {
      6,0,1,
      6,1,2,
      6,2,3,
      6,3,4,
      6,4,5,
      6,5,0
    };

    void Start()
    {
        m_hexMesh = new Mesh();
        m_grid = new HexTile[xDimensions, yDimensions];
        m_width = tileSize * Mathf.Sqrt(3f);
        m_height = tileSize * 2f;

        m_colors = new Color[xDimensions * yDimensions * VERTICES_PER_HEX];
        m_vertices = new Vector3[xDimensions * yDimensions * VERTICES_PER_HEX];
        m_indices = new int[xDimensions * yDimensions * INDICES_PER_HEX];

        for (int x = 0; x < xDimensions; ++x)
        {
            for (int y = 0; y < yDimensions; ++y)
            {
                int isOdd = (y & 1);
                m_grid[x, y].m_worldCenterPos = new Vector2(Mathf.Sqrt(3f) * (x - .5f * isOdd), y * 3f / 2f) * tileSize;
            }
        }

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = m_hexMesh;

       m_outline = new VectorLine("Hex Outline", new List<Vector3>(), 4f, LineType.Continuous);

        m_tileDefinitions = new Dictionary<TileType, TileDefinition>();

        TileDefinition waterTile = new TileDefinition();
        waterTile.m_renderColor = Color.blue;
        waterTile.m_isSolid = false;
        m_tileDefinitions.Add(TileType.WATER, waterTile);

        TileDefinition stoneTile = new TileDefinition();
        stoneTile.m_renderColor = Color.gray;
        stoneTile.m_isSolid = true;
        m_tileDefinitions.Add(TileType.STONE, stoneTile);

        TileDefinition landTile = new TileDefinition();
        landTile.m_isSolid = false;
        landTile.m_renderColor = Color.red;
        m_tileDefinitions.Add(TileType.LAND, landTile);

        //m_outline.rectTransform.position = transform.position;

    }

    Color GetColorFromTileType(TileType t)
    {
        TileDefinition td = m_tileDefinitions[t];
        return td.m_renderColor;
    }

    static Vector2 TransformForAngle(float angleDegrees)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angleDegrees), Mathf.Sin(Mathf.Deg2Rad * angleDegrees));
    }

    void DrawOutlineHex(Vector2 hexWorldCenterPos, float lineWidth, float offset)
    {
        List<Vector3> outlineVertexList = new List<Vector3>();
        Color outlineColor = Color.red;

        Vector2 halfWidth = new Vector2(((xDimensions / 2f) * m_width) - (m_width / 2f), ((yDimensions / 2f) * m_height) - m_height );

        for (int i = 0; i < VERTICES_PER_HEX; ++i)
        {
            if (i == (VERTICES_PER_HEX - 1))
            {
                outlineVertexList.Add((rotate_lookup[0] * tileSize) - halfWidth + hexWorldCenterPos);
            }
            else
                outlineVertexList.Add((rotate_lookup[i] * tileSize) - halfWidth + hexWorldCenterPos);
        }

        m_outline.points3 = outlineVertexList;
        m_outline.Draw();
    }

    void MakeHexWithIndex(HexTile tile, int idx)
    {
        for (int i = 0; i < VERTICES_PER_HEX; ++i)
        {
            m_vertices[i + (VERTICES_PER_HEX * idx)] = (rotate_lookup[i] * tileSize) + tile.m_worldCenterPos;
            m_colors[i + (VERTICES_PER_HEX * idx)] = GetColorFromTileType(tile.m_type);
        }
        for (int i = 0; i < INDICES_PER_HEX; ++i)
        {
            m_indices[i + (INDICES_PER_HEX * idx)] = indices_lookup[i] + (VERTICES_PER_HEX * idx);
        }
    }

    Vector3 CubeCoordinatesForTile(TileCoord tc)
    {
        float x = (tc.x * Mathf.Sqrt(3f) / 3f - tc.y / 3f) / tileSize;
        float z = tc.y * 2f / 3f / tileSize;
        float y = -x - z;
        return new Vector3(x, y, z);
    }

    Vector3 CubeCoordinatesFromMousePosition(Vector2 worldPos)
    {
        float x = (worldPos.x * Mathf.Sqrt(3f) / 3f - worldPos.y / 3f) / tileSize;
        float z = worldPos.y * 2f / 3f / tileSize;
        float y = -x - z;
        return new Vector3(x, y, z);
    }

    TileCoord TileCoordFromCubeCoordinate(int q, int r, int s)
    {
        return new TileCoord(q + (s + (s & 1)) / 2, s);
    }

    Vector3 CubeOffsetVectorForDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.NORTHEAST:
                return new Vector3(1f, 0f, -1f);
            case Direction.NORTHWEST:
                return new Vector3(0f, 1f, -1f);
            case Direction.WEST:
                return new Vector3(-1f, 1f, 0f);
            case Direction.SOUTHWEST:
                return new Vector3(-1f, 0f, 1f);
            case Direction.SOUTHEAST:
                return new Vector3(0f, -1f, 1f);
            default:
                return new Vector3(0f, 0f, 0f);
        }
    }

    TileCoord GetTileCoordinateFromWorldPosition(Vector2 worldPos)
    {
        Vector3 cubeCoordinates = CubeCoordinatesFromMousePosition(worldPos);

        float x = cubeCoordinates.x;
        float y = cubeCoordinates.y;
        float z = cubeCoordinates.z;

        int rx = (int)Mathf.Round(x);
        int ry = (int)Mathf.Round(y);
        int rz = (int)Mathf.Round(z);

        float xDiff = Mathf.Abs(rx - x);
        float yDiff = Mathf.Abs(ry - y);
        float zDiff = Mathf.Abs(rz - z);

        if (xDiff > yDiff && xDiff > zDiff)
            rx = -ry - rz;
        else if (yDiff > zDiff)
            ry = -rx - rz;
        else
            rz = -rx - ry;

        Debug.Log(new Vector3(rx, ry, rz));

        return TileCoordFromCubeCoordinate(rx, ry, rz);
    }

    void OnMouseDown()
    {
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickPos -= transform.position;
        TileCoord coord = GetTileCoordinateFromWorldPosition(clickPos);

        if (coord.x >= 0 && coord.y >= 0 && coord.x < xDimensions && coord.y < yDimensions)
            m_grid[coord.x, coord.y].m_type = TileType.STONE;

        Debug.Log("GridPos: ");
        Debug.Log(m_grid[coord.x, coord.y].m_worldCenterPos);
        Debug.Log("ClickPos: ");
        Debug.Log(clickPos);

        DrawOutlineHex(m_grid[coord.x, coord.y].m_worldCenterPos, 2f, 1f);
        //Debug.Log(clickPos);
        //Debug.Log(new Vector2(coord.x, coord.y));
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
            OnMouseDown();

        for (int x = 0; x < xDimensions; ++x)
        {
            for (int y = 0; y < yDimensions; ++y)
            {
                HexTile ht = m_grid[x, y];
               MakeHexWithIndex(ht, (xDimensions * y) + x);
            }
        }

        m_hexMesh.vertices = m_vertices;
        m_hexMesh.triangles = m_indices;
        m_hexMesh.colors = m_colors;

        m_hexMesh.RecalculateBounds();
    }
}
   
