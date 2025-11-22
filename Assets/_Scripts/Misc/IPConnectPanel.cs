using UnityEngine;
using UnityEngine.UI;
using PurrNet;
using PurrNet.Transports;
using TMPro;
using UnityEngine.SceneManagement;

public class IPConnectPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField _ipInput;
    [SerializeField] private TMP_InputField _portInput;
    [SerializeField] private Button _connectButton;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private string _gameSceneName = "GameScene";
    
    private NetworkManager _networkManager;
    private IConnectable _connectable;
    private bool _isConnecting;

    private void Start()
    {
        _networkManager = InstanceHandler.NetworkManager;
        
        // Получаем транспорт и приводим к IConnectable
        _connectable = _networkManager.transport.transport as IConnectable;
        
        _connectButton.onClick.AddListener(OnConnectClicked);
        
        // Слушаем изменения состояния подключения
        _networkManager.onClientConnectionState += OnClientConnectionStateChanged;
    }

    private void OnConnectClicked()
    {
        if (!ushort.TryParse(_portInput.text, out var port))
        {
            _statusText.text = "Invalid port";
            return;
        }

        string ip = _ipInput.text.Trim();
        if (string.IsNullOrEmpty(ip))
        {
            _statusText.text = "Enter IP address";
            return;
        }

        _statusText.text = "Connecting...";
        _connectButton.interactable = false;
        _ipInput.interactable = false;
        _portInput.interactable = false;
        _isConnecting = true;

        Debug.Log($"[Client] Connecting to {ip}:{port}");

        // НАСТРАИВАЕМ ТРАНСПОРТ
        if (_networkManager.transport.transport is UDPTransport purrTransport)
        {
            purrTransport.address = ip;
            purrTransport.serverPort = port;
        }

        // ИНИЦИАЛИЗИРУЕМ КЛИЕНТА
        _networkManager.StartClient();
    }

    private void OnClientConnectionStateChanged(ConnectionState state)
    {
        if (!_isConnecting)
            return;

        switch (state)
        {
            case ConnectionState.Connecting:
                _statusText.text = "Connecting...";
                break;
            case ConnectionState.Connected:
                _statusText.text = "Connected!";
                _connectButton.interactable = true;
                

                _isConnecting = false;
                break;
            case ConnectionState.Disconnected:
                _statusText.text = "Disconnected";
                _connectButton.interactable = true;
                _ipInput.interactable = true;
                _portInput.interactable = true;
                _isConnecting = false;
                break;
            case ConnectionState.Disconnecting:
                _statusText.text = "Disconnecting...";
                break;
        }
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.onClientConnectionState -= OnClientConnectionStateChanged;
    }
}