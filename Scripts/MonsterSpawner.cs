using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterSpawner : NetworkBehaviour{
    [SerializeField] private MonsterListSO monsterListSO;
    [SerializeField] private Transform[] spawnPoints;

    void Start(){
        if (IsServer){
            InvokeRepeating("SpawnMonster", 0f, Random.Range(6f, 10f));
        }
    }
    public void SpawnMonster(){
        // if there are any monsters in the list
        if (monsterListSO.monsterSOList.Count > 0){
            MonsterSO monsterToSpawn = monsterListSO.monsterSOList[Random.Range(0, monsterListSO.monsterSOList.Count)];

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            var monsterInstantiation = Instantiate(monsterToSpawn.prefab, spawnPoint.position, spawnPoint.rotation);
            monsterInstantiation.GetComponent<NetworkObject>().Spawn();
        }
        else{
            Debug.Log("chek MonsterListSO");
        }
    }
}