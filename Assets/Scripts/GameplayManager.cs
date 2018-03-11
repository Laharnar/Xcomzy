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
}

/// <summary>
/// Controls enemy turn/player turn cycle and player's soldier cycle
/// </summary>
public class GameplayManager : MonoBehaviour {

    AllianceUnits[] flags = new AllianceUnits[2];

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
        // *** PLAYER ***
        // Right click on any slot moves active unit there.
        if (Input.GetMouseButtonDown(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool groundClicked = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(GridSlot.gridLayerName), QueryTriggerInteraction.Ignore);
            if (groundClicked) {
                flags[0].MoveActiveToRaycastedPoint(hit);
            }
        }
    }
}
