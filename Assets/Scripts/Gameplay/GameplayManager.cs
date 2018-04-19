using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls enemy turn/player turn cycle and player's soldier cycle
/// </summary>
public class GameplayManager : MonoBehaviour {

    public static GameplayManager m;

    Team[] flags = new Team[2];

    public Team playerFlag { get { return flags[0]; } }
    public Team enemyFlag { get { return flags[1]; } }

    public static bool IsPlayerTurn { get; internal set; }

    public GlobalUI ui;

    public int[] moveScores;
    public int[] enemyScores;

    public bool drawPlayerUi = true;

    // Use this for initialization
    void Start() {

        m = this;

        for (int i = 0; i < flags.Length; i++) {
            flags[i] = new Team();
        }
        playerFlag.cycle = new PlayerTurnCycle();
        enemyFlag.cycle = new EnemyTurnCycle();

        // find and sort all units in scene by their alliance
        List<Soldier> allUnits = FindObjectsOfType<Soldier>().ToList();
        for (int i = 0; i < allUnits.Count; i++) {
            flags[allUnits[i].allianceId].units.Add(allUnits[i]);
        }

        for (int i = 0; i < flags.Length; i++) {
            flags[i].SnapAllUnitsToGround();
        }

        StartCoroutine(TurnHandler());
    }

    private IEnumerator TurnHandler() {
        for (int i = 0; i < flags.Length; i++) {
            yield return StartCoroutine(flags[i].HandleCoverForAll());
        }
        while (true) {
            for (int i = 0; i < flags.Length; i++) {
                IsPlayerTurn = i == 0;
                flags[i].cycle.Reset(flags[i]);
                if (flags[i].cycle != null) {
                    yield return StartCoroutine(flags[i].cycle.TurnCycle(flags[i], flags[(i + 1) % flags.Length]));
                    Debug.Log("End cycle");
                }
                Debug.Log("End 1 side turn");
            }
            yield return null;
        }
    }
}
