using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Soldier moves on grid slots.
/// Attacks units on taken grids.
/// </summary>
public class Soldier : MonoBehaviour {

    //SoldierAttack activeAttack;

    /// <summary>
    /// Which slot it occupies
    /// </summary>
    internal GridSlot curPositionSlot { get; private set; }

    int soldierId = 0;

    /// <summary>
    /// How many soldiers are on each side, 1 integer per alliance id
    /// </summary>
    static int[] soldierCounts = new int[2];


    public int allianceId = 0;

    // stats
    public int hp = 1;
    public float grenadeRange = 5;

    public bool inOverwatch = false;

    [Header("Cinematics")]
    public float movementSpeed = 1;
    bool cinematicsRunning = false;

    private void Start() {
        RegisterSoldier();
    }

    public void SetCurSlot(GridSlot slot) {
        curPositionSlot = slot;
    }

    private void RegisterSoldier() {
        soldierId = soldierCounts[allianceId];
        soldierCounts[allianceId]++;
    }

    /*private void TryAttackSlot(RaycastHit hit, SoldierAttack attack) {
        throw new NotImplementedException();
    }*/

    public bool MoveToSlot(GridSlot hitSlot, bool cinematics=true) {
        
        if (hitSlot.taken == true)  // can't move on top of other units
            return false;
        if (cinematics) {
            
            MapNode[] path = Pathfinding.FindPathAStar(curPositionSlot.transform.position, hitSlot.transform.position, MapGrid.wholeMap);
            if (path.Length == 0)
                return false;
            StartCoroutine(Cinematics_MoveOnPath(path));
        } else
            transform.position = hitSlot.transform.position;
        // change which slots are taken
        if (curPositionSlot)
            curPositionSlot.taken = null;
        hitSlot.taken = this;
        curPositionSlot = hitSlot;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">assume it's reversed</param>
    /// <returns></returns>
    private IEnumerator Cinematics_MoveOnPath(MapNode[] hitSlot) {
        cinematicsRunning = true;
        for (int i = 0; i < hitSlot.Length; i++) {
            MapNode node = hitSlot[hitSlot.Length - i - 1];
            while (Vector3.Distance(transform.position, node.pos)
                > Time.deltaTime * movementSpeed) {
                Vector3 dir = node.pos - transform.position;
                float slowDown = i == 0 ? Mathf.Clamp(dir.magnitude, 0f, 1f) : 1f;
                transform.Translate(dir.normalized * slowDown*Time.deltaTime*movementSpeed);
                yield return null;
            }
        }
        cinematicsRunning = false;
    }
    
    internal bool AttackSlot(GridSlot hitSlot, int attackType = 0) {
        bool attackCanHappen =
            // gun shot at enemy.
            (attackType == 0 && hitSlot.taken != null && hitSlot.taken.allianceId != allianceId)
            || attackType == 1;

        if (attackCanHappen) {
            // grenade
            if (attackType == 1) {
                AoeDamage(grenadeRange, hitSlot);
            } else {
                // single shot
                Soldier otherUnit = hitSlot.taken;
                otherUnit.Damage(1);
            }
        }
        return attackCanHappen;
    }

    internal IEnumerator CinematicsDone() {
        while (cinematicsRunning) {
            yield return null;
        }
    }

    public void Damage(int v) {
        hp -= v;
        if (hp<=0) {
            Destroy(gameObject);
        }
    }

    public void AoeDamage(float range, GridSlot slot) {
        // Make aoe dmg in range
        GridSlot[] slots = GridSlot.GetSlotsInRange(slot, range);

        // Note: also damages allies.
        // Note: Doesn't work if units aren't on slot!
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i].taken)
                slots[i].taken.Damage(1);
        }
    }

    internal void ToOverwatch() {
        inOverwatch = true;
    }
}

/*public abstract class SoldierAttack {

}*/
