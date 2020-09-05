using System.Collections;
using System.Collections.Generic;
using GameServer;


public static class MultiplierManager
{

	//public static MultiplierManager instance;

	//[Tooltip("Bonus von FormationA gegenüber FormationB")]
	public static FormationTable[] formationMultiplier;

	//[Tooltip("Bonus von TroopA gegenüber TroopB")]
	public static TroopTable[] troopMultiplier;


	//// Start is called before the first frame update
	//void Start()
 //   {
	//	instance = this;
 //   }

	public static FormationTable[] StartFormationMultiplier(FormationSpecsTable[] allTables)
	{
		List<FormationTable> temp = new List<FormationTable>();
		for(int i = 0; i < allTables.Length; i++)
		{
			for(int y = i+ 1; y < allTables.Length; y++)
			{
				temp.Add(new FormationTable
				{
					formationA = allTables[i],
					formationB = allTables[y],
				});
			}
		}
		formationMultiplier = temp.ToArray();
		return formationMultiplier;
	}

	public static TroopTable[] StartTroopMultiplier(Troops[] allTables)
	{
		List<TroopTable> temp = new List<TroopTable>();
		for (int i = 0; i < allTables.Length; i++)
		{
			for (int y = i + 1; y < allTables.Length; y++)
			{
				temp.Add(new TroopTable
				{
					troopA = allTables[i],
					troopB = allTables[y],
				});
			}
		}
		troopMultiplier = temp.ToArray();
		return troopMultiplier;
	}


	/*public void SaveToScript(int formationA, int formationB)
	{
		for (int i = 0; i < formationMultiplier.Length; i++)
		{
			if (formationMultiplier[i].formationA.formationId == formationA && formationMultiplier[i].formationB.formationId == formationB)
			{
				formationMultiplier[i].multiplier = editorMultiplier;
			}
		}
	}*/

	public static float getFormationMultiplier(int formationA, int formationB)
	{
		if (formationA == formationB)
			return 1f;
		else
		{
			for (int i = 0; i < formationMultiplier.Length; i++)
			{
				if (formationMultiplier[i].formationA.formationId == formationA && formationMultiplier[i].formationB.formationId == formationB)
				{
					return formationMultiplier[i].multiplier;
				}
				if (formationMultiplier[i].formationA.formationId == formationB && formationMultiplier[i].formationB.formationId == formationA)
					return 1f / formationMultiplier[i].multiplier;
			}
		}
		Debug.LogError(string.Format("Cant find Formationsbonus with this Index {0}, {1}", formationA, formationB));
		return -1f;
	}

	public static float getTroopMultiplier(int troopA, int troopB)
	{

		if (troopA == troopB)
			return 1f;
		else
		{
			for (int i = 0; i < troopMultiplier.Length; i++)
			{
				if (troopMultiplier[i].troopA.id == troopA && troopMultiplier[i].troopB.id == troopB)
				{
					return troopMultiplier[i].multiplier;
				}
				if (troopMultiplier[i].troopA.id == troopB && troopMultiplier[i].troopB.id == troopA)
					return 1f / troopMultiplier[i].multiplier;
			}
		}
		Debug.LogError(string.Format("Cant find Troopbonus with this Index {0}, {1}", troopA, troopB));
		return -1f;
	}
}
