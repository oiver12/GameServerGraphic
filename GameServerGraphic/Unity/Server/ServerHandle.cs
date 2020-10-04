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
			Debug.LogErrorFormat("User with id {0} had the ErrorMessage \n {1} {2}", _fromClient, condition, stackTrace);
		else
			Debug.LogErrorFormat("User with id {0} had the Excpetion \n {1} {2}", _fromClient, condition, stackTrace);
	}
}

