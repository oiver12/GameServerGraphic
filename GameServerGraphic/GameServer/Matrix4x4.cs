﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Globalization;
using System;

namespace GameServer
{
	/// <summary>
	/// A structure encapsulating a 4x4 matrix.
	/// </summary>
	public struct Matrix4x4
	{
		#region Public Fields
		/// <summary>
		/// Value at row 1, column 1 of the matrix.
		/// </summary>
		public float M11;
		/// <summary>
		/// Value at row 1, column 2 of the matrix.
		/// </summary>
		public float M12;
		/// <summary>
		/// Value at row 1, column 3 of the matrix.
		/// </summary>
		public float M13;
		/// <summary>
		/// Value at row 1, column 4 of the matrix.
		/// </summary>
		public float M14;

		/// <summary>
		/// Value at row 2, column 1 of the matrix.
		/// </summary>
		public float M21;
		/// <summary>
		/// Value at row 2, column 2 of the matrix.
		/// </summary>
		public float M22;
		/// <summary>
		/// Value at row 2, column 3 of the matrix.
		/// </summary>
		public float M23;
		/// <summary>
		/// Value at row 2, column 4 of the matrix.
		/// </summary>
		public float M24;

		/// <summary>
		/// Value at row 3, column 1 of the matrix.
		/// </summary>
		public float M31;
		/// <summary>
		/// Value at row 3, column 2 of the matrix.
		/// </summary>
		public float M32;
		/// <summary>
		/// Value at row 3, column 3 of the matrix.
		/// </summary>
		public float M33;
		/// <summary>
		/// Value at row 3, column 4 of the matrix.
		/// </summary>
		public float M34;

		/// <summary>
		/// Value at row 4, column 1 of the matrix.
		/// </summary>
		public float M41;
		/// <summary>
		/// Value at row 4, column 2 of the matrix.
		/// </summary>
		public float M42;
		/// <summary>
		/// Value at row 4, column 3 of the matrix.
		/// </summary>
		public float M43;
		/// <summary>
		/// Value at row 4, column 4 of the matrix.
		/// </summary>
		public float M44;
		#endregion Public Fields

		private static readonly Matrix4x4 _identity = new Matrix4x4
		(
			1f, 0f, 0f, 0f,
			0f, 1f, 0f, 0f,
			0f, 0f, 1f, 0f,
			0f, 0f, 0f, 1f
		);

		/// <summary>
		/// Returns the multiplicative identity matrix.
		/// </summary>
		public static Matrix4x4 Identity
		{
			get { return _identity; }
		}

		/// <summary>
		/// Returns whether the matrix is the identity matrix.
		/// </summary>
		public bool IsIdentity
		{
			get
			{
				return M11 == 1f && M22 == 1f && M33 == 1f && M44 == 1f && // Check diagonal element first for early out.
									M12 == 0f && M13 == 0f && M14 == 0f &&
					   M21 == 0f && M23 == 0f && M24 == 0f &&
					   M31 == 0f && M32 == 0f && M34 == 0f &&
					   M41 == 0f && M42 == 0f && M43 == 0f;
			}
		}

		/// <summary>
		/// Gets or sets the translation component of this matrix.
		/// </summary>
		public Vector3 Translation
		{
			get
			{
				return new Vector3(M41, M42, M43);
			}
			set
			{
				M41 = value.x;
				M42 = value.y;
				M43 = value.z;
			}
		}

		/// <summary>
		/// Constructs a Matrix4x4 from the given components.
		/// </summary>
		public Matrix4x4(float m11, float m12, float m13, float m14,
						 float m21, float m22, float m23, float m24,
						 float m31, float m32, float m33, float m34,
						 float m41, float m42, float m43, float m44)
		{
			this.M11 = m11;
			this.M12 = m12;
			this.M13 = m13;
			this.M14 = m14;

			this.M21 = m21;
			this.M22 = m22;
			this.M23 = m23;
			this.M24 = m24;

			this.M31 = m31;
			this.M32 = m32;
			this.M33 = m33;
			this.M34 = m34;

			this.M41 = m41;
			this.M42 = m42;
			this.M43 = m43;
			this.M44 = m44;
		}

		public Matrix4x4(Vector4 column1, Vector4 column2, Vector4 column3, Vector4 column4)
		{
			M11 = column1.x;
			M21 = column1.y;
			M31 = column1.z;
			M41 = column1.w;

			M12 = column2.x;
			M22 = column2.y;
			M32 = column2.z;
			M42 = column2.w;

			M13 = column3.x;
			M23 = column3.y;
			M33 = column3.z;
			M43 = column3.w;

			M14 = column4.x;
			M24 = column4.y;
			M34 = column4.z;
			M44 = column4.w;
		}

		/// <summary>
		/// Constructs a Matrix4x4 from the given Matrix3x2.
		/// </summary>
		/// <param name="value">The source Matrix3x2.</param>
		public Matrix4x4(Matrix3x2 value)
		{
			M11 = value.M11;
			M12 = value.M12;
			M13 = 0f;
			M14 = 0f;
			M21 = value.M21;
			M22 = value.M22;
			M23 = 0f;
			M24 = 0f;
			M31 = 0f;
			M32 = 0f;
			M33 = 1f;
			M34 = 0f;
			M41 = value.M31;
			M42 = value.M32;
			M43 = 0f;
			M44 = 1f;
		}

		/// <summary>
		/// Creates a spherical billboard that rotates around a specified object position.
		/// </summary>
		/// <param name="objectPosition">Position of the object the billboard will rotate around.</param>
		/// <param name="cameraPosition">Position of the camera.</param>
		/// <param name="cameraUpVector">The up vector of the camera.</param>
		/// <param name="cameraForwardVector">The forward vector of the camera.</param>
		/// <returns>The created billboard matrix</returns>
		public static Matrix4x4 CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
		{
			const float epsilon = 1e-4f;

			Vector3 zaxis = new Vector3(
				objectPosition.x - cameraPosition.x,
				objectPosition.y - cameraPosition.y,
				objectPosition.z - cameraPosition.z);

			float norm = zaxis.magnitude;

			if (norm < epsilon)
			{
				zaxis = -cameraForwardVector;
			}
			else
			{
				zaxis = Vector3.Multiply(zaxis, 1.0f / (float)Math.Sqrt(norm));
			}

			Vector3 xaxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zaxis));

			Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

			Matrix4x4 result;

			result.M11 = xaxis.x;
			result.M12 = xaxis.y;
			result.M13 = xaxis.z;
			result.M14 = 0.0f;
			result.M21 = yaxis.x;
			result.M22 = yaxis.y;
			result.M23 = yaxis.z;
			result.M24 = 0.0f;
			result.M31 = zaxis.x;
			result.M32 = zaxis.y;
			result.M33 = zaxis.z;
			result.M34 = 0.0f;

			result.M41 = objectPosition.x;
			result.M42 = objectPosition.y;
			result.M43 = objectPosition.z;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a cylindrical billboard that rotates around a specified axis.
		/// </summary>
		/// <param name="objectPosition">Position of the object the billboard will rotate around.</param>
		/// <param name="cameraPosition">Position of the camera.</param>
		/// <param name="rotateAxis">Axis to rotate the billboard around.</param>
		/// <param name="cameraForwardVector">Forward vector of the camera.</param>
		/// <param name="objectForwardVector">Forward vector of the object.</param>
		/// <returns>The created billboard matrix.</returns>
		public static Matrix4x4 CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 rotateAxis, Vector3 cameraForwardVector, Vector3 objectForwardVector)
		{
			const float epsilon = 1e-4f;
			const float minAngle = 1.0f - (0.1f * ((float)Math.PI / 180.0f)); // 0.1 degrees

			// Treat the case when object and camera positions are too close.
			Vector3 faceDir = new Vector3(
				objectPosition.x - cameraPosition.x,
				objectPosition.y - cameraPosition.y,
				objectPosition.z - cameraPosition.z);

			float norm = faceDir.sqrMagnitude;

			if (norm < epsilon)
			{
				faceDir = -cameraForwardVector;
			}
			else
			{
				faceDir = Vector3.Multiply(faceDir, (1.0f / (float)Math.Sqrt(norm)));
			}

			Vector3 yaxis = rotateAxis;
			Vector3 xaxis;
			Vector3 zaxis;

			// Treat the case when angle between faceDir and rotateAxis is too close to 0.
			float dot = Vector3.Dot(rotateAxis, faceDir);

			if (Math.Abs(dot) > minAngle)
			{
				zaxis = objectForwardVector;

				// Make sure passed values are useful for compute.
				dot = Vector3.Dot(rotateAxis, zaxis);

				if (Math.Abs(dot) > minAngle)
				{
					zaxis = (Math.Abs(rotateAxis.z) > minAngle) ? new Vector3(1, 0, 0) : new Vector3(0, 0, -1);
				}

				xaxis = Vector3.Normalize(Vector3.Cross(rotateAxis, zaxis));
				zaxis = Vector3.Normalize(Vector3.Cross(xaxis, rotateAxis));
			}
			else
			{
				xaxis = Vector3.Normalize(Vector3.Cross(rotateAxis, faceDir));
				zaxis = Vector3.Normalize(Vector3.Cross(xaxis, yaxis));
			}

			Matrix4x4 result;

			result.M11 = xaxis.x;
			result.M12 = xaxis.y;
			result.M13 = xaxis.z;
			result.M14 = 0.0f;
			result.M21 = yaxis.x;
			result.M22 = yaxis.y;
			result.M23 = yaxis.z;
			result.M24 = 0.0f;
			result.M31 = zaxis.x;
			result.M32 = zaxis.y;
			result.M33 = zaxis.z;
			result.M34 = 0.0f;

			result.M41 = objectPosition.x;
			result.M42 = objectPosition.y;
			result.M43 = objectPosition.z;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a translation matrix.
		/// </summary>
		/// <param name="position">The amount to translate in each axis.</param>
		/// <returns>The translation matrix.</returns>
		public static Matrix4x4 CreateTranslation(Vector3 position)
		{
			Matrix4x4 result;

			result.M11 = 1.0f;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = 1.0f;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = 1.0f;
			result.M34 = 0.0f;

			result.M41 = position.x;
			result.M42 = position.y;
			result.M43 = position.z;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a translation matrix.
		/// </summary>
		/// <param name="xPosition">The amount to translate on the X-axis.</param>
		/// <param name="yPosition">The amount to translate on the Y-axis.</param>
		/// <param name="zPosition">The amount to translate on the Z-axis.</param>
		/// <returns>The translation matrix.</returns>
		public static Matrix4x4 CreateTranslation(float xPosition, float yPosition, float zPosition)
		{
			Matrix4x4 result;

			result.M11 = 1.0f;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = 1.0f;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = 1.0f;
			result.M34 = 0.0f;

			result.M41 = xPosition;
			result.M42 = yPosition;
			result.M43 = zPosition;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a scaling matrix.
		/// </summary>
		/// <param name="xScale">Value to scale by on the X-axis.</param>
		/// <param name="yScale">Value to scale by on the Y-axis.</param>
		/// <param name="zScale">Value to scale by on the Z-axis.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4x4 CreateScale(float xScale, float yScale, float zScale)
		{
			Matrix4x4 result;

			result.M11 = xScale;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = yScale;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = zScale;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a scaling matrix with a center point.
		/// </summary>
		/// <param name="xScale">Value to scale by on the X-axis.</param>
		/// <param name="yScale">Value to scale by on the Y-axis.</param>
		/// <param name="zScale">Value to scale by on the Z-axis.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4x4 CreateScale(float xScale, float yScale, float zScale, Vector3 centerPoint)
		{
			Matrix4x4 result;

			float tx = centerPoint.x * (1 - xScale);
			float ty = centerPoint.y * (1 - yScale);
			float tz = centerPoint.z * (1 - zScale);

			result.M11 = xScale;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = yScale;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = zScale;
			result.M34 = 0.0f;
			result.M41 = tx;
			result.M42 = ty;
			result.M43 = tz;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a scaling matrix.
		/// </summary>
		/// <param name="scales">The vector containing the amount to scale by on each axis.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4x4 CreateScale(Vector3 scales)
		{
			Matrix4x4 result;

			result.M11 = scales.x;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = scales.y;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = scales.z;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a scaling matrix with a center point.
		/// </summary>
		/// <param name="scales">The vector containing the amount to scale by on each axis.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4x4 CreateScale(Vector3 scales, Vector3 centerPoint)
		{
			Matrix4x4 result;

			float tx = centerPoint.x * (1 - scales.x);
			float ty = centerPoint.y * (1 - scales.y);
			float tz = centerPoint.z * (1 - scales.z);

			result.M11 = scales.x;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = scales.y;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = scales.z;
			result.M34 = 0.0f;
			result.M41 = tx;
			result.M42 = ty;
			result.M43 = tz;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a uniform scaling matrix that scales equally on each axis.
		/// </summary>
		/// <param name="scale">The uniform scaling factor.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4x4 CreateScale(float scale)
		{
			Matrix4x4 result;

			result.M11 = scale;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = scale;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = scale;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a uniform scaling matrix that scales equally on each axis with a center point.
		/// </summary>
		/// <param name="scale">The uniform scaling factor.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The scaling matrix.</returns>
		public static Matrix4x4 CreateScale(float scale, Vector3 centerPoint)
		{
			Matrix4x4 result;

			float tx = centerPoint.x * (1 - scale);
			float ty = centerPoint.y * (1 - scale);
			float tz = centerPoint.z * (1 - scale);

			result.M11 = scale;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = scale;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = scale;
			result.M34 = 0.0f;
			result.M41 = tx;
			result.M42 = ty;
			result.M43 = tz;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix for rotating points around the X-axis.
		/// </summary>
		/// <param name="radians">The amount, in radians, by which to rotate around the X-axis.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4x4 CreateRotationX(float radians)
		{
			Matrix4x4 result;

			float c = (float)Math.Cos(radians);
			float s = (float)Math.Sin(radians);

			// [  1  0  0  0 ]
			// [  0  c  s  0 ]
			// [  0 -s  c  0 ]
			// [  0  0  0  1 ]
			result.M11 = 1.0f;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = c;
			result.M23 = s;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = -s;
			result.M33 = c;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix for rotating points around the X-axis, from a center point.
		/// </summary>
		/// <param name="radians">The amount, in radians, by which to rotate around the X-axis.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4x4 CreateRotationX(float radians, Vector3 centerPoint)
		{
			Matrix4x4 result;

			float c = (float)Math.Cos(radians);
			float s = (float)Math.Sin(radians);

			float y = centerPoint.y * (1 - c) + centerPoint.z * s;
			float z = centerPoint.z * (1 - c) - centerPoint.y * s;

			// [  1  0  0  0 ]
			// [  0  c  s  0 ]
			// [  0 -s  c  0 ]
			// [  0  y  z  1 ]
			result.M11 = 1.0f;
			result.M12 = 0.0f;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = c;
			result.M23 = s;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = -s;
			result.M33 = c;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = y;
			result.M43 = z;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix for rotating points around the Y-axis.
		/// </summary>
		/// <param name="radians">The amount, in radians, by which to rotate around the Y-axis.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4x4 CreateRotationY(float radians)
		{
			Matrix4x4 result;

			float c = (float)Math.Cos(radians);
			float s = (float)Math.Sin(radians);

			// [  c  0 -s  0 ]
			// [  0  1  0  0 ]
			// [  s  0  c  0 ]
			// [  0  0  0  1 ]
			result.M11 = c;
			result.M12 = 0.0f;
			result.M13 = -s;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = 1.0f;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = s;
			result.M32 = 0.0f;
			result.M33 = c;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix for rotating points around the Y-axis, from a center point.
		/// </summary>
		/// <param name="radians">The amount, in radians, by which to rotate around the Y-axis.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4x4 CreateRotationY(float radians, Vector3 centerPoint)
		{
			Matrix4x4 result;

			float c = (float)Math.Cos(radians);
			float s = (float)Math.Sin(radians);

			float x = centerPoint.x * (1 - c) - centerPoint.z * s;
			float z = centerPoint.z * (1 - c) + centerPoint.x * s;

			// [  c  0 -s  0 ]
			// [  0  1  0  0 ]
			// [  s  0  c  0 ]
			// [  x  0  z  1 ]
			result.M11 = c;
			result.M12 = 0.0f;
			result.M13 = -s;
			result.M14 = 0.0f;
			result.M21 = 0.0f;
			result.M22 = 1.0f;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = s;
			result.M32 = 0.0f;
			result.M33 = c;
			result.M34 = 0.0f;
			result.M41 = x;
			result.M42 = 0.0f;
			result.M43 = z;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix for rotating points around the Z-axis.
		/// </summary>
		/// <param name="radians">The amount, in radians, by which to rotate around the Z-axis.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4x4 CreateRotationZ(float radians)
		{
			Matrix4x4 result;

			float c = (float)Math.Cos(radians);
			float s = (float)Math.Sin(radians);

			// [  c  s  0  0 ]
			// [ -s  c  0  0 ]
			// [  0  0  1  0 ]
			// [  0  0  0  1 ]
			result.M11 = c;
			result.M12 = s;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = -s;
			result.M22 = c;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = 1.0f;
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix for rotating points around the Z-axis, from a center point.
		/// </summary>
		/// <param name="radians">The amount, in radians, by which to rotate around the Z-axis.</param>
		/// <param name="centerPoint">The center point.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4x4 CreateRotationZ(float radians, Vector3 centerPoint)
		{
			Matrix4x4 result;

			float c = (float)Math.Cos(radians);
			float s = (float)Math.Sin(radians);

			float x = centerPoint.x * (1 - c) + centerPoint.y * s;
			float y = centerPoint.y * (1 - c) - centerPoint.x * s;

			// [  c  s  0  0 ]
			// [ -s  c  0  0 ]
			// [  0  0  1  0 ]
			// [  x  y  0  1 ]
			result.M11 = c;
			result.M12 = s;
			result.M13 = 0.0f;
			result.M14 = 0.0f;
			result.M21 = -s;
			result.M22 = c;
			result.M23 = 0.0f;
			result.M24 = 0.0f;
			result.M31 = 0.0f;
			result.M32 = 0.0f;
			result.M33 = 1.0f;
			result.M34 = 0.0f;
			result.M41 = x;
			result.M42 = y;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a matrix that rotates around an arbitrary vector.
		/// </summary>
		/// <param name="axis">The axis to rotate around.</param>
		/// <param name="angle">The angle to rotate around the given axis, in radians.</param>
		/// <returns>The rotation matrix.</returns>
		public static Matrix4x4 CreateFromAxisAngle(Vector3 axis, float angle)
		{
			// a: angle
			// x, y, z: unit vector for axis.
			//
			// Rotation matrix M can compute by using below equation.
			//
			//        T               T
			//  M = uu + (cos a)( I-uu ) + (sin a)S
			//
			// Where:
			//
			//  u = ( x, y, z )
			//
			//      [  0 -z  y ]
			//  S = [  z  0 -x ]
			//      [ -y  x  0 ]
			//
			//      [ 1 0 0 ]
			//  I = [ 0 1 0 ]
			//      [ 0 0 1 ]
			//
			//
			//     [  xx+cosa*(1-xx)   yx-cosa*yx-sina*z zx-cosa*xz+sina*y ]
			// M = [ xy-cosa*yx+sina*z    yy+cosa(1-yy)  yz-cosa*yz-sina*x ]
			//     [ zx-cosa*zx-sina*y zy-cosa*zy+sina*x   zz+cosa*(1-zz)  ]
			//
			float x = axis.x, y = axis.y, z = axis.z;
			float sa = (float)Math.Sin(angle), ca = (float)Math.Cos(angle);
			float xx = x * x, yy = y * y, zz = z * z;
			float xy = x * y, xz = x * z, yz = y * z;

			Matrix4x4 result;

			result.M11 = xx + ca * (1.0f - xx);
			result.M12 = xy - ca * xy + sa * z;
			result.M13 = xz - ca * xz - sa * y;
			result.M14 = 0.0f;
			result.M21 = xy - ca * xy - sa * z;
			result.M22 = yy + ca * (1.0f - yy);
			result.M23 = yz - ca * yz + sa * x;
			result.M24 = 0.0f;
			result.M31 = xz - ca * xz + sa * y;
			result.M32 = yz - ca * yz - sa * x;
			result.M33 = zz + ca * (1.0f - zz);
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a perspective projection matrix based on a field of view, aspect ratio, and near and far view plane distances. 
		/// </summary>
		/// <param name="fieldOfView">Field of view in the y direction, in radians.</param>
		/// <param name="aspectRatio">Aspect ratio, defined as view space width divided by height.</param>
		/// <param name="nearPlaneDistance">Distance to the near view plane.</param>
		/// <param name="farPlaneDistance">Distance to the far view plane.</param>
		/// <returns>The perspective projection matrix.</returns>
		public static Matrix4x4 CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
		{
			if (fieldOfView <= 0.0f || fieldOfView >= Math.PI)
				throw new ArgumentOutOfRangeException("fieldOfView");

			if (nearPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException("nearPlaneDistance");

			if (farPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException("farPlaneDistance");

			if (nearPlaneDistance >= farPlaneDistance)
				throw new ArgumentOutOfRangeException("nearPlaneDistance");

			float yScale = 1.0f / (float)Math.Tan(fieldOfView * 0.5f);
			float xScale = yScale / aspectRatio;

			Matrix4x4 result;

			result.M11 = xScale;
			result.M12 = result.M13 = result.M14 = 0.0f;

			result.M22 = yScale;
			result.M21 = result.M23 = result.M24 = 0.0f;

			result.M31 = result.M32 = 0.0f;
			result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
			result.M34 = -1.0f;

			result.M41 = result.M42 = result.M44 = 0.0f;
			result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

			return result;
		}

		/// <summary>
		/// Creates a perspective projection matrix from the given view volume dimensions.
		/// </summary>
		/// <param name="width">Width of the view volume at the near view plane.</param>
		/// <param name="height">Height of the view volume at the near view plane.</param>
		/// <param name="nearPlaneDistance">Distance to the near view plane.</param>
		/// <param name="farPlaneDistance">Distance to the far view plane.</param>
		/// <returns>The perspective projection matrix.</returns>
		public static Matrix4x4 CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance)
		{
			if (nearPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException("nearPlaneDistance");

			if (farPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException("farPlaneDistance");

			if (nearPlaneDistance >= farPlaneDistance)
				throw new ArgumentOutOfRangeException("nearPlaneDistance");

			Matrix4x4 result;

			result.M11 = 2.0f * nearPlaneDistance / width;
			result.M12 = result.M13 = result.M14 = 0.0f;

			result.M22 = 2.0f * nearPlaneDistance / height;
			result.M21 = result.M23 = result.M24 = 0.0f;

			result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
			result.M31 = result.M32 = 0.0f;
			result.M34 = -1.0f;

			result.M41 = result.M42 = result.M44 = 0.0f;
			result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

			return result;
		}

		/// <summary>
		/// Creates a customized, perspective projection matrix.
		/// </summary>
		/// <param name="left">Minimum x-value of the view volume at the near view plane.</param>
		/// <param name="right">Maximum x-value of the view volume at the near view plane.</param>
		/// <param name="bottom">Minimum y-value of the view volume at the near view plane.</param>
		/// <param name="top">Maximum y-value of the view volume at the near view plane.</param>
		/// <param name="nearPlaneDistance">Distance to the near view plane.</param>
		/// <param name="farPlaneDistance">Distance to of the far view plane.</param>
		/// <returns>The perspective projection matrix.</returns>
		public static Matrix4x4 CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
		{
			if (nearPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException("nearPlaneDistance");

			if (farPlaneDistance <= 0.0f)
				throw new ArgumentOutOfRangeException("farPlaneDistance");

			if (nearPlaneDistance >= farPlaneDistance)
				throw new ArgumentOutOfRangeException("nearPlaneDistance");

			Matrix4x4 result;

			result.M11 = 2.0f * nearPlaneDistance / (right - left);
			result.M12 = result.M13 = result.M14 = 0.0f;

			result.M22 = 2.0f * nearPlaneDistance / (top - bottom);
			result.M21 = result.M23 = result.M24 = 0.0f;

			result.M31 = (left + right) / (right - left);
			result.M32 = (top + bottom) / (top - bottom);
			result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
			result.M34 = -1.0f;

			result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
			result.M41 = result.M42 = result.M44 = 0.0f;

			return result;
		}

		/// <summary>
		/// Creates an orthographic perspective matrix from the given view volume dimensions.
		/// </summary>
		/// <param name="width">Width of the view volume.</param>
		/// <param name="height">Height of the view volume.</param>
		/// <param name="zNearPlane">Minimum Z-value of the view volume.</param>
		/// <param name="zFarPlane">Maximum Z-value of the view volume.</param>
		/// <returns>The orthographic projection matrix.</returns>
		public static Matrix4x4 CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
		{
			Matrix4x4 result;

			result.M11 = 2.0f / width;
			result.M12 = result.M13 = result.M14 = 0.0f;

			result.M22 = 2.0f / height;
			result.M21 = result.M23 = result.M24 = 0.0f;

			result.M33 = 1.0f / (zNearPlane - zFarPlane);
			result.M31 = result.M32 = result.M34 = 0.0f;

			result.M41 = result.M42 = 0.0f;
			result.M43 = zNearPlane / (zNearPlane - zFarPlane);
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Builds a customized, orthographic projection matrix.
		/// </summary>
		/// <param name="left">Minimum X-value of the view volume.</param>
		/// <param name="right">Maximum X-value of the view volume.</param>
		/// <param name="bottom">Minimum Y-value of the view volume.</param>
		/// <param name="top">Maximum Y-value of the view volume.</param>
		/// <param name="zNearPlane">Minimum Z-value of the view volume.</param>
		/// <param name="zFarPlane">Maximum Z-value of the view volume.</param>
		/// <returns>The orthographic projection matrix.</returns>
		public static Matrix4x4 CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
		{
			Matrix4x4 result;

			result.M11 = 2.0f / (right - left);
			result.M12 = result.M13 = result.M14 = 0.0f;

			result.M22 = 2.0f / (top - bottom);
			result.M21 = result.M23 = result.M24 = 0.0f;

			result.M33 = 1.0f / (zNearPlane - zFarPlane);
			result.M31 = result.M32 = result.M34 = 0.0f;

			result.M41 = (left + right) / (left - right);
			result.M42 = (top + bottom) / (bottom - top);
			result.M43 = zNearPlane / (zNearPlane - zFarPlane);
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a view matrix.
		/// </summary>
		/// <param name="cameraPosition">The position of the camera.</param>
		/// <param name="cameraTarget">The target towards which the camera is pointing.</param>
		/// <param name="cameraUpVector">The direction that is "up" from the camera's point of view.</param>
		/// <returns>The view matrix.</returns>
		public static Matrix4x4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
		{
			Vector3 zaxis = Vector3.Normalize(cameraPosition - cameraTarget);
			Vector3 xaxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zaxis));
			Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

			Matrix4x4 result;

			result.M11 = xaxis.x;
			result.M12 = yaxis.x;
			result.M13 = zaxis.x;
			result.M14 = 0.0f;
			result.M21 = xaxis.y;
			result.M22 = yaxis.y;
			result.M23 = zaxis.y;
			result.M24 = 0.0f;
			result.M31 = xaxis.z;
			result.M32 = yaxis.z;
			result.M33 = zaxis.z;
			result.M34 = 0.0f;
			result.M41 = -Vector3.Dot(xaxis, cameraPosition);
			result.M42 = -Vector3.Dot(yaxis, cameraPosition);
			result.M43 = -Vector3.Dot(zaxis, cameraPosition);
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Creates a world matrix with the specified parameters.
		/// </summary>
		/// <param name="position">The position of the object; used in translation operations.</param>
		/// <param name="forward">Forward direction of the object.</param>
		/// <param name="up">Upward direction of the object; usually [0, 1, 0].</param>
		/// <returns>The world matrix.</returns>
		public static Matrix4x4 CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
		{
			Vector3 zaxis = Vector3.Normalize(-forward);
			Vector3 xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
			Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

			Matrix4x4 result;

			result.M11 = xaxis.x;
			result.M12 = xaxis.y;
			result.M13 = xaxis.z;
			result.M14 = 0.0f;
			result.M21 = yaxis.x;
			result.M22 = yaxis.y;
			result.M23 = yaxis.z;
			result.M24 = 0.0f;
			result.M31 = zaxis.x;
			result.M32 = zaxis.y;
			result.M33 = zaxis.z;
			result.M34 = 0.0f;
			result.M41 = position.x;
			result.M42 = position.y;
			result.M43 = position.z;
			result.M44 = 1.0f;

			return result;
		}

		/// <summary>
		/// Calculates the determinant of the matrix.
		/// </summary>
		/// <returns>The determinant of the matrix.</returns>
		public float GetDeterminant()
		{
			// | a b c d |     | f g h |     | e g h |     | e f h |     | e f g |
			// | e f g h | = a | j k l | - b | i k l | + c | i j l | - d | i j k |
			// | i j k l |     | n o p |     | m o p |     | m n p |     | m n o |
			// | m n o p |
			//
			//   | f g h |
			// a | j k l | = a ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
			//   | n o p |
			//
			//   | e g h |     
			// b | i k l | = b ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
			//   | m o p |     
			//
			//   | e f h |
			// c | i j l | = c ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
			//   | m n p |
			//
			//   | e f g |
			// d | i j k | = d ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
			//   | m n o |
			//
			// Cost of operation
			// 17 adds and 28 muls.
			//
			// add: 6 + 8 + 3 = 17
			// mul: 12 + 16 = 28

			float a = M11, b = M12, c = M13, d = M14;
			float e = M21, f = M22, g = M23, h = M24;
			float i = M31, j = M32, k = M33, l = M34;
			float m = M41, n = M42, o = M43, p = M44;

			float kp_lo = k * p - l * o;
			float jp_ln = j * p - l * n;
			float jo_kn = j * o - k * n;
			float ip_lm = i * p - l * m;
			float io_km = i * o - k * m;
			float in_jm = i * n - j * m;

			return a * (f * kp_lo - g * jp_ln + h * jo_kn) -
				   b * (e * kp_lo - g * ip_lm + h * io_km) +
				   c * (e * jp_ln - f * ip_lm + h * in_jm) -
				   d * (e * jo_kn - f * io_km + g * in_jm);
		}

		/// <summary>
		/// Attempts to calculate the inverse of the given matrix. If successful, result will contain the inverted matrix.
		/// </summary>
		/// <param name="matrix">The source matrix to invert.</param>
		/// <param name="result">If successful, contains the inverted matrix.</param>
		/// <returns>True if the source matrix could be inverted; False otherwise.</returns>
		public static bool Invert(Matrix4x4 matrix, out Matrix4x4 result)
		{
			//                                       -1
			// If you have matrix M, inverse Matrix M   can compute
			//
			//     -1       1      
			//    M   = --------- A
			//            det(M)
			//
			// A is adjugate (adjoint) of M, where,
			//
			//      T
			// A = C
			//
			// C is Cofactor matrix of M, where,
			//           i + j
			// C   = (-1)      * det(M  )
			//  ij                    ij
			//
			//     [ a b c d ]
			// M = [ e f g h ]
			//     [ i j k l ]
			//     [ m n o p ]
			//
			// First Row
			//           2 | f g h |
			// C   = (-1)  | j k l | = + ( f ( kp - lo ) - g ( jp - ln ) + h ( jo - kn ) )
			//  11         | n o p |
			//
			//           3 | e g h |
			// C   = (-1)  | i k l | = - ( e ( kp - lo ) - g ( ip - lm ) + h ( io - km ) )
			//  12         | m o p |
			//
			//           4 | e f h |
			// C   = (-1)  | i j l | = + ( e ( jp - ln ) - f ( ip - lm ) + h ( in - jm ) )
			//  13         | m n p |
			//
			//           5 | e f g |
			// C   = (-1)  | i j k | = - ( e ( jo - kn ) - f ( io - km ) + g ( in - jm ) )
			//  14         | m n o |
			//
			// Second Row
			//           3 | b c d |
			// C   = (-1)  | j k l | = - ( b ( kp - lo ) - c ( jp - ln ) + d ( jo - kn ) )
			//  21         | n o p |
			//
			//           4 | a c d |
			// C   = (-1)  | i k l | = + ( a ( kp - lo ) - c ( ip - lm ) + d ( io - km ) )
			//  22         | m o p |
			//
			//           5 | a b d |
			// C   = (-1)  | i j l | = - ( a ( jp - ln ) - b ( ip - lm ) + d ( in - jm ) )
			//  23         | m n p |
			//
			//           6 | a b c |
			// C   = (-1)  | i j k | = + ( a ( jo - kn ) - b ( io - km ) + c ( in - jm ) )
			//  24         | m n o |
			//
			// Third Row
			//           4 | b c d |
			// C   = (-1)  | f g h | = + ( b ( gp - ho ) - c ( fp - hn ) + d ( fo - gn ) )
			//  31         | n o p |
			//
			//           5 | a c d |
			// C   = (-1)  | e g h | = - ( a ( gp - ho ) - c ( ep - hm ) + d ( eo - gm ) )
			//  32         | m o p |
			//
			//           6 | a b d |
			// C   = (-1)  | e f h | = + ( a ( fp - hn ) - b ( ep - hm ) + d ( en - fm ) )
			//  33         | m n p |
			//
			//           7 | a b c |
			// C   = (-1)  | e f g | = - ( a ( fo - gn ) - b ( eo - gm ) + c ( en - fm ) )
			//  34         | m n o |
			//
			// Fourth Row
			//           5 | b c d |
			// C   = (-1)  | f g h | = - ( b ( gl - hk ) - c ( fl - hj ) + d ( fk - gj ) )
			//  41         | j k l |
			//
			//           6 | a c d |
			// C   = (-1)  | e g h | = + ( a ( gl - hk ) - c ( el - hi ) + d ( ek - gi ) )
			//  42         | i k l |
			//
			//           7 | a b d |
			// C   = (-1)  | e f h | = - ( a ( fl - hj ) - b ( el - hi ) + d ( ej - fi ) )
			//  43         | i j l |
			//
			//           8 | a b c |
			// C   = (-1)  | e f g | = + ( a ( fk - gj ) - b ( ek - gi ) + c ( ej - fi ) )
			//  44         | i j k |
			//
			// Cost of operation
			// 53 adds, 104 muls, and 1 div.
			float a = matrix.M11, b = matrix.M12, c = matrix.M13, d = matrix.M14;
			float e = matrix.M21, f = matrix.M22, g = matrix.M23, h = matrix.M24;
			float i = matrix.M31, j = matrix.M32, k = matrix.M33, l = matrix.M34;
			float m = matrix.M41, n = matrix.M42, o = matrix.M43, p = matrix.M44;

			float kp_lo = k * p - l * o;
			float jp_ln = j * p - l * n;
			float jo_kn = j * o - k * n;
			float ip_lm = i * p - l * m;
			float io_km = i * o - k * m;
			float in_jm = i * n - j * m;

			float a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
			float a12 = -(e * kp_lo - g * ip_lm + h * io_km);
			float a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
			float a14 = -(e * jo_kn - f * io_km + g * in_jm);

			float det = a * a11 + b * a12 + c * a13 + d * a14;

			if (Math.Abs(det) < float.Epsilon)
			{
				result = new Matrix4x4(float.NaN, float.NaN, float.NaN, float.NaN,
									   float.NaN, float.NaN, float.NaN, float.NaN,
									   float.NaN, float.NaN, float.NaN, float.NaN,
									   float.NaN, float.NaN, float.NaN, float.NaN);
				return false;
			}

			float invDet = 1.0f / det;

			result.M11 = a11 * invDet;
			result.M21 = a12 * invDet;
			result.M31 = a13 * invDet;
			result.M41 = a14 * invDet;

			result.M12 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
			result.M22 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
			result.M32 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
			result.M42 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

			float gp_ho = g * p - h * o;
			float fp_hn = f * p - h * n;
			float fo_gn = f * o - g * n;
			float ep_hm = e * p - h * m;
			float eo_gm = e * o - g * m;
			float en_fm = e * n - f * m;

			result.M13 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
			result.M23 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
			result.M33 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
			result.M43 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

			float gl_hk = g * l - h * k;
			float fl_hj = f * l - h * j;
			float fk_gj = f * k - g * j;
			float el_hi = e * l - h * i;
			float ek_gi = e * k - g * i;
			float ej_fi = e * j - f * i;

			result.M14 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
			result.M24 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
			result.M34 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
			result.M44 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

			return true;
		}

		struct CanonicalBasis
		{
			public Vector3 Row0;
			public Vector3 Row1;
			public Vector3 Row2;
		};

		public static Matrix4x4 InvertMatrix(Matrix4x4 matrix)
		{
			Matrix4x4 result = new Matrix4x4();
			float a = matrix.M11, b = matrix.M12, c = matrix.M13, d = matrix.M14;
			float e = matrix.M21, f = matrix.M22, g = matrix.M23, h = matrix.M24;
			float i = matrix.M31, j = matrix.M32, k = matrix.M33, l = matrix.M34;
			float m = matrix.M41, n = matrix.M42, o = matrix.M43, p = matrix.M44;

			float kp_lo = k * p - l * o;
			float jp_ln = j * p - l * n;
			float jo_kn = j * o - k * n;
			float ip_lm = i * p - l * m;
			float io_km = i * o - k * m;
			float in_jm = i * n - j * m;

			float a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
			float a12 = -(e * kp_lo - g * ip_lm + h * io_km);
			float a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
			float a14 = -(e * jo_kn - f * io_km + g * in_jm);

			float det = a * a11 + b * a12 + c * a13 + d * a14;

			if (Math.Abs(det) < float.Epsilon)
			{
				result = new Matrix4x4(float.NaN, float.NaN, float.NaN, float.NaN,
									   float.NaN, float.NaN, float.NaN, float.NaN,
									   float.NaN, float.NaN, float.NaN, float.NaN,
									   float.NaN, float.NaN, float.NaN, float.NaN);
				return result;
			}

			float invDet = 1.0f / det;

			result.M11 = a11 * invDet;
			result.M21 = a12 * invDet;
			result.M31 = a13 * invDet;
			result.M41 = a14 * invDet;

			result.M12 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
			result.M22 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
			result.M32 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
			result.M42 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

			float gp_ho = g * p - h * o;
			float fp_hn = f * p - h * n;
			float fo_gn = f * o - g * n;
			float ep_hm = e * p - h * m;
			float eo_gm = e * o - g * m;
			float en_fm = e * n - f * m;

			result.M13 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
			result.M23 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
			result.M33 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
			result.M43 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

			float gl_hk = g * l - h * k;
			float fl_hj = f * l - h * j;
			float fk_gj = f * k - g * j;
			float el_hi = e * l - h * i;
			float ek_gi = e * k - g * i;
			float ej_fi = e * j - f * i;

			result.M14 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
			result.M24 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
			result.M34 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
			result.M44 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

			return result;
		}


		//[System.Security.SecuritySafeCritical]
		//struct VectorBasis
		//{
		//	public unsafe Vector3* Element0;
		//	public unsafe Vector3* Element1;
		//	public unsafe Vector3* Element2;
		//}

		/// <summary>
		/// Transforms the given matrix by applying the given Quaternion rotation.
		/// </summary>
		/// <param name="value">The source matrix to transform.</param>
		/// <param name="rotation">The rotation to apply.</param>
		/// <returns>The transformed matrix.</returns>
		public static Matrix4x4 Transform(Matrix4x4 value, Quaternion rotation)
		{
			// Compute rotation matrix.
			float x2 = rotation.x + rotation.x;
			float y2 = rotation.y + rotation.y;
			float z2 = rotation.z + rotation.z;

			float wx2 = rotation.w * x2;
			float wy2 = rotation.w * y2;
			float wz2 = rotation.w * z2;
			float xx2 = rotation.x * x2;
			float xy2 = rotation.x * y2;
			float xz2 = rotation.x * z2;
			float yy2 = rotation.y * y2;
			float yz2 = rotation.y * z2;
			float zz2 = rotation.z * z2;

			float q11 = 1.0f - yy2 - zz2;
			float q21 = xy2 - wz2;
			float q31 = xz2 + wy2;

			float q12 = xy2 + wz2;
			float q22 = 1.0f - xx2 - zz2;
			float q32 = yz2 - wx2;

			float q13 = xz2 - wy2;
			float q23 = yz2 + wx2;
			float q33 = 1.0f - xx2 - yy2;

			Matrix4x4 result;

			// First row
			result.M11 = value.M11 * q11 + value.M12 * q21 + value.M13 * q31;
			result.M12 = value.M11 * q12 + value.M12 * q22 + value.M13 * q32;
			result.M13 = value.M11 * q13 + value.M12 * q23 + value.M13 * q33;
			result.M14 = value.M14;

			// Second row
			result.M21 = value.M21 * q11 + value.M22 * q21 + value.M23 * q31;
			result.M22 = value.M21 * q12 + value.M22 * q22 + value.M23 * q32;
			result.M23 = value.M21 * q13 + value.M22 * q23 + value.M23 * q33;
			result.M24 = value.M24;

			// Third row
			result.M31 = value.M31 * q11 + value.M32 * q21 + value.M33 * q31;
			result.M32 = value.M31 * q12 + value.M32 * q22 + value.M33 * q32;
			result.M33 = value.M31 * q13 + value.M32 * q23 + value.M33 * q33;
			result.M34 = value.M34;

			// Fourth row
			result.M41 = value.M41 * q11 + value.M42 * q21 + value.M43 * q31;
			result.M42 = value.M41 * q12 + value.M42 * q22 + value.M43 * q32;
			result.M43 = value.M41 * q13 + value.M42 * q23 + value.M43 * q33;
			result.M44 = value.M44;

			return result;
		}

		/// <summary>
		/// Transposes the rows and columns of a matrix.
		/// </summary>
		/// <param name="matrix">The source matrix.</param>
		/// <returns>The transposed matrix.</returns>
		public static Matrix4x4 Transpose(Matrix4x4 matrix)
		{
			Matrix4x4 result;

			result.M11 = matrix.M11;
			result.M12 = matrix.M21;
			result.M13 = matrix.M31;
			result.M14 = matrix.M41;
			result.M21 = matrix.M12;
			result.M22 = matrix.M22;
			result.M23 = matrix.M32;
			result.M24 = matrix.M42;
			result.M31 = matrix.M13;
			result.M32 = matrix.M23;
			result.M33 = matrix.M33;
			result.M34 = matrix.M43;
			result.M41 = matrix.M14;
			result.M42 = matrix.M24;
			result.M43 = matrix.M34;
			result.M44 = matrix.M44;

			return result;
		}

		/// <summary>
		/// Linearly interpolates between the corresponding values of two matrices.
		/// </summary>
		/// <param name="matrix1">The first source matrix.</param>
		/// <param name="matrix2">The second source matrix.</param>
		/// <param name="amount">The relative weight of the second source matrix.</param>
		/// <returns>The interpolated matrix.</returns>
		public static Matrix4x4 Lerp(Matrix4x4 matrix1, Matrix4x4 matrix2, float amount)
		{
			Matrix4x4 result;

			// First row
			result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
			result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
			result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
			result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;

			// Second row
			result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
			result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
			result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
			result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;

			// Third row
			result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
			result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
			result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
			result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;

			// Fourth row
			result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
			result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
			result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
			result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;

			return result;
		}

		/// <summary>
		/// Returns a new matrix with the negated elements of the given matrix.
		/// </summary>
		/// <param name="value">The source matrix.</param>
		/// <returns>The negated matrix.</returns>
		public static Matrix4x4 Negate(Matrix4x4 value)
		{
			Matrix4x4 result;

			result.M11 = -value.M11;
			result.M12 = -value.M12;
			result.M13 = -value.M13;
			result.M14 = -value.M14;
			result.M21 = -value.M21;
			result.M22 = -value.M22;
			result.M23 = -value.M23;
			result.M24 = -value.M24;
			result.M31 = -value.M31;
			result.M32 = -value.M32;
			result.M33 = -value.M33;
			result.M34 = -value.M34;
			result.M41 = -value.M41;
			result.M42 = -value.M42;
			result.M43 = -value.M43;
			result.M44 = -value.M44;

			return result;
		}

		/// <summary>
		/// Adds two matrices together.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The resulting matrix.</returns>
		public static Matrix4x4 Add(Matrix4x4 value1, Matrix4x4 value2)
		{
			Matrix4x4 result;

			result.M11 = value1.M11 + value2.M11;
			result.M12 = value1.M12 + value2.M12;
			result.M13 = value1.M13 + value2.M13;
			result.M14 = value1.M14 + value2.M14;
			result.M21 = value1.M21 + value2.M21;
			result.M22 = value1.M22 + value2.M22;
			result.M23 = value1.M23 + value2.M23;
			result.M24 = value1.M24 + value2.M24;
			result.M31 = value1.M31 + value2.M31;
			result.M32 = value1.M32 + value2.M32;
			result.M33 = value1.M33 + value2.M33;
			result.M34 = value1.M34 + value2.M34;
			result.M41 = value1.M41 + value2.M41;
			result.M42 = value1.M42 + value2.M42;
			result.M43 = value1.M43 + value2.M43;
			result.M44 = value1.M44 + value2.M44;

			return result;
		}

		/// <summary>
		/// Subtracts the second matrix from the first.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The result of the subtraction.</returns>
		public static Matrix4x4 Subtract(Matrix4x4 value1, Matrix4x4 value2)
		{
			Matrix4x4 result;

			result.M11 = value1.M11 - value2.M11;
			result.M12 = value1.M12 - value2.M12;
			result.M13 = value1.M13 - value2.M13;
			result.M14 = value1.M14 - value2.M14;
			result.M21 = value1.M21 - value2.M21;
			result.M22 = value1.M22 - value2.M22;
			result.M23 = value1.M23 - value2.M23;
			result.M24 = value1.M24 - value2.M24;
			result.M31 = value1.M31 - value2.M31;
			result.M32 = value1.M32 - value2.M32;
			result.M33 = value1.M33 - value2.M33;
			result.M34 = value1.M34 - value2.M34;
			result.M41 = value1.M41 - value2.M41;
			result.M42 = value1.M42 - value2.M42;
			result.M43 = value1.M43 - value2.M43;
			result.M44 = value1.M44 - value2.M44;

			return result;
		}

		/// <summary>
		/// Multiplies a matrix by another matrix.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Matrix4x4 Multiply(Matrix4x4 value1, Matrix4x4 value2)
		{
			Matrix4x4 result;

			// First row
			result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41;
			result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42;
			result.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43;
			result.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44;

			// Second row
			result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41;
			result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42;
			result.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43;
			result.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44;

			// Third row
			result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41;
			result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42;
			result.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43;
			result.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44;

			// Fourth row
			result.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41;
			result.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42;
			result.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43;
			result.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44;

			return result;
		}

		/// <summary>
		/// Multiplies a matrix by a scalar value.
		/// </summary>
		/// <param name="value1">The source matrix.</param>
		/// <param name="value2">The scaling factor.</param>
		/// <returns>The scaled matrix.</returns>
		public static Matrix4x4 Multiply(Matrix4x4 value1, float value2)
		{
			Matrix4x4 result;

			result.M11 = value1.M11 * value2;
			result.M12 = value1.M12 * value2;
			result.M13 = value1.M13 * value2;
			result.M14 = value1.M14 * value2;
			result.M21 = value1.M21 * value2;
			result.M22 = value1.M22 * value2;
			result.M23 = value1.M23 * value2;
			result.M24 = value1.M24 * value2;
			result.M31 = value1.M31 * value2;
			result.M32 = value1.M32 * value2;
			result.M33 = value1.M33 * value2;
			result.M34 = value1.M34 * value2;
			result.M41 = value1.M41 * value2;
			result.M42 = value1.M42 * value2;
			result.M43 = value1.M43 * value2;
			result.M44 = value1.M44 * value2;

			return result;
		}

		/// <summary>
		/// Returns a new matrix with the negated elements of the given matrix.
		/// </summary>
		/// <param name="value">The source matrix.</param>
		/// <returns>The negated matrix.</returns>
		public static Matrix4x4 operator -(Matrix4x4 value)
		{
			Matrix4x4 m;

			m.M11 = -value.M11;
			m.M12 = -value.M12;
			m.M13 = -value.M13;
			m.M14 = -value.M14;
			m.M21 = -value.M21;
			m.M22 = -value.M22;
			m.M23 = -value.M23;
			m.M24 = -value.M24;
			m.M31 = -value.M31;
			m.M32 = -value.M32;
			m.M33 = -value.M33;
			m.M34 = -value.M34;
			m.M41 = -value.M41;
			m.M42 = -value.M42;
			m.M43 = -value.M43;
			m.M44 = -value.M44;

			return m;
		}

		/// <summary>
		/// Adds two matrices together.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The resulting matrix.</returns>
		public static Matrix4x4 operator +(Matrix4x4 value1, Matrix4x4 value2)
		{
			Matrix4x4 m;

			m.M11 = value1.M11 + value2.M11;
			m.M12 = value1.M12 + value2.M12;
			m.M13 = value1.M13 + value2.M13;
			m.M14 = value1.M14 + value2.M14;
			m.M21 = value1.M21 + value2.M21;
			m.M22 = value1.M22 + value2.M22;
			m.M23 = value1.M23 + value2.M23;
			m.M24 = value1.M24 + value2.M24;
			m.M31 = value1.M31 + value2.M31;
			m.M32 = value1.M32 + value2.M32;
			m.M33 = value1.M33 + value2.M33;
			m.M34 = value1.M34 + value2.M34;
			m.M41 = value1.M41 + value2.M41;
			m.M42 = value1.M42 + value2.M42;
			m.M43 = value1.M43 + value2.M43;
			m.M44 = value1.M44 + value2.M44;

			return m;
		}

		/// <summary>
		/// Subtracts the second matrix from the first.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The result of the subtraction.</returns>
		public static Matrix4x4 operator -(Matrix4x4 value1, Matrix4x4 value2)
		{
			Matrix4x4 m;

			m.M11 = value1.M11 - value2.M11;
			m.M12 = value1.M12 - value2.M12;
			m.M13 = value1.M13 - value2.M13;
			m.M14 = value1.M14 - value2.M14;
			m.M21 = value1.M21 - value2.M21;
			m.M22 = value1.M22 - value2.M22;
			m.M23 = value1.M23 - value2.M23;
			m.M24 = value1.M24 - value2.M24;
			m.M31 = value1.M31 - value2.M31;
			m.M32 = value1.M32 - value2.M32;
			m.M33 = value1.M33 - value2.M33;
			m.M34 = value1.M34 - value2.M34;
			m.M41 = value1.M41 - value2.M41;
			m.M42 = value1.M42 - value2.M42;
			m.M43 = value1.M43 - value2.M43;
			m.M44 = value1.M44 - value2.M44;

			return m;
		}

		/// <summary>
		/// Multiplies a matrix by another matrix.
		/// </summary>
		/// <param name="value1">The first source matrix.</param>
		/// <param name="value2">The second source matrix.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Matrix4x4 operator *(Matrix4x4 value1, Matrix4x4 value2)
		{
			Matrix4x4 m;

			// First row
			m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41;
			m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42;
			m.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43;
			m.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44;

			// Second row
			m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41;
			m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42;
			m.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43;
			m.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44;

			// Third row
			m.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41;
			m.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42;
			m.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43;
			m.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44;

			// Fourth row
			m.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41;
			m.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42;
			m.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43;
			m.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44;

			return m;
		}

		/// <summary>
		/// Multiplies a matrix by a scalar value.
		/// </summary>
		/// <param name="value1">The source matrix.</param>
		/// <param name="value2">The scaling factor.</param>
		/// <returns>The scaled matrix.</returns>
		public static Matrix4x4 operator *(Matrix4x4 value1, float value2)
		{
			Matrix4x4 m;

			m.M11 = value1.M11 * value2;
			m.M12 = value1.M12 * value2;
			m.M13 = value1.M13 * value2;
			m.M14 = value1.M14 * value2;
			m.M21 = value1.M21 * value2;
			m.M22 = value1.M22 * value2;
			m.M23 = value1.M23 * value2;
			m.M24 = value1.M24 * value2;
			m.M31 = value1.M31 * value2;
			m.M32 = value1.M32 * value2;
			m.M33 = value1.M33 * value2;
			m.M34 = value1.M34 * value2;
			m.M41 = value1.M41 * value2;
			m.M42 = value1.M42 * value2;
			m.M43 = value1.M43 * value2;
			m.M44 = value1.M44 * value2;
			return m;
		}

		/// <summary>
		/// Returns a boolean indicating whether the given two matrices are equal.
		/// </summary>
		/// <param name="value1">The first matrix to compare.</param>
		/// <param name="value2">The second matrix to compare.</param>
		/// <returns>True if the given matrices are equal; False otherwise.</returns>
		public static bool operator ==(Matrix4x4 value1, Matrix4x4 value2)
		{
			return (value1.M11 == value2.M11 && value1.M22 == value2.M22 && value1.M33 == value2.M33 && value1.M44 == value2.M44 && // Check diagonal element first for early out.
												value1.M12 == value2.M12 && value1.M13 == value2.M13 && value1.M14 == value2.M14 &&
					value1.M21 == value2.M21 && value1.M23 == value2.M23 && value1.M24 == value2.M24 &&
					value1.M31 == value2.M31 && value1.M32 == value2.M32 && value1.M34 == value2.M34 &&
					value1.M41 == value2.M41 && value1.M42 == value2.M42 && value1.M43 == value2.M43);
		}

		/// <summary>
		/// Returns a boolean indicating whether the given two matrices are not equal.
		/// </summary>
		/// <param name="value1">The first matrix to compare.</param>
		/// <param name="value2">The second matrix to compare.</param>
		/// <returns>True if the given matrices are not equal; False if they are equal.</returns>
		public static bool operator !=(Matrix4x4 value1, Matrix4x4 value2)
		{
			return (value1.M11 != value2.M11 || value1.M12 != value2.M12 || value1.M13 != value2.M13 || value1.M14 != value2.M14 ||
					value1.M21 != value2.M21 || value1.M22 != value2.M22 || value1.M23 != value2.M23 || value1.M24 != value2.M24 ||
					value1.M31 != value2.M31 || value1.M32 != value2.M32 || value1.M33 != value2.M33 || value1.M34 != value2.M34 ||
					value1.M41 != value2.M41 || value1.M42 != value2.M42 || value1.M43 != value2.M43 || value1.M44 != value2.M44);
		}

		/// <summary>
		/// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
		/// </summary>
		/// <param name="other">The matrix to compare this instance to.</param>
		/// <returns>True if the matrices are equal; False otherwise.</returns>
		public bool Equals(Matrix4x4 other)
		{
			return (M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && M44 == other.M44 && // Check diagonal element first for early out.
										M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
					M21 == other.M21 && M23 == other.M23 && M24 == other.M24 &&
					M31 == other.M31 && M32 == other.M32 && M34 == other.M34 &&
					M41 == other.M41 && M42 == other.M42 && M43 == other.M43);
		}

		/// <summary>
		/// Returns a boolean indicating whether the given Object is equal to this matrix instance.
		/// </summary>
		/// <param name="obj">The Object to compare against.</param>
		/// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Matrix4x4)
			{
				return Equals((Matrix4x4)obj);
			}

			return false;
		}

		/// <summary>
		/// Returns a String representing this matrix instance.
		/// </summary>
		/// <returns>The string representation.</returns>
		public override string ToString()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;

			return String.Format(ci, "{{ {{M11:{0} M12:{1} M13:{2} M14:{3}}} {{M21:{4} M22:{5} M23:{6} M24:{7}}} {{M31:{8} M32:{9} M33:{10} M34:{11}}} {{M41:{12} M42:{13} M43:{14} M44:{15}}} }}",
								 M11.ToString(ci), M12.ToString(ci), M13.ToString(ci), M14.ToString(ci),
								 M21.ToString(ci), M22.ToString(ci), M23.ToString(ci), M24.ToString(ci),
								 M31.ToString(ci), M32.ToString(ci), M33.ToString(ci), M34.ToString(ci),
								 M41.ToString(ci), M42.ToString(ci), M43.ToString(ci), M44.ToString(ci));
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
				   M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
				   M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
				   M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
		}

		public static float ConvertDegToRad(float degrees)
		{
			return ((float)Math.PI / (float)180) * degrees;
		}

		public static Matrix4x4 GetTranslationMatrix(Vector3 position)
		{
			return new Matrix4x4(1, 0, 0, 0,
								 0, 1, 0, 0,
								 0, 0, 1, 0,
								 position.x, position.y, position.z, 1);
		}

		public static Matrix4x4 CreateFromQuaternion(Quaternion quaternion)
		{
			Matrix4x4 result;

			float xx = quaternion.x * quaternion.x;
			float yy = quaternion.y * quaternion.y;
			float zz = quaternion.z * quaternion.z;

			float xy = quaternion.x * quaternion.y;
			float wz = quaternion.z * quaternion.w;
			float xz = quaternion.z * quaternion.x;
			float wy = quaternion.y * quaternion.w;
			float yz = quaternion.y * quaternion.z;
			float wx = quaternion.x * quaternion.w;

			result.M11 = 1.0f - 2.0f * (yy + zz);
			result.M12 = 2.0f * (xy + wz);
			result.M13 = 2.0f * (xz - wy);
			result.M14 = 0.0f;
			result.M21 = 2.0f * (xy - wz);
			result.M22 = 1.0f - 2.0f * (zz + xx);
			result.M23 = 2.0f * (yz + wx);
			result.M24 = 0.0f;
			result.M31 = 2.0f * (xz + wy);
			result.M32 = 2.0f * (yz - wx);
			result.M33 = 1.0f - 2.0f * (yy + xx);
			result.M34 = 0.0f;
			result.M41 = 0.0f;
			result.M42 = 0.0f;
			result.M43 = 0.0f;
			result.M44 = 1.0f;

			return result;
		}

		public static Matrix4x4 TRS(Vector3 pos, Quaternion q, Vector3 s)
		{
			//return Transform(CreateTranslation(pos), q);
			//return GetTranslationMatrix(position) * CreateFromQuaternion(rotationAngles) * GetScaleMatrix(scale);
			Matrix4x4 result = new Matrix4x4();
			// Rotation and Scale
			double sqw = q.w * q.w;
			double sqx = q.x * q.x;
			double sqy = q.y * q.y;
			double sqz = q.z * q.z;
			result.M11 = (float)(1 - 2 * sqy - 2 * sqz) * (float)s.x;
			result.M12 = (float)(2 * q.x * q.y - 2 * q.z * q.w);
			result.M13 = (float)(2 * q.x * q.z + 2 * q.y * q.w);
			result.M21 = (float)(2 * q.x * q.y + 2 * q.z * q.w);
			result.M22 = (float)(1 - 2 * sqx - 2 * sqz) * (float)s.y;
			result.M23 = (float)(2 * q.y * q.z - 2 * q.x * q.w);
			result.M31 = (float)(2 * q.x * q.z - 2 * q.y * q.w);
			result.M32 = (float)(2 * q.y * q.z + 2 * q.x * q.w);
			result.M33 = (float)(1 - 2 * sqx - 2 * sqy) * (float)s.z;
			// Translation
			result.M14 = (float)pos.x;
			result.M24 = (float)pos.y;
			result.M34 = (float)pos.z;
			result.M44 = 1.0f;
			// Return result
			return result;
		}

		public Vector3 MultiplyPoint3x4(Vector3 v)
		{
			Vector3 vector3;
			vector3.x = (M11 * v.x + M12 * v.y + M13 * v.z) + M14;
			vector3.y = (M21 * v.x + M22 * v.y + M23 * v.z) + M24;
			vector3.z = (M31 * v.x + M32 * v.y + M33 * v.z) + M34;
			return vector3;
		}

		public Vector3 MultiplyVector(Vector3 v)
		{
			Vector3 vector3;
			vector3.x = (float)((double)this.M11 * (double)v.x + (double)this.M12 * (double)v.y + (double)this.M13 * (double)v.z);
			vector3.y = (float)((double)this.M21 * (double)v.x + (double)this.M22 * (double)v.y + (double)this.M23 * (double)v.z);
			vector3.z = (float)((double)this.M31 * (double)v.x + (double)this.M32 * (double)v.y + (double)this.M33 * (double)v.z);
			return vector3;
		}

		public Vector3 MultiplyPoint(Vector3 v)
		{
			Vector3 vector3;
			vector3.x = (float)((double)this.M11 * (double)v.x + (double)this.M12 * (double)v.y + (double)this.M13 * (double)v.z) + this.M14;
			vector3.y = (float)((double)this.M21 * (double)v.x + (double)this.M22 * (double)v.y + (double)this.M23 * (double)v.z) + this.M24;
			vector3.z = (float)((double)this.M31 * (double)v.x + (double)this.M32 * (double)v.y + (double)this.M33 * (double)v.z) + this.M34;
			float num = 1f / ((float)((double)this.M41 * (double)v.x + (double)this.M42 * (double)v.y + (double)this.M43 * (double)v.z) + this.M44);
			vector3.x *= num;
			vector3.y *= num;
			vector3.z *= num;
			return vector3;
		}

		/// <summary>
		///   <para>Returns a row of the matrix.</para>
		/// </summary>
		/// <param name="i"></param>
		public Vector4 GetRow(int i)
		{
			return new Vector4(this[i, 0], this[i, 1], this[i, 2], this[i, 3]);
		}

		public Vector4 GetColumn(int i)
		{
			return new Vector4(this[0, i], this[1, i], this[2, i], this[3, i]);
		}


		public float this[int row, int column]
		{
			get
			{
				return this[row + column * 4];
			}
			//set
			//{
			//	this[row + column * 4] = value;
			//}
		}

		public float this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return M11;
					case 1:
						return M21;
					case 2:
						return M31;
					case 3:
						return M41;
					case 4:
						return this.M12;
					case 5:
						return this.M22;
					case 6:
						return this.M32;
					case 7:
						return this.M42;
					case 8:
						return this.M13;
					case 9:
						return this.M23;
					case 10:
						return this.M33;
					case 11:
						return this.M43;
					case 12:
						return this.M14;
					case 13:
						return this.M24;
					case 14:
						return this.M34;
					case 15:
						return this.M44;
					default:
						throw new IndexOutOfRangeException("Invalid matrix index!");
				}
			}
			//set
			//{
			//	switch (index)
			//	{
			//		case 0:
			//			this.m00 = value;
			//			break;
			//		case 1:
			//			this.m10 = value;
			//			break;
			//		case 2:
			//			this.m20 = value;
			//			break;
			//		case 3:
			//			this.m30 = value;
			//			break;
			//		case 4:
			//			this.m01 = value;
			//			break;
			//		case 5:
			//			this.m11 = value;
			//			break;
			//		case 6:
			//			this.m21 = value;
			//			break;
			//		case 7:
			//			this.m31 = value;
			//			break;
			//		case 8:
			//			this.m02 = value;
			//			break;
			//		case 9:
			//			this.m12 = value;
			//			break;
			//		case 10:
			//			this.m22 = value;
			//			break;
			//		case 11:
			//			this.m32 = value;
			//			break;
			//		case 12:
			//			this.m03 = value;
			//			break;
			//		case 13:
			//			this.m13 = value;
			//			break;
			//		case 14:
			//			this.m23 = value;
			//			break;
			//		case 15:
			//			this.m33 = value;
			//			break;
			//		default:
			//			throw new IndexOutOfRangeException("Invalid matrix index!");
			//	}
			//}
		}

	}
}