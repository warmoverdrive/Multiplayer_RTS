using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
	[SerializeField] Targetable target = null;

	#region Server

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

	#endregion

	#region Client

	#endregion
}
