using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader{

    // always check here if scenes match builder i unity
    public enum Scene{
        MainMenuScene,
        Level1_1, Level1_2, Level1_3, Level1_4, Level1_5,
        Level2_1, Level2_2, Level2_3, Level2_4, Level2_5,
        Level3_1, Level3_2, Level3_3, Level3_4, Level3_5,
        Level4_1, Level4_2, Level4_3, Level4_4, Level4_5,
        LoadingScene,
        LobbyScene,
        CharacterSelectionScene
    }
    private static Scene target;
    private static AsyncOperation asyncOperation;

    public static void Load(Scene target){
        Loader.target = target;

        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }
    public static void LoadNetwork(Scene target){
        NetworkManager.Singleton.SceneManager.LoadScene(target.ToString(), LoadSceneMode.Single);
    }

    public static void LoadAsync(Scene target){
        Loader.target = target;

        asyncOperation = SceneManager.LoadSceneAsync(Scene.LoadingScene.ToString());
    }
    public static void LoaderCallback(){
        if (asyncOperation != null && asyncOperation.isDone){
            asyncOperation = SceneManager.LoadSceneAsync(target.ToString());
        }
        else{
            SceneManager.LoadScene(target.ToString());
        }    
    }
}
