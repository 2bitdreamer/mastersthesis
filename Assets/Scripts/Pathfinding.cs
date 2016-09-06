using UnityEngine;
using System.Collections;
using Priority_Queue;
using System.Collections.Generic;


public class Pathfinding : MonoBehaviour
{
    private HexGrid m_hexGridRef;

    void Start()
    {
        m_hexGridRef = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
    }

    public PathingInfo DijkstraSearch(TileCoord start)
    {
        int maxQueueSize = m_hexGridRef.gridWidthInHexes * m_hexGridRef.gridHeightInHexes;
        FastPriorityQueue<HexTile> frontier = new FastPriorityQueue<HexTile>(maxQueueSize);
        HexTile nodeAtStart = m_hexGridRef.m_grid[start.x, start.y];

        FastPriorityQueue<HexTile> unvistedNodes = new FastPriorityQueue<HexTile>(maxQueueSize);
        int len = m_hexGridRef.gridWidthInHexes;
        int width = m_hexGridRef.gridHeightInHexes;

        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < width; j++)
            {
                unvistedNodes.Enqueue(m_hexGridRef.m_grid[i, j], double.MaxValue);
            }
        }
        unvistedNodes.UpdatePriority(nodeAtStart, 0);
        nodeAtStart.m_currentPathfindingCost = 0;

        Dictionary<HexTile, HexTile> cameFrom = new Dictionary<HexTile, HexTile>();

        cameFrom[nodeAtStart] = null;

        while (unvistedNodes.Count > 0)
        {
            HexTile currentHexTile = unvistedNodes.Dequeue();
            frontier.Enqueue(currentHexTile, currentHexTile.Priority);

            List<HexTile> neighbors = m_hexGridRef.GetNeighbors(currentHexTile);
            foreach (HexTile h in neighbors)
            {
                //Debug.Log(h.m_type);
                if (h.m_unit != null)
                {
                    //Debug.Log(h.m_unit.m_name);
                }
                else
                {
                    //Debug.Log("h.m_unit was null for tile " + h.m_tileCoord.x + "," + h.m_tileCoord.y);
                }

                bool isNotInFrontier = !(frontier.Contains(h));
                TileDefinition td = m_hexGridRef.GetTileDefinition(h.m_type);
                bool isTileDefinitionNotSolid = !td.m_isSolid;
                bool tileUnitNull = (h.m_unit == null);
                bool nodeAtStartNull = nodeAtStart == null;
                Unit unitAt = h.m_unit;
                int unitTeam = (unitAt != null) ? unitAt.m_team : -1;
                Unit unitAtStart = nodeAtStart.m_unit;
                int nodeAtStartTeam = (unitAtStart != null) ? unitAtStart.m_team : -1;
                bool unitMyTeamOrNull = tileUnitNull || (unitTeam == nodeAtStartTeam);

                if (isNotInFrontier && isTileDefinitionNotSolid && unitMyTeamOrNull)
                {
                    double oldCost = h.Priority;
                    double newCost = currentHexTile.Priority + m_hexGridRef.GetCost(currentHexTile, h, start);
                    if (newCost < oldCost)
                    {
                        cameFrom[h] = currentHexTile;
                        currentHexTile.m_currentPathfindingCost = newCost;
                        unvistedNodes.UpdatePriority(h, newCost);
                    }
                }
            }
        }

        return new PathingInfo(frontier, cameFrom);
    }
}


public class PathingInfo
{

    public FastPriorityQueue<HexTile> m_orderedHexTiles;
    public Dictionary<HexTile, HexTile> m_cameFrom;
    public PathingInfo (FastPriorityQueue<HexTile> orderedHexTiles, Dictionary<HexTile, HexTile> cameFrom)
    {
        m_orderedHexTiles = orderedHexTiles;
        m_cameFrom = cameFrom;
    }
}


