using UnityEngine;

public class FpsFix : MonoBehaviour
{
    void Start() {
        Application.targetFrameRate = 120;
    }
}
