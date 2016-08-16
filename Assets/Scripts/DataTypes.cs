using UnityEngine;
using System.Collections;

using Priority_Queue;

public enum TileType
{
    LAND,
    WATER,
    STONE,
    NUM_TYPES
}

public class TileDefinition
{
    public TileDefinition()
    {
        m_renderColor = Color.black;
        m_movementColor = m_renderColor;
        m_movementColor.a = .5f;
        m_attackRangeColor = Color.gray;
        m_attackShapeColor = Color.yellow;
        m_isSolid = false;
        m_movementCost = 1;
    }

    public Color m_renderColor;
    public Color m_movementColor;
    public Color m_attackRangeColor;
    public Color m_attackShapeColor;
    public bool m_isSolid;
    public int m_movementCost;
}

public class TeamDefinition
{
    public TeamDefinition()
    {
        m_name = "team";
        m_color = Color.blue;
        m_number = 999;
    }
    public Color m_color;
    public string m_name;
    public int m_number;
}

public struct Vertex
{
    public Vector3 m_position;
    public Color m_color;
    public int m_index;
}

public class HexTile : FastPriorityQueueNode
{
    public double GetDistance()
    {
        return -1 * (Priority - double.MaxValue);
    }

    public HexTile(TileCoord coord, Vector2 pos, HexResource resource = null)
    {
        m_worldCenterPos = pos;
        m_type = TileType.LAND;
        m_unit = null;
        m_resource = resource;
        m_tileCoord = coord;
    }

    public Vector2 m_worldCenterPos;
    public TileCoord m_tileCoord;
    //TODO: Properties
    public TileType m_type;
    public Unit m_unit;
    public HexResource m_resource = null;
    public ColorType m_currentColor = ColorType.RENDER;
}

public class GridNode : FastPriorityQueueNode
{
    public HexTile m_hexTile { get; private set; }
    public GridNode(HexTile tile)
    {
        m_hexTile = tile;
    }
}

public enum ColorType
{
    RENDER,
    MOVEMENT,
    ATTACKRANGE,
    ATTACKSHAPE
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

public enum Shape
{
    TRIANGLE,
    SQUARE
}

//The exact sub class of the unit
public enum UnitIdentity
{
    ARTILLERY,
    RUINER,
    MINELAYER,
    SNIPER,
    SHOCKTROOPER,
    ENGINEER,
    SCOUT,
    DESTROYER
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
        if (a.x == b.x && a.y == b.y)
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