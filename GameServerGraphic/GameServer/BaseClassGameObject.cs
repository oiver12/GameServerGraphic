using System;
namespace GameServer
{
	[Serializable]
	public class BaseClassGameObject
	{
		[System.Xml.Serialization.XmlIgnore]
		public Transform transform { get; set; }
		public bool isDestroyed = false;

		//TODO not good better way of delete all references
		public virtual void DestroyObject()
		{
			isDestroyed = true;
			transform = null;
		}
	}
}
