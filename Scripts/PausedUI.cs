using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PausedGameMenuUI : MonoBehaviour{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake(){
        resumeButton.onClick.AddListener( () => {
            GameManager.Instance.PauseGame();
        });
        optionsButton.onClick.AddListener( () =>{
            OptionsUI.Instance.Show();
        });
        mainMenuButton.onClick.AddListener( () => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }
    private void Start(){
        GameManager.Instance.GameManagerPausedEvent += GameManager_GameManagerPausedEvent; // Locally
        GameManager.Instance.GameManagerUnpausedEvent += GameManager_GameManagerUnpausedEvent; // Locally
        Hide();
    }
    private void GameManager_GameManagerPausedEvent(object sender, EventArgs e){
        Show();
    }
    private void GameManager_GameManagerUnpausedEvent(object sender, EventArgs e){
        Hide();
    }
    private void Show(){
        gameObject.SetActive(true);
    }
    private void Hide(){
        gameObject.SetActive(false);
    }
}
