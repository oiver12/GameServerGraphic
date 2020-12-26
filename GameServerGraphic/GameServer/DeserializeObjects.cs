using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

namespace GameServer
{
	public static class DeserializeObjects
	{
		public static void DeserializeMultiplier(byte[] data, out FormationTable[] formationTable, out TroopTable[] troopTable)
		{
			Packet packet = new Packet(data);
			int lengthFormationTable = packet.ReadInt();
			formationTable = new FormationTable[lengthFormationTable];
			for (int i = 0; i < lengthFormationTable; i++)
			{
				formationTable[i] = new FormationTable()
				{
					formationA = GetFormation(packet.ReadInt()),
					formationB = GetFormation(packet.ReadInt()),
					multiplier = packet.ReadFloat(),
				};
			}
			int lengthFromTroopTable = packet.ReadInt();
			troopTable = new TroopTable[lengthFromTroopTable];
			for (int i = 0; i < lengthFromTroopTable; i++)
			{
				troopTable[i] = new TroopTable()
				{
					troopA = GetTroop(packet.ReadInt()),
					troopB = GetTroop(packet.ReadInt()),
					multiplier = packet.ReadFloat(),
				};
			}
		}

		static FormationSpecsTable GetFormation(int formationId)
		{
			FormationSpecsTable[] allFormations = new FormationSpecsTable[FormationManager.formations.Length + 1];
			for (int i = 0; i < FormationManager.formations.Length; i++)
			{
				allFormations[i] = FormationManager.formations[i];
			}
			allFormations[FormationManager.formations.Length] = new FormationSpecsTable()
			{
				formationId = -1,
				formationName = "NoFormation",
			};
			for (int i = 0; i < allFormations.Length; i++)
			{
				if (allFormations[i].formationId == formationId)
					return allFormations[i];
			}
			return null;
		}

		static Troops GetTroop(int troopId)
		{
			for (int i = 0; i < Server.allTroops.Length; i++)
			{
				if (Server.allTroops[i].id == troopId)
					return Server.allTroops[i];
			}
			return null;
		}

		public static FormationSpecsTable[] deserializeFormationManager(byte[] data)
		{
			Packet packet = new Packet(data);
			int lengthformationManager = packet.ReadInt();
			FormationSpecsTable[] formationSpecsTables = new FormationSpecsTable[lengthformationManager];
			for (int i = 0; i < lengthformationManager; i++)
			{
				NormalComponentsObject formationComponentsObject;
				NormalComponentsObject lineFormations;
				FormationSpecsTable form = new FormationSpecsTable();
				//form.formationObject = deserializeGameObject(packet);
				formationComponentsObject = deserializeGameObject(packet);
				form.formationId = packet.ReadInt();
				form.formationName = packet.ReadString();
				form.capacity = packet.ReadInt();
				int lengthAngles = packet.ReadInt();
				form.angles = new float[lengthAngles];
				for (int y = 0; y < lengthAngles; y++)
				{
					form.angles[y] = packet.ReadFloat();
				}
				//form.lineFormationObject = deserializeGameObject(packet);
				lineFormations = deserializeGameObject(packet);
				int lengthFrontLines = packet.ReadInt();
				int[] linesInForntEmpty = new int[lengthFrontLines];
				//form.frontLines = new Lines[lengthFrontLines];
				for (int z = 0; z < lengthFrontLines; z++)
				{
					linesInForntEmpty[z] = packet.ReadInt();
					//int lengthLineIndexers = packet.ReadInt();
					//form.frontLines[z] = new Lines();
					//form.frontLines[z].linesindexes = new int[lengthLineIndexers];
					//for (int d = 0; d < lengthLineIndexers; d++)
					//{
					//	form.frontLines[z].linesindexes[d] = packet.ReadInt();
					//}
				}
				int lengthAllChilds = packet.ReadInt();
				FormationChild[] formationChilds = new FormationChild[lengthAllChilds];
				for (int y = 0; y < lengthAllChilds; y++)
				{
					int line = packet.ReadInt();
					int inFrontIndex = packet.ReadInt();
					int inBackIndex = packet.ReadInt();
					Transform inFront = null;
					Transform inBack = null;
					if (inFrontIndex != -1)
						inFront = formationComponentsObject.transform.childs[inFrontIndex];
					if (inBackIndex != -1)
						inBack = formationComponentsObject.transform.childs[inBackIndex];

					formationChilds[y] = new FormationChild(formationComponentsObject.transform.childs[y], line, inFront, inBack);
				}
				//lengthofAllChilds viel zu viele Linien aber keine bessere Lösung gefunden
				List<int>[] frontLines = new List<int>[lengthFrontLines];
				int[] frontLinesMin = new int[lengthFrontLines];
				int linesCount = 0;
				List<int> linesOrder = new List<int>();
				for (int y = 0; y < lengthAllChilds; y++)
				{
					int myLine = formationChilds[y].line;
					if (frontLines[myLine-1] == null)
					{
						frontLines[myLine-1] = new List<int>();
						frontLinesMin[myLine - 1] = y;
						linesCount++;
					}
					if (!frontLines[myLine - 1].Contains(y))
					{
						frontLines[myLine - 1].Add(y);
					}
					if (!linesOrder.Contains(myLine))
						linesOrder.Add(myLine);
				}
				form.LineOrder = linesOrder.ToArray();
				form.frontLines = new Lines[linesCount];
				for (int y = 0; y < frontLines.Length; y++)
				{
					if (frontLines[y] != null)
					{
						form.frontLines[y] = new Lines();
						form.frontLines[y].linesInFrontEmpty = linesInForntEmpty[y];
						form.frontLines[y].linesindexes = new int[frontLines[y].Count];
						for (int z = 0; z < frontLines[y].Count; z++)
						{
							form.frontLines[y].linesindexes[z] = frontLines[y][z];
							form.frontLines[y].lineStart = frontLinesMin[y];
						}
					}
				}
				FormationObject formationObject = new FormationObject(formationComponentsObject.transform, formationChilds);
				form.formationObject = formationObject;
				formationSpecsTables[i] = form;
			}
			return formationSpecsTables;
		}

		static int GetLine(Transform transformFromGridTile, int lines, NormalComponentsObject lineFormationObject)
		{
			for (int i = 0; i < lineFormationObject.transform.childCount; i++)
			{
				if (transformFromGridTile.localPosition == lineFormationObject.transform.GetChild(i).localPosition)
				{
					int myLine = int.Parse(lineFormationObject.transform.GetChild(i).name);
					return lines - myLine;
				}
			}
			return -4;
		}

		static int GetLinesCount(int formationId, int controlledTroopsCount, Lines[] frontLines)
		{
			for (int i = 0; i < frontLines.Length; i++)
			{
				if (controlledTroopsCount >= frontLines[i].linesindexes[0] && controlledTroopsCount <= frontLines[i].linesindexes[frontLines[i].linesindexes.Length - 1])
					return i + 1;
				if (i == frontLines.Length - 1)
					return i + 1;
			}
			return 0;
		}

		public static Troops[] DeserializeTroops(byte[] data)
		{
			Packet packet = new Packet(data);
			int lengthTroops = packet.ReadInt();
			Troops[] allTroops = new Troops[lengthTroops];
			for (int i = 0; i < lengthTroops; i++)
			{
				int id = packet.ReadInt();
				string name = packet.ReadString();
				TroopClass troopClass = 0;
				int amount = 0;
				foreach(TroopClass enumclass in Enum.GetValues(typeof(TroopClass)))
				{
					if(packet.ReadBool())
					{
						troopClass = troopClass | (TroopClass)(1 << amount);
					}
					amount++;
				}
				float maxHealth = packet.ReadFloat();
				float attackSpeed = packet.ReadFloat();
				float moveSpeed = packet.ReadFloat();
				float placeRadius = packet.ReadFloat();
				float damage = packet.ReadFloat();
				float attackRadius = packet.ReadFloat();
				int maxTroops = packet.ReadInt();
				Troops troop = new Troops(id, name, troopClass, maxHealth, attackSpeed, moveSpeed, placeRadius, damage, attackRadius, maxTroops);
				allTroops[i] = troop;
			}
			return allTroops;
		}

		public static NormalComponentsObject deserializeGameObject(Packet packet)
		{
			Vector3 posParent = packet.ReadVector3();
			Quaternion rotParent = packet.ReadQuaternion();
			NormalComponentsObject returnobject = new NormalComponentsObject(new Transform(posParent, rotParent));
			returnobject.transform.name = packet.ReadString();
			int count = packet.ReadInt();
			for (int i = 0; i < count; i++)
			{
				Vector3 position = packet.ReadVector3();
				Quaternion rotation = packet.ReadQuaternion();
				string name = packet.ReadString();
				Transform child = new Transform(position, rotation, returnobject.transform);
				child.name = name;
			}
			return returnobject;
		}

		public static T DeepClone<T>(this T obj)
		{
			using (var ms = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				ms.Position = 0;

				return (T)formatter.Deserialize(ms);
			}
		}
	}
}
