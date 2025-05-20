using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ImmuneFireMonster : Monster
{
    private float attackCooldown = 1.5f; 
    private float attackTimer = 0f;

    private Player player;

    private new void Start(){
        base.Start();
    }

    protected override void Update(){
        base.Update();
        
        if(!IsFrozen()){
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown) {
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
        agent.speed = monsterData.speed;
        player = FindObjectOfType<Player>();

        if(player != null && agent.enabled){
            agent.SetDestination(player.transform.position);
        }
    }
    protected override void ProcessDamage(int damage, string sourceTag = null)
    {
        if (sourceTag == Fireball.FireballTag)
        {
            Debug.Log("nedaro zalos ugnis");
            return;
        }
        base.ProcessDamage(damage, sourceTag);
    }
}
