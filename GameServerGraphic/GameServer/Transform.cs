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

		private Transform() { }

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
					childs[i].position += translation;
				}
				m_postion = value;
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

				//eine rotations Matrix mit der Differenz der beiden Rotationen Analog zu (trans = Vector3 translation = value - m_postion; aus position)
				if(troopObject != null && troopObject.commanderScript != null && (value * Quaternion.Inverse(m_rotation)).Length() < 0.9f)
					Debug.Log("OK");
				var moveMatrix = System.Numerics.Matrix4x4.CreateFromQuaternion((value * Quaternion.Inverse(m_rotation)).ToSytemNumericQuaternion());
				m_rotation = value;
				for (int i = 0; i < childs.Count; i++)
				{
					//die Rotationsmatrix des Kindes
					var childMatrix = System.Numerics.Matrix4x4.CreateFromQuaternion(childs[i].rotation.ToSytemNumericQuaternion());
					//anwenden der Rotationsmatrix auf das Kind und dann schlussendlich in die Einzelteile zerlegen
					childMatrix = childMatrix * moveMatrix;
					System.Numerics.Matrix4x4.Decompose(childMatrix, out var scale, out var Localrotation, out var translation);
					childs[i].rotation = (Quaternion)Localrotation;
					//die Position ist: Rotationspunkt verschieben in den Ursprung, Rotieren, Punkt wieder zurück verschieben
					childs[i].position = position + (Vector3)moveMatrix.MutliplyPoint((childs[i].m_postion - position).ToSytemNumericVector3());
					//Debug.Log(Quaternion.ToEulerAngles((Quaternion)Localrotation) + "LOc");
					//Debug.Log(position + (Vector3)moveMatrix.MutliplyPoint((childs[i].m_postion - position).ToSytemNumericVector3()));
				}
			}
		}

		public Vector3 eulerAngles
		{
			get
			{
				return Quaternion.ToEulerAngles(m_rotation);
			}

			set
			{
				rotation = Quaternion.Euler(value);
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
				if (value == null)
				{
					if(m_parent != null)
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

		//public Quaternion localRotation
		//{
		//	get
		//	{
		//		if (parent == null)
		//		{
		//			return m_rotation;
		//		}
		//		var testMatrix = System.Numerics.Matrix4x4.CreateTranslation(parent.position.ToSytemNumericVector3()) * System.Numerics.Matrix4x4.CreateFromQuaternion(parent.rotation.ToSytemNumericQuaternion()) * System.Numerics.Matrix4x4.CreateScale(1f);
		//		System.Numerics.Matrix4x4.Invert(testMatrix, out var parentRtsMatrix);
		//		var childMatrix = System.Numerics.Matrix4x4.CreateTranslation(position.ToSytemNumericVector3()) * System.Numerics.Matrix4x4.CreateFromQuaternion(rotation.ToSytemNumericQuaternion()) * System.Numerics.Matrix4x4.CreateScale(1f);
		//		childMatrix = parentRtsMatrix * childMatrix;
		//		System.Numerics.Matrix4x4.Decompose(childMatrix, out var scale, out var Localrotation, out var translation);
		//		return (Quaternion)Localrotation;
		//	}
		//	set
		//	{
		//		var parentRTSMatrix = System.Numerics.Matrix4x4.CreateTranslation(parent.position.ToSytemNumericVector3()) * System.Numerics.Matrix4x4.CreateFromQuaternion(parent.rotation.ToSytemNumericQuaternion()) * System.Numerics.Matrix4x4.CreateScale(1f);
		//		var childMatrix = System.Numerics.Matrix4x4.CreateTranslation(localPosition.ToSytemNumericVector3()) * System.Numerics.Matrix4x4.CreateFromQuaternion(value.ToSytemNumericQuaternion()) * System.Numerics.Matrix4x4.CreateScale(1f);
		//		childMatrix = parentRTSMatrix * childMatrix;
		//		System.Numerics.Matrix4x4.Decompose(childMatrix, out var scale, out var rotationMAtrix, out var translation);
		//		m_rotation = (Quaternion)rotationMAtrix;
		//	}
		//}

		public void Translate(Vector3 translation, Space relativTo)
		{
			if (relativTo == Space.World)
				position += translation;
			else
				localPosition += translation;
		}

		//public Vector3 TransformPoint(Vector3 point)
		//{
		//	return Vector3.zero;
		//}

		public void MoveWithoutChilds(Vector3 newPosition)
		{
			m_postion = newPosition;
		}
	}
}