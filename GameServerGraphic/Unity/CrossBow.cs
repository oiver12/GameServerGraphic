using System.Collections.Generic;
using GameServer;
using System;

public class CrossBow
{

	//public enum CrossBowModes
	//{
	//	nearestObject =1,
	//	furstestObject =2,
	//	manuell = 3,
	//}

	//public float CrossBowRadius;
	//public float attackSpeed;
	//float lastTime;
	//int clientId = 1;
	//public int buildingId;
	//public GameObject pfeilPrefab;
	//public float firingAngle;
	//public float gravity;
	//public float damagePerBomb;
	//public CrossBowModes mode;
	//public float bomRadius;
	//bool hasDone = false;
	//public int bombs;
	//public List<Tuple<float, Vector3>> allProjectTiles = new List<Tuple<float, Vector3>>();

 //   // Start is called before the first frame update
 //   public override void Start()
 //   {
	//	lastTime = Time.time;
	//	buildingId = 0;
 //   }

	//// Update is called once per frame
	//void Update()
	//{
	//	if ((Time.time - lastTime) >= attackSpeed)
	//	{
	//		lastTime = Time.time;
	//		Transform objectToFire = null;
	//		float distance = float.PositiveInfinity;
	//		//foreach (PlacedTroopStruct enemy in Server.clients[clientId].enemyClient.player.placedTroops)
	//		foreach (PlacedTroopStruct enemy in Server.clients[clientId].player.placedTroops)
	//		{
	//			float distanceToEnemy = Vector3.Distance(transform.position, enemy.gameObject.position);
	//			if (distanceToEnemy <= CrossBowRadius)
	//			{
	//				if((int)mode == 1)
	//				{
	//					if (distanceToEnemy < distance)
	//						objectToFire = enemy.gameObject;
	//				}
	//				else if((int)mode == 2)
	//				{
	//					if (distance == float.PositiveInfinity || distance < distanceToEnemy)
	//						objectToFire = enemy.gameObject;
	//				}
	//			}
	//		}
	//		if (objectToFire != null)
	//		{
	//			/*GameObject pfeil = Instantiate(pfeilPrefab, transform);
	//			pfeil.transform.position = transform.position;
	//			pfeil.AddComponent<ProjectTile>();
	//			pfeil.GetComponent<ProjectTile>().damage = damagePerBomb;
	//			pfeil.GetComponent<ProjectTile>().bombRadius = bomRadius;
	//			pfeil.GetComponent<ProjectTile>().clientId = clientId;*/
	//			float flightDuration = ServerSend.fireBuilding(clientId, objectToFire.position, transform.position, firingAngle, gravity, buildingId);
	//			allProjectTiles.Add(new Tuple<float, Vector3>(Time.time + flightDuration, objectToFire.position));
	//			allProjectTiles.Sort((x, y) => (x.Item1).CompareTo(y.Item1));
	//			//ServerSend.fireBuilding(clientId, objectToFire, pfeil.transform, firingAngle, gravity, 0);
	//		}
	//	}
	//	if (Server.clients[1].player.id ==1 && !hasDone)
	//	{
	//		Vector3 startPoint = new Vector3(-5f, 2f, -21.5f);
	//		for (int i = 0; i < bombs; i++)
	//		{
	//			//ServerSend.fireBuilding(clientId, startPoint, transform.position, firingAngle, gravity, buildingId);
	//			float flightDuration = ServerSend.fireBuilding(clientId, startPoint, transform.position, firingAngle, gravity, buildingId);
	//			allProjectTiles.Add(new Tuple<float, Vector3>(Time.time + flightDuration, startPoint));
	//			allProjectTiles.Sort((x, y) => (x.Item1).CompareTo(y.Item1));
	//			startPoint += new Vector3(2f, 0f, 0f);
	//			//ServerSend.fireBuilding(clientId, objectToFire, pfeil.transform, firingAngle, gravity, 0);
	//		}
	//		hasDone = true;
	//	}
	//	if(allProjectTiles.Count > 0)
	//	{
	//		if ((allProjectTiles[0].Item1 - Time.time <= 0f))
	//		{
	//			allProjectTiles.RemoveAt(0);
	//			foreach (PlacedTroopStruct enemy in Server.clients[clientId].player.placedTroops)
	//			{
	//				float DistanceToEnemy = Vector3.Distance(allProjectTiles[0].Item2, enemy.gameObject.position);
	//				if (DistanceToEnemy <= bomRadius)
	//				{
	//					Server.clients[clientId].player.ReduceTroopDamage(enemy.id, -1, (1f / (DistanceToEnemy + 1)) * damagePerBomb);
	//					if (enemy.health - damagePerBomb <= 0)
	//						return;
	//				}
	//			}

	//		}
	//	}
	//}
}
