using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GameServer;

class ServerHandle
{
	public static void Login(int _fromClient, Packet _packet)
	{
		int _clientIdCheck = _packet.ReadInt();
		string _username = _packet.ReadString();
		string _password = _packet.ReadString();
		bool isClone = _packet.ReadBool();

		Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
		if (_fromClient != _clientIdCheck)
		{
			Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
		}

		if (!Database.AccountExist(_username))
		{
			ServerSend.succesfullyLogedIn(_fromClient, false);
			return;
		}
		if (!Database.PasswordOK(_username, _password))
		{
			ServerSend.succesfullyLogedIn(_fromClient, false);
			return;
		}

		Debug.Log("Player " + _username + " succefully logged into his account");
		ServerSend.succesfullyLogedIn(_fromClient, true);
		Server.clients[_fromClient].SendIntoGame(_username);
		Server.clients[_fromClient].player.isClone = isClone;

	}
	public static void Register(int _fromClient, Packet _packet)
	{
		int _clientIdCheck = _packet.ReadInt();
		string _username = _packet.ReadString();
		string _password = _packet.ReadString();

		Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
		if (_fromClient != _clientIdCheck)
		{
			Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
		}

		if (Database.AccountExist(_username))
		{
			ServerSend.succesfullyLogedIn(_fromClient, false);
			return;
		}

		Database.NewAccount(_username, _password);
		ServerSend.succesfullyLogedIn(_fromClient, true);
		Server.clients[_fromClient].SendIntoGame(_username);
	}

	public static void PlacebelTroops(int _fromClient, Packet _packet)
	{
		int length = _packet.ReadInt();
		for (int i = 0; i < length; i++)
		{
			int id = _packet.ReadInt();
			/*foreach (Troops troop in Server.allTroops)
			{
				if (id == troop.id)
				{
					int troopsToPlace = _packet.ReadInt();
					Server.clients[_fromClient].player.placebelTroops.Add(new PlaceTroopsStruct(troop, troopsToPlace, 0));
				}
			}*/
			int troopsToPlace = _packet.ReadInt();
			Server.clients[_fromClient].player.placebelTroops.Add(new PlaceTroopsStruct(Server.allTroops[id], troopsToPlace, 0));
		}
	}

	public static void PlaceTroop(int _fromClient, Packet _packet)
	{
		int id = _packet.ReadInt();
		/*foreach (PlaceTroopsStruct troopStruct in Server.clients[_fromClient].player.placebelTroops)
		{
			if (id == troopStruct.troop.id)
			{
				Vector3 spawnPosition = _packet.ReadVector3();
				int troopid = _packet.ReadInt();
				int commanderid = _packet.ReadInt();
				Server.clients[_fromClient].player.PlaceTroop(troopStruct.troop, spawnPosition, troopid, commanderid);
				break;
			}
		}*/
		Vector3 spawnPosition = _packet.ReadVector3();
		int troopid = _packet.ReadInt();
		int commanderid = _packet.ReadInt();
		bool gedrücktHalten = _packet.ReadBool();
		Server.clients[_fromClient].player.PlaceTroop(Server.allTroops[id], spawnPosition, troopid, commanderid, true, gedrücktHalten);
	}

	public static void TroopMove(int _fromClient, Packet _packet)
	{
		//Vector3 toPosition = _packet.ReadVector3();
		//int id = _packet.ReadInt();
		int lenght = _packet.ReadInt();
		Vector3 toPosition = _packet.ReadVector3();
		//GameObject go= Server.clients[_fromClient].player.SetNewGroupMovement();
		NormalComponentsObject go = null;
		for(int i=0;i<lenght;i++)
		{
			int id = _packet.ReadInt();
			if (i == 0)
				go = Server.clients[_fromClient].player.SetNewGroupMovement(id, lenght);
			if (i < lenght -1)
				Server.clients[_fromClient].player.SendMove(id, toPosition, go, false);
			else
				Server.clients[_fromClient].player.SendMove(id, toPosition, go, true);
		}
		//Server.clients[_fromClient].player.placedTroops[id].position = position;
		//ServerSend.enemyTroopMove(Server.clients[_fromClient].enemyClient.id, id, position);
	}

	public static void ClientSearchForMatch(int _fromClient, Packet _packet)
	{
		bool isAttacking = _packet.ReadBool();
		Server.clients[_fromClient].isAttacker = isAttacking;
		Matchmaking.clientSerachingFroMatch.Add(Server.clients[_fromClient]);
		Debug.Log("Player " + _fromClient +" is searching for a Match");
	}

	public static void SetCommanderChild(int _fromClient, Packet _packet)
	{
		int commanderId = _packet.ReadInt();
		int troopId = _packet.ReadInt();
		bool hinein = _packet.ReadBool();
		Server.clients[_fromClient].player.SetCommanderChild(commanderId, troopId, hinein);
	}

	public static void setPositionFromTroop(int _fromClient, Packet _packet)
	{
		int troopId = _packet.ReadInt();
		Vector3 position = _packet.ReadVector3();
		Server.clients[_fromClient].player.SetPositionOfChild(troopId, position);
	}

	public static void setAttack(int _fromClient, Packet _packet)
	{
		int formationId = _packet.ReadInt();
		int attackType = _packet.ReadInt();
		int commanderId = _packet.ReadInt();
		Server.clients[_fromClient].player.placedTroops[commanderId].gameObject.commanderScript.SetFormation(formationId, attackType);
	}

	public static void PlaceArmy(int _fromClient, Packet _packet)
	{
		int formationId = _packet.ReadInt();
		Troops armyCommander = Server.allTroops[_packet.ReadInt()];
		Vector3 position = _packet.ReadVector3();
		int troopCount = _packet.ReadInt();
		List<Troops> armyTroops = new List<Troops>();
		for(int i = 0; i < troopCount; i++)
		{
			armyTroops.Add(Server.allTroops[_packet.ReadInt()]);
		}
		Server.clients[_fromClient].player.PlaceArmy(formationId, armyCommander, armyTroops, position);
	}

	public static void ExceptionFromClient(int _fromClient, Packet _packet)
	{
		bool isException = _packet.ReadBool();
		string condition = _packet.ReadString();
		string stackTrace = _packet.ReadString();
		if (!isException)
			Debug.LogError("User with id " + _fromClient + "had the ErrorMessage \n" + condition + stackTrace);
		else
			Debug.LogError("User with id " + _fromClient + "had the Excpetion \n" + condition + stackTrace);
	}

	public static void MoveToNewGrid(int _fromClient, Packet _packet)
	{
		int lengthArmys = _packet.ReadInt();
		int[] comanderIds = new int[lengthArmys];
		for (int i = 0; i < lengthArmys; i++)
		{
			comanderIds[i] = _packet.ReadInt();
		}
		Vector3 startPoint = _packet.ReadVector3();
		Vector3 widthDir = _packet.ReadVector3();
		float deltaX = _packet.ReadFloat();
		float gapBetweenArmys = _packet.ReadFloat();
		//GameServerGraphic.Form1.SpawnPointAt(startPoint, System.Drawing.Color.Blue, 7);
		//for (int i = 0; i < comanderIds.Length; i++)
		//{ 
		//	Server.clients[_fromClient].player.placedTroops[comanderIds[i]].gameObject.commanderScript.MakeAttackGrid(widths[i], lengths[i], startPoint, widthDir, deltaX);
		//	startPoint = startPoint - widthDir * (widths[i] * deltaX + gapBetweenArmys);
		//}
		int[] amountUnits = new int[lengthArmys];
		int amountUnitsComplet = 0;
		for (int i = 0; i < comanderIds.Length; i++)
		{
			amountUnits[i] = Server.clients[_fromClient].player.placedTroops[comanderIds[i]].gameObject.commanderScript.controlledTroops.Count + 1;
			amountUnitsComplet += amountUnits[i];
		}

		float distanceSoFar = 0;
		float widthLineMagnitude = widthDir.magnitude;
		for (int comanderNumber = 0; comanderNumber < comanderIds.Length; comanderNumber++)
		{
			CommanderScript comander = Server.clients[_fromClient].player.placedTroops[comanderIds[comanderNumber]].gameObject.commanderScript;
			float distance = (widthLineMagnitude - gapBetweenArmys * (comanderIds.Length - 1)) * ((float)amountUnits[comanderNumber] / (float)amountUnitsComplet);
			distance = Mathf.Max(distance, deltaX * 2);
			distanceSoFar += distance;
			int actuelAmountWidth = Mathf.FloorToInt(distance / deltaX);
			actuelAmountWidth = Mathf.Clamp(actuelAmountWidth, 2, amountUnits[comanderNumber]);
			int actuelAmountLenght = Mathf.CeilToInt((float)amountUnits[comanderNumber] / (float)actuelAmountWidth);
			int rest = amountUnits[comanderNumber] - actuelAmountWidth * (actuelAmountLenght - 1);
			Vector3 lineLength = (Quaternion.Euler(0f, 90f, 0f) * widthDir).normalized;
			widthDir = widthDir.normalized;
			comander.formationObject.formationObjects = new FormationChild[amountUnits[comanderNumber]];
			comander.formationObject.transform.rotation = Quaternion.LookRotation(lineLength, Vector3.up);
			comander.formationObject.transform.name = comander.formationId.ToString();
			int commanderPlace = ((actuelAmountLenght - 2) / 2 *actuelAmountWidth + rest) + actuelAmountWidth / 2;
			for (int i = 0; i < actuelAmountLenght; i++)
			{
				//nach hinten verscheiben
				Vector3 pointOnColumns = startPoint - lineLength * deltaX * (actuelAmountLenght - i);
				pointOnColumns = pointOnColumns + widthDir * (widthLineMagnitude - distanceSoFar);
				int offset = 0;
				if (i == 0)
				{
					offset = (int)(actuelAmountWidth / 2f) - (int)((rest) / 2f);
				}
				for (int y = 0; y < actuelAmountWidth; y++)
				{
					if (i == 0 && y > rest - 1)
						break;

					int index = ((i - 1) * actuelAmountWidth) + y + rest;
					if (i == 0)
						index = y;

					Vector3 pointOnRow = pointOnColumns + widthDir * deltaX * (y + offset);
					if (index < commanderPlace)
						index += 1;
					else if (index == commanderPlace)
					{
						index = 0;
						comander.formationObject.transform.MoveWithoutChilds(pointOnRow);
					}
					//GameServerGraphic.Form1.SpawnPointAt(pointOnRow, System.Drawing.Color.Red, 5);
					comander.formationObject.formationObjects[index] = new FormationChild(new Transform(pointOnRow, Quaternion.Identity, comander.formationObject.transform), actuelAmountLenght - i, null, null);
				}
			}
			for (int i = 0; i < comander.formationObject.formationObjects.Length; i++)
			{
				comander.formationObject.FindInBackAndInFrontTransform(actuelAmountWidth, i, rest, commanderPlace, false);
			}
			comander.formationObject.FindInBackAndInFrontTransform(actuelAmountWidth, commanderPlace, rest, commanderPlace, true);
			int testIndex = 0;
			GameServerGraphic.Form1.SpawnPointAt(comander.formationObject.formationObjects[testIndex].transform.position, System.Drawing.Color.DeepPink, 9);
			if (comander.formationObject.formationObjects[testIndex].inFrontTransform != null)
				GameServerGraphic.Form1.SpawnPointAt(comander.formationObject.formationObjects[testIndex].inFrontTransform.transform.position, System.Drawing.Color.DeepPink, 5);
			if (comander.formationObject.formationObjects[testIndex].inBackTransform != null)
				GameServerGraphic.Form1.SpawnPointAt(comander.formationObject.formationObjects[testIndex].inBackTransform.transform.position, System.Drawing.Color.DeepPink, 5);
			var test = comander.formationObject.formationObjects;
			comander.MakeAttackGrid(lineLength, deltaX);
		}
	}

}

