using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
	[SerializeField] private GameObject unitSpawnerPrefab = null;
	[SerializeField] private GameObject gameOverHandlerPrefab = null;

	public override void OnServerAddPlayer(NetworkConnection conn)
	{
		base.OnServerAddPlayer(conn);

		RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

		player.SetTeamColor(new Color(Random.value, Random.value, Random.value));

		GameObject unitSpawnerInstance = Instantiate(
			unitSpawnerPrefab, 
			conn.identity.transform.position, 
			conn.identity.transform.rotation);

		NetworkServer.Spawn(unitSpawnerInstance, conn);
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
		{
			var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

			NetworkServer.Spawn(gameOverHandlerInstance);
		}
	}
}
