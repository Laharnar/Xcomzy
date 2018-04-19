using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MapNodeType {
    Walkable,
    Climbable,
    Impassable,
    OffLimits
}
public enum ItemType {
    Walkable,
    ThinWall,
    Impassable,
}

public enum SlotType {
    Walkable,
    Cover,
    Inaccesible///off level items.
}
/// <summary>
/// These grid slots are shown when units want to move.
/// Slots are applied as a layer over ground starting from some height to -10.
/// </summary>
public partial class GridSlot : MonoBehaviour {

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
    /// What is on this slot. Null = ground to walk on, non unit
    /// </summary>
    internal Soldier taken;

    public static List<GridSlot> allSlots = new List<GridSlot>();
    
    /// <summary>
    /// All positions, even from deleted impassable slots.
    /// </summary>
    public static List<Vector3> slotPositions = new List<Vector3>();

    internal int id;
    static int idCount = 0;

    /// <summary>
    /// Refers type of slot this is depending on data around it.
    /// Slot that has cover around it is cover type.
    /// </summary>
    public SlotType slotType;

    /// <summary>
    /// Max cover in neighbours around this slot.
    /// </summary>
    int maxCoverHeight = 0;

    // Use this for initialization
    void Awake() {
        id = idCount;
        idCount++;

        taken = null;

        Vector3 vec = new Vector3(transform.position.x, raycastFromHeight, transform.position.z);
        Vector3 minDepth = new Vector3(transform.position.x, minHeight, transform.position.z);
        transform.position = vec;
        // Raycasts down to ground and puts this object on casted position.
        RaycastHit hit;
        Ray ray = new Ray(vec, Vector3.down);
        bool cast = Physics.Raycast(ray, out hit, raycastFromHeight - minHeight,
            1 << LayerMask.NameToLayer(groundLayerName), 
            QueryTriggerInteraction.Ignore);
        slotType = SlotType.Walkable;
        if (cast) {
            Vector3 point = RayMap.GetByRaycast(transform.position, 10).point;
            if (point.y > 0.5f) {
                Destroy(gameObject);
            }
            /*
            if (point.y < 0.5f && point.y >= 0f) {
                slotType = SlotType.Walkable;
            } else if (point.y >0.5f) {
                slotType = SlotType.Cover;
            }*/
            transform.position = hit.point;
        } else { // snap grid slot off map
            transform.position = minDepth;
            gameObject.SetActive(false);
        }

        slotPositions.Add(gameObject.transform.position);

        // grid should slots have this layer by default, so raycasts can work properly
        gameObject.layer = LayerMask.NameToLayer(gridLayerName);

        allSlots.Add(this);
    }

    public static void SetTypes(RayMap map) {
        for (int i = 0; i < allSlots.Count; i++) {
            MapNode[] nbours = RayMap.Get4Neighbours(allSlots[i].id*4);

            allSlots[i].slotType = SlotType.Walkable;
            for (int j = 0; j < nbours.Length; j++) {
                if (nbours[j].nodeType == MapNodeType.Climbable || nbours[j].nodeType == MapNodeType.Impassable) {
                    allSlots[i].slotType = SlotType.Cover;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Detect if slot has only low cover. Useful for determining soldiers animations.
    /// </summary>
    /// <param name="curPositionSlot"></param>
    /// <returns></returns>
    internal static bool OnlyLowCover(GridSlot curPositionSlot) {
        return SlotUnderRange(curPositionSlot, 3);
    }

    internal static bool OnlyGround(GridSlot curPositionSlot) {
        return curPositionSlot.slotType == SlotType.Walkable &&  SlotUnderRange(curPositionSlot, 0.5f);
    }

    internal static Soldier[] GetVisibleEnemySlots(Soldier soldier, Team team2) {
        return team2.units.ToArray();
    }
    

    internal bool HasEnemy() {
        return taken != null && taken.allianceId != 0; // 0:player
    }

    /// <summary>
    /// Returns all slots in range
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    internal static GridSlot[] GetSlotsInRange(GridSlot slot, float range) {
        List<GridSlot> slots = new List<GridSlot>();
        for (int i = 0; i < allSlots.Count; i++) {
            if (allSlots[i] == null)
                continue;
            if (Vector3.Distance(allSlots[i].transform.position, slot.transform.position) <= range) {
                slots.Add(allSlots[i]);
            }
        }
        return slots.ToArray();
    }

    /// <summary>
    /// Returns slots in range without soldiers.
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    internal static GridSlot[] GetAvaliableSlotsInMoveRange(GridSlot slot, float range) {
        List<GridSlot> slots = new List<GridSlot>();
        for (int i = 0; i < allSlots.Count; i++) {
            if (allSlots[i] == null || allSlots[i].taken)
                continue;
            if (Vector3.Distance(allSlots[i].transform.position, slot.transform.position) <= range) {
                slots.Add(allSlots[i]);
            }
        }
        return slots.ToArray();
    }

    public static bool SlotUnderRange(GridSlot curPositionSlot, float range) {
        return RayMap.AllBelowRange(RayMap.Get4Neighbours(curPositionSlot.id * 4), range);
    }
}
public partial class GridSlot {

    internal static float CoverScoreMultiplier(GridSlot gridSlot) {
        float f = 1;
        if (gridSlot.slotType == SlotType.Cover) {
            f = 10;
        } // if (gridSlot.slotType == SlotType.Inaccesible) {
          //  f = 20;
        //}
        return f;
    }

}