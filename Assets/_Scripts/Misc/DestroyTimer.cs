using System.Collections;
using UnityEngine;

public class DestroyTimer : MonoBehaviour {
    [SerializeField] private float timeToDelete = 2f;
    
    private void Start() {
        StartCoroutine(Deleting());
    }

    private IEnumerator Deleting() {
        yield return new WaitForSeconds(timeToDelete);
        Destroy(gameObject);
    }
}
