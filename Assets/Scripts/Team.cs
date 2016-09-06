using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Team
{
    public List<Unit> m_units;
    public float m_income;
    public float m_upkeep;
    public float m_goldReserves;
    public Color m_color;
    public Color m_captureColor;
    public string m_name;
    public int m_number;
    public bool m_getIncomeAtStart;
    public bool m_isAI;

    private HexGrid m_hexGridRef;

    public void Initialize(float income, float incomeReserves, Color color, Color captureColor, string name, int number, bool getIncomeToStart = false)
    {
        m_income = income;
        m_goldReserves = incomeReserves;
        m_color = color;
        m_captureColor = captureColor;
        m_name = name;
        m_number = number;
        m_units = new List<Unit>();
        m_getIncomeAtStart = getIncomeToStart;
        m_hexGridRef = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
    }

    public void AddUnit(Unit u)
    {
        m_upkeep += u.m_upkeep;
        m_units.Add(u);
    }

    public Team()
    {
        m_number = -1;
    }

    public void RemoveUnit(Unit u)
    {
        m_upkeep -= u.m_upkeep;
        m_units.Remove(u);
    }

    public void RecieveIncome()
    {
        m_goldReserves += (m_income - m_upkeep);

        if (m_goldReserves < 0)
        {

            switch (m_number)
            {
                case 0:
                    m_hexGridRef.m_gameState = GameState.PLAYER_TWO_WINS_BANKRUPTCY;
                    break;
                case 1:
                    m_hexGridRef.m_gameState = GameState.PLAYER_ONE_WINS_BANKRUPTCY;
                    break;
            }
        }
    }

    /*
    AI FUNCTIONS WILL START HERE
    @AI
    */

    public bool FindAndAttack(Unit u)
    {
        bool anyInRange = false;

        foreach(Team t in m_hexGridRef.m_teams)
        {
            if (t.m_number == u.m_team)
                continue;

            foreach (Unit unit in t.m_units)
            {
                anyInRange = u.IsInAttackRange(unit.m_position);
                if (anyInRange)
                    break;
            }
        }

        if (!anyInRange)
            return false;


        AttackSimulationResult attackResult = m_hexGridRef.GetBestAttackTarget(u);
        if (attackResult.m_utility > 0)
        {
                u.DoDamage(attackResult.m_coordinate);
                return true;
        }

        return false;
    }


    public void DecideAIAction()
    {
        foreach (Unit u in m_units)
        {
            HexTile tileAt = m_hexGridRef.m_grid[u.m_position.x, u.m_position.y];

            if (tileAt.m_resource != null && tileAt.m_resource.m_owningTeam != u.m_team) //Sitting on an unowned resource? keep sitting on it..
            {
                continue;
            }

            bool successfulAttack = FindAndAttack(u); //Can I attack a unit in a way that makes it a decent trade-off

            if (!successfulAttack)
            {
                List<HexTile> canMoveTo = m_hexGridRef.GetValidMovableTiles(u);

                TileCoord maxAttackTargetLocation = new TileCoord(-1, -1);
                int maxAttackTargetValue = 0;
                TileCoord posToMoveTo = new TileCoord(-1, -1);

                //Do I have an OK target in my current move range
                foreach (HexTile t in canMoveTo)
                {
                    if (!u.m_canAttackAfterMoving)
                        break;

                    TileCoord movePos = t.m_tileCoord;

                    for (int x = (movePos.x - u.m_attackRangeMax); x <= (movePos.x + u.m_attackRangeMax); x++)
                    {
                        for (int y = (movePos.y - u.m_attackRangeMax); y <= (movePos.y + u.m_attackRangeMax); y++)
                        {
                            if (m_hexGridRef.IsInBounds(x, y))
                            {

                                TileCoord tileTargeted = new TileCoord(x, y);

                                int targetLocationValue = m_hexGridRef.CalculateAreaTargetUtility(u, tileTargeted);
                                if (targetLocationValue > maxAttackTargetValue)
                                {
                                    maxAttackTargetValue = targetLocationValue;
                                    maxAttackTargetLocation = tileTargeted;
                                    posToMoveTo = movePos;
                                }
                            }
                        }
                    }
                }

                if (posToMoveTo.x != -1)
                {
                    m_hexGridRef.StartCoroutine(m_hexGridRef.Sleep(1f));
                    u.Move(posToMoveTo);
                    u.DoDamage(maxAttackTargetLocation);
                }
                else if (canMoveTo.Count > 0)
                //No attack target in range, just move as far as you can
                {
                    u.Move(m_hexGridRef.GetBestMovementLocation(u));
                    //u.Move(canMoveTo[canMoveTo.Count - 1].m_tileCoord);
                }

            }
        }

        m_hexGridRef.StartCoroutine(m_hexGridRef.Sleep(1f));
        m_hexGridRef.Invoke("EndTurn", 1f);
    }
}
