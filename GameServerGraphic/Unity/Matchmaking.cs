using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameServer;

class Matchmaking
{

	public static List<Client> clientSerachingFroMatch = new List<Client>();

	public static void Update()
	{
		if (clientSerachingFroMatch.Count == 1)
		{
			if (!clientSerachingFroMatch[0].hasSendMatchmaking)
			{
				ServerSend.SendMessage("No other Player searching for Match", clientSerachingFroMatch[0].id);
				clientSerachingFroMatch[0].hasSendMatchmaking = true;
			}
		}
		else if (clientSerachingFroMatch.Count >= 2)
		{
			/*ServerSend.foundEnemy(clientSerachingFroMatch[0].id);
			ServerSend.foundEnemy(clientSerachingFroMatch[1].id);
			clientSerachingFroMatch[0].enemyClient = clientSerachingFroMatch[1];
			clientSerachingFroMatch[1].enemyClient = clientSerachingFroMatch[0];
			clientSerachingFroMatch[0].player.enemyPlayer = clientSerachingFroMatch[1].player;
			clientSerachingFroMatch[1].player.enemyPlayer = clientSerachingFroMatch[0].player;
			clientSerachingFroMatch.RemoveAt(0);
			clientSerachingFroMatch.RemoveAt(0);*/
			Debug.Log("HERE");
			// https://stackoverflow.com/questions/5953552/how-to-get-the-closest-number-from-a-listint-with-linq   //
			for(int i = 0; i< clientSerachingFroMatch.Count; i++)
			{
				if(clientSerachingFroMatch[i].isAttacker != clientSerachingFroMatch[0].isAttacker)
				{
					Debug.Log("FOund");
					ServerSend.foundEnemy(clientSerachingFroMatch[0].id);
					ServerSend.foundEnemy(clientSerachingFroMatch[i].id);
					clientSerachingFroMatch[0].enemyClient = clientSerachingFroMatch[i];
					clientSerachingFroMatch[i].enemyClient = clientSerachingFroMatch[0];
					clientSerachingFroMatch[0].player.enemyPlayer = clientSerachingFroMatch[i].player;
					clientSerachingFroMatch[i].player.enemyPlayer = clientSerachingFroMatch[0].player;
					clientSerachingFroMatch.RemoveAt(i);
					clientSerachingFroMatch.RemoveAt(0);
				}
			}
		}
	}
}
