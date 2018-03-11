﻿using System;
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

    private void Start() {
        RegisterSoldier();
    }

    private void RegisterSoldier() {
        soldierId = soldierCounts[allianceId];
        soldierCounts[allianceId]++;
    }

    /*private void TryAttackSlot(RaycastHit hit, SoldierAttack attack) {
        throw new NotImplementedException();
    }*/

    public bool MoveToSlot(GridSlot hitSlot) {
        bool moveCanHappen = hitSlot.taken == false;
        if (moveCanHappen) {
            // change which slots are taken
            if (curPositionSlot)
                curPositionSlot.taken = null;
            hitSlot.taken = this;
            curPositionSlot = hitSlot;
            transform.position = hitSlot.transform.position;
        }
        return moveCanHappen;
    }

    internal bool AttackSlot(GridSlot hitSlot, int attackType = 0) {
        bool attackCanHappen = 
            // gun shot at enemy.
            (attackType == 0 && hitSlot.taken != null && hitSlot.taken.allianceId != allianceId);

        if (attackCanHappen) {
            Soldier unit = hitSlot.taken;
            unit.Damage(1);
        }
        return attackCanHappen;
    }

    private void Damage(int v) {
        hp -= v;
        if (hp<=0) {
            Destroy(gameObject);
        }
    }
}

/*public abstract class SoldierAttack {

}*/
