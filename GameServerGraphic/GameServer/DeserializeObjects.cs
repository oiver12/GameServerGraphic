using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GameServer
{
	public static class DeserializeObjects
	{
		public static FormationSpecsTable[] deserializeFormationManager(byte[] data)
		{
			Packet packet = new Packet(data);
			int lengthformationManager = packet.ReadInt();
			FormationSpecsTable[] formationSpecsTables = new FormationSpecsTable[lengthformationManager];
			for (int i = 0; i < lengthformationManager; i++)
			{
				FormationSpecsTable form = new FormationSpecsTable();
				form.formationObject = deserializeGameObject(packet);
				form.formationId = packet.ReadInt();
				form.formationName = packet.ReadString();
				form.capacity = packet.ReadInt();
				int lengthAngles = packet.ReadInt();
				form.angles = new float[lengthAngles];
				for (int y = 0; y < lengthAngles; y++)
				{
					form.angles[y] = packet.ReadFloat();
				}
				form.lineFormationObject = deserializeGameObject(packet);
				int lengthFrontLines = packet.ReadInt();
				form.frontLines = new Lines[lengthFrontLines];
				for (int z = 0; z < lengthFrontLines; z++)
				{
					int lengthLineIndexers = packet.ReadInt();
					form.frontLines[z] = new Lines();
					form.frontLines[z].linesindexes = new int[lengthLineIndexers];
					for (int d = 0; d < lengthLineIndexers; d++)
					{
						form.frontLines[z].linesindexes[d] = packet.ReadInt();
					}
				}
				formationSpecsTables[i] = form;
			}
			return formationSpecsTables;
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
						Debug.Log(troopClass);
						troopClass = troopClass | (TroopClass)(1 << amount);
						Debug.Log(troopClass);
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
			NormalComponentsObject returnobject = new NormalComponentsObject(new Transform(Vector3.zero, Quaternion.Identity));
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
