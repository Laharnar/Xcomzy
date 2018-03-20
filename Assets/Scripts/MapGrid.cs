using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid:MonoBehaviour {
    public Vector3 pointScale;
    public static List<MapNode> wholeMap {
        get {
            if (_wholeMap == null) {
                Debug.Log("Maybe MapGrid object isn't in scene.");
            } return _wholeMap; } private set { _wholeMap = value; } }
    static List<MapNode> _wholeMap;

    //public GridGenerator wh;
    private void Start() {
        wholeMap = new List<MapNode>();
        // detect map
        for (int i = 0; i < GridSlot.slotPositions.Count; i++) {
            wholeMap.Add(new MapNode(GridSlot.slotPositions[i]));

            wholeMap.Add(new MapNode(MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.right * pointScale.x, 10)));
            wholeMap.Add(new MapNode(MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z, 10)));
            wholeMap.Add(new MapNode(MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z + Vector3.right * pointScale.x, 10)));
        }
    }

    Vector3 MaxOfXRaycast(Vector3 point, float height) {
        float h1 = GetPointByRaycast(point + Vector3.forward * 0.01f + Vector3.left * 0.01f, height).y;
        float h2 = GetPointByRaycast(point + Vector3.back * 0.01f + Vector3.left * 0.01f, height).y;
        float h3 = GetPointByRaycast(point + Vector3.forward * 0.01f + Vector3.right * 0.01f, height).y;
        float h4 = GetPointByRaycast(point + Vector3.back * 0.01f + Vector3.right * 0.01f, height).y;
        return new Vector3(point.x,Mathf.Max(h1, h2, h3, h4),  point.z);
    }

    private void OnDrawGizmos() {
            Gizmos.color = Color.red;
        for (int i = 0; i < wholeMap.Count; i++) {
            Gizmos.DrawLine(wholeMap[i].pos, wholeMap[i].pos + Vector3.up);
        }
    }
    Vector3 GetPointByRaycast(Vector3 vec, float raycastFromHeight, float minHeight = -10) {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(vec.x, vec.y+raycastFromHeight, vec.z),
            Vector3.down);
        bool cast = Physics.Raycast(ray,
            out hit,
            raycastFromHeight - minHeight,
            1 << LayerMask.NameToLayer(GridSlot.groundLayerName),
            QueryTriggerInteraction.Ignore
            );
        if (cast) {
            return hit.point;
        }
        return new Vector3(vec.x, minHeight, vec.z);
    }

}
