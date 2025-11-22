using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Fragsurf.Movement;
using PurrNet;
using PurrNet.StateMachine;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : StateNode {
    [Header("Stats")]
    [SerializeField] private float range = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private bool automatic;
    [SerializeField] public float weightMultiplier = 1f;
    [SerializeField] public int gunIdForAnim;
    
    [SerializeField] public bool isKnife = false;
    [SerializeField] public bool isZoomable = false;
    [SerializeField] public bool blockShoot;
    
    [SerializeField] private bool inZoom;
    [SerializeField] private bool blockZoom;
    [SerializeField] private int zoomStage = 0;

    [SerializeField] private GameObject AWPZoomCanvas;
    [SerializeField] private CinemachineCamera Camera;

    [SerializeField] private float headScale = 5f, bodyScale = 1f, armScale = 0.5f, legScale = 0.25f;
    
    [Header("Recoil")]
    [SerializeField] private float recoilStrength = 1f;
    [SerializeField] private float recoilDuration = 0.2f;
    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private float rotationAmount = 25f;
    [SerializeField] private AnimationCurve rotationCurve;
    [SerializeField] public float yRecoilStrength = 5f;
    [SerializeField] public float xRecoilStrength = 5f;
    [SerializeField] public float recoilResetDelay = 0.2f;
    
    [Header("Knife LEFT Hand Rotation Animation")]
    [SerializeField] private float recoilStrengthLEFTHAND = 0.25f;
    [SerializeField] private AnimationCurve recoilCurveLEFTHAND;
    
    [SerializeField] private float rotationAmountLEFTHAND = -60f;
    [SerializeField] private AnimationCurve rotationCurveLEFTHAND;
    
    [Header("Knife RIGHT hand Rotation Animation")]
    [SerializeField] private float recoilStrengthRIGHTHAND = 1f;
    [SerializeField] private AnimationCurve recoilCurveRIGHTHAND;
    
    [SerializeField] private float rotationAmountRIGHTHANDX = 25f;
    [SerializeField] private AnimationCurve rotationCurveRIGHTHANDX;
    
    [SerializeField] private float rotationAmountRIGHTHANDY = 25f;
    [SerializeField] private AnimationCurve rotationCurveRIGHTHANDY;
    
    [SerializeField] private float rotationAmountRIGHTHANDZ = 25f;
    [SerializeField] private AnimationCurve rotationCurveRIGHTHANDZ;

    [Header("Knife settings")] 
    [SerializeField] private float knifeRightHandHitMultiplier = 3f;
    [SerializeField] private float rightHandFireRate = 0.6f;
    
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Transform rightHandTarget, leftHandTarget;
    [SerializeField] private Transform rightIkTarget, leftIkTarget, VrightIkTarget, VleftIkTarget;
    [SerializeField] private List<Renderer> renderers = new();
    [SerializeField] private ParticleSystem environmentHitEffect, playerHitEffect;
    [SerializeField] private GameObject environmentHitObj;
    [SerializeField] private RotationMimic rotationMimic;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] public Ammo ammo;
    [SerializeField] private ReloadSoundsManager reloadSoundsManager;
    
    [SerializeField] public bool isReloading = false;
    
    [SerializeField] private SoundPlayer soundPlayerPrefab;
    
    //[SerializeField] private AudioSource shotSoundPlayer;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SurfCharacter surfController;

    [Header("Sounds references")] 
    [SerializeField] private EventReference _2D_headShot_eventRef;
    [SerializeField] private EventReference _3D_headShot_eventRef;
    [SerializeField] private EventReference _3D_bodyShot_eventRef;
    
    [SerializeField] private EventReference _3D_knifeHitEnv_eventRef;
    [SerializeField] private EventReference _3D_bulletHitEnv_eventRef;
    
    private bool isDrawingWeapon = false;

    //[SerializeField] private List<AudioClip> envHitSounds, playerHitSounds, playerHeadShotsSounds, shotSounds;
    //[SerializeField, Range(0f, 1f)] private float envHitVolume, playerHitVolume, playerHeadShotsVolume, shotVolume; 
    
    [Header("Recoil Pattern (per shot offset X/Y)")]
    public Vector2[] recoilPattern;
    public int FromRecoilIndex = 8;
    
    private float _lastFireTime;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Coroutine _recoilRoutine;
    [SerializeField] private Transform _leftArmOrig;
    [SerializeField] private Transform _rightArmOrig;
    
    [SerializeField] private StudioEventEmitter _shotEmmiter;

    public bool isLookAnim = false;

    public bool isShoting;
    private bool isRightKnifeHit = false;

    private void Awake() {
        ToggleVisuals(false);
        //playerController = GetComponentInParent<PlayerController>();
    }

    public override void Enter(bool asServer) {
        base.Enter(asServer);
        ToggleVisuals(true);
    }

    public override void Exit(bool asServer) {
        base.Exit(asServer);
        ToggleVisuals(false);
    }

    private void Start() {
        _originalPosition = bodyTransform.localPosition;
        _originalRotation = bodyTransform.localRotation;
    }

    private void ToggleVisuals(bool toggle) {
        foreach (var renderer in renderers) {
            renderer.enabled = toggle;
        }
    }

    public void EnableAWPZoom() {
        playerController.SetSensitivityMultipluier(0.5f);
        inZoom = true;
        AWPZoomCanvas.SetActive(true);
        ToggleVisuals(false);
        if(!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager)) return;
        gameViewManager.HideView<MainGameView>();
        gameViewManager.HideView<SettingsView>();
    }

    public void DisableAWPZoom() {
        if (!isZoomable) 
            return;
        inZoom = false;
        AWPZoomCanvas.SetActive(false);
        Camera.Lens.FieldOfView = 68f;
        playerController.SetSensitivityMultipluier(1f);
        Debug.Log("AWP Zoom Disabled");
        zoomStage = 0;
        if(!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager)) return;
        gameViewManager.ShowView<MainGameView>(false);
    }
    
    Vector3 GetSpreadDirection(Vector3 forward, float spreadAngleDeg) {
        Vector3 random = Random.insideUnitSphere * Mathf.Tan(spreadAngleDeg * Mathf.Deg2Rad);
        return (forward + random).normalized;
    }

    public override void StateUpdate(bool asServer) {
        base.StateUpdate(asServer);
        if (!isLookAnim) {
            SetIKTargets();
        }
        isDrawingWeapon = playerController.isDrawingWeapon;
        if (!isOwner) return;
        if (playerController.uiIsOpen || isDrawingWeapon) return;
        if (blockShoot) return;
        if (!isKnife) {
            ShotGunsUpdate();
        }
        else {
            KnifeUpdate();
        }
    }

    private void Update() {
        if (isZoomable && playerController.GetCurrentWeapon() == this) {
            if (Input.GetKeyDown(KeyCode.Mouse1) && !inZoom && !blockZoom || Input.GetKeyDown(KeyCode.Mouse1) && zoomStage == 1 && !blockZoom) {
                zoomStage++;
                if (zoomStage == 1) {
                    EnableAWPZoom();
                    Camera.Lens.FieldOfView = 20f;
                }else if (zoomStage == 2) {
                    playerController.SetSensitivityMultipluier(0.25f);
                    Camera.Lens.FieldOfView = 10f;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1) && inZoom && zoomStage == 2) {
                DisableAWPZoom();
                ToggleVisuals(true);
            }
        }
    }

    private void KnifeUpdate() {
        if (automatic && Input.GetKey(KeyCode.Mouse0) || !automatic && Input.GetMouseButtonDown(0)) {
            if (_lastFireTime + fireRate > Time.unscaledTime)
                return;
            isShoting = true;
            isRightKnifeHit = false;
            isLookAnim = false;
            PlayKnifeAnimation(false);
            _lastFireTime = Time.unscaledTime;
        
            Vector3 dir = GetSpreadDirection(cameraTransform.forward, 0f);
            Ray ray = new Ray(cameraTransform.position + cameraTransform.forward * 0.5f, dir);
        
            if (Physics.Raycast(ray, out var hit, range,hitLayer)) {
                if (IsSelfHit(hit.transform))
                    return; 
                Debug.Log($"Knife hit obj: {hit.transform.name}");
                if (hit.transform.TryGetComponent<Hitbox>(out var hitbox)) {
                    float finalDamage = damage;
                    bool isHeadShot = false;
                    switch(hitbox.hitZone)
                    {
                        case HitZone.Head: finalDamage *= headScale;
                            isHeadShot = true; break;
                        case HitZone.Arm: finalDamage *= armScale; break;
                        case HitZone.Leg: finalDamage *= legScale; break;
                        case HitZone.Body: finalDamage *= bodyScale; break;
                    }
                    PlayerHit(hitbox.playerHealth, hitbox.playerHealth.transform.InverseTransformPoint(hit.point),
                        hit.normal, isHeadShot);
                    if (isServer) {
                        hitbox.playerHealth.ChangeHealth(Convert.ToInt32(-finalDamage), default);
                    }
                }
                else
                {
                    EnvironmentHit(hit.point, hit.normal);
                }
            }

            if (isClient) {
                HandleHit(ray, networkManager.tickModule.rollbackTick);
            }
        }
        else if (Input.GetMouseButtonDown(1)) {
            if (_lastFireTime + rightHandFireRate > Time.unscaledTime)
                return;
            isShoting = true;
            isRightKnifeHit = true;
            isLookAnim = false;
            PlayKnifeAnimation(true);
            _lastFireTime = Time.unscaledTime;
        
            Vector3 dir = GetSpreadDirection(cameraTransform.forward, 0f);
            Ray ray = new Ray(cameraTransform.position + cameraTransform.forward * 0.5f, dir);
        
            if (Physics.Raycast(ray, out var hit, range, hitLayer)) {
                if (IsSelfHit(hit.transform))
                    return; 
                Debug.Log($"Knife hit right obj: {hit.transform.name}");
                if (hit.transform.TryGetComponent<Hitbox>(out var hitbox)) {
                    float finalDamage = damage;
                    bool isHeadShot = false;
                    switch(hitbox.hitZone)
                    {
                        case HitZone.Head: finalDamage *= headScale;
                            isHeadShot = true; break;
                        case HitZone.Arm: finalDamage *= armScale; break;
                        case HitZone.Leg: finalDamage *= legScale; break;
                        case HitZone.Body: finalDamage *= bodyScale; break;
                    }
                    PlayerHit(hitbox.playerHealth, hitbox.playerHealth.transform.InverseTransformPoint(hit.point),
                        hit.normal, isHeadShot);
                    if (isServer) {
                        hitbox.playerHealth.ChangeHealth(Convert.ToInt32(-finalDamage * knifeRightHandHitMultiplier), default);
                    }
                }
                else
                {
                    EnvironmentHit(hit.point, hit.normal);
                }
            }

            if (isClient) {
                HandleHit(ray, networkManager.tickModule.rollbackTick, isRightKnifeHit);
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.F)) {
                PlayKnifeAnimation(false, true);
            }
            isShoting = false;
            return;
        }
    }

    private void ShotGunsUpdate() {
        
        if(isReloading) return;
        if (Input.GetKeyDown(KeyCode.F)) {
            PlayGunLookAnimation();
            return;
        }

        // используем "эффективные" значения (локальная предсказанная на владельце)
        bool primaryIsZero = ammo.IsZeroPrimaryEffective();
        bool secondaryIsZero = ammo.IsZeroSecondaryEffective();
        int currentPrimary = ammo.GetCurrentPrimaryEffective();

        // Группируем условие корректно: ручной reload (нажат R и есть смысл) или автоперезарядка при пустом магазине
        bool pressedReload = Input.GetKeyDown(KeyCode.R);
        bool manualCanReload = pressedReload && currentPrimary < ammo.maxPrimaryAmmo && !secondaryIsZero;
        bool autoReload = primaryIsZero && !secondaryIsZero;

        if (manualCanReload || autoReload) {
            Debug.Log((primaryIsZero && !secondaryIsZero) + " ---------------- RELOAD TRIGGER");
            PlayGunReloadAnimation();
            return;
        }

        if (primaryIsZero) return;
        
        if (_lastFireTime + fireRate > Time.unscaledTime) {
            blockZoom = true;
        }
        else {
            blockZoom = false;
        }
        
        if (automatic && !Input.GetKey(KeyCode.Mouse0) || !automatic && !Input.GetMouseButtonDown(0)) {
            isShoting = false;
            return;
        }

        if (_lastFireTime + fireRate > Time.unscaledTime)
            return;
        
        isShoting = true;
        
        weaponAnimator.Play("Idle", 0, 0f); // или любая базовая/нулевая анимация
        weaponAnimator.Update(0f);   
        weaponAnimator.enabled = false;
        isLookAnim = false;
        PlayShotEffect();
        _lastFireTime = Time.unscaledTime;
        ammo.ChangeAmmo(-1);
        float spread = 0f;
        
        Debug.Log(playerController.IsGrounded() + " " + surfController.moveDir.magnitude);
        if (playerController.IsGrounded() && surfController.moveDir.magnitude < 0.1f && !isZoomable || playerController.IsGrounded() && surfController.moveDir.magnitude < 0.1f && isZoomable && inZoom) {
            spread = 0f;
        }
        else {
            spread = Random.Range(0f, 8f);
        }
        
        if (inZoom) {
            DisableAWPZoom();
            ToggleVisuals(true);
        }
        Vector3 dir = GetSpreadDirection(cameraTransform.forward, spread);
        Ray ray = new Ray(cameraTransform.position + cameraTransform.forward * 0.5f, dir);

        if (Physics.Raycast(ray, out var hit, range, hitLayer)) {
            if (IsSelfHit(hit.transform))
                return; 
            if (hit.transform.TryGetComponent<Hitbox>(out var hitbox)) {
                float finalDamage = damage;
                bool isHeadShot = false;
                switch(hitbox.hitZone)
                {
                    case HitZone.Head: finalDamage *= headScale;
                        isHeadShot = true; break;
                    case HitZone.Arm: finalDamage *= armScale; break;
                    case HitZone.Leg: finalDamage *= legScale; break;
                    case HitZone.Body: finalDamage *= bodyScale; break;
                }
                PlayerHit(hitbox.playerHealth, hitbox.playerHealth.transform.InverseTransformPoint(hit.point),
                    hit.normal, isHeadShot);
                if (isServer) {
                    hitbox.playerHealth.ChangeHealth(Convert.ToInt32(-finalDamage), default);
                }
            }
            else
            {
                EnvironmentHit(hit.point, hit.normal);
            }
        }
        
        // RigidItems impulse
        if (hit.rigidbody != null && hit.transform.CompareTag("RigidItems"))
        {
            float impulseForce = 5f;
            float torqueForce = 3f;

            // Толчок по нормали (естественное движение)
            Vector3 impulse = -hit.normal * impulseForce;
            hit.rigidbody.AddForce(impulse, ForceMode.Impulse);

            // Вращение (раскрутка шарика)
            Vector3 torque = Vector3.Cross(hit.normal, ray.direction) * torqueForce;
            hit.rigidbody.AddTorque(torque, ForceMode.Impulse);
        }


        if (isClient) {
            HandleHit(ray, networkManager.tickModule.rollbackTick);
        }
    }

    [ServerRpc]
    private void HandleHit(Ray ray, Double preciseTick, bool isRightKnifeHit = false, RPCInfo info = default) {
        if (rollbackModule.Raycast(preciseTick, ray, out var hit, range, hitLayer)) {
            //if (IsSelfHit(hit.transform))
               // return; 
            if(hit.transform.TryGetComponent<CharacterController>(out var characterController))
            {
                characterController.enabled = false;
            }

            if (hit.transform.TryGetComponent<Hitbox>(out var hitbox)) {

                float finalDamage = damage;
                bool isHeadShot = false;
                switch (hitbox.hitZone) {
                    case HitZone.Head:
                        finalDamage *= headScale;
                        isHeadShot = true;
                        break;
                    case HitZone.Arm: finalDamage *= armScale; break;
                    case HitZone.Leg: finalDamage *= legScale; break;
                    case HitZone.Body: finalDamage *= bodyScale; break;
                }

                Debug.Log("ServerRPC HIT");
                if (isRightKnifeHit) {
                    hitbox.playerHealth.ChangeHealth(Convert.ToInt32(-finalDamage * knifeRightHandHitMultiplier),
                        info.sender);
                }
                else {
                    hitbox.playerHealth.ChangeHealth(Convert.ToInt32(-finalDamage),
                        info.sender);
                }

                PlayerHitConfirmation(hitbox.playerHealth, hitbox.playerHealth.transform.InverseTransformPoint(hit.point),
                    hit.normal, isHeadShot);
            }
            
            // Server-side physics impulse
            if (hit.rigidbody != null && hit.transform.CompareTag("RigidItems"))
            {
                float impulseForce = 5f;
                float torqueForce = 3f;

                Vector3 impulse = -hit.normal * impulseForce;
                hit.rigidbody.AddForce(impulse, ForceMode.Impulse);

                Vector3 torque = Vector3.Cross(hit.normal, ray.direction) * torqueForce;
                hit.rigidbody.AddTorque(torque, ForceMode.Impulse);
            }


        }
    }

    [ObserversRpc(excludeOwner:true)]
    private void PlayerHitConfirmation(PlayerHealth player, Vector3 localPosition, Vector3 normal, bool isHeadShot) {
        PlayerHit(player, localPosition, normal, isHeadShot);
    }

    private void PlayerHit(PlayerHealth player, Vector3 localPosition, Vector3 normal, bool isHeadShot) {
        if (!player || !player.transform) {
            return;
        }
        if (playerHitEffect) {
            Instantiate(playerHitEffect, player.transform.TransformPoint(localPosition), Quaternion.LookRotation(normal));
        }

        
        if (isHeadShot) {
            Debug.Log("HeadShot");
            //soundPlayer.PlaySound(playerHeadShotsSounds[Random.Range(0, playerHeadShotsSounds.Count)], playerHeadShotsVolume, 120f);
            //FMODUnity.RuntimeManager.PlayOneShot("event:/HeadShots3D", soundPlayer.transform.position);
            //FMODUnity.RuntimeManager.PlayOneShot("event:/LocalHeadShots", soundPlayer.transform.position);
            SpawnFmodSound(_3D_headShot_eventRef, player.transform.TransformPoint(localPosition));
            SpawnFmodSound(_2D_headShot_eventRef, player.transform.TransformPoint(localPosition), true);
            
        }
        else {
            Debug.Log("Default popadanie");
            //soundPlayer.PlaySound(playerHitSounds[Random.Range(0, playerHitSounds.Count)], playerHitVolume);
            //FMODUnity.RuntimeManager.PlayOneShot("event:/JustShots3D", soundPlayer.transform.position);
            SpawnFmodSound(_3D_bodyShot_eventRef, player.transform.TransformPoint(localPosition));
        }
    }
    
    private bool IsSelfHit(Transform hit)
    {
        // Игрок — всегда в корне
        var myRoot = playerController.transform.root;
        var hitRoot = hit.root;

        // Если попали в себя — игнорируем
        if (hitRoot == myRoot)
            return true;

        return false;
    }

    private void SpawnFmodSound(EventReference soundRef, Vector3 pos, bool is2D = false) {
        var soundPlayer = Instantiate(soundPlayerPrefab, pos, Quaternion.identity);
        soundPlayer.SetEvent(soundRef, is2D);
    }

    [ObserversRpc(runLocally: true)]
    private void EnvironmentHit(Vector3 position, Vector3 normal) {
        if (environmentHitEffect) {
            Instantiate(environmentHitEffect, position, Quaternion.LookRotation(normal));
            Instantiate(environmentHitObj, position, Quaternion.LookRotation(normal));
        }

        //var soundPlayer = Instantiate(soundPlayerPrefab, position, Quaternion.identity);
        //soundPlayer.PlaySound(envHitSounds[Random.Range(0, envHitSounds.Count)], envHitVolume);
        if (isKnife) {
            //FMODUnity.RuntimeManager.PlayOneShot("event:/GroundHit3D", soundPlayer.transform.position);
            SpawnFmodSound(_3D_knifeHitEnv_eventRef, position);
        }
        else {
            //FMODUnity.RuntimeManager.PlayOneShot("event:/GroundShotHit3D", soundPlayer.transform.position);
            SpawnFmodSound(_3D_bulletHitEnv_eventRef, position);
        }

    }
    
    private void SetIKTargets() {
        if (rightHandTarget != null) {
            rightIkTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
            VrightIkTarget.SetPositionAndRotation(rightHandTarget.position, rightHandTarget.rotation);
        }
        else {
            rightIkTarget.SetPositionAndRotation(_rightArmOrig.position, _rightArmOrig.localRotation);
            VrightIkTarget.SetPositionAndRotation(_rightArmOrig.position, _rightArmOrig.localRotation);
        }
        if (leftHandTarget != null) {
            leftIkTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
            VleftIkTarget.SetPositionAndRotation(leftHandTarget.position, leftHandTarget.rotation);
        }
        else {
            leftIkTarget.SetPositionAndRotation(_leftArmOrig.position, _leftArmOrig.localRotation);
            VleftIkTarget.SetPositionAndRotation(_leftArmOrig.position, _leftArmOrig.localRotation);
        }
    }
    
    [ObserversRpc(runLocally:true)]
    private void PlayKnifeAnimation(bool rightHand = false, bool look = false)
    {
        weaponAnimator.enabled = true;
        if (look) {
            weaponAnimator.SetTrigger("T_KnifeLook");
            return;
        }
        if(rightHand)
            weaponAnimator.SetTrigger("T_KnifeRight");
        else
            weaponAnimator.SetTrigger("T_KnifeLeft");
        _shotEmmiter.Play();
    }

    [ObserversRpc(runLocally: true)]
    private void PlayGunLookAnimation() {
        weaponAnimator.enabled = true;
        switch (gunIdForAnim) {
            case 0:
                weaponAnimator.SetTrigger("T_PistolLook");
                break;
            case 1:
                weaponAnimator.SetTrigger("T_RifleLook");
                break;
            case 3:
                weaponAnimator.SetTrigger("T_AWPLook");
                break;
        }
    }
    
    [ObserversRpc(runLocally: true)]
    private void PlayGunReloadAnimation() {
        weaponAnimator.enabled = true;
        isReloading = true;
        switch (gunIdForAnim) {
            case 0:
                weaponAnimator.SetTrigger("T_PistolReload");
                PlayReloadSound(1);
                break;
            case 1:
                weaponAnimator.SetTrigger("T_RifleReload");
                PlayReloadSound(0);
                break;
            case 3:
                weaponAnimator.SetTrigger("T_AWPReload");
                PlayReloadSound(2);
                break;
        }
    }

    [ObserversRpc]
    private void PlayReloadSound(int i) {
        reloadSoundsManager.PlayReload(i);
    }

    public void ReloadComplete() {
        isReloading = false;
        if (ammo.maxPrimaryAmmo - ammo.currentAmmo.value <= ammo.secondaryAmmo.value) {
            ammo.ChangeSecondaryAmmo(-(ammo.maxPrimaryAmmo - ammo.currentAmmo.value));
            Debug.Log(-ammo.maxPrimaryAmmo - ammo.currentAmmo.value + " ChangeSECONDARY");
            ammo.ChangeAmmo(ammo.maxPrimaryAmmo - ammo.currentAmmo.value);
            
            Debug.Log(ammo.maxPrimaryAmmo - ammo.currentAmmo.value + " ChangePRIMARY");
        }
        else {
            int bufferSecond = ammo.secondaryAmmo.value;
            ammo.ChangeSecondaryAmmo(-ammo.secondaryAmmo.value);
            ammo.ChangeAmmo(bufferSecond);
        }
    }

    [ObserversRpc(runLocally:true)]
    private void PlayShotEffect() {
        if(muzzleFlash && !isKnife)
            muzzleFlash.Play();
        
        if(_recoilRoutine != null)
            StopCoroutine(_recoilRoutine);
        
        _recoilRoutine = StartCoroutine(PlayRecoil());

        if (isOwner) {
            //shotSoundPlayer.PlayOneShot(shotSounds[Random.Range(0, shotSounds.Count)], shotVolume / 3f);
            //FMODUnity.RuntimeManager.PlayOneShot("event:/GunShot", transform.position);\
            _shotEmmiter.Play();
        }
        else {
            //shotSoundPlayer.PlayOneShot(shotSounds[Random.Range(0, shotSounds.Count)], shotVolume);
            //FMODUnity.RuntimeManager.PlayOneShot("event:/GunShot", transform.position);
            _shotEmmiter.Play();
        }
    }

    private IEnumerator PlayRecoil() {
        float elapsed = 0f;
        while (elapsed < recoilDuration) {
            elapsed += Time.deltaTime;
            float curveTime = elapsed / recoilDuration;
            
            //Position recoil
            float recoilValue = recoilCurve.Evaluate(curveTime);
            Vector3 recoilOffset = Vector3.back * (recoilValue * recoilStrength);
            bodyTransform.localPosition = _originalPosition + recoilOffset;
            
            //Rotation recoil
            float rotationValue = rotationCurve.Evaluate(curveTime);
            Vector3 rotationOffset = new Vector3(rotationValue * rotationAmount, 0f, 0f);
            bodyTransform.localRotation = _originalRotation * Quaternion.Euler(rotationOffset);

            /*if (isKnife) {
                if (!rightKnife) {
                    float recoilValueX = recoilCurveLEFTHAND.Evaluate(curveTime);
                    Vector3 recoilOffsetX = Vector3.forward * (recoilValueX * recoilStrengthLEFTHAND);
                    bodyTransform.localPosition = _originalPosition + recoilOffsetX;
                
                    float rotationValueX = rotationCurveLEFTHAND.Evaluate(curveTime);
                    Vector3 rotationOffsetX = new Vector3(0f, rotationValueX * rotationAmountLEFTHAND, 0f);
                    bodyTransform.localRotation = _originalRotation * Quaternion.Euler(rotationOffsetX);
                }
                else {
                    float recoilValueY = recoilCurveRIGHTHAND.Evaluate(curveTime);
                    Vector3 recoilOffsetY = Vector3.forward * (recoilValueY * recoilStrengthRIGHTHAND);
                    bodyTransform.localPosition = _originalPosition + recoilOffsetY;
                
                    float rotationValueX = rotationCurveRIGHTHANDX.Evaluate(curveTime);
                    float rotationValueY = rotationCurveRIGHTHANDY.Evaluate(curveTime);
                    float rotationValueZ = rotationCurveRIGHTHANDZ.Evaluate(curveTime);
                    Vector3 rotationOffsetY = new Vector3(rotationValueY * rotationAmountRIGHTHANDX, rotationValueY * rotationAmountRIGHTHANDY, rotationValueY * rotationAmountRIGHTHANDZ);
                    bodyTransform.localRotation = _originalRotation * Quaternion.Euler(rotationOffsetY);
                }
            }*/
            
            yield return null;
        }
        bodyTransform.localPosition = _originalPosition;
        bodyTransform.localRotation = _originalRotation;
    }
}
