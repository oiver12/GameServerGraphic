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
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;

namespace GameServerGraphic
{
	public partial class Form1 : Form
	{
		public static Form1 instance;
		public static Image troopImagePrefab;
		public static Timing instanceTiming;
		static AstarPath astarpath;
		static bool isRunning;
		public static Control.ControlCollection myControls;
		public static Graphics myGraphics;
		public static List<Tuple<Transform, PictureBox>> troopsImages = new List<Tuple<Transform, PictureBox>>();
		public static List<Tuple<Vector3, PictureBox>> otherPoints = new List<Tuple<Vector3, PictureBox>>();
		public static int placedTroops = 0;
		public static bool isPaused
		{
			get
			{
				return _isPaused;
			}
			set
			{
				_isPaused = value;
				if (_isPaused)
					buttonPause.ImageIndex = 1;
				else
					buttonPause.ImageIndex = 0;
			}
		}
		static bool _isPaused = false;
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
		static RichTextBox textBoxStatic;
		static Button buttonPause;
		static Vector3 firstPosition = new Vector3(float.NaN, float.NaN, float.NaN);
		static Vector3 secondPosition = new Vector3(float.NaN, float.NaN, float.NaN);
		static Vector3 thirdPosition = new Vector3(float.NaN, float.NaN, float.NaN);
		static Vector3 fourthPosition = new Vector3(float.NaN, float.NaN, float.NaN);
		static BezierCurveXZPlane bezier;

		float randomFloat(Random rand)
		{
			return (float)(rand.NextDouble() * (300f - (-100f)) + (-100f));
		}

		public Form1()
		{
			instance = this;
			InitializeComponent();
			xLenght = Mathf.Abs(maxX) - minX;
			zLength = Mathf.Abs(maxZ) - minZ;
			textBoxStatic = textBox1;
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
			this.FormClosing += Form1_FormClosing;
			mapPicture.MouseDown += new System.Windows.Forms.MouseEventHandler(panel1_MouseDown);
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

			byte[] multiplierData;
			using (var stream = new FileStream(@"..\..\MultiplierData.bytes", FileMode.Open))
			{
				multiplierData = new byte[(int)stream.Length];
				stream.Read(multiplierData, 0, (int)stream.Length);
			}
			FormationTable[] formationTable;
			TroopTable[] troopTable;
			DeserializeObjects.DeserializeMultiplier(multiplierData, out formationTable, out troopTable);
			MultiplierManager.formationMultiplier = formationTable;
			MultiplierManager.troopMultiplier = troopTable;
			//Laden von dem Square Bild für die Truppen
			troopImagePrefab = Image.FromFile(@"..\..\troopImagePrefab.png");
			instanceTiming = Timing.Instance;
			astarpath = new AstarPath(astardatabytes);
			isRunning = true;
			Thread mainThread = new Thread(new ThreadStart(MainThread));
			mainThread.Start();
			button3.ImageList = new ImageList();
			button3.ImageList.Images.Add(Image.FromFile(@"..\..\Pause_Icon.jpg"));
			button3.ImageList.Images.Add(Image.FromFile(@"..\..\Play_Icon.png"));
			button3.ImageList.ImageSize = new Size(40, 40);
			button3.ImageIndex = 0;
			buttonPause = button3;
			//MainThread();
			Server.Start(50, 8000);
		}

		private void MainThread()
		{

			Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
			//Transform parent = new Transform(new Vector3(314, -62, 47), Quaternion.Euler(new Vector3(13, 20, 100)));
			//Transform child = new Transform(new Vector3(300, -50, 10), Quaternion.Euler(new Vector3(20, 10, 50)), parent);
			//parent.rotation = Quaternion.Euler(new Vector3(100, 200, 50));
			//Debug.Log(child.position);
			//TroopComponents troopTest = new TroopComponents(new Transform(new Vector3(374.1f, -67.59f, -8.1f), Quaternion.Identity), new Seeker(), new RichAI(), new AttackingSystem(), new PlayerController(), new CommanderScript());
			//troopTest.seeker.StartPath(troopTest.transform.position, new Vector3(troopTest.transform.position.x + 10f, troopTest.transform.position.y, troopTest.transform.position.z), OnPathComplete);
			DateTime _nextLoop = DateTime.Now;
			while (isRunning)
			{
				//Transform parent = new Transform(new Vector3(314, -62, 47), Quaternion.Euler(new Vector3(13, 20, 100)));
				//Transform child = new Transform(new Vector3(300, -50, 10), Quaternion.Euler(new Vector3(20, 10, 50)), parent);
				//Transform child2 = new Transform(new Vector3(330, -50, 45), Quaternion.Euler(new Vector3(10, 0, 6)), parent);
				//AddTransform(parent, 10, Color.Red);
				//AddTransform(child, 10, Color.Blue);
				//AddTransform(child2, 10, Color.Blue);
				//Thread.Sleep(1000);
				//Vector3 startEulerAngles = Quaternion.ToEulerAngles(parent.rotation);
				//for (int i = 0; i < 1000; i++)
				//{
				//	//parent.position += new Vector3(1, 0, 0);
				//	parent.rotation = parent.rotation * Quaternion.Euler(new Vector3(10, 0, 0));
				//	Thread.Sleep(30);
				//}
				while (_nextLoop < DateTime.Now)
				{
					if (isPaused)
					{
						Thread.Sleep((int)Constants.MS_PER_TICK);
						continue;
					}
					//troopTest.richAI.Update();
					// If the time for the next loop is in the past, aka it's time to execute another tick
					GameLogic.Update(); // Execute game logic
					astarpath.Update();
					_nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK); // Calculate at what point in time the next tick should be executed
					//if (troopsImages.Count > 0)
					//{
					//	troopsImages[0].Item1.position += bezier.Move(20f);
					//}
					Time.time += Constants.MS_PER_TICK / 1000;
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
				//	Vector3 troopPos = troopsImages[i].Item1.position;
				//	//troopPos = ConvertToLocal(new Vector2(troopPos.x, troopPos.z));
				//	//Debug.Log(ConvertToLocal(troopPos));
				//	float relativx = (troopPos.x - minX) / xLenght;
				//	float relativz = (troopPos.z - minZ) / zLength;
				//	relativx = Mathf.Clamp01(relativx);
				//	relativz = Mathf.Clamp01(relativz);
				//	Vector2 test = new Vector2((mapSizeX - (mapSizeX * relativx)), (mapSizeY * relativz));
				//	test = TranslateToNewMap(test);
				//troopsImages[i].Item2.Image = RotateImage(troopsImages[i].Item2.Image, -200f);
				try
				{
					//Debug.Log("Jetzt");
					Point test = GetInCam(troopsImages[i].Item1.position);
					troopsImages[i].Item2.Location = new Point((int)test.X, (int)test.Y);
					troopsImages[i].Item2.BringToFront();
				}
				catch
				{
					Debug.Log("IMG not found");
				}
			}
			for (int i = 0; i < otherPoints.Count; i++)
			{
				Vector3 troopPos = otherPoints[i].Item1;
				//troopPos = ConvertToLocal(new Vector2(troopPos.x, troopPos.z));
				//Debug.Log(ConvertToLocal(troopPos));
				//float relativx = (troopPos.x - minX) / xLenght;
				//float relativz = (troopPos.z - minZ) / zLength;
				//relativx = Mathf.Clamp01(relativx);
				//relativz = Mathf.Clamp01(relativz);
				//Vector2 test = new Vector2((mapSizeX - (mapSizeX * relativx)), (mapSizeY * relativz));
				//test = TranslateToNewMap(test);
				Point test = GetInCam(troopPos);
				otherPoints[i].Item2.Location = new Point((int)test.X, (int)test.Y);
				otherPoints[i].Item2.BringToFront();
			}
			//Debug.Log(myControls[1].Location);
			UIThreadManager.UpdateMain();
		}

		public static Image RotateImage(Image img, float rotationAngle)
		{
			//create an empty Bitmap image
			Bitmap bmp = new Bitmap(img.Width, img.Height);

			//turn the Bitmap into a Graphics object
			Graphics gfx = Graphics.FromImage(bmp);

			//now we set the rotation point to the center of our image
			gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

			//now rotate the image
			gfx.RotateTransform(rotationAngle);

			gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

			//set the InterpolationMode to HighQualityBicubic so to ensure a high
			//quality image once it is transformed to the specified size
			gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

			//now draw our new image onto the graphics object
			gfx.DrawImage(img, new Point(0, 0));

			//dispose of our Graphics object
			gfx.Dispose();

			//return the image
			return bmp;
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
			if(troopTransform.troopObject.commanderScript == null)
				troopImage.ClientSize = new Size(7, 7);
			else
				troopImage.ClientSize = new Size(10 ,10);

			if (troopTransform.troopObject.playerController.myClient.player.isClone)
				troopImage.BackColor = Color.Red;
			else
				troopImage.BackColor = Color.Black;
			troopsImages.Add(new Tuple<Transform, PictureBox>(troopTransform, troopImage));
			UIThreadManager.ExecuteOnMainThread(() => myControls.Add(troopImage));
			UIThreadManager.ExecuteOnMainThread(() => troopImage.MouseClick += new MouseEventHandler(panel1_MouseDown));
		}

		public static void AddTransform(Transform troopTransform, int size, Color color)
		{
			PictureBox troopImage = new PictureBox();
			troopImage.SizeMode = PictureBoxSizeMode.StretchImage;
			troopImage.ClientSize = new Size(size, size);
			troopImage.BackColor = color;
			troopsImages.Add(new Tuple<Transform, PictureBox>(troopTransform, troopImage));
			UIThreadManager.ExecuteOnMainThread(() => myControls.Add(troopImage));
			UIThreadManager.ExecuteOnMainThread(() => troopImage.MouseClick += new MouseEventHandler(panel1_MouseDown));
		}

		public static void ChanceTroopColor(Transform transform, Color color)
		{
			for (int i = 0; i < troopsImages.Count; i++)
			{
				if (troopsImages[i].Item1 == transform)
				{
					PictureBox pic = troopsImages[i].Item2;
					pic.BackColor = color;
				}
			}
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

		public static void SpawnPointAt(Vector3 worldPosition, Color color, int width)
		{
			PictureBox point = new PictureBox();
			point.SizeMode = PictureBoxSizeMode.StretchImage;
			point.ClientSize = new Size(width, width);
			point.BackColor = color;
			otherPoints.Add(new Tuple<Vector3, PictureBox>(worldPosition, point));
			UIThreadManager.ExecuteOnMainThread(() => myControls.Add(point));
		}
		public static void UpdatePointPosition(int indexInTuple, Vector3 position)
		{
			//Tuples sind immutable also ersetzten, nicht sehr gut keine bessere Lösung gefunden
			Tuple<Vector3, PictureBox> tempTuple = new Tuple<Vector3, PictureBox>(position,otherPoints[indexInTuple].Item2);
			otherPoints[indexInTuple] = tempTuple;
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == 'w')
			{
				offsetFromCenter.y += walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
				e.Handled = true;
			}
			if (e.KeyChar == 's')
			{
				offsetFromCenter.y -= walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
				e.Handled = true;
			}
			if (e.KeyChar == 'a')
			{
				offsetFromCenter.x += walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
				e.Handled = true;
			}
			if (e.KeyChar == 'd')
			{
				offsetFromCenter.x -= walkSpeedGraphics * Time.deltaTime;
				mapPicture.Location = new Point((int)((mapSizeX / 2) - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)((mapSizeY / 2) - mapPicture.Size.Height / 2 + offsetFromCenter.y));
				e.Handled = true;
			}

			if(e.KeyChar == 'g')
			{
				firstPosition = new Vector3(float.NaN, float.NaN, float.NaN);
				secondPosition = new Vector3(float.NaN, float.NaN, float.NaN);
				thirdPosition = new Vector3(float.NaN, float.NaN, float.NaN);
				fourthPosition = new Vector3(float.NaN, float.NaN, float.NaN);
				UIThreadManager.ExecuteOnMainThread(() =>
				{
					for (int i = 0; i < otherPoints.Count; i++)
					{
						myControls.Remove(otherPoints[i].Item2);
					}
					otherPoints.Clear();
				}
				);
				e.Handled = true;
			}
			//escape Taste
			if(e.KeyChar == 27)
			{
				isRunning = false;
				this.Close();
			}
		}

		private void Form1_MouseWheel(object sender, MouseEventArgs e)
		{
			int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
			mapScale += numberOfTextLinesToMove * zoomSpeedGraphics * Time.deltaTime;
			mapPicture.Size = new Size((int)(mapSizeX * mapScale), (int)(mapSizeY * mapScale));
			mapPicture.Location = new Point((int)(mapSizeX / 2 - mapPicture.Size.Width / 2 + offsetFromCenter.x), (int)(mapSizeY / 2 - mapPicture.Size.Height / 2 + offsetFromCenter.y));
		}

		private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
		{
			isRunning = false;
		}

		public static void AddToFormConsol(string text, bool error = false)
		{
			if (!error)
			{
				UIThreadManager.ExecuteOnMainThread(() =>
				{ 
					textBoxStatic.AppendText(text + "\r\n", Color.Black);
				}
				);
			}
			else
			{
				UIThreadManager.ExecuteOnMainThread(() =>
				{
					textBoxStatic.AppendText(text + "\r\n", Color.Red);
				}
				);
			}
		}

		private static void panel1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			for (int i = 0; i < troopsImages.Count; i++)
			{
				if (troopsImages[i].Item2 == sender)
				{
					TroopComponents thisTroops = troopsImages[i].Item1.troopObject;
					PlayerController.playerIdNowStop = i;
					isPaused = true;
					//for (int y = 0; y < thisTroops.playerController.Mycommander.commanderScript.formationObject.formationObjects.Length; y++)
					//{
					//	SpawnPointAt(thisTroops.playerController.Mycommander.commanderScript.formationObject.formationObjects[y].transform.position, Color.Beige, 5);
					//}
					//ChanceTroopColor(thisTroops.transform, Color.Pink);
					//string output = thisTroops.SerializeObject();
					Debug.Log(thisTroops.transform.name);
					//isPaused = false;
					return;
				}
			}
			HermitCurveTest();
		}

		static void HermitCurveTest()
		{
			if (float.IsNaN(firstPosition.x))
			{
				Point relativPoint = instance.PointToClient(new Point(MousePosition.X, MousePosition.Y));
				firstPosition = AstarPath.active.GetNearest(CamToWorldSpace(new Vector2(relativPoint.X, relativPoint.Y)), NNConstraint.Default).position;
				SpawnPointAt(firstPosition, Color.Red, 10);
			}
			else if (float.IsNaN(secondPosition.x))
			{
				Point relativPoint = instance.PointToClient(new Point(MousePosition.X, MousePosition.Y));
				secondPosition = AstarPath.active.GetNearest(CamToWorldSpace(new Vector2(relativPoint.X, relativPoint.Y)), NNConstraint.Default).position;
				SpawnPointAt(secondPosition, Color.Red, 10);
			}
			else if (float.IsNaN(thirdPosition.x))
			{
				Point relativPoint = instance.PointToClient(new Point(MousePosition.X, MousePosition.Y));
				thirdPosition = AstarPath.active.GetNearest(CamToWorldSpace(new Vector2(relativPoint.X, relativPoint.Y)), NNConstraint.Default).position;
				SpawnPointAt(thirdPosition, Color.Red, 10);
			}
			else if (float.IsNaN(fourthPosition.x))
			{
				Point relativPoint = instance.PointToClient(new Point(MousePosition.X, MousePosition.Y));
				fourthPosition = AstarPath.active.GetNearest(CamToWorldSpace(new Vector2(relativPoint.X, relativPoint.Y)), NNConstraint.Default).position;
				SpawnPointAt(fourthPosition, Color.Red, 10);
				float radius;
				Vector3 firstDir = (secondPosition - firstPosition);
				Vector3 secondDir = (thirdPosition - fourthPosition);
				//firstDir = Quaternion.Euler(0f, 180f, 0f) * secondDir;
				var stopWatch = new System.Diagnostics.Stopwatch();
				stopWatch.Start();
				//ExtensionMethods.SearchHermiteCurve(firstPosition, firstDir.normalized, thirdPosition, secondDir.normalized, out float radiusFirstDir, out float radiusSecondDir, out float distance);
				bezier = new BezierCurveXZPlane();
				bezier.BezierCurveXZPlaneFromHermitCurve(firstPosition, firstDir, thirdPosition, secondDir , true);
				bezier.CalculateBezierCurve(true);
				//Player.SearchBezierCurve(firstPosition, (secondPosition - firstPosition).normalized, thirdPosition, (thirdPosition - fourthPosition).normalized);
				//Player.DrawNewBezierCruve(new Vector2(firstPosition.x, firstPosition.z), new Vector2(firstDir.x, firstDir.z), new Vector2(thirdPosition.x, thirdPosition.z), new Vector2(secondDir.x, secondDir.z));
				//Vector3 middlePoint;
				//Vector3 pointBeforeFormation;
				//float radius;
				//float factorCircleSide;
				//Player.SearchCircle(firstPosition, secondPosition, dir.normalized, out middlePoint, out radius, out factorCircleSide, out pointBeforeFormation);
				stopWatch.Stop();
				Debug.Log("Time: " + stopWatch.ElapsedMilliseconds);
				//stopWatch.Reset();
				//stopWatch.Start();
				//for (int i = 0; i < 10000; i++)
				//{
				//	Vector3 test = bezier.Move(1);
				//}
				//stopWatch.Stop();
				//Debug.Log("Time: " + stopWatch.Elapsed);
			}
			else
			{
				AddTroop(new Transform(bezier.firstPosition, Quaternion.Identity));
				//Point relativPoint = instance.PointToClient(new Point(MousePosition.X, MousePosition.Y));
				//Vector3 searchPoint = AstarPath.active.GetNearest(CamToWorldSpace(new Vector2(relativPoint.X, relativPoint.Y)), NNConstraint.Default).position;
				//float t = bezier.NewtonApproximation(0.5f, new Vector2(searchPoint.x, searchPoint.z), 1000);
				////Debug.Log(bezier.B(0.567f));
				//Debug.Log(searchPoint);
				//Debug.Log(bezier.B(t));
				//Debug.Log(t);
				//Vector2 testPoint = bezier.B(t);
				//SpawnPointAt(new Vector3(testPoint.x, searchPoint.y, testPoint.y), Color.Blue, 20);
			}
		}

		static Vector3 CamToWorldSpace(Vector2 camPos)
		{
			Matrix4x4 rtsMatrix = Matrix4x4.TRS(new Vector2(mapPicture.Location.X, mapPicture.Location.Y), Quaternion.Identity, new Vector3(mapScale, mapScale, mapScale));
			Vector3 test = Matrix4x4.InvertMatrix(rtsMatrix).MultiplyPoint(camPos);
			//Vector2 test = camPos;
			float x = (mapSizeX- test.x) / mapSizeX;
			float z = (test.y) / mapSizeY;
			Vector2 asdas = new Vector2((mapSizeX - (mapSizeX * x)), (mapSizeY * z));
			Vector3 worldPos = new Vector3((x * xLenght) + minX, -67, (z * zLength) + minZ);
			return worldPos;
		}

		/// <summary>
		/// Serialize von clientId placebelTroops und placedTroops und vom enemyClient
		/// </summary>
		/// <param name="clientId"></param>
		static void SerializeGame(int clientId)
		{
			FileStream fs;
			if (!Server.clients[clientId].player.isClone)
				fs = new FileStream("TestSerialize.dat", FileMode.Create);
			else
				fs = new FileStream("TestSerializeEnemy.dat", FileMode.Create);

			BinaryFormatter formatter = new BinaryFormatter();
			MemoryStream placebelTroops = new MemoryStream();
			MemoryStream placedTroops = new MemoryStream();
			Packet _packet = new Packet();
			try
			{
				//_packet.Write(Server.clients[clientId].player.isClone);
				//_packet.Write(clientId);
				formatter.Serialize(placebelTroops, Server.clients[clientId].player.placebelTroops);
				_packet.Write(placebelTroops.ToArray().Length);
				_packet.Write(placebelTroops.ToArray());

				formatter.Serialize(placedTroops, Server.clients[clientId].player.placedTroops);
				_packet.Write(placedTroops.ToArray().Length);
				_packet.Write(placedTroops.ToArray());
				fs.Write(_packet.ToArray(), 0, _packet.Length());
				ServerSend.SendSerializeInGame(clientId, true);
				//formatter.Serialize(fs, troopObject);
			}
			catch(SerializationException e)
			{
				Debug.LogError(e);
				throw;
			}
			finally
			{
				fs.Close();
				placebelTroops.Close();
				placedTroops.Close();
			}
		}

		static void deserializeGame(int clientId)
		{
			try
			{
				BinaryFormatter formatter = new BinaryFormatter();
				byte[] deserializeData;
				if (!Server.clients[clientId].player.isClone)
				{
					using (var stream = new FileStream("TestSerialize.dat", FileMode.Open))
					{
						deserializeData = new byte[(int)stream.Length];
						stream.Read(deserializeData, 0, (int)stream.Length);
					}
				}
				else
				{
					using (var stream = new FileStream("TestSerializeEnemy.dat", FileMode.Open))
					{
						deserializeData = new byte[(int)stream.Length];
						stream.Read(deserializeData, 0, (int)stream.Length);
					}
				}
				Packet _packet = new Packet(deserializeData);
				//int clientId = _packet.ReadInt();
				int placebelTroopsLength = _packet.ReadInt();
				using (var ms = new MemoryStream(_packet.ReadBytes(placebelTroopsLength)))
				{
					Server.clients[clientId].player.placebelTroops = (List<PlaceTroopsStruct>)formatter.Deserialize(ms);
				}
				//TODO set richpath graph property
				int placedTroopsLength = _packet.ReadInt();
				using (var ms = new MemoryStream(_packet.ReadBytes(placedTroopsLength)))
				{
					Server.clients[clientId].player.placedTroops = (List<PlacedTroopStruct>)formatter.Deserialize(ms);
				}
                foreach(var placedTroop in Server.clients[clientId].player.placedTroops)
                {
					placedTroop.gameObject.playerController.myClient = Server.clients[clientId];
					placedTroop.gameObject.newAttackSystem.myClient = Server.clients[clientId];
					placedTroop.gameObject.newAttackSystem.myClient = Server.clients[clientId];
					placedTroop.gameObject.richAI.InitializeAfterDeserialized();
					AddTroop(placedTroop.gameObject.transform);
                }
				Server.clients[clientId].player.InizializeKdTree();
				//TroopComponents vegleich = troopsImages[0].Item1.troopObject;
				ServerSend.SendSerializeInGame(clientId, false);
				Debug.Log("Worked");
			}
			catch (SerializationException ex)
			{
				Debug.Log(ex);
				throw;

			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			foreach(int i in Server.clients.Keys)
			{
				try
				{
					List<PlaceTroopsStruct> placed = Server.clients[i].player.placebelTroops;
				}
				catch
				{
					Debug.Log("Nicht hier");
					continue;
				}
				SerializeGame(i);
				SerializeGame(Server.clients[i].enemyClient.id);
				break;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			foreach (int i in Server.clients.Keys)
			{
				try
				{
					List<PlaceTroopsStruct> placed = Server.clients[i].player.placebelTroops;
				}
				catch
				{
					Debug.Log("Nicht hier");
					continue;
				}
				Debug.Log("Hier");
				deserializeGame(i);
				deserializeGame(Server.clients[i].enemyClient.id);
				break;
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			isPaused = !isPaused;
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
