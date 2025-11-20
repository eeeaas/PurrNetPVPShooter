using UnityEngine;

public class DrawAnimator : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    public void DrawAnimEnd() {
        if (playerController == null) return;

        if (playerController.isOwner) {
            playerController.OnDrawWeaponComplete();
        } else {
            // Удалённые клиенты — просто визуально завершают (если нужно)
            var w = playerController.GetCurrentWeapon();
            if (w != null) w.isLookAnim = false;
        }
    }

    public void ReloadAnimEnd() {
        if (playerController == null) return;

        if (playerController.isOwner) {
            // Владелец сообщает своему PlayerController, что анимация закончилась
            playerController.OnReloadComplete();
        } else {
            // Удалённые клиенты: отключаем визуальный флаг перезарядки локально
            var w = playerController.GetCurrentWeapon();
            if (w != null) w.isReloading = false;
        }
    }

    public void UnsetHands() {
        if (playerController == null) return;
        playerController.GetCurrentWeapon().isLookAnim = true;
    }
    
    public void SetHands() {
        if (playerController == null) return;
        playerController.GetCurrentWeapon().isLookAnim = false;
    }
}