using System;
using PurrNet;
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    [SerializeField] private PlayerController playerController;
    bool isOpen = false;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && !isOpen) {
            isOpen = true;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            playerController.uiIsOpen = isOpen;
            
            if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager)) return;
            if(!InstanceHandler.TryGetInstance(out SettingsView settingsView)) return;
            gameViewManager.ShowView<SettingsView>(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isOpen){
            isOpen = false;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager)) return;
            if(!InstanceHandler.TryGetInstance(out SettingsView settingsView)) return;
            gameViewManager.HideView<SettingsView>();
            
            playerController.playerAiming.sensitivityMultiplier = settingsView.GetSensitivitySliderValue();
            
            playerController.uiIsOpen = isOpen;
        }
    }
}
