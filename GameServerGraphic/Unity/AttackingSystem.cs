using System.Collections;
using GameServer;
using System.Collections.Generic;
using MEC;
using System;

public class AttackingSystem : MonoBehaviour
{
	public int enemyPlayerHitting;
	public int lineInFormation;
	public float frontLineMinAttackRange;
	public AttackStyle myAttackStyle;
	public TroopComponents enemyAttackPlayer;
	public TroopComponents troopObject;

	public const int attackSearchRange = 25;
	public const int maxPlayerPerChargeAttack = 3;

	protected bool hasToSearch = false;
	protected int troopId;
	protected float lastTime;
	protected float searchSpeed = 0.1f;
	protected PlayerController playerController;
	protected object coroutineHandle = null;

	Client myClient;
	
	public override void Start(TroopComponents _troopObject)
    {
		troopObject = _troopObject;
		playerController = troopObject.playerController;
		myClient = Server.clients[playerController.clientId];
		lastTime = Time.time;
		troopId = playerController.troopId;
		enemyPlayerHitting = 0;
		lineInFormation = -1;
	}

	public virtual void Update()
    {
		if (playerController.currentState != STATE.Following && playerController.currentState != STATE.Hitting && playerController.currentState != STATE.HittingInFormation)
			return;

		/*if (playerController.currentState == STATE.Following && enemyAttackPlayer != null)
		{
			//Wenn die Andere Truppe läuft, dann muss  man ihr nachhlaufen
			if (enemyAttackPlayer.currentState == STATE.Moving || enemyAttackPlayer.currentState == STATE.Following)
			{
				Debug.Log("Now Walk wit him");
				playerController.currentState = STATE.Following;
				playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
				return;
			}
			//wenn man einen einzelnen Spieler nicht in Formation ist, dann einfach angreifen
			if (playerController.currentState != STATE.attackGrid)
			{
				if (Vector3.Distance(transform.position, enemyAttackPlayer.transform.position) <= playerController.agent.stoppingDistance)
				{
					playerController.currentState = STATE.Hitting;
				}
			}
		}*/
		if (playerController.currentState == STATE.Hitting || playerController.currentState ==STATE.HittingInFormation)
		{
			try
			{
				//wenn der enemyTroop weg läuft, müssen wir nachlaufen
				if (enemyAttackPlayer.playerController.currentState == STATE.Moving || enemyAttackPlayer.playerController.currentState == STATE.Following)
				{
					if (playerController.currentState == STATE.Hitting)
					{
						playerController.currentState = STATE.Following;
						playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
					}
					else
					{
						CommanderWalkFormation();
					}
				}
				if (playerController.currentState == STATE.Hitting)
				{
					if(coroutineHandle == null)
						Timing.RunCoroutine(AttackNormalWithoutAttackGrid());
				}
				else //HittingInFormation
				{
					//if(coroutineHandle == null)
					//	coroutineHandle = Timing.RunCoroutine(AttackInFormation());
				}
			}
			catch(NullReferenceException)
			{
				return;
			}
		}
	}

	#region attackTypes
	private IEnumerator<float> AttackNormalWithoutAttackGrid()
	{
		while (true)
		{
			if (enemyAttackPlayer == null)
			{
				Debug.Log("has been destroyed");
				//startedAttacking = false;
				playerController.currentState = STATE.Following;
				if (myClient.enemyClient.player.placedTroops.Count == 0)
				{
					Debug.Log("Stop fight");
					ServerSend.StartFight(playerController.clientId, troopId, false, false);
					playerController.currentState = STATE.Idle;
					Timing.KillCoroutines((CoroutineHandle)coroutineHandle);
					coroutineHandle = null;
					yield return 0f;
				}
				TroopComponents nearestEnemyPlayer = myClient.enemyClient.player.FindNearestTroop(troopObject.transform.position);
				if (Vector3.Distance(troopObject.transform.position, nearestEnemyPlayer.transform.position) <= Get_AttackRange(nearestEnemyPlayer.playerController) + 5f)
				{
					enemyAttackPlayer = nearestEnemyPlayer;
					playerController.MoveToPosition(nearestEnemyPlayer.transform.position, false);
					Debug.Log(enemyAttackPlayer.playerController.troopId);
				}
				else
				{
					Debug.Log("Stop fight");
					ServerSend.StartFight(playerController.clientId, troopId, false, false);
					playerController.currentState = STATE.Idle;
					Timing.KillCoroutines((CoroutineHandle)coroutineHandle);
					coroutineHandle = null;
					yield return 0f;
				}
			}
			if (Vector3.Distance(troopObject.transform.position, enemyAttackPlayer.transform.position) <= Get_AttackRange(enemyAttackPlayer.playerController, true))
			{
				//Debug.Log(Vector3.Distance(transform.position, enemyAttackPlayer.transform.position));
				myClient.player.enemyPlayer.ReduceTroopDamage(enemyAttackPlayer.playerController.troopId, troopId, playerController.myTroop.damage);
			}
			yield return Timing.WaitForSeconds(playerController.attackSpeed);
		}
	}

	private IEnumerator<float> AttackInFormation()
	{
		while (true)
		{
			if (enemyAttackPlayer == null)
			{
				CommanderWalkFormation();
			}
			try
			{
				if (Vector3.Distance(troopObject.transform.position, enemyAttackPlayer.transform.position) <= Get_AttackRange(enemyAttackPlayer.playerController, true))
				{
					//Debug.DrawRay(transform.position, enemyAttackPlayer.transform.position - transform.position, Color.red);
					myClient.player.enemyPlayer.ReduceTroopDamage(enemyAttackPlayer.playerController.troopId, troopId, playerController.myTroop.damage);
				}
				else
				{
					CommanderWalkFormation();
				}
			}
			//catch (MissingReferenceException)
			//{
			//	CommanderWalkFormation();
			//}
			catch(NullReferenceException)
			{
				CommanderWalkFormation();
			}

			yield return Timing.WaitForSeconds(playerController.attackSpeed);
		}
	}

	public void AttackInFormationPublic()
	{
		//PlayerController commander = playerController.Mycommander.GetComponent<PlayerController>();
		//if(commander.transform.position - enemyAttackPlayer)
		//commander.richAI.canMove = false;
		//commander.CheckForSend(SendState.noSend);
		//commander.currentState = STATE.Idle;
		//if((playerController.Mycommander.position - enemyAttackPlayer.transform.position))

		if (enemyAttackPlayer == null)
		{
			CommanderWalkFormation();
		}
		try
		{
			if (Vector3.Distance(troopObject.transform.position, enemyAttackPlayer.transform.position) <= Get_AttackRange(enemyAttackPlayer.playerController, true))
			{
				//Debug.DrawRay(transform.position, enemyAttackPlayer.transform.position - transform.position, Color.red);
				myClient.player.enemyPlayer.ReduceTroopDamage(enemyAttackPlayer.playerController.troopId, troopId, playerController.myTroop.damage);
			}
			else
			{
				CommanderWalkFormation();
			}
		}
		//catch (MissingReferenceException)
		//{
		//	CommanderWalkFormation();
		//}
		catch (NullReferenceException)
		{
			CommanderWalkFormation();
		}
	}

	/// <summary>
	/// wird einmal gecallt, wenn eine gegnerische Truppe zum angreifen gefunden wurde, der Commander wird gestoppt und es wird auf HittingInFormation gesetzt. In der Update Loop wird jetzt das angreifen gestartet
	/// </summary>
	private void AttackNormalTroop(TroopComponents enemyClosest)
	{
		PlayerController commander = playerController.Mycommander.playerController;
		//if (commander.currentState == STATE.Following)
		//{
		//	Debug.Log("OKSD");
		//	commander.richAI.canMove = false;
		//	playerController.CheckForSend(SendState.noSend);
		//	commander.currentState = STATE.Idle;
		//}
		//ServerSend.hasReachedDestination(true, commander.troopId, myClient.id, commander.transform.position, commander.troopId);
		enemyAttackPlayer = enemyClosest;
		playerController.enemyTroop = enemyClosest.transform;
		playerController.currentState = STATE.Following;
		//playerController.currentState = STATE.HittingInFormation;
		//TODO send clients
	}

	/// <summary>
	/// Die Funktion wird einmal gecallt wenn eine gegnerische Truppe gefunden wurde, dann wird in der Update Loop angegriffen
	/// </summary>
	private void AttackCharce(TroopComponents enemyClosest)
	{
		enemyAttackPlayer = enemyClosest;
		Debug.Log("Has To Move");
		playerController.currentState = STATE.Following;
		playerController.MoveToPosition(enemyClosest.transform.position, false);
	}
	#endregion

	//wenn eine Truppe in der AttackGrid einen getötet hat, kann der Commander weiter laufen oder aufhören anzugreifen
	protected void CommanderWalkFormation()
	{
		Debug.Log("CommanderWalkFormation");
		if (coroutineHandle != null)
		{
			Timing.KillCoroutines((CoroutineHandle)coroutineHandle);
			coroutineHandle = null;
		}
		if(myClient == null)
			myClient = Server.clients[playerController.clientId];

		if (myClient.enemyClient.player.placedTroops.Count > 0)
		{
			TroopComponents commander = playerController.Mycommander;
			if(commander.playerController.circleWalk)
			{
				return;
			}
			TroopComponents enemyTroop = myClient.enemyClient.player.FindNearestTroop(troopObject.transform.parent.position);
			if ((commander.transform.position - enemyTroop.transform.position).sqrMagnitude <= (attackSearchRange * attackSearchRange))//wenn die Distanz 15 oder kleiner ist
			{
				if (myAttackStyle == AttackStyle.Charge)
				{
					playerController.currentState = STATE.Following;
				}
				else
				{
					playerController.currentState = STATE.attackGrid;
					commander.attackingSystem.enemyAttackPlayer = enemyTroop;
					commander.commanderScript.SetAttackInForm(enemyTroop, true);
				}
			}
			//wenn keine Truppe in der Nähe ist, dann muss der Commander alle Truppen auf nicht angreifen setzten
			else
			{
				playerController.Mycommander.commanderScript.StopAttack();
				playerController.Mycommander.commanderScript.SetFormation();
			}
		}
		else
		{
			playerController.Mycommander.commanderScript.StopAttack();
			playerController.Mycommander.commanderScript.SetFormation();
		}
	}

	public float Get_AttackRange(PlayerController enemyClosest = null, bool searchOtherTroopRadius = false)
	{
		if (!searchOtherTroopRadius)
			return playerController.attackRange;
		else
		{
			if (enemyClosest == null)
			{
				if (enemyAttackPlayer == null)
					return playerController.attackRange;
				else
					return playerController.attackRange + enemyAttackPlayer.playerController.attackRange;
			}
			else
				return playerController.attackRange + enemyClosest.troopRadius;
		}
	}

	//all 0.2 Sekunden wird geschaut ob ein anderer Spieler in der Nähe ist(nicht jeder Frame!)
	public virtual void StartInvokingRepeat(AttackStyle _myAttackSytle)
	{
		myAttackStyle = _myAttackSytle;
		hasToSearch = true;
		//transform.parent = transform;
		playerController.isAttacking = true;
		Timing.RunCoroutine(searchForEnemyPlayerCourutine().CancelWith(troopObject));
	}

	//aufhören des Schauens nach anderen Truppen
	public void StopRepeat()
	{
		hasToSearch = false;
	}

	protected virtual IEnumerator<float> searchForEnemyPlayerCourutine()
	{
		if (troopObject.commanderScript == null)
			enemyAttackPlayer = null;
		while(hasToSearch)
		{
			if (!(enemyAttackPlayer != null && playerController.currentState == STATE.HittingInFormation))
				SearchForEnemyPlayer();

			yield return Timing.WaitForSeconds(searchSpeed);
		}
	}

	//suche nach EnemyPlayer wenn in Formation und man gegen Truppen läuft
	private void SearchForEnemyPlayer()
	{
		//transform.parent = transform;
		CommanderScript comm = playerController.Mycommander.commanderScript;
		if (enemyAttackPlayer != null && myAttackStyle == AttackStyle.Charge)
		{
			float attackR = Get_AttackRange(enemyAttackPlayer.playerController, true);
			if ((troopObject.transform.position - enemyAttackPlayer.transform.position).sqrMagnitude < (attackR * attackR))
			{
				if (!comm.attackTroops.Contains(troopObject.transform))
					comm.attackTroops.Add(troopObject.transform);
				if(playerController.currentState == STATE.Following)
					ServerSend.hasReachedDestination(true, playerController.troopId, myClient.id, troopObject.transform.position, playerController.troopId);
				playerController.currentState = STATE.HittingInFormation;
			}
			return;
		}
		//int  layerMask = 1 << 8;
		//RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, float.PositiveInfinity, layerMask);
		//if(hits.Length > 0)
		//{
		//	for (int i = 0; i < hits.Length; i++)
		//	{
		//		if()
		//	}
		//}

		//wenn kein Gegner mehr lebt
		TroopComponents enemyClosest = myClient.enemyClient.player.FindNearestTroop(troopObject.transform.position);
		if (enemyClosest == null)
		{
			comm.attackTroops.Clear();
			CommanderWalkFormation();
			return;
		}

		//nomrmal angereifen
		if (myAttackStyle == AttackStyle.Normal)
		{
			//if (lineInFormation == 0)
			//{
				if ((troopObject.transform.position - enemyClosest.transform.position).sqrMagnitude < (attackSearchRange* attackSearchRange))
				{
					if (!comm.attackTroops.Contains(troopObject.transform))
						comm.attackTroops.Add(troopObject.transform);

					AttackNormalTroop(enemyClosest);
				}
				else
					comm.attackTroops.Remove(troopObject.transform);
			//}
			//else
			//{
			//	float attackRange = Get_AttackRange(enemyClosest.GetComponent<PlayerController>(), true);
			//	if ((transform.position - enemyClosest.position).sqrMagnitude < (attackRange * attackRange))
			//	{
			//		if (!comm.attackTroops.Contains(transform))
			//			comm.attackTroops.Add(transform);

			//		AttackNormalTroop(enemyClosest);
			//	}
			//	else
			//		comm.attackTroops.Remove(transform);
			//}
		}

		//angreifen im Charge 
		else if (myAttackStyle == AttackStyle.Charge)
		{
			//es wird in einem Radius von 10 meter gesucht ob noch eine Truppe lebt
			if ((troopObject.transform.position - enemyClosest.transform.position).sqrMagnitude < (attackSearchRange * attackSearchRange))
			{
				//eine Variabel auf deer Truppe zeigt an, von wie vielenTruppen man gerade angegriffen wird.
				if (enemyClosest != enemyAttackPlayer)
				{
					if (enemyAttackPlayer != null && enemyAttackPlayer.attackingSystem.enemyPlayerHitting > 0)
						enemyAttackPlayer.attackingSystem.enemyPlayerHitting--;
					//es dürfen nur immer drei Truppen eine andere Truppe angreifen, dass sie sich verteilen --> es greifen schon 3 an, also muss eine andere Truppe angegriffen werden
					if (enemyClosest.attackingSystem.enemyPlayerHitting >= maxPlayerPerChargeAttack)
					{
						if (enemyClosest.GetParentTroopComponents().commanderScript != null)
						{
							CommanderScript enemyCOmmander = enemyClosest.playerController.Mycommander.commanderScript;
							float distance = float.PositiveInfinity;
							for (int i = 0; i < enemyCOmmander.controlledTroops.Count; i++)
							{
								if (enemyCOmmander.controlledTroops[i].attackingSystem.enemyPlayerHitting < maxPlayerPerChargeAttack)
								{
									float tempDistance = Vector3.Distance(troopObject.transform.position, enemyCOmmander.controlledTroops[i].transform.position);
									if (tempDistance < distance)
									{
										distance = tempDistance;
										enemyClosest = enemyCOmmander.controlledTroops[i];
									}
								}
							}
							if (enemyCOmmander.troopObject.attackingSystem.enemyPlayerHitting < maxPlayerPerChargeAttack && Vector3.Distance(troopObject.transform.position, enemyCOmmander.troopObject.transform.position) < distance)
							{
								Debug.Log("Has choosen Commander");
								enemyClosest = enemyCOmmander.troopObject;
							}
						}
					}
					enemyClosest.attackingSystem.enemyPlayerHitting++;
				}
				AttackCharce(enemyClosest);
			}
			else
			{
				comm.attackTroops.Remove(troopObject.transform);
				CommanderWalkFormation();
			}
		}
	}
}
