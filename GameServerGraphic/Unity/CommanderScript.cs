using System.Collections.Generic;
using GameServer;
using Pathfinding;
using MEC;
using System;
using GameServerGraphic;
using System.Drawing;

public enum AttackStyle
{
	Normal,
	Press,
	Charge
}

[System.Serializable]
public class CommanderScript : MonoBehaviour
{
	public bool attackGrid;
	public bool formationHasToStayInLine;
	public bool hasToWalk = false;
	public bool commanderWalkDuringRotation = true;
	public bool hasStoppedAttack = false;
	public int formationId;
	public int childReachedPositionCount;
	public float formationRadius;
	public float minAttackRange = 0f;
	public float formationDeltaX = 5f;
	public Vector3 tempAttackGridDir;
	public AttackStyle attackStyleAtMoment = AttackStyle.Normal;
	public FormationObject formationObject;
	//public NormalComponentsObject circleRenderer;
	public List<Transform> attackTroops = new List<Transform>();
	public List<TroopComponents> controlledTroops = new List<TroopComponents>();
	public List<TroopComponents>[] attackingLines; //-->attackingLines[0] ist erste Linie, attackingLines[0][0] ist erste Truppe auf erstert Linie
	public TroopComponents troopObject;

	protected Vector3 positonToWalkTo;
	protected PlayerController playerController;
	//protected AttackingSystem attackingSystem;
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
		//attackingSystem = troopObject.attackingSystem;
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
			seeker.traversableTags = GetAgentType(agentRadius + 1);
			if (playerController.commanderIsTurning)
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

	public void SetFormation(bool toNewAttackGrid = false)
	{
		/*if (playerController.currentState == STATE.Following)
			StopAttack();*/

		float radius = FormationManager.SetFormation(formationId, controlledTroops, troopObject, playerController.clientId, formationHasToStayInLine, toNewAttackGrid);
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

	public float prepareForFormation()
	{
		richAI.radius = 0.5f;
		float slowestTroop = troopObject.richAI.maxSpeed;
		foreach (TroopComponents troop in controlledTroops)
		{
			if (troop.richAI.maxSpeed < slowestTroop)
				slowestTroop = troop.richAI.maxSpeed;
			troop.transform.parent = troopObject.transform.parent;
			troop.richAI.SetPath(null);
			//troop.playerController.transformOnAttackGrid = null;
		}
		return slowestTroop;
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
		//for(int i = 0; i<controlledTroops.Count; i++)
		//{
		//	controlledTroops[i].attackingSystem.enemyAttackPlayer = nearetsObject;
		//}
		SetAttackInForm(nearetsObject, false);
	}

	/// <summary>
	///Wenn der Commander in der Formation auf die anderen Truppen AttackForm trifft, dann muss die Formation richtig angreifen
	/// </summary>
	public virtual void SetAttackInForm(TroopComponents enemyAttackPlayer, bool hasMadeTurn)
	{
		Debug.Log("Attack with: " + hasMadeTurn + "  " + attackStyleAtMoment);
		bool attackOtherAttackGrid = false;
		TroopComponents otherCommander = null;

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

	protected virtual void AttackNormalOtherFormation(TroopComponents enemyAttackPlayer, TroopComponents otherCommander, bool hasMadeTurn)
	{
		if(!hasMadeTurn)
		{
			troopObject.newAttackSystem.StartFollowing(otherCommander, attackStyleAtMoment);
		}
		else
		{
			attackingLines = new List<TroopComponents>[20];
			int maxLine = 0;
			for (int i = 0; i < controlledTroops.Count; i++)
			{
				//wenn sterben, die Truppe hinten dran Informieren um seine Platz einzunehem, wenn schon tot oder weg, eine nächsten in der nächsten Riehe finden
				int myLine = controlledTroops[i].playerController.lineInFormation;
				if (attackingLines[myLine - 1] == null)
				{
					attackingLines[myLine - 1] = new List<TroopComponents>();
					maxLine++;
				}
				if (!attackingLines[myLine - 1].Contains(controlledTroops[i]))
				{
					attackingLines[myLine - 1].Add(controlledTroops[i]);
				}
				controlledTroops[i].richAI.enabled = true;
				controlledTroops[i].richAI.canMove = true;
				controlledTroops[i].playerController.isAttacking = true;
				controlledTroops[i].transform.parent = null;
				controlledTroops[i].playerController.currentState = STATE.Following;
			}
			Array.Resize(ref attackingLines, maxLine);
		}
	}

	void AttackChargeOtherFormation(TroopComponents enemyAttackPlayer, TroopComponents otherCommander, bool hasMadeTurn)
	{
		if (!hasMadeTurn)
		{
			troopObject.newAttackSystem.StartFollowing(otherCommander, attackStyleAtMoment);
		}
		else
		{
			Random rand = new Random();
			attackingLines = new List<TroopComponents>[20];
			int maxLine = 0;
			for (int i = 0; i < controlledTroops.Count; i++)
			{
				//wenn sterben, die Truppe hinten dran Informieren um seine Platz einzunehem, wenn schon tot oder weg, eine nächsten in der nächsten Riehe finden
				int myLine = controlledTroops[i].playerController.lineInFormation;
				if (attackingLines[myLine - 1] == null)
				{
					attackingLines[myLine - 1] = new List<TroopComponents>();
					maxLine++;
				}
				if (!attackingLines[myLine - 1].Contains(controlledTroops[i]))
				{
					attackingLines[myLine - 1].Add(controlledTroops[i]);
				}
				controlledTroops[i].richAI.enabled = true;
				controlledTroops[i].richAI.canMove = true;
				//controlledTroops[i].richAI.onSearchPath += controlledTroops[i].playerController.UpdateEnemyTroopPoisition;
				if (myLine == 1)
					controlledTroops[i].newAttackSystem.StartAttack((float)rand.NextDouble(), attackStyleAtMoment);
				else
					controlledTroops[i].newAttackSystem.StartAttack(0, attackStyleAtMoment);
				controlledTroops[i].playerController.isAttacking = true;
				controlledTroops[i].transform.parent = null;
				controlledTroops[i].playerController.currentState = STATE.Following;
			}
			Array.Resize(ref attackingLines, maxLine);
			richAI.onSearchPath += playerController.UpdateEnemyTroopPoisition;
			playerController.isAttacking = true;
			troopObject.newAttackSystem.enemyPlayer = otherCommander;
			troopObject.newAttackSystem.StartAttack((float)rand.NextDouble(), attackStyleAtMoment);
		}
	}

	#region AttackSyles
	//protected virtual void AttackNormalOtherFormation(TroopComponents enemyAttackPlayer, TroopComponents otherCommander, bool hasMadeTurn)
	//{
	//	enemyAttackPlayer = otherCommander;
	//	enemyAttackPlayer.commanderScript.GettingAttacked(attackStyleAtMoment);
	//	float preRadius = richAI.radius;
	//	richAI.radius = 0.5f;
	//	seeker.traversableTags = GetAgentType(0f);
	//	playerController.currentState = STATE.Following;
	//	attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
	//	if (hasMadeTurn)
	//	{
	//		//ServerSend.troopMove(true, playerController.clientId, playerController.troopId, transform.position, Vector3.zero, 1f, true, false);
	//		//richAI.endReachedDistance = Server.clients[playerController.clientId].player.placedTroops[playerController.troopId].troop.attackRadius;
	//		//playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
	//		ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
	//		SetLineAttack();
	//	}
	//	else
	//	{
	//		//TODO try to attack without circle walk --> prevent backwards Circle
	//		if ((troopObject.transform.position - enemyAttackPlayer.transform.position).sqrMagnitude <= distanceFormPointToArmy * distanceFormPointToArmy)
	//			Debug.Log("Is nearer");
	//		//der Commander wenn der angreifft läuft einen Bogen um in einem richtigen Winkel auf die Truppe zu treffen. Diese Winkel sind bie Formationen definiert
	//		positonToWalkTo = GetPointBeforeFormation(enemyAttackPlayer, preRadius);
	//		middlePoint = getCirlceMiddlePoint(enemyAttackPlayer.transform);
	//		Form1.SpawnPointAt(middlePoint, Color.Green, 10);
	//		Form1.SpawnPointAt(positonToWalkTo, Color.Red, 10);
	//		playerController.circleMiddlePoint = middlePoint;
	//		//circleRenderer.transform.position = middlePoint;
	//		//circleRenderer.GetComponent<LineRendererCircle>().radius = Vector3.Distance(transform.position, middlePoint);
	//		//circleRenderer.GetComponent<LineRendererCircle>().CreatePoints();
	//		movement = middlePoint - positonToWalkTo;
	//		movement = new Vector3(movement.z, 0, -movement.x);
	//		float dot = Vector3.Dot(movement, positonToWalkTo - enemyAttackPlayer.transform.position);
	//		if (dot < 0f)
	//			factorCircleSide = 1f;
	//		else
	//			factorCircleSide = -1f;

	//		playerController.factorCircleSide = factorCircleSide;
	//		movement = middlePoint - troopObject.transform.position;
	//		movement = new Vector3(movement.z, 0, -movement.x) * factorCircleSide;
	//		if (Vector3.Angle(troopObject.transform.forward, movement) > 16f)
	//		{
	//			hasToWalk = false;
	//			playerController.currentWalkMode = WalkMode.Normal;
	//			playerController.StartTruning(movement.normalized);
	//		}
	//		else
	//		{
	//			playerController.currentState = STATE.Following;
	//			hasToWalk = true;
	//			playerController.currentWalkMode = WalkMode.CircleWalk;
	//		}
	//		//richAI.endReachedDistance = 0.9f;
	//		Timing.RunCoroutine(CheckForDistanceCoroutine(), playerController.clientId.ToString());
	//	}
	//}

	//void AttackChargeOtherFormation(TroopComponents enemyAttackPlayer, TroopComponents otherCommander, bool hasMadeTurn)
	//{
	//	enemyAttackPlayer = otherCommander;
	//	playerController.currentState = STATE.Following;
	//	//attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
	//	if (!hasMadeTurn)
	//	{
	//		positonToWalkTo = enemyAttackPlayer.transform.position + ((troopObject.transform.position - enemyAttackPlayer.transform.position).normalized * ((formationRadius / 2) + (enemyAttackPlayer.commanderScript.formationRadius / 2) + 8f));
	//		//richAI.endReachedDistance = 1.3f;
	//		playerController.MoveToPosition(positonToWalkTo, false);
	//		Timing.RunCoroutine(CheckForDistanceCoroutine(), playerController.clientId.ToString());
	//	}
	//	else
	//	{
	//		enemyAttackPlayer.commanderScript.GettingAttacked(attackStyleAtMoment);
	//		float preRadius = richAI.radius;
	//		richAI.radius = 0.5f;
	//		seeker.traversableTags = GetAgentType(0);
	//		for (int i = 0; i < controlledTroops.Count; i++)
	//		{
	//			PlayerController controlledTroopPlayerController = controlledTroops[i].playerController;
	//			controlledTroopPlayerController.richAI.enabled = true;
	//			controlledTroopPlayerController.currentState = STATE.Following;
	//			controlledTroopPlayerController.MoveToPosition(enemyAttackPlayer.transform.position + (controlledTroops[i].transform.position - troopObject.transform.position), false);
	//			//controlledTroops[i].attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
	//			//controlledTroopPlayerController.RVOController.priority = 0.4f;
	//		}
	//		playerController.currentState = STATE.Following;
	//		playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
	//		//attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
	//		//playerController.RVOController.priority = 0.4f;
	//		//ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
	//		troopObject.playerController.ignoreCommanderTurning = true;
	//	}
	//}

	private void AttackNormalOtherTroops(TroopComponents enemyAttackPlayer)
	{
		richAI.radius = 0.5f;
		seeker.traversableTags = GetAgentType(0);
		playerController.currentState = STATE.Following;
		//attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
		//playerController.ignoreCommanderTurning = true;
		//Collider[] coliders = Physics.OverlapSphere(enemyAttackPlayer.position, 5f, LayerMask.GetMask("Player"));
		//ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
		for (int i = 0; i < controlledTroops.Count; i++)
		{
			//controlledTroops[i].attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
			//controlledTroops[i].attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		}
	}

	private void AttackChargeOtherTroops(TroopComponents enemyAttackPlayer, bool hasMadeTurn)
	{
		playerController.currentState = STATE.Following;
		//attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		if (!hasMadeTurn)
		{
			positonToWalkTo = enemyAttackPlayer.transform.position + ((troopObject.transform.position - enemyAttackPlayer.transform.position).normalized * ((formationRadius / 2) + (enemyAttackPlayer.commanderScript.formationRadius / 2) + distanceFormPointToArmy));
			//richAI.endReachedDistance = 1.3f;
			playerController.MoveToPosition(positonToWalkTo, false);
			Timing.RunCoroutine(CheckForDistanceCoroutine(), playerController.clientId.ToString());
		}
		else
		{
			richAI.radius = 0.5f;
			seeker.traversableTags = GetAgentType(0);
			playerController.currentState = STATE.Following;
			//attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
			playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
			//attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
			//ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
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
					//controlledTroopPlayerController.troopObject.attackingSystem.enemyAttackPlayer = colliders[i];
				}
				else
				{
					if (y > colliders.Length - 1)
						y = -1;
					y++;
					controlledTroopPlayerController.MoveToPosition(colliders[colliders.Length - y].transform.position, false);
					//controlledTroopPlayerController.troopObject.attackingSystem.enemyAttackPlayer = colliders[colliders.Length - y];
				}
				//controlledTroops[i].attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
			}
		}
	}
	#endregion

	public void StopAttack()
	{
		Debug.Log("Stop Attack");
		playerController.currentState = STATE.Idle;
		playerController.ignoreCommanderTurning = false;
		//attackingSystem.StopRepeat();
		//playerController.RVOController.priority = 0.5f;
		playerController.currentWalkMode = WalkMode.Normal;
		hasToWalk = false;
		hasToCheckDistance = false;
		ServerSend.StartFight(playerController.clientId, playerController.troopId, false, true);
		//hasToWalk = false;
		for(int i =0; i < controlledTroops.Count; i++)
		{
			controlledTroops[i].transform.parent = troopObject.transform;
			//controlledTroops[i].GetComponent<PlayerController>().RVOController.priority = 0.5f;
			controlledTroops[i].playerController.currentState = STATE.attackGrid;
			//controlledTroops[i].attackingSystem.StopRepeat();
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
		//float smallestDistance = float.PositiveInfinity;
		while (hasToCheckDistance)
		{
			//if ((troopObject.transform.position - positonToWalkTo).magnitude < smallestDistance)
			//	smallestDistance = (troopObject.transform.position - positonToWalkTo).magnitude;
			// TODO better distance Search MACH DAS NICHT SO SEHR SCHLECHT
			//positonToWalkTo = (middlePoint - positonToWalkTo).normalized * (troopObject.transform.position - middlePoint).magnitude;
			//Form1.UpdatePointPosition(1, positonToWalkTo);
			if ((troopObject.transform.position - positonToWalkTo).sqrMagnitude < 1f)
			{
				Debug.Log("Ckeck for Distance finished");
				hasToCheckDistance = false;
				playerController.currentWalkMode = WalkMode.Normal;
				hasToWalk = false;
				//SetAttackInForm(attackingSystem.enemyAttackPlayer, true);
			}
			yield return Timing.WaitForSeconds(0.2f);
		}
	}


	public void MakeAttackGrid(/*int width, int length, Vector3 startPoint, Vector3 widthDir, float deltaX*/ Vector3 lineLength, float deltaX)
	{
		////float deltaX = 2f;
		//Vector3 lineLength = (Quaternion.Euler(0f, 90f, 0f) * widthDir).normalized;
		//int amountUnits = controlledTroops.Count + 1;
		//formationObject.formationObjects = new FormationChild[amountUnits];
		//tempAttackGridDir = lineLength;
		//formationObject.transform.rotation = Quaternion.LookRotation(lineLength, Vector3.up);
		//formationObject.transform.name = formationId.ToString();
		//int rest = amountUnits - width * (length - 1);
		//int commanderPlace = ((length-2) / 2 * width + rest) + width / 2;
		////formationObject.transform.rotation = Quaternion.LookRotation(lineLength, Vector3.up);
		////relative to Commander and move whole thing with commander
		//for (int i = 0; i < length; i++)
		//{
		//	Vector3 pointOnColumns = startPoint + lineLength * deltaX * i;
		//	int offset = 0;
		//	if (i == 0)
		//	{
		//		offset = (int)(width / 2f) - (int)((rest) / 2f);
		//	}
		//	for (int y = 0; y < width; y++)
		//	{
		//		if (i == 0 && y > rest-1)
		//			break;
		//		Vector3 pointOnRow = pointOnColumns + widthDir * deltaX * (y + offset);
		//		int index = ((i - 1) * width) + y + rest;
		//		if (i == 0)
		//			index = y;

		//		if (index < commanderPlace)
		//			index += 1;
		//		else if (index == commanderPlace)
		//		{
		//			index = 0;
		//			formationObject.transform.MoveWithoutChilds(pointOnRow);
		//		}
		//		Form1.SpawnPointAt(pointOnRow, Color.Red, 5);
		//		formationObject.formationObjects[index] = new FormationChild(new Transform(pointOnRow, Quaternion.Identity, formationObject.transform), i, null, null);
		//	}
		//}
		formationDeltaX = deltaX;
		attackGrid = true;
		Vector3 commanderToPos = formationObject.formationObjects[0].transform.position;
		formationObject.transform.localPosition = Vector3.zero;
		//formationObject.transform.rotation = troopObject.transform.rotation;
		formationObject.transform.parent = troopObject.transform;
		Form1.SpawnPointAt(formationObject.transform.position, Color.Blue, 10);
		Form1.SpawnPointAt(formationObject.transform.position + formationObject.transform.forward * 10f, Color.AliceBlue, 10);
		for (int i = 0; i < controlledTroops.Count; i++)
		{
			controlledTroops[i].transform.parent = null;
		}
		troopObject.playerController.StartHermitCurve(commanderToPos, lineLength);
		//float radius;
		//Vector3 pointBeforeFormation;
		//Player.SearchCircle(troopObject.transform.position, commanderToPos, lineLength, out troopObject.playerController.circleMiddlePoint, out radius, out troopObject.playerController.factorCircleSide, out pointBeforeFormation);
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

	public CommanderScript() { }
}
