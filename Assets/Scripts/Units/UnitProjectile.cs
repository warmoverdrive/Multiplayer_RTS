using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;

	private void Start()
	{
		rb.velocity = transform.forward * launchForce;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		Invoke(nameof(DestroySelf), destroyAfterSeconds);
	}

	[Server]
	private void DestroySelf()
	{
		NetworkServer.Destroy(gameObject);
	}
}
