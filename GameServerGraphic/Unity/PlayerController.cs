using GameServer;
using Pathfinding;
using System.Collections.Generic;
using MEC;
using Pathfinding.RVO;
using GameServerGraphic;

public enum STATE
{
	Idle,
	Moving,
	attackGrid,
	Following,
	Hitting,
	HittingInFormation
}

public enum WalkMode
{
	Normal,
	InAttackGrid,
	InNewBoxAttackGrid,
	BezierCurveWalk,
	CircleWalk,
}

public enum SendState
{
	noSend,
	walk, 
	leftPath,
	toAttackGrid,
	pushed,
	circleWalk,
}

[System.Serializable]
public class PlayerController : MonoBehaviour
{

	public bool enabled = true;
	public bool commanderIsTurning;
	public bool ignoreCommanderTurning;
	public bool isAttacking = false;
	public int clientId;
	public int troopId;
	public int formationId = -2;
	public int lineInFormation;
	public float factorCircleSide;
	public float attackRange;
	public float attackSpeed;
	public float troopRadius;
	public float startTimeTurnCommander;
	public Vector3 movementDirectionCircle;
	public Vector3 circleMiddlePoint;
	public STATE currentState;
	public WalkMode currentWalkMode = WalkMode.Normal;
	public SendState currentSendState = SendState.noSend;
	public Troops myTroop { get; private set; }
	public FormationChild transformOnAttackGrid;
	public RichAI richAI;
	public TroopComponents Mycommander;
	public TroopComponents troopObject;
	public Seeker seeker;
	public TroopComponents enemyTroop;

	const float walkSendTime = 0.5f;
	const float leftPathSendTime = 0.1f;
	const float toAttackgridSendTime = 0.5f;
	const float pushedSendTime = 0.1f;
	const float circleWalkSendTime = 0.1f;
	const float toAttackGridBezierCurveSendTime = 0.5f;

	public static int playerIdNowStop = 0;

	bool waitAfterStart = false;
	bool setOneTime;
	float remainingTime;
	Vector3 pointToSendToClient;
	Vector3 lastPosition;
	Vector3 zielPunkt;
	Vector3 dirToWalkIn;
	Vector3 startingPoint;
	Rect3D bounds;
	CoroutineHandle? courutineHandle = null;
	BezierCurveXZPlane bezierCurve;
	//AttackingSystem attackingSystem;

	[System.NonSerialized]
	[System.Xml.Serialization.XmlIgnore]
	public Client myClient;


	public override void Start(TroopComponents _troopComponents)
	{
		troopObject = _troopComponents;
		ignoreCommanderTurning = false;
		seeker = troopObject.seeker;
		richAI = troopObject.richAI;
		//attackingSystem = troopObject.attackingSystem;
		troopRadius = richAI.radius;
		commanderIsTurning = false;
		myClient = Server.clients[clientId];
		attackRange += troopRadius;
		Timing.RunCoroutine(waitAfterStartCourutine(), clientId.ToString());
		myTroop = Server.clients[clientId].player.placedTroops[troopId].troop;
		if (Mycommander != null && currentState == STATE.attackGrid || Mycommander != null && Mycommander == troopObject)
			return;

		currentState = STATE.Idle;
		if (troopObject.commanderScript != null)
			Mycommander = troopObject;

		bounds.center = troopObject.transform.position;
		bounds.size =  new Vector3(1.5f, 1.5f, 1.5f);
		troopObject.richAI.canSearch = true;
		//troopObject.richAI.destination = new Vector3(360f, -67f, -7.6f);
		//Form1.SpawnPointAt(new Vector3(360f, -67f, -7.6f), System.Drawing.Color.Red, 10);
		Form1.AddTroop(troopObject.transform);
	}


	public void Update()
	{
		if (!enabled)
			return;
		if (!isAttacking)
		{
			NormalUpdate();
		}
		else
		{
			troopObject.newAttackSystem.Update();
			//AttackUpdate();
		}
	}

	private void NormalUpdate()
	{
		//if (troopObject.commanderScript != null)
		//	Debug.Log(currentWalkMode.ToString() + currentState.ToString());
		if (!commanderIsTurning || troopObject.commanderScript != null && troopObject.commanderScript.commanderWalkDuringRotation)
		{
			/*if (path == null)
			{
				//We have no path to move after yet
				return;
			}*/

			if (currentState == STATE.Moving || currentState == STATE.Following)
			{
				if (richAI.pathPending)
				{
					//currentState = STATE.Idle;
					return;
				}
				if (richAI.reachedEndOfPath && currentWalkMode == WalkMode.Normal)
				{
					ReachedEndOfPath();
					return;
				}

				#region deletedTurnDuringWalk
				//Ray ray = new Ray(richAI., richAI.steeringTarget - startingPoint);
				//Debug.DrawRay(startingPoint, richAI.steeringTarget - startingPoint);
				//angleToDirection = Vector3.Angle(ray.direction, richAI.desiredVelocity);

				/*//check if Commander is turning withh a group, then he has to turn
				if (Vector3.Angle(transform.forward, agent.steeringTarget - transform.position) > 20f && GetComponent<CommanderScript>() != null && angleToDirection < 10f && !agent.pathPending)
				{
					if (GetComponent<CommanderScript>().attackGrid && Vector3.Distance(transform.position, agent.steeringTarget) > 2f + agent.stoppingDistance && !ignoreCommanderTurning)
					{
						Debug.LogFormat("Commander is turning with Angle of: {0} and angleToDirecton of: {1}", Vector3.Angle(transform.forward, agent.steeringTarget - transform.position), angleToDirection);
						CheckForSend(SendState.noSend);
						agent.isStopped = true;
						commanderIsTurning = true;
						GetComponent<CommanderScript>().prepareForFormation(true);
						setOneTime = false;
						ServerSend.troopMove(true, clientId, troopId, Vector3.zero, pointToSendToClient, 2000f, false);
						ServerSend.troopMove(false, myClient.enemyClient.id, troopId, Vector3.zero, pointToSendToClient, 2000f, false);
						return;
					}
				}*/
				#endregion

				remainingTime = Vector3.Distance(pointToSendToClient, troopObject.transform.position) / richAI.desiredVelocity.magnitude;

				////hat den Ursprünglichen Weg verlassem
				//if (angleToDirection > 10f && currentState != STATE.attackGrid && !tempAttackGrid && !agent.pathPending)
				//{
				//	CheckForSend(SendState.leftPath);
				//}
				//else
				//{
				if (currentWalkMode == WalkMode.CircleWalk)
				{
					movementDirectionCircle = circleMiddlePoint - troopObject.transform.position;
					movementDirectionCircle = new Vector3(movementDirectionCircle.z, 0, -movementDirectionCircle.x) * factorCircleSide;
					movementDirectionCircle = movementDirectionCircle.normalized * richAI.maxSpeed;
					richAI.Move(movementDirectionCircle * Time.deltaTime);
					richAI.FinalizeRotation(Quaternion.LookRotation(movementDirectionCircle, Vector3.up));
					CheckForSend(SendState.circleWalk);
				}
				else if(currentWalkMode == WalkMode.BezierCurveWalk)
				{
					if (bezierCurve.tMove < 1f)
					{
						Vector3 moveDir = bezierCurve.Move(richAI.maxSpeed);
						richAI.updateRotation = true;
						if (moveDir != Vector3.zero)
						{
							richAI.Move(moveDir);
							richAI.FinalizeRotation(Quaternion.LookRotation(moveDir, Vector3.up));
						}
						CheckForSend(SendState.noSend);
						//Debug.Log(Quaternion.ToEulerAngles(Quaternion.LookRotation(moveDir, Vector3.up)));
					}
					else
					{
						ReachedEndOfPath();
					}
				}
				//läuft in die AttackForm
				else if (currentWalkMode == WalkMode.InAttackGrid)
				{
					if (richAI.hasPath)
						richAI.SetPath(null);
					if (!Mycommander.commanderScript.commanderWalkDuringRotation)
					{
						if ((troopObject.transform.position - zielPunkt).sqrMagnitude <= richAI.endReachedDistance * richAI.endReachedDistance)
						{
							ReachedEndOfPath();
							return;
						}
						if (transformOnAttackGrid == null)
							return;
						//troopObject.richAI.destination = Mycommander.transform. transformOnAttackGrid.transform.localPosition
						CheckForSend(SendState.toAttackGrid);
						richAI.Move((transformOnAttackGrid.transform.position - troopObject.transform.position).normalized * richAI.maxSpeed * Time.deltaTime);
					}
					else
					{
						//if ((troopObject.transform.position - transformOnAttackGrid.transform.position).sqrMagnitude <= richAI.endReachedDistance * richAI.endReachedDistance && Time.time - startTimeTurnCommander > 1f)
						if ((troopObject.transform.position - transformOnAttackGrid.transform.position).sqrMagnitude <= richAI.endReachedDistance * richAI.endReachedDistance && Time.time - Mycommander.playerController.startTimeTurnCommander > 1f)
						{
							Debug.Log("OK");
							ReachedEndOfPath();
							return;
						}
						CheckForSend(SendState.toAttackGrid);
						richAI.Move((transformOnAttackGrid.transform.position - troopObject.transform.position).normalized * richAI.maxSpeed * Time.deltaTime);
						//richAI.destination = transformOnAttackGrid.transform.position;
					}
				}
				else if(currentWalkMode == WalkMode.InNewBoxAttackGrid && transformOnAttackGrid != null)
				{
					troopObject.richAI.destination = transformOnAttackGrid.transform.position;
					//troopObject.richAI.Move((transformOnAttackGrid.transform.position - troopObject.transform.position).normalized * richAI.maxSpeed * Time.deltaTime);
					CheckForSend(SendState.toAttackGrid);
				}
				//normales laufen
				else
				{
					CheckForSend(SendState.walk);
				}
				//}
				//Debug.DrawRay(transform.position, transform.forward * 10f, Color.yellow);
			}
			else
			{
				//wenn sich die Troop bewegt ohne zu laufen(z.B. auf die Seite geschoben)
				if (currentState != STATE.attackGrid && /*currentWalkMode != WalkMode.CircleWalk &&*/ waitAfterStart)
				{
					if ((troopObject.transform.position - lastPosition).sqrMagnitude > 5f)
					{
						CheckForSend(SendState.pushed);
						lastPosition = troopObject.transform.position;
					}
					else if (/*currentWalkMode != WalkMode.CircleWalk &&*/ currentSendState != SendState.noSend)
						CheckForSend(SendState.noSend);
				}
			}
		}
		else
		{
			if (Vector3.Angle(troopObject.transform.forward, richAI.steeringTarget - troopObject.transform.position) > 5f)
			{
				if (richAI.pathPending)
					return;
				Quaternion tragetLook = Quaternion.LookRotation(richAI.steeringTarget - troopObject.transform.position, Vector3.up);
				//richAI.FinalizeMovement(transform.position, tragetLook);
				//Quaternion q = richAI.SimulateRotationTowards(transform.forward, 360f);
				richAI.FinalizeMovement(troopObject.transform.position, tragetLook);
				//transform.rotation = tragetLook;
			}
			else if (!setOneTime)
			{
				troopObject.commanderScript.formationObject.transform.rotation = troopObject.transform.rotation;
				//troopObject.commanderScript.formationHasToStayInLine = true;
				setOneTime = true;
				troopObject.commanderScript.SetFormation();
			}
		}
	}

	void AttackUpdate()
	{
		////if (troopId == playerIdNowStop)
		////	Debug.Log("NOw");
	}

	public void UpdateEnemyTroopPoisition()
	{
		//if (enemyTroop != null)
		//	richAI.destination = enemyTroop.transform.position;
		//else
		//	richAI.destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
		//Debug.Log("Calculate Path");
	}


	#region SendTroopMove
	/// <summary>
	/// Das Einleiten von dem Senden in einer Coroutine wird hier eingeleitet. Mit dem currentSendState kann gesagt werde, wie die Truppe gerade sendet.
	/// Mit dem courutineHandle wird die momentate coroutine angezeigt (ist ein Object, da man keine Variabel als CoroutineHandel definieren kann). 
	/// Ausserdem kann es auf Null gesetzt werden, wenn keine Coroutine läuft und eine spezifische Coroutine gestoppt werden.
	/// </summary>
	public void CheckForSend(SendState lookingState)
	{
		//Debug.Log("Now Check For Send");
		currentSendState = lookingState;
		if (lookingState == SendState.noSend)
		{
			if(courutineHandle != null)
				Timing.KillCoroutines((CoroutineHandle)courutineHandle);
			courutineHandle = null;
			return;
		}
		if (courutineHandle == null || currentSendState != lookingState)
		{
			if (courutineHandle != null)
			{
				Timing.KillCoroutines(courutineHandle.Value);
				courutineHandle = null;
			}
			switch (lookingState)
			{
				case SendState.walk:
					courutineHandle = Timing.RunCoroutine(sendTroopMove(walkSendTime)/*.CancelWith(troopObject)*/, clientId.ToString());
					break;
				case SendState.toAttackGrid:
					courutineHandle = Timing.RunCoroutine(sendTroopMove(toAttackgridSendTime)/*.CancelWith(troopObject)*/, clientId.ToString());
					break;
				case SendState.leftPath:
					courutineHandle = Timing.RunCoroutine(sendTroopMove(leftPathSendTime)/*.CancelWith(troopObject)*/, clientId.ToString());
					break;
				case SendState.pushed:
					courutineHandle = Timing.RunCoroutine(sendTroopMove(pushedSendTime)/*.CancelWith(troopObject)*/, clientId.ToString());
					break;
				case SendState.circleWalk:
					courutineHandle = Timing.RunCoroutine(sendTroopMove(circleWalkSendTime)/*.CancelWith(troopObject)*/, clientId.ToString());
					break;
			}
		}
	}

	IEnumerator<float> sendTroopMove(float waitTime)
	{
		while (true)
		{
			switch (currentSendState)
			{
				case SendState.walk:
					//pointToSendToClient = richAI.steeringTarget - ((richAI.steeringTarget - transform.position).normalized * (richAI.endReachedDistance - (richAI.radius/2)));
					pointToSendToClient = richAI.steeringTarget;
					ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
					ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
					break;
				case SendState.toAttackGrid:
					//pointToSendToClient = Mycommander.InverseTransformPoint(transformOnAttackGrid.position);
					pointToSendToClient = transformOnAttackGrid.transform.localPosition;
					ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
					ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
					break;
				case SendState.leftPath:
					ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, richAI.desiredVelocity, remainingTime, true);
					ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, richAI.desiredVelocity, remainingTime, true);
					break;
				case SendState.pushed:
					ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, Vector3.zero, 1000f, false);
					ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, Vector3.zero, 1000f, false);
					break;
				case SendState.circleWalk:
					ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, movementDirectionCircle, 10f, true);
					ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, movementDirectionCircle, 10f, true);
					break;
			}
			yield return Timing.WaitForSeconds(waitTime);
		}
	}
	#endregion

	public void MoveToPosition(Vector3 position, bool toAttackGrid)
	{
		//if (commanderIsTurning)
		//{
		//	return;
		//}
		if (currentWalkMode == WalkMode.BezierCurveWalk)
		{
			ignoreCommanderTurning = false;
			currentWalkMode = WalkMode.Normal;
		}
		enabled = true;
		richAI.enabled = true;
		startingPoint = troopObject.transform.position;
		if (currentState != STATE.attackGrid || troopObject.commanderScript != null || toAttackGrid)
		{
			if (Vector3.Angle(troopObject.transform.forward, position - troopObject.transform.position) > 16f && troopObject.commanderScript != null && troopObject.commanderScript.attackGrid && !ignoreCommanderTurning)
			{
				var enemy = myClient.enemyClient.player.FindNearestTroop(troopObject.transform.position);
				if (enemy != null && Vector3.Distance(enemy.transform.position, troopObject.transform.position) < 4f)
					Debug.Log("Found near enemy"); //wenn sehr nahe anderen Truppen nicht Truppe drehen
				else
				{
					//nachdem der Weg fertig ist, kann sich der Commander drehen
					seeker.StartPath(troopObject.transform.position, position, waitTillPathIsReady);
					ServerSend.troopMove(true, clientId, troopId, Vector3.zero, pointToSendToClient, 2000f, false);
					//to enemy
					ServerSend.troopMove(false, myClient.enemyClient.id, troopId, Vector3.zero, pointToSendToClient, 2000f, false);
					return;
				}
			}
			else
			{
				if (toAttackGrid)
				{
					//TODO wieso ist transformOnAttackGrid manchmal null
					if (transformOnAttackGrid != null && (troopObject.transform.position - transformOnAttackGrid.transform.position).sqrMagnitude <= richAI.endReachedDistance * richAI.endReachedDistance)
					{
						pointToSendToClient = transformOnAttackGrid.transform.localPosition;
						ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
						ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
						troopObject.transform.parent = Mycommander.transform;
						currentWalkMode = WalkMode.Normal;
						currentState = STATE.attackGrid;
						richAI.enabled = false;
						Mycommander.commanderScript.childhasReachedGridPoint(troopId, troopObject.transform.position, clientId);
						return;
					}
					richAI.destination = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
					richAI.SetPath(null);
					currentWalkMode = WalkMode.InAttackGrid;
					currentState = STATE.Moving;
					troopObject.transform.parent = Mycommander.transform.parent;
					zielPunkt = position;
					dirToWalkIn = (position - troopObject.transform.position).normalized;
					//Form1.SpawnPointAt(position, System.Drawing.Color.Red, 10);
				}
				else
				{
					if (currentState == STATE.Following)
						seeker.StartPath(troopObject.transform.position, position, OnPathReadyFollowing);
					else
						seeker.StartPath(troopObject.transform.position, position, OnPathReadyNormal);
				}
			}
		}
		else
		{
			richAI.enabled = false;
		}
	}

	//man schhaut ob in der Nähe, wo man geklickt hat eine andere Truppe steht, wenn ja, dann muss man angreifen
	public bool checkIfAttack(Vector3 position)
	{
		if (myClient.enemyClient.player.placedTroops.Count != 0)
		{
			TroopComponents nearestObject = myClient.enemyClient.player.FindNearestTroop(position);
			float checkDistance = 2f;
			if (nearestObject.playerController.Mycommander != null)
			{
				checkDistance = nearestObject.playerController.Mycommander.commanderScript.formationRadius;
				nearestObject = nearestObject.playerController.Mycommander;
			}
			float distanceToEnemy = (position - nearestObject.transform.position).sqrMagnitude;
			if (distanceToEnemy < checkDistance * checkDistance)
			{
				//isAttacking = true;
				currentState = STATE.Following;
				//agent.stoppingDistance = 0.1f;
				//richAI.endReachedDistance = myClient.player.placedTroops[troopId].troop.attackRadius - 0.2f;

				//troopObject.attackingSystem.enemyAttackPlayer = nearestObject;
				if (troopObject.commanderScript != null && troopObject.commanderScript.attackGrid)
				{
					troopObject.commanderScript.BeginAttackInFormation(nearestObject);
				}
				return true;
			}
			else
			{
				if (troopObject.commanderScript != null && troopObject.commanderScript.attackGrid)
					troopObject.commanderScript.StopAttack();
			}
		}
		return false;
	}

	private void ReachedEndOfPath()
	{
		if (troopObject.commanderScript != null && troopObject.commanderScript.controlledTroops.Count > 0 && troopObject.commanderScript.controlledTroops[0].playerController.currentWalkMode == WalkMode.InNewBoxAttackGrid)
		{
			troopObject.transform.rotation = Quaternion.LookRotation(bezierCurve.fourthPosition-bezierCurve.thirdPosition, Vector3.up);
			troopObject.commanderScript.formationObject.transform.rotation = Quaternion.LookRotation(bezierCurve.fourthPosition - bezierCurve.thirdPosition, Vector3.up);
			ignoreCommanderTurning = false;
			currentState = STATE.Idle;
			currentWalkMode = WalkMode.Normal;
			richAI.SetPath(null);
			troopObject.commanderScript.SetFormation();
			CheckForSend(SendState.noSend);
			//seeker.StartPath(troopObject.transform.position, bezierCurve.fourthPosition);
			ServerSend.hasReachedDestination(true, troopId, clientId, troopObject.transform.position, -1);
			ServerSend.hasReachedDestination(false, troopId, myClient.enemyClient.id, troopObject.transform.position, -1);
			return;
		}
		/*if (tempAttackGrid && GetComponentInParent<CommanderScript>() == null)
			return;*/
		CheckForSend(SendState.noSend);
		currentState = STATE.Idle;
		richAI.canMove = true;
		if (currentWalkMode == WalkMode.InAttackGrid)
		{
			currentWalkMode = WalkMode.Normal;
			currentState = STATE.attackGrid;
			CheckForSend(SendState.noSend);
			troopObject.transform.parent = Mycommander.transform;
			richAI.enabled = false;
			troopObject.transform.position = transformOnAttackGrid.transform.position;
			Mycommander.commanderScript.childhasReachedGridPoint(troopId, troopObject.transform.position, clientId);
			return;
			//ServerSend.hasReachedDestination(true, troopId, clientId, transform.position, troopId);
		}
		//if (troopObject.commanderScript == null)
		//{
		//	foreach (PlacedTroopStruct commander in myClient.player.placedTroops)
		//	{
		//		if (commander.troop.klasse.IsFlagSet(TroopClass.Commander))
		//		{
		//			CommanderScript comm = commander.gameObject.commanderScript;

		//			if ((troopObject.transform.position - commander.gameObject.transform.position).sqrMagnitude <= commander.troop.placeRadius * commander.troop.placeRadius && !comm.controlledTroops.Contains(troopObject) && comm.troopObject.playerController.currentState == STATE.Idle)
		//			{
		//				int commanderId = comm.troopObject.playerController.troopId;
		//				myClient.player.SetCommanderChild(commanderId, troopId, true);
		//				ServerSend.hasReachedDestination(true, troopId, clientId, troopObject.transform.position, commanderId);
		//				ServerSend.hasReachedDestination(false, troopId, myClient.enemyClient.id, troopObject.transform.position, commanderId);
		//			}
		//		}
		//	}
		//}
		ServerSend.hasReachedDestination(true, troopId, clientId, troopObject.transform.position, -1);
		ServerSend.hasReachedDestination(false, troopId, myClient.enemyClient.id, troopObject.transform.position, -1);
		if(troopObject.GetParentNormalComponents() != null)
			troopObject.GetParentNormalComponents().groupMovement.ReachedDestination(troopObject);
	}

	public void ResumeCommanderWalk()
	{
		ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, richAI.steeringTarget, 3000f, false);
		ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, richAI.steeringTarget, 3000f, false);
		commanderIsTurning = false;
		if (troopObject.commanderScript.commanderWalkDuringRotation)
		{
			richAI.maxSpeed = myTroop.moveSpeed;
		}
		else
		{
			//if (currentWalkMode != WalkMode.CircleWalk)
			//{
			richAI.enabled = true;
			richAI.canMove = true;
			//if (currentState == STATE.Following)
			//	troopObject.commanderScript.SetAttackInForm(troopObject.attackingSystem.enemyAttackPlayer, false);
			//}
			//else
			//{
			//	currentState = STATE.Following;
			//	currentWalkMode = WalkMode.CircleWalk;
			//	troopObject.commanderScript.hasToWalk = true;
			//}
		}
	}

	public void StartHermitCurve(Vector3 secondPos, Vector3 secondDir)
	{
		bezierCurve = new BezierCurveXZPlane();
		bezierCurve.BezierCurveXZPlaneFromHermitCurve(troopObject.transform.position, troopObject.transform.forward, secondPos, secondDir, false);
		bezierCurve.CalculateBezierCurve(true);
		bezierCurve.StartMove();
		currentState = STATE.Moving;
		currentWalkMode = WalkMode.BezierCurveWalk;
		Vector3 moveDir = bezierCurve.Move(richAI.maxSpeed);
		troopObject.commanderScript.formationObject.transform.rotation = Quaternion.LookRotation(moveDir, Vector3.up);
		troopObject.commanderScript.SetFormation(true);
		bezierCurve.StartMove();
		troopObject.commanderScript.formationObject.transform.rotation = troopObject.transform.rotation;
		//clear Path
		richAI.SetPath(null);
		Vector3[] wayPoints = { bezierCurve.firstPosition, bezierCurve.secondPosition, bezierCurve.thirdPosition, bezierCurve.fourthPosition };
		ServerSend.startPath(wayPoints, true, troopId, true, clientId, bezierCurve.maxDistanceBetweenTwoPoints, richAI.maxSpeed);
	}

	void OnPathReadyNormal(Path p)
	{
		if(troopObject.commanderScript != null)
		{
			ServerSend.startPath(seeker.lastCompletedVectorPath.ToArray(), false, troopId, true, clientId);
		}
		richAI.canMove = true;
		currentState = STATE.Moving;
	}

	void OnPathReadyFollowing(Path p)
	{
		richAI.canMove = true;
		currentState = STATE.Following;
	}

	void waitTillPathIsReady(Path p)
	{
		startTimeTurnCommander = Time.time;
		commanderIsTurning = true;
		if (!troopObject.commanderScript.commanderWalkDuringRotation)
		{
			CheckForSend(SendState.noSend);
			richAI.canMove = false;
			troopObject.commanderScript.prepareForFormation();
		}
		else
		{
			CheckForSend(SendState.walk);
			richAI.canMove = true;
			troopObject.commanderScript.formationObject.transform.rotation = Quaternion.LookRotation(p.vectorPath[1] - troopObject.transform.position, Vector3.up);
			troopObject.commanderScript.SetFormation();
			troopObject.commanderScript.formationObject.transform.rotation = troopObject.transform.rotation;
			float slowestTroop = troopObject.commanderScript.prepareForFormation();
			troopObject.transform.rotation = Quaternion.LookRotation(p.vectorPath[1] - troopObject.transform.position, Vector3.up);
			richAI.maxSpeed = slowestTroop * 0.5f;
		}
		setOneTime = false;
		currentState = STATE.Moving;
		ServerSend.startPath(p.vectorPath.ToArray(), false, troopId, true, clientId);
	}

	IEnumerator<float> waitAfterStartCourutine()
	{
		yield return Timing.WaitForSeconds(0.5f);
		waitAfterStart = true;
	}

	public void StartTruning(Vector3 direction)
	{
		currentState = STATE.Following;
		richAI.steeringTarget = troopObject.transform.position + direction * 10f;
		richAI.canMove = false;
		commanderIsTurning = true;
		troopObject.commanderScript.prepareForFormation();
		setOneTime = false;
		//seeker.StartPath(transform.position ,transform.position + (direction * 10f), OnPathReadyFollowing);
	}

	//public void DestroyObject()
	//{
	//	Destroy(gameObject);
	//}

	public PlayerController() { }
}
