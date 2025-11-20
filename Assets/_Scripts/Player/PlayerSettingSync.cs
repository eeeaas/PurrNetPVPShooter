using Fragsurf.Movement;
using PurrNet;
using UnityEngine;

public class PlayerSettingSync : NetworkBehaviour
{
    [ObserversRpc(bufferLast:true)]
    public void Rpc_SetAutoBhop(bool enabled)
    {
        var surf = GetComponent<SurfCharacter>();
        if (surf != null)
            surf.movementConfig.autoBhop = enabled;
    }
}
