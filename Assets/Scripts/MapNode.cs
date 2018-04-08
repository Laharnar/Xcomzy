using System.Collections.Generic;
using UnityEngine;
public class MapNode {
    public Vector3 pos;
    public int id = -1;
    static int idCount;
    static List<MapNode> allNodes = new List<MapNode>();
    internal SlotType nodeType;

    public MapNode(Vector3 pos, SlotType nodeType, bool setId = true) {
        this.pos = pos;
        this.nodeType = nodeType;
        if (setId) {
            UpdateId();
        }
    }

    void UpdateId() {
        if (id == -1) {
            id = idCount;
            idCount++;
            allNodes.Add(this);
        }
    }

    /// <summary>
    /// Assumes pos exists in allNodes
    /// </summary>
    /// <param name="goalPos"></param>
    /// <returns></returns>
    internal static MapNode FindNode(Vector3 pos) {
        for (int i = 0; i < allNodes.Count; i++) {
            if (pos == allNodes[i].pos) {
                return allNodes[i];
            }
        }
        Debug.Log("No match found");
        return null;
    }


    internal static MapNode FindNode(int id) {
        for (int i = 0; i < allNodes.Count; i++) {
            if (id == allNodes[i].id) {
                return allNodes[i];
            }
        }
        Debug.Log("No match found for id "+id);
        return null;
    }
}
