using System;
using System.Collections;
using UnityEngine;

public class SmoothArms : MonoBehaviour {
    [SerializeField] private float smoothDuration = 0.2f;
    [SerializeField] private float smoothStrength = 1f;
    [SerializeField] private AnimationCurve smoothCurve;
    
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    
    float elapsed = 0f;

    private void Start() {
        _originalPosition = transform.localPosition;
        _originalRotation = transform.localRotation;
    }

    private void Update() {
        elapsed += Time.deltaTime;
        float curveTime = elapsed / smoothDuration;
            
        //Position recoil
        float recoilValue = smoothCurve.Evaluate(curveTime);
        Vector3 recoilOffset = new Vector3(0,0.1f,0) * (recoilValue * smoothStrength);
        transform.localPosition = _originalPosition + recoilOffset;

        if (elapsed >= smoothDuration) {
            elapsed = 0f;
        }
    }
}
