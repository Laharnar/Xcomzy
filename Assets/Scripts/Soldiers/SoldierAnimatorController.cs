using System;
using UnityEngine;

public class SoldierAnimatorController :MonoBehaviour{
    public Animator anim;
    public void RunAnimation(string v) {
        anim.SetBool(v, true);
    }

    public void StopAnimation(string v) {
        anim.SetBool(v, false);
    }

    public void TriggerAnimation(string v) {
        anim.SetTrigger(v);
    }
}