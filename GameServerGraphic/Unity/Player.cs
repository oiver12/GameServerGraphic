using System.Collections.Generic;
using GameServer;
using Pathfinding;
using GameServerGraphic;

[System.Serializable]
public class PlaceTroopsStruct
{

	public PlaceTroopsStruct(Troops _troop, int _troopsToPlace, int _troopsPlaced)
	{
		troop = _troop;
		troopsToPlace = _troopsToPlace;
		troopsPlaced = _troopsPlaced;
	}
	public Troops troop;
	public int troopsToPlace;
	public int troopsPlaced;
}

[System.Serializable]
public class PlacedTroopStruct
{
	public PlacedTroopStruct(Troops _troop, int _id, TroopComponents _gameObject, float _health)
	{
		troop = _troop;
		id = _id;
		gameObject = _gameObject;
		health = _health;
	}
	public Troops troop;
	public TroopComponents gameObject;
	public int id;
	public float health;
}

[System.Serializable]
public class Player
{
	public string username;
	public List<PlaceTroopsStruct> placebelTroops = new List<PlaceTroopsStruct>();
	public List<PlacedTroopStruct> placedTroops = new List<PlacedTroopStruct>();
	public int id;
	public Player enemyPlayer;
	public bool isClone;

	KdTree<TroopComponents> troopsKDTree = new KdTree<TroopComponents>();
	//List<Transform> allGroups;
	//KDTree kd = new KDTree();

	public Player(int _id, string _username)
	{
		id = _id;
		username = _username;
		Debug.Log("Player Initialized with " + username);
	}

	//public void InitializePlayer(int _id, string _username)
	//{
	//	id = _id;
	//	username = _username;
	//	Debug.Log("Player Initialized with " + username);
	//}

	public void Update()
	{
		for (int i = 0; i < placedTroops.Count; i++)
		{
			placedTroops[i].gameObject.Update();
		}
	}

	public void PlaceTroop(Troops troop, Vector3 spawnPosition, int troopId, int commanderId, bool sendToClient, bool gedrücktHalten)
	{
		CommanderScript commander = null;
		FormationChild transformOnAttackGrid = null;
		if (!troop.klasse.IsFlagSet(TroopClass.Commander))
			commander = placedTroops[commanderId].gameObject.commanderScript;
		if (!troop.klasse.IsFlagSet(TroopClass.Commander) && placedTroops[commanderId].gameObject.commanderScript.controlledTroops.Count + 1 > placedTroops[commanderId].troop.maxTroops)
		{
			ServerSend.TroopPlace(id, troop, Vector3.zero, 2000, 2000, true);
			return;
		}
		if (gedrücktHalten)
		{
			int placedTroopsInCommander = commander.controlledTroops.Count;
			if (commander.formationObject == null)
				FormationManager.PlaceFormationObjectForCommander(commander.troopObject, 0);

			/*if (placedTroopsInCommander == placedTroops[commanderId].gameObject.GetComponent<CommanderScript>().formationObject.transform.childCount - 1)
			{
				//return weil keine Truppe mehr beim Commander Platz hat
				ServerSend.TroopPlace(id, troop, Vector3.zero, 2000, 2000, true);
				return;
			}*/
			commander.troopObject.playerController.transformOnAttackGrid = commander.formationObject.formationObjects[0];
			commander.troopObject.playerController.transformOnAttackGrid.troopOnFormationChild = commander.troopObject;
			transformOnAttackGrid = commander.formationObject.formationObjects[placedTroopsInCommander + 1];
			spawnPosition = transformOnAttackGrid.transform.position;
		}
		TroopComponents troopObject = NetworkManager.InstantiateTroop(troop, spawnPosition);
		Form1.placedTroops++;
		troopObject.richAI.Teleport(spawnPosition, false);
		//troopObject.richAI.enabled = false;
		//if (troop.klasse.IsFlagSet(TroopClass.Commander))
		//{
		//	if (troop.klasse.IsFlagSet(TroopClass.Archer))
		//	{
		//		//troopObject.gameObject.AddComponent<ArcherCommander>();
		//		troopObject.commanderScript = new ArcherCommander();
		//	}
		//	else
		//	{
		//		//troopObject.gameObject.AddComponent<CommanderScript>();;
		//		troopObject.commanderScript = new CommanderScript();
		//	}
		//	//troopObject.transform.parent = Server.clients[id].playerGameObject.transform;
		//}
		//else
		if (!troop.klasse.IsFlagSet(TroopClass.Commander))
		{
			commander.controlledTroops.Add(troopObject);
			troopObject.transform.parent = commander.troopObject.transform;
		}

		PlayerController playerControllerTroop = troopObject.playerController;
		playerControllerTroop.clientId = id;
		playerControllerTroop.troopId = troopId;
		playerControllerTroop.attackSpeed = troop.attackSpeed;
		playerControllerTroop.attackRange = troop.attackRadius;
		placedTroops.Add(new PlacedTroopStruct(troop, troopId, troopObject, troop.maxHealth));
		troopsKDTree.Add(troopObject);
		playerControllerTroop.Start(troopObject);
		//troopObject.attackingSystem.Start(troopObject);
		troopObject.newAttackSystem.Start(troopObject);
		if (troopObject.commanderScript != null)
			troopObject.commanderScript.Start(troopObject);
		//TODO Check if Player can place
		foreach (PlaceTroopsStruct troopStruct in placebelTroops)
		{
			if (troop == troopStruct.troop)
			{
				if (troopStruct.troopsToPlace > 0)
				{
					troopStruct.troopsPlaced++;
					troopStruct.troopsToPlace--;
					break;
				}
				else
				{
					Debug.Log("Player: " + username + " tried to place more Troops");
					break;
				}
			}
		}
		if(commander != null)
			commander.troopObject.richAI.radius = 0.6f;
		troopObject.richAI.enabled = true;
		if (sendToClient)
		{
			ServerSend.TroopPlace(id, troop, spawnPosition, troopId, commanderId, true);
			ServerSend.TroopPlace(enemyPlayer.id, troop, spawnPosition, troopId, commanderId, false);
		}
		if(gedrücktHalten)
		{
			//Timing.RunCoroutine(waitForLastTroopSet(commanderId));
			troopObject.transform.parent = commander.troopObject.transform;
			playerControllerTroop.Mycommander = commander.troopObject;
			playerControllerTroop.currentWalkMode = WalkMode.InAttackGrid;
			playerControllerTroop.formationId = commander.formationId;
			playerControllerTroop.transformOnAttackGrid = transformOnAttackGrid;
			playerControllerTroop.transformOnAttackGrid.troopOnFormationChild = playerControllerTroop.troopObject;
			playerControllerTroop.currentState = STATE.attackGrid;
			float distance = (playerControllerTroop.troopObject.transform.position - commander.troopObject.transform.position).magnitude;
			commander.SetAgentRadius(distance);
			commander.formationRadius = distance + 1f;
		}
	}

	public void PlaceArmy(int formationId, Troops arnyCommander, List<Troops> armyTroops, Vector3 spawnPosition)
	{
		//Place Commander
		PlaceTroop(arnyCommander, spawnPosition, placedTroops.Count, arnyCommander.id, false, false);
		TroopComponents commanderObject = placedTroops[placedTroops.Count - 1].gameObject;
		commanderObject.playerController.Mycommander = commanderObject;
		int commanderId = placedTroops.Count-1;
		//Instantiate Formation
		FormationObject formation = FormationManager.PlaceFormationObjectForCommander(commanderObject, formationId);
		//Place all Troops
		List<int> ids = new List<int>();
		List<Vector3> positions = new List<Vector3>();
		float distance = 0f;
		for (int i =0; i<armyTroops.Count; i++)
		{
			//Vector3 position = formation.GetChild(i+1).position -offset;
			FormationChild transformOnAttackGrid = formation.formationObjects[i + 1];
			PlaceTroop(armyTroops[i], transformOnAttackGrid.transform.position, placedTroops.Count, commanderId, false, false);
			ids.Add(armyTroops[i].id);
			positions.Add(transformOnAttackGrid.transform.position);
			PlayerController playerControllerTroop = placedTroops[placedTroops.Count - 1].gameObject.playerController;
			playerControllerTroop.troopObject.transform.parent = commanderObject.transform;
			playerControllerTroop.Mycommander = commanderObject;
			playerControllerTroop.currentWalkMode = WalkMode.InAttackGrid;
			playerControllerTroop.formationId = commanderObject.commanderScript.formationId;
			playerControllerTroop.transformOnAttackGrid = transformOnAttackGrid;
			playerControllerTroop.transformOnAttackGrid.troopOnFormationChild = playerControllerTroop.troopObject;
			playerControllerTroop.currentState = STATE.attackGrid;
			float tempDistance = (playerControllerTroop.troopObject.transform.position - commanderObject.transform.position).sqrMagnitude;
			if (tempDistance > distance)
				distance = tempDistance;
		}
		CommanderScript commander = commanderObject.commanderScript;
		commander.formationRadius = distance + 1;
		commander.SetAgentRadius(distance);
		commander.formationHasToStayInLine = true;
		commander.attackGrid = true;
		ServerSend.PlaceArmy(id, arnyCommander.id, spawnPosition, ids, positions, true);
		ServerSend.PlaceArmy(enemyPlayer.id, arnyCommander.id, spawnPosition, ids, positions, false);
	}

	public void SendMove(int troopId, Vector3 position, NormalComponentsObject go, bool lastTroop)
	{
		placedTroops[troopId].gameObject.transform.parent = go.transform;
		go.groupMovement.AddTrop(placedTroops[troopId].gameObject, lastTroop, position);
	}

	public NormalComponentsObject SetNewGroupMovement(int troopId, int length)
	{
		if(placedTroops[troopId].gameObject.GetParentNormalComponents() != null)
		{
			if (placedTroops[troopId].gameObject.transform.parent.childCount == length)
			{
				return placedTroops[troopId].gameObject.GetParentNormalComponents();
			}
			else
			{
				NormalComponentsObject go = new NormalComponentsObject(new Transform(Vector3.zero, Quaternion.Identity), new GroupMovement());
				//go.transform.parent = Server.clients[id].playerGameObject.transform;
				//go.AddComponent<GroupMovement>();
				go.transform.tag = "GroupMovement";
				Debug.Log("IsByNewPlace");
				return go;
			}
		}
		else
		{
			NormalComponentsObject go = new NormalComponentsObject(new Transform(Vector3.zero, Quaternion.Identity), new GroupMovement());
			//go.transform.parent = Server.clients[id].playerGameObject.transform;
			//go.AddComponent<GroupMovement>();
			go.transform.tag = "GroupMovement";
			return go;
		}
	}

	public void SetCommanderChild(int commanderId, int troopId, bool hinein)
	{
		if(hinein)
		{
			placedTroops[troopId].gameObject.playerController.Mycommander = placedTroops[commanderId].gameObject;
			placedTroops[troopId].gameObject.transform.parent = placedTroops[commanderId].gameObject.transform;
			//placedTroops[troopId].gameObject.GetComponent<PlayerController>().enabled = false;
			if(!placedTroops[commanderId].gameObject.commanderScript.controlledTroops.Contains(placedTroops[troopId].gameObject))
			{
				placedTroops[commanderId].gameObject.commanderScript.controlledTroops.Add(placedTroops[troopId].gameObject);
			}
		}
		else
		{
			//placedTroops[troopId].gameObject.transform.parent = Server.clients[id].playerGameObject.transform;
			placedTroops[troopId].gameObject.playerController.enabled = true;
			placedTroops[troopId].gameObject.richAI.enabled = true;
			//wenn man nicht in Formation steht muss die stopping Distanz grösser werden
			//placedTroops[troopId].gameObject.GetComponent<PlayerController>().richAI.endReachedDistance = 2f;
			placedTroops[troopId].gameObject.playerController.currentWalkMode = WalkMode.Normal;
			Debug.Log("Set Comman Child");
			placedTroops[troopId].gameObject.playerController.currentState = STATE.Idle;
			if (placedTroops[commanderId].gameObject.commanderScript.controlledTroops.Contains(placedTroops[troopId].gameObject))
				placedTroops[commanderId].gameObject.commanderScript.controlledTroops.Remove(placedTroops[troopId].gameObject);
		}
	}

	public void SetPositionOfChild(int troopId, Vector3 position)
	{
		//placedTroops[troopId].gameObject.transform.localPosition = position;
	}

	/*public void ReduceEnemyTroopDamage(int troopId, int enemyTroopId, float multiplier)
	{
		float damage = placedTroops[troopId].troop.damage * multiplier;
		enemyPlayer.placedTroops[enemyTroopId].health -= damage;
		if (enemyPlayer.placedTroops[enemyTroopId].health > 0f)
		{
			ServerSend.troopDamage(id, troopId, enemyTroopId, damage, true);
			//ServerSend.troopDamage(enemyPlayer.id, troopId, enemyTroopId, damage, false);
		}
		else
		{
			ServerSend.troopDamage(id, troopId, enemyTroopId, damage, true);
			//ServerSend.troopDamage(enemyPlayer.id, troopId, enemyTroopId, damage, false);
			if (enemyPlayer.placedTroops[enemyTroopId].gameObject.GetComponentInParent<CommanderScript>() != null)
			{
				enemyPlayer.placedTroops[enemyTroopId].gameObject.GetComponentInParent<CommanderScript>().controlledTroops.Remove(enemyPlayer.placedTroops[enemyTroopId].gameObject);
			}
			enemyPlayer.placedTroops[troopId].gameObject.GetComponent<PlayerController>().DestroyObject();
			enemyPlayer.placedTroops.RemoveAt(enemyTroopId);
			enemyPlayer.UpdateTroopId();
			enemyPlayer.troopsKDTree.RemoveAt(troopId);
			enemyPlayer.troopsKDTree.UpdatePositions();
		}

	}*/

	public void ReduceTroopDamage(int troopId, int enemyTroopId, float damage)
	{
		//Truppe ist schon zerstört worden
		if (troopId < 0 || troopId >= placedTroops.Count)
			return;
		PlacedTroopStruct ownTroop = placedTroops[troopId];
		PlacedTroopStruct enemyTroop = enemyPlayer.placedTroops[enemyTroopId];
		float formationMultiplier = 1f;
		bool ownTroopInAttackGrid = ownTroop.gameObject.playerController.currentState == STATE.attackGrid || ownTroop.gameObject.playerController.currentState == STATE.HittingInFormation;
		bool enemyTroopInAttackGrid = enemyTroop.gameObject.playerController.currentState == STATE.attackGrid || enemyTroop.gameObject.playerController.currentState == STATE.HittingInFormation;

		if(ownTroopInAttackGrid && enemyTroopInAttackGrid)
			formationMultiplier = MultiplierManager.getFormationMultiplier(enemyTroop.gameObject.playerController.formationId, ownTroop.gameObject.playerController.formationId);
		else if(ownTroopInAttackGrid && !enemyTroopInAttackGrid)
			formationMultiplier = MultiplierManager.getFormationMultiplier(-1, ownTroop.gameObject.playerController.formationId);
		else if(!ownTroopInAttackGrid && enemyTroopInAttackGrid)
			formationMultiplier = MultiplierManager.getFormationMultiplier(enemyTroop.gameObject.playerController.formationId , - 1);

		float troopMultiplier = MultiplierManager.getTroopMultiplier(enemyTroop.troop.id, ownTroop.troop.id);
		damage *= formationMultiplier * troopMultiplier;
		ownTroop.health -= damage;
		if (ownTroop.health > 0f)
		{
			ServerSend.troopDamage(id, troopId, enemyTroopId, damage, false, false);
			ServerSend.troopDamage(enemyPlayer.id, enemyTroopId, troopId, damage, true, false);
		}
		//Troop is dead
		else
		{
			ServerSend.troopDamage(id, troopId, enemyTroopId, damage, false, true);
			ServerSend.troopDamage(enemyPlayer.id, enemyTroopId, troopId, damage, true, true);
			//deparent all troops to not destroy them
			DestroyTroop(ownTroop);
		}

	}

	public void ReduceTroopDamage(int[] troopIds, int enemyTroopId, float damage)
	{
		//TODO EIGENE TROOP MULTIPLIER NOCH EINFÜHREN
		PlacedTroopStruct ownTroop = placedTroops[troopIds[0]];
		PlacedTroopStruct enemyTroop = enemyPlayer.placedTroops[enemyTroopId];
		float formationMultiplier = 1f;
		bool ownTroopInAttackGrid = ownTroop.gameObject.playerController.currentState == STATE.attackGrid || ownTroop.gameObject.playerController.currentState == STATE.HittingInFormation;
		bool enemyTroopInAttackGrid = enemyTroop.gameObject.playerController.currentState == STATE.attackGrid || enemyTroop.gameObject.playerController.currentState == STATE.HittingInFormation;

		if (ownTroopInAttackGrid && enemyTroopInAttackGrid)
			formationMultiplier = MultiplierManager.getFormationMultiplier(enemyTroop.gameObject.playerController.formationId, ownTroop.gameObject.playerController.formationId);
		else if (ownTroopInAttackGrid && !enemyTroopInAttackGrid)
			formationMultiplier = MultiplierManager.getFormationMultiplier(-1, ownTroop.gameObject.playerController.formationId);
		else if (!ownTroopInAttackGrid && enemyTroopInAttackGrid)
			formationMultiplier = MultiplierManager.getFormationMultiplier(enemyTroop.gameObject.playerController.formationId, -1);

		float troopMultiplier = MultiplierManager.getTroopMultiplier(enemyTroop.troop.id, ownTroop.troop.id);
		damage *= formationMultiplier * troopMultiplier;
		for (int i = 0; i < troopIds.Length; i++)
		{
			if (troopIds[i] < 0 || troopIds[i] >= placedTroops.Count)
			if (troopIds[i] < 0 || troopIds[i] >= placedTroops.Count)
				return;
			ownTroop = placedTroops[troopIds[i]];
			ownTroop.health -= damage;
			//Troop is dead
			if(ownTroop.health <= 0f)
			{
				DestroyTroop(ownTroop);
			}
		}
		ServerSend.troopDamageToGroup(enemyPlayer.id, troopIds, enemyTroopId, damage);
	}

	public void DestroyTroop(PlacedTroopStruct ownTroop)
	{
		//deparent all troops to not destroy them
		if (ownTroop.gameObject.commanderScript != null)
		{
			CommanderScript commander = ownTroop.gameObject.commanderScript;
			for (int y = 0; y < commander.controlledTroops.Count; y++)
			{
				commander.controlledTroops[y].transform.parent = commander.troopObject.transform.parent;
				Debug.Log("Reduce Troop");
				commander.controlledTroops[y].playerController.currentState = STATE.Idle;
				commander.controlledTroops[y].playerController.Mycommander = null;
				commander.controlledTroops[y].playerController.transformOnAttackGrid.troopOnFormationChild = null;
				commander.controlledTroops[y].playerController.transformOnAttackGrid = null;
			}
		}
		if (ownTroop.gameObject.GetParentTroopComponents() != null)
		{
			ownTroop.gameObject.GetParentTroopComponents().commanderScript.controlledTroops.Remove(ownTroop.gameObject);
		}
		int index = ownTroop.gameObject.playerController.troopId;
		placedTroops.RemoveAt(index);
		Form1.RemoveTroop(ownTroop.gameObject.transform);
		ownTroop.gameObject.DestroyObject();
		UpdateTroopId();
		troopsKDTree.RemoveAt(index);
		troopsKDTree.UpdatePositions();
	}

	public void UpdateTroopId()
	{
		for(int i = 0;i < placedTroops.Count; i++)
		{
			placedTroops[i].gameObject.playerController.troopId = i;
		}
	}

	//public void FindAllEnemyTroopGroups()
	//{
	//	GameObject[] allGroupMovements = GameObject.FindGameObjectsWithTag("GroupMovement");
	//}

	public TroopComponents FindNearestTroop(Vector3 position)
	{
		troopsKDTree.UpdatePositions();
		return troopsKDTree.FindClosest(position);
	}

	public void InizializeKdTree()
	{
		troopsKDTree = new KdTree<TroopComponents>();
		for (int i = 0; i < placedTroops.Count; i++)
		{
			troopsKDTree.Add(placedTroops[i].gameObject);
		}
	}

	public void ResetPlayer()
	{
		for (int i = 0; i < placedTroops.Count; i++)
		{
			Form1.RemoveTroop(placedTroops[i].gameObject.transform);
		}
		placedTroops.Clear();
		placebelTroops.Clear();
		troopsKDTree.Clear();
	}

}
