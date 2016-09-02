using UnityEngine;
using Priority_Queue;
using System.Collections.Generic;
using Vectrosity;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Reflection;

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
    public GameObject m_buildMenu;
    public GameObject m_gameOverMenu;
    public GameObject m_startMenu;

    public TileCoord m_buildTileCoord;

    public List<Team> m_teams;
    private List<HexResource> m_hexResources;

    private Pathfinding m_pathfinding;

    Mesh m_hexMesh;

    Vector3[] m_vertices;
    int[] m_indices;
    Color[] m_colors;

    public float m_hexWidth;
    public float m_hexHeight;

    public Dictionary<TileType, TileDefinition> m_tileDefinitions;
    
    private int m_curTeam = 0;
    private int m_round = 0;

    private Text m_UIText;

    private const int VERTICES_PER_HEX = 7;
    private const int INDICES_PER_HEX = 18;

    public static List<Unit> s_unitBlueprints;
    public GameState m_gameState;
    public float m_minimumMovementCost;



    private static readonly int[] indices_lookup = new int[INDICES_PER_HEX] {
      6,0,1,
      6,1,2,
      6,2,3,
      6,3,4,
      6,4,5,
      6,5,0
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


    void Awake()
    {
        CreateUnitBlueprints();
    }

    void Start()
    {
        m_actionMenu = GameObject.FindGameObjectWithTag("ActionMenu");
        m_specialText = GameObject.FindGameObjectWithTag("SpecialText");
        m_actionMenu.SetActive(false);
        m_buildMenu = GameObject.FindGameObjectWithTag("BuildMenu");
        m_buildMenu.SetActive(false);
        m_gameOverMenu = GameObject.FindGameObjectWithTag("GameOverMenu");
        m_gameOverMenu.SetActive(false);
        m_startMenu = GameObject.FindGameObjectWithTag("Start Menu") as GameObject;

        m_selectedTileCoords = new TileCoord(-1, -1);
        m_hexMesh = new Mesh();
        m_grid = new HexTile[gridWidthInHexes, gridHeightInHexes];
        m_outlines = new VectorLine[gridWidthInHexes, gridHeightInHexes];
        m_gameState = GameState.GAME_ONGOING;

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

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = m_hexMesh;


        m_minimumMovementCost = 0.5f;

        InitializeTerrainTypes();
        CreateResources();

        GameObject ng = GameObject.FindGameObjectWithTag("NoiseGenerator");
        TextureCreator tg = ng.GetComponent<TextureCreator>();
        tg.GenerateLevelTexture();


        //point cloud -- biggest/smallest x and biggest/smallest y used to frame camera
        for (int x = 0; x < gridWidthInHexes; ++x)
        {
            for (int y = 0; y < gridHeightInHexes; ++y)
            {
                //int isOdd = (y & 1);
                // Vector2 pos = new Vector2(Mathf.Sqrt(3f) * (x - .5f * isOdd), y * 3f / 2f) * tileSize;
                Vector2 pos = CalculateHexCenter(x, y);

                HexTile t = new HexTile(new TileCoord(x, y), pos);
                int xTexel = Mathf.RoundToInt((x + 1) * (512f / gridWidthInHexes));
                int yTexel = Mathf.RoundToInt((y + 1) * (512f / gridHeightInHexes));
                Color pixelColor = tg.m_texture.GetPixel(xTexel, yTexel);
                TileType tileType = PixelToTileType(pixelColor);
                t.m_type = tileType;
                m_grid[x, y] = t;

                VectorLine newVL = new VectorLine("Hex Outline" + x + "," + y, new List<Vector3>(), 5f, LineType.Continuous);
                //Hex Outline Color
                //newVL.color = new Color(.804f, .918f, .918f);
                newVL.color = Color.black;
                m_outlines[x, y] = newVL;
                GenerateHexOutlines(m_grid[x, y].m_worldCenterPos, 1f, new TileCoord(x, y));

            }
        }

        Color goldResourceColor = new Color(.96f, .69f, .26f);
        GenerateResource(m_grid[0, 0], ResourceType.MINE);
        GenerateResource(m_grid[1, 1], ResourceType.MINE);

        //"factory"                     
        GenerateResource(m_grid[0, 9], ResourceType.FACTORY);
        CreateTeams();


        SpawnUnits();
        CheckIfSpawnedOnResource();
        DimInactiveUnits();

        if (m_teams[m_curTeam].m_getIncomeAtStart)
            m_teams[m_curTeam].RecieveIncome();

        //m_outline.rectTransform.position = transform.position;

        m_pathfinding = GetComponent<Pathfinding>();

        foreach (VectorLine vl in m_outlines)
        {
            vl.Draw3DAuto();
        }

        SetupCamera();

        InvokeRepeating("UpdateUIText", 0f, 0.5f); //If this is done every frame, crash happens. Why?


}

    static float GetDistanceBetweenColors(Color a, Color b)
    {

        float dR = a.r - b.r;
        float dG = a.g - b.g;
        float dB = a.b - b.b;

        return Mathf.Sqrt((dR * dR) + (dG * dG) + (dB * dB));
    }

    static TileType PixelToTileType(Color source)
    {
        List<Color> palette = new List<Color>();
        palette.Add(Color.green);
        palette.Add(Color.red);
        palette.Add(Color.white);
        palette.Add(Color.cyan);
        palette.Add(Color.blue);

        // Store the current closest distance and colour:
        float CurrentClosestDistance = float.MaxValue;
        Color CurrentClosestColour = Color.black;

        // Iterate over the possible matches, and check the distance each time.
        foreach (Color c in palette)
        {
            float d = GetDistanceBetweenColors(source, c);
            if (d < CurrentClosestDistance)
            {
                CurrentClosestDistance = d;
                CurrentClosestColour = c;
            }
        }

        TileType resultTerrain = TileType.STONE;

        if (CurrentClosestColour == Color.green)
        {
            resultTerrain = TileType.FOREST;
        }
        else if (CurrentClosestColour == Color.red)
        {
            resultTerrain = TileType.LAND;
        }
        else if (CurrentClosestColour == Color.cyan)
        {
            resultTerrain = TileType.ROAD;
        }
        else if (CurrentClosestColour == Color.white)
        {
            resultTerrain = TileType.STONE;
        }
        else if (CurrentClosestColour == Color.blue)
        {
            resultTerrain = TileType.WATER;
        }
        return resultTerrain;
    }


    //Right now, creates one of each unit, excluding tank, and avoiding spawning on top of solid tiles
    public void SpawnUnits()
    {

        int currentUnit = 0;

        for (int y = 0; y < gridHeightInHexes; y++)
        {

            if (currentUnit >= 4)
                break;

            for (int x = 0; x < gridWidthInHexes; x++)
            {
                if (currentUnit >= 4)
                    break;

                HexTile hta = m_grid[x, y];

                if (m_tileDefinitions[hta.m_type].m_isSolid)
                    continue;

                SpawnUnit(new TileCoord(x, y), 0, currentUnit);
                currentUnit++;
            }
        }

        currentUnit = 0;



        for (int y = gridHeightInHexes - 1; y > 0; y--)
        {
            if (currentUnit >= 4)
                break;
            for (int x = gridWidthInHexes - 1; x > 0; x--)
            {
                if (currentUnit >= 4)
                    break;

                HexTile hta = m_grid[x, y];

                if (m_tileDefinitions[hta.m_type].m_isSolid)
                    continue;

                SpawnUnit(new TileCoord(x, y), 1, currentUnit);
                currentUnit++;
            }
        }

    }

    public void SetupCamera()
    {
        float camx = m_grid[gridWidthInHexes - 1, gridHeightInHexes - 1].m_worldCenterPos.x - m_grid[0, 0].m_worldCenterPos.x;
        float camy = m_grid[gridWidthInHexes - 1, gridHeightInHexes - 1].m_worldCenterPos.y - m_grid[0, 0].m_worldCenterPos.y;
        //float camy = (gridHeightInHexes / 2.5f) * m_hexHeight - (m_hexHeight / 2f);
        Vector2 cameraOffset = new Vector2(camx / 2f, (camy / 2f) + (m_hexHeight));

        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera") as GameObject;
        Camera c = camera.GetComponent<Camera>() as Camera;
        // VectorLine.SetCamera3D(camera);
        c.transform.position = new Vector3(cameraOffset.x, cameraOffset.y, -10f);

        CameraController cc = c.GetComponent<CameraController>();
        cc.m_resetCamera = c.transform.position;
        c.orthographicSize = (m_hexHeight * gridHeightInHexes) / 2f - (m_hexHeight / 2f);
        cc.m_defaultScale = c.orthographicSize;
    }

    public void InitializeTerrainTypes()
    {
        m_tileDefinitions = new Dictionary<TileType, TileDefinition>();

        TileDefinition landTile = new TileDefinition();
        landTile.m_isSolid = false;
        landTile.m_renderColor = new Color(.49f, .65f, .25f);
        landTile.m_defense = 0;
        landTile.m_movementColor = new Color(.49f, .65f, .25f, .5f);
        landTile.m_attackRangeColor = new Color(.647f, .373f, .251f);
        landTile.m_attackShapeColor = new Color(.725f, .847f, .055f);
        m_tileDefinitions.Add(TileType.LAND, landTile);

        TileDefinition waterTile = new TileDefinition();
        waterTile.m_renderColor = new Color(.24f, .404f, .906f);
        waterTile.m_movementColor = new Color(.24f, .404f, .906f, .5f);
        waterTile.m_attackRangeColor = new Color(.827f, .235f, .906f);
        waterTile.m_attackShapeColor = new Color(.467f, .933f, .807f);
        waterTile.m_defense = -1;
        waterTile.m_isSolid = false;
        waterTile.m_movementCost = 3f;
        m_tileDefinitions.Add(TileType.WATER, waterTile);

        TileDefinition stoneTile = new TileDefinition();
        stoneTile.m_renderColor = new Color(.67f, .71f, .74f);
        stoneTile.m_movementColor = new Color(.67f, .71f, .74f, .5f);
        stoneTile.m_attackRangeColor = new Color(.803f, .576f, .643f);
        stoneTile.m_attackShapeColor = new Color(.863f, .89f, .71f);
        stoneTile.m_isSolid = true;
        stoneTile.m_movementCost = float.MaxValue;
        m_tileDefinitions.Add(TileType.STONE, stoneTile);
        
        TileDefinition forestTile = new TileDefinition();
        forestTile.m_isSolid = false;
        forestTile.m_renderColor = new Color(.16f, .27f, .016f);
        forestTile.m_defense = 1;
        forestTile.m_movementCost = 2f;
        forestTile.m_attackRangeColor = new Color(.5f, .1f, .1f);
        forestTile.m_attackShapeColor = new Color(.8f, .3f, .3f);
        m_tileDefinitions.Add(TileType.FOREST, forestTile);

        TileDefinition roadTile = new TileDefinition();
        roadTile.m_isSolid = false;
        roadTile.m_renderColor = new Color(.949f, .886f, .749f);
        roadTile.m_defense = 0;
        roadTile.m_movementCost = 0.5f;
        roadTile.m_attackRangeColor = new Color(.7f, .8f, .7f);
        roadTile.m_attackShapeColor = new Color(.3f, .4f, .3f);
        m_tileDefinitions.Add(TileType.ROAD, roadTile);

        TileDefinition mountainTile = new TileDefinition();
        mountainTile.m_isSolid = false;
        mountainTile.m_renderColor = new Color(.459f, .259f, .055f);
        mountainTile.m_defense = 2;
        mountainTile.m_movementCost = 3f;
        mountainTile.m_attackRangeColor = new Color(.5f, .5f, .5f);
        mountainTile.m_attackShapeColor = new Color(.8f, .4f, .4f);
        m_tileDefinitions.Add(TileType.MOUNTAINS, mountainTile);


    }

    public void SetUpPlayMode(int button)
    {
        GameObject startMenu = GameObject.FindGameObjectWithTag("Start Menu") as GameObject;
        startMenu.SetActive(false);

        switch (button)
        {
            case 0:
                Debug.Log("pvp");
                m_teams[0].m_isAI = false;
                m_teams[1].m_isAI = false;
                break;
            case 1:
                Debug.Log("pva");
                m_teams[0].m_isAI = false;
                m_teams[1].m_isAI = true;
                break;
            case 2:
                Debug.Log("ava");
                m_teams[0].m_isAI = true;
                m_teams[1].m_isAI = true;
                StartTurn();
                break;
        }
    }

    void GenerateResource(HexTile ht, ResourceType rt)
    {
        GameObject g = Instantiate(m_resourcePref, new Vector3(0f, 0f, -.1f), Quaternion.identity) as GameObject;
        HexResource hr = g.GetComponent<HexResource>();
        hr.Initialize(ht, rt);
        ht.m_resource = hr;
        m_hexResources.Add(hr);
    }



    void SpawnUnit(TileCoord tc, int teamNumber, int unitNumber, bool incurCosts = false)
    {
        HexTile tileToSpawnAt = m_grid[tc.x, tc.y];

        GameObject g = Instantiate(m_unitPref, tileToSpawnAt.m_worldCenterPos, Quaternion.identity) as GameObject;
        Destroy(g.GetComponent<Unit>());
        Unit u = g.AddComponent(s_unitBlueprints[unitNumber]);
        Team team = m_teams[teamNumber];

        if (incurCosts)
        {
            int cost = u.m_cost;
            if (team.m_goldReserves >= cost)
            {
                team.m_goldReserves -= cost;
            }
            else
            {
                Destroy(u);
                return;
            }
        }

        u.Initialize(team);
        u.SetPosition(tc);
        team.AddUnit(u);
        m_grid[tc.x, tc.y].m_unit = u;
    }

    void SpawnUnit(TileCoord tc, int teamNumber, UnitIdentity unitName, bool incurCosts=false)
    {

        HexTile tileToSpawnAt = m_grid[tc.x, tc.y];

        GameObject g = Instantiate(m_unitPref, tileToSpawnAt.m_worldCenterPos, Quaternion.identity) as GameObject;
        Unit u = g.AddComponent<Unit>();
        Team team = m_teams[teamNumber];

        switch (unitName)
        {
            case UnitIdentity.SCOUT:
                u.InitializeScout(tc, team);
                break;
            case UnitIdentity.SHOCKTROOPER:
                u.InitializeShocktrooper(tc, team);
                break;
            case UnitIdentity.SNIPER:
                u.InitializeSniper(tc, team);
                break;
            case UnitIdentity.ARTILLERY:
                u.InitializeArtillery(tc, team);
                break;
            case UnitIdentity.TANK:
                u.InitializeTank(tc, team);
                break;
        }

        if (incurCosts)
        {
            int cost = u.m_cost;
            if (team.m_goldReserves >= cost)
            {
                team.m_goldReserves -= cost;
            }
            else
            {
                Destroy(u);
                return;
            }
        }


        team.AddUnit(u);
        m_grid[tc.x, tc.y].m_unit = u;

    }


    Unit GenerateBlueprintUnit(UnitIdentity unitName)
    {
        GameObject g = Instantiate(m_unitPref) as GameObject;
        Unit u = g.AddComponent<Unit>();
       // g.SetActive(false);

        switch (unitName)
        {
            case UnitIdentity.SCOUT:
                u.InitializeScout(new TileCoord(), new Team());
                break;
            case UnitIdentity.SHOCKTROOPER:
                u.InitializeShocktrooper(new TileCoord(), new Team());
                break;
            case UnitIdentity.SNIPER:
                u.InitializeSniper(new TileCoord(), new Team());
                break;
            case UnitIdentity.ARTILLERY:
                u.InitializeArtillery(new TileCoord(), new Team());
                break;
            case UnitIdentity.TANK:
                u.InitializeTank(new TileCoord(), new Team());
                break;
        }

        return u;
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
                    m_teams[u.m_team].m_income += (hr.m_operatingBonus * u.m_operatingIncomeFraction);
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

        /*
        #TODO: Not quite pixel perfect
        */
        
        Vector3 foundHexTilePos = m_grid[minTileCoord.x, minTileCoord.y].m_worldCenterPos;
        if (minTileCoord.x == gridHeightInHexes - 1 || minTileCoord.y == gridHeightInHexes - 1 || minTileCoord.x == 0 || minTileCoord.y == 0)
        {
            if (((worldPos.x > (foundHexTilePos.x + m_hexWidth / 2f)) || (worldPos.y > foundHexTilePos.y + m_hexWidth / 2f)))
                return new TileCoord(-1, -1);
        }
        

        return minTileCoord;
    }

    void OnTerraformKey()
    {

        if (m_selectedTileCoords.x >= 0 && m_selectedTileCoords.y >= 0)
        {
            TileType t = m_grid[m_selectedTileCoords.x, m_selectedTileCoords.y].m_type;
            int next = ((int)t) + 1;
            if (next >= (int)TileType.NUM_TYPES)
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
                    u.m_tilesInMovementRange.Clear();
                    PathingInfo pI = m_pathfinding.DijkstraSearch(u.m_position);
                    FastPriorityQueue<HexTile> order = pI.m_orderedHexTiles;

                    while (order.Count > 0)
                    {
                        HexTile current = order.Dequeue();
                        if (current.Priority <= u.m_movesRemaining && (current.m_unit == null))
                        {
                            current.m_currentColor = ColorType.MOVEMENT;
                            u.m_tilesInMovementRange.Add(current);
                        }
                        else
                            current.m_currentColor = ColorType.RENDER;
                    }
                    break;
                }
            }
        }
    }

    public List<HexTile> GetValidMovableTiles(Unit unit)
    {
        PathingInfo pI = m_pathfinding.DijkstraSearch(unit.m_position);
        FastPriorityQueue<HexTile> order = pI.m_orderedHexTiles;

        List<HexTile> list = new List<HexTile>();
        int wantedRadius = unit.m_movementRange;
        int wantedRadiusSquared = wantedRadius * wantedRadius;
        int maxTiles = (3 * wantedRadiusSquared) - (3 * wantedRadius) + 1;
        int numAdded = 0;

        while (order.Count > 0)
        {
            HexTile current = order.Dequeue();
            if ((current.Priority <= unit.m_movementRange) && (current.m_unit == null))
            {
                list.Add(current);
                numAdded++;
                if (numAdded >= maxTiles)
                    break;
            }
        }

        return list;
    }

    void OnRightMouseUp()
    {

        TileCoord coord = GetTileCoordinateForCurrentMousePosition();
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

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

        if (unitWasSelected)
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
        TileCoord coord = GetTileCoordinateForCurrentMousePosition();

        if (coord.x < 0 || coord.y < 0 || coord.x >= gridWidthInHexes || coord.y >= gridHeightInHexes)
            return;

        Debug.Log("Click Info: " + coord.x + "," + coord.y);

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

        if (unitClicked != null)
        {
            if (!(unitClicked.m_attemptingAttack))
            {
                if((lastSelected != null) && lastSelected.m_attemptingAttack)
                {
                    lastSelected.m_tilesInMovementRange.Clear(); ;
                    lastSelected.m_attemptingAttack = false;
                    m_unitAttemptingAttack = null;
                
                    foreach (HexTile t in m_grid)
                    {
                        t.m_currentColor = ColorType.RENDER;
                    }

            }
            if (unitClicked == lastSelected && unitClicked.m_hasAction)
                {
                    if (unitClicked.m_movesRemaining == 0)
                        unitClicked.m_tilesInMovementRange.Clear();
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
                            unitClicked.m_tilesInMovementRange.Clear();
                        m_unitAttemptingAction = unitClicked;
                        unitClicked.ActionMenuOpened();
                        m_actionMenu.SetActive(true);
                        Vector2 worldPos = m_grid[coord.x, coord.y].m_worldCenterPos;
                        DrawSelectedOutlineHex(worldPos, 1f, coord);
                        m_selectedTileCoords = coord;
                        m_selectedOutline.active = true;
                        return;
                    }

                    unitClicked.m_tilesInMovementRange.Clear();
                    PathingInfo pI = m_pathfinding.DijkstraSearch(coord);
                    FastPriorityQueue<HexTile> order = pI.m_orderedHexTiles;

                    while (order.Count > 0)
                    {
                        HexTile current = order.Dequeue();
                        if ((current.Priority <= unitClicked.m_movesRemaining) && (current.m_unit == null))
                        {
                            current.m_currentColor = ColorType.MOVEMENT;
                            unitClicked.m_tilesInMovementRange.Add(current);
                        }
                        else
                            current.m_currentColor = ColorType.RENDER;
                    }
                }
            }
            else
            {
                m_unitAttemptingAttack.DoDamage(coord);
                unitClicked.m_attemptingAttack = false;
                m_unitAttemptingAttack = null;
            }
        }
        //No unit was selected, so see if a unit was selected previously and if so move it to the clicked position if
        //that position is in it's movement range.

        else if (lastSelected != null)
        {
            if (lastSelected.m_attemptingAttack)
            {
                m_unitAttemptingAttack.DoDamage(coord);
                lastSelected.m_attemptingAttack = false;
                m_unitAttemptingAttack = null;
            }
            foreach (HexTile t in m_grid)
            {
                t.m_currentColor = ColorType.RENDER;
            }

            bool reachable = false;
            int distance = 999;

            foreach (HexTile h in lastSelected.m_tilesInMovementRange)
            {
                TileCoord coord2 = GetTileCoordinateFromWorldPosition(h.m_worldCenterPos);
                if ((coord2.x == coord.x) && (coord2.y == coord.y))
                {
                    reachable = true;
                    distance = (int)(h.Priority);
                    break;
                }
            }
            if (reachable)
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
                    m_teams[lastSelected.m_team].m_income -= (resource.m_operatingBonus * lastSelected.m_operatingIncomeFraction);
                }

                lastSelected.m_movesRemaining -= distance;

                lastSelected.Move(coord);

                if (!lastSelected.m_canAttackAfterMoving)
                {
                    lastSelected.m_hasAction = false;
                }

                ht = m_grid[coord.x, coord.y];
                ht.m_unit = lastSelected;

                resource = ht.m_resource;
                if (resource != null)
                {
                    m_teams[lastSelected.m_team].m_income += (resource.m_operatingBonus * lastSelected.m_operatingIncomeFraction);
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

        if (lastSelected != null && !lastSelected.m_hasAction && lastSelected.m_movesRemaining == 0 && lastSelected.m_team == m_curTeam)
        {
            HandleEndOfUnitsAction(lastSelected);
        }

    }


    public void DimUnit(Unit u)
    {
        Color c = u.m_textMesh.color;
        c.a = .5f;
        u.m_textMesh.color = c;
    }

    public void HandleEndOfUnitsAction(Unit u)
    {
        DimUnit(u);

        int team = u.m_team;
        bool unitsRemaining = false;
        foreach (Unit u2 in m_teams[team].m_units)
        {
            if ((u2.m_team == team) && ((u2.m_movesRemaining > 0) || u2.m_hasAction))
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

    public TileCoord FindNearestEnemy(Unit u)
    {
        TileCoord myCoord = u.m_position;
        TileCoord minLoc = new TileCoord(-1, -1);
        float minDistance = float.MaxValue;

        foreach (Team t in m_teams)
        {
            if (t.m_number == u.m_team)
                continue;

            foreach (Unit enemy in t.m_units)
            {
                TileCoord enemyLoc = enemy.m_position;
                float distanceToEnemy = Distance(myCoord, enemyLoc);
                if (distanceToEnemy < minDistance)
                {
                    minDistance = distanceToEnemy;
                    minLoc = enemyLoc;
                }
            }

        }

        return minLoc;
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

    public void EndTurn()
    {
        if (m_gameState != GameState.GAME_ONGOING)
        {
            HandleEndOfGame();
        }

        UpdateResourceOwningBonuses();
        

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
                    u2.m_hasAction = true;
                    Color c = u2.m_textMesh.color;
                    c.a = 1f;
                    u2.m_textMesh.color = c;
                }
                else
                {
                    u2.m_movesRemaining = 0;
                    u2.m_hasAction = false;
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
                            hr.m_pointsTowardsOwning += unitOnTile.m_captureRatePerTurn;
                            if (hr.m_pointsTowardsOwning + unitOnTile.m_captureTurnReduction >= hr.m_pointsToOwn)
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
                            hr.m_pointsTowardsOwning = unitOnTile.m_captureRatePerTurn;
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
            else if (unitOnTile != null && unitOnTile.m_team != hr.m_owningTeam)
            {
                //if there's a unit on it that is not the owning team
                m_teams[hr.m_owningTeam].m_income -= hr.m_owningBonus;
                hr.m_owningTeam = -1;
                hr.m_teamWorkingTowardsOwning = unitOnTile.m_team;
                hr.Recolor(hr.m_baseColor);
                if (unitOnTile.m_team == m_curTeam)
                    hr.m_pointsTowardsOwning = unitOnTile.m_captureRatePerTurn;
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
            foreach (Unit u in m_teams[m_curTeam].m_units)
            {
                u.m_hasAction = true;
            }
        }
        else if (m_teams[m_curTeam].m_getIncomeAtStart)
        {
            m_teams[m_curTeam].RecieveIncome();
        }

        if(m_teams[m_curTeam].m_isAI)
        {
            m_teams[m_curTeam].DecideAIAction();
        }
    }

    void UpdateUIText()
    {
        string colorText = "";
        switch (m_curTeam)
        {
            case 0:
                colorText = "<color=#adffff>";
                break;
            case 1:
                colorText  = "<color=red>";
                break;
        }

        if (m_curTeam < m_teams.Count)
        {
            Team team = m_teams[m_curTeam];
            m_UIText.text = "<b>Active team: </b>" + colorText + "" + team.m_name +
                " </color> <b>Income: </b>" + colorText + "" + team.m_income +
                "</color> <b> Upkeep: </b>" + colorText + "" + team.m_upkeep +
                "</color> <b> Reserves: </b>" + colorText + "" + team.m_goldReserves + "</color>";
        }
    }

    void Update()
    {
        if (m_unitAttemptingAttack != null)
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

        TileCoord coord = GetTileCoordinateForCurrentMousePosition();

        if (coord.x < 0 || coord.y < 0 || coord.x >= gridWidthInHexes || coord.y >= gridHeightInHexes)
            return;



        //Is mouse over not over a UI element or is AI's turn
        if (!(EventSystem.current.IsPointerOverGameObject()) && !(m_teams[m_curTeam].m_isAI))
        {
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
            {
                EndTurn();
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                HexTile t = m_grid[coord.x, coord.y];
                if ((!t.m_unit) && t.m_resource && t.m_resource.m_resourceType == ResourceType.FACTORY && t.m_resource.m_owningTeam == m_curTeam)
                {
                    m_buildTileCoord = coord;
                    OpenBuildMenu();
                }

            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                m_gameState = GameState.PLAYER_ONE_WINS_ANNIHILATION;
                HandleEndOfGame();
            }

        }
        else
        {
            if (m_startMenu.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Keypad0))
                {
                    SetUpPlayMode(0);
                }
                else if (Input.GetKeyDown(KeyCode.Keypad1))
                {
                    SetUpPlayMode(1);
                }
                else if (Input.GetKeyDown(KeyCode.Keypad2))
                {
                    SetUpPlayMode(2);
                }
            }
        }


    }

       bool IsInSelectedUnitAttackRange(float distance)
    {
        bool isInRange = (Mathf.Approximately(distance, m_unitAttemptingAttack.m_attackRangeMin)) ||
            (Mathf.Approximately(distance, m_unitAttemptingAttack.m_attackRangeMax));
        isInRange = isInRange ||
            (distance >= m_unitAttemptingAttack.m_attackRangeMin &&
             distance <= m_unitAttemptingAttack.m_attackRangeMax);
        return isInRange;
    }

    bool IsInUnitAttackRange(Unit u, float distance)
    {
        bool isInRange = (Mathf.Approximately(distance, u.m_attackRangeMin)) ||
            (Mathf.Approximately(distance, u.m_attackRangeMax));
        isInRange = isInRange ||
            (distance >= u.m_attackRangeMin &&
             distance <= u.m_attackRangeMax);
        return isInRange;
    }

    public float CubeDistance(Vector3 a, Vector3 b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2f;
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
        float distance = CubeDistance(c1, c2);

        if (IsInSelectedUnitAttackRange(distance))
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
                        distance = CubeDistance(c1, c2);

                        if (IsInSelectedUnitAttackRange(distance))
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

        for (int direction = 0; direction < 6; direction++)
        {
            TileCoord neighborCoords = GetNeighborInDirection(tileCoord, direction);
            if (IsInBounds(neighborCoords))
                neighbors.Add(m_grid[neighborCoords.x, neighborCoords.y]);

        }

        return neighbors;
    }

    public bool IsRoughTerrain(HexTile t)
    {
        return t.m_type != TileType.ROAD && t.m_type != TileType.LAND;
    }

    public float GetCost(HexTile from, HexTile to, TileCoord unitPos)
    {
        //Vector3 toCubeCoords = CubeCoordinatesFromWorldPosition(from.m_worldCenterPos);
        //TileCoord toTileCoord = TileCoordFromCubeCoordinate((int)toCubeCoords.x, (int)toCubeCoords.x, (int)toCubeCoords.z);

        TileCoord toTileCoord = GetTileCoordinateFromWorldPosition(to.m_worldCenterPos);

        if (IsInBounds(toTileCoord))
        {
            int tileX = toTileCoord.x;
            int tileY = toTileCoord.y;
            HexTile tileAtNewPos = m_grid[tileX, tileY];
            HexTile tileAtUnitPos = m_grid[unitPos.x, unitPos.y];

            TileDefinition thisTileDef = m_tileDefinitions[tileAtNewPos.m_type];
            float moveCost = thisTileDef.m_movementCost;

            Unit unit = tileAtUnitPos.m_unit;
            if (unit && IsRoughTerrain(tileAtNewPos))
            {
                moveCost = Mathf.Max(m_minimumMovementCost, unit.m_roughTerrainMovesModifier + moveCost);
            }

            return moveCost;
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

    public void ExitPressed(int menuVal)
    {
        if(menuVal == 0)
            ExitActionMenu();
        else if(menuVal == 1)
            ExitBuildMenu();
    }

    public void OpenBuildMenu()
    {
        //Check if selected thing is a factory, if the current team owns it, and
        //if no unit is on it. If all true:
        //m_buildMenu.SetActive(true);
        BuildMenu bm = GetComponent<BuildMenu>();
        bm.EnterMenu();
    }

    public void ExitActionMenu()
    {
        m_actionMenu.SetActive(false);
        m_unitAttemptingAction = null;
    }

    public void ExitBuildMenu()
    {
        m_buildMenu.SetActive(false);
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

    public void BuildUnit(int buttonPressed)
    {
        UnitIdentity id = (UnitIdentity)buttonPressed;
        SpawnUnit(m_buildTileCoord, m_curTeam, id, true);
        Team currentTeam = m_teams[m_curTeam];
        Unit u = currentTeam.m_units[currentTeam.m_units.Count - 1];
        u.m_hasAction = false;
        u.m_hasAttacked = true;
        u.m_hasSpecial = false;
        u.m_movesRemaining = 0;
        DimUnit(u);
        Debug.Log(buttonPressed);
        m_buildMenu.SetActive(false);
    }

    public void HandleEndOfGame()
    {
        m_gameOverMenu.SetActive(true);
        Text gameOverText = GameObject.FindGameObjectWithTag("GameOverMenuText").GetComponent<Text>();


        switch (m_gameState)
        {
            case GameState.PLAYER_ONE_WINS_ANNIHILATION:
              gameOverText.text = "PLAYER ONE WINS BY ANNIHILATION";
                break;
            case GameState.PLAYER_TWO_WINS_ANNIHILATION:
                gameOverText.text = "PLAYER TWO WINS BY ANNIHILATION";
                break;
            case GameState.PLAYER_ONE_WINS_BANKRUPTCY:
                gameOverText.text = "PLAYER ONE WINS BY BANKRUPTCY";
                break;
            case GameState.PLAYER_TWO_WINS_BANKRUPTCY:
                gameOverText.text = "PLAYER TWO WINS BY BANKRUPTCY";
                break;
        }
        
    }

    private void CreateResources()
    {
        HexResource.s_resourceMap = new Dictionary<ResourceType, ResourceData>();

        ResourceData mineData = new ResourceData();
        mineData.m_allowsConstruction = false;
        mineData.m_resourceShape = Shape.SQUARE;
        mineData.m_operatingBonus = 3;
        mineData.m_owningBonus = 2;
        mineData.m_pointsToOwn = 3;
        mineData.m_resourceColor = new Color(.96f, .69f, .26f);
        HexResource.s_resourceMap[ResourceType.MINE] = mineData;

        ResourceData factoryData = new ResourceData();
        factoryData.m_allowsConstruction = true;
        factoryData.m_operatingBonus = 0;
        factoryData.m_owningBonus = 0;
        factoryData.m_pointsToOwn = 3;
        factoryData.m_resourceColor = new Color(1f, 1f, 1f);
        factoryData.m_resourceShape = Shape.TRIANGLE;
        HexResource.s_resourceMap[ResourceType.FACTORY] = factoryData;
    }

    private void CreateTeams()
    {
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
    }

    private void CreateUnitBlueprints()
    {
        s_unitBlueprints = new List<Unit>();

        Unit scoutBlueprint = GenerateBlueprintUnit(UnitIdentity.SCOUT);
        s_unitBlueprints.Add(scoutBlueprint);

        Unit shockBlueprint = GenerateBlueprintUnit(UnitIdentity.SHOCKTROOPER);
        s_unitBlueprints.Add(shockBlueprint);

        Unit sniperBlueprint = GenerateBlueprintUnit(UnitIdentity.SNIPER);
        s_unitBlueprints.Add(sniperBlueprint);

        Unit artilleryBlueprint = GenerateBlueprintUnit(UnitIdentity.ARTILLERY);
        s_unitBlueprints.Add(artilleryBlueprint);

        Unit tankBlueprint = GenerateBlueprintUnit(UnitIdentity.TANK);
        s_unitBlueprints.Add(tankBlueprint);
    }

    public TileCoord GetTileCoordinateForCurrentMousePosition()
    {
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickPos -= transform.position;
        TileCoord coord = GetTileCoordinateFromWorldPosition(clickPos);
        return coord;
    }

    public float Distance(TileCoord a, TileCoord b)
    {
        HexTile aH = m_grid[a.x, a.y];
        HexTile bH = m_grid[b.x, a.y];

        Vector3 cubeA = CubeCoordinatesFromWorldPosition(aH.m_worldCenterPos);
        Vector3 cubeB = CubeCoordinatesFromWorldPosition(bH.m_worldCenterPos);

        return CubeDistance(cubeA, cubeB);
    }

    /* @AI_Helpers
    AI helper functions start here
    */


    /*
    Finds expected combat result
    Damage Dealt - Damage Taken
    */
    public int GetAttackValue(Unit unitToCheck, Unit enemyUnit)
    {
        int positiveValue = 0;
        int negativeValue = 0;

        TileType attackerTerrain = m_grid[unitToCheck.m_position.x, unitToCheck.m_position.y].m_type;
        int attackerTerrainDefense = m_tileDefinitions[attackerTerrain].m_defense;

        TileType defenderTerrain = m_grid[enemyUnit.m_position.x, enemyUnit.m_position.y].m_type;
        int defenderTerrainDefense = m_tileDefinitions[defenderTerrain].m_defense;

        positiveValue = enemyUnit.CalculateDamageTaken(unitToCheck.m_armorPenetration, unitToCheck.m_attackPower, attackerTerrainDefense);

        if (enemyUnit.m_team == unitToCheck.m_team) //Hitting allies with AoE is bad...
            positiveValue *= -1;

        if (positiveValue < enemyUnit.m_hp && enemyUnit.IsInAttackRange(unitToCheck.m_position)) // Retaliation damage is only taken into account if the attack wouldn't destroy the enemy unit
        {
            negativeValue = Mathf.FloorToInt(enemyUnit.m_retaliationDamageFraction * unitToCheck.CalculateDamageTaken(enemyUnit.m_armorPenetration, enemyUnit.m_armorPenetration, defenderTerrainDefense));
        }

        return positiveValue - negativeValue;
    }

    /*

    */

    public int CheckAreaAtTarget(Unit attacker, TileCoord target)
    {
        int targetLocationValue = 0;

        foreach (TileCoord attackLocations in attacker.m_displacementsForShape)
        {
            TileCoord newCoord = new TileCoord(target.x + attackLocations.x, target.y + attackLocations.y);
            if (newCoord.x >= 0 && newCoord.y >= 0 && newCoord.x < gridWidthInHexes && newCoord.y < gridHeightInHexes)
            {
                HexTile checkTile = m_grid[newCoord.x, newCoord.y];
                if (checkTile.m_unit != null)
                {
                    targetLocationValue += GetAttackValue(attacker, checkTile.m_unit);
                }
            }
        }
        return targetLocationValue;
    }

    public IEnumerator Sleep(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    /*
    Finds the attack target position with the best combat result 
    BRUTE FORCE -- VERY SLOW
    */

    public AttackSimulationResult GetBestAttackTarget(Unit attacker)
    {
        TileCoord maxAttackTargetLocation = new TileCoord(-1, -1);
        int maxAttackTargetValue = -1;

        TileCoord attackerLoc = attacker.m_position;
        TileCoord attackerPos = attacker.m_position;

        //Simulates an attack on each tile coordinate in attacker's attack range, and accumulates the values from AoE attacks
        for (int x = (attackerPos.x - attacker.m_attackRangeMax); x <= (attackerPos.x + attacker.m_attackRangeMax); x++)
        {
            for (int y = (attackerPos.y - attacker.m_attackRangeMax); y <= (attackerPos.y + attacker.m_attackRangeMax); y++)
            {
                if (x >= 0 && x < gridWidthInHexes && y >= 0 && y < gridHeightInHexes)
                {
                    TileCoord tileTargeted = new TileCoord(x, y);

                    int targetLocationValue = CheckAreaAtTarget(attacker, tileTargeted);
                    if (targetLocationValue > maxAttackTargetValue)
                    {
                        maxAttackTargetValue = targetLocationValue;
                        maxAttackTargetLocation = tileTargeted;
                    }
                }
            }
        }

        AttackSimulationResult ar;
        ar.m_coordinate = maxAttackTargetLocation;
        ar.m_utility = maxAttackTargetValue;
        return ar;
    }


}
