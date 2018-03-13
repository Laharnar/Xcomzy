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

    // Use this for initialization
    void Awake() {
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
            1<<LayerMask.NameToLayer(groundLayerName),
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
            if (Vector3.Distance(allSlots [i].transform.position, slot.transform.position) <=range) {
                slots.Add(allSlots[i]);
            }
        }
        return slots.ToArray();
    }
}
