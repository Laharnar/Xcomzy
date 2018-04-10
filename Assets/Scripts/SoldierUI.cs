using UnityEngine;
using UnityEngine.UI;
public class SoldierUI :MonoBehaviour {
    public Soldier source;
    public Sprite coverNone, coverLow, coverHigh;

    public Image coverImgTarget;

    void UpdateCoverUI() {
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
}
