using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class RTSNetworkManager : NetworkManager
{
	[SerializeField] private GameObject unitBasePrefab = null;
	[SerializeField] private GameObject gameOverHandlerPrefab = null;

	public static event Action ClientOnConnected;
	public static event Action ClientOnDisconnected;

	private bool isGameInProgress = false;
	public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

	#region Server

	public override void OnServerConnect(NetworkConnection conn)
	{
		if (!isGameInProgress)
			return;
		conn.Disconnect();
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		var player = conn.identity.GetComponent<RTSPlayer>();

		Players.Remove(player);
	}

	public override void OnStopServer()
	{
		Players.Clear();
		isGameInProgress = false;
	}

	public void StartGame()
	{
		if (Players.Count < 2)
			return;

		isGameInProgress = true;

		ServerChangeScene("Scene_Map_01");
	}

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		base.OnServerAddPlayer(conn);

		RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

		Players.Add(player);

		player.SetDisplayName($"Player {Players.Count}");

		player.SetTeamColor(new Color(
			UnityEngine.Random.value,
			UnityEngine.Random.value,
			UnityEngine.Random.value));

		// sets party owner to player connected first (host)
		player.SetPartyOwner(Players.Count == 1);
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
		{
			var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

			NetworkServer.Spawn(gameOverHandlerInstance);

			foreach (var player in Players)
			{
				var baseInstance = Instantiate(
					unitBasePrefab, GetStartPosition().position, Quaternion.identity);
				NetworkServer.Spawn(baseInstance, player.connectionToClient);
			}

		}
	}

	#endregion

	#region Client

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		ClientOnConnected?.Invoke();
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);

		ClientOnDisconnected?.Invoke();
	}

	public override void OnStopClient()
	{
		Players.Clear();
	}

	#endregion





}
