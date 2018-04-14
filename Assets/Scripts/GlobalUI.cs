using UnityEngine;

public class GlobalUI:MonoBehaviour {

    public RectTransform[] abilitiesUi;
    public RectTransform selectedSkillCursor;

    public RectTransform[] targetedEnemiesUi;
    public RectTransform selectedEnemyCursor;

    void Update() {
        UpdateSelectedAbility(GameplayManager.IsPlayerTurn);
        UpdateSelectedEnemyTarget(GameplayManager.IsPlayerTurn);
    }

    public void UpdateSelectedAbility(bool visible) {
        for (int i = 0; i < abilitiesUi.Length; i++) {
            abilitiesUi[i].gameObject.SetActive(visible);
        }
        PlayerTurnCycle ptc = ((PlayerTurnCycle)GameplayManager.m.playerFlag.cycle);
        if (visible == false || ptc.uiCommandKey == -1)
            selectedSkillCursor.position = new Vector3(-3, 0, 0);
        else
            selectedSkillCursor.position = abilitiesUi[ptc.uiCommandKey].position;
    }

    public void UpdateSelectedEnemyTarget(bool visible) {
        for (int i = 0; i < targetedEnemiesUi.Length; i++) {
            targetedEnemiesUi[i].gameObject.SetActive(visible);
        }

        PlayerTurnCycle ptc = ((PlayerTurnCycle)GameplayManager.m.playerFlag.cycle);
        if (visible == false || ptc.targetedEnemy == -1)
            selectedEnemyCursor.position = new Vector3(-3, 0, 0);
        else
            selectedEnemyCursor.position = targetedEnemiesUi[ptc.targetedEnemy].position;
    }
}
