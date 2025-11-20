using System.Collections;
using UnityEngine;
using PurrNet;
using PurrNet.StateMachine;

public class WaitForPlayersState : StateNode
{
    [SerializeField] private int minPlayers = 2;

    private GameSettingsManager _gameSettings;

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);
        Debug.Log("[WaitForPlayersState] Enter: asServer=" + asServer);

        if (!asServer) return;  // только сервер запускает логику

        // Получаем GameSettings один раз
        if (_gameSettings == null)
            _gameSettings = FindObjectOfType<GameSettingsManager>();

        if (_gameSettings != null)
            minPlayers = _gameSettings.playerCount;

        // Запускаем корутину
        StartCoroutine(WaitForPlayersCoroutine());
    }

    private IEnumerator WaitForPlayersCoroutine()
    {
        Debug.Log("[WaitForPlayersState] Waiting for players...");
        while (InstanceHandler.NetworkManager.players.Count < minPlayers)
            yield return null;

        Debug.Log("[WaitForPlayersState] Min players connected, switching state");

        // Меняем состояние на сервере
        machine.Next();
    }
}