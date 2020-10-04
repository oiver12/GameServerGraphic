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
		public static Graphics myGraphics;
		public static List<Tuple<Transform, PictureBox>> troopsImages = new List<Tuple<Transform, PictureBox>>();
		public static int placedTroops = 0;
		const float minX = 70;
		const float maxX = 494;
		const float minZ = -239;
		const float maxZ = 185;
		static float xLenght;
		static float zLength;
		const int mapSizeX = 999;
		const int mapSizeY = 999;
		static float mapScale = 1;
		static PictureBox mapPicture;
		Vector2 offsetFromCenter = new Vector2(0f, 0f);
		float walkSpeedGraphics = 500f;
		float zoomSpeedGraphics = 0.75f;

		float randomFloat(Random rand)
		{
			return (float)(rand.NextDouble() * (300f - (-100f)) + (-100f));
		}

		public Form1()
		{
			InitializeComponent();
			xLenght = Mathf.Abs(maxX) - minX;
			zLength = Mathf.Abs(maxZ) - minZ;

			//size of map Image --> Display it in full Size
			ClientSize = new Size(mapSizeX, mapSizeY);
			//display Background Image --> Same as MiniMap on Client
			mapPicture = new PictureBox();
			mapPicture.ImageLocation = @"..\..\Mini Map.png";
			mapPicture.SizeMode = PictureBoxSizeMode.Zoom;
			mapPicture.Size = new Size(mapSizeX, mapSizeY);
			Controls.Add(mapPicture);
			this.KeyPreview = true;
			this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(Form1_MouseWheel);
			myControls = Controls;
			myGraphics = this.CreateGraphics();

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
			Vector2 f = TranslateToNewMap(new Vector2(0f, 0f));
			for (int i = 0; i < troopsImages.Count; i++)
			{
				Vector3 troopPos = troopsImages[i].Item1.position;
				//troopPos = ConvertToLocal(new Vector2(troopPos.x, troopPos.z));
				//Debug.Log(ConvertToLocal(troopPos));
				float relativx = (troopPos.x - minX) / xLenght;
				float relativz = (troopPos.z - minZ) / zLength;
				relativx = Mathf.Clamp01(relativx);
				relativz = Mathf.Clamp01(relativz);
				Vector2 test = new Vector2((mapSizeX - (mapSizeX * relativx)), (mapSizeY * relativz));
				test = TranslateToNewMap(test);
				troopsImages[i].Item2.Location = new Point((int)test.x, (int)test.y);
				troopsImages[i].Item2.BringToFront();
			}
			//Debug.Log(myControls[1].Location);
			UIThreadManager.UpdateMain();
		}

		static Point GetInCam(Vector3 worldPos)
		{
			//troopPos = ConvertToLocal(new Vector2(troopPos.x, troopPos.z));
			//Debug.Log(ConvertToLocal(troopPos));
			float relativx = (worldPos.x - minX) / xLenght;
			float relativz = (worldPos.z - minZ) / zLength;
			relativx = Mathf.Clamp01(relativx);
			relativz = Mathf.Clamp01(relativz);
			Vector2 test = new Vector2((mapSizeX - (mapSizeX * relativx)), (mapSizeY * relativz));
			test = TranslateToNewMap(test);
			return new Point((int)test.x, (int)test.y);
		}

		static Vector2 TranslateToNewMap(Vector3 troopPos)
		{
			Matrix4x4 rtsMatrix = Matrix4x4.TRS(new Vector2(mapPicture.Location.X, mapPicture.Location.Y), Quaternion.Identity, new Vector3(mapScale, mapScale, mapScale));
			return rtsMatrix.MultiplyPoint(troopPos);
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

		public static void DrawPointAt(Vector3 worldPosition, int width)
		{
			Point camPoint = GetInCam(worldPosition);
			UIThreadManager.ExecuteOnMainThread(() => myGraphics.FillRectangle(Brushes.Black, camPoint.X, camPoint.Y, width, width));
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == 'w')
			{
				offsetFromCenter.y += walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
			}
			if (e.KeyChar == 's')
			{
				offsetFromCenter.y -= walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
			}
			if (e.KeyChar == 'a')
			{
				offsetFromCenter.x += walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
			}
			if (e.KeyChar == 'd')
			{
				offsetFromCenter.x -= walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
			}
		}

		private void Form1_MouseWheel(object sender, MouseEventArgs e)
		{
			int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
			mapScale += numberOfTextLinesToMove * zoomSpeedGraphics * Time.deltaTime;
			mapPicture.Size = new Size((int)(mapSizeX * mapScale), (int)(mapSizeY * mapScale));
			mapPicture.Location = new Point((int)(mapSizeX / 2 - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)(mapSizeY / 2 - mapPicture.Size.Height / 2 + offsetFromCenter.y));
		}
#endif

		//void TestKDTree()
		//{
		//	var tree = new KdTree.KdTree<float, int>(2, new FloatMath());
		//	var newTree = new KdTree<BaseClassGameObject>();
		//	Random rand = new Random();
		//	for (int i = 0; i < 2000; i++)
		//	{
		//		float pointX = randomFloat(rand);
		//		float pointY = randomFloat(rand);
		//		tree.Add(new[] { pointX, pointY }, 1);
		//		newTree.Add(new BaseClassGameObject() { transform = new Transform(new Vector2(pointX, pointY), Quaternion.Identity) });
		//	}
		//	var timer = new System.Diagnostics.Stopwatch();
		//	float sum1 = 0f;
		//	float sum2 = 0f;
		//	for (int g = 0; g < 1000; g++)
		//	{
		//		timer.Reset();
		//		timer.Start();
		//		var nearest = tree.GetNearestNeighbours(new[] { 50f, 10f }, 1);
		//		//var nearest = tree.RadialSearch(new[] { 50f, 10f }, 100f);
		//		timer.Stop();
		//		sum1 += timer.ElapsedMilliseconds;
		//	}
		//	for (int g = 0; g < 1000; g++)
		//	{
		//		timer.Reset();
		//		timer.Start();
		//		var nearest = newTree.FindClosest(new Vector2(50f, 10f));
		//		//var nearest = newTree.FindClose(new Vector2(50f, 10f));
		//		timer.Stop();
		//		sum2 += timer.ElapsedMilliseconds;
		//	}
		//	sum1 /= 1000f;
		//	sum2 /= 1000f;
		//	Debug.Log("Form First: " + sum1 + ". From Second: " + sum2);
		//}
	}
}
