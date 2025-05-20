using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WindAttack : Ability{
    private Vector3 startPosition;
    private float distanceTraveled = 0;

    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float slowMultiplier = 0.5f;

    public override void OnCollisionEnter(Collision collision)
    {
        if (IsServer && NetworkObject.IsSpawned)
        {
            DespawnServerRpc();
        }
    }
    protected override void OnTriggerEnter(Collider other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monster.ApplyDamage(abilityData.damage, gameObject.tag);
            Vector3 knockbackDirection = (monster.transform.position - transform.position).normalized;
            monster.ApplyKnockbackClientRpc(knockbackDirection, knockbackForce);
            monster.ApplySlowClientRpc(slowDuration, slowMultiplier);
        }
        if (IsServer && NetworkObject.IsSpawned)
        {
            DespawnServerRpc();
        }

    }
    public override void Activate(Vector3 targetPosition)
    {
        base.Activate(targetPosition);

        Vector3 direction = (targetPosition - transform.position).normalized;

        direction.y = 0;

        GetComponent<Rigidbody>().velocity = direction * abilityData.speed;
        startPosition = transform.position;
    }
    protected override void Update()
    {
        base.Update();

        distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= abilityData.range)
        {
            if (IsServer && NetworkObject.IsSpawned)
            {
                DespawnServerRpc();
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
        Destroy(gameObject);
    }
}
