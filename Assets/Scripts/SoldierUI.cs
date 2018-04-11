using UnityEngine;
using UnityEngine.UI;
public class SoldierUI :MonoBehaviour {
    public Soldier source;
    public Image coverNone, coverLow, coverHigh;

    public Image coverImgTarget;

    public Image[] hpUi;

    public Image[] ammoUi;



    private void Update() {
        bool showGlobalUi = false;
        if (GameplayManager.m.playerFlag.ActiveSoldier.soldierId == source.soldierId) {
            showGlobalUi = true;
        }

        UpdateHpUi();
        UpdateAmmoUi(showGlobalUi);
        UpdateCoverUI();
    }

    public void UpdateCoverUI() {
        switch (source.curCoverHeight) {
            case 0:
                coverNone.enabled = true;
                coverLow.enabled = false;
                coverHigh.enabled = false;
                break;
            case 1:
                coverLow.enabled = true;
                coverNone.enabled = false;
                coverHigh.enabled = false;
                break;
            case 2:
                coverHigh.enabled = true;
                coverNone.enabled = false;
                coverLow.enabled = false;
                break;
        }
        /*switch (source.curCoverHeight) {
            case 0:
                coverImgTarget.sprite = coverNone;
                break;
            case 1:
                coverImgTarget.sprite = coverLow;
                break;
            case 2:
                coverImgTarget.sprite = coverHigh;
                break;
        }*/
    }

    public void UpdateHpUi() {
        for (int i = 0; i < hpUi.Length; i++) {
            hpUi[i].enabled = i < source.hp;
        }
    }
    public void UpdateAmmoUi(bool visible) {
        for (int i = 0; i < ammoUi.Length; i++) {
            ammoUi[i].enabled = visible && i < source.gun.ammoLeft;
        }
    }

    
}
