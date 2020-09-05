using System.Collections.Generic;
using GameServer;
using MEC;

public class ArcherCommander : CommanderScript
{
	public override void SetAttackInForm(TroopComponents enemyAttackPlayer, bool hasMadeTurn)
	{
		bool attackOtherAttackGrid = false;
		TroopComponents otherCommander = null;
		//check ob der Attackierte in Formation steht, weil dann treffen zwei Formation aufeinander
		if(enemyAttackPlayer.playerController.Mycommander != null)
		{
			otherCommander = enemyAttackPlayer.playerController.Mycommander;
			attackOtherAttackGrid = true;
		}

		//wenn eine andere AttackForm angegriffen wird
		if (attackOtherAttackGrid)
		{
			//wenn normal angegriffen wird
			if (attackStyleAtMoment == AttackStyle.Normal)
				AttackNormalOtherFormation(enemyAttackPlayer, otherCommander, hasMadeTurn);
			////wenn in die Andere Formation rein gelaufen wird
			//else if (attackStyleAtMoment == AttackStyle.Charge)
			//	AttackChargeOtherFormation(enemyAttackPlayer, otherCommander, hasMadeTurn);

		}
		//wenn andere Truppen nicht in Formation angegriffen werden
		//else
		//{
		//	if (attackStyleAtMoment == AttackStyle.Normal)
		//		AttackNormalOtherTroops(enemyAttackPlayer);

		//	else if (attackStyleAtMoment == AttackStyle.Charge)
		//		AttackChargeOtherTroops(enemyAttackPlayer, hasMadeTurn);
		//}
	}

	protected override void AttackNormalOtherFormation(TroopComponents enemyAttackPlayer, TroopComponents otherCommander, bool hasMadeTurn)
	{
		enemyAttackPlayer = otherCommander;
		enemyAttackPlayer.commanderScript.GettingAttacked(attackStyleAtMoment);
		float preRadius = richAI.radius;
		richAI.radius = 0.5f;
		//seeker.traversableTags = GetAgentType(0);
		playerController.currentState = STATE.Following;
		attackingSystem.enemyAttackPlayer = enemyAttackPlayer;
		if (!hasMadeTurn)
		{
			positonToWalkTo = enemyAttackPlayer.transform.position + ((troopObject.transform.position - enemyAttackPlayer.transform.position).normalized * playerController.myTroop.attackRadius);
			//richAI.endReachedDistance = 1.3f;
			playerController.MoveToPosition(positonToWalkTo, false);
			Timing.RunCoroutine(CheckForDistanceCoroutine());
		}
		else
		{
			Debug.Log("StartFight");
			ServerSend.StartFight(playerController.clientId, playerController.troopId, true, true, (int)attackStyleAtMoment, attackingSystem.frontLineMinAttackRange);
			troopObject.attackingSystem.StartInvokingRepeat(attackStyleAtMoment);
		}
	}

	protected override IEnumerator<float> CheckForDistanceCoroutine()
	{
		while (playerController.commanderIsTurning)
			yield return 0f;
		bool hasToCheckDistance = true;
		while (hasToCheckDistance)
		{
			Debug.Log("Check for Distance");
			float sqrdistance = (troopObject.transform.position - positonToWalkTo).sqrMagnitude;
			if (sqrdistance < (richAI.endReachedDistance + 0.1f) * (richAI.endReachedDistance + 0.1f))
			{
				hasToCheckDistance = false;
				playerController.circleWalk = false;
				hasToWalk = false;
				SetAttackInForm(attackingSystem.enemyAttackPlayer, true);
			}
			yield return Timing.WaitForSeconds(0.2f);
		}
	}

	public override void BeginAttackInFormation(TroopComponents nearetsObject)
	{
		Debug.Log("Begin Attack");
		SetAttackInForm(nearetsObject, false);
	}
}
