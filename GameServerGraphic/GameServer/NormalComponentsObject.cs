using System;

namespace GameServer
{
	[Serializable]
	public class NormalComponentsObject : BaseClassGameObject
	{
		//public Transform transform;
		public GroupMovement groupMovement;

		public NormalComponentsObject(Transform trans, GroupMovement group = null)
		{
			transform = trans;
			transform.normalComponents = this;
			groupMovement = group;
		}
	}
}
