using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Netcode;

public class Fireball : Ability{
    // flamethrower now, not fireball
    public static readonly string FireballTag = "Fireball";

    private float damageTimer = 0f;
    private float damageInterval = 0.5f; // interval for damage application
    private float flamethrowerDuration = 5f; 

    private bool isActive = false;

    [SerializeField] private VisualEffect flamethrowerVFX;


    private void Start(){
        damageTimer = damageInterval;

        if (flamethrowerVFX == null)
        {
            Debug.LogError("VFX nera!");
        }
    }
    protected override void Update(){
        base.Update();
        if (isActive && IsClient && OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            var player = Player.LocalInstance;
            transform.position = player.GetHoldingPointPos();
            transform.rotation = player.transform.rotation;
        }
    }
    public override void OnCollisionEnter(Collision collision){
        //Destroy(gameObject); senesneje dalyje dingdavo ant susidurimo
    }

    [ClientRpc]
    public void ActivateClientRpc(){
        isActive = true;
        if (flamethrowerVFX != null)
        {
            flamethrowerVFX.Play();
        }
        if (IsServer)
            StartCoroutine(FlamethrowerRoutine());
    }
    [ServerRpc(RequireOwnership = false)]
    public void DeactivateServerRpc(ServerRpcParams rpcParams = default){
        StopClientRpc();
        StartCoroutine(DelayedDespawn());
    }
    private IEnumerator DelayedDespawn(){
        yield return new WaitForSeconds(2f);
        if (NetworkObject != null && NetworkObject.IsSpawned){
            NetworkObject.Despawn(true);
        }
    }
    [ClientRpc]
    private void StopClientRpc(){
        isActive = false;
        flamethrowerVFX.Stop();
    }
    // singleplayer buves
    public override void Activate(Vector3 targetPosition){
        base.Activate(targetPosition);
        isActive = true;

        StartCoroutine(FlamethrowerRoutine());

    }
    private IEnumerator FlamethrowerRoutine(){
        float elapsedTime = 0f;

        while (elapsedTime < flamethrowerDuration && isActive){
            elapsedTime += Time.deltaTime;
            damageTimer -= Time.deltaTime;

            if(damageTimer <= 0f){
                ApplyDamage();
                damageTimer = damageInterval;                
            }

            yield return null;
        }
        isActive = false;
        if (flamethrowerVFX != null){
            flamethrowerVFX.Stop();
        }
        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

    private void ApplyDamage(){
        if (!IsServer) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, abilityData.range);
        foreach(var hitCollider in hitColliders){
            Monster monster = hitCollider.GetComponent<Monster>();
            if (monster != null){
                monster.ApplyDamage(abilityData.damage, gameObject.tag);
            }
        }        
    }
}