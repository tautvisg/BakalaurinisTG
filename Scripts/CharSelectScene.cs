using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;


public class CharSelectScene : NetworkBehaviour{

    public static CharSelectScene Instance{get; private set;}

    public event EventHandler OnReadyChanged;

    private Dictionary<ulong, bool> readyList;

    private void Awake(){
        Instance = this;
        readyList = new Dictionary<ulong, bool>();
    }

    public void SetPlayerReady(){
        SetPlayerReadyServerRpc();
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default){
        SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        readyList[serverRpcParams.Receive.SenderClientId] = true;

        bool allReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(!readyList.ContainsKey(clientId) || !readyList[clientId]){ 
                allReady = false;
                break;
            }
        }
        if(allReady){
            GameLobby.Instance.DeleteLobby();
            Loader.LoadNetwork(Loader.Scene.Level1_1);
        }
    }
    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId){
        readyList[clientId] = true;
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }
    public bool IsPlayerReady(ulong clientId){
        return readyList.ContainsKey(clientId) && readyList[clientId];
    }
}
