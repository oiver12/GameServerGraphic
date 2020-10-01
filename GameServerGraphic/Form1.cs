using System;
using System.Windows.Forms;
using GameServer;
using System.Drawing;
using System.IO;
using Pathfinding;
using MEC;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameServerGraphic
{
	public partial class Form1 : Form
	{
		public static Image troopImagePrefab;
		public static Timing instanceTiming;
		static AstarPath astarpath;
		static bool isRunning;
		public static Control.ControlCollection myControls;
		public static List<Tuple<Transform, PictureBox>> troopsImages = new List<Tuple<Transform, PictureBox>>();
		public static List<Tuple<Vector3, PictureBox>> wayPoints = new List<Tuple<Vector3, PictureBox>>();
		public static int placedTroops = 0;
		const float minX = 70;
		const float maxX = 494;
		const float minZ = -239;
		const float maxZ = 185;
		static float xLenght;
		static float zLength;
		const int mapSizeX = 999;
		const int mapSizeY = 999;

		public Form1()
		{
			InitializeComponent();
			xLenght = Mathf.Abs(maxX) - minX;
			zLength = Mathf.Abs(maxZ) - minZ;

			//size of map Image --> Display it in full Size
			ClientSize = new Size(mapSizeX, mapSizeY);
			//display Background Image --> Same as MiniMap on Client
			PictureBox pictureBox1 = new PictureBox();
			pictureBox1.ImageLocation = @"..\..\Mini Map.png";
			pictureBox1.Size = new Size(mapSizeX, mapSizeY);
			Controls.Add(pictureBox1);

			myControls = Controls;

			//Rennen von Update Loop während dem Idle
			Application.Idle += HandleApplicationIdle;

			//Laden von GaphData, gespeichert von Unity
			byte[] astardatabytes;
			using (var stream = new FileStream(@"..\..\GraphCache1.bytes", FileMode.Open))
			{
				astardatabytes = new byte[(int)stream.Length];
				stream.Read(astardatabytes, 0, (int)stream.Length);
			}

			//Laden von FormationDaten, gespeichert in Unity
			byte[] formationManagerBytes;
			using (var stream = new FileStream(@"..\..\FormationManager.bytes", FileMode.Open))
			{
				formationManagerBytes = new byte[(int)stream.Length];
				stream.Read(formationManagerBytes, 0, (int)stream.Length);
			}
			FormationManager.formations = DeserializeObjects.deserializeFormationManager(formationManagerBytes);

			//Laden von TruppenDaten, gespeichert in Unity
			byte[] troopData;
			using (var stream = new FileStream(@"..\..\TroopsData.bytes", FileMode.Open))
			{
				troopData = new byte[(int)stream.Length];
				stream.Read(troopData, 0, (int)stream.Length);
			}
			Server.allTroops = DeserializeObjects.DeserializeTroops(troopData);

			//Leden von dem Square Bild für die Truppen
			troopImagePrefab = Image.FromFile(@"..\..\troopImagePrefab.png");
			instanceTiming = Timing.Instance;
			astarpath = new AstarPath(astardatabytes);
			isRunning = true;
			Thread mainThread = new Thread(new ThreadStart(MainThread));
			mainThread.Start();
			//MainThread();
			Server.Start(50, 8000);
		}

		private void MainThread()
		{
			Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
			//TroopComponents troopTest = new TroopComponents(new Transform(new Vector3(374.1f, -67.59f, -8.1f), Quaternion.Identity), new Seeker(), new RichAI(), new AttackingSystem(), new PlayerController(), new CommanderScript());
			//troopTest.seeker.StartPath(troopTest.transform.position, new Vector3(troopTest.transform.position.x + 10f, troopTest.transform.position.y, troopTest.transform.position.z), OnPathComplete);
			DateTime _nextLoop = DateTime.Now;
			while (isRunning)
			{
				while (_nextLoop < DateTime.Now)
				{
					//troopTest.richAI.Update();
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

		//public static void OnPathComplete(Pathfinding.Path p)
		//{
		//	Image wayPointPrefab = Image.FromFile(@"..\..\redsquare.jpg");
		//	for (int i = 0; i < p.vectorPath.Count; i++)
		//	{
		//		PictureBox troopImage = new PictureBox();
		//		troopImage.SizeMode = PictureBoxSizeMode.StretchImage;
		//		troopImage.ClientSize = new Size(10, 10);
		//		troopImage.Image = wayPointPrefab;
		//		wayPoints.Add(new Tuple<Vector3, PictureBox>(p.vectorPath[i], troopImage));
		//		UIThreadManager.ExecuteOnMainThread(() => myControls.Add(troopImage));
		//	}
		//}

		void HandleApplicationIdle(object sender, EventArgs e)
		{
			while (IsApplicationIdle())
			{
				Render();
			}
		}

		void Render()
		{
			for (int i = 0; i < troopsImages.Count; i++)
			{
				Vector3 troopPos = troopsImages[i].Item1.position;
				float relativx = (troopPos.x - minX) / xLenght;
				float relativz = (troopPos.z - minZ) / zLength;
				Debug.Log(relativx);
				relativx = Mathf.Clamp01(relativx);
				relativz = Mathf.Clamp01(relativz);
				troopsImages[i].Item2.Location = new Point((int)(mapSizeX - (mapSizeX * relativx)), (int)(mapSizeY * relativz));
				troopsImages[i].Item2.BringToFront();
			}
			for (int i = 0; i < wayPoints.Count; i++)
			{
				Vector3 troopPos = wayPoints[i].Item1;
				float relativx = (troopPos.x - minX) / xLenght;
				float relativz = (troopPos.z - minZ) / zLength;
				wayPoints[i].Item2.Location = new Point((int)(mapSizeX - (mapSizeX * relativx)), (int)(mapSizeY * relativz));
				wayPoints[i].Item2.BringToFront();
			}
			//Debug.Log(myControls[1].Location);
			UIThreadManager.UpdateMain();
		}
		
		bool IsApplicationIdle()
		{
			NativeMessage result;
			return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NativeMessage
		{
			public IntPtr Handle;
			public uint Message;
			public IntPtr WParameter;
			public IntPtr LParameter;
			public uint Time;
			public Point Location;
		}

		[DllImport("user32.dll")]
		public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

		//private void pictureBox1_Click(object sender, EventArgs e)
		//{
		//	pictureBox1.Location = new Point(50, 10);
		//}
#if graphic
		public static void AddTroop(Transform troopTransform)
		{
			PictureBox troopImage = new PictureBox();
			troopImage.SizeMode = PictureBoxSizeMode.StretchImage;
			troopImage.ClientSize = new Size(10, 10);
			troopImage.Image = troopImagePrefab;
			troopsImages.Add(new Tuple<Transform, PictureBox>(troopTransform, troopImage));
			UIThreadManager.ExecuteOnMainThread(() => myControls.Add(troopImage));
		}

		public static void RemoveTroop(Transform transform)
		{
			for (int i = 0; i < troopsImages.Count; i++)
			{
				if(troopsImages[i].Item1 == transform)
				{
					PictureBox pic = troopsImages[i].Item2;
					UIThreadManager.ExecuteOnMainThread(() => myControls.Remove(pic));
					troopsImages.RemoveAt(i);
				}
			}
		}
#endif
	}
}
