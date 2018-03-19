using System;
using System.Collections.Generic;
using UnityEngine;
public static class Pathfinding {
    
    /// <summary>
    /// Finds path on 1 floor grid.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    public static MapNode[] FindPathAStar(Vector3 startPos, Vector3 goalPos, List<MapNode> allSlots) {
        /*if (start == null || goal == null) {
            Debug.Log("FindPathAStar: start or goal is null.");
            return new MapNode[0];
        }*/
        if (startPos == goalPos) {
            Debug.Log("goal is same as cur pos");
            return new MapNode[0];
        }
        MapNode start = MapNode.FindNode(startPos);
        MapNode goal = MapNode.FindNode(goalPos);

        // The set of nodes already evaluated
        HashSet<MapNode> closedSet = new HashSet<MapNode>();

        // The set of currently discovered nodes that are not evaluated yet.
        // Initially, only the start node is known.
        HashSet<MapNode> openSet = new HashSet<MapNode>();
        openSet.Add(start);

        // For each node, which node it can most efficiently be reached from.
        // If a node can be reached from many nodes, cameFrom will eventually contain the
        // most efficient previous step.
        Dictionary<MapNode, MapNode> cameFrom = new Dictionary<MapNode, MapNode>();

        // For each node, the cost of getting from the start node to that node.
        // init as inifinity
        Dictionary<MapNode, float> gScore = new Dictionary<MapNode, float>();

        // For each node, the total cost of getting from the start node to the goal
        // by passing by that node. That value is partly known, partly heuristic.
        //map with default value of Infinity
        Dictionary<MapNode, float> fScore = new Dictionary<MapNode, float>();
        for (int i = 0; i < allSlots.Count; i++) {
            gScore.Add(allSlots[i], float.PositiveInfinity);
            fScore.Add(allSlots[i], float.PositiveInfinity);
            cameFrom.Add(allSlots[i], new MapNode(Vector3.one * -1000));
        }

        // The cost of going from start to start is zero.
        gScore[start] = 0f;

        // For the first node, that value is completely heuristic.
        fScore[start] = HeuristicCostEstimate(start, goal);
        while (openSet.Count > 0) {
            // the node in openSet having the lowest fScore[] value
            MapNode current = null;//Vector3.zero; // grid slot
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
            List<MapNode> curNeighbors = new List<MapNode>();
            AddItem(current.id+1, curNeighbors, allSlots);
            AddItem(current.id-1, curNeighbors, allSlots);
            AddItem(current.id-20, curNeighbors, allSlots);
            AddItem(current.id+20, curNeighbors, allSlots);

            foreach (var neighbor in curNeighbors) {
                if (neighbor == null)
                    continue;
                if (closedSet.Contains(neighbor))
                    continue;       // Ignore the neighbor which is already evaluated.

                if (!openSet.Contains(neighbor)) // Discover a new node
                    openSet.Add(neighbor);

                // The distance from start to a neighbor
                //the "dist_between" function may vary as per the solution requirements.
                float tentative_gScore = gScore[current] + Vector3.Distance(current.pos, neighbor.pos);
                if (tentative_gScore >= gScore[neighbor])
                    continue;        // This is not a better path.

                // This path is the best until now. Record it!
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentative_gScore;
                fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
            }
        }
        return new MapNode[0];
    }

    private static List<MapNode> AddItem(int v, List<MapNode> curNeighbors, List<MapNode> allNodes) {
        if (v >= 0 && v < curNeighbors.Count) {
            curNeighbors.Add(allNodes[v]);
        }
        return curNeighbors;
    }

    private static float HeuristicCostEstimate(MapNode neighbor, MapNode goal) {
        return goal.pos.x - neighbor.pos.x
             + goal.pos.y - neighbor.pos.y;
    }

    private static MapNode[] ReconstructPath(Dictionary<MapNode, MapNode> cameFrom, MapNode currentNode) {
        MapNode current = currentNode;
        int errLen = 10000;
        List<MapNode> total_path = new List<MapNode>();
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
