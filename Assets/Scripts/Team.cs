using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Team {
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

    //this is only ever used if the team is AI.
    public void DecideAIAction()
    {

        m_hexGridRef.Invoke("EndTurn",2f);
    }
}
