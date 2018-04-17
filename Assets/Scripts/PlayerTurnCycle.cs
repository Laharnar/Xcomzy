using System.Collections;
using UnityEngine;
class PlayerTurnCycle : ITurnCycle {

    bool clickedOnce = false;

    public int attackCommand { get; private set; } // which type of attack is being used. attack with right click and 1 is different
    public int uiCommandKey { get; private set; } // which type of attack ui is being used active. right click and 1 is same
    public int targetedEnemy { get; private set; }

    void Init() {
        attackCommand = -1;
        uiCommandKey = -1;
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

    private IEnumerator HandleOverwatchWithoutFog(Soldier activeSoldier, Team otherTeam) {
        for (int i = 0; i < otherTeam.units.Count; i++) {
            if (otherTeam.units[i].inOverwatch) {
                otherTeam.units[i].AttackSlot(activeSoldier.curPositionSlot);
                otherTeam.units[i].inOverwatch = false;
                otherTeam.units[i].gun.Fire("Overwatch");
                yield return otherTeam.units[i].StartCoroutine(otherTeam.units[i].Cinematics_Shoot(activeSoldier.curPositionSlot));
            }
        }
    }

    private void SwapTarget(Team activeTeam) {
        targetedEnemy = (targetedEnemy + 1) % activeTeam.units.Count;
        PlayerCamera.ResetFocus();
    }

    private void SwapSoldierIfNoTurns(Team team) {
        if (team.AnySoldierActionsLeft()) {
            while (team.ActiveSoldier.actionsLeft == 0) {
                team.NextSoldier();
            }
            PlayerCamera.ResetFocus();
        }
        attackCommand = -1;
        targetedEnemy = -1;
        uiCommandKey = -1;
    }

    void SwapSoldier(Team activeTeam) {
        if (activeTeam.AnySoldierActionsLeft()) {
            activeTeam.NextSoldier();
            PlayerCamera.ResetFocus();
        }
        attackCommand = -1;
        targetedEnemy = -1;
        uiCommandKey = -1;
    }   

    public IEnumerator TurnCycle(Team team, Team team2) {
        while (team.AnySoldierActionsLeft()) {
            int lastCommand = attackCommand;
            // *** PLAYER ***
            // Right click on any slot moves active unit there.
            GridSlot hitSlot = GetGridUnderMouse();
            if (hitSlot != null) {
                MapNode[] path = Pathfinding.FindPathAStar(team.ActiveSoldier.curPositionSlot.transform.position, hitSlot.transform.position, MapGrid.wholeMap);

                // mouse clicks
                if (hitSlot && Input.GetMouseButtonDown(1) && team.ActiveSoldier.NearEnough(path.Length)) {
                    // shoot at enemy
                    if (hitSlot.HasEnemy()) {
                        attackCommand = 1;
                        uiCommandKey = 0;
                        // Require 2 clicks to auto attack enemy
                        if (!clickedOnce)
                            clickedOnce = true;
                        else {
                            clickedOnce = false;
                            targetedEnemy = hitSlot.taken.soldierId;
                            //flags[0].MoveActiveToRaycastedPoint(hit);
                            team.ActiveSoldier.AttackSlot(hitSlot);
                            SwapSoldierIfNoTurns(team);
                        }
                    } else {
                        // move to slot
                        attackCommand = -1;
                        targetedEnemy = -1;
                        uiCommandKey = -1;

                        yield return team.ActiveSoldier.StartCoroutine(MoveToSlot(team, team2, hitSlot, path));
                    }
                }

                // fire at nearest enemy
                if (Input.GetKeyDown(KeyCode.Alpha1)) {
                    attackCommand = 2;
                    uiCommandKey = 0;
                    Soldier nearestEnemy = team2.GetNearestTo(team.ActiveSoldier);
                    if (nearestEnemy) {
                        targetedEnemy = nearestEnemy.soldierId;
                        if (!clickedOnce)
                            clickedOnce = true;
                        else {
                            clickedOnce = false;
                            team.ActiveSoldier.AttackSlot(nearestEnemy.curPositionSlot);
                            SwapSoldierIfNoTurns(team);
                        }
                    }
                }

                // grenade throw
                if (hitSlot && Input.GetKeyDown(KeyCode.Alpha3)) {
                    attackCommand = 3;
                    uiCommandKey = 2;
                    if (!clickedOnce) {
                        clickedOnce = true;
                    } else {
                        clickedOnce = false;
                        team.ActiveSoldier.AttackSlot(hitSlot, 1);
                        SwapSoldierIfNoTurns(team);
                    }
                }
                // overwatch
                if (Input.GetKeyDown(KeyCode.Alpha2)) {
                    attackCommand = 4;
                    uiCommandKey = 1;
                    if (!clickedOnce) {
                        clickedOnce = true;
                    } else {
                        clickedOnce = false;
                        team.ActiveSoldier.ToOverwatch();
                        SwapSoldierIfNoTurns(team);
                    }
                }
                // reload
                if (Input.GetKeyDown(KeyCode.R)) {
                    attackCommand = 5;
                    uiCommandKey = 3;
                    if (!clickedOnce) {
                        clickedOnce = true;
                    } else {
                        clickedOnce = false;
                        team.ActiveSoldier.Reload();
                        SwapSoldierIfNoTurns(team);
                    }
                }

                // FIXED: it will work to click on enemy with right click and then 1.
                // makes sure you can't do mouse+something else attack
                if (attackCommand != lastCommand && lastCommand != 0) {
                    clickedOnce = false;
                }

                // tabbing swaps units
                if (Input.GetKeyDown(KeyCode.Tab)) {
                    if (targetedEnemy == -1)
                        SwapSoldier(team);
                    else {
                        SwapTarget(team);
                    }
                }

                // *** ENEMIES *** deprecated
                // if enemy moves, trigger all overwatched player's soldiers

                // AI: move all enemies
                /*for (int i = 0; i < team2.units.Count; i++) {
                    // trigger player's overwatch
                    HandleOverwatchWithoutFog(team2.units[i], team);
                }*/
            }
            yield return null;
        }
    }

    public IEnumerator MoveToSlot(Team team, Team team2, GridSlot slot, MapNode[] path) {
        // coroutines seem to take a little time to start, and since they run parallel, moving doesn't start yet.
        team.ActiveSoldier.moving = true;
        GameplayManager.m.StartCoroutine(team.ActiveSoldier.MoveToSlot(slot, path));
        // enemies in overwatch fire at player's soldier when it moves.
        float t = Time.time;
        float r = 0.5f;
        while (team.ActiveSoldier.moving) {
            if (Time.time > t) {
                t = Time.time + r;
                yield return GameplayManager.m.StartCoroutine(HandleOverwatchWithoutFog(team.ActiveSoldier, team2));
            } else yield return null;
        }
        //Debug.Log("moving done");
        //yield return team.ActiveSoldier.CinematicsDone();
        //team.ActiveSoldier.HandleCover();
        team.ActiveSoldier.HandleCoverAfterMovement();
        //Debug.Log("cover check");
        SwapSoldierIfNoTurns(team);
    }

    public void Reset(Team team) {
        for (int i = 0; i < team.units.Count; i++) {
            team.units[i].ResetActions();
        }
    }
}
