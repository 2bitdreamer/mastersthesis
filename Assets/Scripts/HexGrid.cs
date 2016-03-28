using UnityEngine;
using System.Collections;
using System;

public struct HexTile {
    public HexTile(Vector2 pos)
    {
        m_worldCenterPos = pos;
        m_type = TileType.LAND;
    }

    public Vector2 m_worldCenterPos;
    //TODO: Properties
    public TileType m_type;
}

public enum TileType
{
    LAND,
    WATER,
    STONE
}

public struct TileCoord
{
    public int x, y;
    public TileCoord(int xp, int yp)
    {
        x = xp;
        y = yp;
    }

    public static bool operator == (TileCoord a, TileCoord b)
    {
        if (a.x == b.x && a.x == a.y)
            return true;
        return false;
    }

    public static bool operator !=(TileCoord a, TileCoord b)
    {
        if (a.x == b.x && a.x == a.y)
            return false;

        return true;
    }

    public static TileCoord operator +(TileCoord a, TileCoord b)
    {
        return new TileCoord(a.x + b.x, a.y + b.y);
    }

    public static TileCoord operator -(TileCoord a, TileCoord b)
    {
        return new TileCoord(a.x - b.x, a.y - b.y);
    }
}

public class HexGrid : MonoBehaviour
{
    public int xDimensions = 5;
    public int yDimensions = 5;
    public HexTile[,] m_grid;
    public float tileSize = 10f;

    Mesh mesh;
    Vector3[] vertices;
    int[] indices;
    Color[] colors;

    private float m_width;
    private float m_height;

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

    enum Direction
    {
        NORTHWEST,
        NORTHEAST,
        EAST,
        SOUTHEAST,
        SOUTHWEST,
        WEST
    }

    void Start()
    {
        mesh = new Mesh();
        m_grid = new HexTile[xDimensions, yDimensions];
        m_width = tileSize * Mathf.Sqrt(3f);
        m_height = tileSize * 2f;

        for (int x = 0; x < xDimensions; ++x)
        {
            for (int y = 0; y < yDimensions; ++y)
            {
                m_grid[x, y].m_worldCenterPos = new Vector2(Mathf.Sqrt(3f) * (x - .5f * (y & 1)), y * 3f / 2f) * tileSize;
            }
        }

        m_grid[0, 0].m_type = TileType.WATER;

        colors = new Color[xDimensions * yDimensions * VERTICES_PER_HEX];
        vertices = new Vector3[xDimensions * yDimensions * VERTICES_PER_HEX];
        indices = new int[xDimensions * yDimensions * INDICES_PER_HEX];
        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
    }

    Color GetColorFromTileType(TileType t)
    {
        switch (t)
        {
            case TileType.LAND:
                return Color.red;
            case TileType.WATER:
                return Color.blue;
            case TileType.STONE:
                return Color.gray;
            default:
                return Color.black;
        }
    }

    static Vector2 TransformForAngle(float angleDegrees)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angleDegrees), Mathf.Sin(Mathf.Deg2Rad * angleDegrees));
    }

    void MakeHexWithIndex(HexTile tile, int idx)
    {
        for (int i = 0; i < VERTICES_PER_HEX; ++i)
        {
            vertices[i + (VERTICES_PER_HEX * idx)] = (rotate_lookup[i] * tileSize) + tile.m_worldCenterPos;
            colors[i + (VERTICES_PER_HEX * idx)] = GetColorFromTileType(tile.m_type);
        }
        for (int i = 0; i < INDICES_PER_HEX; ++i)
        {
            indices[i + (INDICES_PER_HEX * idx)] = indices_lookup[i] + (VERTICES_PER_HEX * idx);
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

        //Debug.Log(clickPos);
        Debug.Log(new Vector2(coord.x, coord.y));
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

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.colors = colors;

        mesh.RecalculateBounds();
    }
}
   
