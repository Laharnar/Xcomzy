using System;
using UnityEngine;

public class Gun : MonoBehaviour {
    [SerializeField] int ammo=4;
    
    public int ammoLeft { get; private set; }

    public void Fire(string shotType) {
        // standard shot
        if (shotType == "Standard") {
            AmmoDown(1);
        }
        // overwatch shot
        else if (shotType == "Overwatch") {
            AmmoDown(2);
        }
    }

    void AmmoDown(int amt) {
        ammoLeft = Mathf.Clamp(ammoLeft-amt, 0, ammo);
    }

    internal void Reload() {
        ammoLeft = ammo;
    }
}