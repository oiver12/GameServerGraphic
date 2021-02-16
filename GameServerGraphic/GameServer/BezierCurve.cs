using GameServerGraphic;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
	public class BezierCurveXZPlane
	{
		public float lengthFirstDir;
		public float lengthSecondDir;
		public float limitDistance = 0.09f;
		public float stepSizeCalculate = 0.01f;
		public float distanceBezierCurve = float.NaN;
		public float tMove = 0f;
		public List<Vector3> intersectPoints;

		const float distanceMultiplicator = 2;
		const float angleMultiplicatorMin = 0.2f;
		const float angleMultiplicatorMax = 2.5f;
		const float angleSwitchDir = 140f;

		private float maxDistanceBetweenTwoPoints = float.NaN;
		//double[] polynomFunction;
		private Vector3 _firstPosition;
		private Vector3 _secondPosition;
		private Vector3 _thirdPosition;
		private Vector3 _fourthPosition;
		private Vector2 firstPos2;
		private Vector2 secondPos2;
		private Vector2 thirdPos2;
		private Vector2 fourthPos2;

		public Vector3 firstPosition
		{
			get => _firstPosition;
			set
			{
				_firstPosition = value;
				firstPos2 = new Vector2(_firstPosition.x, _firstPosition.z);
			}
		}
		public Vector3 secondPosition
		{
			get => _secondPosition;
			set
			{
				_secondPosition = value;
				secondPos2 = new Vector2(_secondPosition.x, _secondPosition.z);
			}
		}
		public Vector3 thirdPosition
		{
			get => _thirdPosition;
			set
			{
				_thirdPosition = value;
				thirdPos2 = new Vector2(_thirdPosition.x, _thirdPosition.z);
			}
		}
		public Vector3 fourthPosition
		{
			get => _fourthPosition;
			set
			{
				_fourthPosition = value;
				fourthPos2 = new Vector2(_fourthPosition.x, _fourthPosition.z);
			}
		}

		public BezierCurveXZPlane(Vector3 firstPos, Vector3 secondPos, Vector3 thirdPos, Vector3 fourthPos)
		{
			firstPosition = firstPos;
			secondPosition = secondPos;
			thirdPosition = thirdPos;
			fourthPosition = fourthPos;
			//TODO calculate radius etc
		}

		public BezierCurveXZPlane(){ }

		public void BezierCurveXZPlaneFromHermitCurve(Vector3 firstPos, Vector3 firstDir, Vector3 secondPos, Vector3 secondDir, bool canSwitchDir)
		{
			firstPos2 = new Vector2(firstPos.x, firstPos.z);
			secondPos2 = new Vector2(secondPos.x, secondPos.z);
			Vector2 firstDir2 = new Vector2(firstDir.x, firstDir.z);
			Vector2 secondDir2 = new Vector2(secondDir.x, secondDir.z);

			firstDir2 = firstDir2.normalized;
			secondDir2 = secondDir2.normalized;
			float radius = ((firstPos2+firstDir2) - (secondPos2-secondDir2)).magnitude * distanceMultiplicator;
			float angleFirstDir = Vector2.Angle(secondPos2 - firstPos2, firstDir2);
			float angleSecondDir = Vector2.Angle(secondPos2 - firstPos2, secondDir2);
			//Debug.Log(angleFirstDir);
			if (angleFirstDir >= angleSwitchDir)
			{
				firstDir2 *= -1f;
				if (angleSecondDir >= angleSwitchDir)
					secondDir2 *= -1f;
				radius = ((firstPos2 + firstDir2) - (secondPos2 - secondDir2)).magnitude * distanceMultiplicator;
				angleFirstDir = Vector2.Angle(secondPos2 - firstPos2, firstDir2);
				angleSecondDir = Vector2.Angle(secondPos2 - firstPos2, secondDir2);
			}
			lengthFirstDir = ExtensionMethods.Map(angleMultiplicatorMin, angleMultiplicatorMax, 0, 180, angleFirstDir) * radius;
			//Debug.Log(angleSecondDir);
			lengthSecondDir = ExtensionMethods.Map(angleMultiplicatorMin, angleMultiplicatorMax, 0, 180, angleSecondDir) * radius;
			firstDir2 *= lengthFirstDir;
			secondDir2 *= lengthSecondDir;

			firstDir = new Vector3(firstDir2.x, 0f, firstDir2.y);
			secondDir = new Vector3(secondDir2.x, 0f, secondDir2.y);

			firstPosition = firstPos;
			secondPosition = firstPos + (firstDir / 3);
			thirdPosition = secondPos - (secondDir / 3);
			fourthPosition = secondPos;
		}

		public void CalculateBezierCurve(bool shouldDraw)
		{
			//List<Tuple<float, float>> allDistances1 = new List<Tuple<float, float>>();
			//List<Tuple<float, float>> allDistances2 = new List<Tuple<float, float>>();
			//List<double> x = new List<double>();
			//List<double> y = new List<double>();
			intersectPoints = new List<Vector3>();
			distanceBezierCurve = 0f;
			Vector2 lastPos = Vector2.zero;
			maxDistanceBetweenTwoPoints = 0f;
			for (float t = 0; t < 1; t += stepSizeCalculate)
			{
				////P = (1−t)3P1 + 3(1−t)2tP2 + 3(1−t)t2P3 + t3P4
				Vector2 pos2 = (1 - t) * (1 - t) * (1 - t) * firstPos2 + 3 * (1 - t) * (1 - t) * t * secondPos2 + 3 * (1 - t) * t * t * thirdPos2 + t * t * t * fourthPos2;
				Vector3 pos = new Vector3(pos2.x, firstPosition.y, pos2.y);
				if (t != 0)
				{
					//y.Add((pos2 - lastPos).magnitude);
					//x.Add(t);
					//allDistances1.Add(new Tuple<float, float>(t, dB(t).magnitude/*(pos2 - lastPos).magnitude*/));
					//allDistances2.Add(new Tuple<float, float>(t, (pos2 - lastPos).magnitude));

					float tempDistanceBetweenTwoPoints = (pos2 - lastPos).magnitude;
					if (tempDistanceBetweenTwoPoints > maxDistanceBetweenTwoPoints)
						maxDistanceBetweenTwoPoints = tempDistanceBetweenTwoPoints;

					distanceBezierCurve += tempDistanceBetweenTwoPoints;
				}
				lastPos = pos2;
				float tempDistance = (pos - AstarPath.active.GetNearest(pos, NNConstraint.Default).position).sqrMagnitude;
				if (tempDistance > limitDistance)
				{
					if(shouldDraw)
						Form1.SpawnPointAt(pos, System.Drawing.Color.Red, 10);

					intersectPoints.Add(pos);
				}
				if(shouldDraw)
					Form1.SpawnPointAt(pos, System.Drawing.Color.Pink, 2);
			}
			//Form1.SpawnPointAt(new Vector3(spawn.x, firstPosition.y, spawn.y), System.Drawing.Color.Red, 20);
			//polynomFunction = ExtensionMethods.Polyfit(x.ToArray(), y.ToArray(), 5);
			//ExtensionMethods.WriteToExcel(allDistances1, allDistances2);
		}

		public Vector2 B(float t)
		{
			return (1 - t) * (1 - t) * (1 - t) * firstPos2 + 3 * (1 - t) * (1 - t) * t * secondPos2 + 3 * (1 - t) * t * t * thirdPos2 + t * t * t * fourthPos2;
		}

		public float BOnlyX(float t)
		{
			return (1 - t) * (1 - t) * (1 - t) * firstPos2.x + 3 * (1 - t) * (1 - t) * t * secondPos2.x + 3 * (1 - t) * t * t * thirdPos2.x + t * t * t * fourthPos2.x;
		}

		public Vector2 dB(float t)
		{
			return 3 * (1 - t) * (1 - t) * (secondPos2 - firstPos2) + 6 * (1 - t) * t * (thirdPos2 - secondPos2) + 3 * t * t * (fourthPos2 - thirdPos2);
		}

		public float dBOnlyX(float t)
		{
			return 3 * (1 - t) * (1 - t) * (secondPos2.x - firstPos2.x) + 6 * (1 - t) * t * (thirdPos2.x - secondPos2.x) + 3 * t * t * (fourthPos2.x - thirdPos2.x);
		}

		public Vector2 GetSecondDerivative(float t)
		{
			return 6 * (1 - t) * (thirdPos2 - 2 * secondPos2 + firstPos2) + 6 * t * (fourthPos2 - 2 * thirdPos2 + secondPos2);
		}


		public float NewtonApproximation(float tGuess, Vector2 positionNow, int itterations)
		{
			//https://math.stackexchange.com/questions/140253/newtons-method-and-approximating-parameters-for-b%C3%A9zier-curves
			//for (int i = 0; i < itterations; i++)
			//{
			//	Vector2 normalPos = B(tGuess);
			//	Vector2 direction = dB(tGuess);
			//	Vector2 secondDerivative = GetSecondDerivative(tGuess);
			//	tGuess = tGuess - ((normalPos.x - positionNow.x) * direction.x) / ((normalPos.x - positionNow.x) * secondDerivative.x + direction.x * direction.x);
			//}
			//return tGuess;
			for (int n = 0; n < itterations; n++)
			{
				Vector2 d = dB(tGuess);
				float x = (B(tGuess).x - positionNow.x) / d.x;
				float z = (B(tGuess).y - positionNow.y) / d.y;
				//if (x > 0 && z < 0 || x < 0 && z > 0)
				//	Debug.Log("NOT good");
				if (Math.Abs(x) > Math.Abs(z) || tGuess - x > 1f || tGuess - x < 0f)
					tGuess -= x;
				else if (tGuess - z > 1f || tGuess - z < 0f)
					tGuess -= z;
				else
					return tGuess;
			}

			return tGuess;
		}

		public void StartMove()
		{
			tMove = 0f;
		}

		public Vector3 Move(float moveSpeed)
		{
			Vector2 moveDir = dB(tMove);
			float moveDirMagnitude = moveDir.magnitude;
			//float distance = (float)polynomFunction[5] * tMove * tMove * tMove * tMove * tMove+ (float)polynomFunction[4] * tMove * tMove * tMove * tMove + (float)polynomFunction[3] * tMove * tMove * tMove + (float)polynomFunction[2] * tMove * tMove + (float)polynomFunction[1] * tMove + (float)polynomFunction[0];
			//laufe weniger schnell in den Kurven --> es hat Kurven wenn die Distance für eine Δt von stepSiteCalculate klein ist im Vergleich zum grössten Abstand
			//evt. Abhängigkeit von der Breite der Armee(äusserste Truppe nicht so schnell laufen)?
			moveSpeed *= (moveDirMagnitude * stepSizeCalculate) / maxDistanceBetweenTwoPoints;
			//die Distance welchen diesen Frame gelaufen wird: Δs = Time.deltaTime(=Δt) * moveSpeed(=v). Man weiss das eine Distance von distance eine Vergrösserung in t von stepSizeCalculate zur Folge hat also: (Δs/distance) * stepSizeCalculate = Vergrösserung in t in diesem Frame
			//tMove += ((Time.deltaTime * moveSpeed) / distance) * stepSizeCalculate;
			tMove += (Time.deltaTime * moveSpeed) / moveDirMagnitude;
			if(tMove >= 1f)
			{
				return Vector3.zero;
			}
			return new Vector3(moveDir.x, 0f, moveDir.y).normalized * Time.deltaTime * moveSpeed;
		}
	}
}
