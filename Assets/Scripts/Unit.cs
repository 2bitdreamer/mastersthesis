﻿using UnityEngine;
using Priority_Queue;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class Unit : MonoBehaviour {
    public TileCoord m_position;
    public char m_displayCharacter;
    public Font m_font;
    public Material m_material;
    public TextMesh m_textMesh;
    public bool m_selected;
    public bool m_attemptingAttack = false;
    public bool m_hasAttacked;
    public Vector3 m_charHalfSize;
    public List<HexTile> m_tilesInRange;
    public int m_team;
    public float m_upkeep;
    public bool m_hasAction = true;
    
    public int m_movementRange;
    public int m_movesRemaining;

    //stats
    public int m_hp;
    public int m_attackRangeMin;
    public int m_cost;
    public int m_attackRangeMax;
    public int m_attackPower;
    public int m_defense;
    public int m_armorPenetration;

    public int m_roughTerrainMovesModifier;
    public bool m_canAttackAfterMoving;
    public float m_movesLeftAfterAttackingFraction;
    public int m_captureTurnReduction;
    public int m_captureRatePerTurn;

    public List<TileCoord> m_displacementsForShape;
    public bool m_hasSpecial;
    public string  m_specialAbilityName;
    public float m_operatingIncomeFraction;
    public float m_retaliationDamageFraction;

    private HexGrid m_hexGridRef;


    public Unit()
    {
        m_roughTerrainMovesModifier = 0;
        m_movesLeftAfterAttackingFraction = 0f;
        m_operatingIncomeFraction = 1f;
        m_canAttackAfterMoving = true; //#TODO: wanted to make this an int, number of attacks after moving (to allow silly units), but nontrivial in current setup
        m_captureRatePerTurn = 1;
        m_retaliationDamageFraction = 1f;
    }

    public void Initialize(Team team)
    {
        m_team = team.m_number;
        m_font = Resources.Load<Font>("FreeSans");
        m_material = Resources.Load<Material>("HexGridMaterial");

        m_selected = false;
        
        m_hexGridRef = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
        m_tilesInRange = new List<HexTile>();
        m_team = team.m_number;

        m_textMesh = gameObject.GetComponent<TextMesh>();
        m_textMesh.characterSize = .25f * m_hexGridRef.TILE_SIZE;
        m_textMesh.font = m_font;
        m_textMesh.fontSize = 60;
        m_textMesh.text = m_displayCharacter.ToString();
        m_textMesh.color = team.m_color;

        // MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        //meshRenderer.material = m_material;
    }

    public void SetPosition(TileCoord pos)
    {
        transform.position = m_hexGridRef.m_grid[pos.x, pos.y].m_worldCenterPos;
        Bounds bounds = m_textMesh.GetComponent<Renderer>().bounds;
        m_charHalfSize = bounds.extents;
        transform.position += new Vector3(-(m_charHalfSize.x), m_charHalfSize.y * .9f, 0f);
    }

    public void InitializeScout(TileCoord pos, Team team)
    {
        List<TileCoord> displacementsForShape = new List<TileCoord>();

        displacementsForShape.Add(new TileCoord(0, 0));
        /*displacementsForShape.Add(new TileCoord(-1, 0));
        displacementsForShape.Add(new TileCoord(-1, 1));
        displacementsForShape.Add(new TileCoord(0, 1));
        displacementsForShape.Add(new TileCoord(1, 0));
        displacementsForShape.Add(new TileCoord(1, -1));
        displacementsForShape.Add(new TileCoord(0, -1));*/

        m_position = pos;
        m_hp = 3;
        m_attackPower = 2;
        m_defense = 0;
        m_armorPenetration = 1;
        m_movementRange = 6;
        m_movesRemaining = m_movementRange;

        m_upkeep = 1;
        m_cost = 4;

        m_displayCharacter = '!';
        m_displacementsForShape = displacementsForShape;
        m_attackRangeMin = 1;
        m_attackRangeMax = 3;
        m_captureTurnReduction = 0;
        m_roughTerrainMovesModifier = -1;
        m_movesLeftAfterAttackingFraction = 1f;

        Initialize(team);

        if (pos.x >= 0)
        {
            SetPosition(pos);
        }

       
    }


    public void InitializeShocktrooper(TileCoord pos, Team team)
    {
        Initialize(team);

        List<TileCoord> displacementsForShape = new List<TileCoord>();
        displacementsForShape.Add(new TileCoord(0, 0));

        m_position = pos;
        m_hp = 5;
        m_attackPower = 4;
        m_defense = 2;
        m_armorPenetration = 1;
        m_movementRange = 4;
        m_movesRemaining = m_movementRange;

        m_upkeep = 1;
        m_cost = 6;

        m_displayCharacter = '#';
        m_displacementsForShape = displacementsForShape;
        m_attackRangeMin = 1;
        m_attackRangeMax = 2;
        m_captureTurnReduction = 1;

        Initialize(team);

        if (pos.x >= 0)
        {
            SetPosition(pos);
        }

    }


    public void InitializeArtillery(TileCoord pos, Team team)
    {
        Initialize(team);

        List<TileCoord> displacementsForShape = new List<TileCoord>();
        displacementsForShape.Add(new TileCoord(0, 0)); //2,5
        displacementsForShape.Add(new TileCoord(0, 1)); //2,6
        displacementsForShape.Add(new TileCoord(0, 2)); //2,7
        displacementsForShape.Add(new TileCoord(-1, 1)); //1,6
        displacementsForShape.Add(new TileCoord(-1, 2)); //1,7
        displacementsForShape.Add(new TileCoord(-2, 2)); //0,7

        m_position = pos;
        m_hp = 7;
        m_attackPower = 4;
        m_defense = 2;
        m_armorPenetration = 2;
        m_movementRange = 4;
        m_movesRemaining = m_movementRange;

        m_upkeep = 1;
        m_cost = 5;

        m_displayCharacter = '%';
        m_displacementsForShape = displacementsForShape;
        m_attackRangeMin = 2;
        m_attackRangeMax = 6;
        m_captureTurnReduction = 0;
        m_roughTerrainMovesModifier = 1;
        m_operatingIncomeFraction = 0f;
        m_captureRatePerTurn = 0;
        m_canAttackAfterMoving = false;
        m_retaliationDamageFraction = 0f;

        Initialize(team);

        if (pos.x >= 0)
        {
            SetPosition(pos);
        }
    }

    public void InitializeSniper(TileCoord pos, Team team)
    {
        Initialize(team);

        List<TileCoord> displacementsForShape = new List<TileCoord>();
        displacementsForShape.Add(new TileCoord(0, 0));

        m_position = pos;
        m_hp = 2;
        m_attackPower = 5;
        m_defense = 0;
        m_armorPenetration = 3;
        m_movementRange = 3;
        m_movesRemaining = m_movementRange;

        m_upkeep = 1;
        m_cost = 5;

        m_displayCharacter = '>';
        m_displacementsForShape = displacementsForShape;
        m_attackRangeMin = 1;
        m_attackRangeMax = 5;
        m_captureTurnReduction = 0;

        Initialize(team);

        if (pos.x >= 0)
        {
            SetPosition(pos);
        }
    }

    public void InitializeTank(TileCoord pos, Team team)
    {
        Initialize(team);

        List<TileCoord> displacementsForShape = new List<TileCoord>();
        displacementsForShape.Add(new TileCoord(0, 0)); //4,4
        displacementsForShape.Add(new TileCoord(-1, 0));
        displacementsForShape.Add(new TileCoord(-1, 1));
        displacementsForShape.Add(new TileCoord(0, 1));
        displacementsForShape.Add(new TileCoord(1, 0));
        displacementsForShape.Add(new TileCoord(1, -1));
        displacementsForShape.Add(new TileCoord(0, -1));

        displacementsForShape.Add(new TileCoord(-2, 2));
        displacementsForShape.Add(new TileCoord(0, 2));
        displacementsForShape.Add(new TileCoord(2, 0));
        displacementsForShape.Add(new TileCoord(2, -2));
        displacementsForShape.Add(new TileCoord(0, -2));
        displacementsForShape.Add(new TileCoord(-2, 0));


        m_position = pos;
        m_hp = 12;
        m_attackPower = 3;
        m_defense = 4;
        m_armorPenetration = 3;
        m_movementRange = 5;
        m_movesRemaining = m_movementRange;

        m_upkeep = 3;
        m_cost = 11;

        m_displayCharacter = '8';
        m_displacementsForShape = displacementsForShape;
        m_attackRangeMin = 1;
        m_attackRangeMax = 4;
        m_captureTurnReduction = 0;
        m_roughTerrainMovesModifier = 2;
        m_operatingIncomeFraction = 0f;
        m_captureRatePerTurn = 0;

        Initialize(team);

        if (pos.x >= 0)
        {
            SetPosition(pos);
        }
    }

    public void Move(TileCoord coord)
    {
        m_position = coord;
        transform.position = m_hexGridRef.m_grid[m_position.x, m_position.y].m_worldCenterPos;
        transform.position += new Vector3(-m_textMesh.characterSize / 2f, m_textMesh.characterSize / 2f, 0f);

        transform.position = m_hexGridRef.m_grid[m_position.x, m_position.y].m_worldCenterPos;
        Bounds bounds = m_textMesh.GetComponent<Renderer>().bounds;
        m_charHalfSize = bounds.extents;
        transform.position += new Vector3(-(m_charHalfSize.x), m_charHalfSize.y * .9f, 0f);
    }
    // Use this for initialization
    void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void ActionMenuOpened()
    {
        if (m_hasSpecial)
        {
            GameObject g = m_hexGridRef.m_specialText;
            Text t = g.GetComponent<Text>();
            t.text = m_specialAbilityName;
            Button b = g.GetComponent<Button>();
            b.interactable = true;
        }
        else
        {
            GameObject g = m_hexGridRef.m_specialText;
            Text t = g.GetComponent<Text>();
            t.text = "Special Ability (None Available)";
            g = m_hexGridRef.m_specialText;
            Button b = g.GetComponent<Button>();
            b.interactable = false;
        }
    } 
    
    public virtual void OnAttackPressed()
    {
        m_tilesInRange.Clear();
        HexTile[,] grid = m_hexGridRef.m_grid;

        foreach(HexTile ht in grid)
        {
            ht.m_currentColor = ColorType.RENDER;
        }

        HexTile attackTile = grid[m_position.x, m_position.y];
        Vector3 attackCubeCord = m_hexGridRef.CubeCoordinatesFromWorldPosition(attackTile.m_worldCenterPos);
        
        for (int x = (m_position.x - m_attackRangeMax); x <= (m_position.x + m_attackRangeMax); x++)
        {
            for (int y = (m_position.y - m_attackRangeMax); y <= (m_position.y + m_attackRangeMax); y++)
            {
                if (x >= 0 && x < m_hexGridRef.gridWidthInHexes && y >= 0 && y < m_hexGridRef.gridHeightInHexes)
                {
                    HexTile t = grid[x, y];
                    bool isInRange = IsInRange(new TileCoord(x,y));

                    if (isInRange)
                    {
                        t.m_currentColor = ColorType.ATTACKRANGE;
                    }
                }
            }
        }

        m_attemptingAttack = true;

        /*m_hasAction = false;
        m_movesRemaining = 0;
        */
    }

    public virtual void OnSpecialAbilityPressed()
    {
        m_hasAction = false;
        m_movesRemaining = 0;
        m_tilesInRange.Clear();
    }

    public bool IsInRange(TileCoord clickedCoord)
    {
        HexTile hexTile = m_hexGridRef.m_grid[clickedCoord.x, clickedCoord.y];
        Vector3 c = m_hexGridRef.CubeCoordinatesFromWorldPosition(hexTile.m_worldCenterPos);

        HexTile myTile = m_hexGridRef.m_grid[m_position.x, m_position.y];
        Vector3 c2 = m_hexGridRef.CubeCoordinatesFromWorldPosition(myTile.m_worldCenterPos);

        float cubeDistance = CubeDistance(c, c2);
        bool isInRange = (Mathf.Approximately(cubeDistance, m_attackRangeMin)) || (Mathf.Approximately(cubeDistance, m_attackRangeMax));
        isInRange = isInRange || (cubeDistance >= m_attackRangeMin && cubeDistance <= m_attackRangeMax);
        return isInRange;
    }

    public void DoDamage(TileCoord clickCoord)
    {
        HexTile[,] grid = m_hexGridRef.m_grid;
        bool isInRange = IsInRange(clickCoord);
        bool unitInRange = false;

        if (isInRange)
        {
            foreach (TileCoord ht in m_displacementsForShape)
            {
                TileCoord newCoord = new TileCoord(clickCoord.x + ht.x, clickCoord.y + ht.y);
                if (newCoord.x >= 0 && newCoord.y >= 0 && newCoord.x < m_hexGridRef.gridWidthInHexes && newCoord.y < m_hexGridRef.gridHeightInHexes)
                {
                    Unit u = grid[newCoord.x, newCoord.y].m_unit;
                    if (u != null)  
                    {
                        unitInRange = true;
                        int remainingHP = u.TakeDamage(m_armorPenetration, m_attackPower);
                        if (remainingHP > 0)
                        {
                            CheckDefenderRetaliation(u, m_position);
                        }
                    }
                }
            }
        }
        if (unitInRange)
        {
            m_hasAction = false;
            m_movesRemaining = Mathf.FloorToInt(m_movesLeftAfterAttackingFraction * m_movesRemaining);
            m_tilesInRange.Clear();

            if (m_movesRemaining == 0)
            {
                m_hexGridRef.HandleEndOfUnitsAction(this);
            }
        }
    }

    /*
    public List<TileCoord> GenerateCircularDisplacements(int radius)
    {
        List<TileCoord> disps = new List<TileCoord>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == -radius && y == -radius)
                    continue;
                if (x == radius && y == radius)
                    continue;
                disps.Add(new TileCoord(x, y));
            }
        }
        return disps;
    }
    */

    public int TakeDamage(int ap, int dmg)
    {
        TileType tileType = m_hexGridRef.m_grid[m_position.x, m_position.y].m_type;
        int terrainDefense = m_hexGridRef.m_tileDefinitions[tileType].m_defense;
        Debug.Log("Terrain mitigates damage by " + terrainDefense); 

        int totalDamage = Mathf.Max((dmg - Mathf.Max((m_defense + terrainDefense - ap), 0)), 1); //Change this to be more readable
        Debug.Log("Unit took " + totalDamage);
        m_hp -= totalDamage;

        if(m_hp <= 0)
        {
            m_hexGridRef.m_grid[m_position.x, m_position.y].m_unit = null;
            m_hexGridRef.m_teams[m_team].RemoveUnit(this);

            Team team = m_hexGridRef.m_teams[m_team];
            if (team.m_units.Count == 0)
            {
                switch (team.m_number)
                {
                    case 0:
                        m_hexGridRef.m_gameState = GameState.PLAYER_TWO_WINS_ANNIHILATION;
                        break;
                    case 1:
                        m_hexGridRef.m_gameState = GameState.PLAYER_ONE_WINS_ANNIHILATION;
                        break;
                }
            }

            //m_hexGridRef.HandleEndOfUnitsAction(this);
            Destroy(this.gameObject);
            return 0;
        }
        return m_hp;
    }

    private float CubeDistance(Vector3 a, Vector3 b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2f;
    }


    private void CheckDefenderRetaliation(Unit defender, TileCoord attackerPos)
    {
        HexTile[,] grid = m_hexGridRef.m_grid;
        bool isInRange = defender.IsInRange(attackerPos);

        if (isInRange)
        {
            foreach (TileCoord ht in m_displacementsForShape)
            {
                TileCoord newCoord = new TileCoord(attackerPos.x + ht.x, attackerPos.y + ht.y);
                if (newCoord.x >= 0 && newCoord.y >= 0 && newCoord.x < m_hexGridRef.gridWidthInHexes && newCoord.y < m_hexGridRef.gridHeightInHexes)
                {
                    HexTile hexTileAt = grid[newCoord.x, newCoord.y];
                    Unit u = hexTileAt.m_unit;
                    if (u != null)
                    {
                        int modifiedRetaliationDamage = Mathf.FloorToInt(defender.m_attackPower * defender.m_retaliationDamageFraction);
                        u.TakeDamage(defender.m_armorPenetration, modifiedRetaliationDamage);
                        Debug.Log("Retaliation damage dealt with a base power of " + modifiedRetaliationDamage);
                    }
                }
            }
        }

    }

}
