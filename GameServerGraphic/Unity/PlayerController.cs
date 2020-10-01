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

public enum SendState
{
	noSend,
	walk, 
	leftPath,
	toAttackGrid,
	pushed,
	circleWalk
}

public class PlayerController : MonoBehaviour
{
	public bool enabled = true;
	public bool circleWalk;
	public bool tempAttackGrid;
	public bool commanderIsTurning;
	public bool ignoreCommanderTurning;
	public int clientId;
	public int troopId;
	public int formationId = -2;
	public int indexOnAttackGrid = -1;
	public float attackRange;
	public float attackSpeed;
	public float troopRadius;
	public STATE currentState;
	public SendState currentSendState = SendState.noSend;
	public Troops myTroop { get; private set; }
	public Transform transformOnAttackGrid;
	public TroopComponents Mycommander;
	public TroopComponents troopObject;

	const float maxDistanceToAttackGrid = 5f;
	const float walkSendTime = 0.5f;
	const float leftPathSendTime = 0.1f;
	const float toAttackgridSendTime = 0.5f;
	const float pushedSendTime = 0.1f;
	const float circleWalkSendTime = 0.1f;

	public bool isAttacking = false;
	public float factorCircleSide;
	public Vector3 movementDirectionCircle;
	public Vector3 circleMiddlePoint;
	public RichAI richAI;
	//public RVOController RVOController;
	public Seeker seeker;
	public Transform enemyTroop;

	bool waitAfterStart = false;
	bool hasDone;
	bool setOneTime;
	float remainingTime;
	float angleToDirection;
	float lastTimeAttack = 0f;
	object courutineHandle = null;
	Vector3 pointToSendToClient;
	Vector3 lastPosition;
	Vector3 zielPunkt;
	Vector3 dirToWalkIn;
	Vector3 startingPoint;
	Rect3D bounds;
	AttackingSystem attackingSystem;
	Client myClient;


	public override void Start(TroopComponents _troopComponents)
	{
		troopObject = _troopComponents;
		ignoreCommanderTurning = false;
		seeker = troopObject.seeker;
		richAI = troopObject.richAI;
		attackingSystem = troopObject.attackingSystem;
		troopRadius = richAI.radius;
		tempAttackGrid = false;
		hasDone = false;
		commanderIsTurning = false;
		myClient = Server.clients[clientId];
		attackRange += troopRadius;
		Timing.RunCoroutine(waitAfterStartCourutine());
		myTroop = Server.clients[clientId].player.placedTroops[troopId].troop;
		if (Mycommander != null && currentState == STATE.attackGrid || Mycommander != null && Mycommander == troopObject)
			return;

		currentState = STATE.Idle;
		if (troopObject.commanderScript != null)
			Mycommander = troopObject;

		bounds.center = troopObject.transform.position;
		bounds.size =  new Vector3(1.5f, 1.5f, 1.5f);
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
			AttackUpdate();
		}
	}

	private void NormalUpdate()
	{
		if (!commanderIsTurning)
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
				if (richAI.reachedEndOfPath && !circleWalk)
				{
					ReachedEndOfPath();
					return;
				}

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

				remainingTime = Vector3.Distance(pointToSendToClient, troopObject.transform.position) / richAI.desiredVelocity.magnitude;

				////hat den Ursprünglichen Weg verlassem
				//if (angleToDirection > 10f && currentState != STATE.attackGrid && !tempAttackGrid && !agent.pathPending)
				//{
				//	CheckForSend(SendState.leftPath);
				//}
				//else
				//{
				if (circleWalk)
				{
					movementDirectionCircle = circleMiddlePoint - troopObject.transform.position;
					movementDirectionCircle = new Vector3(movementDirectionCircle.z, 0, -movementDirectionCircle.x) * factorCircleSide;
					movementDirectionCircle = movementDirectionCircle.normalized * richAI.maxSpeed;
					//Debug.DrawRay(transform.position, movementDirectionCircle.normalized);
					richAI.Move(movementDirectionCircle * Time.deltaTime);
					richAI.FinalizeRotation(Quaternion.LookRotation(movementDirectionCircle, Vector3.up));
					//Quaternion rotation = Quaternion.LookRotation(movementDirectionCircle, Vector3.up);
					//transform.rotation = rotation;
					Debug.Log("Now Circle Walk");
					CheckForSend(SendState.circleWalk);
				}
				//läuft in die AttackForm
				if (tempAttackGrid)
				{
					if ((troopObject.transform.position - zielPunkt).sqrMagnitude <= richAI.endReachedDistance * richAI.endReachedDistance)
					{
						ReachedEndOfPath();
						return;
					}
					if (transformOnAttackGrid == null)
						return;
					CheckForSend(SendState.toAttackGrid);
					richAI.Move(dirToWalkIn * richAI.maxSpeed * Time.deltaTime);

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
				if (currentState != STATE.attackGrid && !circleWalk && waitAfterStart)
				{
					if ((troopObject.transform.position - lastPosition).sqrMagnitude > 5f)
					{
						CheckForSend(SendState.pushed);
						lastPosition = troopObject.transform.position;
					}
					else if (!circleWalk && currentSendState != SendState.noSend)
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
				setOneTime = true;
				troopObject.commanderScript.SetFormation();
			}
		}
	}

	void AttackUpdate()
	{
		if (enemyTroop == null)
			return;
		if ((troopObject.transform.position - enemyTroop.position).sqrMagnitude <= attackRange * attackRange)
		{
			if (Time.time - lastTimeAttack >= attackSpeed)
			{
				lastTimeAttack = Time.time;
				attackingSystem.AttackInFormationPublic();
			}
		}
		else if ((troopObject.transform.position - transformOnAttackGrid.position).sqrMagnitude < maxDistanceToAttackGrid * maxDistanceToAttackGrid)
		{

			troopObject.transform.Translate((enemyTroop.position - troopObject.transform.position).normalized * 3f * Time.deltaTime, Space.World);
		}
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
		if (courutineHandle == null)
		{
			if(lookingState == SendState.walk)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(walkSendTime)/*.CancelWith(troopObject)*/);
			else if(lookingState == SendState.toAttackGrid)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(toAttackgridSendTime)/*.CancelWith(troopObject)*/);
			else if(lookingState == SendState.leftPath)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(leftPathSendTime)/*.CancelWith(troopObject)*/);
			else if (lookingState == SendState.pushed)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(pushedSendTime)/*.CancelWith(troopObject)*/);
			else if (lookingState == SendState.circleWalk)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(circleWalkSendTime)/*.CancelWith(troopObject)*/);
		}
		else if (currentSendState != lookingState)
		{
			if (courutineHandle != null)
				courutineHandle = Timing.KillCoroutines((CoroutineHandle)courutineHandle);
			if (lookingState == SendState.walk)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(walkSendTime)/*.CancelWith(troopObject)*/);
			else if (lookingState == SendState.toAttackGrid)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(toAttackgridSendTime)/*.CancelWith(troopObject)*/);
			else if (lookingState == SendState.leftPath)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(leftPathSendTime)/*.CancelWith(troopObject)*/);
			else if (lookingState == SendState.pushed)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(pushedSendTime)/*.CancelWith(troopObject)*/);
			else if (lookingState == SendState.circleWalk)
				courutineHandle = Timing.RunCoroutine(sendTroopMove(circleWalkSendTime)/*.CancelWith(troopObject)*/);
		}
	}

	IEnumerator<float> sendTroopMove(float waitTime)
	{
		while (true)
		{
			if (currentSendState == SendState.walk)
			{
				//pointToSendToClient = richAI.steeringTarget - ((richAI.steeringTarget - transform.position).normalized * (richAI.endReachedDistance - (richAI.radius/2)));
				pointToSendToClient = richAI.steeringTarget;
				ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
				ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
			}
			else if (currentSendState == SendState.toAttackGrid)
			{
				//pointToSendToClient = Mycommander.InverseTransformPoint(transformOnAttackGrid.position);
				pointToSendToClient = transformOnAttackGrid.localPosition;
				ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
				ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
			}
			else if(currentSendState == SendState.leftPath)
			{
				ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, richAI.desiredVelocity, remainingTime, true);
				ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, richAI.desiredVelocity, remainingTime, true);
			}
			else if(currentSendState == SendState.pushed)
			{
				ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, Vector3.zero, 1000f, false);
				ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, Vector3.zero, 1000f, false);
			}
			else if(currentSendState == SendState.circleWalk)
			{
				ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, movementDirectionCircle, 10f, true);
				ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, movementDirectionCircle, 10f, true);
			}
			yield return Timing.WaitForSeconds(waitTime);
		}
	}
	#endregion

	public void MoveToPosition(Vector3 position, bool toAttackGrid)
	{
		if (commanderIsTurning)
		{
			return;
		}
		enabled = true;
		richAI.enabled = true;
		hasDone = false;
		startingPoint = troopObject.transform.position;
		if (currentState != STATE.attackGrid || troopObject.commanderScript != null || toAttackGrid)
		{
			if (Vector3.Angle(troopObject.transform.forward, position - troopObject.transform.position) > 16f && troopObject.commanderScript != null && troopObject.commanderScript.attackGrid)
			{
				Transform enemy = myClient.enemyClient.player.FindNearestTroop(troopObject.transform.position).transform;
				if (enemy != null && Vector3.Distance(enemy.position, troopObject.transform.position) < 4f)
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
					if ((troopObject.transform.position - transformOnAttackGrid.position).sqrMagnitude <= richAI.endReachedDistance * richAI.endReachedDistance)
					{
						pointToSendToClient = transformOnAttackGrid.localPosition;
						ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
						ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, pointToSendToClient, richAI.maxSpeed, false);
						troopObject.transform.parent = Mycommander.transform;
						tempAttackGrid = false;
						currentState = STATE.attackGrid;
						//CheckForSend(SendState.noSend);
						richAI.enabled = false;
						Mycommander.commanderScript.childhasReachedGridPoint(troopId, troopObject.transform.position, clientId);
						return;
					}
					tempAttackGrid = true;
					currentState = STATE.Moving;
					troopObject.transform.parent = Mycommander.transform.parent;
					zielPunkt = position;
					dirToWalkIn = (position - troopObject.transform.position).normalized;
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
				troopObject.attackingSystem.enemyAttackPlayer = nearestObject;
				if (troopObject.commanderScript != null && troopObject.commanderScript.attackGrid)
				{
					Debug.Log("OK");
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
		/*if (tempAttackGrid && GetComponentInParent<CommanderScript>() == null)
			return;*/
		Debug.Log("Reached End");
		CheckForSend(SendState.noSend);
		currentState = STATE.Idle;
		richAI.canMove = true;
		if (tempAttackGrid)
		{
			tempAttackGrid = false;
			currentState = STATE.attackGrid;
			CheckForSend(SendState.noSend);
			troopObject.transform.parent = Mycommander.transform;
			richAI.enabled = false;
			Mycommander.commanderScript.childhasReachedGridPoint(troopId, troopObject.transform.position, clientId);
			return;
			//ServerSend.hasReachedDestination(true, troopId, clientId, transform.position, troopId);
		}
		if (troopObject.commanderScript == null)
		{
			foreach (PlacedTroopStruct commander in myClient.player.placedTroops)
			{
				if (commander.troop.klasse.IsFlagSet(TroopClass.Commander))
				{
					CommanderScript comm = commander.gameObject.commanderScript;

					if ((troopObject.transform.position - commander.gameObject.transform.position).sqrMagnitude <= commander.troop.placeRadius * commander.troop.placeRadius && !comm.controlledTroops.Contains(troopObject) && comm.troopObject.playerController.currentState == STATE.Idle)
					{
						int commanderId = comm.troopObject.playerController.troopId;
						myClient.player.SetCommanderChild(commanderId, troopId, true);
						ServerSend.hasReachedDestination(true, troopId, clientId, troopObject.transform.position, commanderId);
						ServerSend.hasReachedDestination(false, troopId, myClient.enemyClient.id, troopObject.transform.position, commanderId);
					}
				}
			}
		}
		ServerSend.hasReachedDestination(true, troopId, clientId, troopObject.transform.position, -1);
		ServerSend.hasReachedDestination(false, troopId, myClient.enemyClient.id, troopObject.transform.position, -1);
		troopObject.GetParentNormalComponents().groupMovement.ReachedDestination(troopObject);
	}

	public void ResumeCommanderWalk()
	{
		ServerSend.troopMove(true, clientId, troopId, troopObject.transform.position, richAI.steeringTarget, 3000f, false);
		ServerSend.troopMove(false, myClient.enemyClient.id, troopId, troopObject.transform.position, richAI.steeringTarget, 3000f, false);
		if (!circleWalk)
		{
			richAI.enabled = true;
			richAI.canMove = true;
			commanderIsTurning = false;
			if (currentState == STATE.Following)
				troopObject.commanderScript.SetAttackInForm(troopObject.attackingSystem.enemyAttackPlayer, false);
		}
		else
		{
			commanderIsTurning = false;
			currentState = STATE.Following;
			circleWalk = true;
			troopObject.commanderScript.hasToWalk = true;
		}
	}

	void OnPathReadyNormal(Path p)
	{
		/*GameObject go = new GameObject();
		go.transform.position = (Vector3)p.path[0].position;*/
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
		CheckForSend(SendState.noSend);
		richAI.canMove = false;
		commanderIsTurning = true;
		troopObject.commanderScript.prepareForFormation();
		setOneTime = false;
		currentState = STATE.Moving;
	}

	IEnumerator<float> waitAfterStartCourutine()
	{
		Debug.Log("asdasuhdo8asgfuidz");
		yield return Timing.WaitForSeconds(0.5f);
		waitAfterStart = true;
		Debug.Log("ASfais");
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
}
