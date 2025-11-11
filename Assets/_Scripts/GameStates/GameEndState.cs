using System.Collections.Generic;
using System.Linq;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class GameEndState : StateNode
{
    public override void Enter(bool asServer) {
        base.Enter(asServer);
        if (!asServer) return;

        if (!InstanceHandler.TryGetInstance(out ScoreManager scoreManager)) {
            Debug.Log($"GameEndState failed to get scoremanager!", this);
            return;
        }

        var winner = scoreManager.GetWinner();
        if (winner == default) {
            Debug.Log($"GameEndState failed to get winner!", this);
            return;
        }

        if (!InstanceHandler.TryGetInstance(out EndGameView endGameView)) {
            Debug.Log("GameEndState failed to get end game view!", this);
            return;
        }
        
        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager)) {
            Debug.Log("GameEndState failed to get gameViewManager!", this);
            return;
        }

        
        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>();
        Debug.Log($"Game has now ended! winner: {winner}");
    }
}
