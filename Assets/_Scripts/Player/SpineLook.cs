using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SpineLook : MonoBehaviour
{
    [Header("References")]
    public Transform spineConstraint;

    public Transform target;


    void Update()
    {
        spineConstraint.SetPositionAndRotation(target.position, target.rotation);
    }
}