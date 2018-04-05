using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls enemy turn/player turn cycle and player's soldier cycle
/// </summary>
public class GameplayManager : MonoBehaviour {
    public static GameplayManager m;

    Team[] flags = new Team[2];

    public Team playerFlag { get { return flags[0]; } }

    bool clickedEnemyOnce = false;

    int attackCommand = 0;

    // Use this for initialization
    void Start() {

        m = this;

        for (int i = 0; i < flags.Length; i++) {
            flags[i] = new Team();
        }

        // sort all units in scene by their alliance
        List<Soldier> allUnits = FindObjectsOfType<Soldier>().ToList();
        for (int i = 0; i < allUnits.Count; i++) {
            flags[allUnits[i].allianceId].units.Add(allUnits[i]);
        }

        for (int i = 0; i < flags.Length; i++) {
            flags[i].SnapAllUnitsToGround();
        }

        StartCoroutine(CinematicUpdate());
    }

    static GridSlot GetGridUnderMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool groundClicked = Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer(GridSlot.gridLayerName), QueryTriggerInteraction.Ignore);
        if (groundClicked) {
            return hit.transform.parent.GetComponent<GridSlot>();
        }
        return null;
    }

    // Update is called once per frame
    IEnumerator CinematicUpdate() {
        while (true) {
            int lastCommand = attackCommand;
            // *** PLAYER ***
            // Right click on any slot moves active unit there.
            GridSlot hitSlot = GetGridUnderMouse();

            if (hitSlot && Input.GetMouseButtonDown(1)) {
                attackCommand = 1;
                if (hitSlot.HasEnemy()) {
                    // Require 2 clicks to auto attack enemy
                    if (!clickedEnemyOnce)
                        clickedEnemyOnce = true;
                    else {
                        clickedEnemyOnce = false;
                        //flags[0].MoveActiveToRaycastedPoint(hit);
                        playerFlag.ActiveSoldier.AttackSlot(hitSlot);
                        EndSoldiersTurn();
                    }
                } else {
                    if (playerFlag.ActiveSoldier.MoveToSlot(hitSlot)) {
                        // enemies in overwatch fire at player's soldier when it moves.
                        HandleOverwatchWithoutFog(playerFlag.ActiveSoldier, flags[1]);
                        
                        yield return playerFlag.ActiveSoldier.CinematicsDone();

                        EndSoldiersTurn();
                    }
                }
            }

            // fire at nearest enemy
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                attackCommand = 2;
                Soldier nearestEnemy = flags[1].GetNearestTo(playerFlag.ActiveSoldier);
                if (!clickedEnemyOnce)
                    clickedEnemyOnce = true;
                else {
                    clickedEnemyOnce = false;
                    playerFlag.ActiveSoldier.AttackSlot(nearestEnemy.curPositionSlot);
                    EndSoldiersTurn();
                }
            }

            // grenade throw
            if (hitSlot && Input.GetKeyDown(KeyCode.Alpha3)) {
                attackCommand = 3;
                if (!clickedEnemyOnce) {
                    clickedEnemyOnce = true;
                } else {
                    clickedEnemyOnce = false;
                    playerFlag.ActiveSoldier.AttackSlot(hitSlot, 1);
                }
            }
            // overwatch throw
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                attackCommand = 4;
                if (!clickedEnemyOnce) {
                    clickedEnemyOnce = true;
                } else {
                    clickedEnemyOnce = false;
                    playerFlag.ActiveSoldier.ToOverwatch();
                    EndSoldiersTurn();
                }
            }
            // FIXED: it will work to click on enemy with right click and then 1.
            // makes sure you can't do mouse+something else attack
            if (attackCommand != lastCommand && lastCommand != 0) {
                clickedEnemyOnce = false;
            }

            // tabbing swaps units
            if (Input.GetKeyDown(KeyCode.Tab)) {
                EndSoldiersTurn();
            }

            // *** ENEMIES ***
            // if enemy moves, trigger all overwatched player's soldiers

            // AI: move all enemies
            for (int i = 0; i < flags[1].units.Count; i++) {
                // trigger player's overwatch
                HandleOverwatchWithoutFog(flags[1].units[i], playerFlag);
            }
            yield return null;
        }
    }

    private void HandleOverwatchWithoutFog(Soldier activeSoldier, Team otherTeam) {
        for (int i = 0; i < otherTeam.units.Count; i++) {
            if (otherTeam.units[i].inOverwatch) {
                otherTeam.units[i].AttackSlot(activeSoldier.curPositionSlot);
                otherTeam.units[i].inOverwatch = false;
                otherTeam.units[i].StartCoroutine(otherTeam.units[i].Cinematics_Shoot(activeSoldier.curPositionSlot));
            }
        }
    }

    void EndSoldiersTurn() {
        playerFlag.NextSoldier();
        PlayerCamera.ResetFocus();
    }
}
