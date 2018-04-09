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
    
    SoldierAnimatorController animations;

    // gizmos
    const float climbDetectionRange = 0.8f;
    bool running = false;
    
    private void Start() {
        RegisterSoldier();
        animations = GetComponent<SoldierAnimatorController>();
        if (animations == null) {
            Debug.LogWarning("Missing SoldierAnimatorController component on "+name+". Could be intentional.", transform);
        }
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

    internal void HandleCover() {
        if (animations) {
            if (MapGrid.OnlyLowCover(curPositionSlot)) {
                animations.RunAnimation("Crouch");
            } else {
                animations.StopAnimation("Crouch");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">assume it's reversed</param>
    /// <returns></returns>
    private IEnumerator Cinematics_MoveOnPath(MapNode[] hitSlot) {
        int movementMode = 1;
        // Note: maybe need different animations when going over wall.(node has different height)
        if (animations)
            animations.RunAnimation("Run");
        running = false;
        //cinematicsRunning = true;
        for (int i = 0; i < hitSlot.Length; i++) {
            MapNode node = hitSlot[hitSlot.Length - i - 1];
            while (Vector3.Distance(transform.position, node.pos) > Time.deltaTime * movementSpeed) {
                if (movementMode == 0) { // moves towards point
                    Vector3 dir = node.pos - transform.position;
                    float slowDown = i == 0 ? Mathf.Clamp(dir.magnitude, 0f, 1f) : 1f;
                    transform.Translate(dir.normalized * slowDown * Time.deltaTime * movementSpeed);
                }else 
                if (movementMode == 1) { // >active<moves and rotates towards point, standard for animated characters
                    Debug.Log("move 1");
                    running = true;
                    Vector3 dir = node.pos - transform.position;
                    float slowDown = i == 0 ? Mathf.Clamp(dir.magnitude, 0f, 1f) : 1f;
                    transform.forward = dir.normalized;
                    transform.Translate(Vector3.forward * slowDown * Time.deltaTime * movementSpeed);

                    RaycastHit h = UpRaycast(transform.position+offset, climbDetectionRange);
                    if (h.transform!= null && h.transform.tag == "JumpOver") {
                        if (animations)
                            animations.TriggerAnimation("ClimbOver");
                    }
                }
                yield return null;
            }
        }
        running = false;
        //cinematicsRunning = false;
        /* 2 ways:
         * - raycast
         * constant short forward raycast
         * -- Flimsy?
         * - collisions
         * on hit, 
         * ++ ez to implement, non general solution
         * */

        if (animations) {
            animations.StopAnimation("Run");
        }
    }
    public Vector3 offset = new Vector3(0, 1, 0);
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Debug.Log("draw gizmos");
        //if(running)
            Gizmos.DrawRay(transform.position+offset, transform.forward * climbDetectionRange);
    }

    RaycastHit UpRaycast(Vector3 point, float len) {
        RaycastHit hit;
        Ray ray = new Ray(point, transform.forward*len);
        bool cast = Physics.Raycast(ray,
            out hit,
            len,
            1 << LayerMask.NameToLayer(GridSlot.groundLayerName),
            QueryTriggerInteraction.Ignore
            );
        if (cast) {
            //return hit.point;
        }
        return hit;
    }

    internal IEnumerator Cinematics_Shoot(GridSlot slot) {
        cinematicsRunning = true;
        yield return StartCoroutine(Cinematics_StandAndTurn(slot));
        if (animations)
            animations.TriggerAnimation("Shoot");
        cinematicsRunning = false;
        // Will automatically return to correct state, courch or stand
    }

    private IEnumerator Cinematics_StandAndTurn(GridSlot hitSlot) {
        // Note: if crouching, stand animation
        // Note: then turn to target first.
        // Note(2): add slow turn or smth
        transform.forward = (hitSlot.transform.position - transform.position).normalized;
        yield return new WaitForSeconds(0.5f);
        
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
                StartCoroutine(Cinematics_Throw("Grenade type 1", hitSlot));
            } else {
                // single shot
                Soldier otherUnit = hitSlot.taken;
                otherUnit.Damage(1);
                StartCoroutine(Cinematics_Shoot(hitSlot));
            }
        }
        return attackCanHappen;
    }

    private IEnumerator Cinematics_Throw(string projectile, GridSlot hitSlot) {
        cinematicsRunning = false;
        yield return StartCoroutine(Cinematics_StandAndTurn(hitSlot));
        if (animations)
            animations.TriggerAnimation("Throw");
        // Will automatically return to correct state, courch or stand
        cinematicsRunning = false;
    }

    /// <summary>
    /// Use in cinematics update, to make sure all soldiers run their animations.
    /// Note: currently overwatch shoot animations come AFTER move animations.
    /// </summary>
    /// <returns></returns>
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
