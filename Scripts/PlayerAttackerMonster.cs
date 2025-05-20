using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAttackerMonster : Monster{
    private float attackCooldown = 1.5f;
    private float attackTimer = 0f;

    private Player player;

    private new void Start(){
        base.Start();
    }

    protected override void Update(){
        base.Update();

        if (!IsFrozen())
        { 
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                Act();
                attackTimer = 0f;
            }
            MoveTowardsPlayer();
        }
        
    }
    public override void Act(){
        Player player = FindObjectOfType<Player>();
        if(player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if(distanceToPlayer <= monsterData.range)
            {
                player.TakeDamage(monsterData.damage);
            }
        }    
    }
    private void MoveTowardsPlayer(){    
        player = FindObjectOfType<Player>();

        if(agent.enabled && agent.isOnNavMesh && player != null){
            GetComponent<NavMeshAgent>().speed = monsterData.speed;
            GetComponent<NavMeshAgent>().SetDestination(player.transform.position);
        }
    }
}
