using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType {
    Walkable,
    ThinWall,
    Impassable,
}
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
    /// <summary>
    /// All positions, even from deleted impassable slots.
    /// </summary>
    public static List<Vector3> slotPositions = new List<Vector3>();

    internal int id;
    static int idCount = 0;

    public SlotType slotType;

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
        Ray ray = new Ray(vec,
            Vector3.down);
        bool cast = Physics.Raycast(ray,
            out hit,
            raycastFromHeight - minHeight,
            1 << LayerMask.NameToLayer(groundLayerName),
            QueryTriggerInteraction.Ignore
            );
        slotType = SlotType.Impassable;
        if (cast) {
            Vector3 point = MapGrid.GetByRaycast(transform.position, 10).point;
            if (point.y < 0.5f && point.y >= 0f) {
                slotType = SlotType.Walkable;
            } else if (point.y >0.5f) {
                slotType = SlotType.Impassable;
            }
            transform.position = hit.point;
        } else { // snap grid slot off map
            transform.position = minDepth;
            gameObject.SetActive(false);
        }

        slotPositions.Add(gameObject.transform.position);

        if (slotType == SlotType.Impassable) {
            Destroy(gameObject);
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
            if (allSlots[i] == null)
                continue;
            if (Vector3.Distance(allSlots[i].transform.position, slot.transform.position) <= range) {
                slots.Add(allSlots[i]);
            }
        }
        return slots.ToArray();
    }
    

}
