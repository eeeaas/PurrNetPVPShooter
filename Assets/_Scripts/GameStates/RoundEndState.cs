using System.Collections;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class RoundEndState : StateNode
{
    [SerializeField] private float _delay = 5f;
    [SerializeField] private float _restartGameDelay = 7f;
    [SerializeField] private StateNode spawningState; // следующая стадия спавна
    [SerializeField] private StateNode startState; // следующая стадия спавна
    [SerializeField] private int roundsToWin = 3; // сколько побед нужно для выигрыша

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        if (!asServer) return;

        CheckForGameEnd();
    }

    private void CheckForGameEnd()
    {
        if (!InstanceHandler.TryGetInstance(out RoundManager roundManager))
        {
            Debug.LogError("RoundEndState: RoundManager not found!");
            return;
        }

        int teamAScore = roundManager.TeamAScore;
        int teamBScore = roundManager.TeamBScore;


        // проверка на победу
        if (teamAScore >= roundsToWin)
        {
            EndGame(TeamID.TeamA);
        }
        else if (teamBScore >= roundsToWin)
        {
            EndGame(TeamID.TeamB);
        }
        else
        {
            // если никто не достиг нужного количества побед — новый раунд через задержку
            StartCoroutine(DelayNextState());
        }
    }

    private IEnumerator DelayNextState()
    {
        yield return new WaitForSeconds(_delay);
        machine.SetState(spawningState);
    }

    private IEnumerator IRestartAllGame() {
        yield return new WaitForSeconds(_restartGameDelay);
        if (isServer) {
            RestartAllGame();
            machine.SetState(startState);
        }
    }
    
    [ObserversRpc(runLocally:true)]
    private void EndGame(TeamID winner)
    {
        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager)) return;
        if (!InstanceHandler.TryGetInstance(out EndGameView endGameView)) return;

        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>();

        StartCoroutine(IRestartAllGame());
        Debug.Log($"Game End! Winner: {winner}");
    }

    [Server]
    private void RestartAllGame() {
        if (!InstanceHandler.TryGetInstance(out RoundManager roundManager))
        {
            Debug.LogError("RoundEndState: RoundManager not found!");
            return;
        }
        RestartUI();
        roundManager.ResetScores();
    }

    [ObserversRpc(runLocally:true)]
    private void RestartUI() {
        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager)) return;
        if (!InstanceHandler.TryGetInstance(out EndGameView endGameView)) return;
        if (!InstanceHandler.TryGetInstance(out MainGameView mainGameView))
        {
            Debug.LogError("RoundEndState: RoundManager not found!");
            return;
        }
        gameViewManager.HideView<EndGameView>();
        gameViewManager.ShowView<MainGameView>();
        mainGameView.UpdateScore(0,0);
    }
}