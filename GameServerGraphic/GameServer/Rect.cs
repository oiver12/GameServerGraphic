﻿using System;

namespace GameServer
{
	/// <summary>
	///   <para>A 2D Rectangle defined by X and Y position, width and height.</para>
	/// </summary>
	public struct Rect
	{
		private float m_XMin;
		private float m_YMin;
		private float m_Width;
		private float m_Height;

		/// <summary>
		///   <para>The X coordinate of the rectangle.</para>
		/// </summary>
		public float x
		{
			get
			{
				return this.m_XMin;
			}
			set
			{
				this.m_XMin = value;
			}
		}

		/// <summary>
		///   <para>The Y coordinate of the rectangle.</para>
		/// </summary>
		public float y
		{
			get
			{
				return this.m_YMin;
			}
			set
			{
				this.m_YMin = value;
			}
		}

		/// <summary>
		///   <para>The X and Y position of the rectangle.</para>
		/// </summary>
		public Vector2 position
		{
			get
			{
				return new Vector2(this.m_XMin, this.m_YMin);
			}
			set
			{
				this.m_XMin = value.x;
				this.m_YMin = value.y;
			}
		}

		/// <summary>
		///   <para>The position of the center of the rectangle.</para>
		/// </summary>
		public Vector2 center
		{
			get
			{
				return new Vector2(this.x + this.m_Width / 2f, this.y + this.m_Height / 2f);
			}
			set
			{
				this.m_XMin = value.x - this.m_Width / 2f;
				this.m_YMin = value.y - this.m_Height / 2f;
			}
		}

		/// <summary>
		///   <para>The position of the minimum corner of the rectangle.</para>
		/// </summary>
		public Vector2 min
		{
			get
			{
				return new Vector2(this.xMin, this.yMin);
			}
			set
			{
				this.xMin = value.x;
				this.yMin = value.y;
			}
		}

		/// <summary>
		///   <para>The position of the maximum corner of the rectangle.</para>
		/// </summary>
		public Vector2 max
		{
			get
			{
				return new Vector2(this.xMax, this.yMax);
			}
			set
			{
				this.xMax = value.x;
				this.yMax = value.y;
			}
		}

		/// <summary>
		///   <para>The width of the rectangle, measured from the X position.</para>
		/// </summary>
		public float width
		{
			get
			{
				return this.m_Width;
			}
			set
			{
				this.m_Width = value;
			}
		}

		/// <summary>
		///   <para>The height of the rectangle, measured from the Y position.</para>
		/// </summary>
		public float height
		{
			get
			{
				return this.m_Height;
			}
			set
			{
				this.m_Height = value;
			}
		}

		/// <summary>
		///   <para>The width and height of the rectangle.</para>
		/// </summary>
		public Vector2 size
		{
			get
			{
				return new Vector2(this.m_Width, this.m_Height);
			}
			set
			{
				this.m_Width = value.x;
				this.m_Height = value.y;
			}
		}

		[Obsolete("use xMin")]
		public float left
		{
			get
			{
				return this.m_XMin;
			}
		}

		[Obsolete("use xMax")]
		public float right
		{
			get
			{
				return this.m_XMin + this.m_Width;
			}
		}

		[Obsolete("use yMin")]
		public float top
		{
			get
			{
				return this.m_YMin;
			}
		}

		[Obsolete("use yMax")]
		public float bottom
		{
			get
			{
				return this.m_YMin + this.m_Height;
			}
		}

		/// <summary>
		///   <para>The minimum X coordinate of the rectangle.</para>
		/// </summary>
		public float xMin
		{
			get
			{
				return this.m_XMin;
			}
			set
			{
				float xMax = this.xMax;
				this.m_XMin = value;
				this.m_Width = xMax - this.m_XMin;
			}
		}

		/// <summary>
		///   <para>The minimum Y coordinate of the rectangle.</para>
		/// </summary>
		public float yMin
		{
			get
			{
				return this.m_YMin;
			}
			set
			{
				float yMax = this.yMax;
				this.m_YMin = value;
				this.m_Height = yMax - this.m_YMin;
			}
		}

		/// <summary>
		///   <para>The maximum X coordinate of the rectangle.</para>
		/// </summary>
		public float xMax
		{
			get
			{
				return this.m_Width + this.m_XMin;
			}
			set
			{
				this.m_Width = value - this.m_XMin;
			}
		}

		/// <summary>
		///   <para>The maximum Y coordinate of the rectangle.</para>
		/// </summary>
		public float yMax
		{
			get
			{
				return this.m_Height + this.m_YMin;
			}
			set
			{
				this.m_Height = value - this.m_YMin;
			}
		}

		/// <summary>
		///   <para>Creates a new rectangle.</para>
		/// </summary>
		/// <param name="x">The X value the rect is measured from.</param>
		/// <param name="y">The Y value the rect is measured from.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		public Rect(float x, float y, float width, float height)
		{
			this.m_XMin = x;
			this.m_YMin = y;
			this.m_Width = width;
			this.m_Height = height;
		}

		/// <summary>
		///   <para>Creates a rectangle given a size and position.</para>
		/// </summary>
		/// <param name="position">The position of the minimum corner of the rect.</param>
		/// <param name="size">The width and height of the rect.</param>
		public Rect(Vector2 position, Vector2 size)
		{
			this.m_XMin = position.x;
			this.m_YMin = position.y;
			this.m_Width = size.x;
			this.m_Height = size.y;
		}

		/// <summary>
		///   <para></para>
		/// </summary>
		/// <param name="source"></param>
		public Rect(Rect source)
		{
			this.m_XMin = source.m_XMin;
			this.m_YMin = source.m_YMin;
			this.m_Width = source.m_Width;
			this.m_Height = source.m_Height;
		}

		public static bool operator !=(Rect lhs, Rect rhs)
		{
			if ((double)lhs.x == (double)rhs.x && (double)lhs.y == (double)rhs.y && (double)lhs.width == (double)rhs.width)
				return (double)lhs.height != (double)rhs.height;
			return true;
		}

		public static bool operator ==(Rect lhs, Rect rhs)
		{
			if ((double)lhs.x == (double)rhs.x && (double)lhs.y == (double)rhs.y && (double)lhs.width == (double)rhs.width)
				return (double)lhs.height == (double)rhs.height;
			return false;
		}

		/// <summary>
		///   <para>Creates a rectangle from min/max coordinate values.</para>
		/// </summary>
		/// <param name="xmin">The minimum X coordinate.</param>
		/// <param name="ymin">The minimum Y coordinate.</param>
		/// <param name="xmax">The maximum X coordinate.</param>
		/// <param name="ymax">The maximum Y coordinate.</param>
		/// <returns>
		///   <para>A rectangle matching the specified coordinates.</para>
		/// </returns>
		public static Rect MinMaxRect(float xmin, float ymin, float xmax, float ymax)
		{
			return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
		}

		/// <summary>
		///   <para>Set components of an existing Rect.</para>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public void Set(float x, float y, float width, float height)
		{
			this.m_XMin = x;
			this.m_YMin = y;
			this.m_Width = width;
			this.m_Height = height;
		}

		/// <summary>
		///   <para>Returns true if the x and y components of point is a point inside this rectangle. If allowInverse is present and true, the width and height of the Rect are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
		/// </summary>
		/// <param name="point">Point to test.</param>
		/// <param name="allowInverse">Does the test allow the Rect's width and height to be negative?</param>
		/// <returns>
		///   <para>True if the point lies within the specified rectangle.</para>
		/// </returns>
		public bool Contains(Vector2 point)
		{
			if ((double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax && (double)point.y >= (double)this.yMin)
				return (double)point.y < (double)this.yMax;
			return false;
		}

		/// <summary>
		///   <para>Returns true if the x and y components of point is a point inside this rectangle. If allowInverse is present and true, the width and height of the Rect are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
		/// </summary>
		/// <param name="point">Point to test.</param>
		/// <param name="allowInverse">Does the test allow the Rect's width and height to be negative?</param>
		/// <returns>
		///   <para>True if the point lies within the specified rectangle.</para>
		/// </returns>
		public bool Contains(Vector3 point)
		{
			if ((double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax && (double)point.y >= (double)this.yMin)
				return (double)point.y < (double)this.yMax;
			return false;
		}

		/// <summary>
		///   <para>Returns true if the x and y components of point is a point inside this rectangle. If allowInverse is present and true, the width and height of the Rect are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
		/// </summary>
		/// <param name="point">Point to test.</param>
		/// <param name="allowInverse">Does the test allow the Rect's width and height to be negative?</param>
		/// <returns>
		///   <para>True if the point lies within the specified rectangle.</para>
		/// </returns>
		public bool Contains(Vector3 point, bool allowInverse)
		{
			if (!allowInverse)
				return this.Contains(point);
			bool flag = false;
			if ((double)this.width < 0.0 && (double)point.x <= (double)this.xMin && (double)point.x > (double)this.xMax || (double)this.width >= 0.0 && (double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax)
				flag = true;
			return flag && ((double)this.height < 0.0 && (double)point.y <= (double)this.yMin && (double)point.y > (double)this.yMax || (double)this.height >= 0.0 && (double)point.y >= (double)this.yMin && (double)point.y < (double)this.yMax);
		}

		private static Rect OrderMinMax(Rect rect)
		{
			if ((double)rect.xMin > (double)rect.xMax)
			{
				float xMin = rect.xMin;
				rect.xMin = rect.xMax;
				rect.xMax = xMin;
			}
			if ((double)rect.yMin > (double)rect.yMax)
			{
				float yMin = rect.yMin;
				rect.yMin = rect.yMax;
				rect.yMax = yMin;
			}
			return rect;
		}

		/// <summary>
		///   <para>Returns true if the other rectangle overlaps this one. If allowInverse is present and true, the widths and heights of the Rects are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
		/// </summary>
		/// <param name="other">Other rectangle to test overlapping with.</param>
		/// <param name="allowInverse">Does the test allow the Rects' widths and heights to be negative?</param>
		public bool Overlaps(Rect other)
		{
			if ((double)other.xMax > (double)this.xMin && (double)other.xMin < (double)this.xMax && (double)other.yMax > (double)this.yMin)
				return (double)other.yMin < (double)this.yMax;
			return false;
		}

		/// <summary>
		///   <para>Returns true if the other rectangle overlaps this one. If allowInverse is present and true, the widths and heights of the Rects are allowed to take negative values (ie, the min value is greater than the max), and the test will still work.</para>
		/// </summary>
		/// <param name="other">Other rectangle to test overlapping with.</param>
		/// <param name="allowInverse">Does the test allow the Rects' widths and heights to be negative?</param>
		public bool Overlaps(Rect other, bool allowInverse)
		{
			Rect rect = this;
			if (allowInverse)
			{
				rect = Rect.OrderMinMax(rect);
				other = Rect.OrderMinMax(other);
			}
			return rect.Overlaps(other);
		}

		/// <summary>
		///   <para>Returns a point inside a rectangle, given normalized coordinates.</para>
		/// </summary>
		/// <param name="rectangle">Rectangle to get a point inside.</param>
		/// <param name="normalizedRectCoordinates">Normalized coordinates to get a point for.</param>
		public static Vector2 NormalizedToPoint(Rect rectangle, Vector2 normalizedRectCoordinates)
		{
			return new Vector2(Mathf.Lerp(rectangle.x, rectangle.xMax, normalizedRectCoordinates.x), Mathf.Lerp(rectangle.y, rectangle.yMax, normalizedRectCoordinates.y));
		}

		/// <summary>
		///   <para>Returns the normalized coordinates cooresponding the the point.</para>
		/// </summary>
		/// <param name="rectangle">Rectangle to get normalized coordinates inside.</param>
		/// <param name="point">A point inside the rectangle to get normalized coordinates for.</param>
		public static Vector2 PointToNormalized(Rect rectangle, Vector2 point)
		{
			return new Vector2(Mathf.InverseLerp(rectangle.x, rectangle.xMax, point.x), Mathf.InverseLerp(rectangle.y, rectangle.yMax, point.y));
		}

		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.width.GetHashCode() << 2 ^ this.y.GetHashCode() >> 2 ^ this.height.GetHashCode() >> 1;
		}

		public override bool Equals(object other)
		{
			if (!(other is Rect))
				return false;
			Rect rect = (Rect)other;
			if (this.x.Equals(rect.x) && this.y.Equals(rect.y) && this.width.Equals(rect.width))
				return this.height.Equals(rect.height);
			return false;
		}
	}
}
