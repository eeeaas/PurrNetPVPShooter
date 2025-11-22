using FMODUnity;
using UnityEngine;
using FMOD.Studio;

public class ReloadSoundsManager : MonoBehaviour
{
    [SerializeField] private EventReference _3D_akReload_eventRef;
    [SerializeField] private EventReference _3D_deagleReload_eventRef;
    [SerializeField] private EventReference _3D_awpReload_eventRef;

    private EventInstance akReloadInstance;
    private EventInstance deagleReloadInstance;
    private EventInstance awpReloadInstance;
    private EventInstance currentPlayingInstance;

    void Start()
    {
        // Создаем экземпляры событий заранее
        akReloadInstance = RuntimeManager.CreateInstance(_3D_akReload_eventRef);
        deagleReloadInstance = RuntimeManager.CreateInstance(_3D_deagleReload_eventRef);
        awpReloadInstance = RuntimeManager.CreateInstance(_3D_awpReload_eventRef);
        
        // Устанавливаем 3D атрибуты для обоих экземпляров
        Set3DAttributesForInstance(akReloadInstance, transform.position);
        Set3DAttributesForInstance(deagleReloadInstance, transform.position);
    }

    void Update()
    {
        // Обновляем позицию звуков каждый кадр (если объект движется)
        if (akReloadInstance.isValid())
            Set3DAttributesForInstance(akReloadInstance, transform.position);
        
        if (deagleReloadInstance.isValid())
            Set3DAttributesForInstance(deagleReloadInstance, transform.position);
        
        if (awpReloadInstance.isValid())
            Set3DAttributesForInstance(awpReloadInstance, transform.position);
    }

    public void PlayReload(int id) 
    {
        // Останавливаем текущий звук, если он играет
        StopReload();

        // Обновляем позицию перед воспроизведением
        Set3DAttributesForInstance(akReloadInstance, transform.position);
        Set3DAttributesForInstance(deagleReloadInstance, transform.position);
        Set3DAttributesForInstance(awpReloadInstance, transform.position);

        switch (id) 
        {
            case 0:
                currentPlayingInstance = akReloadInstance;
                break;
            case 1:
                currentPlayingInstance = deagleReloadInstance;
                break;
            case 2:
                currentPlayingInstance = awpReloadInstance;
                break;
        }

        // Запускаем новый звук
        currentPlayingInstance.start();
    }

    public void StopReload() 
    {
        if (currentPlayingInstance.isValid())
        {
            // Останавливаем текущий звук
            currentPlayingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    // Устанавливаем 3D атрибуты для экземпляра
    private void Set3DAttributesForInstance(EventInstance instance, Vector3 position)
    {
        if (instance.isValid())
        {
            FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(position);
            instance.set3DAttributes(attributes);
        }
    }

    // Альтернативный вариант с плавной остановкой
    public void StopReloadFadeOut()
    {
        if (currentPlayingInstance.isValid())
        {
            currentPlayingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    void OnDestroy()
    {
        // Освобождаем ресурсы
        if (akReloadInstance.isValid())
        {
            akReloadInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            akReloadInstance.release();
        }
        
        if (deagleReloadInstance.isValid())
        {
            deagleReloadInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            deagleReloadInstance.release();
        }
        
        if (awpReloadInstance.isValid())
        {
            awpReloadInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            awpReloadInstance.release();
        }
    }
}