using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLobby : MonoBehaviour{
    public static GameLobby Instance {get; private set;}

    private Lobby joinedLobby;

    public event EventHandler CreateLobbyStart;
    public event EventHandler CreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler JoinFailed;
    public event EventHandler<LobbyListChangeEventArgs> LobbyListChange;
    public class LobbyListChangeEventArgs : EventArgs{
        public List<Lobby> lobbyList;
    }


    private void Awake(){
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeUnityAuth();
    }

    private async void InitializeUnityAuth(){
        if(UnityServices.State != ServicesInitializationState.Initialized){
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 1000).ToString());

            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private bool LobbyHost(){
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private async Task<Allocation> AllocateRelay(){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(GameMultiplayer.MAX_PLAYER_AMOUNT-1);
            return allocation;
        }catch(RelayServiceException e){
                Debug.Log(e);
                return default;
        }
    }
    private async Task<String> GetRelayJoinCode(Allocation allocation){
        
        try{
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            return relayJoinCode;
        }catch(RelayServiceException e){
            Debug.Log(e);
            return default;
        }
    }
    private async Task<JoinAllocation> JoinRelay(string joinCode){
        try{
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            return joinAllocation;
        }catch(RelayServiceException e){
            Debug.Log(e);
            return default;

        }
    }
    public async void CreateLobby(string lobbyName, bool isPrivate){
        CreateLobbyStart?.Invoke(this, EventArgs.Empty);
        try{
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 3, new CreateLobbyOptions{
                IsPrivate = isPrivate,
            });
            
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions{
                Data = new Dictionary<string, DataObject>{
                    {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
                } 
            });
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            GameMultiplayer.Instance.StartHost();
            Debug.Log("lobby code - " + lobbyCode + " relay join code - " + relayJoinCode);
            Loader.LoadNetwork(Loader.Scene.CharacterSelectionScene);
        }
        catch(LobbyServiceException e){
            Debug.Log(e);
            CreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void QuickJoinLobby(){
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
        try{
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            
            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            GameMultiplayer.Instance.StartClient();

        } catch(LobbyServiceException e){
            Debug.Log(e);
            JoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public Lobby GetLobby(){
        return joinedLobby;
    }

    public async void DeleteLobby(){
        if(joinedLobby != null){
            try{
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
                
                joinedLobby = null;
            }catch(LobbyServiceException e){
                Debug.Log(e);
            }
        }
    }

    public async void LeaveLobby(){
         if(joinedLobby != null){
            try{
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }catch(LobbyServiceException e){
                    Debug.Log(e);
            }
        }
    }
    public async void KickPlayer(string playerId){
         if(LobbyHost()){
            try{
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }catch(LobbyServiceException e){
                    Debug.Log(e);
            }
        }
    }
    public async void JoinCode(string lobbyCode){
        OnJoinStarted?.Invoke(this, EventArgs.Empty);

        try{
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value; 
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            GameMultiplayer.Instance.StartClient();
            
        } catch(LobbyServiceException e){
            Debug.Log(e);
            JoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
    public async void JoinId(string lobbyId){
        OnJoinStarted?.Invoke(this, EventArgs.Empty);

        try{
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            
            string relayJoinCode = joinedLobby.Data["RelayJoinCode"].Value;
            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            GameMultiplayer.Instance.StartClient();
            
        } catch(LobbyServiceException e){
            Debug.Log(e);
            JoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }
}
