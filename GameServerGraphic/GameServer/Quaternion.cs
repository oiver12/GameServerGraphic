// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Globalization;
using System;

namespace GameServer
{
	/// <summary>
	/// A structure encapsulating a four-dimensional vector (x,y,z,w), 
	/// which is used to efficiently rotate an object about the (x,y,z) vector by the angle theta, where w = cos(theta/2).
	/// </summary>
	public struct Quaternion
	{
		/// <summary>
		/// Specifies the X-value of the vector component of the Quaternion.
		/// </summary>
		public float x;
		/// <summary>
		/// Specifies the Y-value of the vector component of the Quaternion.
		/// </summary>
		public float y;
		/// <summary>
		/// Specifies the Z-value of the vector component of the Quaternion.
		/// </summary>
		public float z;
		/// <summary>
		/// Specifies the rotation component of the Quaternion.
		/// </summary>
		public float w;

		/// <summary>
		/// Returns a Quaternion representing no rotation. 
		/// </summary>
		public static Quaternion Identity
		{
			get { return new Quaternion(0, 0, 0, 1); }
		}

		/// <summary>
		/// Returns whether the Quaternion is the identity Quaternion.
		/// </summary>
		public bool IsIdentity
		{
			get { return x == 0f && y == 0f && z == 0f && w == 1f; }
		}

		/// <summary>
		/// Constructs a Quaternion from the given components.
		/// </summary>
		/// <param name="x">The X component of the Quaternion.</param>
		/// <param name="y">The Y component of the Quaternion.</param>
		/// <param name="z">The Z component of the Quaternion.</param>
		/// <param name="w">The W component of the Quaternion.</param>
		public Quaternion(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		/// <summary>
		/// Constructs a Quaternion from the given vector and rotation parts.
		/// </summary>
		/// <param name="vectorPart">The vector part of the Quaternion.</param>
		/// <param name="scalarPart">The rotation part of the Quaternion.</param>
		public Quaternion(Vector3 vectorPart, float scalarPart)
		{
			x = vectorPart.x;
			y = vectorPart.y;
			z = vectorPart.z;
			w = scalarPart;
		}

		/// <summary>
		/// Calculates the length of the Quaternion.
		/// </summary>
		/// <returns>The computed length of the Quaternion.</returns>
		public float Length()
		{
			float ls = x * x + y * y + z * z + w * w;

			return (float)Math.Sqrt((double)ls);
		}

		/// <summary>
		/// Calculates the length squared of the Quaternion. This operation is cheaper than Length().
		/// </summary>
		/// <returns>The length squared of the Quaternion.</returns>
		public float LengthSquared()
		{
			return x * x + y * y + z * z + w * w;
		}

		/// <summary>
		/// Divides each component of the Quaternion by the length of the Quaternion.
		/// </summary>
		/// <param name="value">The source Quaternion.</param>
		/// <returns>The normalized Quaternion.</returns>
		public static Quaternion Normalize(Quaternion value)
		{
			Quaternion ans;

			float ls = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;

			float invNorm = 1.0f / (float)Math.Sqrt((double)ls);

			ans.x = value.x * invNorm;
			ans.y = value.y * invNorm;
			ans.z = value.z * invNorm;
			ans.w = value.w * invNorm;

			return ans;
		}

		/// <summary>
		/// Creates the conjugate of a specified Quaternion.
		/// </summary>
		/// <param name="value">The Quaternion of which to return the conjugate.</param>
		/// <returns>A new Quaternion that is the conjugate of the specified one.</returns>
		public static Quaternion Conjugate(Quaternion value)
		{
			Quaternion ans;

			ans.x = -value.x;
			ans.y = -value.y;
			ans.z = -value.z;
			ans.w = value.w;

			return ans;
		}

		/// <summary>
		/// Returns the inverse of a Quaternion.
		/// </summary>
		/// <param name="value">The source Quaternion.</param>
		/// <returns>The inverted Quaternion.</returns>
		public static Quaternion Inverse(Quaternion value)
		{
			//  -1   (       a              -v       )
			// q   = ( -------------   ------------- )
			//       (  a^2 + |v|^2  ,  a^2 + |v|^2  )

			Quaternion ans;

			float ls = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
			float invNorm = 1.0f / ls;

			ans.x = -value.x * invNorm;
			ans.y = -value.y * invNorm;
			ans.z = -value.z * invNorm;
			ans.w = value.w * invNorm;

			return ans;
		}

		/// <summary>
		/// Creates a Quaternion from a vector and an angle to rotate about the vector.
		/// </summary>
		/// <param name="axis">The vector to rotate around.</param>
		/// <param name="angle">The angle, in radians, to rotate around the vector.</param>
		/// <returns>The created Quaternion.</returns>
		public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
		{
			Quaternion ans;

			float halfAngle = angle * 0.5f;
			float s = (float)Math.Sin(halfAngle);
			float c = (float)Math.Cos(halfAngle);

			ans.x = axis.x * s;
			ans.y = axis.y* s;
			ans.z = axis.z * s;
			ans.w = c;

			return ans;
		}

		/// <summary>
		/// Creates a new Quaternion from the given yaw, pitch, and roll, in radians.
		/// </summary>
		/// <param name="yaw">The yaw angle, in radians, around the Y-axis.</param>
		/// <param name="pitch">The pitch angle, in radians, around the X-axis.</param>
		/// <param name="roll">The roll angle, in radians, around the Z-axis.</param>
		/// <returns></returns>
		public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
		{
			//  Roll first, about axis the object is facing, then
			//  pitch upward, then yaw to face into the new heading
			float sr, cr, sp, cp, sy, cy;

			float halfRoll = roll * 0.5f;
			sr = (float)Math.Sin(halfRoll);
			cr = (float)Math.Cos(halfRoll);

			float halfPitch = pitch * 0.5f;
			sp = (float)Math.Sin(halfPitch);
			cp = (float)Math.Cos(halfPitch);

			float halfYaw = yaw * 0.5f;
			sy = (float)Math.Sin(halfYaw);
			cy = (float)Math.Cos(halfYaw);

			Quaternion result;

			result.x = cy * sp * cr + sy * cp * sr;
			result.y = sy * cp * cr - cy * sp * sr;
			result.z = cy * cp * sr - sy * sp * cr;
			result.w = cy * cp * cr + sy * sp * sr;

			return result;
		}

		/// <summary>
		/// Creates a Quaternion from the given rotation matrix.
		/// </summary>
		/// <param name="matrix">The rotation matrix.</param>
		/// <returns>The created Quaternion.</returns>
		public static Quaternion CreateFromRotationMatrix(Matrix4x4 matrix)
		{
			float trace = matrix.M11 + matrix.M22 + matrix.M33;

			Quaternion q = new Quaternion();

			if (trace > 0.0f)
			{
				float s = (float)Math.Sqrt(trace + 1.0f);
				q.w = s * 0.5f;
				s = 0.5f / s;
				q.x = (matrix.M23 - matrix.M32) * s;
				q.y = (matrix.M31 - matrix.M13) * s;
				q.z = (matrix.M12 - matrix.M21) * s;
			}
			else
			{
				if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
				{
					float s = (float)Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
					float invS = 0.5f / s;
					q.x = 0.5f * s;
					q.y = (matrix.M12 + matrix.M21) * invS;
					q.z = (matrix.M13 + matrix.M31) * invS;
					q.w = (matrix.M23 - matrix.M32) * invS;
				}
				else if (matrix.M22 > matrix.M33)
				{
					float s = (float)Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
					float invS = 0.5f / s;
					q.x = (matrix.M21 + matrix.M12) * invS;
					q.y = 0.5f * s;
					q.z = (matrix.M32 + matrix.M23) * invS;
					q.w = (matrix.M31 - matrix.M13) * invS;
				}
				else
				{
					float s = (float)Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
					float invS = 0.5f / s;
					q.x = (matrix.M31 + matrix.M13) * invS;
					q.y = (matrix.M32 + matrix.M23) * invS;
					q.z = 0.5f * s;
					q.w = (matrix.M12 - matrix.M21) * invS;
				}
			}

			return q;
		}

		/// <summary>
		/// Calculates the dot product of two Quaternions.
		/// </summary>
		/// <param name="quaternion1">The first source Quaternion.</param>
		/// <param name="quaternion2">The second source Quaternion.</param>
		/// <returns>The dot product of the Quaternions.</returns>
		public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
		{
			return quaternion1.x * quaternion2.x +
				   quaternion1.y * quaternion2.y +
				   quaternion1.z * quaternion2.z +
				   quaternion1.w * quaternion2.w;
		}

		/// <summary>
		/// Interpolates between two quaternions, using spherical linear interpolation.
		/// </summary>
		/// <param name="quaternion1">The first source Quaternion.</param>
		/// <param name="quaternion2">The second source Quaternion.</param>
		/// <param name="amount">The relative weight of the second source Quaternion in the interpolation.</param>
		/// <returns>The interpolated Quaternion.</returns>
		public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
		{
			const float epsilon = 1e-6f;

			float t = amount;

			float cosOmega = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y +
							 quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;

			bool flip = false;

			if (cosOmega < 0.0f)
			{
				flip = true;
				cosOmega = -cosOmega;
			}

			float s1, s2;

			if (cosOmega > (1.0f - epsilon))
			{
				// Too close, do straight linear interpolation.
				s1 = 1.0f - t;
				s2 = (flip) ? -t : t;
			}
			else
			{
				float omega = (float)Math.Acos(cosOmega);
				float invSinOmega = (float)(1 / Math.Sin(omega));

				s1 = (float)Math.Sin((1.0f - t) * omega) * invSinOmega;
				s2 = (flip)
					? (float)-Math.Sin(t * omega) * invSinOmega
					: (float)Math.Sin(t * omega) * invSinOmega;
			}

			Quaternion ans;

			ans.x = s1 * quaternion1.x + s2 * quaternion2.x;
			ans.y = s1 * quaternion1.y + s2 * quaternion2.y;
			ans.z = s1 * quaternion1.z + s2 * quaternion2.z;
			ans.w = s1 * quaternion1.w + s2 * quaternion2.w;

			return ans;
		}

		/// <summary>
		///  Linearly interpolates between two quaternions.
		/// </summary>
		/// <param name="quaternion1">The first source Quaternion.</param>
		/// <param name="quaternion2">The second source Quaternion.</param>
		/// <param name="amount">The relative weight of the second source Quaternion in the interpolation.</param>
		/// <returns>The interpolated Quaternion.</returns>
		public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
		{
			float t = amount;
			float t1 = 1.0f - t;

			Quaternion r = new Quaternion();

			float dot = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y +
						quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;

			if (dot >= 0.0f)
			{
				r.x = t1 * quaternion1.x + t * quaternion2.x;
				r.y = t1 * quaternion1.y + t * quaternion2.y;
				r.z = t1 * quaternion1.z + t * quaternion2.z;
				r.w = t1 * quaternion1.w + t * quaternion2.w;
			}
			else
			{
				r.x = t1 * quaternion1.x - t * quaternion2.x;
				r.y = t1 * quaternion1.y - t * quaternion2.y;
				r.z = t1 * quaternion1.z - t * quaternion2.z;
				r.w = t1 * quaternion1.w - t * quaternion2.w;
			}

			// Normalize it.
			float ls = r.x * r.x + r.y * r.y + r.z * r.z + r.w * r.w;
			float invNorm = 1.0f / (float)Math.Sqrt((double)ls);

			r.x *= invNorm;
			r.y *= invNorm;
			r.z *= invNorm;
			r.w *= invNorm;

			return r;
		}

		/// <summary>
		/// Concatenates two Quaternions; the result represents the value1 rotation followed by the value2 rotation.
		/// </summary>
		/// <param name="value1">The first Quaternion rotation in the series.</param>
		/// <param name="value2">The second Quaternion rotation in the series.</param>
		/// <returns>A new Quaternion representing the concatenation of the value1 rotation followed by the value2 rotation.</returns>
		public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			// Concatenate rotation is actually q2 * q1 instead of q1 * q2.
			// So that's why value2 goes q1 and value1 goes q2.
			float q1x = value2.x;
			float q1y = value2.y;
			float q1z = value2.z;
			float q1w = value2.w;

			float q2x = value1.x;
			float q2y = value1.y;
			float q2z = value1.z;
			float q2w = value1.w;

			// cross(av, bv)
			float cx = q1y * q2z - q1z * q2y;
			float cy = q1z * q2x - q1x * q2z;
			float cz = q1x * q2y - q1y * q2x;

			float dot = q1x * q2x + q1y * q2y + q1z * q2z;

			ans.x = q1x * q2w + q2x * q1w + cx;
			ans.y = q1y * q2w + q2y * q1w + cy;
			ans.z = q1z * q2w + q2z * q1w + cz;
			ans.w = q1w * q2w - dot;

			return ans;
		}

		/// <summary>
		/// Flips the sign of each component of the quaternion.
		/// </summary>
		/// <param name="value">The source Quaternion.</param>
		/// <returns>The negated Quaternion.</returns>
		public static Quaternion Negate(Quaternion value)
		{
			Quaternion ans;

			ans.x = -value.x;
			ans.y = -value.y;
			ans.z = -value.z;
			ans.w = -value.w;

			return ans;
		}

		/// <summary>
		/// Adds two Quaternions element-by-element.
		/// </summary>
		/// <param name="value1">The first source Quaternion.</param>
		/// <param name="value2">The second source Quaternion.</param>
		/// <returns>The result of adding the Quaternions.</returns>
		public static Quaternion Add(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			ans.x = value1.x + value2.x;
			ans.y = value1.y + value2.y;
			ans.z = value1.z + value2.z;
			ans.w = value1.w + value2.w;

			return ans;
		}

		/// <summary>
		/// Subtracts one Quaternion from another.
		/// </summary>
		/// <param name="value1">The first source Quaternion.</param>
		/// <param name="value2">The second Quaternion, to be subtracted from the first.</param>
		/// <returns>The result of the subtraction.</returns>
		public static Quaternion Subtract(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			ans.x = value1.x - value2.x;
			ans.y = value1.y - value2.y;
			ans.z = value1.z - value2.z;
			ans.w = value1.w - value2.w;

			return ans;
		}

		/// <summary>
		/// Multiplies two Quaternions together.
		/// </summary>
		/// <param name="value1">The Quaternion on the left side of the multiplication.</param>
		/// <param name="value2">The Quaternion on the right side of the multiplication.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Quaternion Multiply(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			float q1x = value1.x;
			float q1y = value1.y;
			float q1z = value1.z;
			float q1w = value1.w;

			float q2x = value2.x;
			float q2y = value2.y;
			float q2z = value2.z;
			float q2w = value2.w;

			// cross(av, bv)
			float cx = q1y * q2z - q1z * q2y;
			float cy = q1z * q2x - q1x * q2z;
			float cz = q1x * q2y - q1y * q2x;

			float dot = q1x * q2x + q1y * q2y + q1z * q2z;

			ans.x = q1x * q2w + q2x * q1w + cx;
			ans.y = q1y * q2w + q2y * q1w + cy;
			ans.z = q1z * q2w + q2z * q1w + cz;
			ans.w = q1w * q2w - dot;

			return ans;
		}

		/// <summary>
		/// Multiplies a Quaternion by a scalar value.
		/// </summary>
		/// <param name="value1">The source Quaternion.</param>
		/// <param name="value2">The scalar value.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Quaternion Multiply(Quaternion value1, float value2)
		{
			Quaternion ans;

			ans.x = value1.x * value2;
			ans.y = value1.y * value2;
			ans.z = value1.z * value2;
			ans.w = value1.w * value2;

			return ans;
		}

		/// <summary>
		/// Divides a Quaternion by another Quaternion.
		/// </summary>
		/// <param name="value1">The source Quaternion.</param>
		/// <param name="value2">The divisor.</param>
		/// <returns>The result of the division.</returns>
		public static Quaternion Divide(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			float q1x = value1.x;
			float q1y = value1.y;
			float q1z = value1.z;
			float q1w = value1.w;

			//-------------------------------------
			// Inverse part.
			float ls = value2.x * value2.x + value2.y * value2.y +
					   value2.z * value2.z + value2.w * value2.w;
			float invNorm = 1.0f / ls;

			float q2x = -value2.x * invNorm;
			float q2y = -value2.y * invNorm;
			float q2z = -value2.z * invNorm;
			float q2w = value2.w * invNorm;

			//-------------------------------------
			// Multiply part.

			// cross(av, bv)
			float cx = q1y * q2z - q1z * q2y;
			float cy = q1z * q2x - q1x * q2z;
			float cz = q1x * q2y - q1y * q2x;

			float dot = q1x * q2x + q1y * q2y + q1z * q2z;

			ans.x = q1x * q2w + q2x * q1w + cx;
			ans.y = q1y * q2w + q2y * q1w + cy;
			ans.z = q1z * q2w + q2z * q1w + cz;
			ans.w = q1w * q2w - dot;

			return ans;
		}

		/// <summary>
		/// Flips the sign of each component of the quaternion.
		/// </summary>
		/// <param name="value">The source Quaternion.</param>
		/// <returns>The negated Quaternion.</returns>
		public static Quaternion operator -(Quaternion value)
		{
			Quaternion ans;

			ans.x = -value.x;
			ans.y = -value.y;
			ans.z = -value.z;
			ans.w = -value.w;

			return ans;
		}

		/// <summary>
		/// Adds two Quaternions element-by-element.
		/// </summary>
		/// <param name="value1">The first source Quaternion.</param>
		/// <param name="value2">The second source Quaternion.</param>
		/// <returns>The result of adding the Quaternions.</returns>
		public static Quaternion operator +(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			ans.x = value1.x + value2.x;
			ans.y = value1.y + value2.y;
			ans.z = value1.z + value2.z;
			ans.w = value1.w + value2.w;

			return ans;
		}

		/// <summary>
		/// Subtracts one Quaternion from another.
		/// </summary>
		/// <param name="value1">The first source Quaternion.</param>
		/// <param name="value2">The second Quaternion, to be subtracted from the first.</param>
		/// <returns>The result of the subtraction.</returns>
		public static Quaternion operator -(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			ans.x = value1.x - value2.x;
			ans.y = value1.y - value2.y;
			ans.z = value1.z - value2.z;
			ans.w = value1.w - value2.w;

			return ans;
		}

		/// <summary>
		/// Multiplies two Quaternions together.
		/// </summary>
		/// <param name="value1">The Quaternion on the left side of the multiplication.</param>
		/// <param name="value2">The Quaternion on the right side of the multiplication.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Quaternion operator *(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			float q1x = value1.x;
			float q1y = value1.y;
			float q1z = value1.z;
			float q1w = value1.w;

			float q2x = value2.x;
			float q2y = value2.y;
			float q2z = value2.z;
			float q2w = value2.w;

			// cross(av, bv)
			float cx = q1y * q2z - q1z * q2y;
			float cy = q1z * q2x - q1x * q2z;
			float cz = q1x * q2y - q1y * q2x;

			float dot = q1x * q2x + q1y * q2y + q1z * q2z;

			ans.x = q1x * q2w + q2x * q1w + cx;
			ans.y = q1y * q2w + q2y * q1w + cy;
			ans.z = q1z * q2w + q2z * q1w + cz;
			ans.w = q1w * q2w - dot;

			return ans;
		}

		/// <summary>
		/// Multiplies a Quaternion by a scalar value.
		/// </summary>
		/// <param name="value1">The source Quaternion.</param>
		/// <param name="value2">The scalar value.</param>
		/// <returns>The result of the multiplication.</returns>
		public static Quaternion operator *(Quaternion value1, float value2)
		{
			Quaternion ans;

			ans.x = value1.x * value2;
			ans.y = value1.y * value2;
			ans.z = value1.z * value2;
			ans.w = value1.w * value2;

			return ans;
		}

		/// <summary>
		/// Divides a Quaternion by another Quaternion.
		/// </summary>
		/// <param name="value1">The source Quaternion.</param>
		/// <param name="value2">The divisor.</param>
		/// <returns>The result of the division.</returns>
		public static Quaternion operator /(Quaternion value1, Quaternion value2)
		{
			Quaternion ans;

			float q1x = value1.x;
			float q1y = value1.y;
			float q1z = value1.z;
			float q1w = value1.w;

			//-------------------------------------
			// Inverse part.
			float ls = value2.x * value2.x + value2.y * value2.y +
					   value2.z * value2.z + value2.w * value2.w;
			float invNorm = 1.0f / ls;

			float q2x = -value2.x * invNorm;
			float q2y = -value2.y * invNorm;
			float q2z = -value2.z * invNorm;
			float q2w = value2.w * invNorm;

			//-------------------------------------
			// Multiply part.

			// cross(av, bv)
			float cx = q1y * q2z - q1z * q2y;
			float cy = q1z * q2x - q1x * q2z;
			float cz = q1x * q2y - q1y * q2x;

			float dot = q1x * q2x + q1y * q2y + q1z * q2z;

			ans.x = q1x * q2w + q2x * q1w + cx;
			ans.y = q1y * q2w + q2y * q1w + cy;
			ans.z = q1z * q2w + q2z * q1w + cz;
			ans.w = q1w * q2w - dot;

			return ans;
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given Quaternions are equal.
		/// </summary>
		/// <param name="value1">The first Quaternion to compare.</param>
		/// <param name="value2">The second Quaternion to compare.</param>
		/// <returns>True if the Quaternions are equal; False otherwise.</returns>
		public static bool operator ==(Quaternion value1, Quaternion value2)
		{
			return (value1.x == value2.x &&
					value1.y == value2.y &&
					value1.z == value2.z &&
					value1.w == value2.w);
		}

		/// <summary>
		/// Returns a boolean indicating whether the two given Quaternions are not equal.
		/// </summary>
		/// <param name="value1">The first Quaternion to compare.</param>
		/// <param name="value2">The second Quaternion to compare.</param>
		/// <returns>True if the Quaternions are not equal; False if they are equal.</returns>
		public static bool operator !=(Quaternion value1, Quaternion value2)
		{
			return (value1.x != value2.x ||
					value1.y != value2.y ||
					value1.z != value2.z ||
					value1.w != value2.w);
		}

		/// <summary>
		/// Returns a boolean indicating whether the given Quaternion is equal to this Quaternion instance.
		/// </summary>
		/// <param name="other">The Quaternion to compare this instance to.</param>
		/// <returns>True if the other Quaternion is equal to this instance; False otherwise.</returns>
		public bool Equals(Quaternion other)
		{
			return (x == other.x &&
					y == other.y &&
					z == other.z &&
					w == other.w);
		}

		/// <summary>
		/// Returns a boolean indicating whether the given Object is equal to this Quaternion instance.
		/// </summary>
		/// <param name="obj">The Object to compare against.</param>
		/// <returns>True if the Object is equal to this Quaternion; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Quaternion)
			{
				return Equals((Quaternion)obj);
			}

			return false;
		}

		/// <summary>
		/// Returns a String representing this Quaternion instance.
		/// </summary>
		/// <returns>The string representation.</returns>
		public override string ToString()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;

			return String.Format(ci, "{{X:{0} Y:{1} Z:{2} W:{3}}}", x.ToString(ci), y.ToString(ci), z.ToString(ci), w.ToString(ci));
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>The hash code.</returns>
		public override int GetHashCode()
		{
			return x.GetHashCode() + y.GetHashCode() + z.GetHashCode() + w.GetHashCode();
		}

		public static Vector3 operator *(Quaternion rotation, Vector3 point)
		{
			float num1 = rotation.x * 2f;
			float num2 = rotation.y * 2f;
			float num3 = rotation.z * 2f;
			float num4 = rotation.x * num1;
			float num5 = rotation.y * num2;
			float num6 = rotation.z * num3;
			float num7 = rotation.x * num2;
			float num8 = rotation.x * num3;
			float num9 = rotation.y * num3;
			float num10 = rotation.w * num1;
			float num11 = rotation.w * num2;
			float num12 = rotation.w * num3;
			Vector3 vector3;
			vector3.x = (float)((1.0 - ((double)num5 + (double)num6)) * (double)point.x + ((double)num7 - (double)num12) * (double)point.y + ((double)num8 + (double)num11) * (double)point.z);
			vector3.y = (float)(((double)num7 + (double)num12) * (double)point.x + (1.0 - ((double)num4 + (double)num6)) * (double)point.y + ((double)num9 - (double)num10) * (double)point.z);
			vector3.z = (float)(((double)num8 - (double)num11) * (double)point.x + ((double)num9 + (double)num10) * (double)point.y + (1.0 - ((double)num4 + (double)num5)) * (double)point.z);
			return vector3;
		}

		public static Quaternion Euler(Vector3 v)
		{
			return Euler(v.y, v.x, v.z);
		}

		//public static Quaternion Euler(float yaw, float pitch, float roll)
		//{
		//	yaw *= Mathf.Deg2Rad;
		//	pitch *= Mathf.Deg2Rad;
		//	roll *= Mathf.Deg2Rad;
		//	float rollOver2 = roll * 0.5f;
		//	float sinRollOver2 = (float)Math.Sin((double)rollOver2);
		//	float cosRollOver2 = (float)Math.Cos((double)rollOver2);
		//	float pitchOver2 = pitch * 0.5f;
		//	float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
		//	float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
		//	float yawOver2 = yaw * 0.5f;
		//	float sinYawOver2 = (float)Math.Sin((double)yawOver2);
		//	float cosYawOver2 = (float)Math.Cos((double)yawOver2);
		//	Quaternion result;
		//	result.w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
		//	result.x = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
		//	result.y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
		//	result.z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

		//	return result;
		//}

		public static Quaternion Euler(double yaw, double pitch, double roll) // yaw (Z), pitch (Y), roll (X)
		{
			yaw *= Mathf.Deg2Rad;
			pitch *= Mathf.Deg2Rad;
			roll *= Mathf.Deg2Rad;
			// Abbreviations for the various angular functions
			double cy = Math.Cos(yaw * 0.5);
			double sy = Math.Sin(yaw * 0.5);
			double cp = Math.Cos(pitch * 0.5);
			double sp = Math.Sin(pitch * 0.5);
			double cr = Math.Cos(roll * 0.5);
			double sr = Math.Sin(roll * 0.5);

			Quaternion q;
			q.w = (float)(cr * cp * cy + sr * sp * sy);
			q.x = (float)(sr * cp * cy - cr * sp * sy);
			q.y = (float)(cr * sp * cy + sr * cp * sy);
			q.z = (float)(cr * cp * sy - sr * sp * cy);

			return q;
		}

		public static Vector3 ToEulerAngles(Quaternion q)
		{
			Vector3 angles;

			// roll (x-axis rotation)
			double sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
			double cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
			angles.x = (float)Math.Atan2(sinr_cosp, cosr_cosp);

			// pitch (y-axis rotation)
			double sinp = 2 * (q.w * q.y - q.z * q.x);
			if (Math.Abs(sinp) >= 1)
				angles.y = (float)Mathf.copysign(Math.PI / 2, sinp); // use 90 degrees if out of range
			else
				angles.y = (float)Math.Asin(sinp);

			// yaw (z-axis rotation)
			double siny_cosp = 2 * (q.w * q.z + q.x * q.y);
			double cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
			angles.z = (float)Math.Atan2(siny_cosp, cosy_cosp);
			angles.x *= Mathf.Rad2Deg;
			angles.y *= Mathf.Rad2Deg;
			angles.z *= Mathf.Rad2Deg;
			return angles;
		}

		static Vector3 NormalizeAngles(Vector3 angles)
		{
			angles.x = NormalizeAngle(angles.x);
			angles.y = NormalizeAngle(angles.y);
			angles.z = NormalizeAngle(angles.z);
			return angles;
		}

		static float NormalizeAngle(float angle)
		{
			while (angle > 360)
				angle -= 360;
			while (angle < 0)
				angle += 360;
			return angle;
		}

		/// <summary>
		/// Evaluates a rotation needed to be applied to an object positioned at sourcePoint to face destPoint
		/// </summary>
		/// <param name="sourcePoint">Coordinates of source point</param>
		/// <param name="destPoint">Coordinates of destionation point</param>
		/// <returns></returns>
		public static Quaternion LookRotation(Vector3 forwardVector, Vector3 upwardVector)
		{
			forwardVector = Vector3.Normalize(forwardVector);

			float dot = Vector3.Dot(Vector3.forward, forwardVector);

			if (Math.Abs(dot - (-1.0f)) < 0.000001f)
			{
				return new Quaternion(Vector3.up.x, Vector3.up.y, Vector3.up.z, 3.1415926535897932f);
			}
			if (Math.Abs(dot - (1.0f)) < 0.000001f)
			{
				return Quaternion.Identity;
			}

			float rotAngle = (float)Math.Acos(dot);
			Vector3 rotAxis = Vector3.Cross(Vector3.forward, forwardVector);
			rotAxis = Vector3.Normalize(rotAxis);
			return CreateFromAxisAngle(rotAxis, rotAngle);
		}

		public static Quaternion FromToRotation(Vector3 from, Vector3 to)
		{
			Vector3 axis = Vector3.Cross(from, to).normalized;
			double phi = Math.Acos(Vector3.Dot(from, to) / (from.magnitude * to.magnitude));
			return new Quaternion(
				Mathf.Cos((float)phi / 2),
				Mathf.Sin((float)phi / 2) * axis.x,
				Mathf.Sin((float)phi / 2) * axis.y,
				Mathf.Sin((float)phi / 2) * axis.z);
		}

		public static float Angle(Quaternion from, Quaternion to)
		{
			float dot = Dot(from, to);
			if (dot >= 1f)
				return 0f;
			return (float)(Mathf.Rad2Deg *  2 * Math.Acos(Quaternion.Dot(from, to)));
		}

		/// <summary>
		///   <para>Rotates a rotation from towards to.</para>
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="maxDegreesDelta"></param>
		public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
		{
			float num = Quaternion.Angle(from, to);
			if ((double)num == 0.0)
				return to;
			float t = Mathf.Min(1f, maxDegreesDelta / num);
			Quaternion q = Quaternion.Slerp(from, to, t);
			return q;
		}
	}
}