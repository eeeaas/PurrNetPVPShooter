using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostGameSettings : MonoBehaviour {
    [SerializeField] private Toggle autoBhop_Toggle;
    [SerializeField] private TMP_InputField playerCount_Inputfield;
    [SerializeField] private TMP_InputField mapId_Inputfield;

    public bool GetAutoBhopToggle() {
        return autoBhop_Toggle.isOn;
    }

    public int GetPlayerCountInputField() {
        return int.TryParse(playerCount_Inputfield.text, out int playerCount) ? playerCount : 1;
    }

    public int GetMapId() {
        return int.TryParse(mapId_Inputfield.text, out int mapId) ? mapId : 0;
    }
}
