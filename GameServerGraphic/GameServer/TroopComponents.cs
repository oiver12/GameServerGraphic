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
		public AttackingSystem attackingSystem { get; set; }

		private TroopComponents(){}
		public TroopComponents(Transform tr, Seeker sk, RichAI rich, AttackingSystem _attackingSystem, PlayerController _playerController, CommanderScript commander)
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
			attackingSystem = _attackingSystem;
			//attackingSystem.Start(this);
			seeker.traversableTags = ~0;
			var funnel = new FunnelModifier();
			funnel.seeker = seeker;
			funnel.OnEnable();
			//seeker.RegisterModifier(new FunnelModifier());
#if graphic
			Form1.AddTroop(transform);
#endif
		}

		public void Update()
		{
			playerController.Update();
			richAI.Update();
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

		//TODO not good better design
		public override void DestroyObject()
		{
			seeker = null;
			richAI = null;
			commanderScript = null;
			playerController = null;
			attackingSystem = null;
			base.DestroyObject();
		}
	}
}
