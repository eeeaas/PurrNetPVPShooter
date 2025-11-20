using System;
using System.Collections.Generic;
using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private RotationMimic cameraMimic;
    [SerializeField] private List<Renderer> renderers = new();
    
    [SerializeField] private List<Renderer> firstPersonRenderers = new();
    private bool isSpectatorView;

    protected override void OnSpawned() {
        base.OnSpawned();
        InstanceHandler.GetInstance<PlayerCameraManager>().RegisterCamera(this);
        if (isOwner)
        {
            // Я — владелец: руки видны, тело скрыто
            TogglePlayerBody(false);
            ToggleFirstPerson(true);
        }
        else
        {
            // Это не мой персонаж: тело видно, руки скрыты
            TogglePlayerBody(true);
            ToggleFirstPerson(false);
        }
    }

    protected override void OnDespawned() {
        base.OnDespawned();
        TogglePlayerBody(false);
        ToggleFirstPerson(false);
        InstanceHandler.GetInstance<PlayerCameraManager>().UnregisterCamera(this);
    }

    private void TogglePlayerBody(bool toggle) {
        foreach (var render in renderers) {
            render.shadowCastingMode = toggle ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
        }
    }
    
    private void ToggleFirstPerson(bool toggle)
    {
        foreach (var render in firstPersonRenderers)
        {
            render.shadowCastingMode = toggle ? ShadowCastingMode.Off : ShadowCastingMode.ShadowsOnly;
        }
    }

    public void ToggleCamera(bool toggle)
    {
        playerCamera.Priority = toggle ? 10 : 0;
        isSpectatorView = toggle && !isOwner;

        if (isOwner)
        {
            // Собственная камера → руки видны, тело скрыто
            TogglePlayerBody(false);
            ToggleFirstPerson(true);
        }
        else if (isSpectatorView)
        {
            // Это активная spectator-камера → тело скрыто, руки включаем
            TogglePlayerBody(false);
            ToggleFirstPerson(true);
        }
        else
        {
            // Камера неактивна или это чужой персонаж → всё скрыто
            TogglePlayerBody(true);
            ToggleFirstPerson(false);
        }
    }

    private void Update() {
        if (isOwner) return;
        transform.rotation = cameraMimic.transform.rotation;
    }
}
