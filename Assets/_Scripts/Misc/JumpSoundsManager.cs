using FMODUnity;
using PurrNet;
using UnityEngine;

public class JumpSoundsManager : NetworkBehaviour {
    [SerializeField] private EventReference _3D_JumpStart_eventRef;
    [SerializeField] private EventReference _3D_JumpEnd_eventRef;
    
    [ObserversRpc]
    public void JumpSoundStart() {
        RuntimeManager.PlayOneShot(_3D_JumpStart_eventRef, transform.position);
    }

    [ObserversRpc]
    public void JumpSoundEnd() {
        RuntimeManager.PlayOneShot(_3D_JumpEnd_eventRef, transform.position);
    }
}
