using System.Collections;
using PurrNet;
using TMPro;
using UnityEngine;

public class EndGameView : View {
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private TMP_Text winnerText;
    
    private void Awake() {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy() {
        InstanceHandler.UnregisterInstance<EndGameView>();
    }

    public void SetWinner(TeamID winner) {
        winnerText.text = $"{winner} wins the game!";
    }

    public override void OnShow() {
        
    }

    public override void OnHide() {
        
    }
}
