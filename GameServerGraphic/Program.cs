using System;
using System.Windows.Forms;
using GameServer;
using Pathfinding;
using System.IO;
using System.Threading;
using MEC;

namespace GameServerGraphic
{
	static class Program
	{
		public static Timing instanceTiming;
		static AstarPath astarpath;
		static bool isRunning;
		/// <summary>
		/// Der Haupteinstiegspunkt für die Anwendung.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
			//byte[] astardatabytes;
			//using (var stream = new FileStream(@"C:\Users\Oliver\Documents\Visual Studio 2017\Projects\GameServerGraphic\GameServerGraphic\GraphCache1.bytes", FileMode.Open))
			//{
			//	astardatabytes = new byte[(int)stream.Length];
			//	stream.Read(astardatabytes, 0, (int)stream.Length);
			//}

			//byte[] formationManagerBytes;
			//using (var stream = new FileStream(@"C:\Users\Oliver\Documents\Visual Studio 2017\Projects\GameServerGraphic\GameServerGraphic\FormationManager.bytes", FileMode.Open))
			//{
			//	formationManagerBytes = new byte[(int)stream.Length];
			//	stream.Read(formationManagerBytes, 0, (int)stream.Length);
			//}
			//FormationManager.formations = DeserializeObjects.deserializeFormationManager(formationManagerBytes);

			//byte[] troopData;
			//using (var stream = new FileStream(@"C:\Users\Oliver\Documents\Visual Studio 2017\Projects\GameServerGraphic\GameServerGraphic\TroopsData.bytes", FileMode.Open))
			//{
			//	troopData = new byte[(int)stream.Length];
			//	stream.Read(troopData, 0, (int)stream.Length);
			//}
			//Server.allTroops = DeserializeObjects.DeserializeTroops(troopData);

			//astarpath = new AstarPath(astardatabytes);
			//isRunning = true;
			//Thread mainThread = new Thread(new ThreadStart(MainThread));
			//mainThread.Start();

			//Server.Start(50, 8000);
		}
		public static void OnPathComplete(Pathfinding.Path p)
		{
			Debug.Log(p.vectorPath.Count);
		}

		private static void MainThread()
		{
			Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
			DateTime _nextLoop = DateTime.Now;
			while (isRunning)
			{
				while (_nextLoop < DateTime.Now)
				{
					// If the time for the next loop is in the past, aka it's time to execute another tick
					GameLogic.Update(); // Execute game logic

					_nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK); // Calculate at what point in time the next tick should be executed
					Time.time += Constants.MS_PER_TICK;
					Time.frameCount++;
					if (_nextLoop > DateTime.Now)
					{
						// If the execution time for the next tick is in the future, aka the server is NOT running behind
						Thread.Sleep(_nextLoop - DateTime.Now); // Let the thread sleep until it's needed again.
					}
				}
			}
		}
	}
}

namespace GameServer
{
	public static class SystemInfo
	{
		public static int systemMemorySize = 16316;
	}
}
