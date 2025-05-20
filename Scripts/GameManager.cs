using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;
using UnityEngine.AI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class GameManager : NetworkBehaviour{
    
    public static GameManager Instance {get; private set;}

    public event EventHandler GameStateChangedEvent;
    public event EventHandler GameManagerPausedEvent;
    public event EventHandler GameManagerUnpausedEvent;
    public event EventHandler LocalPlayerReady;
    public event EventHandler GamePausedMP;
    public event EventHandler GameUnpausedMP;

    private enum GameState{
        WaitingStart,
        CountdownTime,
        LevelStarted,
        LevelEnded
    }
    private NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(
        GameState.WaitingStart, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    [SerializeField] private NetworkVariable<float> levelDuration = new  NetworkVariable<float>(300f); // duration that the specific level has
    private bool isLocalPlayerReady;
    private NetworkVariable<float> countdown = new NetworkVariable<float>(3f); // countdown for the level to start
    private float timePassed = 0f;
    private bool Paused = false;  
    private NetworkVariable<bool>GamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> readyList;
    private Dictionary<ulong, bool> playerPauses;
    [SerializeField] private Transform playerPrefab;

    private void Awake(){
        Instance = this;
        readyList = new Dictionary<ulong, bool>();
        playerPauses = new Dictionary<ulong, bool>();
        if (!IsServer && gameState.Value != GameState.LevelStarted)
            Time.timeScale = 0f;
    }
    private void Start(){
        GameInput.Instance.PauseGameEvent += GameInput_PauseGameEvent;
        GameInput.Instance.TakeActionEvent += GameInput_TakeActionEvent;
    }
    public override void OnNetworkSpawn(){
        gameState.OnValueChanged += GameState_OnValueChanged;
        GamePaused.OnValueChanged += GamePaused_OnValueChanged;
        if(IsServer){
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }
    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }
    private void GamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if(GamePaused.Value){
            Time.timeScale = 0f;
            GamePausedMP?.Invoke(this, EventArgs.Empty);
        }else{
            Time.timeScale = 1f;
            GameUnpausedMP?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        GameStateChangedEvent?.Invoke(this, EventArgs.Empty);

        switch (newValue){
            case GameState.WaitingStart:
            case GameState.CountdownTime:
            case GameState.LevelEnded:
                Time.timeScale = 0f;
                break;
            case GameState.LevelStarted:
                Time.timeScale = 1f;
                break;
        }
    }

    private void GameInput_TakeActionEvent(object sender, EventArgs e){
        if(gameState.Value == GameState.WaitingStart){
            isLocalPlayerReady = true;

            LocalPlayerReady?.Invoke(this, EventArgs.Empty);
            SetPlayerReadyServerRpc();

        }
    }

      [ServerRpc (RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default){
        readyList[serverRpcParams.Receive.SenderClientId] = true;
        bool allReady = true;
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(!readyList.ContainsKey(clientId) || !readyList[clientId]){
                // This player is not ready
                allReady = false;
                break;
            }
        }
        if(allReady){
            gameState.Value = GameState.CountdownTime;
        }
    }

    private void Update(){
        if(!IsServer){
            return;
        }

        switch(gameState.Value){
            case GameState.WaitingStart:
                Time.timeScale = 0f;
                break;
            case GameState.CountdownTime:
                countdown.Value -= Time.unscaledDeltaTime;
                if(countdown.Value <= 0){
                    gameState.Value = GameState.LevelStarted;
                    Time.timeScale = 1f;
                }
                break;
            case GameState.LevelStarted:
                levelDuration.Value -= Time.deltaTime;
                timePassed += Time.deltaTime;
                if(levelDuration.Value < 0f){
                    gameState.Value = GameState.LevelEnded;
                }
                break;
            case GameState.LevelEnded:
                Time.timeScale = 0;
                break;
        }
    }
    private void GameInput_PauseGameEvent(object sender, System.EventArgs e){
        PauseGame();
    }
    public void PauseGame(){
        Paused = !Paused; 
        if (Paused){
            PauseGameServerRpc();
            GameManagerPausedEvent?.Invoke(this, EventArgs.Empty);
        } else {
            UnpauseGameServerRpc();
            GameManagerUnpausedEvent?.Invoke(this, EventArgs.Empty);
        }
    }
    [ServerRpc (RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default){
        playerPauses[serverRpcParams.Receive.SenderClientId] = true;
        TestPauses();
    }
    [ServerRpc (RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default){
        playerPauses[serverRpcParams.Receive.SenderClientId] = false;
        TestPauses();
    }
    private void TestPauses(){
        foreach(ulong clientId in NetworkManager.Singleton.ConnectedClientsIds){
            if(playerPauses.ContainsKey(clientId) && playerPauses[clientId]){
                // player is paused;
                GamePaused.Value = true;
                return;
            }
        }

        // All players are unpaused;
        GamePaused.Value = false;
    }

    public bool CountdownActive(){
        return gameState.Value == GameState.CountdownTime;
    }
    public float GetCountdown(){
        return countdown.Value;
    }
    public bool IsWaitingToStart() {
        return gameState.Value == GameState.WaitingStart;
    }
    public bool HasCountdownStarted() {
        return gameState.Value == GameState.CountdownTime;
    }
    public bool HasLevelEnded(){
        return gameState.Value == GameState.LevelEnded;
    }  
    public bool HasLevelStarted(){
        return gameState.Value == GameState.LevelStarted;
    }
    public float GetLevelDuration(){
        return levelDuration.Value;
    }
    public float GetTimePassed(){
        return timePassed;
    }
    public bool IsLocalPlayerReady(){
        return isLocalPlayerReady;
    }
}
