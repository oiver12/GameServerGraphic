using System.Collections;
using System.Collections.Generic;
using GameServer;
using Pathfinding;

public static class NetworkManager
{
	//public static Troops[] allTroops;
    // Start is called before the first frame update
  //  void Start()
  //  {
		////QualitySettings.vSyncCount = 0;
		////Application.targetFrameRate = 30;
		//Server.allTroops = allTroops;
  //  }

	//private void OnApplicationQuit()
	//{
	//	Server.Stop();
	//}

	public static TroopComponents InstantiateTroop(Troops troop, Vector3 spawnPosition)
	{
		if (troop.klasse.IsFlagSet(TroopClass.Commander))
		{
			if (troop.klasse.IsFlagSet(TroopClass.Archer))
			{
				return new TroopComponents(new Transform(spawnPosition, Quaternion.Identity), new Seeker(), new RichAI(), new AttackingSystem(), new PlayerController(), new ArcherCommander());
			}
			else
			{
				return new TroopComponents(new Transform(spawnPosition, Quaternion.Identity), new Seeker(), new RichAI(), new AttackingSystem(), new PlayerController(), new CommanderScript());
			}
		}
		else
			return new TroopComponents(new Transform(spawnPosition, Quaternion.Identity), new Seeker(), new RichAI(), new AttackingSystem(), new PlayerController(), null);
	}
}
