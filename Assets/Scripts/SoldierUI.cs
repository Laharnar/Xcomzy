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

    public Image isSelectedUi;
    public Image hpBackground;

    private void Update() {
        bool showGlobalUiForSoldier = false;
        if (GameplayManager.IsPlayerTurn 
            && GameplayManager.m.playerFlag.ActiveSoldier.soldierId == source.soldierId && GameplayManager.m.drawPlayerUi) {
            showGlobalUiForSoldier = true;
        }
        UpdateHpUi(GameplayManager.m.drawPlayerUi);
        UpdateAmmoUi(showGlobalUiForSoldier);
        UpdateCoverUI(true);
        UpdateActionsUi(GameplayManager.m.drawPlayerUi);
        if (isSelectedUi && GameplayManager.m.drawPlayerUi)
            isSelectedUi.enabled = showGlobalUiForSoldier;
    }

    public void UpdateCoverUI(bool visible) {
        switch (source.curCoverHeight) {
            case 0:
                coverNone.enabled = true && visible;
                coverLow.enabled = false && visible;
                coverHigh.enabled = false && visible;
                break;
            case 1:
                coverLow.enabled = true && visible;
                coverNone.enabled = false && visible;
                coverHigh.enabled = false && visible;
                break;
            case 2:
                coverHigh.enabled = true && visible;
                coverNone.enabled = false && visible;
                coverLow.enabled = false && visible;
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

    public void UpdateHpUi(bool visible) {
        if (hpBackground!= null) {
            hpBackground.enabled = visible;
        }
        for (int i = 0; i < hpUi.Length; i++) {
            hpUi[i].enabled = i < source.hp && visible;
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
