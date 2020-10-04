using System.Collections.Generic;
using GameServer;
using Pathfinding;
using MEC;

public enum AttackStyle
{
	Normal,
	Press,
	Charge
}

public class CommanderScript : MonoBehaviour
{

	public bool attackGrid;
	public bool formationHasToStayInLine;
	public bool hasToWalk = false;
	public int formationId;
	public int childReachedPositionCount;
	public float formationRadius;
	public float minAttackRange = 0f;
	public AttackStyle attackStyleAtMoment = 0;
	public NormalComponentsObject formationObject;
	//public NormalComponentsObject circleRenderer;
	public List<Transform> attackTroops = new List<Transform>();
	public List<TroopComponents> controlledTroops = new List<TroopComponents>();
	public TroopComponents troopObject;

	protected Vector3 positonToWalkTo;
	protected PlayerController playerController;
	protected AttackingSystem attackingSystem;
	protected RichAI richAI;
	protected Seeker seeker;

	const float distanceFormPointToArmy = 15f;

	bool hasToCheckDistance = false;
	float agentRadius;
	float factorCircleSide;
	Vector3 middlePoint;
	Vector3 movement;

	public override void Start(TroopComponents _troopObject)
    {
		troopObject = _troopObject;
		attackGrid = false;
		childReachedPositionCount = 0;
		playerController = troopObject.playerController;
		attackingSystem = troopObject.attackingSystem;
		richAI = troopObject.richAI;
		seeker = troopObject.seeker;
		//circleRenderer = GameObject.Find("LineRendererForCommanderAttack");
    }

	/// <summary>
	/// wenn eine Truppe im attackGridist wird hier childReachedPositionCount++ gemacht.Wenn jetzt alle Truppenin der Armee die Position erreicht haben, kann der Commander weiter laufen
	/// </summary>
	public void childhasReachedGridPoint(int troopId, Vector3 position, int clientId)
	{
		childReachedPositionCount++;
		if (childReachedPositionCount == controlledTroops.Count)
		{
			Debug.Log("All Troops Arrived");
			richAI.radius = agentRadius + 1f;
			//if(GetComponent<PlayerController>().currentState  != STATE.Following)
			//	GetComponent<PlayerController>().richAI.endReachedDistance = Mathf.CeilToInt(agentRadius - 2f);
			seeker.traversableTags = GetAgentType(agentRadius + 1);
			if(playerController.commanderIsTurning)
			{
				playerController.ResumeCommanderWalk();
			}
		}
	}

	public int GetAgentType(float radius)
	{
		//Debug.Log("Set Other Agent");
		//int Intradius = Mathf.CeilToInt(radius + 0.5f);
		//Intradius -= 1;
		//Intradius = Mathf.Clamp(Intradius, 0, 3);
		//return 1 << Intradius;
		//return 1 << 0;
		return ~0;
	}

	public void SetFormation()
	{
		/*if (playerController.currentState == STATE.Following)
			StopAttack();*/

		float radius = FormationManager.SetFormation(formationId, controlledTroops, troopObject, playerController.clientId, formationHasToStayInLine);
		SetAgentRadius(radius);
	}

	public void SetFormation(int _formationId, int attackStyle)
	{
		if (playerController.currentState == STATE.Following)
			StopAttack();

		attackStyleAtMoment = (AttackStyle)attackStyle;
		float radius = FormationManager.SetFormation(_formationId, controlledTroops, troopObject, playerController.clientId, formationHasToStayInLine);
		SetAgentRadius(radius);

	}

	public void prepareForFormation()
	{
		richAI.radius = 0.5f;
		foreach (TroopComponents troop in controlledTroops)
		{
			troop.transform.parent = troopObject.transform.parent;
			troop.playerController.transformOnAttackGrid = null;
		}
	}

	public void SetAgentRadius(float radius)
	{
		agentRadius = radius;
	}

	/// <summary>
	///wenn ein Gegener Trupp gefunden ist, dann auf allen Kontrollierten Truppen die Enemy Trupp setzten
	/// </summary>
	public virtual void BeginAttackInFormation(TroopComponents nearetsObject)
	{
		for(int i = 0; i<controlledTroops.Count; i++)
		{
			controlledTroops[i].attackingSystem.enemyAttackPlayer = nearetsObject;
		}
		SetAttackInForm(nearetsObject, false);
	}

	/// <summary>
	///Wenn der Commander in der Formation auf die anderen Truppen AttackForm trifft, dann muss die Formation richtig angreifen
	/// </summary>
	public virtual void SetAttackInForm(TroopComponents enemyAttackPlayer, bool hasMadeTurn)
	{
		bool attackOtherAttackGrid = false;
		TroopComponents otherCommander = null;
		//check ob der Attackierte in Formation steht, weil dann treffen zwei Formation aufeinander
		//if(enemyAttackPlayer.GetComponent<CommanderScript>() == null)
		//{
		//	if (enemyAttackPlayer.GetComponent<PlayerController>().currentState == STATE.attackGrid)
		//	{
		//		attackOtherAttackGrid = true;
		//		otherCommander = attackingSystem.enemyAttackPlayer.transform.parent;
		//	}
		//}
		//else
		//{
		//	if (enemyAttackPlayer.GetComponent<CommanderScript>().attackGrid)
		//	{
		//		attackOtherAttackGrid = true;
		//		otherCommander = attackingSystem.enemyAttackPlayer.transform;
		//	}
		//}
		if (enemyAttackPlayer.playerController.Mycommander != null)
		{
			otherCommander = enemyAttackPlayer.playerController.Mycommander;
			attackOtherAttackGrid = true;
		}
		//wenn eine andere AttackForm angegriffen wird
		if (attackOtherAttackGrid)
		{
			//wenn normal angegriffen wird
			if (attackStyleAtMoment == AttackStyle.Normal)
				AttackNormalOtherFormation(enemyAttackPlayer, otherCommander, hasMadeTurn);
			//wenn in die Andere Formation rein gelaufen wird
			else if (attackStyleAtMoment == AttackStyle.Charge)
				AttackChargeOtherFormation(enemyAttackPlayer, otherCommander, hasMadeTurn);

		}
		//wenn andere Truppen nicht in Formation angegriffen werden
		else
		{
			if (attackStyleAtMoment == AttackStyle.Normal)
				AttackNormalOtherTroops(enemyAttackPlayer);

			else if (attackStyleAtMoment == AttackStyle.Charge)
				AttackChargeOtherTroops(enemyAttackPlayer, hasMadeTurn);
		}
	}

	#region AttackSyles
	protected virtual void AttackNormalOtherFormation(TroopComponents enemyAttackPlayer, TroopComponents otherCommander, bool hasMadeTurn)
	{
		enemyAttackPlayer = otherCommander;
		enemyAttackPlayer.commanderScript.GettingAttacked(attackStyleAtMoment);
		float preRadius = richAI.radius;
		richAI.radius = 0.5f;
		seeker.traversableTags = GetAgentType(0f);
		playerController.currentState = STATE.Following;
		attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		if (hasMadeTurn)
		{
			//ServerSend.troopMove(true, playerController.clientId, playerController.troopId, transform.position, Vector3.zero, 1f, true, false);
			//richAI.endReachedDistance = Server.clients[playerController.clientId].player.placedTroops[playerController.troopId].troop.attackRadius;
			//playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
			ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
			SetLineAttack();
		}
		else
		{
			//TODO try to attack without circle walk --> prevent backwards Circle
			if ((troopObject.transform.position - enemyAttackPlayer.transform.position).sqrMagnitude <= distanceFormPointToArmy * distanceFormPointToArmy)
				Debug.Log("Is nearer");
			//der Commander wenn der angreifft läuft einen Bogen um in einem richtigen Winkel auf die Truppe zu treffen. Diese Winkel sind bie Formationen definiert
			positonToWalkTo = GetPointBeforeFormation(enemyAttackPlayer, preRadius);
			middlePoint = getCirlceMiddlePoint(enemyAttackPlayer.transform);
			playerController.circleMiddlePoint = middlePoint;
			//circleRenderer.transform.position = middlePoint;
			//circleRenderer.GetComponent<LineRendererCircle>().radius = Vector3.Distance(transform.position, middlePoint);
			//circleRenderer.GetComponent<LineRendererCircle>().CreatePoints();
			movement = middlePoint - positonToWalkTo;
			movement = new Vector3(movement.z, 0, -movement.x);
			float dot = Vector3.Dot(movement, positonToWalkTo - enemyAttackPlayer.transform.position);
			if (dot < 0f)
				factorCircleSide = 1f;
			else
				factorCircleSide = -1f;

			playerController.factorCircleSide = factorCircleSide;
			movement = middlePoint - troopObject.transform.position;
			movement = new Vector3(movement.z, 0, -movement.x) * factorCircleSide;
			if (Vector3.Angle(troopObject.transform.forward, movement) > 16f)
			{
				hasToWalk = false;
				playerController.circleWalk = false;
				playerController.StartTruning(movement.normalized);
			}
			else
			{
				playerController.currentState = STATE.Following;
				hasToWalk = true;
				playerController.circleWalk = true;
			}
			//richAI.endReachedDistance = 0.9f;
			Timing.RunCoroutine(CheckForDistanceCoroutine());
		}
	}

	void AttackChargeOtherFormation(TroopComponents enemyAttackPlayer, TroopComponents otherCommander, bool hasMadeTurn)
	{
		enemyAttackPlayer = otherCommander;
		playerController.currentState = STATE.Following;
		attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		if (!hasMadeTurn)
		{
			positonToWalkTo = enemyAttackPlayer.transform.position + ((troopObject.transform.position - enemyAttackPlayer.transform.position).normalized * ((formationRadius / 2) + (enemyAttackPlayer.commanderScript.formationRadius / 2) + 8f));
			//richAI.endReachedDistance = 1.3f;
			playerController.MoveToPosition(positonToWalkTo, false);
			Timing.RunCoroutine(CheckForDistanceCoroutine());
		}
		else
		{
			enemyAttackPlayer.commanderScript.GettingAttacked(attackStyleAtMoment);
			float preRadius = richAI.radius;
			richAI.radius = 0.5f;
			seeker.traversableTags = GetAgentType(0);
			for (int i = 0; i < controlledTroops.Count; i++)
			{
				PlayerController controlledTroopPlayerController = controlledTroops[i].playerController;
				controlledTroopPlayerController.richAI.enabled = true;
				controlledTroopPlayerController.currentState = STATE.Following;
				controlledTroopPlayerController.MoveToPosition(enemyAttackPlayer.transform.position + (controlledTroops[i].transform.position - troopObject.transform.position), false);
				controlledTroops[i].attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
				//controlledTroopPlayerController.RVOController.priority = 0.4f;
			}
			playerController.currentState = STATE.Following;
			playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
			attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
			//playerController.RVOController.priority = 0.4f;
			ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
			troopObject.playerController.ignoreCommanderTurning = true;
		}
	}

	private void AttackNormalOtherTroops(TroopComponents enemyAttackPlayer)
	{
		richAI.radius = 0.5f;
		seeker.traversableTags = GetAgentType(0);
		playerController.currentState = STATE.Following;
		attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
		//playerController.ignoreCommanderTurning = true;
		//Collider[] coliders = Physics.OverlapSphere(enemyAttackPlayer.position, 5f, LayerMask.GetMask("Player"));
		ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
		for (int i = 0; i < controlledTroops.Count; i++)
		{
			controlledTroops[i].attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
			controlledTroops[i].attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		}
	}

	private void AttackChargeOtherTroops(TroopComponents enemyAttackPlayer, bool hasMadeTurn)
	{
		playerController.currentState = STATE.Following;
		attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		if (!hasMadeTurn)
		{
			positonToWalkTo = enemyAttackPlayer.transform.position + ((troopObject.transform.position - enemyAttackPlayer.transform.position).normalized * ((formationRadius / 2) + (enemyAttackPlayer.commanderScript.formationRadius / 2) + distanceFormPointToArmy));
			//richAI.endReachedDistance = 1.3f;
			playerController.MoveToPosition(positonToWalkTo, false);
			Timing.RunCoroutine(CheckForDistanceCoroutine());
		}
		else
		{
			richAI.radius = 0.5f;
			seeker.traversableTags = GetAgentType(0);
			playerController.currentState = STATE.Following;
			attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
			playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
			attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
			ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
			//Collider[] colliders = Physics.OverlapSphere(enemyAttackPlayer.position, AttackingSystem.attackSearchRange, LayerMask.GetMask("Player"));
			TroopComponents[] colliders = new TroopComponents[2];
			int y = 0;
			for (int i = 0; i < controlledTroops.Count; i++)
			{
				PlayerController controlledTroopPlayerController = controlledTroops[i].playerController;
				controlledTroopPlayerController.richAI.enabled = true;
				controlledTroopPlayerController.currentState = STATE.Following;
				//controlledTroopPlayerController.RVOController.priority = 0.4f;
				if (colliders.Length > i)
				{
					controlledTroopPlayerController.MoveToPosition(colliders[i].transform.position, false);
					controlledTroopPlayerController.troopObject.attackingSystem.enemyAttackPlayer = colliders[i];
				}
				else
				{
					if (y > colliders.Length - 1)
						y = -1;
					y++;
					controlledTroopPlayerController.MoveToPosition(colliders[colliders.Length - y].transform.position, false);
					controlledTroopPlayerController.troopObject.attackingSystem.enemyAttackPlayer = colliders[colliders.Length - y];
				}
				controlledTroops[i].attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
			}
		}
	}
	#endregion

	public void StopAttack()
	{
		Debug.Log("Stop Attack");
		playerController.currentState = STATE.Idle;
		playerController.ignoreCommanderTurning = false;
		attackingSystem.StopRepeat();
		//playerController.RVOController.priority = 0.5f;
		playerController.circleWalk = false;
		hasToWalk = false;
		hasToCheckDistance = false;
		ServerSend.StartFight(playerController.clientId, playerController.troopId, false, true);
		//hasToWalk = false;
		for(int i =0; i < controlledTroops.Count; i++)
		{
			controlledTroops[i].transform.parent = troopObject.transform;
			//controlledTroops[i].GetComponent<PlayerController>().RVOController.priority = 0.5f;
			controlledTroops[i].playerController.currentState = STATE.attackGrid;
			controlledTroops[i].attackingSystem.StopRepeat();
		}
	}

	/// <summary>
	///Wenn ein Commander angegriffen wurde
	/// </summary>
	public void GettingAttacked(AttackStyle otherAttackStyle)
	{
		richAI.radius = 0.5f;
		seeker.traversableTags = GetAgentType(0);
		for (int i = 0; i < controlledTroops.Count; i++)
		{
			controlledTroops[i].richAI.enabled = true;
		}
	}


	/// <summary>
	///schaut all 0.2 Sekunden ob der Punkt vor der Formation erreicht wurde
	/// </summary>
	protected virtual IEnumerator<float> CheckForDistanceCoroutine()
	{
		hasToCheckDistance = true;
		while (hasToCheckDistance)
		{
			if ((troopObject.transform.position - positonToWalkTo).sqrMagnitude < 0.5f * 0.5f)
			{
				Debug.Log("Ckeck for Distance finished");
				hasToCheckDistance = false;
				playerController.circleWalk = false;
				hasToWalk = false;
				SetAttackInForm(attackingSystem.enemyAttackPlayer, true);
			}
			yield return Timing.WaitForSeconds(0.2f);
		}
	}


	protected void SetLineAttack()
	{
		minAttackRange = float.PositiveInfinity;
		List<AttackingSystem> firstLine = new List<AttackingSystem>();
		int lines = FormationManager.GetLinesCount(formationId, controlledTroops.Count);
		//die Linien setzten auf der Truppe und bei der ersten Riehe den niedrigsten Attack Range setzen
		for (int i = 0; i < controlledTroops.Count; i++)
		{
			int line = FormationManager.GetLine(formationId, controlledTroops[i].playerController.transformOnAttackGrid, lines);
			controlledTroops[i].attackingSystem.lineInFormation = line;
			if (line == 0)
			{
				float attackRange = controlledTroops[i].playerController.attackRange;
				if (attackRange < minAttackRange)
					minAttackRange = attackRange;
				firstLine.Add(controlledTroops[i].attackingSystem);
			}
			controlledTroops[i].transform.parent = troopObject.transform.parent;
			controlledTroops[i].attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
		}
		//for (int i = 0; i < firstLine.Count; i++)
		//{
		//	firstLine[i].frontLineMinAttackRange = minAttackRange;
		//	firstLine[i].GetComponent<AttackingSystem>().StartInvokingRepeat(attackStyleAtMoment);
		//}
		//CancelInvoke();
		//}
	}

	/// <summary>
	///wir veersuchen einen Punkt vor der Truppe zu bekommen, dass wir nicht schräg auf die Truppe treffen sondern von einem bestimmten Winkel(vorne, hinte etc.). Diese Winkel definieren wir in einer Formation im Editor
	/// </summary>
	private Vector3 GetPointBeforeFormation(TroopComponents enemyAttackPlayer, float agentRadius)
	{
		float closestAngle = float.PositiveInfinity;
		int closestAngleId = 0;
		CommanderScript othercommander = enemyAttackPlayer.commanderScript;
		//wir suchen den nächsten Winkel indem wir enemyAttackPlayer.forward drehen und dann den Winkel zwischen Player und diesem Vector berchen. Den kleinsten Winkel speichern wir
		for (int i = 0; i < FormationManager.formations[othercommander.formationId].angles.Length; i++)
		{
			float angle = Vector3.Angle(Quaternion.CreateFromAxisAngle(Vector3.up, FormationManager.formations[othercommander.formationId].angles[i]) * enemyAttackPlayer.transform.forward, (troopObject.transform.position - enemyAttackPlayer.transform.position));
			if(angle < closestAngle)
			{
				closestAngle = angle;
				closestAngleId = i;
			}
		}
		//der Punkt ist bei diesem Winkel in diese Richtung um 6 davor, dass die Armee dann noch 6 units gerade  auf diese  Arme läuft
		return enemyAttackPlayer.transform.position + (Quaternion.CreateFromAxisAngle(Vector3.up, FormationManager.formations[othercommander.formationId].angles[closestAngleId]) * enemyAttackPlayer.transform.forward * ((formationRadius / 2) + (enemyAttackPlayer.commanderScript.formationRadius / 2) + distanceFormPointToArmy));

	}

	/// <summary>
	/// bekommenvon dem Mittelpunkt eines Kreises, auf  welchem die Formation im richtigen Winkel am Schluss auf die angeriffene Formation trifft.
	/// </summary>
	private Vector3 getCirlceMiddlePoint(Transform enemyAttackPlayer)
	{
		//berechnen von einem Mittelpunkt für den Kreis den man laufen muss um gerade auf den Punkt vor der Formation zu treffen
		Vector2 dirFromEnemy = new Vector2(positonToWalkTo.x - enemyAttackPlayer.position.x, positonToWalkTo.z - enemyAttackPlayer.position.z);
		dirFromEnemy = Vector2.Perpendicular(dirFromEnemy).normalized;
		Vector2 dirFromMy = new Vector2(positonToWalkTo.x - troopObject.transform.position.x, positonToWalkTo.z - troopObject.transform.position.z);
		dirFromMy = Vector2.Perpendicular(dirFromMy).normalized;
		middlePoint = Vector3.zero;
		Vector2 middlePointFromMy = new Vector2((troopObject.transform.position.x + positonToWalkTo.x) / 2, (troopObject.transform.position.z + positonToWalkTo.z) / 2);
		bool isIntersect = LineLineIntersection(out middlePoint, new Vector3(positonToWalkTo.x, 0f, positonToWalkTo.z), new Vector3(dirFromEnemy.x, 0f, dirFromEnemy.y), new Vector3(middlePointFromMy.x, 0f, middlePointFromMy.y), new Vector3(dirFromMy.x, 0f, dirFromMy.y));
		middlePoint = new Vector3(middlePoint.x, troopObject.transform.position.y, middlePoint.z);
		return middlePoint;
	}

	/// <summary>
	///berechnen von Schnittpunkt von zwei Linien um Mittelpunkt zu finden
	/// </summary>
	bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{
		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

		//is coplanar, and not parrallel
		if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	//public bool otherTroopsInRadius(float radius, Vector3 position)
	//{
	//	playerController.ignoreCommanderTurning = true;
	//	Collider[] colliders = Physics.OverlapSphere(position, radius, LayerMask.GetMask("Player"));
	//	for(int i = 0; i < colliders.Length; i++)
	//	{
	//		if (colliders[i].GetComponent<PlayerController>().clientId != playerController.clientId)
	//			return true;
	//	}
	//	return false;
	//}

	//public bool isTroopBeforeMe()
	//{

	//}
}
