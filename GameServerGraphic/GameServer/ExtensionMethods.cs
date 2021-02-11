using GameServerGraphic;
using Pathfinding;
using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GameServer
{
	static class ExtensionMethods
	{
		#region CurveSearch
		#region DeltedCurveSearch
		//public static List<Vector3> SearchHermiteCurve(Vector3 firstPos3, Vector3 dirFirstPos3, Vector3 secondPos3, Vector3 dirSecondPos3, out float radiusFirstDir, out float radiusSecondDir, out float distance)
		//{
		//	float radius = (firstPos3 - secondPos3).magnitude * 3;
		//	float limitDistance = 0.09f;
		//	Vector2 firstPos = new Vector2(firstPos3.x, firstPos3.z);
		//	Vector2 firstDir = new Vector2(dirFirstPos3.x, dirFirstPos3.z);
		//	Vector2 secondDir = new Vector2(dirSecondPos3.x, dirSecondPos3.z);
		//	Vector2 secondPos = new Vector2(secondPos3.x, secondPos3.z);
		//	float angleFirstDir = Vector2.Angle(secondPos - firstPos, firstDir);
		//	radiusFirstDir = Map(0.5f, 2, 0, 180, angleFirstDir) * radius;
		//	float angleSecondDir = Vector2.Angle(firstPos - secondPos, secondDir);
		//	radiusSecondDir = Map(0.5f, 2, 0, 180, angleSecondDir) * radius;
		//	firstDir *= radiusFirstDir;
		//	secondDir *= radiusSecondDir;
		//	//float angle = Vector2.Angle(firstDir, secondDir);
		//	//Debug.Log(angle);
		//	//if (angle > 90f)
		//	//{
		//	//	secondDir *= -1f;
		//	//}
		//	List<Vector3> intersectPoints = new List<Vector3>();
		//	bool shouldUSeBezier = true;
		//	if (shouldUSeBezier)
		//	{
		//		dirFirstPos3 *= radiusFirstDir;
		//		dirSecondPos3 *= radiusSecondDir;
		//		intersectPoints = SearchBezierCurve(firstPos3, firstPos3 + (dirFirstPos3 / 3), secondPos3 - (dirSecondPos3 / 3), secondPos3, out distance);
		//		return intersectPoints;
		//	}
		//	float stepSize = 0.01f;
		//	distance = 0f;
		//	Vector2 lastPos = Vector2.zero;
		//	List<Tuple<float, float>> allDistances = new List<Tuple<float, float>>();
		//	List<double> x = new List<double>();
		//	List<double> y = new List<double>();
		//	Matrix4x4 h = new Matrix4x4(2, -2, 1, 1,
		//								-3, 3, -2, -1,
		//								 0, 0, 1, 0,
		//								 1, 0, 0, 0);
		//	Vector3 C = new Vector4(firstPos.x, secondPos.x, firstDir.x, secondDir.x);
		//	Vector2 dirBetweenPoints = secondPos - firstPos;
		//	for (float t = 0; t < 1; t += stepSize)
		//	{
		//		//Vector4 S = new Vector4(t * t * t, t * t, t, 1);
		//		//var zesz =  h.MultiplyPoint(S) * C;
		//		Vector2 pos2 = (2 * t * t * t - 3 * t * t + 1) * firstPos + (t * t * t - 2 * t * t + t) * firstDir + (-2 * t * t * t + 3 * t * t) * secondPos + (t * t * t - t * t) * secondDir;
		//		if (t != 0)
		//		{
		//			//y.Add((pos2 - lastPos).magnitude);
		//			//x.Add(t);
		//			allDistances.Add(new Tuple<float, float>(t, (pos2 - lastPos).magnitude));
		//			distance += (pos2 - lastPos).magnitude;
		//		}
		//		lastPos = pos2;
		//		//long hash = 0;
		//		//unchecked
		//		//{
		//		//	const int b = 31;
		//		//	int a = 23;
		//		//	hash = hash * a + (pos2.x).GetHashCode();
		//		//	a = a * b;
		//		//	hash = hash * a + (pos2.y).GetHashCode();
		//		//}
		//		//allDistances.Add(new Tuple<float, float>(pos2.x, ((6 * t * t - 6 * t) * firstPos + (3 * t * t - 4 * t + 1) * firstDir + (6 * t - 6 * t * t) * secondPos + (3 * t * t - 2 * t) * secondDir).x));
		//		x.Add(pos2.x);
		//		y.Add(t);
		//		////P = (1−t)3P1 + 3(1−t)2tP2 + 3(1−t)t2P3 + t3P4
		//		//Vector2 pos2 = Mathf.Pow(1 - t, 3) * firstPos + 3 * Mathf.Pow(1 - t, 2) * t * secondPos + 3 * (1 - t) * t * t * thirdPos + Mathf.Pow(t, 3) * fourthPos;
		//		Vector3 pos = new Vector3(pos2.x, firstPos3.y, pos2.y);
		//		float tempDistance = (pos - AstarPath.active.GetNearest(pos, NNConstraint.Default).position).sqrMagnitude;
		//		if (tempDistance > limitDistance)
		//		{
		//			Form1.SpawnPointAt(pos, System.Drawing.Color.Red, 10);
		//			intersectPoints.Add(pos);
		//		}
		//		Form1.SpawnPointAt(pos, System.Drawing.Color.Red, 2);
		//		//Form1.SpawnPointAt(new Vector3(porjection.x, firstPos3.y, porjection.y), System.Drawing.Color.Red, 2);
		//	}
		//	WriteToExcel(allDistances);
		//	//double[] test = Polyfit(x.ToArray(), y.ToArray(), 5);
		//	//for (int i = 0; i < test.Length; i++)
		//	//{
		//	//	Debug.Log(test[i]);
		//	//}
		//	//Debug.Log(distance);
		//	return intersectPoints;
		//}

		//public static Vector2 GetDirHermitSpine(Vector2 firstPos, Vector2 firstDir, Vector2 secondPos, Vector2 secondDir, float t)
		//{
		//	return (6 * t * t - 6 * t) * firstPos + (3 * t * t - 4 * t + 1) * firstDir + (6 * t - 6 * t * t) * secondPos + (3 * t * t - 2 * t) * secondDir;
		//}
		//public static void DrawNewBezierCruve(Vector2 firstPos, Vector2 firstDir, Vector2 secondPos, Vector2 secondDir)
		//{
		//	firstDir = firstDir.normalized;
		//	secondDir = secondDir.normalized;
		//	Vector3 intersectionPoint3;
		//	LineLineIntersection(out intersectionPoint3, firstPos, firstDir, secondPos, secondDir);
		//	Vector2 intersectionPoint = new Vector2(intersectionPoint3.x, intersectionPoint3.z);
		//	//float w = Mathf.Sqrt(0.5f * (1 + Vector2.Dot(firstDir, secondDir)));
		//	float w = 1f;
		//	//w = Mathf.Clamp(w, 0f, 0.7f);
		//	Debug.Log(w);
		//	float stepSize = 0.01f;
		//	//List<Vector3> intersectPoints = new List<Vector3>();
		//	for (float t = 0; t < 1; t += stepSize)
		//	{
		//		Vector2 pos2 = ((1 - t) * (1 - t) * firstPos + 2 * w * t * (1 - t) * intersectionPoint + t * t * secondPos) / ((1 - t) * (1 - t) + 2 * w * t * (1 - t) + t * t);
		//		////P = (1−t)3P1 + 3(1−t)2tP2 + 3(1−t)t2P3 + t3P4
		//		//Vector2 pos2 = Mathf.Pow(1 - t, 3) * firstPos + 3 * Mathf.Pow(1 - t, 2) * t * secondPos + 3 * (1 - t) * t * t * thirdPos + Mathf.Pow(t, 3) * fourthPos;
		//		Vector3 pos = new Vector3(pos2.x, firstPos.y, pos2.y);
		//		Form1.SpawnPointAt(pos, System.Drawing.Color.Pink, 2);
		//	}

		//}

		//public static List<Vector3> SearchCircle(Vector3 firstPos, Vector3 secondPos, Vector3 dir, out Vector3 middlePoint, out float radius, out float factorCircleSide, out Vector3 pointBeforeFormation)
		//{
		//	//berechnen von einem Punkt vor der Formation und einem Kreismittelpunkt, welcher durch beide Punkte geht und die Richtung zeigt. Berechnen von einem angle(Kreis geht von wo bis wo) und einem Offset(wo fängt er an) und einem dot(welche Richtung)
		//	float stepDistance = 1f;
		//	float limitDistance = 0.09f;

		//	pointBeforeFormation = secondPos - dir * 6;
		//	Vector2 dirFromEnemy = new Vector2(-dir.x, -dir.z);
		//	if (Vector3.Angle(Quaternion.CreateFromAxisAngle(Vector3.up, Mathf.PI) * dir, firstPos - secondPos) > Vector3.Angle(dir, firstPos - secondPos))
		//	{
		//		pointBeforeFormation = secondPos + dir * 6;
		//		dirFromEnemy = new Vector2(dir.x, dir.z);
		//	}
		//	//SpawnPointAt(pointBeforeFormation, Color.Green, 10);

		//	dirFromEnemy = Vector2.Perpendicular(dirFromEnemy).normalized;
		//	Vector2 dirFromMy = new Vector2(pointBeforeFormation.x - firstPos.x, pointBeforeFormation.z - firstPos.z);
		//	dirFromMy = Vector2.Perpendicular(dirFromMy).normalized;
		//	middlePoint = Vector3.zero;
		//	Vector2 middlePointFromMy = new Vector2((firstPos.x + pointBeforeFormation.x) / 2, (firstPos.z + pointBeforeFormation.z) / 2);
		//	bool isIntersect = Mathf.LineLineIntersection(out middlePoint, new Vector3(pointBeforeFormation.x, 0f, pointBeforeFormation.z), new Vector3(dirFromEnemy.x, 0f, dirFromEnemy.y), new Vector3(middlePointFromMy.x, 0f, middlePointFromMy.y), new Vector3(dirFromMy.x, 0f, dirFromMy.y));
		//	middlePoint = new Vector3(middlePoint.x, firstPos.y, middlePoint.z);

		//	Vector3 movement = middlePoint - pointBeforeFormation;
		//	movement = new Vector3(movement.z, 0, -movement.x);
		//	//wenn ähnliche Richtung dann positiv, sonst negativ
		//	float dot = Vector3.Dot(movement, pointBeforeFormation - secondPos);
		//	radius = (middlePoint - firstPos).magnitude;
		//	//gut wir wollen Radians NICHT ANGLE!!
		//	float angle = Vector3.AngleBetween(middlePoint - firstPos, middlePoint - pointBeforeFormation);
		//	float offset = Vector3.AngleBetween(middlePoint - firstPos, middlePoint - new Vector3(radius + middlePoint.x, middlePoint.y, middlePoint.z));
		//	float stepSize = angle / (radius * angle / stepDistance);
		//	List<Vector3> intersectPoints = new List<Vector3>();
		//	if (dot < 0f)
		//		factorCircleSide = 1f;
		//	else
		//		factorCircleSide = -1f;
		//	for (float t = 0; t < angle; t += stepSize)
		//	{
		//		Vector3 pos = Vector3.zero;
		//		if (dot > 0f)
		//			pos = new Vector3(radius * Mathf.Cos(-t - offset) + middlePoint.x, middlePoint.y, radius * Mathf.Sin(-t - offset) + middlePoint.z);
		//		else
		//			pos = new Vector3(radius * Mathf.Cos(t - offset) + middlePoint.x, middlePoint.y, radius * Mathf.Sin(t - offset) + middlePoint.z);

		//		Form1.SpawnPointAt(pos, System.Drawing.Color.RoyalBlue, 2);
		//		float tempDistance = (pos - AstarPath.active.GetNearest(pos, NNConstraint.Default).position).sqrMagnitude;
		//		if (tempDistance > limitDistance)
		//		{
		//			intersectPoints.Add(pos);
		//			Form1.SpawnPointAt(pos, System.Drawing.Color.Red, 10);
		//		}
		//	}
		//	return intersectPoints;
		//}

		//public static List<Vector3> SearchBezierCurve(Vector3 firstPos3, Vector3 secondPos3, Vector3 thirdPos3, Vector3 fourthPos3, out float distance)
		//{
		//	//float radius = 100f;
		//	float limitDistance = 0.09f;

		//	//float dot = Vector3.Dot(dirFirstPos3, dirSecondPos3);
		//	//if (dot > 0)
		//	//{
		//	//	dirSecondPos3 *= -1f;
		//	//}
		//	Vector2 firstPos = new Vector2(firstPos3.x, firstPos3.z);
		//	Vector2 secondPos = new Vector2(secondPos3.x, secondPos3.z);
		//	Vector2 thirdPos = new Vector2(thirdPos3.x, thirdPos3.z);
		//	//Vector2 secondPos = new Vector2(firstPos.x * dirFirstPos3.x, firstPos3.z  * dirFirstPos3.z);
		//	//Vector2 firstDir = new Vector2(dirFirstPos3.x, dirFirstPos3.z) * radius;
		//	//Vector2 thirdPos = new Vector2(secondPos3.x + radius * dirSecondPos3.x, secondPos3.z + radius * dirSecondPos3.z);
		//	//Vector2 secondDir = new Vector2(dirSecondPos3.x, dirSecondPos3.z) * radius;
		//	Vector2 fourthPos = new Vector2(fourthPos3.x, fourthPos3.z);
		//	float stepSize = 0.01f;
		//	List<Vector3> intersectPoints = new List<Vector3>();
		//	distance = 0f;
		//	Vector2 lastPos = Vector2.zero;
		//	List<Tuple<float, float>> allDistances = new List<Tuple<float, float>>();
		//	for (float t = 0; t < 1; t += stepSize)
		//	{
		//		//Vector2 pos2 = (2 * t * t * t - 3 * t * t + 1) * firstPos + (t * t * t - 2 * t * t + t) * firstDir + (-2 * t * t * t + 3 * t * t) * fourthPos + (t * t * t - t * t) * secondDir;
		//		////P = (1−t)3P1 + 3(1−t)2tP2 + 3(1−t)t2P3 + t3P4
		//		//Vector2 pos2 = Mathf.Pow(1 - t, 3) * firstPos + 3 * Mathf.Pow(1 - t, 2) * t * secondPos + 3 * (1 - t) * t * t * thirdPos + Mathf.Pow(t, 3) * fourthPos;
		//		Vector2 pos2 = (1 - t) * (1 - t) * (1 - t) * firstPos + 3 * (1 - t) * (1 - t) * t * secondPos + 3 * (1 - t) * t * t * thirdPos + t * t * t * fourthPos;
		//		Vector3 pos = new Vector3(pos2.x, firstPos3.y, pos2.y);
		//		if (t != 0)
		//		{
		//			//y.Add((pos2 - lastPos).magnitude);
		//			//x.Add(t);
		//			allDistances.Add(new Tuple<float, float>(t, (pos2 - lastPos).magnitude));
		//			distance += (pos2 - lastPos).magnitude;
		//		}
		//		lastPos = pos2;
		//		float tempDistance = (pos - AstarPath.active.GetNearest(pos, NNConstraint.Default).position).sqrMagnitude;
		//		if (tempDistance > limitDistance)
		//		{
		//			Form1.SpawnPointAt(pos, System.Drawing.Color.Red, 10);
		//			intersectPoints.Add(pos);
		//		}
		//		Form1.SpawnPointAt(pos, System.Drawing.Color.Pink, 2);
		//	}
		//	WriteToExcel(allDistances);
		//	return intersectPoints;
		//}
		#endregion
		#endregion

		public static float Map(float from, float to, float from2, float to2, float value)
		{
			if (value <= from2)
			{
				return from;
			}
			else if (value >= to2)
			{
				return to;
			}
			else
			{
				return (to - from) * ((value - from2) / (to2 - from2)) + from;
			}
		}

		public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
		{
			Vector3 lineVec3 = linePoint2 - linePoint1;
			Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
			Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

			float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

			//is coplanar, and not parrallel
			if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
			{
				float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
				intersection = linePoint1 + (lineVec1 * s);
				return true;
			}
			else
			{
				intersection = Vector3.zero;
				return false;
			}
		}

		public static double[] Polyfit(double[] x, double[] y, int degree)
		{
			// Vandermonde matrix
			var v = new DenseMatrix(x.Length, degree + 1);
			for (int i = 0; i < v.RowCount; i++)
				for (int j = 0; j <= degree; j++) v[i, j] = Math.Pow(x[i], j);
			var yv = new DenseVector(y).ToColumnMatrix();
			MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = v.QR();
			// Math.Net doesn't have an "economy" QR, so:
			// cut R short to square upper triangle, then recompute Q
			var r = qr.R.SubMatrix(0, degree + 1, 0, degree + 1);
			var q = v.Multiply(r.Inverse());
			var p = r.Inverse().Multiply(q.TransposeThisAndMultiply(yv));
			return p.Column(0).ToArray();
		}

		public static void WriteToExcel(List<Tuple<float, float>> values1, List<Tuple<float, float>> values2 = null)
		{
			Excel.Application excelApp = new Excel.Application();
			if (excelApp != null)
			{
				Excel.Workbook excelWorkbook = excelApp.Workbooks.Add();
				Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelWorkbook.Sheets.Add();
				//excelWorksheet.Cells[1, 1] = "1";
				for (int i = 0; i < values1.Count; i++)
				{
					excelWorksheet.Cells[i+1, 1] = values1[i].Item1;
					excelWorksheet.Cells[i+1, 2] = values1[i].Item2;
				}
				if(values2 != null)
				{
					for (int i = 0; i < values2.Count; i++)
					{
						excelWorksheet.Cells[i + 1, 3] = values2[i].Item1;
						excelWorksheet.Cells[i + 1, 4] = values2[i].Item2;
					}
				}

				excelApp.ActiveWorkbook.SaveAs(@"abc.xlsx");

				excelWorkbook.Close();
				excelApp.Quit();

				System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelWorksheet);
				System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelWorkbook);
				System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelApp);
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public static int CombineHashCodes(int h1, int h2)
		{
			return (((h1 << 5) + h1) ^ h2);
		}

		public static System.Numerics.Vector3 MutliplyPoint(this System.Numerics.Matrix4x4 thisMatrix, System.Numerics.Vector3 v)
		{
			System.Numerics.Vector3 vector3;
			vector3.X = (float)((double)thisMatrix.M11 * (double)v.X + (double)thisMatrix.M21 * (double)v.Y + (double)thisMatrix.M31 * (double)v.Z) + thisMatrix.M41;
			vector3.Y = (float)((double)thisMatrix.M12 * (double)v.X + (double)thisMatrix.M22 * (double)v.Y + (double)thisMatrix.M32 * (double)v.Z) + thisMatrix.M42;
			vector3.Z = (float)((double)thisMatrix.M13 * (double)v.X + (double)thisMatrix.M23 * (double)v.Y + (double)thisMatrix.M33 * (double)v.Z) + thisMatrix.M43;
			float num = 1f / ((float)((double)thisMatrix.M14 * (double)v.X + (double)thisMatrix.M24 * (double)v.Y + (double)thisMatrix.M34 * (double)v.Z) + thisMatrix.M44);
			vector3.X *= num;
			vector3.Y *= num;
			vector3.Z *= num;
			return vector3;
		}
	}
}
