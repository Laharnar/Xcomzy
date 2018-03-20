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
        Dictionary<int, MapNode> nodes = new Dictionary<int, MapNode>();
        for (int i = 0; i < allSlots.Count; i++) {
            nodes.Add(allSlots[i].id, allSlots[i]);
        }

        int start = MapNode.FindNode(startPos).id;
        int goal = MapNode.FindNode(goalPos).id;
        // The set of nodes already evaluated
        HashSet<int> closedSet = new HashSet<int>();

        // The set of currently discovered nodes that are not evaluated yet.
        // Initially, only the start node is known.
        HashSet<int> openSet = new HashSet<int>();
        openSet.Add(start);

        // For each node, which node it can most efficiently be reached from.
        // If a node can be reached from many nodes, cameFrom will eventually contain the
        // most efficient previous step.
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();

        // For each node, the cost of getting from the start node to that node.
        // init as inifinity
        Dictionary<int, float> gScore = new Dictionary<int, float>();

        // For each node, the total cost of getting from the start node to the goal
        // by passing by that node. That value is partly known, partly heuristic.
        //map with default value of Infinity
        Dictionary<int, float> fScore = new Dictionary<int, float>();
        for (int i = 0; i < allSlots.Count; i++) {
            gScore.Add(allSlots[i].id, float.PositiveInfinity);
            fScore.Add(allSlots[i].id, float.PositiveInfinity);
            cameFrom.Add(allSlots[i].id, -1);
        }

        // The cost of going from start to start is zero.
        gScore[start] = 0f;

        // For the first node, that value is completely heuristic.
        fScore[start] = HeuristicCostEstimate(nodes[start], nodes[goal]);
        while (openSet.Count > 0) {
            // the node in openSet having the lowest fScore[] value
            MapNode current = null;//Vector3.zero; // grid slot
            float minFScore = float.MaxValue;
            foreach (var slot in openSet) {
                if (fScore[slot] < minFScore) {
                    minFScore = fScore[slot];
                    current = nodes[slot];
                }
            }
            if (current.Equals(nodes[goal]))
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current.id);
            closedSet.Add(current .id);

            // Assumes 1 layered grid.
            List<MapNode> curNeighbors = new List<MapNode>();
            AddItem(current.id+1, ref curNeighbors, allSlots);
            AddItem(current.id-1, ref curNeighbors, allSlots);
            AddItem(current.id-20,ref  curNeighbors, allSlots);
            AddItem(current.id+20,ref  curNeighbors, allSlots);

            foreach (var neighbor in curNeighbors) {
                if (neighbor == null)
                    continue;
                if (closedSet.Contains(neighbor.id))
                    continue;       // Ignore the neighbor which is already evaluated.

                if (!openSet.Contains(neighbor.id)) // Discover a new node
                    openSet.Add(neighbor.id);

                // The distance from start to a neighbor
                //the "dist_between" function may vary as per the solution requirements.
                float tentative_gScore = gScore[current.id] + Vector3.Distance(current.pos, neighbor.pos);
                if (tentative_gScore >= gScore[neighbor.id])
                    continue;        // This is not a better path.

                // This path is the best until now. Record it!
                cameFrom[neighbor.id] = current.id;
                gScore[neighbor.id] = tentative_gScore;
                fScore[neighbor.id] = gScore[neighbor.id] + HeuristicCostEstimate(neighbor, nodes[goal]);
            }
        }
        return new MapNode[0];
    }

    private static void AddItem(int v, ref List<MapNode> curNeighbors, List<MapNode> allNodes) {
        if (v >= 0 && v < allNodes.Count) {
            curNeighbors.Add(allNodes[v]);
        }
    }

    private static float HeuristicCostEstimate(MapNode neighbor, MapNode goal) {
        return goal.pos.x - neighbor.pos.x
             + goal.pos.y - neighbor.pos.y;
    }

    private static MapNode[] ReconstructPath(Dictionary<int, int> cameFrom, MapNode currentNode) {
        Debug.Log("reconstrctin"+currentNode.id+""+cameFrom[currentNode.id]);
        MapNode current = currentNode;
        int errLen = 10000;
        List<MapNode> total_path = new List<MapNode>();
        total_path.Add(current);
        while (current != null && current.id != -1 && cameFrom.ContainsKey(current.id) && errLen > 0) {
            errLen--;
            current = MapNode.FindNode(cameFrom[current.id]);
            if (current!= null && current.id != -1)
                total_path.Add(current);
        }
        if (errLen <= 0) {
            Debug.Log("Error infi loop, cyclic path");
        }
        return total_path.ToArray();
    }
}
