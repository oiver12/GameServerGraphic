using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	public static class GameServerRandom
	{
		public static Vector3 randomSpherePoint (Random rand)
		{
			float u = (float)rand.NextDouble();
			float v = (float)rand.NextDouble();
			var theta = 2 * Math.PI * u;
			var phi = Math.Acos(2 * v - 1);
			var x = (1f * Math.Sin(phi) * Math.Cos(theta));
			var y = (1f * Math.Sin(phi) * Math.Sin(theta));
			var z = (1 * Math.Cos(phi));
			return new Vector3((float)x, (float)y, (float)z);
		}
}
}
