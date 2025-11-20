using System.Collections.Generic;
using System.Linq;
using PurrNet;
using PurrNet.StateMachine;
using UnityEngine;

public class GameEndState : StateNode
{
    public override void Enter(bool asServer) {
        base.Enter(asServer);
        if (!asServer) return;
        
        if (!InstanceHandler.TryGetInstance(out RoundManager roundManager)) return;
        var winner = roundManager.GetMatchWinner();

    }
}
