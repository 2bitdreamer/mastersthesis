using UnityEngine;
using System.Collections;

using Priority_Queue;
using System;
using System.Reflection;

public static class ExtensionMethods
{
    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }
}


public struct AttackSimulationResult
{
        public TileCoord m_coordinate;
        public int m_utility;
}

public enum TileType
{
    LAND,
    WATER,
    STONE,
    FOREST,
    ROAD,
    MOUNTAINS,
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
        m_movementCost = 1f;
        m_defense = 0;
    }

    public Color m_renderColor;
    public Color m_movementColor;
    public Color m_attackRangeColor;
    public Color m_attackShapeColor;
    public bool m_isSolid;
    public float m_movementCost;
    public int m_defense;
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

public enum GameState
{
    PLAYER_ONE_WINS_BANKRUPTCY,
    PLAYER_TWO_WINS_BANKRUPTCY,
    PLAYER_ONE_WINS_ANNIHILATION,
    PLAYER_TWO_WINS_ANNIHILATION,
    GAME_ONGOING,
    NUM_STATES
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
    SCOUT,
    SHOCKTROOPER,
    SNIPER,
    ARTILLERY,
    TANK,
    TEST
}

public class TileCoord
{
    public int x, y;
    public TileCoord(int xp, int yp)
    {
        x = xp;
        y = yp;
    }

    public TileCoord()
    {
        x = -1;
        y = -1;
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