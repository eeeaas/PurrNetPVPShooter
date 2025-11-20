using UnityEngine;
using PurrNet;

/// <summary>
/// Помечает корневой объект игрока его PlayerID.
/// Используется сервером для фильтрации self-hit при rollback-рейкастах.
/// </summary>
public class OwnerTag : MonoBehaviour
{
    public PlayerID owner;
}