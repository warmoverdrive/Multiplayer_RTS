using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private List<Unit> myUnits = new List<Unit>();

	public List<Unit> GetMyUnits() { return myUnits; }

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();

		// Subscribing to events and hooking them to local methods
		Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
		Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
	}

	public override void OnStopServer()
	{
		base.OnStopServer();

		// unsubscribing to events and removing hooks
		Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
		Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
	}

	private void ServerHandleUnitSpawned(Unit unit)
	{
		// check that unit is owned by this player (by checking connection ID)
		if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
			return;
		myUnits.Add(unit);
	}

	private void ServerHandleUnitDespawned(Unit unit)
	{
		// check that unit is owned by this player (by checking connection ID)
		if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
			return;
		myUnits.Remove(unit);
	}

	#endregion

	#region Client

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!isClientOnly)
			return;
		Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
		Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		if (!isClientOnly)
			return;
		Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
		Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
	}

	private void AuthorityHandleUnitSpawned(Unit unit)
	{
		if (!hasAuthority)
			return;
		myUnits.Add(unit);
	}

	private void AuthorityHandleUnitDespawned(Unit unit)
	{
		if (!hasAuthority)
			return;
		myUnits.Remove(unit);
	}

	#endregion
}
