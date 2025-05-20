using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class GameData {
    public long lastUpdated; 
    public string currentLevel;
  

    public GameData(){
        this.deliveriesMade = 0;
        this.currentLevel = "Level1_1";
    }
    public string GetCurrentLevel(){
        return currentLevel;
    }
    public string GetLastPlayedDate(){
        DateTime lastPlayedDate = DateTime.FromBinary(lastUpdated);
        string lastPlayedDateString = lastPlayedDate.ToString("yyyy-MM-dd HH:mm");
        return lastPlayedDateString;
    }
}
