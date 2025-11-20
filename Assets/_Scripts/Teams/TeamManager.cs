using System.Collections.Generic;
using System.Linq;
using PurrNet;
using UnityEngine;

public class TeamManager : NetworkBehaviour {
    private readonly Dictionary<TeamID, List<PlayerHealth>> teams = new();
    [SerializeField] private int roundsToWin = 3; // количество побед для выигрыша игры


    private void Awake() {
        InstanceHandler.RegisterInstance(this);
        teams[TeamID.TeamA] = new();
        teams[TeamID.TeamB] = new();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        InstanceHandler.UnregisterInstance<TeamManager>();
    }

    [Server]
    public void RegisterPlayer(PlayerHealth player) {
        var teamComp = player.GetComponent<PlayerTeam>();
        if (teamComp == null) return;

        var team = teamComp.Team;
        if (!teams.ContainsKey(team))
            teams[team] = new List<PlayerHealth>();

        teams[team].Add(player);
    }

    [Server]
    public void UnregisterPlayer(PlayerHealth player) {
        foreach (var list in teams.Values)
            list.Remove(player);
    }

    [Server]
    public bool IsTeamEliminated(TeamID team) {
        return teams[team].All(p => p == null || p.Health <= 0);
    }

    [Server]
    public TeamID GetWinningTeam() {
        bool teamADead = IsTeamEliminated(TeamID.TeamA);
        bool teamBDead = IsTeamEliminated(TeamID.TeamB);

        if (teamADead && !teamBDead) return TeamID.TeamB;
        if (teamBDead && !teamADead) return TeamID.TeamA;

        return TeamID.None; // Ничья или ещё не закончено
    }

    [Server]
    public void Clear() {
        teams[TeamID.TeamA].Clear();
        teams[TeamID.TeamB].Clear();
    }
    
    [Server]
    public void OnPlayerDied(PlayerID deadPlayer)
    {
        // Проверяем, вымерла ли какая-то команда
        var winner = GetWinningTeam();
        if (winner != TeamID.None) {
            if (InstanceHandler.TryGetInstance(out RoundManager roundManager))
                roundManager.AddRoundWin(winner);
        }
    }

}