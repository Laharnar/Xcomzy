using System.Collections.Generic;
using UnityEngine;

public class Team {
    public List<Soldier> units = new List<Soldier>();
    public int activePlayerSoldier = 0;

    public Soldier ActiveSoldier { get { return units[activePlayerSoldier]; } }

    public void InitUnitPos(GridSlot hitSlot) {
        bool moveOk = ActiveSoldier.MoveToSlot(hitSlot, false);
        if (!moveOk) {
            ActiveSoldier.AttackSlot(hitSlot);
        }
        NextSoldier();
    }

    public void SnapAllUnitsToGround() {
        for (int i = 0; i < units.Count; i++) {
            Ray ray = new Ray(units[i].transform.position, Vector3.down);
            RaycastHit hit;
            bool groundClicked = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(GridSlot.gridLayerName), QueryTriggerInteraction.Ignore);
            if (groundClicked) {
                GridSlot hitSlot = hit.transform.parent.GetComponent<GridSlot>();
                units[i].SetCurSlot(hitSlot);
                InitUnitPos(hitSlot);
            }
        }
    }

    internal Soldier GetNearestTo(Soldier soldier) {
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
}
