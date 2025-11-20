using System.Collections;
using UnityEngine;
using PurrNet;

public class MapsSync : NetworkBehaviour
{
    public static MapsSync Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // pending - задаётся ДО начала загрузки сцены сервером (из HostPanel)
    private int _pendingMapId = -1;
    private string _pendingSceneName = null;
    private int _pendingBuildIndex = -1;
    private Coroutine _watchCoroutine;

    // Вызвать на сервере (HostPanel) перед LoadSceneAsync
    public void SetPendingMap(int mapId, string sceneName)
    {
        _pendingMapId = mapId;
        _pendingSceneName = sceneName;
        _pendingBuildIndex = GetBuildIndexByName(sceneName);
        if (_watchCoroutine != null)
        {
            StopCoroutine(_watchCoroutine);
            _watchCoroutine = null;
        }
        _watchCoroutine = StartCoroutine(WatchForSceneAndNotify());
    }

    private int GetBuildIndexByName(string sceneName)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            var n = System.IO.Path.GetFileNameWithoutExtension(path);
            if (n == sceneName) return i;
        }
        return -1;
    }

    private IEnumerator WatchForSceneAndNotify()
    {
        var net = InstanceHandler.NetworkManager;

        // Ждём, пока сцена сервера пометится как загруженная в sceneModule.
        // Если buildIndex неизвестен, делаем разумный таймаут fallback.
        if (_pendingBuildIndex >= 0)
        {
            while (!net.sceneModule.IsSceneLoaded(_pendingBuildIndex))
                yield return null;
        }
        else
        {
            // fallback: ждём немного (или можно ждать until any scene loaded)
            float timeout = 10f;
            while (timeout > 0f)
            {
                // Пытаемся найти индекс заново
                _pendingBuildIndex = GetBuildIndexByName(_pendingSceneName);
                if (_pendingBuildIndex >= 0)
                {
                    while (!net.sceneModule.IsSceneLoaded(_pendingBuildIndex))
                        yield return null;
                    break;
                }
                timeout -= Time.deltaTime;
                yield return null;
            }
        }

        // если нет pending - ничего не делаем
        if (_pendingMapId < 0)
        {
            _watchCoroutine = null;
            yield break;
        }

        // Сцена на сервере загружена — отправляем mapId клиентам безопасно
        SendMapIdToClients(_pendingMapId);

        // Очистка
        _pendingMapId = -1;
        _pendingSceneName = null;
        _pendingBuildIndex = -1;
        _watchCoroutine = null;
    }

    // Серверный метод, вызывающий RPC (без bufferLast)
    public void SendMapIdToClients(int mapId)
    {
        SendMapIdToClientsRpc(mapId);
    }

    [ObserversRpc] // НЕ bufferLast
    private void SendMapIdToClientsRpc(int mapId)
    {
        var net = InstanceHandler.NetworkManager;
        // Защита от перезапуска сцены на хосте (сервер+локальный клиент)
        // При необходимости замените isServer/isClient на ваши API имена (IsServer/IsClient и т.д.)
        bool isHost = net.isServer && net.isClient;
        if (isHost)
        {
            // Хост уже загрузил сцену на сервере — не выполнять локальную LoadScene ещё раз.
            return;
        }

        // Клиенты получают mapId — выполняем клиентскую логику загрузки/инициализации.
        // Тут можно запускать локальную загрузку ассетов/инициализацию, если нужно.
        ClientLoadMap(mapId);
    }

    private void ClientLoadMap(int mapId)
    {
        switch (mapId)
        {
            case 0:
                InstanceHandler.NetworkManager.sceneModule.LoadSceneAsync("Dust2");
                break;
            case 1:
                InstanceHandler.NetworkManager.sceneModule.LoadSceneAsync("Office");
                break;
            default:
                Debug.LogWarning($"[MapsSync] Unknown mapId {mapId}");
                break;
        }
    }
}