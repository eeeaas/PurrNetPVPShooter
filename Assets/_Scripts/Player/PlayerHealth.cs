using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerHealth : NetworkBehaviour {
    [SerializeField] private SyncVar<int> health = new(100);
    [SerializeField] private int selfLayer, otherLayer;

    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private SoundPlayer soundPlayerPrefab;
    [SerializeField, Range(0f, 1f)] private float deathVolume = 0.5f;
    [SerializeField] private Transform hitboxRoot;

    public Action<PlayerID> OnDeath_Server;

    public int Health => health;

    protected override void OnSpawned() {
        base.OnSpawned();
        
        //var actualLayer = isOwner ? selfLayer : otherLayer;
        //SetLayerRecursively(gameObject, actualLayer);
        if (isOwner) {
            InstanceHandler.GetInstance<MainGameView>().UpdateHealth(health.value);
            health.onChanged += OnHealthChanged;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        health.onChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int newHealth) {
        InstanceHandler.GetInstance<MainGameView>().UpdateHealth(newHealth);
    }

    private void SetLayerRecursively(GameObject obj, int layer) {
        obj.layer = layer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public void ChangeHealth(int amount, PlayerID shooter) {
        if (!isServer) {
            Debug.Log("Only the server should change player health!");
            return;
        }

        health.value += amount;

        if (health <= 0) {
            if (InstanceHandler.TryGetInstance(out ScoreManager scoreManager)) {
                scoreManager.AddKill(shooter);
                if(owner.HasValue)
                    scoreManager.AddDeath(owner.Value);
            }
            OnPlayerDeathRpc();
            PlayDeathEffects();
            if(owner.HasValue)
                OnDeath_Server?.Invoke(owner.Value);
            Destroy(gameObject);
        }
    }
    
    [ObserversRpc(runLocally: true)]
    private void OnPlayerDeathRpc()
    {
        // Если это владелец — активируем spectator-камеру
        if (isOwner)
        {
            Debug.Log("[PlayerHealth] Owner died — switching to spectator mode");
            // Попробуем включить spectator-режим через PlayerCameraManager
            if (InstanceHandler.TryGetInstance(out PlayerCameraManager camManager))
            {
                camManager.EnableSpectatorMode();
            }
            else
            {
                Debug.LogWarning("PlayerCameraManager not found — cannot switch camera!");
            }
        }
    }


    [ObserversRpc(runLocally: true)]
    private void PlayDeathEffects() {
        Instantiate(deathParticles, transform.position + Vector3.up, Quaternion.identity);
        var soundPlayer = Instantiate(soundPlayerPrefab, transform.position + Vector3.up, Quaternion.identity);
        FMODUnity.RuntimeManager.PlayOneShot("event:/Deaths3D", soundPlayer.transform.position);
    }
}
