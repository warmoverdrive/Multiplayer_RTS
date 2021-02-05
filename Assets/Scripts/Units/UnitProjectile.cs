using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;

	private int damageToDeal = 0;

	public void SetDamageToDeal(int damage) { damageToDeal = damage; }

	private void Start()
	{
		rb.velocity = transform.forward * launchForce;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		Invoke(nameof(DestroySelf), destroyAfterSeconds);
	}

	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		if(other.TryGetComponent(out NetworkIdentity networkIdentity))
		{
			if (networkIdentity.connectionToClient == connectionToClient)
				return;
		}

		if (other.TryGetComponent(out Health health))
		{
			health.DealDamage(damageToDeal);
		}

		DestroySelf();
	}

	[Server]
	private void DestroySelf()
	{
		NetworkServer.Destroy(gameObject);
	}
}
