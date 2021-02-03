using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
	[SerializeField] private Health health = null;
	[SerializeField] private Unit unitPrefab = null;
	[SerializeField] private Transform unitSpawnPoint = null;
	[SerializeField] private TMP_Text remainingUnitsText = null;
	[SerializeField] private Image unitProgressImage = null;
	[SerializeField] private float maxUnitQueue = 5;
	[SerializeField] private float spawnMoveRange = 7;
	[SerializeField] private float unitSpawnDuration = 5f;

	private float progressImageVelocity;

	[SyncVar (hook = nameof(ClientHandleQueuedUnitsUpdated))]
	private int queuedUnits;
	[SyncVar]
	private float unitTimer;

	private void Update()
	{
		if (isServer)
		{
			ProduceUnits();
		}
		if (isClient)
		{
			UpdateTimerDisplay();
		}
	}

	#region Server

	public override void OnStartServer()
	{
		base.OnStartServer();
		health.ServerOnDie += ServerHandleDie;
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		health.ServerOnDie -= ServerHandleDie;
	}

	[Server]
	private void ProduceUnits()
	{
		if (queuedUnits == 0)
			return;

		unitTimer += Time.deltaTime;
		if (unitTimer < unitSpawnDuration)
			return;

		GameObject unitInstance = Instantiate(
			unitPrefab.gameObject,
			unitSpawnPoint.position,
			unitSpawnPoint.rotation);

		NetworkServer.Spawn(unitInstance, connectionToClient);

		Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
		spawnOffset.y = unitSpawnPoint.position.y;

		UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
		unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

		queuedUnits--;
		unitTimer = 0;
	}

	[Server]
	private void ServerHandleDie()
	{
		NetworkServer.Destroy(gameObject);
	}

	[Command]
	private void CmdSpawnUnit()
	{
		if (queuedUnits == maxUnitQueue)
			return;

		RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

		if (player.GetResources() < unitPrefab.GetResourceCost())
			return;

		queuedUnits++;

		player.ModifyResources(-unitPrefab.GetResourceCost());
	}

	#endregion

	#region Client
	
	private void UpdateTimerDisplay()
	{
		float newProgress = unitTimer / unitSpawnDuration;

		if (newProgress < unitProgressImage.fillAmount)
			unitProgressImage.fillAmount = newProgress;
		else
			unitProgressImage.fillAmount = Mathf.SmoothDamp(
				unitProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
	}

	[Client]
	private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
	{
		remainingUnitsText.text = newUnits.ToString();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;
		if (!hasAuthority)
			return;

		CmdSpawnUnit();
	}

	#endregion

}
