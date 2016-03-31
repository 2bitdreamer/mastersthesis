using UnityEngine;
using System.Collections;

public enum TileType
{
    LAND,
    WATER,
    STONE,
    NUM_TYPES
}

public struct TileDefinition
{
    public Color m_renderColor;
    public bool m_isSolid;
}

public struct Vertex
{
    public Vector3 m_position;
    public Color m_color;
    public int m_index;
}

public struct HexTile
{
    public HexTile(Vector2 pos)
    {
        m_worldCenterPos = pos;
        m_type = TileType.LAND;
    }

    public Vector2 m_worldCenterPos;
    //TODO: Properties
    public TileType m_type;
}

enum Direction
{
    NORTHWEST,
    NORTHEAST,
    EAST,
    SOUTHEAST,
    SOUTHWEST,
    WEST
}

public struct TileCoord
{
    public int x, y;
    public TileCoord(int xp, int yp)
    {
        x = xp;
        y = yp;
    }

    public static bool operator ==(TileCoord a, TileCoord b)
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