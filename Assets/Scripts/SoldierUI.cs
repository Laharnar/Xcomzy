using UnityEngine;
using UnityEngine.UI;
public class SoldierUI :MonoBehaviour {
    public Soldier source;
    public Sprite coverNone, coverLow, coverHigh;

    public Image coverImgTarget;

    public Image[] hpUi;

    public Image[] ammoUi;

    public RectTransform[] abilitiesUi;
    public RectTransform selectedAbilityCursor;

    public void UpdateCoverUI() {
        switch (source.curCoverHeight) {
            case 0:
                coverImgTarget.sprite = coverNone;
                break;
            case 1:
                coverImgTarget.sprite = coverLow;
                break;
            case 2:
                coverImgTarget.sprite = coverHigh;
                break;
        }
    }

    public void UpdateHpUi() {
        for (int i = 0; i < hpUi.Length; i++) {
            hpUi[i].enabled = i < source.hp;
        }
    }
    public void UpdateAmmoUi() {
        for (int i = 0; i < ammoUi.Length; i++) {
            ammoUi[i].enabled = i < source.gun.ammoLeft;
        }
    }
    public void UpdateSelectedAbility() {
        if (GameplayManager.m.attackCommand == -1)
            selectedAbilityCursor.position = new Vector3(0, 0, 0);
        else 
            selectedAbilityCursor.position = abilitiesUi[GameplayManager.m.attackCommand].position;
    }
}
