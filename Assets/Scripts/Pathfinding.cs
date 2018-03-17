using System.Collections.Generic;
using UnityEngine;
public static class Pathfinding {
    
    /// <summary>
    /// Finds path on 1 floor grid.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    public static GridSlot[] FindPathAStar(GridSlot start, GridSlot goal) {
        if (start == null || goal == null) {
            Debug.Log("FindPathAStar: start or goal is null.");
            return new GridSlot[0];
        }
        if (start == goal) {
            Debug.Log("goal is same as cur pos");
            return new GridSlot[0];
        }
        // The set of nodes already evaluated
        HashSet<GridSlot> closedSet = new HashSet<GridSlot>();


        // The set of currently discovered nodes that are not evaluated yet.
        // Initially, only the start node is known.
        HashSet<GridSlot> openSet = new HashSet<GridSlot>();
        openSet.Add(start);

        // For each node, which node it can most efficiently be reached from.
        // If a node can be reached from many nodes, cameFrom will eventually contain the
        // most efficient previous step.
        Dictionary<GridSlot, GridSlot> cameFrom = new Dictionary<GridSlot, GridSlot>();

        // For each node, the cost of getting from the start node to that node.
        // init as inifinity
        Dictionary<GridSlot, float> gScore = new Dictionary<GridSlot, float>();

        // For each node, the total cost of getting from the start node to the goal
        // by passing by that node. That value is partly known, partly heuristic.
        //map with default value of Infinity
        Dictionary<GridSlot, float> fScore = new Dictionary<GridSlot, float>();
        List<GridSlot> allSlots = GridSlot.allSlots;
        for (int i = 0; i < allSlots.Count; i++) {
            gScore.Add(allSlots[i], float.PositiveInfinity);
            fScore.Add(allSlots[i], float.PositiveInfinity);
            cameFrom.Add(allSlots[i], null);
        }

        // The cost of going from start to start is zero.
        gScore[start] = 0f;

        // For the first node, that value is completely heuristic.
        fScore[start] = HeuristicCostEstimate(start, goal);
        while (openSet.Count > 0) {
            // the node in openSet having the lowest fScore[] value
            GridSlot current = null;
            float minFScore = float.MaxValue;
            foreach (var slot in openSet) {
                if (fScore[slot] < minFScore) {
                    minFScore = fScore[slot];
                    current = slot;
                }
            }
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            // Assumes 1 layered grid.
            GridSlot[] curNeighbors = new GridSlot[4]{// 4 neighbouring slots.
                GridSlot.GetSlotById(current.id+1),
                GridSlot.GetSlotById(current.id-1),
                GridSlot.GetSlotById(current.id-20),
                GridSlot.GetSlotById(current.id+20),
            };

            foreach (var neighbor in curNeighbors) {
                if (neighbor == null)
                    continue;
                if (closedSet.Contains(neighbor))
                    continue;       // Ignore the neighbor which is already evaluated.

                if (!openSet.Contains(neighbor)) // Discover a new node
                    openSet.Add(neighbor);

                // The distance from start to a neighbor
                //the "dist_between" function may vary as per the solution requirements.
                float tentative_gScore = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);
                if (tentative_gScore >= gScore[neighbor])
                    continue;        // This is not a better path.

                // This path is the best until now. Record it!
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentative_gScore;
                fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
            }
        }
        return new GridSlot[0];
    }

    private static float HeuristicCostEstimate(GridSlot neighbor, GridSlot goal) {
        return goal.transform.position.x - neighbor.transform.position.x
             + goal.transform.position.y - neighbor.transform.position.y;
    }

    private static GridSlot[] ReconstructPath(Dictionary<GridSlot, GridSlot> cameFrom, GridSlot current) {
        int errLen = 10000;
        List<GridSlot> total_path = new List<GridSlot>();
        total_path.Add(current);
        while (current != null && cameFrom.ContainsKey(current) && errLen > 0) {
            errLen--;
            current = cameFrom[current];
            if (current != null)
                total_path.Add(current);
        }
        if (errLen <= 0) {
            Debug.Log("Error infi loop, cyclic path");
        }
        return total_path.ToArray();
    }

}
