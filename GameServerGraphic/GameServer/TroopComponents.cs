using Pathfinding;
using GameServerGraphic;

namespace GameServer
{
	public class TroopComponents : BaseClassGameObject
	{
		public bool enabled = true;
		//public Transform transform;
		public Seeker seeker;
		public RichAI richAI;
		public CommanderScript commanderScript;
		public PlayerController playerController;
		public AttackingSystem attackingSystem;

		public TroopComponents(Transform tr, Seeker sk, RichAI rich, AttackingSystem _attackingSystem, PlayerController _playerController, CommanderScript commander)
		{
			transform = tr;
			transform.troopObject = this;
			seeker = sk;
			seeker.Awake();
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
#if graphic
			Form1.AddTroop(transform);
#endif
		}

		public void Update()
		{
			playerController.Update();
			richAI.Update();
			attackingSystem.Update();
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
	}
}
