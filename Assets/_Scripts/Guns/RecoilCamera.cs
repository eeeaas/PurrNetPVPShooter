using System.Collections;
using UnityEngine;

public class RecoilCamera : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Gun currentGun;
    [SerializeField] private Transform camera; // MainCamera

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 12f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private float sensitivityMultiplier = 0.25f;
    private Coroutine recoilResetCoroutine;

    private Vector2 recoilTarget;
    private Vector2 recoilCurrent;
    [SerializeField] private int recoilIndex;
    private System.Random random;
    
    private bool isReduced;
    private float originalSensitivity;

    public Vector2 GetRecoilAngles() => recoilCurrent;

    public void SetGun(Gun gun)
    {
        currentGun = gun;
        if (gun == null) return;
        random = new System.Random(gun.name.GetHashCode());
        recoilIndex = 0;
    }

    private void Update()
    {
        if (camera != null)
            camera.localRotation = Quaternion.Euler(-recoilCurrent.y, recoilCurrent.x, 0f);
        if (currentGun == null) return;
        
        if (currentGun.isShoting) {
            ApplyRecoilStep();
            currentGun.isShoting = false;
            if (recoilResetCoroutine != null)
            {
                StopCoroutine(recoilResetCoroutine);
                recoilResetCoroutine = null;
            }
        }
        else
        {
            if (recoilResetCoroutine == null)
                recoilResetCoroutine = StartCoroutine(ResetRecoilAfterDelay());
        }

        recoilCurrent = Vector2.Lerp(recoilCurrent, recoilTarget, Time.deltaTime * smoothSpeed);
        recoilTarget = Vector2.Lerp(recoilTarget, Vector2.zero, Time.deltaTime * returnSpeed);
        if (recoilTarget.magnitude < 0.2f) {
            //recoilIndex = 0;
            //recoilTarget = Vector2.zero;
            Debug.Log("im zero");
        }
    }
    
    private IEnumerator ResetRecoilAfterDelay()
    {
        yield return new WaitForSeconds(currentGun.recoilResetDelay);
        recoilIndex = 0;
        //recoilTarget = Vector2.zero;
        recoilResetCoroutine = null;
    }


    public void ApplyRecoilStep()
    {
        if (currentGun == null) return;
        if (random == null)
            random = new System.Random(currentGun.name.GetHashCode());

        Vector2 step;
        if (currentGun.recoilPattern != null && currentGun.recoilPattern.Length > 0)
        {
            Vector2 patternStep = currentGun.recoilPattern[recoilIndex];
            recoilIndex++;
            if (recoilIndex >= currentGun.recoilPattern.Length - Random.Range(0,3)) {
                recoilIndex = currentGun.FromRecoilIndex;
            }
            step = new Vector2(patternStep.x * currentGun.xRecoilStrength, patternStep.y * currentGun.yRecoilStrength);
        }
        else
        {
            float x = (float)(random.NextDouble() * 2 - 1) * currentGun.xRecoilStrength;
            float y = Mathf.Lerp(currentGun.yRecoilStrength * 0.8f, currentGun.yRecoilStrength * 1.2f, (float)random.NextDouble());
            step = new Vector2(x, y);
        }

        recoilTarget += step;
        // Временно уменьшаем чувствительность
        if (playerController != null && !isReduced)
        {
            originalSensitivity = playerController.LookSensitivity;
            playerController.LookSensitivity *= sensitivityMultiplier;
            isReduced = true;
            playerController.StartCoroutine(RestoreSensitivity());
        }
    }

    private IEnumerator RestoreSensitivity()
    {
        yield return new WaitForSeconds(0.15f);
        if (playerController != null)
            playerController.LookSensitivity = originalSensitivity;
        isReduced = false;
    }
}
