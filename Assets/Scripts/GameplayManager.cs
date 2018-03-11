using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllianceUnits {
    public List<Soldier> units = new List<Soldier>();
    public int activePlayerSoldier = 0;


    public void MoveActiveToRaycastedPoint(RaycastHit hit) {
        GridSlot hitSlot = hit.transform.parent.GetComponent<GridSlot>();
        bool moveOk = units[activePlayerSoldier].MoveToSlot(hitSlot);
        if (!moveOk) {
            units[activePlayerSoldier].AttackSlot(hitSlot);
        }
        activePlayerSoldier = (activePlayerSoldier + 1) % units.Count;
    }

    public void SnapAllUnitsToGround() {
        for (int i = 0; i < units.Count; i++) {
            Ray ray = new Ray(units[i].transform.position, Vector3.down);
            RaycastHit hit;
            bool groundClicked = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(GridSlot.gridLayerName), QueryTriggerInteraction.Ignore);
            if (groundClicked) {
                MoveActiveToRaycastedPoint(hit);
            }
        }
    }

    internal Soldier GetNearest(Soldier soldier) {
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
}

/// <summary>
/// Controls enemy turn/player turn cycle and player's soldier cycle
/// </summary>
public class GameplayManager : MonoBehaviour {

    AllianceUnits[] flags = new AllianceUnits[2];

    bool clickedEnemyOnce = false;

    int attackCommand = 0;

    // Use this for initialization
    void Start () {
        for (int i = 0; i < flags.Length; i++) {
            flags[i] = new AllianceUnits();
        }

        // sort all units in scene by their alliance
        List<Soldier> allUnits = FindObjectsOfType<Soldier>().ToList();
        for (int i = 0; i < allUnits.Count; i++) {
            flags[allUnits[i].allianceId].units.Add(allUnits[i]);
        }

        for (int i = 0; i < flags.Length; i++) {
            flags[i].SnapAllUnitsToGround();
        }
	}
	
	// Update is called once per frame
	void Update () {
        int lastCommand = attackCommand;
        // *** PLAYER ***
        // Right click on any slot moves active unit there.
        if (Input.GetMouseButtonDown(1)) {
            attackCommand = 1;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool groundClicked = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(GridSlot.gridLayerName), QueryTriggerInteraction.Ignore);
            if (groundClicked) {
                GridSlot hitSlot = hit.transform.parent.GetComponent<GridSlot>();
                if (hitSlot.HasEnemy()) {
                    // Require 2 clicks to auto attack enemy
                    if (!clickedEnemyOnce)
                        clickedEnemyOnce = true;
                    else {
                        clickedEnemyOnce = false;
                        //flags[0].MoveActiveToRaycastedPoint(hit);
                        flags[0].units[flags[0].activePlayerSoldier].AttackSlot(hitSlot);
                        flags[0].activePlayerSoldier = (flags[0].activePlayerSoldier + 1) % flags[0].units.Count;
                    }
                } else {
                    flags[0].units[flags[0].activePlayerSoldier].MoveToSlot(hitSlot);
                    flags[0].activePlayerSoldier = (flags[0].activePlayerSoldier + 1) % flags[0].units.Count;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            attackCommand = 2;
            Soldier nearestEnemy = flags[1].GetNearest(flags[0].units[flags[0].activePlayerSoldier]);
            if (!clickedEnemyOnce)
                clickedEnemyOnce = true;
            else {
                clickedEnemyOnce = false;
                //flags[0].MoveActiveToRaycastedPoint(hit);
                flags[0].units[flags[0].activePlayerSoldier].AttackSlot(nearestEnemy.curPositionSlot);
                flags[0].activePlayerSoldier = (flags[0].activePlayerSoldier + 1) % flags[0].units.Count;
            }
        }
        // FIXED: it will work to click on enemy with right click and then 1.
        // makes sure you can't do mouse+1 attack
        if (attackCommand != lastCommand && lastCommand!= 0) {
            clickedEnemyOnce = false;
        }
    }
}
