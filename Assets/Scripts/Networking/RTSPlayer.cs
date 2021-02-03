using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    private List<Unit> myUnits = new List<Unit>();
	private List<Building> myBuildings = new List<Building>();

	public List<Unit> GetMyUnits() { return myUnits; }
	public List<Building> GetMyBuildings() { return myBuildings; }

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();

		// Subscribing to events and hooking them to local methods
		Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
		Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
		Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
		Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
	}

	public override void OnStopServer()
	{
		base.OnStopServer();

		// unsubscribing to events and removing hooks
		Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
		Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
		Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
		Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
	}

	[Server]
	private void ServerHandleUnitSpawned(Unit unit)
	{
		// check that unit is owned by this player (by checking connection ID)
		if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
			return;
		myUnits.Add(unit);
	}
	[Server]
	private void ServerHandleUnitDespawned(Unit unit)
	{
		// check that unit is owned by this player (by checking connection ID)
		if (unit.connectionToClient.connectionId != connectionToClient.connectionId)
			return;
		myUnits.Remove(unit);
	}
	[Server]
	private void ServerHandleBuildingSpawned(Building building)
	{
		if (building.connectionToClient.connectionId != connectionToClient.connectionId)
			return;
		myBuildings.Add(building);
	}
	[Server]
	private void ServerHandleBuildingDespawned(Building building)
	{
		if (building.connectionToClient.connectionId != connectionToClient.connectionId)
			return;
		myBuildings.Remove(building);
	}


	#endregion

	#region Client

	public override void OnStartAuthority()
	{
		base.OnStartAuthority();
		if (NetworkServer.active)
			return;
		Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
		Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
		Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
		Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
	}

	public override void OnStopClient()
	{
		base.OnStopClient();
		if (!isClientOnly || !hasAuthority)
			return;
		Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
		Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
		Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
		Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
	}

	[Client]
	private void AuthorityHandleUnitSpawned(Unit unit)
	{
		myUnits.Add(unit);
	}
	[Client]
	private void AuthorityHandleUnitDespawned(Unit unit)
	{
		myUnits.Remove(unit);
	}
	[Client]
	private void AuthorityHandleBuildingSpawned(Building building)
	{
		myBuildings.Add(building);
	}
	[Client]
	private void AuthorityHandleBuildingDespawned(Building building)
	{
		myBuildings.Add(building);
	}

	#endregion
}
