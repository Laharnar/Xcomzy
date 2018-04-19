using System.Collections;
using UnityEngine;
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
        yield return new WaitForSeconds(2);
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
                    * RayMap.CoverScoreMultiplier(slots[j]));
                if (moveData[j].score > best) {
                    best = moveData[j].score;
                    bestId = j;
                }
                moveScores[j] = moveData[j].score;
            }
            GameplayManager.m.moveScores = moveScores;
            int bestMoveDataId = bestId;
            // execute best move
            GridSlot bestMove = slots[bestMoveDataId];
            MapNode[] path = Pathfinding.FindPathAStar(unit.curPositionSlot.transform.position, bestMove.transform.position, RayMap.wholeMap);
            yield return unit.StartCoroutine(MoveToSlot(team, team2, bestMove, path));

            if (attack != -1) {
                // calculates scores for enemies
                bestId = -1;
                for (int j = 0; j < enemyData.Length; j++) {
                    enemyData[j].score = (int)(ClampedReverseDist(enemiesInRange[j], 100, slots, j)
                        * (10-RayMap.CoverScoreMultiplier(enemiesInRange[j].curPositionSlot)));
                    if (enemyData[j].score > best) {
                        best = enemyData[j].score;
                        bestId = j;
                    }
                    enemyScores[j] = enemyData[j].score;
                }
            }

            // execute best attack
            int bestEnemyId = bestId;
            if (bestEnemyId != -1) {
                Soldier bestEnemy = enemiesInRange[bestEnemyId];

                unit.AttackSlot(bestEnemy.curPositionSlot);
                GameplayManager.m.enemyScores = enemyScores;

            } else {
                Debug.Log("No enemies in range");
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
