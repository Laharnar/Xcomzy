using System.Collections;
public interface ITurnCycle {

    IEnumerator TurnCycle(Team team, Team team2);
    void Reset(Team team);
}
