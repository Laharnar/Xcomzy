using System;
using System.Collections.Generic;
using UnityEngine;

public class Team {
    public List<Soldier> units = new List<Soldier>();
    public int activePlayerSoldier = 0;

    public Soldier ActiveSoldier { get { return units[activePlayerSoldier]; } }

    public ITurnCycle cycle;
    
    public void SnapAllUnitsToGround() {
        for (int i = 0; i < units.Count; i++) {
            Ray ray = new Ray(units[i].transform.position, Vector3.down);
            RaycastHit hit;
            bool groundClicked = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(GridSlot.gridLayerName), QueryTriggerInteraction.Ignore);
            if (groundClicked) {
                GridSlot hitSlot = hit.transform.parent.GetComponent<GridSlot>();
                units[i].SetCurSlot(hitSlot);

                MapNode[] path = Pathfinding.FindPathAStar(units[i].curPositionSlot.transform.position, hitSlot.transform.position, MapGrid.wholeMap);
                units[i].StartCoroutine(units[i].MoveToSlot(hitSlot, path, false, false));

            }
        }
    }

    internal Soldier GetNearestTo(Soldier soldier) {
        if (units.Count == 0)
            return null;
        float dist = float.MaxValue;
        int best = 0;
        for (int i = 0; i < units.Count; i++) {
            float d = Vector3.Distance(units[i].transform.position, soldier.transform.position);
            if (d < dist) {
                dist = d;
                best = i;
            }
        }
        return units[best];
    }

    internal void NextSoldier() {
        activePlayerSoldier = (activePlayerSoldier + 1) % units.Count;
    }

    internal bool AnySoldierActionsLeft() {
        for (int i = 0; i < units.Count; i++) {
            if (units[i].actionsLeft > 0)
                return true;
        }
        return false;
    }
}
