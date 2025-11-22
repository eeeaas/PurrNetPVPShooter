using UnityEngine;
using UnityEngine.UI;
using PurrNet;
using PurrNet.Transports;
using TMPro;
using UnityEngine.SceneManagement;

public class HostPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField _portInput;
    [SerializeField] private GameSettingsManager _gameSettings;
    [SerializeField] private Button _startHostButton;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private string _gameSceneName = "GameScene";
    
    private NetworkManager _networkManager;

    private void Start()
    {
        _networkManager = InstanceHandler.NetworkManager;
        
        _startHostButton.onClick.AddListener(OnStartHostClicked);
        
        _portInput.text = "5000";
        
        // Слушаем изменения состояния сервера
        _networkManager.onServerConnectionState += OnServerConnectionStateChanged;
    }

    private void OnStartHostClicked()
    {
        if (!ushort.TryParse(_portInput.text, out var port))
        {
            _statusText.text = "Invalid port";
            return;
        }

        _statusText.text = "Starting server...";
        _startHostButton.interactable = false;
        _portInput.interactable = false;

        // Запускаем сервер
        _networkManager.StartHost();
    }

    private void OnServerConnectionStateChanged(ConnectionState state)
    {
        switch (state)
        {
            case ConnectionState.Connecting:
                _statusText.text = "Starting...";
                break;
            case ConnectionState.Connected:
                _statusText.text = "Server running!";
                _startHostButton.interactable = true;
                int mapId = _gameSettings.mapId;
                switch (mapId)
                {
                    case 0:
                        if (MapsSync.Instance != null)
                        {
                            MapsSync.Instance.SetPendingMap(mapId, "Dust2");
                        }

                        InstanceHandler.NetworkManager.sceneModule.LoadSceneAsync("Dust2");
                        break;
                    case 1:
                        if (MapsSync.Instance != null)
                        {
                            MapsSync.Instance.SetPendingMap(mapId, "Dust2");
                        }

                        InstanceHandler.NetworkManager.sceneModule.LoadSceneAsync("Office");
                        break;
                    case 2:
                        if (MapsSync.Instance != null)
                        {
                            MapsSync.Instance.SetPendingMap(mapId, "Poligon");
                        }

                        InstanceHandler.NetworkManager.sceneModule.LoadSceneAsync("Poligon");
                        break;
                }

                // Отправляем всем клиентам mapId через ObserversRpc
                //MapsSync.Instance.SendMapIdToClients(mapId);
                break;
            case ConnectionState.Disconnected:
                _statusText.text = "Stopped";
                _startHostButton.interactable = true;
                _portInput.interactable = true;
                break;
            case ConnectionState.Disconnecting:
                _statusText.text = "Stopping...";
                break;
        }
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.onServerConnectionState -= OnServerConnectionStateChanged;
    }

}