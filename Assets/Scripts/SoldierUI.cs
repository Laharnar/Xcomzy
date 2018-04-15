using UnityEngine;
using UnityEngine.UI;
public class SoldierUI :MonoBehaviour {
    public Soldier source;
    public Image coverNone, coverLow, coverHigh;

    public Image coverImgTarget;

    public Image[] hpUi;

    public Image[] ammoUi;
    public Image ammoBackground;

    public Image[] actionsUi;


    private void Update() {
        bool showGlobalUi = false;
        if (GameplayManager.IsPlayerTurn 
            && GameplayManager.m.playerFlag.ActiveSoldier.soldierId == source.soldierId) {
            showGlobalUi = true;
        }
        UpdateHpUi();
        UpdateAmmoUi(showGlobalUi);
        UpdateCoverUI();
        UpdateActionsUi(true);
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
        ammoBackground.enabled = visible;

        for (int i = 0; i < ammoUi.Length; i++) {
            ammoUi[i].enabled = visible && i < source.gun.ammoLeft;
        }
    }

    public void UpdateActionsUi(bool visible) {
        for (int i = 0; i < actionsUi.Length; i++) {
            actionsUi[i].enabled = visible && i < source.actionsLeft;
        }
    }
}
