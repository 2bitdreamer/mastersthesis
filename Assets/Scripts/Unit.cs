using UnityEngine;
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

    //stats
    public int m_movementRange;
    public int m_movesRemaining;
    public int m_hp;
    public int m_curHp;
    public int m_attackRangeMin;
    public int m_attackRangeMax;
    public int m_damage;
    public int m_defense;
    public int m_armorPenetration;
    public int m_pointsTowardsCapture;
    public List<TileCoord> m_displacementsForShape;
    public bool m_hasSpecial;
    public string  m_specialAbilityName;

    private HexGrid m_hexGridRef;


    public void Initialize(TileCoord pos, int moves, char displayChar, Team team, float upkeep, int hp, int attackRangeMin, int attackRangeMax, int damage, int defense, int armorPenetration, int pointsTowardsCapture, List<TileCoord> displacementsForShape, bool hasSpecial = false, string SpecialAbilityName = "")
    {
        //subclasses with a special ability nead to have true,abilityName at the end of the initialize call so that the action menu
        //displays the ability correctly

        m_font = Resources.Load<Font>("FreeSans");
        m_material = Resources.Load<Material>("HexGridMaterial");

        m_selected = false;
        m_position = pos;
        m_movementRange = moves;
        m_movesRemaining = m_movementRange;
        m_displayCharacter = displayChar;
        m_hexGridRef = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
        m_tilesInRange = new List<HexTile>();
        m_team = team.m_number;
        m_upkeep = upkeep;
        m_displacementsForShape = displacementsForShape;
        m_hasSpecial = hasSpecial;
        m_specialAbilityName = SpecialAbilityName;

        m_hp = hp;
        m_curHp = m_hp;
        m_attackRangeMin = attackRangeMin;
        m_attackRangeMax = attackRangeMax;
        m_damage = damage;
        m_defense = defense;
        m_armorPenetration = armorPenetration;
        m_pointsTowardsCapture = pointsTowardsCapture;

        m_textMesh = gameObject.GetComponent<TextMesh>();
        m_textMesh.characterSize = .25f * m_hexGridRef.TILE_SIZE;
        m_textMesh.font = m_font;
        m_textMesh.fontSize = 60;
        m_textMesh.text = m_displayCharacter.ToString();
        m_textMesh.color = team.m_color;
        

        transform.position = m_hexGridRef.m_grid[m_position.x, m_position.y].m_worldCenterPos;
        Bounds bounds =  m_textMesh.GetComponent<Renderer>().bounds;
        m_charHalfSize = bounds.extents;
        transform.position += new Vector3(-(m_charHalfSize.x), m_charHalfSize.y *.9f, 0f);

        // MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        //meshRenderer.material = m_material;
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
                    Vector3 c = m_hexGridRef.CubeCoordinatesFromWorldPosition(t.m_worldCenterPos);
                    float distance = Mathf.Max(Mathf.Abs(c.x - attackCubeCord.x), Mathf.Abs(c.y - attackCubeCord.y), Mathf.Abs(c.z - attackCubeCord.z));
                    bool isInRange = (Mathf.Approximately(distance, m_attackRangeMin)) || (Mathf.Approximately(distance, m_attackRangeMax));
                    isInRange = isInRange || (distance >= m_attackRangeMin && distance <= m_attackRangeMax);

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

    public void DoDamage(Vector3 clickPos)
    {
        HexTile[,] grid = m_hexGridRef.m_grid;
        TileCoord clickCoord = m_hexGridRef.GetTileCoordinateFromWorldPosition(clickPos);
        Vector3 c = m_hexGridRef.CubeCoordinatesFromWorldPosition(clickPos);
        HexTile myTile = grid[m_position.x, m_position.y];
        Vector3 c2 = m_hexGridRef.CubeCoordinatesFromWorldPosition(myTile.m_worldCenterPos);

        float distance = Mathf.Max(Mathf.Abs(c.x - c2.x), Mathf.Abs(c.y - c2.y), Mathf.Abs(c.z - c2.z));
        bool isInRange = (Mathf.Approximately(distance, m_attackRangeMin)) || (Mathf.Approximately(distance, m_attackRangeMax));
        isInRange = isInRange || (distance >= m_attackRangeMin && distance <= m_attackRangeMax);
        bool unitInRange = false;

        if (isInRange)
        {
            foreach(TileCoord ht in m_displacementsForShape)
            {
                TileCoord newCoord = new TileCoord(clickCoord.x + ht.x, clickCoord.y + ht.y);
                if (newCoord.x >= 0 && newCoord.y >= 0 && newCoord.x < m_hexGridRef.gridWidthInHexes && newCoord.y < m_hexGridRef.gridHeightInHexes)
                {
                    Unit u = grid[newCoord.x, newCoord.y].m_unit;
                    if (u != null)
                    {
                        unitInRange = true;
                        u.TakeDamage(m_armorPenetration, m_damage);
                    }
                }
            }
        }
        if(unitInRange)
        {
            m_hasAction = false;
            m_movesRemaining = 0;
            m_tilesInRange.Clear();
            m_hexGridRef.HandleEndOfUnitsAction(this);
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

    public void TakeDamage(int ap, int dmg)
    {
        m_hp -= Mathf.Max((dmg - Mathf.Max((m_defense - m_armorPenetration), 0)),1);

        if(m_hp <= 0)
        {
            m_hexGridRef.m_grid[m_position.x, m_position.y].m_unit = null;
            m_hexGridRef.m_teams[m_team].RemoveUnit(this);
            DestroyImmediate(this.gameObject);
        }
    }
}
