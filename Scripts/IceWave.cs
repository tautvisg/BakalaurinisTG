using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class IceWave : Ability
{
    private Vector3 startPosition; // So we can delete the icewave after some distance traveled
    private float distanceTraveled = 0;
    private float freezeDuration = 5.0f;

    public override void OnCollisionEnter(Collision collision)
    {
        if (IsServer && NetworkObject.IsSpawned)
        {
            DespawnServerRpc();
        }
    }
    protected override void OnTriggerEnter(Collider other)
    {
        Debug.Log("IceWave.cs onTrigerEntered");

        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monster.FreezeServerRpc(freezeDuration);
            monster.ApplyDamage(abilityData.damage, gameObject.tag);
            if (IsServer && NetworkObject.IsSpawned)
            {
                DespawnServerRpc();
            }
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