using System;

namespace GameServer
{
	public class FormationObject
	{
		public readonly Transform transform;
		public readonly FormationChild[] formationObjects;

		public FormationObject(Transform transform, FormationChild[] formationObjects)
		{
			this.transform = transform;
			this.formationObjects = formationObjects;
		}
	}

	public class FormationChild : BaseClassGameObject
	{
		public readonly int line;
		public readonly Transform inFrontTransform;
		public readonly Transform inBackTransform;

		public FormationChild(Transform transform, int line, Transform inFrontTransform, Transform inBackTransform)
		{
			this.transform = transform;
			this.line = line;
			this.inFrontTransform = inFrontTransform;
			this.inBackTransform = inBackTransform;
		}
	}
}
