using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    public Vector3 defaultOffset;
    // Update is called once per frame
    void LateUpdate () {
        transform.position = GameplayManager.m.playerFlag.ActiveSoldier
            .transform.position + defaultOffset;
    }

    internal static void LockOn(Soldier soldier) {
        //throw new NotImplementedException();
    }
}
