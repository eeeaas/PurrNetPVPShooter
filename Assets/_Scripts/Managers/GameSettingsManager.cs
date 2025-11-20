using System;
using PurrNet;
using TMPro;
using UnityEngine;

public class GameSettingsManager : MonoBehaviour
{
    [SerializeField] private HostGameSettings _hostGameSettings;

    public int mapId = 0;
    public bool autoBhop = false;
    public int playerCount = 1;

    private void Update() {
        if (_hostGameSettings == null) return;
        autoBhop = _hostGameSettings.GetAutoBhopToggle();
        playerCount = _hostGameSettings.GetPlayerCountInputField();
        mapId = _hostGameSettings.GetMapId();
    }
}
