using System.Collections.Generic;
using GameServer;
using MEC;

public class ArcherAttack : AttackingSystem
{
	Client myClient;
	Transform parent;
	float preRadius;
	bool hasToNotDO = false;
	Vector3 archerShootPosition;

	private void Start()
	{
		playerController = troopObject.playerController;
		myClient = Server.clients[playerController.clientId];
	}

	// Update is called once per frame
	public override void Update()
    {
		CheckForArcherSplitUp();
		if (playerController.commanderIsTurning)
			return;
		if (playerController.currentState == STATE.HittingInFormation && coroutineHandle == null)
		{
			Debug.Log("Done");
			coroutineHandle = Timing.RunCoroutine(ShootAllArcher());
		}
			
	}

	public override void StartInvokingRepeat(AttackStyle _myAttackSytle)
	{
		Debug.Log("StartInvokReapeat");
		myAttackStyle = _myAttackSytle;
		hasToSearch = true;
		archerShootPosition = enemyAttackPlayer.transform.position;
		Timing.RunCoroutine(searchForEnemyPlayerCourutine()/*.CancelWith(troopObject)*/);
		playerController.currentState = STATE.HittingInFormation;
	}

	protected override IEnumerator<float> searchForEnemyPlayerCourutine()
	{
		while (hasToSearch)
		{
			SearchForArcherAttack();
			yield return Timing.WaitForSeconds(searchSpeed);
		}
	}

	IEnumerator<float> ShootAllArcher()
	{
		while (true)
		{
			try
			{
				CommanderScript comm = playerController.Mycommander.commanderScript;
				ServerSend.fightArcher(playerController.clientId, enemyAttackPlayer.transform.position, troopObject.transform.position, 45f, 15f, troopId, true);
				//Collider[] colliders = Physics.OverlapSphere(enemyAttackPlayer.transform.position, GetComponent<CommanderScript>().formationRadius, LayerMask.GetMask("Player"));
				TroopComponents[] colliders = new TroopComponents[2];
				List<int> ids = new List<int>();
				for (int i = 0; i < colliders.Length; i++)
				{
					if (colliders[i].playerController.clientId != playerController.clientId)
					{
						ids.Add(colliders[i].playerController.troopId);
					}
				}
				myClient.player.enemyPlayer.ReduceTroopDamage(ids.ToArray(), troopId, playerController.myTroop.damage);
			}
			//catch (MissingReferenceException)
			//{
				
			//}
			catch(System.NullReferenceException)
			{

			}
			yield return Timing.WaitForSeconds(playerController.attackSpeed);
		}
	} 

	private void SearchForArcherAttack()
	{
		if (enemyAttackPlayer == null)
		{
			TroopComponents enemyClosest = myClient.enemyClient.player.FindNearestTroop(archerShootPosition);
			if (enemyClosest == null)
			{
				troopObject.commanderScript.attackTroops.Clear();
				CommanderWalkFormation();
				return;
			}
			if ((archerShootPosition - enemyClosest.transform.position).sqrMagnitude <= attackSearchRange * attackSearchRange)
			{
				enemyAttackPlayer = enemyClosest;
				archerShootPosition = enemyClosest.transform.position;

				//GameObject go = new GameObject();
				//go.transform.position = archerShootPosition;
			}
			else
			{
				Debug.Log(Vector3.Distance(archerShootPosition, enemyClosest.transform.position));
			}
		}
	}

	void CheckForArcherSplitUp()
	{
		//wenn man einen Archer ist und im AttackGrid und jemand angreift, dann muss man in Reichweite stehen bleiben
		if (playerController.currentState == STATE.attackGrid && enemyAttackPlayer != null && playerController.myTroop.klasse.IsFlagSet(TroopClass.Archer) && !playerController.Mycommander.playerController.myTroop.klasse.IsFlagSet(TroopClass.Archer))
		{
			//Das der Archer aus der Formation läuft Set up
			if (Vector3.Distance(troopObject.transform.position, enemyAttackPlayer.transform.position) <= playerController.attackRange + 4f)
			{
				parent = troopObject.transform.parent;
				parent.troopObject.commanderScript.controlledTroops.Remove(troopObject);
				troopObject.transform.parent = parent.parent;
				troopObject.richAI.enabled = true;
				preRadius = parent.troopObject.richAI.radius;
				parent.troopObject.richAI.radius = 1.5f;
				hasToNotDO = true;
				//GetComponent<PlayerController>().RVOController.priority = 0.1f;
				troopObject.playerController.tempAttackGrid = false;
				playerController.currentState = STATE.Following;
				//playerController.richAI.endReachedDistance = playerController.myTroop.attackRadius;
				playerController.MoveToPosition(enemyAttackPlayer.transform.position, false);
				ServerSend.StartFight(playerController.clientId, troopId, true, false);
			}
		}
		//Wenn der Archer nochh in dem Radius vom Kommander steht, dann muss der Kommander Radius kleiner sein, dass er nicht veggeschoben wird.
		if (hasToNotDO)
		{
			//wenn der  Archer wieder genug weg vom Commander ist, dann kann der Commander seinen ursprünglichen Radius annehemen
			if (Vector3.Distance(troopObject.transform.position, parent.position) >= preRadius)
			{
				hasToNotDO = false;
				parent.troopObject.richAI.radius = preRadius;
			}
			else
				parent.troopObject.richAI.radius = 1.5f;
		}

	}
}
