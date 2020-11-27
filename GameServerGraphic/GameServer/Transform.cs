using System.Collections.Generic;
using GameServerGraphic;


namespace GameServer
{
	public enum Space
	{
		World,
		Self
	}

	[System.Serializable]
	public class Transform
	{

		public List<Transform> childs = new List<Transform>();

		public Transform(Vector3 pos, Quaternion q, Transform par = null)
		{
			position = pos;
			rotation = q;
			if(par != null)
				parent = par;
		}

		Vector3 m_postion;
		Quaternion m_rotation;
		//Vector3 m_localPosition;
		string m_tag;
		Transform m_parent;
		public TroopComponents troopObject;
		public NormalComponentsObject normalComponents;
		public string name;

		public string tag
		{
			get
			{
				return m_tag;
			}
			set
			{
				m_tag = value;
			}
		}

		public Vector3 position
		{
			get
			{
				return m_postion;
			}
			set
			{
				Vector3 translation = value - m_postion;
				for (int i = 0; i < childs.Count; i++)
				{
					//childs[i].localPosition = childs[i].localPosition;
					childs[i].position += translation;
				}
				//if(Form1.placedTroops >= 3f)
				//{
				//	if (troopObject != null && troopObject.commanderScript == null)
				//	{
				//		//Breakpoint
				//		var asdas = Server.clients;
				//		Debug.Log("OK");
				//	}
				//}
				m_postion = value;
				//if(parent != null)
				//{
				//	Matrix4x4 rtsMatrix = Matrix4x4.TRS(parent.position, parent.rotation, Vector3.one);
				//	rtsMatrix = Matrix4x4.InvertMatrix(rtsMatrix);
				//	//m_localPosition = rtsMatrix.MultiplyPoint(m_postion);
				//}
			}
		}

		public Quaternion rotation
		{
			get
			{
				if (float.IsNaN(m_rotation.w))
					throw new System.Exception("Rot was NaN");
				return m_rotation;
			}
			set
			{
				if (float.IsNaN(value.w))
					throw new System.Exception("Rot was NaN");
				Quaternion oldrotation = m_rotation;
				m_rotation = value;
				for (int i = 0; i < childs.Count; i++)
				{
					Matrix4x4 rtsMatrix = Matrix4x4.TRS(m_postion, oldrotation, Vector3.one);
					rtsMatrix = Matrix4x4.InvertMatrix(rtsMatrix);
					Vector3 oldlocalposition = rtsMatrix.MultiplyPoint(childs[i].position);
					//Debug.Log(op);
					childs[i].localPosition = oldlocalposition;
					//Debug.Log(childs[i].localPosition);
				}
			}
		}

		public Transform parent
		{
			get
			{
				return m_parent;
			}
			set
			{
				if(value == null)
				{
					m_parent.childs.Remove(this);
					m_parent = null;
					return;
				}
				if(m_parent != null && m_parent != value)
				{
					m_parent.childs.Remove(this);
				}
				if(!value.childs.Contains(this))
					value.childs.Add(this);

				m_parent = value;
			}
		}

		public Transform GetChild(int value)
		{
			return childs[value];
		}

		public int childCount
		{
			get
			{
				return childs.Count;
			}
		}

		public Vector3 forward
		{
			get
			{
				return rotation * Vector3.forward;
			}
			//set
			//{
			//	this.rotation = Quaternion.LookRotation(value);
			//}
		}

		public Vector3 localPosition
		{
			get
			{
				if (parent == null)
					return position;

				Matrix4x4 rtsMatrix = Matrix4x4.TRS(parent.position, parent.rotation, Vector3.one);
				rtsMatrix = Matrix4x4.InvertMatrix(rtsMatrix);
				return rtsMatrix.MultiplyPoint(m_postion);
			}
			set
			{
				if (parent == null)
				{
					m_postion = value;
					return;
				}
				Matrix4x4 rtsMatrix = Matrix4x4.TRS(parent.position, parent.rotation, Vector3.one);
				position = rtsMatrix.MultiplyPoint(value);
			}
		}

		public void Translate(Vector3 translation, Space relativTo)
		{
			if (relativTo == Space.World)
				position += translation;
			else
				localPosition += translation;
		}
	}
}