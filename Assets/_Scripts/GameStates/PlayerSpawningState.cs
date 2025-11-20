using System.Collections;
using System.Collections.Generic;
using Fragsurf.Movement;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawningState : StateNode {
    [SerializeField] private PlayerHealth playerPrefab;
    [SerializeField] private List<Transform> currentSpawnPoints = new();
    
    [SerializeField] private GameSettingsManager _gameSettings;
    private int mapId;
    
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer) return;
        _gameSettings = InstanceHandler.NetworkManager.GetComponent<GameSettingsManager>();
        StartCoroutine(WaitPlayersThenSpawn());
    }

    private IEnumerator WaitPlayersThenSpawn()
    {
        // Ждем пока в списке players появятся реальные объекты
        while (networkManager.players.Count == 0)
        {
            yield return null;
        }

        Debug.Log("Players ready, spawning!");

        //FindSpawnPoints();
        DespawnPlayers();
        
        var spawnedPlayers = SpawnPlayers();
        machine.Next(spawnedPlayers);
    }

    /*
    private void FindSpawnPoints()
    {
        spawnPoints.Clear();

        int sceneCount = SceneManager.sceneCount;

        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            // Пропускаем LobbySample, ищем только реальные игровые сцены
            if (scene.name == "LobbySample") 
                continue;

            GameObject[] roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    if (t.CompareTag("SpawnPoint"))
                        spawnPoints.Add(t);
                }
            }
        }

        Debug.Log($"[PlayerSpawningState] Found {spawnPoints.Count} spawn points across loaded scenes.");
    }*/

    private List<PlayerHealth> SpawnPlayers() {
        var spawnedPlayers = new List<PlayerHealth>();
        if (!InstanceHandler.TryGetInstance(out TeamManager teamManager)) return spawnedPlayers;

        teamManager.Clear();

        int currentSpawnIndex = 0;
        int i = 0;
        foreach (var player in networkManager.players) {
            var spawnPoint = currentSpawnPoints[currentSpawnIndex];
            var newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            newPlayer.GiveOwnership(player);
            newPlayer.GetComponent<PlayerSettingSync>().Rpc_SetAutoBhop(_gameSettings.autoBhop);

            // Назначаем команду
            var teamComp = newPlayer.GetComponent<PlayerTeam>();
            var team = (i % 2 == 0) ? TeamID.TeamA : TeamID.TeamB;
            teamComp.SetTeam(team);

            teamManager.RegisterPlayer(newPlayer);
            spawnedPlayers.Add(newPlayer);
            
            currentSpawnIndex = (currentSpawnIndex + 1) % currentSpawnPoints.Count;
            i++;
        }

        return spawnedPlayers;
    }

    private void DespawnPlayers() {
        var allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var player in allPlayers) {
            Destroy(player.gameObject);
        }
    }

    public override void Exit(bool asServer) {
        base.Exit(asServer);
    }
}
