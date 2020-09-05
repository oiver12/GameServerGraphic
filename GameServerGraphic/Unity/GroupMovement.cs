using System.Collections;
using System.Collections.Generic;
using GameServer;

public class GroupMovement
{
	List<TroopComponents> troopsToWalk = new List<TroopComponents>();
	Vector3 sumOfAll = Vector3.zero;
	int durchläufe = 0;
	public Vector3 middlePoint;

	public void AddTrop(TroopComponents troop, bool lastTroop, Vector3 toPosition)
	{
		sumOfAll += troop.transform.position;
		durchläufe++;
		if (troop.playerController.currentState == STATE.Following && troop.commanderScript != null && troop.commanderScript.attackGrid)
		{
			if (!troop.playerController.checkIfAttack(toPosition))
				troop.commanderScript.StopAttack();
		}
		else
			troop.playerController.checkIfAttack(toPosition);

		if (!troopsToWalk.Contains(troop))
			troopsToWalk.Add(troop);
		if(lastTroop)
		{
			middlePoint = sumOfAll / durchläufe;
			foreach(TroopComponents child in troopsToWalk)
			{
				//wenn ein Commander am angreifen ist, dann muss nicht alles gesetzt werden, sondern in AttckForm angreifen
				if (child.playerController.currentState == STATE.Following && child.commanderScript != null && child.commanderScript.attackGrid)
				{
					return;
				}
				/*else if (child.GetComponent<CommanderScript>() != null && child.GetComponent<CommanderScript>().attackGrid)
				{
					Vector3 offset = child.position - middlePoint;
					float distance = offset.magnitude;
					float distance = Mathf.Clamp(distance, child.GetComponent<PlayerController>)
				}
				else
				{*/
					Vector3 offset = child.transform.position - middlePoint;
					float distance = offset.magnitude;
					//distance = distance / Mathf.Pow(1.1f, distance);
					if(child.commanderScript != null && child.commanderScript.attackGrid)
						distance = Mathf.Clamp(distance, child.commanderScript.formationRadius + 1f, child.commanderScript.formationRadius + 3f);
					else
						distance = Mathf.Clamp(distance, child.richAI.radius + 0.5f, child.playerController.richAI.radius + 2f);

					child.playerController.MoveToPosition(toPosition + offset.normalized * distance, false);
				//}
			}
			sumOfAll = Vector3.zero;
			durchläufe = 0;
		}
	}

	public void ReachedDestination(TroopComponents troop)
	{
		//troop.GetComponent<PlayerController>().richAI.endReachedDistance = /*Mathf.CeilToInt(troop.GetComponent<PlayerController>().agent.radius + 2f);*/2f;
		troopsToWalk.Remove(troop);
	}
}
