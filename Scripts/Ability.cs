using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Ability : NetworkBehaviour{
    public AbilitySO abilityData;

    private float cooldownTimer;
    protected bool isOnCooldown;



    public virtual void Activate(Vector3 targetPosition){
        // base activate
        cooldownTimer = abilityData.cooldown;

    }

    public virtual void OnCollisionEnter(Collision collision){
        // destroys ability object on colliding or does something else (specification in overriden classes)
    }
    // check if collider is trigger for monster to take damage
    protected virtual void OnTriggerEnter(Collider other){
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monster.ApplyDamage(abilityData.damage, gameObject.tag);
        }
    }

    protected virtual void Update(){
        cooldownTimer = Mathf.Max(0, cooldownTimer - Time.deltaTime);

        isOnCooldown = cooldownTimer > 0;
    }
    public bool IsOnCooldown(){
        return isOnCooldown;
    }
    public float GetCooldownProgress(){
        Debug.Log(cooldownTimer / abilityData.cooldown);
        return cooldownTimer / abilityData.cooldown;
    }
}