using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour {
    private List<PlayerCamera> allPlayerCameras = new();
    private bool canSwitchCamera;
    private int currentIndex;

    private void Awake() {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy() {
        InstanceHandler.UnregisterInstance<PlayerCameraManager>();
    }
    
    public void EnableSpectatorMode()
    {
        if (allPlayerCameras.Count == 0)
        {
            Debug.LogWarning("No cameras available for spectator mode!");
            return;
        }

        canSwitchCamera = true;
        currentIndex = 0;

        // Деактивируем все камеры, кроме первой наблюдаемой
        for (int i = 0; i < allPlayerCameras.Count; i++)
        {
            allPlayerCameras[i].ToggleCamera(i == 0);
        }

        Debug.Log("[PlayerCameraManager] Spectator mode enabled!");
    }


    public void RegisterCamera(PlayerCamera cam) {
        if (allPlayerCameras.Contains(cam))
            return;
        allPlayerCameras.Add(cam);
        if (cam.isOwner) {
            canSwitchCamera = false;
            cam.ToggleCamera(true);
        }
    }

    public void UnregisterCamera(PlayerCamera cam) {
        if(allPlayerCameras.Contains(cam))
            allPlayerCameras.Remove(cam);
        if (cam.isOwner) {
            canSwitchCamera = true;
            SwitchNext();
        }
    }

    private void Update() {
        if(!canSwitchCamera) return;
        
        if(Input.GetKeyDown(KeyCode.Mouse0))
            SwitchNext();
        if(Input.GetKeyDown(KeyCode.Mouse1))
            SwitchPrevious();
    }

    private void SwitchNext() {
        if (allPlayerCameras.Count <= 0)
            return;
        allPlayerCameras[currentIndex].ToggleCamera(false);
        currentIndex++;
        if (currentIndex >= allPlayerCameras.Count) {
            currentIndex = 0;
        }
        allPlayerCameras[currentIndex].ToggleCamera(true);
    }
    
    private void SwitchPrevious() {
        if (allPlayerCameras.Count <= 0)
            return;
        allPlayerCameras[currentIndex].ToggleCamera(false);
        currentIndex--;
        if (currentIndex < 0) {
            currentIndex = allPlayerCameras.Count - 1;
        }
        allPlayerCameras[currentIndex].ToggleCamera(true);
    }
}
