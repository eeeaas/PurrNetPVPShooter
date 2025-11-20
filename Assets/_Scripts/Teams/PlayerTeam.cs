using PurrNet;
using UnityEngine;

public class PlayerTeam : NetworkBehaviour {
    [SerializeField] private SyncVar<TeamID> team = new();

    public TeamID Team => team.value;

    public void SetTeam(TeamID newTeam) {
        if (isServer)
            team.value = newTeam;
    }

    protected override void OnSpawned() {
        base.OnSpawned();
        if (!isOwner) return;
        if (InstanceHandler.TryGetInstance(out MainGameView mainGameView))
        {
            bool isTeamA = Team == TeamID.TeamA;
            mainGameView.UpdateYourTeam(isTeamA);
        }
    }
}