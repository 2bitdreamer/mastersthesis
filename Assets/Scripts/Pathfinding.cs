using UnityEngine;
using System.Collections;
using Priority_Queue;
using System.Collections.Generic;


public class Pathfinding : MonoBehaviour
{
    private TileCoord m_start = new TileCoord(0,0);
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
        int len = m_hexGridRef.m_grid.GetLength(0);
        int width = m_hexGridRef.m_grid.GetLength(1);

        for (int i = 0; i < len; i++)
        {
            for (int j = 0; j < width; j++)
            {
                unvistedNodes.Enqueue(m_hexGridRef.m_grid[i, j], double.MaxValue);
            }
        }
        unvistedNodes.UpdatePriority(nodeAtStart, 0);

        Dictionary<HexTile, HexTile> cameFrom = new Dictionary<HexTile, HexTile>();

        cameFrom[nodeAtStart] = null;

        while (unvistedNodes.Count > 0)
        {
            HexTile currentHexTile = unvistedNodes.Dequeue();
            frontier.Enqueue(currentHexTile, currentHexTile.Priority);

            List<HexTile> neighbors = m_hexGridRef.GetNeighbors(currentHexTile);
            foreach (HexTile h in neighbors)
            {

                if (!(frontier.Contains(h)) && !(m_hexGridRef.GetTileDefinition(h.m_type).m_isSolid)
                    && ((h.m_unit == null) || (h.m_unit.m_team == nodeAtStart.m_unit.m_team)))
                {
                    double oldCost = h.Priority;
                    double newCost = currentHexTile.Priority + m_hexGridRef.GetCost(currentHexTile, h, start);
                    if (newCost < oldCost)
                    {
                        cameFrom[h] = currentHexTile;
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


