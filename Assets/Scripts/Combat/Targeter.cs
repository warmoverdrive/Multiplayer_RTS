using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
	private Targetable target = null;

	public Targetable GetTarget() { return target; }

	#region Server

	public override void OnStartServer()
	{
		GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
	}

	public override void OnStopServer()
	{
		GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
	}

	[Command]
    public void CmdSetTarget(GameObject targetGameObject)
	{
		if (!targetGameObject.TryGetComponent(out Targetable newTarget))
			return;

		target = newTarget;
	}

	[Server]
	public void ClearTarget()
	{
		target = null;
	}

	[Server]
	private void ServerHandleGameOver()
	{
		ClearTarget();
	}

	#endregion
}
