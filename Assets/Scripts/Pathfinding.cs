using UnityEngine;
using System.Collections;
using Priority_Queue;
using System.Collections.Generic;


public class Pathfinding : MonoBehaviour
 {
    private HexGrid m_hexGridRef;
    FastPriorityQueue<GridNode> m_frontier;


    void Initialize()
    {
        m_hexGridRef = GameObject.FindGameObjectWithTag("HexGrid").GetComponent<HexGrid>();
    }

    void DijkstraSearch(TileCoord start, TileCoord goal)
    {
        HexTile tileAtStart = m_hexGridRef.m_grid[start.x, start.y];
        HexTile goalTile = m_hexGridRef.m_grid[goal.x, goal.y];

        GridNode nodeAtStart = new GridNode(tileAtStart);
        m_frontier.Enqueue(nodeAtStart, 0);

        Dictionary<GridNode, GridNode> cameFrom = new Dictionary<GridNode, GridNode>();
        Dictionary<GridNode, int> costSoFar = new Dictionary<GridNode, int>();

        cameFrom[nodeAtStart] = null;
        costSoFar[nodeAtStart] = 0;

        while (m_frontier.Count > 0)
        {
            GridNode current = m_frontier.Dequeue();
            HexTile currentHexTile = current.m_hexTile;

            if (currentHexTile.m_worldCenterPos == goalTile.m_worldCenterPos)
                break;

            List<HexTile> neighbors = m_hexGridRef.GetNeighbors(currentHexTile);
            foreach(HexTile h in neighbors)
            {
                int newCost = costSoFar[current] + m_hexGridRef.GetCost(currentHexTile, h);
            }
 
        }
    }

    /*
    frontier = PriorityQueue()
frontier.put(start, 0)
came_from = {}
cost_so_far = {}
came_from[start] = None
cost_so_far[start] = 0

while not frontier.empty():
   current = frontier.get()

   if current == goal:
      break
   
   for next in graph.neighbors(current):
      new_cost = cost_so_far[current] + graph.cost(current, next)
      if next not in cost_so_far or new_cost < cost_so_far[next]:
         cost_so_far[next] = new_cost
         priority = new_cost
         frontier.put(next, priority)
         came_from[next] = current
    */
}
