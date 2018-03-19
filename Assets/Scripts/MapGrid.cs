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

            wholeMap.Add(new MapNode(GetPointByRaycast(GridSlot.slotPositions[i] + Vector3.right * pointScale.x, 10)));
            wholeMap.Add(new MapNode(GetPointByRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z,10)));
        }
    }

    Vector3 GetPointByRaycast(Vector3 vec, float raycastFromHeight, float minHeight = -10) {
        RaycastHit hit;
        Ray ray = new Ray(vec,
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
