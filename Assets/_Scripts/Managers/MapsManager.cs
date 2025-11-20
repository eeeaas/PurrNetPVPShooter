using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class MapsManager : NetworkBehaviour
{
    [SerializeField] private List<Transform> maps = new();

    private void Awake() {
        InstanceHandler.RegisterInstance(this);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        InstanceHandler.UnregisterInstance<MapsManager>();
    }
    
    [ObserversRpc(bufferLast:true)]
    public void SetMap(int mapId) {
        foreach (var map in maps) {
            map.gameObject.SetActive(false);
        }
        maps[mapId].gameObject.SetActive(true);
    }
}
