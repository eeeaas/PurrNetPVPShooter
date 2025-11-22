using UnityEngine;

public class BotHealth : MonoBehaviour {
    [SerializeField] private int health = 100;
    
    public void ChangeHealth(int amount) {
        health += amount;

        if (health <= 0) {
            Destroy(gameObject);
        }
    }
}
