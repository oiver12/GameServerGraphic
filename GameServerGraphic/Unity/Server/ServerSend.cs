using System;
using System.Collections.Generic;
using System.Text;
using GameServer;

public class ServerSend
{
	static int j = 0;
	private static void SendTCPData(int _toClient, Packet _packet)
	{
		_packet.WriteLength();
		Server.clients[_toClient].tcp.SendData(_packet);
	}

	private static void SendUDPData(int _toClient, Packet _packet)
	{
		_packet.WriteLength();
		Server.clients[_toClient].udp.SendData(_packet);
	}

	private static void SendTCPDataToAll(Packet _packet)
	{
		_packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++)
		{
			Server.clients[i].tcp.SendData(_packet);
		}
	}
	private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
	{
		_packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++)
		{
			if (i != _exceptClient)
			{
				Server.clients[i].tcp.SendData(_packet);
			}
		}
	}

	private static void SendUDPDataToAll(Packet _packet)
	{
		_packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++)
		{
			Server.clients[i].udp.SendData(_packet);
		}
	}
	private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
	{
		_packet.WriteLength();
		for (int i = 1; i <= Server.MaxPlayers; i++)
		{
			if (i != _exceptClient)
			{
				Server.clients[i].udp.SendData(_packet);
			}
		}
	}

	#region Packets
	public static void Welcome(int _toClient, string _msg)
	{
		using (Packet _packet = new Packet((int)ServerPackets.welcome))
		{
			_packet.Write(_msg);
			_packet.Write(_toClient);

			SendTCPData(_toClient, _packet);
		}
	}

	public static void succesfullyLogedIn(int _toClient, bool status)
	{
		using (Packet _packet = new Packet((int)ServerPackets.succesfullyLogedIn))
		{
			_packet.Write(status);

			SendTCPData(_toClient, _packet);
		}
	}

	public static void foundEnemy(int _toClient)
	{
		using (Packet _packet = new Packet((int)ServerPackets.foundEnemy))
		{
			_packet.Write(true);

			SendTCPData(_toClient, _packet);
		}
	}

	public static void SendMessage(string msg, int _toClient)
	{
		using (Packet _packet = new Packet((int)ServerPackets.message))
		{
			_packet.Write(msg);
			SendTCPData(_toClient, _packet);
		}
	}

	public static void TroopPlace(int _toClient, Troops troop, Vector3 spawnPosition, int troopId, int commanderId, bool ownTroop)
	{
		using (Packet _packet = new Packet((int)ServerPackets.TroopPlace))
		{
			_packet.Write(false);
			_packet.Write(ownTroop);
			_packet.Write((ushort)troop.id);
			_packet.Write(spawnPosition);
			_packet.Write((ushort)troopId);
			_packet.Write(commanderId);
			SendTCPData(_toClient, _packet);
		}
	}

	public static void PlaceArmy(int _toClient, int commanderId, Vector3 spawnPosition, List<int> troopIds, List<Vector3> positions, bool ownTroop)
	{
		using (Packet _packet = new Packet((int)ServerPackets.TroopPlace))
		{
			_packet.Write(true);
			_packet.Write(ownTroop);
			_packet.Write((ushort)commanderId);
			_packet.Write(spawnPosition);
			_packet.Write((ushort)troopIds.Count);
			for (int i = 0; i < troopIds.Count; i++)
			{
				_packet.Write((ushort)troopIds[i]);
				_packet.Write(positions[i]);
			}

			SendTCPData(_toClient, _packet);
		}
	}

	//public static void PlaceArmy(int _toClient, int formationId, Troops arnyCommander, List<Troops> armyTroops, Vector3 spawnPosition)

	public static void troopMove(bool ownTroop ,int _toClient, int troopId, Vector3 position, Vector3 nextDestination, float remainingTime, bool hasLeftPath)
	{
		using (Packet _packet = new Packet((int)ServerPackets.troopMove))
		{
			_packet.Write(ownTroop);
			_packet.Write((ushort)troopId);
			_packet.Write(position);
			_packet.Write(nextDestination);
			_packet.Write(remainingTime);
			_packet.Write(hasLeftPath);
			SendUDPData(_toClient, _packet);
		}
	}

	public static void hasReachedDestination(bool ownTroop,int troopId, int toClient, Vector3 position, int commanderId)
	{
		using (Packet _packet = new Packet((int)ServerPackets.hasReachedDestination))
		{
			_packet.Write(ownTroop);
			_packet.Write((ushort)troopId);
			_packet.Write(position);
			_packet.Write(commanderId);
			SendTCPData(toClient, _packet);
		}
	}

	public static void troopDamage(int toClient, int troopId, int enemyTroopId, float damage, bool myTroopDamage, bool troopDeath)
	{
		using(Packet _packet = new Packet((int)ServerPackets.troopDamage))
		{
			_packet.Write((ushort)troopId);
			_packet.Write((ushort)enemyTroopId);
			_packet.Write(damage);
			_packet.Write(myTroopDamage);
			_packet.Write(troopDeath);
			SendUDPData(toClient, _packet);
		}
	}

	public static void troopDamageToGroup(int toClient, int[] troopIds, int enemyTroopId, float damage)
	{
		using (Packet _packet = new Packet((int)ServerPackets.troopDamageToGroup))
		{
			_packet.Write((ushort)troopIds.Length);
			for (int i = 0; i < troopIds.Length; i++)
			{
				_packet.Write((ushort)troopIds[i]);
			}
			_packet.Write((ushort)enemyTroopId);
			_packet.Write(damage);
			_packet.Write(true);
			SendUDPData(toClient, _packet);
		}
		using (Packet _packet = new Packet((int)ServerPackets.troopDamageToGroup))
		{
			_packet.Write((ushort)troopIds.Length);
			for (int i = 0; i < troopIds.Length; i++)
			{
				_packet.Write((ushort)troopIds[i]);
			}
			_packet.Write((ushort)enemyTroopId);
			_packet.Write(damage);
			_packet.Write(false);
			SendUDPData(Server.clients[toClient].enemyClient.id, _packet);
		}
		}

	public static void SetInAttackForm(int toClient, int commanderId, Quaternion commanderAngle, float radius, bool ownTroop)
	{
		using (Packet _packet = new Packet((int)ServerPackets.setAttackForm))
		{
			_packet.Write((ushort)commanderId);
			_packet.Write(commanderAngle);
			_packet.Write(ownTroop);
			if(!ownTroop)
				_packet.Write(radius);
			SendTCPData(toClient, _packet);
		}
	}

	public static float fireBuilding(int toClient, Vector3 target, Vector3 projectTile, float firingAngle, float gravity, int buildingId)
	{
		float target_Distance = Vector3.Distance(projectTile, target);
		Vector3 dir = target - projectTile; // get Target Direction
		float height = dir.y; // get height difference
		dir.y = 0; // retain only the horizontal difference
		float dist = dir.magnitude; // get horizontal direction
		float a = firingAngle * Mathf.Deg2Rad; // Convert angle to radians
		dir.y = dist * Mathf.Tan(a); // set dir to the elevation angle.
		dist += height / Mathf.Tan(a); // Correction for small height differences

		// Calculate the velocity magnitude
		float velocity = Mathf.Sqrt(dist *gravity / Mathf.Sin(2 * a));
		dir = velocity * dir.normalized; // Return a normalized vector.
		float flightDuration = Vector3.Distance(target, projectTile) / Math.Abs(dir.y);
		//Projectile.GetComponent<ProjectTile>().StartCoroutine(Projectile.GetComponent<ProjectTile>().startThrow(flightDuration, Target.position));
		using (Packet _packet = new Packet((int)ServerPackets.fireBuilding))
		{
			_packet.Write(/*Target.position*/ target);
			_packet.Write(flightDuration);
			_packet.Write(gravity);
			_packet.Write(dir.x);
			_packet.Write(dir.y);
			_packet.Write(dir.z);
			_packet.Write((ushort)buildingId);
			SendTCPData(toClient, _packet);
		}
		return flightDuration;
	}

	public static void StartFight(int _toClient, int troopId, bool startFight, bool toAllInFormation, int attackType = 0, float attackRangeFirstLine = 0f)
	{
		using (Packet _packet = new Packet((int)ServerPackets.StartFight))
		{
			_packet.Write((ushort)troopId);
			_packet.Write(startFight);
			_packet.Write(toAllInFormation);
			_packet.Write(true);
			if (startFight && toAllInFormation)
				_packet.Write(attackRangeFirstLine);
			SendTCPData(_toClient, _packet);
		}
		using (Packet _packet = new Packet((int)ServerPackets.StartFight))
		{
			_packet.Write((ushort)troopId);
			_packet.Write(startFight);
			_packet.Write(toAllInFormation);
			_packet.Write(false);
			if (startFight && toAllInFormation)
			{
				_packet.Write(attackRangeFirstLine);
				_packet.Write(attackType);
			}
			SendTCPData(Server.clients[_toClient].enemyClient.id, _packet);
		}
	}

	public static void fightArcher(int toClient, Vector3 target, Vector3 projectTile, float firingAngle, float gravity, int troopId, bool armyShoot)
	{
		Vector3 dir = target - projectTile; // get Target Direction
		float height = dir.y; // get height difference
		dir.y = 0; // retain only the horizontal difference
		float dist = dir.magnitude; // get horizontal direction
		float a = firingAngle * Mathf.Deg2Rad; // Convert angle to radians
		dir.y = dist * Mathf.Tan(a); // set dir to the elevation angle.
		dist += height / Mathf.Tan(a); // Correction for small height differences
		// Calculate the velocity magnitude
		float velocity = Mathf.Sqrt(dist * gravity / Mathf.Sin(2 * a));
		dir = velocity * dir.normalized; // Return a normalized vector.
		float flightDuration = Vector3.Distance(target, projectTile) / Math.Abs(dir.y);
		using (Packet _packet = new Packet((int)ServerPackets.fightArcher))
		{
			_packet.Write(target);
			_packet.Write(flightDuration);
			_packet.Write(gravity);
			_packet.Write(dir);
			_packet.Write((ushort)troopId);
			_packet.Write(armyShoot);
			SendTCPData(toClient, _packet);
		}
	}

	public static void SendSerializeInGame(int toClient, bool serialize)
	{
		using (Packet _packet = new Packet((int)ServerPackets.serializeInGameDebugging))
		{
			_packet.Write(serialize);
			SendTCPData(toClient, _packet);
		}
	}

	#endregion
}
