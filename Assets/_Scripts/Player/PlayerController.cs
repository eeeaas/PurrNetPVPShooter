using System;
using System.Collections;
using System.Collections.Generic;
using Fragsurf.Movement;
using PurrNet;
using PurrNet.StateMachine;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private float gravity = -9.81f;
    
    [SerializeField] private float coyoteTime = 0.2f; // 0.2 сек после отрыва от земли
    private float coyoteTimer;

    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    public float LookSensitivity { get => lookSensitivity; set { lookSensitivity = value; } }

    [Header("References")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private NetworkAnimator animator;
    [SerializeField] private StateMachine stateMachine;
    [SerializeField] private List<StateNode> weaponStates = new();
    [SerializeField] private RecoilCamera recoilCamera;
    [SerializeField] private SurfCharacter surfCharacter;
    [SerializeField] private JumpSoundsManager jumpSoundsManager;
    [SerializeField] private SwitchSoundsManager switchSoundsManager;
    [SerializeField] private ReloadSoundsManager reloadSoundsManager;
    [SerializeField] public PlayerAiming playerAiming;
    
    //[SerializeField] private List<AudioClip> stepsSounds;
    //[SerializeField] private List<AudioClip> switchingSounds;
    //[SerializeField] private AudioSource stepsAudioSource;
    //[SerializeField] private AudioSource switchAudioSource;
    [SerializeField] private FMODUnity.StudioEventEmitter eventEmitter;
    [SerializeField] private Animator weaponAnimator;
    public bool isDrawingWeapon = false;

    
    [SerializeField, Range(0f,1f)] private float stepsVolume = 0.5f;
    [SerializeField, Range(0f,3f)] private float stepsInterval = 0.25f;
    [SerializeField] public bool isWalk;
    [SerializeField] private bool isSprint;
    public bool uiIsOpen = false;
    private bool wasGrounded;


    //[SerializeField] private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private float weightMultiplier = 1f;
    
    //public bool GetIsGrounded => characterController.isGrounded;

    private Vector3 moveDir;
    public Vector3 GetMoveDirection => moveDir;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
        surfCharacter.enabled = isOwner;
        playerAiming.enabled = isOwner;
        
        tempWalkSpeed = surfCharacter.movementConfig.walkSpeed;
        tempSprintSpeed = surfCharacter.movementConfig.sprintSpeed;
        //characterController.enabled = isOwner;
        if(isOwner) DrawWeapon(0);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            enabled = false;
            return;
        }
    }

    private bool isInAir = false;        // игрок сейчас в воздухе
    private bool justLanded = false;     // игрок только что приземлился
    private float landingTimer = 0f;     // таймер после приземления
    [SerializeField] private float landingCooldown = 0.3f; // 1 секунда буфера
    private float lastJumpTime = -1f;    // для общего cooldown прыжка

    private void Update()
    {
        HandleFootsteps();
        HandleWeaponSwitching();
        //HandleMovement();
        isSprint = surfCharacter.moveData.sprinting;
        isWalk = surfCharacter.moveDir.magnitude > 0.2f && IsGrounded() && !isSprint;
        animator.SetFloat("Forward", surfCharacter.vert);
        animator.SetFloat("Sideways", surfCharacter.horiz);
        
        HandleJumpsSounds();
    }

    private void HandleJumpsSounds() {
        bool isGrounded = IsGrounded();
        float currentTime = Time.time;

        // --- Таймер после приземления ---
        if (justLanded)
        {
            landingTimer += Time.deltaTime;
            if (landingTimer >= landingCooldown)
            {
                justLanded = false;
                landingTimer = 0f;
            }
        }

        // --- Прыжок ---
        if (!isInAir && !isGrounded)
        {
            // Игрок оторвался от земли
            // Проверяем: если только что приземлились и таймер не истек — не проигрываем звук прыжка
            if (!justLanded)
            {
                jumpSoundsManager.JumpSoundStart();
                lastJumpTime = currentTime;
            }
            isInAir = true;
        }

        // --- Приземление ---
        if (isInAir && isGrounded)
        {
            jumpSoundsManager.JumpSoundEnd();
            isInAir = false;

            // Запускаем таймер, запрещающий звук прыжка после приземления
            justLanded = true;
            landingTimer = 0f;

            lastJumpTime = -1f;
        }

        // Обновляем состояние для следующего кадра
        wasGrounded = isGrounded;
    }
    
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckOffset = -0.6f;
    [SerializeField] private LayerMask groundMask;

    public bool IsGrounded()
    {
        Vector3 spherePos = transform.position + Vector3.up * groundCheckOffset;
        return Physics.CheckSphere(spherePos, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }
    
    /*
    private void HandleMovement()
    {
        bool isGrounded = GetIsGrounded;
        
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        moveDir = moveDirection;

        float currentSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
            isSprint = true;
        }
        else
        {
            currentSpeed = moveSpeed;
            isSprint = false;
        }
        currentSpeed *= weightMultiplier;

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        isWalk = moveDirection.magnitude > 0.2f && isGrounded && !isSprint;

        if (Input.GetButtonDown("Jump") && coyoteTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            coyoteTimer = 0f; // сброс таймера после прыжка
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        animator.SetFloat("Forward", vertical);
        animator.SetFloat("Sideways", horizontal);
    }*/

    private float stepsTimer = 0f;
    private void HandleFootsteps()
    {
        if (!isWalk)
        {
            return;
        }

        stepsTimer += Time.deltaTime;
        if (stepsTimer >= stepsInterval)
        {
            stepsTimer = 0f;
            PlayFootstepSoundRpc();
        }
    }

    [ObserversRpc]
    private void PlayFootstepSoundRpc()
    {
        float volume = isOwner ? stepsVolume / 3f : stepsVolume;
        eventEmitter.Play();
    }

    /*private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        Vector2 recoilAngles = Vector2.zero;
        if (playerCamera.TryGetComponent(out RecoilCamera recoilCam))
        {
            recoilAngles = recoilCam.GetRecoilAngles();
        }

        recoilCamera.transform.localRotation =
            Quaternion.Euler(verticalRotation - recoilAngles.y, recoilAngles.x, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }*/

    private void HandleWeaponSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            DrawWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            DrawWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            DrawWeapon(2);
    }

    private float tempWalkSpeed;
    private float tempSprintSpeed;
    private Gun currentGun;
    private void DrawWeapon(int weaponIndex)
    {
        if (isDrawingWeapon) return; // уже достаем

        isDrawingWeapon = true;
        
        // Меняем состояние оружия
        Gun curGun = weaponStates[weaponIndex].GetComponent<Gun>();
        stateMachine.SetState(weaponStates[weaponIndex]);
        recoilCamera.SetGun(curGun);
        weightMultiplier = curGun.weightMultiplier;
        surfCharacter.movementConfig.walkSpeed = tempWalkSpeed * weightMultiplier;
        surfCharacter.movementConfig.sprintSpeed = tempSprintSpeed * weightMultiplier;
        switchSoundsManager.OnGunSwitch(weaponIndex);
        currentGun = curGun;
        RpcStopReloadSounds();
        if (!curGun.isKnife) {
            InstanceHandler.GetInstance<MainGameView>().ShowAmmo();
            curGun.ammo.UpdateAmmoUI();
            curGun.isReloading = false;
        }
        else {
            InstanceHandler.GetInstance<MainGameView>().HideAmmo();
        }
        weaponAnimator.enabled = true;
    
        // **Сбрасываем аниматор на "нулевую" позу**
        weaponAnimator.Play("Idle", 0, 0f);
        weaponAnimator.Update(0f);

        // Сброс всех триггеров
        weaponAnimator.ResetTrigger("T_DrawPistol");
        weaponAnimator.ResetTrigger("T_DrawRifle");
        weaponAnimator.ResetTrigger("T_DrawKnife");
        /*weaponAnimator.ResetTrigger("T_KnifeLeft");
        weaponAnimator.ResetTrigger("T_KnifeRight");
        weaponAnimator.ResetTrigger("T_PistolLook");
        weaponAnimator.ResetTrigger("T_RifleLook");
        weaponAnimator.ResetTrigger("T_KnifeLook");*/

        // Выбираем триггер по gunIdForAnim
        int gunId = curGun.gunIdForAnim;
        switch (gunId)
        {
            case 0: weaponAnimator.SetTrigger("T_DrawPistol"); break;
            case 1: weaponAnimator.SetTrigger("T_DrawRifle"); break;
            case 2: weaponAnimator.SetTrigger("T_DrawKnife"); break;
        }

        // Флаг isDrawingWeapon сбросится в конце анимации через Animation Event
    }

    [ObserversRpc]
    private void RpcStopReloadSounds() {
        reloadSoundsManager.StopReload();
    }
    
// Этот метод вызываем через Animation Event в конце анимации достания
    public void OnDrawWeaponComplete()
    {
        isDrawingWeapon = false;
    }

    public void OnReloadComplete() {
        if (isOwner) {
            GetCurrentWeapon().ReloadComplete();
        }
    }

    public Gun GetCurrentWeapon() {
        return currentGun;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.03f, Vector3.down * groundCheckDistance);
        
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        Vector3 end = origin + Vector3.down * (groundCheckDistance + groundCheckRadius);


        // верхняя сфера (старт)
        Gizmos.DrawWireSphere(origin, groundCheckRadius);

        // нижняя сфера (конец)
        Gizmos.DrawWireSphere(end, groundCheckRadius);

        // соединительная линия
        Gizmos.DrawLine(origin + Vector3.right * groundCheckRadius, end + Vector3.right * groundCheckRadius);
        Gizmos.DrawLine(origin - Vector3.right * groundCheckRadius, end - Vector3.right * groundCheckRadius);
        Gizmos.DrawLine(origin + Vector3.forward * groundCheckRadius, end + Vector3.forward * groundCheckRadius);
        Gizmos.DrawLine(origin - Vector3.forward * groundCheckRadius, end - Vector3.forward * groundCheckRadius);
    }
#endif
}
