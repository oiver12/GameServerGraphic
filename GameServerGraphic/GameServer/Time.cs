using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
	public static class Time
	{
		public static float time;

		public static float deltaTime
		{
			get
			{
				return Constants.TimeBetweenFrame;
			}
		}

		public static float realtimeSinceStartup
		{
			get
			{
				return time;
			}
		}
		public static int frameCount;

		public static float fixedDeltaTime
		{
			get
			{
				return Constants.TimeBetweenFrame;
			}
		}

		public static float fixedTime
		{
			get
			{
				return time;
			}
		}
	}
}
