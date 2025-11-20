using PurrNet;
using UnityEngine;

public class Ammo : NetworkBehaviour
{
    [Header("Ammo")]
    [SerializeField] public SyncVar<int> currentAmmo = new();
    [SerializeField] public int maxPrimaryAmmo = 30;
    [SerializeField] public SyncVar<int> secondaryAmmo = new(120);

    public SyncVar<bool> zeroAmmo = new(false);
    public SyncVar<bool> zeroSecondaryAmmo = new(false);

    // Локальные (ненетворк) предсказанные значения, используются у владельца для мгновенной обратной связи.
    private int _predictedCurrentAmmo;
    private int _predictedSecondaryAmmo;
    private bool _predictedZeroAmmo;
    private bool _predictedZeroSecondaryAmmo;

    protected override void OnSpawned() {
        base.OnSpawned();

        if (isServer) {
            currentAmmo.value = maxPrimaryAmmo;
        }

        // Инициализация локальных значений из SyncVar'ов
        _predictedCurrentAmmo = currentAmmo.value;
        _predictedSecondaryAmmo = secondaryAmmo.value;
        _predictedZeroAmmo = zeroAmmo.value;
        _predictedZeroSecondaryAmmo = zeroSecondaryAmmo.value;

        if (isOwner) {
            InstanceHandler.GetInstance<MainGameView>().UpdateAmmoText(GetCurrentPrimaryEffective(), GetSecondaryEffective());
            currentAmmo.onChanged += OnSyncCurrentAmmoChanged;
            secondaryAmmo.onChanged += OnSyncSecondaryAmmoChanged;
            zeroAmmo.onChanged += OnSyncZeroAmmoChanged;
            zeroSecondaryAmmo.onChanged += OnSyncZeroSecondaryAmmoChanged;
        }
    }

    private void OnSyncCurrentAmmoChanged(int newVal) {
        // Авторитетное значение от сервера — обновляем локальную предсказанную копию и UI.
        _predictedCurrentAmmo = newVal;
        _predictedZeroAmmo = zeroAmmo.value;
        if (isOwner) InstanceHandler.GetInstance<MainGameView>().UpdateAmmoText(GetCurrentPrimaryEffective(), GetSecondaryEffective());
    }

    private void OnSyncSecondaryAmmoChanged(int newVal) {
        _predictedSecondaryAmmo = newVal;
        _predictedZeroSecondaryAmmo = zeroSecondaryAmmo.value;
        if (isOwner) InstanceHandler.GetInstance<MainGameView>().UpdateAmmoText(GetCurrentPrimaryEffective(), GetSecondaryEffective());
    }

    private void OnSyncZeroAmmoChanged(bool newVal) {
        _predictedZeroAmmo = newVal;
        if (isOwner) InstanceHandler.GetInstance<MainGameView>().UpdateAmmoText(GetCurrentPrimaryEffective(), GetSecondaryEffective());
    }

    private void OnSyncZeroSecondaryAmmoChanged(bool newVal) {
        _predictedZeroSecondaryAmmo = newVal;
        if (isOwner) InstanceHandler.GetInstance<MainGameView>().UpdateAmmoText(GetCurrentPrimaryEffective(), GetSecondaryEffective());
    }

    private void OnDestroy() {
        base.OnDestroy();
        currentAmmo.onChanged -= OnSyncCurrentAmmoChanged;
        secondaryAmmo.onChanged -= OnSyncSecondaryAmmoChanged;
        zeroAmmo.onChanged -= OnSyncZeroAmmoChanged;
        zeroSecondaryAmmo.onChanged -= OnSyncZeroSecondaryAmmoChanged;
    }

    // Метод, который возвращает текущее эффективное значение патронов для использования в Gun:
    // у владельца — локальная (предсказанная), у остальных — SyncVar.
    public int GetCurrentPrimaryEffective() {
        return isOwner ? _predictedCurrentAmmo : currentAmmo.value;
    }

    public int GetSecondaryEffective() {
        return isOwner ? _predictedSecondaryAmmo : secondaryAmmo.value;
    }

    public bool IsZeroPrimaryEffective() {
        return isOwner ? _predictedZeroAmmo : zeroAmmo.value;
    }

    public bool IsZeroSecondaryEffective() {
        return isOwner ? _predictedZeroSecondaryAmmo : zeroSecondaryAmmo.value;
    }

    // Обновление UI вручную
    public void UpdateAmmoUI() {
        InstanceHandler.GetInstance<MainGameView>().UpdateAmmoText(GetCurrentPrimaryEffective(), GetSecondaryEffective());
    }

    // Меняем боезапас — если вызвано клиентом, делаем локальную предсказанную корректировку и отправляем ServerRpc.
    public void ChangeAmmo(int amount) {
        if (!isServer) {
            // ПРЕДСКАЗАНИЕ: мгновенно уменьшаем локальное значение у владельца для избежания гонок UI/логики
            if (isOwner) {
                _predictedCurrentAmmo += amount;
                if (_predictedCurrentAmmo < 0) _predictedCurrentAmmo = 0;
                _predictedZeroAmmo = _predictedCurrentAmmo <= 0;
                UpdateAmmoUI();
            }
            ChangeAmmo_ServerRpc(amount);
            return;
        }

        // Серверная ветка: авторитетно меняем SyncVar'ы
        currentAmmo.value += amount;

        if (currentAmmo.value < 1) {
            zeroAmmo.value = true;
            currentAmmo.value = 0;
        }
        else zeroAmmo.value = false;

        // На сервере также держим предсказанные значения в согласии, чтобы локальный владелец (если это хост) был в порядке.
        _predictedCurrentAmmo = currentAmmo.value;
        _predictedZeroAmmo = zeroAmmo.value;
    }

    // То же для вторичных патронов
    public void ChangeSecondaryAmmo(int amount) {
        if (!isServer) {
            if (isOwner) {
                _predictedSecondaryAmmo += amount;
                if (_predictedSecondaryAmmo < 0) _predictedSecondaryAmmo = 0;
                _predictedZeroSecondaryAmmo = _predictedSecondaryAmmo <= 0;
                UpdateAmmoUI();
            }
            ChangeSecondaryAmmo_ServerRpc(amount);
            return;
        }
        secondaryAmmo.value += amount;
        if (secondaryAmmo.value < 1) {
            zeroSecondaryAmmo.value = true;
            secondaryAmmo.value = 0;
        } else {
            zeroSecondaryAmmo.value = false;
        }

        _predictedSecondaryAmmo = secondaryAmmo.value;
        _predictedZeroSecondaryAmmo = zeroSecondaryAmmo.value;
    }

    [ServerRpc]
    public void ChangeSecondaryAmmo_ServerRpc(int amount) {
        // Серверная реализация (повторно, т.к. клиент вызывает RPC)
        secondaryAmmo.value += amount;
        if (secondaryAmmo.value < 1) {
            zeroSecondaryAmmo.value = true;
            secondaryAmmo.value = 0;
        } else {
            zeroSecondaryAmmo.value = false;
        }
    }

    [ServerRpc]
    public void ChangeAmmo_ServerRpc(int amount) {
        currentAmmo.value += amount;

        if (currentAmmo.value < 1) {
            zeroAmmo.value = true;
            currentAmmo.value = 0;
        }
        else zeroAmmo.value = false;
    }
}