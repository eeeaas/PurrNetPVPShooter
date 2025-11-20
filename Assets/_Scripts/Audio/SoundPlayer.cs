using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundPlayer : MonoBehaviour
{
    private EventReference eventRef;
    private bool is2D = false;

    private EventInstance instance;

    public void SetEvent(EventReference eventReference, bool twoD = false)
    {
        if (eventReference.IsNull)
        {
            Debug.LogError("[SoundPlayer] Передан пустой EventReference!");
            return;
        }
        eventRef = eventReference;
        is2D = twoD;
    }

    private void Start()
    {
        if (eventRef.IsNull)
        {
            Debug.LogWarning($"[SoundPlayer] Пустой EventReference! Уничтожаю объект: {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        instance = RuntimeManager.CreateInstance(eventRef);

        if (!is2D)
        {
            RuntimeManager.AttachInstanceToGameObject(instance, transform);
        }
        else
        {
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));
        }

        instance.start();
        instance.release(); // позволяем FMOD уничтожить instance, НО он жив пока играет

        StartCoroutine(WaitForSoundEnd());
    }

    private System.Collections.IEnumerator WaitForSoundEnd()
    {
        PLAYBACK_STATE state;

        // ждём пока будет PLAYING
        do
        {
            instance.getPlaybackState(out state);
            yield return null;

        } while (state != PLAYBACK_STATE.STOPPED && state != PLAYBACK_STATE.STOPPING);

        Destroy(gameObject);
    }
}