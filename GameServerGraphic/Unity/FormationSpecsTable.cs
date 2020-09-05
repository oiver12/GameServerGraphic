using System.Collections.Generic;
using GameServer;

//[CreateAssetMenu(fileName = "New Formation", menuName = "Formation")]
public class FormationSpecsTable
{
	public NormalComponentsObject formationObject;
	public int formationId;
	public string formationName;
	public int capacity;
	public float[] angles;
	public NormalComponentsObject lineFormationObject;
	public Lines[] frontLines;
}

//[System.Serializable]
public class Lines
{
	public Lines()
	{

	}
	//public int lineStart;
	public int[] linesindexes;
	//public int linEnd;
}
