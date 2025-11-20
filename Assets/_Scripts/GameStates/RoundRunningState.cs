using System.Collections.Generic;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundRunningState : StateNode<List<PlayerHealth>> {
    public override void Enter(List<PlayerHealth> data, bool asServer) {
        base.Enter(data, asServer);
        if (!asServer) return;
        foreach (var player in data) {
            player.OnDeath_Server += OnPlayerDeath;
        }
    }

    private void OnPlayerDeath(PlayerID deadPlayer) {
        if (!InstanceHandler.TryGetInstance(out TeamManager teamManager)) return;

        var winner = teamManager.GetWinningTeam();
        if (winner != TeamID.None) {
            if (InstanceHandler.TryGetInstance(out RoundManager roundManager)) {
                roundManager.AddRoundWin(winner);
            }
            machine.Next(); // конец раунда
        }
    }
}

