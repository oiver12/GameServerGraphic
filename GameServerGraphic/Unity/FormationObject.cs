using System;

namespace GameServer
{
	[System.Serializable]
	public class FormationObject
	{
		public readonly Transform transform;
		public FormationChild[] formationObjects;

		private FormationObject() { }

		public FormationObject(Transform transform, FormationChild[] formationObjects)
		{
			this.transform = transform;
			this.formationObjects = formationObjects;
		}
	}
	[System.Serializable]
	public class FormationChild : BaseClassGameObject
	{
		public int line;
		public Transform inFrontTransform;
		public Transform inBackTransform;

		private FormationChild() { }

		public FormationChild(Transform transform, int line, Transform inFrontTransform, Transform inBackTransform)
		{
			this.transform = transform;
			this.line = line;
			this.inFrontTransform = inFrontTransform;
			this.inBackTransform = inBackTransform;
		}
	}
}
