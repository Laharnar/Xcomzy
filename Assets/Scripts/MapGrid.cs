using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid:MonoBehaviour {
    public const int pointsPerNode = 4;

    public Vector3 pointScale;
    public static List<MapNode> wholeMap {
        get {
            if (_wholeMap == null) {
                InitSingleton();
                if (_wholeMap == null)
                    Debug.Log("Maybe MapGrid object isn't in scene.");
            }
            return _wholeMap;
        }
        private set { _wholeMap = value; }
    }
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
            Vector3 p0 = MaxOfXRaycast(GridSlot.slotPositions[i], 10);
            Vector3 p1 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.right * pointScale.x, 10);
            Vector3 p2 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z, 10);
            Vector3 p3 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z + Vector3.right * pointScale.x, 10);
            SlotType t0 = MaxTypeOfXRaycast(GridSlot.slotPositions[i], 10);
            SlotType t1 = MaxTypeOfXRaycast(GridSlot.slotPositions[i] + Vector3.right * pointScale.x, 10);
            SlotType t2 = MaxTypeOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z, 10);
            SlotType t3 = MaxTypeOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z + Vector3.right * pointScale.x, 10);
            // all slots are walkable if it's ground.
            wholeMap.Add(new MapNode(p0,t0));
            wholeMap.Add(new MapNode(p1,t1));
            wholeMap.Add(new MapNode(p2,t2));
            wholeMap.Add(new MapNode(p3,t3));
        }

    }

    internal static GridSlot[] GetUntakenCoverInMovementRange(Soldier unit, float fullMovementRange, Team team, Team team2) {
        throw new NotImplementedException();
    }

    internal static GridSlot[] GetUntakenCoverInMovementRange(Soldier unit, float fullMovementRange, Soldier[] enemiesInRange) {
        throw new NotImplementedException();
    }

    internal static GridSlot[] GetCoverInMovementRange(Soldier unit, float fullMovementRange) {
        throw new NotImplementedException();
    }

    Vector3 MaxOfXRaycast(Vector3 point, float height) {
        float h1 = GetByRaycast(point + Vector3.forward * 0.01f + Vector3.left * 0.01f, height).point.y;
        float h2 = GetByRaycast(point + Vector3.back * 0.01f + Vector3.left * 0.01f, height).point.y;
        float h3 = GetByRaycast(point + Vector3.forward * 0.01f + Vector3.right * 0.01f, height).point.y;
        float h4 = GetByRaycast(point + Vector3.back * 0.01f + Vector3.right * 0.01f, height).point.y;
        return new Vector3(point.x,Mathf.Max(h1, h2, h3, h4),  point.z);
    }

    SlotType MaxTypeOfXRaycast(Vector3 point, float height) {
        RaycastHit h1 = GetByRaycast(point + Vector3.forward * 0.01f + Vector3.left * 0.01f, height);
        RaycastHit h2 = GetByRaycast(point + Vector3.back * 0.01f + Vector3.left * 0.01f, height);
        RaycastHit h3 = GetByRaycast(point + Vector3.forward * 0.01f + Vector3.right * 0.01f, height);
        RaycastHit h4 = GetByRaycast(point + Vector3.back * 0.01f + Vector3.right * 0.01f, height);
        SlotType max = SlotType.Walkable;
        //        Debug.Log(h1.transform);
        max = MaxCompatibilityType(h1, max);
        max = MaxCompatibilityType(h2, max);
        max = MaxCompatibilityType(h3, max);
        max = MaxCompatibilityType(h4, max);
        return max;
    }

    internal static bool OnlyGround(GridSlot curPositionSlot) {
        return AllUnderRange(curPositionSlot, 0.5f);
    }

    internal static float CoverScoreMultiplier(GridSlot gridSlot) {
        float f = 1;
        if (gridSlot.slotType == SlotType.ThinWall) {
            f = 2.5f;
        } else if (gridSlot.slotType == SlotType.Impassable) { // NOTE: doesn't distinguish between High and Medium cube wall!
            f = 5f;
        }
        return f;
    }

    private SlotType MaxCompatibilityType(RaycastHit h1, SlotType max) {
        if (h1.transform != null) {
            LevelItemType c = h1.transform.GetComponent<LevelItemType>();
            if (c == null) { Debug.LogError("Err: missing level  item type"+h1.transform.name, h1.transform); }
            else
                max = (SlotType)Mathf.Max((int)max, (int)c.itemPassabilityType);
        }
        return max;
    }
    
    static bool IsBelowRange(int id, float range) {
        // Checks if point is in low cover range
        return wholeMap[id].pos.y < range;
    }

    private void OnDrawGizmos() {
        if (wholeMap != null)
        for (int i = 0; i < wholeMap.Count; i++) {
            float h = 0.5f;
            Gizmos.color = Color.red;
            if (wholeMap[i].nodeType == SlotType.Walkable) {
                Gizmos.color = Color.green;
                h = 0.5f;
            }
            if (wholeMap[i].nodeType == SlotType.ThinWall) {
                Gizmos.color = Color.yellow;
                h = 2f;
            }
            Gizmos.DrawLine(wholeMap[i].pos, wholeMap[i].pos + Vector3.up*h);
        }
    }
    public static RaycastHit GetByRaycast(Vector3 vec, float raycastFromHeight, float minHeight = -10) {
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
            //return hit.point;
        }
        return hit;
        //return new Vector3(vec.x, minHeight, vec.z);
    }

    /// <summary>
    /// Detect if slot has only low cover. Useful for determining soldiers animations.
    /// </summary>
    /// <param name="curPositionSlot"></param>
    /// <returns></returns>
    internal static bool OnlyLowCover(GridSlot curPositionSlot) {
        return AllUnderRange(curPositionSlot, 3);
    }

    private static bool AllUnderRange(GridSlot curPositionSlot, float range) {
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
        bool gotLowCover = AllBelowRange(neighbourIds, range);
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
