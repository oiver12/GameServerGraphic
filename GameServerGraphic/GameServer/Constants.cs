﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
	class Constants
	{
		public const int TICKS_PER_SEC = 30; // How many ticks per second
		public const float MS_PER_TICK = 1000f / TICKS_PER_SEC; // How many milliseconds per tick
		public const float TimeBetweenFrame = 1f / TICKS_PER_SEC;
	}
}
