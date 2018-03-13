using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AllianceUnits {
    public List<Soldier> units = new List<Soldier>();
    public int activePlayerSoldier = 0;

    public object ActiveSoldier { get { return units[activePlayerSoldier]; } }

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

    internal void NextSoldier() {
        activePlayerSoldier = (activePlayerSoldier + 1) % units.Count;
    }
}

/// <summary>
/// Controls enemy turn/player turn cycle and player's soldier cycle
/// </summary>
public class GameplayManager : MonoBehaviour {
    public static GameplayManager m;

    AllianceUnits[] flags = new AllianceUnits[2];

    public AllianceUnits playerFlag { get { return flags[0]; } }

    bool clickedEnemyOnce = false;

    int attackCommand = 0;

    // Use this for initialization
    void Start () {

        m = this;

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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool groundClicked = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(GridSlot.gridLayerName), QueryTriggerInteraction.Ignore);

        if (Input.GetMouseButtonDown(1)) {
            attackCommand = 1;
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
                        flags[0].NextSoldier();
                    }
                } else {
                    flags[0].units[flags[0].activePlayerSoldier].MoveToSlot(hitSlot);
                    flags[0].NextSoldier();
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
                flags[0].ActiveSoldier.AttackSlot(nearestEnemy.curPositionSlot);
                flags[0].NextSoldier();
            }
        }


        // grenade throw
        // Untested: err with clicking?
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            attackCommand = 3;
            if (!clickedEnemyOnce) {
                Debug.Log("Gren ready");
                clickedEnemyOnce = true;
            } else {
                Debug.Log("Gren throw");
                clickedEnemyOnce = false;
                //flags[0].MoveActiveToRaycastedPoint(hit);
                GridSlot hitSlot = hit.transform.parent.GetComponent<GridSlot>();
                flags[0].units[flags[0].activePlayerSoldier].AttackSlot(hitSlot, 1);
            }
        }

        // FIXED: it will work to click on enemy with right click and then 1.
        // makes sure you can't do mouse+1 attack
        if (attackCommand != lastCommand && lastCommand!= 0) {
            clickedEnemyOnce = false;
        }

        // tabbing swaps units
        if (Input.GetKeyDown(KeyCode.Tab)) {
            flags[0].activePlayerSoldier = (flags[0].activePlayerSoldier + 1) % flags[0].units.Count;
        }

    }
}
