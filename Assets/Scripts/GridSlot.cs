using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// These grid slots are shown when units want to move.
/// Slots are applied as a layer over ground starting from some height to -10.
/// </summary>
public class GridSlot : MonoBehaviour {

    public const string gridLayerName = "Grid";
    public const string groundLayerName = "Ground";

    /// <summary>
    /// To what height are slots set when there is no ground.
    /// </summary>
    private const float minHeight = -10f;

    /// <summary>
    /// What point to raycast from. Set it to lower to make it work for multiple floors.
    /// </summary>
    public float raycastFromHeight = 10f;

    /// <summary>
    /// What is on this slot. Nothing = ground, structure, or unit
    /// </summary>
    internal Soldier taken;

    public static List<GridSlot> allSlots = new List<GridSlot>();

    internal int id;
    static int idCount = 0;

    // Use this for initialization
    void Awake() {
        id = idCount;
        idCount++;

        taken = null;

        Vector3 vec = new Vector3(transform.position.x, raycastFromHeight, transform.position.z);
        Vector3 minPoint = new Vector3(transform.position.x, minHeight, transform.position.z);
        transform.position = vec;
        // Raycasts down to ground and puts this object on casted position.
        RaycastHit hit;
        Ray ray = new Ray(vec,
            Vector3.down);
        bool cast = Physics.Raycast(ray,
            out hit,
            raycastFromHeight - minHeight,
            1 << LayerMask.NameToLayer(groundLayerName),
            QueryTriggerInteraction.Ignore
            );
        if (cast) {
            transform.position = hit.point;
        } else {
            transform.position = minPoint;
            gameObject.SetActive(false);
        }

        SetLayer();

        allSlots.Add(this);
    }

    private void SetLayer() {
        gameObject.layer = LayerMask.NameToLayer(gridLayerName);
    }

    internal bool HasEnemy() {
        return taken != null && taken.allianceId != 0; // 0:player
    }

    internal static GridSlot[] GetSlotsInRange(GridSlot slot, float range) {
        List<GridSlot> slots = new List<GridSlot>();
        for (int i = 0; i < allSlots.Count; i++) {
            if (Vector3.Distance(allSlots[i].transform.position, slot.transform.position) <= range) {
                slots.Add(allSlots[i]);
            }
        }
        return slots.ToArray();
    }

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
            for (int i = 0; i < allSlots.Count; i++) {
                if (fScore[allSlots[i]] < minFScore) {
                    minFScore = fScore[allSlots[i]];
                    current = allSlots[i];
                }
            }
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            // Assumes 1 layered grid.
            GridSlot[] curNeighbors = new GridSlot[4]{// 4 neighbouring slots.
                allSlots[current.id+1],
                allSlots[current.id-1],
                allSlots[current.id-20],
                allSlots[current.id+20],
            };
            foreach (var neighbor in curNeighbors) {
                if (neighbor == null)
                    continue;
                if (closedSet.Contains(neighbor))
                    continue;       // Ignore the neighbor which is already evaluated.

                if (openSet.Contains(neighbor)) // Discover a new node
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
        while (cameFrom.ContainsKey(current) && errLen > 0){
            errLen--;
            current = cameFrom[current];
            total_path.Add(current);
        }
        if (errLen <= 0) {
            Debug.Log("Error infi loop, cyclic path");
        }
        return total_path.ToArray();
    }
}
