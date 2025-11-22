using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class BotSpawner : NetworkBehaviour
{
    [SerializeField] private PlayerHealth botPrefab;
    [SerializeField] private List<Transform> spawnPoints;

    protected override void OnSpawned(bool asServer) {
        base.OnSpawned(asServer);
        Debug.Log("Spawned");
        if (asServer) {
            Debug.Log("ServerSpawned");
            foreach (var spawnPoint in spawnPoints) { 
                var bot = Instantiate(botPrefab, spawnPoint.position, spawnPoint.rotation);
                bot.GiveOwnership(PlayerID.Server);
                Debug.Log( bot + " " + PlayerID.Server);
            }
        }
    }
}
