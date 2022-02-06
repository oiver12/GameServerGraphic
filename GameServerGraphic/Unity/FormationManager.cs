using System.Collections;
using System.Collections.Generic;
using GameServer;
using System;
using System.Diagnostics;

public static class FormationManager
{
	//public static FormationManager instance;
	public static FormationSpecsTable[] formations;

 //   // Start is called before the first frame update
 //   void Start()
 //   {
	//	Matrix4x4 matrix = new Matrix4x4(new Vector4(5f, 6f, 3f, 5f), new Vector4(3f, 5f, 7f, 1f), new Vector4(3f, 6f, 1f, 2f), new Vector4(5f, 0f, 1f, 2f));
	//	Debug.Log(matrix.MultiplyPoint(new Vector3(1f, 5f, 6f)));
	//	instance = this;
	//}

	public static float SetFormation(int id, List<TroopComponents> troops, TroopComponents commander, int clientId, bool hasToStayInLine, bool toNewAttackGrid = false)
	{
		float distance = 0f;
		commander.richAI.radius = 0.5f;
		commander.commanderScript.formationId = id;
		//das erst mal Formation setzten
		//TODO nicht deepClone, nur referenz benutzten
		if (!commander.commanderScript.attackGrid)
		{
			//GameObject formation = Instantiate(formations[id].formationObject, Server.clients[clientId].playerGameObject.transform);
			//TODO get Prefabs from Unity
			FormationObject formation = formations[id].formationObject.Copy();
			formation.transform.name = id.ToString();
			formation.transform.position = commander.transform.position;
			formation.transform.parent = commander.transform;
			formation.transform.rotation = commander.transform.rotation;
			commander.playerController.formationId = id;
			commander.commanderScript.childReachedPositionCount = 0;

			if (hasToStayInLine)
				distance = MakeFormationStayInLine(formation, troops, commander, id);
			else if (toNewAttackGrid)
				distance = MakeFormationToNewAttackGrid(formation, troops, commander, id);
			else
				distance = MakeFormation(formation, troops, commander, id);
			commander.commanderScript.formationObject = formation;
			commander.commanderScript.attackGrid = true;
		}
		else
		{
			//die gleiche Formation noch mal setzten --> FormationObject kann bleiben
			if (commander.commanderScript.formationObject.transform.name.Contains(id.ToString()) || toNewAttackGrid)
			{
				FormationObject formation = commander.commanderScript.formationObject;
				commander.commanderScript.childReachedPositionCount = 0;
				//if(!toNewAttackGrid)
				//	formation.transform.rotation = commander.transform.rotation;

				if (hasToStayInLine)
					distance = MakeFormationStayInLine(formation, troops, commander, id);
				else if (toNewAttackGrid)
					distance = MakeFormationToNewAttackGrid(formation, troops, commander, id);
				else
					distance = MakeFormation(formation, troops, commander, id);
			}
			//neue Formation, also neue Formation setzten
			else
			{
				//Destroy(commander.GetComponent<CommanderScript>().formationObject);
				//GameObject formation = Instantiate(formations[id].formationObject, Server.clients[clientId].playerGameObject.transform);
				FormationObject formation = formations[id].formationObject.Copy();
				formation.transform.name = id.ToString();
				formation.transform.position = commander.transform.position;
				formation.transform.parent = commander.transform;
				formation.transform.rotation = commander.transform.rotation;
				commander.playerController.formationId = id;
				commander.commanderScript.childReachedPositionCount = 0;

				if (hasToStayInLine)
					distance = MakeFormationStayInLine(formation, troops, commander, id);
				else
					distance = MakeFormation(formation, troops, commander, id);

				commander.commanderScript.formationObject = formation;
				commander.commanderScript.attackGrid = true;
			}
		}
		commander.playerController.transformOnAttackGrid = commander.commanderScript.formationObject.formationObjects[0];
		commander.playerController.transformOnAttackGrid.troopOnFormationChild = commander;
		commander.commanderScript.formationRadius = distance + 1;
		ServerSend.SetInAttackForm(clientId, commander.playerController.troopId, commander.commanderScript.formationObject.transform.rotation, distance + 1, true, commander.commanderScript.commanderWalkDuringRotation);
		ServerSend.SetInAttackForm(Server.clients[clientId].enemyClient.id, commander.playerController.troopId, commander.commanderScript.formationObject.transform.rotation, distance + 1, false, commander.commanderScript.commanderWalkDuringRotation);
		return distance;
	}


	//TODO refacture this is schmutz
	private static float MakeFormation(FormationObject formation, List<TroopComponents> troops, TroopComponents commander, int id)
	{
		float distance = 0f;
		KdTree<FormationChild> allChildren = new KdTree<FormationChild>();
		for (int i = 0; i < formation.formationObjects.Length; i++)
		{
			float tempDistance = (formation.formationObjects[0].transform.position - formation.formationObjects[i].transform.position).sqrMagnitude;
			if (tempDistance > distance)
				distance = tempDistance;
			//allChildren.Add(formation.transform.GetChild(i).position);
			//bekommen von allen children in die Liste, jedoch auch noch alle equivalenten Punkte bekommen.
			allChildren.Add(formation.formationObjects[i]);
			if (i == troops.Count)
			{
				for (int y = 1; y < 5; y++)
				{
					if (i + y >= formation.formationObjects.Length)
						break;
					if (formation.formationObjects[i].transform.name.Contains(formation.formationObjects[i].transform.name))
					{
						allChildren.Add(formation.formationObjects[i]);
					}
					else
						break;
				}
				break;
			}
		}
		allChildren.RemoveAt(0);
		allChildren.UpdatePositions();
		int lines = GetLinesCount(id, troops.Count);
		commander.playerController.lineInFormation = GetLine(formation.formationObjects[0], lines);
		for (int i = 0; i < troops.Count; i++)
		{
			PlayerController playerControllerTroop = troops[i].playerController;
			playerControllerTroop.Mycommander = commander;
			playerControllerTroop.currentWalkMode = WalkMode.InAttackGrid;
			playerControllerTroop.formationId = id;
			//playerControllerTroop.richAI.endReachedDistance = 0.2f;
			Transform nearestObject = allChildren.FindClosest(troops[i].transform.position).transform;
			//GameServerGraphic.Form1.SpawnPointAt(nearestObject.position, System.Drawing.Color.Blue, 10);
			int index = allChildren.ToList().FindIndex(a => a.transform == nearestObject);
			//bekommen von Zahl, auf welchem  die Truppe steht
			int line = GetLine(allChildren[index], lines);
			playerControllerTroop.transformOnAttackGrid = allChildren[index];
			playerControllerTroop.transformOnAttackGrid.troopOnFormationChild = playerControllerTroop.troopObject;
			troops[i].playerController.lineInFormation = line;
			//playerControllerTroop.indexOnAttackGrid = nearestObject.GetSiblingIndex();
			allChildren.RemoveAt(index);
			allChildren.UpdatePositions();
			if (!commander.commanderScript.commanderWalkDuringRotation)
			{
				troops[i].transform.parent = commander.transform;
				playerControllerTroop.MoveToPosition(nearestObject.position, true);
			}
			else
			{
				playerControllerTroop.currentState = STATE.Moving;
				troops[i].richAI.enabled = true;
				troops[i].richAI.canMove = true;
			}
		}
		return Mathf.Sqrt(distance);
	}

	public static float MakeFormationToNewAttackGrid(FormationObject formation, List<TroopComponents> troops, TroopComponents commander, int id)
	{
		float distance = 0f;
		KdTree<FormationChild> allChildren = new KdTree<FormationChild>();
		for (int i = 1; i < formation.formationObjects.Length; i++)
		{
			float tempDistance = (formation.formationObjects[0].transform.position - formation.formationObjects[i].transform.position).sqrMagnitude;
			if (tempDistance > distance)
				distance = tempDistance;

			allChildren.Add(formation.formationObjects[i]);
		}
		allChildren.UpdatePositions();
		int lines = GetLinesCount(id, troops.Count);
		commander.playerController.lineInFormation = GetLine(formation.formationObjects[0], lines);
		for (int i = 0; i < troops.Count; i++)
		{
			//troops[i].transform.parent = commander.transform;
			PlayerController playerControllerTroop = troops[i].playerController;
			playerControllerTroop.Mycommander = commander;
			playerControllerTroop.currentWalkMode = WalkMode.InNewBoxAttackGrid;	
			playerControllerTroop.formationId = id;
			playerControllerTroop.currentState = STATE.Moving;
			troops[i].richAI.enabled = true;
			troops[i].richAI.canMove = true;
			Transform nearestObject = allChildren.FindClosest(troops[i].transform.position).transform;
			//GameServerGraphic.Form1.SpawnPointAt(nearestObject.position, System.Drawing.Color.Blue, 10);
			int index = allChildren.ToList().FindIndex(a => a.transform == nearestObject);
			int line = GetLine(allChildren[index], lines);
			playerControllerTroop.transformOnAttackGrid = allChildren[index];
			playerControllerTroop.transformOnAttackGrid.troopOnFormationChild = playerControllerTroop.troopObject;
			allChildren.RemoveAt(index);
			allChildren.UpdatePositions();
		}
		commander.playerController.ignoreCommanderTurning = true;
		//commander.playerController.MoveToPosition(formation.formationObjects[0].transform.position, false);
		return distance;
	}

	private static float MakeFormationStayInLine(FormationObject formation, List<TroopComponents> troops, TroopComponents commander, int id)
	{
		float distance = 0f;
		/*KdTree<Transform>[] allchildren = new KdTree<Transform>[formations[id].frontLines.Length];
		for(int i = 0; i < formations[id].frontLines.Length; i++)
		{
			for(int j = 0; j < formations[id].frontLines[i].linesindexes.Length; j++)
			{
				int indexer = formations[id].frontLines[i].linesindexes[j];
				if (indexer < formation.transform.childCount)
				{

				}
			}
		}*/
		for (int i = 0; i < troops.Count; i++)
		{
			float tempDistance = (formation.transform.GetChild(0).position - formation.transform.GetChild(i).position).sqrMagnitude;
			if (tempDistance > distance)
				distance = tempDistance;
			troops[i].transform.parent = commander.transform;
			PlayerController playerControllerTroop = troops[i].playerController;
			playerControllerTroop.Mycommander = commander;
			playerControllerTroop.currentWalkMode = WalkMode.InAttackGrid;
			playerControllerTroop.formationId = id;
			//playerControllerTroop.richAI.endReachedDistance = 0.2f;
			FormationChild nearestObject = playerControllerTroop.transformOnAttackGrid;
			//bekommen von Zahl, auf welchem  die Truppe steht
			playerControllerTroop.transformOnAttackGrid = nearestObject;
			playerControllerTroop.transformOnAttackGrid.troopOnFormationChild = playerControllerTroop.troopObject;
			playerControllerTroop.MoveToPosition(nearestObject.transform.position, true);
		}
		return Mathf.Sqrt(distance);
	}

	public static FormationObject PlaceFormationObjectForCommander(TroopComponents commander, int id)
	{
		FormationObject formation = formations[id].formationObject.Copy();
		formation.transform.parent = commander.transform;
		//GameObject formation = Instantiate(formations[id].formationObject, commander);
		formation.transform.name = id.ToString();
		formation.transform.position = commander.transform.position;
		formation.transform.rotation = commander.transform.rotation;
		commander.playerController.formationId = id;
		commander.commanderScript.formationObject = formation;
		commander.commanderScript.attackGrid = true;
		return formation;
	}

	///// <summary>
	///// auf welcher vorderer Linie die Truppe steht. Denn das ist wichtig, dass die zweite Reihe richtig angreift
	///// </summary>
	public static int GetLine(FormationChild transformFromGridTile, int lines)
	{
		//for (int i = 0; i < formations[formationId].lineFormationObject.transform.childCount; i++)
		//{
		//	if (transformFromGridTile.localPosition == formations[formationId].lineFormationObject.transform.GetChild(i).localPosition)
		//	{
		//		int myLine = int.Parse(formations[formationId].lineFormationObject.transform.GetChild(i).name);
		//		return lines - myLine;
		//	}
		//}
		//return -4;
		int myLine = transformFromGridTile.line - lines;
		return myLine;
	}

	public static int GetLinesCount(int formationId, int controlledTroopsCount)
	{
		int[] LineOrder = formations[formationId].LineOrder;
		Lines[] allLines = formations[formationId].frontLines;
		int lastLine = formations[formationId].LineOrder[formations[formationId].LineOrder.Length-1] -1;
		for (int i = 0; i < formations[formationId].LineOrder.Length; i++)
		{
			if (formations[formationId].frontLines[formations[formationId].LineOrder[i] - 1].lineStart > controlledTroopsCount)
			{
				lastLine = formations[formationId].LineOrder[i - 1] - 1;
				break;
			}
		}
		return formations[formationId].frontLines[lastLine].linesInFrontEmpty;
	}
}
