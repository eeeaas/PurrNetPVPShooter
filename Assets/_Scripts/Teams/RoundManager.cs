using PurrNet;
using UnityEngine;

public class RoundManager : NetworkBehaviour {
    [SerializeField] private SyncVar<int> teamAScore = new();
    [SerializeField] private SyncVar<int> teamBScore = new();
    public int TeamAScore => teamAScore.value;
    public int TeamBScore => teamBScore.value;

    private void Awake() {
        InstanceHandler.RegisterInstance(this);
        teamAScore.onChanged += _ => UpdateUI();
        teamBScore.onChanged += _ => UpdateUI();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        InstanceHandler.UnregisterInstance<RoundManager>();
    }

    [Server]
    public void AddRoundWin(TeamID team) {
        if (team == TeamID.TeamA) teamAScore.value++;
        else if (team == TeamID.TeamB) teamBScore.value++;
        UpdateUI();
    }

    private void UpdateUI() {
        if (InstanceHandler.TryGetInstance(out MainGameView mainGameView))
            mainGameView.UpdateScore(teamAScore.value, teamBScore.value);
    }

    [Server]
    public TeamID GetMatchWinner() {
        if (teamAScore.value > teamBScore.value) return TeamID.TeamA;
        if (teamBScore.value > teamAScore.value) return TeamID.TeamB;
        return TeamID.None;
    }

    [Server]
    public void ResetScores() {
        teamAScore.value = 0;
        teamBScore.value = 0;
        UpdateUI();
    }
}