using System;
using System.Collections;
using UnityEngine;

/*public abstract class SoldierAttack {

}*/

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

    public int soldierId { get; private set; }

    /// <summary>
    /// How many soldiers are on each side, 1 integer per alliance id
    /// </summary>
    static int[] soldierCounts = new int[2];
    public int allianceId = 0;

    [Header("Stats")]
    public int hp = 1;
    public float grenadeRange = 5;
    public int actions = 2;
    public int movementRange1 = 8; // ********** TODO, implement limit into movement.
    public int movementRange2 = 7; // ********** TODO, implement limit into movement. summed to mr1

    [Header("Cinematics")]
    public float movementSpeed = 1;

    public Vector3 offset = new Vector3(0, 1, 0);

    public bool inOverwatch = false;
    [SerializeField] bool running = false;
    [SerializeField] bool cinematicsRunning = false;

    SoldierAnimatorController animations;

   

    /// <summary>
    /// Use when updating ui.
    /// Values are 0-2, 0 is ground, 1 half cover, 2 full cover.
    /// </summary>
    public int curCoverHeight { get; private set; }
    public int actionsLeft { get; private set; }
    public int activeCommand { get; private set; } // attack, grenade, etc

    // gizmos
    const float climbDetectionRange = 0.8f;
    // -- end gizmos

    public Gun gun;

    internal bool moving = false;

    public float fullMovementRange { get { return movementRange1 + movementRange2; } }

    private void Start() {
        RegisterSoldier();
        actionsLeft = actions;
        animations = GetComponent<SoldierAnimatorController>();
        if (animations == null) {
            Debug.LogWarning("Missing SoldierAnimatorController component on "+name+". Could be intentional.", transform);
        }
    }
    internal IEnumerator HandleCoverAfterMovement() {
        yield return StartCoroutine(MovementDone());
        Debug.Log("handling cover");
        HandleCover();
    }

    public void SetCurSlot(GridSlot slot) {
        curPositionSlot = slot;
    }

    private void RegisterSoldier() {
        soldierId = soldierCounts[allianceId];
        soldierCounts[allianceId]++;
    }

    internal bool NearEnough(int length) {
        return actionsLeft == 2 ? length <= movementRange1 + movementRange2
             : actionsLeft == 1 ? length <= movementRange2 : false;
    }

    /*private void TryAttackSlot(RaycastHit hit, SoldierAttack attack) {
        throw new NotImplementedException();
    }*/

    internal void HandleCover() {
        if (animations) {
            // TODO: Cover check doesn't work in all directions
            if (GridSlot.OnlyGround(curPositionSlot)) {
                Debug.Log("only ground");
                curCoverHeight = 0;
                animations.StopAnimation("Crouch");
            } else if (GridSlot.OnlyLowCover(curPositionSlot)) {
                Debug.Log("low cover");
                curCoverHeight = 1;
                animations.RunAnimation("Crouch");
            } else {
                Debug.Log("high cover");
                curCoverHeight = 2;
                animations.StopAnimation("Crouch");
            }
        }
    }


    public IEnumerator MoveToSlot(GridSlot hitSlot, MapNode[] path, bool cinematics=true, bool consumeAction = true) {
        if (moving) {
            Debug.LogWarning("Multiple ovelapping coroutine move calls");
        }
        if (path.Length > 0) {
            if (consumeAction) {
                ConsumeActions(1);
            }
            if (hitSlot.taken != null)  // can't move on top of other units
                moving = true;
            Debug.Log("oving:"+moving + cinematics+hitSlot.taken);
            if (cinematics) {
                yield return StartCoroutine(Cinematics_MoveOnPath(path));
            } else
                transform.position = hitSlot.transform.position;
            // change which slots are taken
            if (curPositionSlot)
                curPositionSlot.taken = null;
            Debug.Log("oving done");
        }
        moving = false;
        hitSlot.taken = this;
        curPositionSlot = hitSlot;
        transform.position = hitSlot.transform.position;
        //return true;
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

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;

        //if(running)
            Gizmos.DrawRay(transform.position+offset, transform.forward * climbDetectionRange);
        Gizmos.color = Color.blue;

        Gizmos.DrawRay(transform.position+Vector3.forward*0.8f, Vector3.forward * 2);
    }

    RaycastHit UpRaycast(Vector3 point, float len) {
        RaycastHit hit;
        Ray ray = new Ray(point, transform.forward*len);
        bool cast = Physics.Raycast(ray,
            out hit, len,
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
            || attackType == 1 || attackType == 2;
        if (attackCanHappen) {
            // grenade
            if (attackType == 1) {
                ConsumeActions(2);
                AoeDamage(grenadeRange, hitSlot);
                StartCoroutine(Cinematics_Throw("Grenade type 1", hitSlot));
            } else if (attackType == 0) {
                // single shot
                gun.Fire("Standard");
                ConsumeActions(2);
                Soldier otherUnit = hitSlot.taken;
                StartCoroutine(Cinematics_Shoot(hitSlot));
                otherUnit.Damage(1);
            } else { 
                Debug.Log("Overwatch reaction");
                // overwatch reaction shot
                gun.Fire("Overwatch");
                Soldier otherUnit = hitSlot.taken;
                StartCoroutine(Cinematics_Shoot(hitSlot));
                otherUnit.Damage(1);
            }
        }
        return attackCanHappen;
    }

    internal void ResetActions() {
        actionsLeft = actions;
    }

    private IEnumerator Cinematics_Throw(string projectile, GridSlot hitSlot) {
        ConsumeActions(2);
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
    internal IEnumerator MovementDone() {
        while (moving) {
            yield return null;
        }
    }
    public void Damage(int v) {
        hp -= v;
        if (hp<=0) {
            Destroy(gameObject);
        }
    }
    public void Reload() {
        ConsumeActions(2);
        gun.Reload();
    }

    public void AoeDamage(float range, GridSlot slot) {
        ConsumeActions(2);
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
        ConsumeActions(2);
        inOverwatch = true;
    }

    void ConsumeActions(int num) {
        actionsLeft = Mathf.Clamp(actionsLeft-num, 0, actions);
        Debug.Log("Consumed actions: "+num + " Left:"+actionsLeft);
    }
}
