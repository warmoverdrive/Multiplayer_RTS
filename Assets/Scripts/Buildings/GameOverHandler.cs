using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
	private List<UnitBase> bases = new List<UnitBase>();

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();

		UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
		UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
	}

	public override void OnStopServer()
	{
		base.OnStopServer();

		UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
		UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
	}

	[Server]
	private void ServerHandleBaseSpawned(UnitBase unitBase)
	{
		bases.Add(unitBase);
	}

	[Server]
	private void ServerHandleBaseDespawned(UnitBase unitBase)
	{
		bases.Remove(unitBase);

		if (bases.Count != 1)
			return;
		else
			Debug.Log("Game over");
	}

	#endregion

	#region Client

	#endregion
}
