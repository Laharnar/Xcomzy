using System;
using System.Collections.Generic;
using UnityEngine;

public partial class RayMap:MonoBehaviour {

    static RayMap m;

    public const int pointsPerNode = 4;

    public Vector3 pointScale = Vector3.one;// vec.one
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

    public float thinWallMul = 2.5f;
    public float fatWallMul = 5f;

    public static void InitSingleton() {
        m = GameObject.FindObjectOfType<RayMap>();
        m.Init();
    }

    //public GridGenerator wh;
    void Init() {//Start
        wholeMap = new List<MapNode>();
        int climbPoints = 0, walkPoints = 0, highPoints = 0;
        // detect map height at different points from center
        for (int i = 0; i < GridSlot.slotPositions.Count; i++) {
            Vector3 p0 = MaxOfXRaycast(GridSlot.slotPositions[i], 10);
            Vector3 p1 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.right * pointScale.x, 10);
            Vector3 p2 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z, 10);
            Vector3 p3 = MaxOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z + Vector3.right * pointScale.x, 10);
            MapNodeType t0 = MaxTypeOfXRaycast(GridSlot.slotPositions[i], 10);
            MapNodeType t1 = MaxTypeOfXRaycast(GridSlot.slotPositions[i] + Vector3.right * pointScale.x, 10);
            MapNodeType t2 = MaxTypeOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z, 10);
            MapNodeType t3 = MaxTypeOfXRaycast(GridSlot.slotPositions[i] + Vector3.forward * pointScale.z + Vector3.right * pointScale.x, 10);
            // all slots are walkable if it's ground.
            wholeMap.Add(new MapNode(p0,t0));
            wholeMap.Add(new MapNode(p1,t1));
            wholeMap.Add(new MapNode(p2,t2));
            wholeMap.Add(new MapNode(p3,t3));
            CountTypes(ref climbPoints, ref walkPoints, ref highPoints, t0);
            CountTypes(ref climbPoints, ref walkPoints, ref highPoints, t1);
            CountTypes(ref climbPoints, ref walkPoints, ref highPoints, t2);
            CountTypes(ref climbPoints, ref walkPoints, ref highPoints, t3);
        }
        Debug.Log("Generated raymap: Climb:"+climbPoints + " Walk:"+walkPoints + " Inaccessible:"+highPoints);

        GridSlot.AfterRaymapInit();
    }

    private void CountTypes(ref int climbPoints, ref int walkPoints, ref int highPoints, MapNodeType t1) {
        switch (t1) {
            case MapNodeType.Walkable:
                walkPoints++;
                break;
            case MapNodeType.Climbable:
                climbPoints++;
                break;
            case MapNodeType.Inaccesable:
                highPoints++;
                break;
            case MapNodeType.OffLimits:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Max height of 4 points around selected point.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    Vector3 MaxOfXRaycast(Vector3 point, float height) {
        float max = float.MinValue;
        Vector3[] pts = Get4Points(point);
        for (int i = 0; i < 4; i++) {
            RaycastHit h = GetByRaycast(pts[i], height);
            if (h.transform!= null)
                max = Mathf.Max(max,h.point.y);
        }
        return new Vector3(point.x, max, point.z);
    }

    /// <summary>
    /// Max type of slot(walkable, impassable...) around some point.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    MapNodeType MaxTypeOfXRaycast(Vector3 point, float height) {
        MapNodeType max = MapNodeType.Walkable;
        Vector3[] pts = Get4Points(point);
        for (int i = 0; i < 4; i++) {
            RaycastHit h = GetByRaycast(pts[i], height);
            if (h.transform != null)
            max = MaxCompatibilityType(h, max);
        }
        return max;
    }

    internal static Vector3[] Get4Points(Vector3 point) {
        Vector3[] pts = new Vector3[4];
        pts[0] = point + Vector3.forward * 0.01f + Vector3.left * 0.01f;
        pts[1] = point + Vector3.back * 0.01f + Vector3.left * 0.01f;
        pts[2] = point + Vector3.forward * 0.01f + Vector3.right * 0.01f;
        pts[3] = point + Vector3.back * 0.01f + Vector3.right * 0.01f;
        return pts;
    }

    internal static MapNode[] Get4Neighbours(int rayMapNodeId) {
        int[] neighbourIds = new int[4] {
            rayMapNodeId+1,
            rayMapNodeId+2,
            rayMapNodeId-2,//-4+2,
            rayMapNodeId-GridGenerator.gen.w*4+1
        };
        List<MapNode> nbours = new List<MapNode>();
        for (int i = 0; i < neighbourIds.Length; i++) {
            if (neighbourIds[i] > -1 && neighbourIds[i] < wholeMap.Count)
                nbours.Add(wholeMap[neighbourIds[i]]);
        }
        return nbours.ToArray();
    }

    private MapNodeType MaxCompatibilityType(RaycastHit h1, MapNodeType max) {
        LevelItemType c = h1.transform.GetComponent<LevelItemType>();
        if (c == null) {
            Debug.LogError("Err: missing level  item type" + h1.transform.name, h1.transform); } 
        else
            max = (MapNodeType)Mathf.Max((int)max, (int)c.itemPassabilityType);
        return max;
    }

    private void OnDrawGizmos() {
        if (wholeMap != null)
        for (int i = 0; i < wholeMap.Count; i++) {
            float h = 0.5f;
            Gizmos.color = Color.red;
            if (wholeMap[i].nodeType == MapNodeType.Walkable) {
                Gizmos.color = Color.green;
                h = 0.5f;
            }
            if (wholeMap[i].nodeType == MapNodeType.Climbable) {
                Gizmos.color = Color.yellow;
                h = 2f;
            }
            Gizmos.DrawLine(wholeMap[i].pos, wholeMap[i].pos + Vector3.up*h);
        }
    }
}

public partial class RayMap {

    
    internal static float CoverScoreMultiplier(GridSlot gridSlot) {
        float f = 1;
        if (gridSlot.slotType == SlotType.Cover) {
            f = m.thinWallMul;
        } else if (gridSlot.slotType == SlotType.Cover) { // NOTE: doesn't distinguish between High and Medium cube wall!
            f = m.fatWallMul;
        }
        return f;
    }

    public static RaycastHit GetByRaycast(Vector3 vec, float raycastFromHeight, float minHeight = -10) {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(vec.x, vec.y + raycastFromHeight, vec.z),
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

    public static bool AllBelowRange(MapNode[] ids, float range) {
        for (int i = 0; i < ids.Length; i++) {
            //if (wholeMap[i].nodeType == MapNodeType.Climbable || wholeMap[i].nodeType == MapNodeType.Impassable) { 
            if (ids[i].pos.y >= range) { // low cover or ground
                return false;
            }
        }
        return true;
    }
}