using UnityEngine;

public enum HitZone { Head, Body, Arm, Leg }

public class Hitbox : MonoBehaviour
{
    public HitZone hitZone;
    public PlayerHealth playerHealth;
}
