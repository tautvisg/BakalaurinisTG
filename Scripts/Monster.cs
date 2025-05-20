using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public abstract class Monster : NetworkBehaviour{
    public MonsterSO monsterData; // reference to monsterSO which had data about mosnster

    private NetworkVariable<int> health = new NetworkVariable<int>();

    private float freezeTimer;

    private float slowTimer;
    private float originalSpeed;

    [SerializeField] private Material frozenMaterial;
    //[SerializeField] private Sprite npcSprite;

    protected SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    protected NavMeshAgent agent;

    protected void Start(){

        if (IsServer){
            health.Value = monsterData.health;
        }
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        if (spriteRenderer != null){
            originalMaterial = spriteRenderer.material;
        }

        originalSpeed = agent.speed;

    }
    protected virtual void Update(){

        if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
        }
        else
        {
            GetComponent<NavMeshAgent>().enabled = true;

            if (spriteRenderer != null){
                spriteRenderer.material = originalMaterial; // resets to original material after freeze is over
            }
        }

        if (slowTimer > 0){
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0)
            {
                agent.speed = originalSpeed;
            }
        }
        if (!IsServer){
            return;
        }

    }
    public abstract void Act();

    public void ApplyDamage(int damage, string sourceTag = null){
        if (IsServer){
            ProcessDamage(damage, sourceTag);
        }
        else{
            TakeDamageServerRpc(damage, sourceTag);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public virtual void TakeDamageServerRpc(int damage, string sourceTag = null, ServerRpcParams serverRpcParams = default){
        if (!IsServer){
            return;
        }
        ProcessDamage(damage, sourceTag);
    }
    protected virtual void ProcessDamage(int damage, string sourceTag){
        //Debug.Log("Monster health before taking damage: " + health.Value);
        health.Value -= damage;
        //Debug.Log("Monster health: " + health.Value);

        if (health.Value <= 0)
        {
            MonsterDieServerRpc();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    protected virtual void MonsterDieServerRpc(){
        if (!IsServer){
            return;
        }
        MonsterDieClientRpc();
        GetComponent<NetworkObject>().Despawn();
    }
    [ClientRpc]
    private void MonsterDieClientRpc(){
        Destroy(gameObject);
    }
    protected virtual void OnDie() // krepsius vagiantis monstras turetu ismesti krepsi
    {
    }


    [ServerRpc(RequireOwnership = false)]
    public void FreezeServerRpc(float duration){
        if (!IsServer)
        {
            return;
        }
        freezeTimer = duration;
        FreezeClientRpc(duration);
    }
    [ClientRpc]
    private void FreezeClientRpc(float duration, ClientRpcParams clientRpcParams = default){
        freezeTimer = duration;
        agent.enabled = false;
        if (spriteRenderer != null)
            spriteRenderer.material = frozenMaterial;
    }
    // Old singleplayer function
    public void Freeze(float duration){
        freezeTimer = duration;
        GetComponent<NavMeshAgent>().enabled = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.material = frozenMaterial; // sets monster material to blue
        }
    }
    public bool IsFrozen(){
        return freezeTimer > 0;
    }

    [ClientRpc]
    public void ApplyKnockbackClientRpc(Vector3 direction, float force){
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
            StartCoroutine(ReenableNav());
        }
    }
    // Old singleplayer function
    public void ApplyKnockback(Vector3 direction, float force){
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
            StartCoroutine(ReenableNav());
        }
    }

    private IEnumerator ReenableNav(){
        yield return new WaitForSeconds(0.5f);
        agent.enabled = true;
        agent.speed = originalSpeed;
    }
    [ClientRpc]
    public void ApplySlowClientRpc(float duration, float multiplier){
        agent.speed = originalSpeed * multiplier;
        ResetSpeedAfter(duration);
    }
    // old kodai
    public void ApplySlow(float duration, float multiplier){
        slowTimer = duration;
        agent.speed = originalSpeed * multiplier;
    }
    private IEnumerator ResetSpeedAfter(float duration){
        yield return new WaitForSeconds(duration);
        agent.speed = originalSpeed;
    }
}
