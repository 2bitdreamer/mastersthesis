using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Team {
    public List<Unit> m_units;
    public float m_income;
    public float m_upkeep;
    public float m_incomeReserves;
    public Color m_color;
    public Color m_captureColor;
    public string m_name;
    public int m_number;
    public bool m_getIncomeAtStart;

    public void Initialize(float income, float incomeReserves, Color color, Color captureColor, string name, int number, bool getIncomeToStart = false)
    {
        m_income = income;
        m_incomeReserves = incomeReserves;
        m_color = color;
        m_captureColor = captureColor;
        m_name = name;
        m_number = number;
        m_units = new List<Unit>();
        m_getIncomeAtStart = getIncomeToStart;
    }

    public void AddUnit(Unit u)
    {
        m_upkeep += u.m_upkeep;
        m_units.Add(u);
    }

    public void RemoveUnit(Unit u)
    {
        m_upkeep -= u.m_upkeep;
        m_units.Remove(u);
    }

    public void RecieveIncome()
    {
        m_incomeReserves += (m_income - m_upkeep);
    }
}
