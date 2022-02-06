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
		public void FindInBackAndInFrontTransform(int actuelAmountWidth, int i, int rest, int commanderPlace, bool isCommander)
		{
			int indexDahinter = i - actuelAmountWidth;
			int indexDavor = i + actuelAmountWidth;
			if (i < rest)
			{
				indexDavor = i + rest + ((actuelAmountWidth - rest) / 2);
			}
			if (i < rest + actuelAmountWidth)
			{
				indexDahinter = i - (rest + ((actuelAmountWidth - rest) / 2));
				if (indexDahinter >= rest + ((actuelAmountWidth - rest) / 2))
					indexDahinter = -1;
			}
			if (i > commanderPlace && indexDahinter < commanderPlace)
				indexDahinter++;
			if (i < commanderPlace && indexDavor > commanderPlace)
				indexDavor--;

			if (isCommander)
			{
				i = 0;
				indexDahinter++;
				Debug.Log(indexDavor);
			}
			if (indexDahinter > 0 && indexDahinter < formationObjects.Length)
				formationObjects[i].inBackTransform = formationObjects[indexDahinter];

			if (indexDavor > 0 && indexDavor < formationObjects.Length)
				formationObjects[i].inFrontTransform = formationObjects[indexDavor];
		}
	}
	[System.Serializable]
	public class FormationChild : BaseClassGameObject
	{
		public int line;
		public FormationChild inFrontTransform;
		public FormationChild inBackTransform;
		public TroopComponents troopOnFormationChild;

		private FormationChild() { }

		public FormationChild(Transform transform, int line, FormationChild inFrontTransform, FormationChild inBackTransform)
		{
			this.transform = transform;
			this.line = line;
			this.inFrontTransform = inFrontTransform;
			this.inBackTransform = inBackTransform;
		}
	}
}
