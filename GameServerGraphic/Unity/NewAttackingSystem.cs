using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
	[System.Serializable]
	public class NewAttackingSystem : MonoBehaviour
	{
		public int enemyTroopAttacking;
		public int troopsAttackingFirstLine = 0;
		public TroopComponents enemyPlayer;
		[System.NonSerialized]
		[System.Xml.Serialization.XmlIgnore]
		public Client myClient;

		const int maxTroopsOnOneTroops = 1;
		const float distanceToPointBeforeFormation = 20;
		const float maxDistanceToAttackGrid = 5f;
		const float distanceTroopInBackAttack = 10f;

		float lastTimeAttack = 0f;
		float startTimeAttackCharge = 0f;
		float randomTimeDelayChargeAttack = 10f;
		bool distanceChecking = false;
		bool isAttacking = false;
		AttackStyle attackStyle = AttackStyle.Charge;
		TroopComponents troopObject;
		PlayerController playerController;

		public override void Start(TroopComponents _troopObject)
		{
			troopObject = _troopObject;
			playerController = _troopObject.playerController;
			myClient = playerController.myClient;
		}

		public void Update()
		{
			if (distanceChecking)
			{
				if ((enemyPlayer.transform.position - troopObject.transform.position).sqrMagnitude <= distanceToPointBeforeFormation * distanceToPointBeforeFormation)
				{
					troopObject.commanderScript.SetAttackInForm(enemyPlayer, true);
				}
			}
			else
			{
				if(attackStyle == AttackStyle.Normal)
				{
					if (playerController.lineInFormation == 1)
					{
						troopObject.richAI.destination = troopObject.transform.position;
						if(enemyPlayer == null || enemyPlayer.isDestroyed || Time.frameCount % 30 == 0)
						{
							enemyPlayer = FindClosestNormalAttack();
						}
						else
						{
							if ((enemyPlayer.transform.position - troopObject.transform.position).sqrMagnitude < playerController.attackRange * playerController.attackRange)
							{
								GameServerGraphic.Form1.ChanceTroopColor(troopObject.transform, System.Drawing.Color.Pink);
								if (!isAttacking)
								{
									if (playerController.Mycommander.newAttackSystem.troopsAttackingFirstLine == 0)
										CommanderStopFirstLineAttack();
									playerController.Mycommander.newAttackSystem.troopsAttackingFirstLine++;
								}
								isAttacking = true;
							}
						}
					}
					troopObject.richAI.destination = troopObject.richAI.destination = troopObject.playerController.transformOnAttackGrid.transform.position;
				}
				else if (attackStyle == AttackStyle.Charge)
				{
					if (playerController.lineInFormation == 1 || enemyPlayer != null)
					{
						if (Time.time - startTimeAttackCharge < randomTimeDelayChargeAttack)
						{
							troopObject.richAI.destination = troopObject.transform.position;
							return;
						}
						// I.) Nicht weiter weg als .. von AttackGrid Position gehen, wenn Truppe angreifen
						// II.) Nicht mehr als 3 Truppen pro gegnerische Truppe
						// III.) Preferiere gegnerische Truppe mit weniger Truppen an sich
						if ((troopObject.transform.position - playerController.transformOnAttackGrid.transform.position).sqrMagnitude < maxDistanceToAttackGrid * maxDistanceToAttackGrid)
						{
							if ((enemyPlayer == null || enemyPlayer.isDestroyed) && playerController.lineInFormation == 1)
							{
								enemyPlayer = FindClosestChargeAttack(true);
								if (enemyPlayer != null)
								{
									troopObject.richAI.destination = enemyPlayer.transform.position;
								}
								else
									troopObject.richAI.destination = playerController.transformOnAttackGrid.transform.position;
							}
							else if (enemyPlayer == null || enemyPlayer.isDestroyed)
								enemyPlayer = null;
							//wenn die Truppe eine gegnerische Truppe hat zum angreifen
							else
							{
								//wenn in AttackRadius angreifen
								if ((enemyPlayer.transform.position - troopObject.transform.position).sqrMagnitude < playerController.attackRange * playerController.attackRange)
								{
									GameServerGraphic.Form1.ChanceTroopColor(troopObject.transform, System.Drawing.Color.Pink);
									troopObject.richAI.destination = troopObject.transform.position;
									if (Time.time - lastTimeAttack > playerController.attackSpeed)
									{
										lastTimeAttack = Time.time;
										myClient.player.enemyPlayer.ReduceTroopDamage(enemyPlayer.playerController.troopId, playerController.troopId, playerController.myTroop.damage);
									}
								}
								else
								{
									GameServerGraphic.Form1.ChanceTroopColor(troopObject.transform, System.Drawing.Color.Black);
									troopObject.richAI.destination = enemyPlayer.transform.position;
								}
							}

						}
						else
						{
							troopObject.richAI.destination = troopObject.transform.position;
						}
					}
					else
					{
						if ((troopObject.playerController.transformOnAttackGrid.inFrontTransform.troopOnFormationChild.transform.position - troopObject.transform.position).sqrMagnitude > troopObject.playerController.Mycommander.commanderScript.formationDeltaX * troopObject.playerController.Mycommander.commanderScript.formationDeltaX)
							troopObject.richAI.destination = troopObject.playerController.transformOnAttackGrid.inFrontTransform.troopOnFormationChild.transform.position;
						else
							troopObject.richAI.destination = troopObject.transform.position;

						if (Time.frameCount % 30 == 0)
							enemyPlayer = FindClosestChargeAttack(false);

					}
				}
			}
		}

		public void StartFollowing(TroopComponents otherComander, AttackStyle attackStyle)
		{
			//TODO check ob point weiter weg als distance
			this.attackStyle = attackStyle;
			distanceChecking = true;
			troopObject.playerController.currentState = STATE.Following;
			troopObject.playerController.MoveToPosition(otherComander.transform.position, false);
			enemyPlayer = otherComander;
			troopObject.playerController.isAttacking = true;
		}

		public void CommanderStopFirstLineAttack()
		{
			playerController.Mycommander.richAI.canMove = false;
			playerController.Mycommander.richAI.destination = playerController.Mycommander.transform.position;
		}

		public void StartAttack(float randomIntervall, AttackStyle attackstyle)
		{
			this.attackStyle = attackstyle;
			if (attackstyle == AttackStyle.Charge)
			{
				enemyPlayer = null;
				distanceChecking = false;
				troopObject.richAI.SetPath(null);
				startTimeAttackCharge = Time.time;
				randomTimeDelayChargeAttack = randomIntervall;
			}
			else if(attackStyle == AttackStyle.Normal)
			{
				enemyPlayer = null;
				distanceChecking = false;
				troopObject.richAI.SetPath(null);
			}
		}

		public TroopComponents FindClosestNormalAttack()
		{
			TroopComponents enemyNearest = myClient.enemyClient.player.FindNearestTroop(troopObject.transform.position);
			if (enemyNearest.newAttackSystem.enemyTroopAttacking < maxTroopsOnOneTroops)
			{
				ChooseEnemyTroop(enemyNearest);
				return enemyNearest;
			}
			else return null;
		}

		public TroopComponents FindClosestChargeAttack(bool troopOnFirstLine)
		{
			//TODO find near player by this because other troop is there
			TroopComponents enemyNearest = myClient.enemyClient.player.FindNearestTroop(troopObject.transform.position);
			if (!troopOnFirstLine && enemyNearest.newAttackSystem.enemyTroopAttacking < maxTroopsOnOneTroops)
				Debug.Log((enemyNearest.transform.position - troopObject.transform.position).sqrMagnitude);
			if (!troopOnFirstLine)
				Debug.Log("OK Here Attack" + enemyNearest.newAttackSystem.enemyTroopAttacking.ToString());
			if (enemyNearest.newAttackSystem.enemyTroopAttacking < maxTroopsOnOneTroops && (troopOnFirstLine || (enemyNearest.transform.position - troopObject.transform.position).sqrMagnitude < distanceTroopInBackAttack * distanceTroopInBackAttack))
			{
				ChooseEnemyTroop(enemyNearest);
				return enemyPlayer;
			}
			else if (troopOnFirstLine)
			{
				TroopComponents commanderOther = enemyNearest.playerController.Mycommander;
				if (commanderOther == null)
					return null;
				else
				{
					enemyNearest = null;
					float distance = float.PositiveInfinity;
					for (int i = 0; i < commanderOther.commanderScript.controlledTroops.Count; i++)
					{
						if (commanderOther.commanderScript.controlledTroops[i].newAttackSystem.enemyTroopAttacking < maxTroopsOnOneTroops)
						{
							float tempDistance = (commanderOther.commanderScript.controlledTroops[i].transform.position - troopObject.transform.position).sqrMagnitude;
							if (tempDistance < distance)
							{
								distance = tempDistance;
								enemyNearest = commanderOther.commanderScript.controlledTroops[i];
							}
						}
					}
					ChooseEnemyTroop(enemyNearest);
					return enemyPlayer;
				}
			}
			else
				return null;
		}

		public void ChooseEnemyTroop(TroopComponents enemyNearest)
		{
			if (enemyPlayer != null && !enemyPlayer.isDestroyed && enemyNearest != enemyPlayer)
			{
				enemyPlayer.newAttackSystem.enemyTroopAttacking--;
			}
			enemyNearest.newAttackSystem.enemyTroopAttacking++;
			enemyPlayer = enemyNearest;
		}
	}
}
