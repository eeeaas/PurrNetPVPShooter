using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class ScoreboardView : View {
    [SerializeField] private Transform scoreboardEntriesParent;
    [SerializeField] private ScoreboardEntry scoreboardEntryPrefab;
    
    private GameViewManager _gameViewManager;

    private void Awake() {
        InstanceHandler.RegisterInstance(this);
    }

    private void Start() {
        _gameViewManager = InstanceHandler.GetInstance<GameViewManager>();
    }

    private void OnDestroy() {
        InstanceHandler.UnregisterInstance<ScoreboardView>();
    }

    public void SetData(Dictionary<PlayerID, ScoreManager.ScoreData> data) {
        foreach (Transform child in scoreboardEntriesParent) {
            Destroy(child.gameObject);
        }
        
        foreach (var playerScore in data) {
            var entry = Instantiate(scoreboardEntryPrefab, scoreboardEntriesParent);
            entry.SetData(playerScore.Key.id.ToString(), playerScore.Value.kills, playerScore.Value.deaths);
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            _gameViewManager.ShowView<ScoreboardView>(false);
        }

        if (Input.GetKeyUp(KeyCode.Tab)) {
            _gameViewManager.HideView<ScoreboardView>();
        }
    }

    public override void OnShow() {
        
    }

    public override void OnHide() {
        
    }
}
