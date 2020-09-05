using System;
using System.Windows.Forms;
using GameServer;
using System.Drawing;
using System.IO;
using Pathfinding;
using GameServer;
using MEC;
using System.Threading;
using System.Collections.Generic;

namespace GameServerGraphic
{
	public partial class Form1 : Form
	{

		public static Timing instanceTiming;
		static AstarPath astarpath;
		static bool isRunning;

		public Form1()
		{
			InitializeComponent();
			pictureBox1.ImageLocation = @"C:\Users\Oliver\Documents\Visual Studio 2017\Projects\GameServerGraphic\GameServerGraphic\Test1.png";
			pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
			Debug.Log(pictureBox1.ImageLocation);


			byte[] astardatabytes;
			using (var stream = new FileStream(@"C:\Users\Oliver\Documents\Visual Studio 2017\Projects\GameServerGraphic\GameServerGraphic\GraphCache1.bytes", FileMode.Open))
			{
				astardatabytes = new byte[(int)stream.Length];
				stream.Read(astardatabytes, 0, (int)stream.Length);
			}

			byte[] formationManagerBytes;
			using (var stream = new FileStream(@"C:\Users\Oliver\Documents\Visual Studio 2017\Projects\GameServerGraphic\GameServerGraphic\FormationManager.bytes", FileMode.Open))
			{
				formationManagerBytes = new byte[(int)stream.Length];
				stream.Read(formationManagerBytes, 0, (int)stream.Length);
			}
			FormationManager.formations = DeserializeObjects.deserializeFormationManager(formationManagerBytes);

			byte[] troopData;
			using (var stream = new FileStream(@"C:\Users\Oliver\Documents\Visual Studio 2017\Projects\GameServerGraphic\GameServerGraphic\TroopsData.bytes", FileMode.Open))
			{
				troopData = new byte[(int)stream.Length];
				stream.Read(troopData, 0, (int)stream.Length);
			}
			Server.allTroops = DeserializeObjects.DeserializeTroops(troopData);
			instanceTiming = Timing.Instance;
			astarpath = new AstarPath(astardatabytes);
			isRunning = true;
			Thread mainThread = new Thread(new ThreadStart(MainThread));
			mainThread.Start();
			Server.Start(50, 8000);
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
					astarpath.Update();
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

		private void pictureBox1_Click(object sender, EventArgs e)
		{
			pictureBox1.Location = new Point(50, 10);
		}
	}
}
