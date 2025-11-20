using PurrNet;
using PurrNet.Steam;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.UI;

public class SwitchBetweenTransports : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    
    public void OnToggleChanged() {
        InstanceHandler.NetworkManager.transport =
            toggle.isOn
                ? InstanceHandler.NetworkManager.GetComponent<SteamTransport>()
                : InstanceHandler.NetworkManager.GetComponent<UDPTransport>();
    }
}
