using System;
using PurrNet;
using TMPro;
using UnityEngine;

public class MainGameView : View {
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text yourTeamText;

    [SerializeField] private TMP_Text primaryAmmoText;
    [SerializeField] private TMP_Text secondaryAmmoText;
    [SerializeField] private GameObject ammoGameObject;

    private void Awake() {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy() {
        InstanceHandler.UnregisterInstance<MainGameView>();
    }

    public override void OnShow() {
        
    }

    public override void OnHide() {
        
    }

    public void ShowAmmo() {
        ammoGameObject.SetActive(true);
    }

    public void HideAmmo() {
        ammoGameObject.SetActive(false);
    }

    public void UpdateAmmoText(int ammo, int secondaryAmmo) {
        primaryAmmoText.text = ammo.ToString();
        secondaryAmmoText.text = secondaryAmmo.ToString();
    }

    public void UpdateYourTeam(bool team) {
        yourTeamText.text = team ? "TeamA" : "TeamB";
    }

    public void UpdateScore(int a, int b) {
        scoreText.text = "TeamA   " + a + " : " + b + "   TeamB";
    }

    public void UpdateHealth(int health) {
        if (health < 0) {
            health = 0;
        }
        healthText.text = health.ToString();
    }
}
