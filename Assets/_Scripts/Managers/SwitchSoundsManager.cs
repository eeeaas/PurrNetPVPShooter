using FMODUnity;
using UnityEngine;

public class SwitchSoundsManager : MonoBehaviour {
    [SerializeField] private EventReference _2D_knifeDraw_eventRef;
    [SerializeField] private EventReference _2D_akDraw_eventRef;
    [SerializeField] private EventReference _2D_deagleDraw_eventRef;

    public void OnGunSwitch(int id) {
        switch (id) {
            case 0:
                RuntimeManager.PlayOneShot(_2D_akDraw_eventRef, transform.position);
                break;
            case 1:
                RuntimeManager.PlayOneShot(_2D_deagleDraw_eventRef, transform.position);
                break;
            case 2:
                RuntimeManager.PlayOneShot(_2D_knifeDraw_eventRef, transform.position);
                break;
        }
    }
}
