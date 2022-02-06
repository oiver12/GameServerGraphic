using Pathfinding;
using GameServerGraphic;
using System;

namespace GameServer
{
	[Serializable]
	public class TroopComponents : BaseClassGameObject
	{
		public bool enabled = true;
		//public Transform transform;
		public Seeker seeker { get; set; }
		public RichAI richAI { get; set; }
		public CommanderScript commanderScript { get; set; }
		public PlayerController playerController { get; set; }
		//public AttackingSystem attackingSystem { get; set; }
		public NewAttackingSystem newAttackSystem { get; set; }

		private TroopComponents(){}
		public TroopComponents(Transform tr, Seeker sk, RichAI rich, PlayerController _playerController, CommanderScript commander, NewAttackingSystem _newAttackingSystem)
		{
			transform = tr;
			transform.troopObject = this;
			seeker = sk;
			seeker.Awake(this);
			richAI = rich;
			richAI.OnEnable(this);
			richAI.Start();
			commanderScript = commander;
			//commanderScript.Start(this);
			playerController = _playerController;
			//playerController.Start(this);
			//attackingSystem = _attackingSystem;
			//attackingSystem.Start(this);
			newAttackSystem = _newAttackingSystem;
			seeker.traversableTags = ~0;
			var funnel = new FunnelModifier();
			funnel.seeker = seeker;
			funnel.OnEnable();
			//seeker.RegisterModifier(new FunnelModifier());
#if graphic
			//Form1.AddTroop(transform);
#endif
		}

		public void Update()
		{
			playerController.Update();
			richAI.Update();
			//newAttackSystem.Update();
			//attackingSystem.Update();
		}

		public NormalComponentsObject GetParentNormalComponents()
		{
			if (transform.parent == null)
				return null;
				//throw new System.ArgumentException("Parent is null and cant have an Object");

			return transform.parent.normalComponents;
		}

		public TroopComponents GetParentTroopComponents()
		{
			if (transform.parent == null)
				return null;
			return transform.parent.troopObject;
		}

		//TODO not good better design needed
		public override void DestroyObject()
		{
			MEC.Timing.KillCoroutines(playerController.clientId.ToString());
			seeker = null;
			richAI = null;
			commanderScript = null;
			playerController = null;
			//attackingSystem = null;
			base.DestroyObject();
		}
	}
}
