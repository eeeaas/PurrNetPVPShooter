using System.Collections;
using UnityEngine;
using PurrNet;
using PurrNet.StateMachine;

public class GameStateMachine : NetworkBehaviour
{
    [SerializeField] private StateMachine _machine;
    [SerializeField] private StateNode _startState;

    private void Awake()
    {
        _machine = GetComponent<StateMachine>();
    }

    protected override void OnSpawned(bool asServer) {
        base.OnSpawned(asServer);
        if(!asServer) return;
        
        StartCoroutine(DelayedStart());
    }


    private IEnumerator DelayedStart()
    {
        // Ждём один кадр, чтобы всё инициализировалось, и чтобы PurrNet точно пометил сцену как tracked
        while (!gameObject.activeInHierarchy)
            yield return null;

        // (опционально) убедимся, что PurrNet видит сцену как tracked — полезный debug
        Debug.Log("[GameStateMachineBootstrap] Starting StateMachine on server in scene: " + gameObject.scene.name);

        _machine.SetState(_startState);
    }
}