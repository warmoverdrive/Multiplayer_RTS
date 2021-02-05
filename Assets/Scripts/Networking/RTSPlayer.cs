using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
	[SerializeField] private Transform cameraTransform = null;
	[SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
	[SerializeField] private Building[] buildings = new Building[0];
	[SerializeField] private float buildingRangeLimit = 5f;

	[SerializeField]
	[SyncVar (hook = nameof(ClientHandleResourcesUpdated))]
	private int resources = 500;
	[SyncVar (hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
	private bool isPartyOwner = false;
	[SyncVar (hook = nameof(ClientHandleDisplayNameUpdated))]
	private string displayName;

	public event Action<int> ClientOnResourcesUpdated;

	public static event Action ClientOnInfoUpdated;
	public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

	private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
	[SerializeField] private List<Building> myBuildings = new List<Building>();

	public string GetDisplayName() { return displayName; }
	public bool GetIsPartOwner() { return isPartyOwner; }
	public Transform GetCameraTransform() { return cameraTransform; }
	public Color GetTeamColor() { return teamColor; }
	public int GetResources() { return resources; }
	public List<Unit> GetMyUnits() { return myUnits; }
	public List<Building> GetMyBuildings() { return myBuildings; }

	public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 position)
	{
		if (Physics.CheckBox(
			position + buildingCollider.center, buildingCollider.size / 2,
			Quaternion.identity, buildingBlockLayer))
			return false;

		foreach (var building in myBuildings)
		{
			if ((position - building.transform.position).sqrMagnitude <=
				buildingRangeLimit * buildingRangeLimit)
			{
				return true;
			}
		}

		return false;
	}

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();

		// Subscribing to events and hooking them to local methods
		Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
		Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
		Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
		Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

		DontDestroyOnLoad(gameObject);
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
	public void SetDisplayName(string newDisplayName) { displayName = newDisplayName; }
	[Server]
	public void SetPartyOwner(bool state) { isPartyOwner = state; }
	[Server]
	public void SetTeamColor(Color newTeamColor) { teamColor = newTeamColor; }
	[Server]
	public void ModifyResources(int changeInResources) { resources += changeInResources; }

	[Command]
	public void CmdStartGame()
	{
		if (!isPartyOwner)
			return;

		((RTSNetworkManager)NetworkManager.singleton).StartGame();
	}

	[Command]
	public void CmdTryPlaceBuilding(int buildingID, Vector3 position)
	{
		Building buildingToPlace = null;

		foreach (var building in buildings)
		{
			if (building.GetID() == buildingID)
			{
				buildingToPlace = building;
				break;
			}
		}

		if (buildingToPlace == null)
			return;

		if (resources < buildingToPlace.GetPrice())
			return;

		BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

		if (!CanPlaceBuilding(buildingCollider, position))
			return;

		GameObject buildingInstance = Instantiate(
			buildingToPlace.gameObject, position, buildingToPlace.transform.rotation);

		NetworkServer.Spawn(buildingInstance, connectionToClient);

		ModifyResources(-buildingToPlace.GetPrice());
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

	public override void OnStartClient()
	{
		if (NetworkServer.active)
			return;

		DontDestroyOnLoad(gameObject);

		((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
	}

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
		ClientOnInfoUpdated?.Invoke();

		if (!isClientOnly)
			return;

		((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

		if (!hasAuthority)
			return;

		Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
		Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
		Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
		Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
	}

	[Client]
	private void ClientHandleResourcesUpdated(int oldResources, int newResources)
	{
		ClientOnResourcesUpdated?.Invoke(newResources);
	}

	private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
	{
		ClientOnInfoUpdated?.Invoke();
	}

	private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
	{
		if (!hasAuthority)
			return;
		AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
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
