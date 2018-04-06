using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid:MonoBehaviour {
    public const int pointsPerNode = 4;

    public Vector3 pointScale;
    public static List<MapNode> wholeMap {
        get {
            if (_wholeMap == null) {
                Debug.Log("Maybe MapGrid object isn't in scene.");
            } return _wholeMap; } private set { _wholeMap = value; } }
    static List<MapNode> _wholeMap;
    static List<bool> mask;

    public static void InitSingleton() {
        GameObject.FindObjectOfType<MapGrid>().Init();
    }

    //public GridGenerator wh;
    void Init() {//Start
        wholeMap = new List<MapNode>();
        // detect map
        for (int i = 0; i < GridSlot.slotPositions.Count; i++) {
            Vector3 p = MaxOfXRaycast(GridSlot.slotPositions[i], 10);
            Vector3 p1 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.right * pointScale.x, 10);
            Vector3 p2 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z, 10);
            Vector3 p3 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z + Vector3.right * pointScale.x, 10);
            // all slots are walkable if it's ground.
            wholeMap.Add(new MapNode(p, false));
            wholeMap.Add(new MapNode(p1, false));
            wholeMap.Add(new MapNode(p2, false));
            wholeMap.Add(new MapNode(p3, false));
        }

        for (int i = 0; i < wholeMap.Count; i+=4) {
            wholeMap[i].walkable = wholeMap[i].pos.y < 0.5f;
            wholeMap[i + 1].walkable = wholeMap[i + 1].pos.y < 3 && wholeMap[i].walkable;
            wholeMap[i + 2].walkable = wholeMap[i + 2].pos.y < 3 && wholeMap[i].walkable;
            wholeMap[i + 3].walkable = wholeMap[i + 3].pos.y < 0.5f;
        }
    }

    Vector3 MaxOfXRaycast(Vector3 point, float height) {
        float h1 = GetPointByRaycast(point + Vector3.forward * 0.01f + Vector3.left * 0.01f, height).y;
        float h2 = GetPointByRaycast(point + Vector3.back * 0.01f + Vector3.left * 0.01f, height).y;
        float h3 = GetPointByRaycast(point + Vector3.forward * 0.01f + Vector3.right * 0.01f, height).y;
        float h4 = GetPointByRaycast(point + Vector3.back * 0.01f + Vector3.right * 0.01f, height).y;
        return new Vector3(point.x,Mathf.Max(h1, h2, h3, h4),  point.z);
    }

    static bool IsBelowRange(int id, float range) {
        // Checks if point is in low cover range
        return wholeMap[id].pos.y < range;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < wholeMap.Count; i++) {
            if (wholeMap[i].walkable) {
                Gizmos.color = Color.green;
            } else {
                Gizmos.color = Color.red;
            }
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

    internal static bool MaxLowCover(GridSlot curPositionSlot) {
        int id = curPositionSlot.id * 4;
        /* neighbours mid-path cover: 
        right:id; i+1, forw:i+2, 
        left:id-1;i+1
        back:id+width;i+2
        */
        int[] neighbourIds = new int[4] {
            id+1,
            id+2,
            id-3,//-4+1,
            id+GridGenerator.gen.w*4+2
        };
        bool gotLowCover = AllBelowRange(neighbourIds, 3);
        return gotLowCover;
    }

    private static bool AllBelowRange(int[] neighbourIds, float range) {
        for (int i = 0; i < neighbourIds.Length; i++) {
            if (neighbourIds[i] > -1 && neighbourIds[i] < wholeMap.Count) {
                if (!IsBelowRange(neighbourIds[i], range)) { // low cover or ground
                    return false;
                }
            }
        }
        return true;
    }
}
