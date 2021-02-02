using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    [ServerCallback]
	private void Update()
	{
        Targetable target = targeter.GetTarget();

        if (target == null)
            return;

		if (!CanFireAtTarget(target))
			return;

        // get the target rotation relative to us
        Quaternion targetRotation = Quaternion.LookRotation(
            target.transform.position - transform.position);
        // rotate us towards the target
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate) + lastFireTime)
		{
            // get the rotation from the projectile spawner towards the target AimAtPoint
            Quaternion projectileRotation = Quaternion.LookRotation(
                target.GetAimAtPoint().position - projectileSpawnPoint.position);
            // instantiate the prefab on the server
            GameObject projectileInstance = Instantiate(
                projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            // spawn projectile over the network and give ownership to this unit's owner
            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
		}
	}

    [Server]
    private bool CanFireAtTarget(Targetable target)
	{
        return (target.transform.position - transform.position).sqrMagnitude 
            < fireRange * fireRange;
    }
}
