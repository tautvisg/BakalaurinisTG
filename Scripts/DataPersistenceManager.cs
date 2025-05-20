using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class DataPersistenceManager : MonoBehaviour{   
    [SerializeField] private string fileName;
    public static DataPersistenceManager Instance {get; private set;}
    [SerializeField] private bool useEncryption;

    private GameData gameData;
    private FileDataHandler fileDataHandler;
    private string selectedProfileID = "";
    private List<IDataPersistence> dataPersistenceObjects;



    private void Awake(){
        if(Instance != null){
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

        this.selectedProfileID = fileDataHandler.GetMostRecentlyPlayedSaveFile();
    }
    private void OnEnable(){
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

    }
    private void OnDisable(){
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();

        //AbilityManager.Instance.LoadData(gameData); // Maybe this is bad check later here
    }
    public void OnSceneUnloaded(Scene scene){
        SaveGame();
    }
    public void ChangeSelectedProfileId(string newProfileId){
        this.selectedProfileID = newProfileId;
        LoadGame();
     }
    public void NewGame(){
        this.gameData = new GameData();
    }
    public void LoadGame(){

        this.gameData = fileDataHandler.Load(selectedProfileID);

        if(this.gameData == null){
            NewGame();
        }
        
        if(this.gameData == null){
            return;
        }

        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects){
            dataPersistenceObj.LoadData(gameData);
        }
    }
    public void SaveGame(){
        if(this.gameData == null){
            Debug.LogWarning("Klaida: Nebuvo rasti duomenys, kuriuos galėtume išsaugoti");
            return;
        }

        gameData.lastUpdated = System.DateTime.Now.ToBinary();

        fileDataHandler.Save(gameData, selectedProfileID);
    }
    private void OnApplicationQuit(){
            SaveGame();
    }
    private List<IDataPersistence> FindAllDataPersistenceObjects(){
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects); 
    }
    public void UpdateCurrentLevel(string levelName){
        if(gameData != null){
            gameData.currentLevel = levelName;
        }
    }
    public GameData GetGameData(){
        return gameData;
    }
    public bool HasGameData(){
        return gameData != null;
    }
    public Dictionary<string, GameData> GetAllProfielsGameData(){
        return fileDataHandler.LoadAllProfiles();
    }
}
