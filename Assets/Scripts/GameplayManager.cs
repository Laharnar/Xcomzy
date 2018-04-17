using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITurnCycle {

    IEnumerator TurnCycle(Team team, Team team2);
    void Reset(Team team);
}
 class EnemyTurnCycle : ITurnCycle {


    public int[] moveScores;
    public int[] enemyScores;
    public void Reset(Team team) {
        for (int i = 0; i < team.units.Count; i++) {
            team.units[i].ResetActions();
        }
    }

    // Handles cover differently.
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
        yield return team.ActiveSoldier.CinematicsDone();
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

    internal void SwapSoldier(Team team) {
        team.NextSoldier();
        PlayerCamera.ResetFocus();
    }

    public IEnumerator TurnCycle(Team team, Team team2) {
        // AI calculations list of commands
        // yield return execute calculated commands and moves
        yield return new WaitForSeconds(5);
        Debug.Log("End Enemy turn");
        /*stategies:
         Scoring nearby slots based on utility.
         n^2 n = around 20 = 400 per character
         
        */
        for (int i = 0; i < team.units.Count; i++) {
            Soldier unit = team.units[i];

            //Generate data for all slots in move range and for soldiers in range that can be attacked. taken move slots are excluded.
            GridSlot[] slots = GridSlot.GetAvaliableSlotsInMoveRange(unit.curPositionSlot, unit.fullMovementRange);
            Soldier[] enemiesInRange = GridSlot.GetVisibleEnemySlots(unit, team2);
            AiSlotData[] moveData = new AiSlotData[slots.Length];
            AiSlotData[] enemyData = new AiSlotData[enemiesInRange.Length];
            moveScores = new int[moveData.Length];
            enemyScores = new int[enemyData.Length];
            for (int j = 0; j < slots.Length; j++) {
                moveData[j] = new AiSlotData(slots[j].id);
            }
            for (int j = 0; j < enemyData.Length; j++) {
                enemyData[j] = new AiSlotData(enemiesInRange[j].soldierId);
            }
            // Calculations have to be remade for every slots for every enemy. Not good.
            // Utility score.
            // + when enemies are on slot. +100
            // + cover height +0 +50 +100
            // - distance to enemy (gt dist, less)
            // 
            /* how to choose to move vs choose to attack
             * just distance
             * */
            // here we choose which actions will be prioritized, movement or shooting.
            int possibleMoveActions = 0;
            int attack = -1;
            if (enemiesInRange.Length == 0) {
                continue;
            }
            attack = 0; // shoot, when there are some enemies
            float dist = Vector3.Distance(enemiesInRange[0].transform.position, unit.transform.position);
            if (dist <= unit.movementRange1) {
                possibleMoveActions = 1;
            }
            if (enemiesInRange.Length == 0) {
                possibleMoveActions = 2;
                attack = -1;
            }

            int bestId = -1;
            int best = 0;
            // calculates scores for movement slots
            for (int j = 0; j < moveData.Length; j++) {
                moveData[j].score = (int)(ClampedReverseDist(unit, unit.fullMovementRange, slots, j) 
                    * MapGrid.CoverScoreMultiplier(slots[j]));
                if (moveData[j].score > best) {
                    best = moveData[j].score;
                    bestId = j;
                }
                moveScores[j] = moveData[j].score;
            }
                GameplayManager.m.moveScores = moveScores;
            int bestMoveDataId = bestId;
            GridSlot bestMove = slots[bestMoveDataId];
            MapNode[] path = Pathfinding.FindPathAStar(unit.curPositionSlot.transform.position, bestMove.transform.position, MapGrid.wholeMap);
            yield return unit.StartCoroutine(MoveToSlot(team, team2, bestMove, path));

            if (attack != -1) {
                // calculates scores for enemies
                bestId = -1;
                for (int j = 0; j < enemyData.Length; j++) {
                    enemyData[j].score = (int)(ClampedDist(enemiesInRange[j], 100, slots, j)
                        * MapGrid.CoverScoreMultiplier(enemiesInRange[j].curPositionSlot));
                    if (enemyData[j].score > best) {
                        best = enemyData[j].score;
                        bestId = j;
                    }
                    enemyScores[j] = enemyData[j].score;
                }
                int bestEnemyDataId = bestId;
                if (bestEnemyDataId != -1) {
                    Soldier bestEnemy = enemiesInRange[bestEnemyDataId];

                    unit.AttackSlot(bestEnemy.curPositionSlot);
                    GameplayManager.m.enemyScores = enemyScores;

                } else {
                    Debug.Log("No enemies in range");
                }
            }

            team.ActiveSoldier.HandleCover();
            SwapSoldier(team);
        }
    }

    private int ClampedDist(Soldier unit, float fullMovementRange, GridSlot[] slots, int j) {
        return (int)Mathf.Clamp(
                        Vector3.Distance(slots[j].transform.position, unit.transform.position), 0f, fullMovementRange);
    }

    /// <summary>
    /// further: less score, clamped at full movement range.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="coverInRange"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    private static int ClampedReverseDist(Soldier unit, float maxRange, GridSlot[] slots, int j) {
        return (int)maxRange - (int)Mathf.Clamp(
                        Vector3.Distance(slots[j].transform.position, unit.transform.position), 0f, maxRange);
    }

    public class AiSlotData {
        public int score;
        public int slotIndexInSourceArr;

        public AiSlotData(int slotIndexInSourceArr) {
            score = 0;
            this.slotIndexInSourceArr = slotIndexInSourceArr;
        }
    }
}

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
        yield return team.ActiveSoldier.CinematicsDone();
        team.ActiveSoldier.HandleCover();
        //Debug.Log("cover check");
        SwapSoldierIfNoTurns(team);
    }

    public void Reset(Team team) {
        for (int i = 0; i < team.units.Count; i++) {
            team.units[i].ResetActions();
        }
    }
}

/// <summary>
/// Controls enemy turn/player turn cycle and player's soldier cycle
/// </summary>
public class GameplayManager : MonoBehaviour {

    public static GameplayManager m;

    Team[] flags = new Team[2];

    public Team playerFlag { get { return flags[0]; } }
    public Team enemyFlag { get { return flags[1]; } }

    public static bool IsPlayerTurn { get; internal set; }

    public GlobalUI ui;

    public int[] moveScores;
    public int[] enemyScores;

    // Use this for initialization
    void Start() {

        m = this;

        for (int i = 0; i < flags.Length; i++) {
            flags[i] = new Team();
        }
        playerFlag.cycle = new PlayerTurnCycle();
        enemyFlag.cycle = new EnemyTurnCycle();

        // find and sort all units in scene by their alliance
        List<Soldier> allUnits = FindObjectsOfType<Soldier>().ToList();
        for (int i = 0; i < allUnits.Count; i++) {
            flags[allUnits[i].allianceId].units.Add(allUnits[i]);
        }

        for (int i = 0; i < flags.Length; i++) {
            flags[i].SnapAllUnitsToGround();
        }

        StartCoroutine(TurnHandler());
    }

    private IEnumerator TurnHandler() {
        while (true) {
            for (int i = 0; i < flags.Length; i++) {
                IsPlayerTurn = i == 0;
                flags[i].cycle.Reset(flags[i]);
                if (flags[i].cycle != null) {
                    yield return StartCoroutine(flags[i].cycle.TurnCycle(flags[i], flags[(i + 1) % flags.Length]));
                    Debug.Log("End cycle");
                }
                Debug.Log("End 1 side turn");
            }
            yield return null;
        }
    }
}
