using UnityEngine;
using Priority_Queue;
using System.Collections.Generic;
using Vectrosity;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    public int gridWidthInHexes = 10;
    public VectorLine[,] m_outlines;
    public VectorLine m_selectedOutline;
    TileCoord m_selectedTileCoords;

    public int gridHeightInHexes = 5;
    public GameObject m_unitPref;
    public GameObject m_resourcePref;

    public HexTile[,] m_grid;
    public float TILE_SIZE = 10f;

    Unit m_unitAttemptingAction = null;
    Unit m_unitAttemptingAttack = null;
    TileCoord m_previousTileAttackShapeWasDrawnOn = new TileCoord(-1, -1);

    public GameObject m_actionMenu;
    public GameObject m_specialText;

    public List<Team> m_teams;
    private List<HexResource> m_hexResources;

    private Pathfinding m_pathfinding;

    Mesh m_hexMesh;

    Vector3[] m_vertices;
    int[] m_indices;
    Color[] m_colors;

    private float m_hexWidth;
    private float m_hexHeight;
    private Dictionary<TileType, TileDefinition> m_tileDefinitions;
    private int m_curTeam = 0;
    private int m_round = 0;

    private Text m_UIText;

    private const int VERTICES_PER_HEX = 7;
    private const int INDICES_PER_HEX = 18;



    private static readonly int[] indices_lookup = new int[INDICES_PER_HEX] {
      6,0,1,
      6,1,2,
      6,2,3,
      6,3,4,
      6,4,5,
      6,5,0
    };


    private static readonly TileCoord[,] directions = new TileCoord[2, 6] {
   { new TileCoord(+1, 0), new TileCoord(+1, -1), new TileCoord(0, -1),
     new TileCoord(-1, 0), new TileCoord(0, +1), new TileCoord(+1, +1) },
   { new TileCoord(+1, 0), new TileCoord(0, -1), new TileCoord(-1, -1),
     new TileCoord(-1, 0), new TileCoord(-1, +1), new TileCoord(0, +1)}
    };

    private static readonly TileCoord[] s_neighborDirections = new TileCoord[6] {
        new TileCoord(1, 0),
        new TileCoord(0, 1),
        new TileCoord(-1, 1),
        new TileCoord(-1, 0),
        new TileCoord(0, -1),
        new TileCoord(1, -1)
    };

    private static readonly Vector2[] s_neighborDisplacements = new Vector2[6] {
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 0f),   Mathf.Sin(Mathf.Deg2Rad * 0f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 60f),  Mathf.Sin(Mathf.Deg2Rad * 60f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 120f), Mathf.Sin(Mathf.Deg2Rad * 120f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 180f), Mathf.Sin(Mathf.Deg2Rad * 180f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 240f), Mathf.Sin(Mathf.Deg2Rad * 240f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * 300f), Mathf.Sin(Mathf.Deg2Rad * 300f))
    };

    private static readonly Vector2[] rotate_lookup = new Vector2[VERTICES_PER_HEX] {
      TransformForAngle(-30), TransformForAngle(-90), TransformForAngle(-150),
      TransformForAngle(-210), TransformForAngle(-270), TransformForAngle(-330),
      new Vector2(0,0)
    };

    private static readonly Vector2[] s_vertexDisplacements = new Vector2[VERTICES_PER_HEX] {
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * -30f),  Mathf.Sin(Mathf.Deg2Rad * -30f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * -90f),  Mathf.Sin(Mathf.Deg2Rad * -90f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * -150f), Mathf.Sin(Mathf.Deg2Rad * -150f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * -210f), Mathf.Sin(Mathf.Deg2Rad * -210f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * -270f), Mathf.Sin(Mathf.Deg2Rad * -270f)),
        new Vector2(Mathf.Cos(Mathf.Deg2Rad * -330f), Mathf.Sin(Mathf.Deg2Rad * -330f)),
        new Vector2(0,0)
    };



    void Start()
    {
        m_actionMenu = GameObject.FindGameObjectWithTag("ActionMenu");
        m_specialText = GameObject.FindGameObjectWithTag("SpecialText");
        m_actionMenu.SetActive(false);
        m_selectedTileCoords = new TileCoord(-1, -1);
        m_hexMesh = new Mesh();
        m_grid = new HexTile[gridWidthInHexes, gridHeightInHexes];
        m_outlines = new VectorLine[gridWidthInHexes, gridHeightInHexes];

        m_selectedOutline = new VectorLine("Hex Outline", new List<Vector3>(), 8f, LineType.Continuous);
        m_selectedOutline.color = new Color(.45f, .114f, .87f);

        m_hexWidth = TILE_SIZE * Mathf.Sqrt(3f);
        m_hexHeight = TILE_SIZE * 2f;

        m_colors = new Color[gridWidthInHexes * gridHeightInHexes * VERTICES_PER_HEX];
        m_vertices = new Vector3[gridWidthInHexes * gridHeightInHexes * VERTICES_PER_HEX];
        m_indices = new int[gridWidthInHexes * gridHeightInHexes * INDICES_PER_HEX];

        GameObject HUD = GameObject.FindGameObjectWithTag("HUD");
        m_UIText = HUD.GetComponentInChildren<Text>();


        m_teams = new List<Team>();
        m_hexResources = new List<HexResource>();

        Vector2 cameraOffset = new Vector2(((gridWidthInHexes / 2f) * m_hexWidth) - (m_hexWidth / 2f), ((gridHeightInHexes / 2f) * m_hexHeight) - m_hexHeight);
        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera") as GameObject;
        Camera c = camera.GetComponent<Camera>() as Camera;
        // VectorLine.SetCamera3D(camera);
        c.transform.position = new Vector3(cameraOffset.x, cameraOffset.y, -10f);

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = m_hexMesh;

        //point cloud -- biggest/smallest x and biggest/smallest y used to frame camera
        for (int x = 0; x < gridWidthInHexes; ++x)
        {
            for (int y = 0; y < gridHeightInHexes; ++y)
            {
                //int isOdd = (y & 1);
                // Vector2 pos = new Vector2(Mathf.Sqrt(3f) * (x - .5f * isOdd), y * 3f / 2f) * tileSize;
                Vector2 pos = CalculateHexCenter(x, y);


                m_grid[x, y] = new HexTile(new TileCoord(x, y), pos);
                VectorLine newVL = new VectorLine("Hex Outline" + x + "," + y, new List<Vector3>(), 5f, LineType.Continuous);
                //Hex Outline Color
                //newVL.color = new Color(.804f, .918f, .918f);
                newVL.color = Color.black;
                m_outlines[x, y] = newVL;
                GenerateHexOutlines(m_grid[x, y].m_worldCenterPos, 1f, new TileCoord(x, y));

            }
        }

        Color goldResourceColor = new Color(.96f, .69f, .26f);
        GenerateResource(m_grid[0, 0], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[1, 1], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[2, 2], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[3, 3], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[4, 4], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[5, 5], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[6, 6], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[7, 7], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[8, 8], goldResourceColor, Shape.SQUARE, 3, 2, 1);
        GenerateResource(m_grid[9, 9], goldResourceColor, Shape.SQUARE, 3, 2, 1);

        //"factory"                     
        GenerateResource(m_grid[0, 9], goldResourceColor, Shape.TRIANGLE, 3, 2, 1);

        m_tileDefinitions = new Dictionary<TileType, TileDefinition>();

        TileDefinition waterTile = new TileDefinition();
        waterTile.m_renderColor = new Color(.24f, .404f, .906f);
        waterTile.m_movementColor = new Color(.24f, .404f, .906f, .5f);
        waterTile.m_attackRangeColor = new Color(.827f, .235f, .906f);
        waterTile.m_attackShapeColor = new Color(.467f, .933f, .807f);
        waterTile.m_isSolid = false;
        waterTile.m_movementCost = 2;
        m_tileDefinitions.Add(TileType.WATER, waterTile);

        TileDefinition stoneTile = new TileDefinition();
        stoneTile.m_renderColor = new Color(.67f, .71f, .74f);
        stoneTile.m_movementColor = new Color(.67f, .71f, .74f, .5f);
        stoneTile.m_attackRangeColor = new Color(.803f, .576f, .643f);
        stoneTile.m_attackShapeColor = new Color(.863f, .89f, .71f);
        stoneTile.m_isSolid = true;
        stoneTile.m_movementCost = int.MaxValue;
        m_tileDefinitions.Add(TileType.STONE, stoneTile);

        TileDefinition landTile = new TileDefinition();
        landTile.m_isSolid = false;
        landTile.m_renderColor = new Color(.49f, .65f, .25f);
        landTile.m_movementColor = new Color(.49f, .65f, .25f, .5f);
        landTile.m_attackRangeColor = new Color(.647f, .373f, .251f);
        landTile.m_attackShapeColor = new Color(.725f, .847f, .055f);
        m_tileDefinitions.Add(TileType.LAND, landTile);

        Team team0 = new Team();
        Color blueColor = new Color(.68f, 1f, 1f);
        Color blueCapColor = new Color(0f, .46f, .46f);
        team0.Initialize(10, 100, blueColor, blueCapColor, "Team 0", 0);
        Team team1 = new Team();

        Color redColor = new Color(.48f, 0f, 0f);
        Color redCapColor = new Color(1f, .48f, .48f);
        team1.Initialize(10, 100, redColor, redCapColor, "Team 1", 1);

        m_teams.Add(team0);
        m_teams.Add(team1);

        /*SpawnUnit(new TileCoord(0, 0), 4, '@', 0,1,1,1,1,1,1,1,1);
        SpawnUnit(new TileCoord(1, 0), 4, '@', 0,1,1,1,1,1,1,1,1);
        SpawnUnit(new TileCoord(9, 9), 4, '@', 1,1,1,1,1,1,1,1,1);
        SpawnUnit(new TileCoord(8, 9), 4, '@', 1,1,1,1,1,1,1,1,1);*/

        SpawnUnit(new TileCoord(0, 0), 0, UnitIdentity.SCOUT);
        SpawnUnit(new TileCoord(1, 0), 0, UnitIdentity.SCOUT);
        SpawnUnit(new TileCoord(2, 0), 0, UnitIdentity.SHOCKTROOPER);
        SpawnUnit(new TileCoord(3, 0), 0, UnitIdentity.SNIPER);

        SpawnUnit(new TileCoord(9, 9), 1, UnitIdentity.SCOUT);
        SpawnUnit(new TileCoord(8, 9), 1, UnitIdentity.SCOUT);
        SpawnUnit(new TileCoord(7, 9), 1, UnitIdentity.SHOCKTROOPER);
        SpawnUnit(new TileCoord(6, 9), 1, UnitIdentity.SNIPER);

        CheckIfSpawnedOnResource();
        DimInactiveUnits();
        if (m_teams[m_curTeam].m_getIncomeAtStart)
            m_teams[m_curTeam].RecieveIncome();
        UpdateUIText();

        //m_outline.rectTransform.position = transform.position;

        m_pathfinding = GetComponent<Pathfinding>();

        foreach (VectorLine vl in m_outlines)
        {
            vl.Draw3DAuto();
        }
    }

    void GenerateResource(HexTile ht, Color color, Shape shape, int pointsToOwn, int owningBonus, int operatingBonus)
    {
        GameObject g = Instantiate(m_resourcePref, new Vector3(0f, 0f, -.1f), Quaternion.identity) as GameObject;
        HexResource hr = g.GetComponent<HexResource>();
        hr.Initialize(ht.m_worldCenterPos, color, shape, pointsToOwn, owningBonus, operatingBonus, ht);
        ht.m_resource = hr;
        m_hexResources.Add(hr);
    }

    void SpawnUnit(TileCoord tc, int teamNumber, UnitIdentity unitName)
    {
        GameObject g = Instantiate(m_unitPref, m_grid[tc.x, tc.y].m_worldCenterPos, Quaternion.identity) as GameObject;
        Unit u = null;
        Team team = m_teams[teamNumber];

        switch (unitName)
        {
            case UnitIdentity.SCOUT:
                u = g.AddComponent<Scout>();
                Scout scout = u as Scout;
                scout.Initialize(tc, m_teams[teamNumber]);
                break;
            case UnitIdentity.SHOCKTROOPER:
                u = g.AddComponent<ShockTrooper>();
                ShockTrooper shock = u as ShockTrooper;
                shock.Initialize(tc, m_teams[teamNumber]);
                break;
            case UnitIdentity.SNIPER:
                u = g.AddComponent<Sniper>();
                Sniper sniper = u as Sniper;
                sniper.Initialize(tc, m_teams[teamNumber]);
                break;
        }

        team.AddUnit(u);
        m_grid[tc.x, tc.y].m_unit = u;
    }

    void CheckIfSpawnedOnResource()
    {
        foreach (Team t in m_teams)
        {
            foreach (Unit u in t.m_units)
            {
                TileCoord tc = u.m_position;
                HexResource hr = m_grid[tc.x, tc.y].m_resource;
                if (hr != null)
                {
                    m_teams[u.m_team].m_income += hr.m_operatingBonus;
                }
            }
        }
    }

    Color GetColorFromTileType(TileType t)
    {
        TileDefinition td = m_tileDefinitions[t];
        return td.m_renderColor;
    }

    Color GetMovementColorFromTileType(TileType t)
    {
        TileDefinition td = m_tileDefinitions[t];
        return td.m_movementColor;
    }

    Color GetAttackRangeColorFromTileType(TileType t)
    {
        TileDefinition td = m_tileDefinitions[t];
        return td.m_attackRangeColor;
    }

    Color GetAttackShapeColorFromTileType(TileType t)
    {
        TileDefinition td = m_tileDefinitions[t];
        return td.m_attackShapeColor;
    }

    static Vector2 TransformForAngle(float angleDegrees)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angleDegrees), Mathf.Sin(Mathf.Deg2Rad * angleDegrees));
    }

    void GenerateHexOutlines(Vector2 hexWorldCenterPos, float offset, TileCoord tilePos)
    {
        List<Vector3> outlineVertexList = new List<Vector3>();

        //Vector2 halfWidth = new Vector2(0f, 0f);//new Vector2(((xDimensions / 2f) * m_width) - (m_width / 2f), ((yDimensions / 2f) * m_height) - m_height );

        for (int vertexNumber = 0; vertexNumber < VERTICES_PER_HEX; ++vertexNumber)
        {
            int vertexIndex = (vertexNumber == VERTICES_PER_HEX - 1) ? 0 : vertexNumber;
            outlineVertexList.Add((s_vertexDisplacements[vertexIndex] * TILE_SIZE) + hexWorldCenterPos);
        }

        m_outlines[tilePos.x, tilePos.y].points3 = outlineVertexList;
    }


    void DrawSelectedOutlineHex(Vector2 hexWorldCenterPos, float offset, TileCoord tilePos)
    {
        List<Vector3> outlineVertexList = new List<Vector3>();

        for (int vertexNumber = 0; vertexNumber < VERTICES_PER_HEX; ++vertexNumber)
        {
            int vertexIndex = (vertexNumber == VERTICES_PER_HEX - 1) ? 0 : vertexNumber;
            outlineVertexList.Add((s_vertexDisplacements[vertexIndex] * TILE_SIZE) + hexWorldCenterPos);
        }

        m_selectedOutline.points3 = outlineVertexList;
    }

    void MakeHexWithIndex(HexTile tile, int idx)
    {
        for (int i = 0; i < VERTICES_PER_HEX; ++i)
        {
            m_vertices[i + (VERTICES_PER_HEX * idx)] = (s_vertexDisplacements[i] * TILE_SIZE) + tile.m_worldCenterPos;
            Color c = Color.black;
            switch (tile.m_currentColor)
            {
                case (ColorType.RENDER):
                    c = GetColorFromTileType(tile.m_type);
                    break;
                case (ColorType.MOVEMENT):
                    c = GetMovementColorFromTileType(tile.m_type);
                    break;
                case (ColorType.ATTACKRANGE):
                    c = GetAttackRangeColorFromTileType(tile.m_type);
                    break;
                case (ColorType.ATTACKSHAPE):
                    c = GetAttackShapeColorFromTileType(tile.m_type);
                    break;
            }

            m_colors[i + (VERTICES_PER_HEX * idx)] = c;
        }
        for (int i = 0; i < INDICES_PER_HEX; ++i)
        {
            m_indices[i + (INDICES_PER_HEX * idx)] = indices_lookup[i] + (VERTICES_PER_HEX * idx);
        }
    }

    Vector3 CubeCoordinatesForTile(TileCoord tc)
    {
        float x = (tc.x * Mathf.Sqrt(3f) / 3f - tc.y / 3f) / TILE_SIZE;
        float z = tc.y * 2f / 3f / TILE_SIZE;
        float y = -x - z;
        return new Vector3(x, y, z);
    }

    public Vector3 CubeCoordinatesFromWorldPosition(Vector2 worldPos)
    {
        float x = (worldPos.x * Mathf.Sqrt(3f) / 3f - worldPos.y / 3f) / TILE_SIZE;
        float z = worldPos.y * 2f / 3f / TILE_SIZE;
        float y = -x - z;
        return new Vector3(x, y, z);
    }

    TileCoord TileCoordFromCubeCoordinate(int q, int r, int s)
    {
        return new TileCoord(q, r);
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

    public TileCoord GetTileCoordinateFromWorldPosition(Vector2 worldPos)
    {
        TileCoord minTileCoord = new TileCoord(-1, -1);
        float minDist = float.MaxValue;
        for (int x = 0; x < gridWidthInHexes; x++)
        {
            for (int y = 0; y < gridHeightInHexes; y++)
            {
                HexTile t = m_grid[x, y];
                Vector2 distance = t.m_worldCenterPos - worldPos;
                float dist = Vector2.SqrMagnitude(distance);
                if (dist < minDist)
                {
                    minDist = dist;
                    minTileCoord = t.m_tileCoord;
                }
            }
        }
        return minTileCoord;
    }

    void OnTerraformKey()
    {

        if (m_selectedTileCoords.x >= 0 && m_selectedTileCoords.y >= 0)
        {
            TileType t = m_grid[m_selectedTileCoords.x, m_selectedTileCoords.y].m_type;
            int next = ((int)t) + 1;
            if (next > 2)
                next = 0;
            HexTile tile = m_grid[m_selectedTileCoords.x, m_selectedTileCoords.y];
            tile.m_type = (TileType)(next);
        }

        foreach (Team t in m_teams)
        {
            foreach (Unit u in t.m_units)
            {
                if (u.m_selected)
                {
                    u.m_tilesInRange.Clear();
                    PathingInfo pI = m_pathfinding.DijkstraSearch(u.m_position);
                    FastPriorityQueue<HexTile> order = pI.m_orderedHexTiles;

                    while (order.Count > 0)
                    {
                        HexTile current = order.Dequeue();
                        if (current.Priority <= u.m_movesRemaining && (current.m_unit == null))
                        {
                            current.m_currentColor = ColorType.MOVEMENT;
                            u.m_tilesInRange.Add(current);
                        }
                        else
                            current.m_currentColor = ColorType.RENDER;
                    }
                    break;
                }
            }
        }
    }

    void OnRightMouseUp()
    {
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickPos -= transform.position;
        TileCoord coord = GetTileCoordinateFromWorldPosition(clickPos);

        m_selectedOutline.active = false;
        m_selectedTileCoords = new TileCoord(-1, -1);

        bool unitWasSelected = false;
        foreach (Team t in m_teams)
        { 
            foreach (Unit u in t.m_units)
            {
                if (u.m_selected)
                {
                    u.m_selected = false;
                    if (u.m_attemptingAttack)
                    {
                        u.m_attemptingAttack = false;
                    }
                    unitWasSelected = true;
                    break;
                }
            }
        }
        if(unitWasSelected)
        {
            foreach (HexTile t in m_grid)
            {
                t.m_currentColor = ColorType.RENDER;
            }
        }
        m_unitAttemptingAttack = null;
    }

    void OnLeftMouseUp()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickPos -= transform.position;
        TileCoord coord = GetTileCoordinateFromWorldPosition(clickPos);

        if (coord.x < 0 || coord.y < 0 || coord.x >= gridWidthInHexes || coord.y >= gridHeightInHexes)
            return;

        Debug.Log("Click Info: " + coord.x + "," + coord.y + " " + clickPos);

        
        bool drawOutline = true;
        //Debug.Log(clickPos);
        //Debug.Log(new Vector2(coord.x, coord.y));

        //see if the position is any of our units
        Unit unitClicked = null;
        Unit lastSelected = null;

        foreach (Team t in m_teams)
        {
            foreach (Unit u in t.m_units)
            {
                if (u.m_selected)
                    lastSelected = u;

                if (coord.x == u.m_position.x && coord.y == u.m_position.y)
                {

                    if (u.m_team == m_curTeam)
                    {
                        unitClicked = u;
                        u.m_selected = true;
                    }
                }
                else
                {
                    u.m_selected = false;
                }
            }
        }

        //A unit was selected, so calculate what tiles it can move to.
        if (unitClicked != null)
        {
            if (!(unitClicked.m_attemptingAttack))
            {
                if (unitClicked == lastSelected && unitClicked.m_hasAction)
                {
                    if(unitClicked.m_movesRemaining == 0)
                        unitClicked.m_tilesInRange.Clear();
                    m_unitAttemptingAction = unitClicked;
                    unitClicked.ActionMenuOpened();
                    m_actionMenu.SetActive(true);
                    Vector2 worldPos = m_grid[coord.x, coord.y].m_worldCenterPos;
                    DrawSelectedOutlineHex(worldPos, 1f, coord);
                    m_selectedTileCoords = coord;
                    m_selectedOutline.active = true;
                    return;
                }
                else
                {
                    if ((unitClicked.m_movesRemaining <= 0) && unitClicked.m_hasAction)
                    {
                        if (unitClicked.m_movesRemaining == 0)
                            unitClicked.m_tilesInRange.Clear();
                        m_unitAttemptingAction = unitClicked;
                        unitClicked.ActionMenuOpened();
                        m_actionMenu.SetActive(true);
                        Vector2 worldPos = m_grid[coord.x, coord.y].m_worldCenterPos;
                        DrawSelectedOutlineHex(worldPos, 1f, coord);
                        m_selectedTileCoords = coord;
                        m_selectedOutline.active = true;
                        return;
                    }

                    unitClicked.m_tilesInRange.Clear();
                    PathingInfo pI = m_pathfinding.DijkstraSearch(coord);
                    FastPriorityQueue<HexTile> order = pI.m_orderedHexTiles;

                    while (order.Count > 0)
                    {
                        HexTile current = order.Dequeue();
                        if ((current.Priority <= unitClicked.m_movesRemaining) && (current.m_unit == null))
                        {
                            current.m_currentColor = ColorType.MOVEMENT;
                            unitClicked.m_tilesInRange.Add(current);
                        }
                        else
                            current.m_currentColor = ColorType.RENDER;
                    }
                }
            }
            else
            {
                m_unitAttemptingAttack.DoDamage(clickPos);
                unitClicked.m_attemptingAttack = false;
                m_unitAttemptingAttack = null;
            }
        }
        //No unit was selected, so see if a unit was selected previously and if so move it to the clicked position if
        //that position is in it's movement range.
        else if(lastSelected != null)
        {
            if(lastSelected.m_attemptingAttack)
            {
                m_unitAttemptingAttack.DoDamage(clickPos);
                lastSelected.m_attemptingAttack = false;
                m_unitAttemptingAttack = null;
            }
            foreach (HexTile t in m_grid)
            {
                t.m_currentColor = ColorType.RENDER;
            }

            bool reachable = false;
            int distance = 999;
           foreach(HexTile h in lastSelected.m_tilesInRange)
            {
                TileCoord coord2 = GetTileCoordinateFromWorldPosition(h.m_worldCenterPos);
                if ((coord2.x == coord.x) && (coord2.y == coord.y))
                {
                    reachable = true;
                    distance = (int) (h.Priority);
                    break;
                }
            }
           if(reachable)
            {
                drawOutline = false;
                m_selectedOutline.active = false;
                m_selectedTileCoords = new TileCoord(-1, -1);
                lastSelected.m_selected = false;
                TileCoord oldPos = lastSelected.m_position;
                HexTile ht = m_grid[oldPos.x, oldPos.y];
                ht.m_unit = null;
                HexResource resource = ht.m_resource;
                if (resource != null)
                {
                    m_teams[lastSelected.m_team].m_income -= resource.m_operatingBonus;
                    UpdateUIText();
                }

                lastSelected.m_movesRemaining -= distance;
                
                lastSelected.Move(coord);
                ht = m_grid[coord.x, coord.y];
                ht.m_unit = lastSelected;

                resource = ht.m_resource;
                if (resource != null)
                {
                    m_teams[lastSelected.m_team].m_income += resource.m_operatingBonus;
                    UpdateUIText();
                }
            }
            lastSelected.m_selected = false;
        }

        if (drawOutline)
        {
            Vector2 worldPos = m_grid[coord.x, coord.y].m_worldCenterPos;
            DrawSelectedOutlineHex(worldPos, 1f, coord);
            m_selectedTileCoords = coord;
            m_selectedOutline.active = true;
        }
        
    }

    public void HandleEndOfUnitsAction(Unit u)
    {
        Color c = u.m_textMesh.color;
        c.a = .5f;
        u.m_textMesh.color = c;

        int team = u.m_team;
        bool unitsRemaining = false;
        foreach(Unit u2 in m_teams[team].m_units)
        {
            if ((u2.m_team == team) && ((u2.m_movesRemaining > 0)||u2.m_hasAction))
            {
                unitsRemaining = true;
                break;
            }
        }

        foreach (HexTile t in m_grid)
        {
            t.m_currentColor = ColorType.RENDER;
        }

        if (!unitsRemaining)
        {
            EndTurn();
        }
    }

    void DimInactiveUnits()
    {
        foreach (Team t in m_teams)
        { 
            foreach (Unit u2 in t.m_units)
            {
                if (u2.m_team != m_curTeam)
                {
                    u2.m_movesRemaining = 0;
                    Color c = u2.m_textMesh.color;
                    c.a = .5f;
                    u2.m_textMesh.color = c;
                    u2.m_selected = false;
                }
            }
        }
    }

    void EndTurn()
    {
        UpdateResourceOwningBonuses();
        int team = m_curTeam;
        m_curTeam++;
        if (m_curTeam >= m_teams.Count)
        {
            m_curTeam = 0;
            m_round++;
        }

        foreach (Team t in m_teams)
        { 
            foreach (Unit u2 in t.m_units)
            {
                if (u2.m_team == m_curTeam)
                {
                    u2.m_movesRemaining = u2.m_movementRange;
                    Color c = u2.m_textMesh.color;
                    c.a = 1f;
                    u2.m_textMesh.color = c;
                }
                else
                {
                    u2.m_movesRemaining = 0;
                    Color c = u2.m_textMesh.color;
                    c.a = .5f;
                    u2.m_textMesh.color = c;
                    u2.m_selected = false;
                }
            }
        }

        foreach (HexTile t in m_grid)
        {
            t.m_currentColor = ColorType.RENDER;
        }

        StartTurn();
    }

    void UpdateResourceOwningBonuses()
    {
        foreach (HexResource hr in m_hexResources)
        {
            Unit unitOnTile = hr.m_tile.m_unit;
            if (hr.m_owningTeam == -1)
            {
                //no owning team
                if (unitOnTile != null)
                {
                    if (hr.m_teamWorkingTowardsOwning == unitOnTile.m_team)
                    {
                        if (unitOnTile.m_team == m_curTeam)
                        {
                            //the team ending their turn
                            hr.m_pointsTowardsOwning += unitOnTile.m_pointsTowardsCapture;
                            if (hr.m_pointsTowardsOwning >= hr.m_pointsToOwn)
                            {
                                hr.m_owningTeam = m_curTeam;
                                m_teams[m_curTeam].m_income += hr.m_owningBonus;
                                hr.m_teamWorkingTowardsOwning = -1;
                                
                                hr.Recolor(m_teams[m_curTeam].m_captureColor);
                            }
                        }
                    }
                    else
                    {
                        //a unit is on it and it is it's first turn working towards the goal
                        hr.m_teamWorkingTowardsOwning = unitOnTile.m_team;
                        if (unitOnTile.m_team == m_curTeam)
                            hr.m_pointsTowardsOwning = unitOnTile.m_pointsTowardsCapture;
                        else
                            hr.m_pointsTowardsOwning = 0;
                    }

                }
                else
                {
                    hr.m_teamWorkingTowardsOwning = -1;
                    hr.m_pointsTowardsOwning = 0;
                }
            }
            else if(unitOnTile != null && unitOnTile.m_team != hr.m_owningTeam)
            {
                //if there's a unit on it that is not the owning team
                m_teams[hr.m_owningTeam].m_income -= hr.m_owningBonus;
                hr.m_owningTeam = -1;
                hr.m_teamWorkingTowardsOwning = unitOnTile.m_team;
                hr.Recolor(hr.m_baseColor);
                if (unitOnTile.m_team == m_curTeam)
                    hr.m_pointsTowardsOwning = unitOnTile.m_pointsTowardsCapture;
                else
                    hr.m_pointsTowardsOwning = 0;

            }
        }
    }

    void StartTurn()
    {
        
        if (m_round != 0)
        {
            m_teams[m_curTeam].RecieveIncome();
            UpdateUIText();
            foreach(Unit u in m_teams[m_curTeam].m_units)
            {
                u.m_hasAction = true;
            }
        }
        else if(m_teams[m_curTeam].m_getIncomeAtStart)
        {
            m_teams[m_curTeam].RecieveIncome();
            UpdateUIText();
        }
    }

    void UpdateUIText()
    {
        Team player = m_teams[0];
        m_UIText.text = "<b>Your team: </b><color=#adffff>" + player.m_name +
            " </color> <b>Income: </b><color=#adffff>" + player.m_income +
            "</color> <b> Upkeep: </b><color=#adffff>" + player.m_upkeep +
            "</color> <b> Reserves: </b><color=#adffff>" + player.m_incomeReserves +
            "</color> <b> Active Team: </b>";
        switch (m_curTeam)
        {
            case 0:
                m_UIText.text += "<color=#adffff>";
                break;
            case 1:
                m_UIText.text += "<color=red>";
                break;
        }
        m_UIText.text += m_teams[m_curTeam].m_name + "</color>";
    }

    void Update()
    {
        if(m_unitAttemptingAttack != null)
        {
            UpdateAttackShape();
        }

        m_selectedOutline.Draw3DAuto();

        //Show border on all hexes, thicker one on one selected
        //Add some solid hexes, do Dj. algorithm

        //c.transform.position += new Vector3(Mathf.Cos(Time.realtimeSinceStartup), Mathf.Sin(Time.realtimeSinceStartup), 0f);


        for (int x = 0; x < gridWidthInHexes; ++x)
        {
            for (int y = 0; y < gridHeightInHexes; ++y)
            {
                HexTile ht = m_grid[x, y];
                MakeHexWithIndex(ht, (gridWidthInHexes * y) + x);
            }
        }

        m_hexMesh.vertices = m_vertices;
        m_hexMesh.triangles = m_indices;
        m_hexMesh.colors = m_colors;

        m_hexMesh.RecalculateBounds();

        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickPos -= transform.position;
        TileCoord coord = GetTileCoordinateFromWorldPosition(clickPos);

        if (coord.x < 0 || coord.y < 0 || coord.x >= gridWidthInHexes || coord.y >= gridHeightInHexes)
            return;

        if (Input.GetMouseButtonUp(0))
            OnLeftMouseUp();
        else if (Input.GetMouseButtonUp(1))
            OnRightMouseUp();
        else if (Input.GetKeyUp(KeyCode.T))
            OnTerraformKey();

        if (Input.GetKeyDown(KeyCode.S))
            SpawnTest();
        else if (Input.GetKeyDown(KeyCode.K))
            KillTest();
        else if (Input.GetKeyDown(KeyCode.E))
            EndTurn();


    }

    void UpdateAttackShape()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos -= transform.position;
        TileCoord mouseCoord = GetTileCoordinateFromWorldPosition(mousePos);
        if (mouseCoord == m_previousTileAttackShapeWasDrawnOn)
            return;
        if (mouseCoord.x < 0 || mouseCoord.y < 0 || mouseCoord.x >= gridWidthInHexes || mouseCoord.y >= gridHeightInHexes)
            return;
        TileCoord unitCoord = m_unitAttemptingAttack.m_position;

        Vector3 unitPos = m_grid[unitCoord.x, unitCoord.y].m_worldCenterPos;

        Vector3 c1 = CubeCoordinatesFromWorldPosition(mousePos);
        Vector3 c2 = CubeCoordinatesFromWorldPosition(unitPos);
        float distance = Mathf.Max(Mathf.Abs(c1.x - c2.x), Mathf.Abs(c1.y - c2.y), Mathf.Abs(c1.z - c2.z));
        bool isInRange = (Mathf.Approximately(distance, m_unitAttemptingAttack.m_attackRangeMin)) || (Mathf.Approximately(distance, m_unitAttemptingAttack.m_attackRangeMax));
        isInRange = isInRange || (distance >= m_unitAttemptingAttack.m_attackRangeMin && distance <= m_unitAttemptingAttack.m_attackRangeMax);
        if (isInRange)
        {
            if (m_previousTileAttackShapeWasDrawnOn.x != -1)
            {
                foreach (TileCoord t in m_unitAttemptingAttack.m_displacementsForShape)
                {
                    TileCoord newCoord = new TileCoord(m_previousTileAttackShapeWasDrawnOn.x + t.x, m_previousTileAttackShapeWasDrawnOn.y + t.y);
                    if (newCoord.x >= 0 && newCoord.y >= 0 && newCoord.x < gridWidthInHexes && newCoord.y < gridHeightInHexes)
                    {
                        HexTile h = m_grid[newCoord.x, newCoord.y];
                        Vector3 highlightPos = h.m_worldCenterPos;
                        c1 = CubeCoordinatesFromWorldPosition(highlightPos);
                        distance = Mathf.Max(Mathf.Abs(c1.x - c2.x), Mathf.Abs(c1.y - c2.y), Mathf.Abs(c1.z - c2.z));
                        isInRange = (Mathf.Approximately(distance, m_unitAttemptingAttack.m_attackRangeMin)) || (Mathf.Approximately(distance, m_unitAttemptingAttack.m_attackRangeMax));
                        isInRange = isInRange || (distance >= m_unitAttemptingAttack.m_attackRangeMin && distance <= m_unitAttemptingAttack.m_attackRangeMax);
                        if (isInRange)
                            h.m_currentColor = ColorType.ATTACKRANGE;
                        else
                            h.m_currentColor = ColorType.RENDER;
                    }
                }
            }

            foreach (TileCoord t in m_unitAttemptingAttack.m_displacementsForShape)
            {
                TileCoord newCoord = new TileCoord(mouseCoord.x + t.x, mouseCoord.y + t.y);
                if (newCoord.x >= 0 && newCoord.y >= 0 && newCoord.x < gridWidthInHexes && newCoord.y < gridHeightInHexes)
                {
                    HexTile h = m_grid[newCoord.x, newCoord.y];
                    h.m_currentColor = ColorType.ATTACKSHAPE;
                }
            }
            

            
            m_previousTileAttackShapeWasDrawnOn = mouseCoord;

        }
    }

    void SpawnTest()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos -= transform.position;
        TileCoord coord = GetTileCoordinateFromWorldPosition(mousePos);

        bool occupied = false;
        foreach (Team t in m_teams)
        {
            foreach (Unit u in t.m_units)
            {
                if ((u.m_position.x == coord.x) && (u.m_position.y == coord.y))
                    occupied = true;
            }
        }

        if (!occupied)
        {
            SpawnUnit(coord, 1, UnitIdentity.SCOUT);
        }
    }
    
    void KillTest()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos -= transform.position;
        TileCoord coord = GetTileCoordinateFromWorldPosition(mousePos);

        Unit occupyingUnit = null;
        Team occupyingUnitsTeam = null;
        foreach (Team t in m_teams)
        {
            foreach (Unit u in t.m_units)
            {
                if ((u.m_position.x == coord.x) && (u.m_position.y == coord.y))
                {
                    occupyingUnit = u;
                    occupyingUnitsTeam = t;
                }
            }
        }

        if (occupyingUnit != null)
        {
            occupyingUnitsTeam.RemoveUnit(occupyingUnit);
            Destroy(occupyingUnit.gameObject);
            m_grid[coord.x, coord.y].m_unit = null;
        }
    }

    public List<HexTile> GetNeighbors(HexTile currentHexTile)
    {
        TileCoord tileCoord = currentHexTile.m_tileCoord;

        List<HexTile> neighbors = new List<HexTile>();

        for(int direction = 0; direction < 6; direction++)
        {
            TileCoord neighborCoords = GetNeighborInDirection(tileCoord, direction);
            if (IsInBounds(neighborCoords))
                neighbors.Add(m_grid[neighborCoords.x, neighborCoords.y]);

        }

        return neighbors;
    }

    public int GetCost(HexTile from, HexTile to)
    {
        //Vector3 toCubeCoords = CubeCoordinatesFromWorldPosition(from.m_worldCenterPos);
        //TileCoord toTileCoord = TileCoordFromCubeCoordinate((int)toCubeCoords.x, (int)toCubeCoords.x, (int)toCubeCoords.z);

        TileCoord toTileCoord = GetTileCoordinateFromWorldPosition(to.m_worldCenterPos);

        if (IsInBounds(toTileCoord))
        {
            int tileX = toTileCoord.x;
            int tileY = toTileCoord.y;
            HexTile tileAt = m_grid[tileX, tileY];
            TileDefinition thisTileDef = m_tileDefinitions[tileAt.m_type];
            return thisTileDef.m_movementCost;
        }
        else
        {
            return int.MaxValue;
        }
    }

    public TileDefinition GetTileDefinition(TileType t)
    {
        return m_tileDefinitions[t];
    }

    public void AttackPressed()
    {
        m_unitAttemptingAttack = m_unitAttemptingAction;
        m_unitAttemptingAction.OnAttackPressed();

        int team = m_unitAttemptingAction.m_team;

        m_unitAttemptingAction = null;
        ExitActionMenu();
    }

    public void SpecialPressed()
    {
        m_unitAttemptingAction.OnSpecialAbilityPressed();

        int team = m_unitAttemptingAction.m_team;
        HandleEndOfUnitsAction(m_unitAttemptingAction);

        Debug.Log("Special Pressed");
        m_unitAttemptingAction = null;
        ExitActionMenu();
    }

    public void ExitPressed()
    {
        ExitActionMenu();
    }

    public void ExitActionMenu()
    {
        m_actionMenu.SetActive(false);
        m_unitAttemptingAction = null;
    }

    public TileCoord GetNeighborInDirection(TileCoord tileCoord, int direction)
    {
        /*
           int parity = tileCoord.y & 1;
        TileCoord thisDir = directions[parity, direction];
        TileCoord targetDir = new TileCoord(thisDir.x + tileCoord.x, tileCoord.y + thisDir.y);
        return targetDir;
        */

        TileCoord thisDir = s_neighborDirections[direction];
        TileCoord targetDir = new TileCoord(thisDir.x + tileCoord.x, tileCoord.y + thisDir.y);
        return targetDir;

    }

    public bool IsInBounds(TileCoord coord)
    {
        bool inBounds = (coord.x < gridWidthInHexes && coord.x >= 0 && coord.y < gridHeightInHexes && coord.y >= 0);
        return inBounds;
    }

    public Vector2 CalculateHexCenter(int x, int y)
    {
        Vector2 xBasis = Mathf.Sqrt(3) * TILE_SIZE * new Vector2(1f, 0f);
        Vector2 yBasis = Mathf.Sqrt(3) * TILE_SIZE * new Vector2(Mathf.Cos(Mathf.Deg2Rad * 60f), Mathf.Sin(Mathf.Deg2Rad * 60f));
        Vector2 hexCenter = (xBasis * x) + (yBasis * y);
        return hexCenter;
    }

}
   
