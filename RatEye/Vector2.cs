﻿namespace RatEye
{
	/// <summary>
	/// A 2D vector
	/// </summary>
	public class Vector2
	{
		/// <summary>
		/// The X value
		/// </summary>
		public int X;

		/// <summary>
		/// The Y value
		/// </summary>
		public int Y;

		/// <summary>
		/// System.Drawing.Point to Vector2
		/// </summary>
		/// <param name="point">System.Drawing.Point to convert</param>
		public Vector2(System.Drawing.Point point) : this(point.X, point.Y) { }

		/// <summary>
		/// OpenCvSharp.Point to Vector2
		/// </summary>
		/// <param name="point">OpenCvSharp.Point to convert</param>
		public Vector2(OpenCvSharp.Point point) : this(point.X, point.Y) { }

		/// <summary>
		/// System.Drawing.Size to Vector2
		/// </summary>
		/// <param name="size">System.Drawing.Size to convert</param>
		public Vector2(System.Drawing.Size size) : this(size.Width, size.Height) { }

		/// <summary>
		/// OpenCvSharp.Size to Vector2
		/// </summary>
		/// <param name="size">OpenCvSharp.Size to convert</param>
		public Vector2(OpenCvSharp.Size size) : this(size.Width, size.Height) { }

		/// <summary>
		/// Constructor for Vector2
		/// </summary>
		/// <param name="x">The X value</param>
		/// <param name="y">The Y value</param>
		public Vector2(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Returns a new <see cref="Vector2"/> with <c>X = 0</c> and <c>Y = 0</c>
		/// </summary>
		public static Vector2 Zero => new Vector2(0, 0);

		/// <summary>
		/// Returns a new <see cref="Vector2"/> with <c>X = 1</c> and <c>Y = 1</c>
		/// </summary>
		public static Vector2 One => new Vector2(1, 1);

		/// <summary>
		/// Vector2 to System.Drawing.Point
		/// </summary>
		/// <param name="vec">Vector2 to convert</param>
		public static implicit operator System.Drawing.Point(Vector2 vec)
		{
			return new System.Drawing.Point(vec.X, vec.Y);
		}

		/// <summary>
		/// Vector2 to OpenCvSharp.Point
		/// </summary>
		/// <param name="vec">Vector2 to convert</param>
		public static implicit operator OpenCvSharp.Point(Vector2 vec)
		{
			return new OpenCvSharp.Point(vec.X, vec.Y);
		}

		/// <summary>
		/// Vector2 to System.Drawing.Size
		/// </summary>
		/// <param name="vec">Vector2 to convert</param>
		public static implicit operator System.Drawing.Size(Vector2 vec)
		{
			return new System.Drawing.Size(vec.X, vec.Y);
		}

		/// <summary>
		/// Vector2 to OpenCvSharp.Size
		/// </summary>
		/// <param name="vec">Vector2 to convert</param>
		public static implicit operator OpenCvSharp.Size(Vector2 vec)
		{
			return new OpenCvSharp.Size(vec.X, vec.Y);
		}

		/// <summary>
		/// Vector addition
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Vector B</param>
		/// <returns>Vector A + Vector B</returns>
		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X + b.X, a.Y + b.Y);
		}

		/// <summary>
		/// Vector addition with scalar
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Scalar B</param>
		/// <returns>Vector A + Scalar B</returns>
		public static Vector2 operator +(Vector2 a, int b)
		{
			return new Vector2(a.X + b, a.Y + b);
		}

		/// <summary>
		/// Vector subtraction
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Vector B</param>
		/// <returns>Vector A - Vector B</returns>
		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X - b.X, a.Y - b.Y);
		}

		/// <summary>
		/// Vector subtraction with scalar
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Scalar B</param>
		/// <returns>Vector A - Scalar B</returns>
		public static Vector2 operator -(Vector2 a, int b)
		{
			return new Vector2(a.X - b, a.Y - b);
		}

		/// <summary>
		/// Vector multiplication
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Vector B</param>
		/// <returns>Vector A * Vector B</returns>
		public static Vector2 operator *(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X * b.X, a.Y * b.Y);
		}

		/// <summary>
		/// Vector multiplication with scalar
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Scalar B</param>
		/// <returns>Vector A * Scalar B</returns>
		public static Vector2 operator *(Vector2 a, int b)
		{
			return new Vector2(a.X * b, a.Y * b);
		}

		/// <summary>
		/// Vector multiplication with scalar
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Scalar B</param>
		/// <returns>Vector A * Scalar B</returns>
		public static Vector2 operator *(Vector2 a, float b)
		{
			return new Vector2((int)(a.X * b), (int)(a.Y * b));
		}

		/// <summary>
		/// Vector division
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Vector B</param>
		/// <returns>Vector A / Vector B</returns>
		public static Vector2 operator /(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X / b.X, a.Y / b.Y);
		}

		/// <summary>
		/// Vector division with scalar
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Scalar B</param>
		/// <returns>Vector A / Scalar B</returns>
		public static Vector2 operator /(Vector2 a, int b)
		{
			return new Vector2(a.X / b, a.Y / b);
		}

		/// <summary>
		/// Vector division with scalar
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Scalar B</param>
		/// <returns>Vector A / Scalar B</returns>
		public static Vector2 operator /(Vector2 a, float b)
		{
			return new Vector2((int)(a.X / b), (int)(a.Y / b));
		}

		/// <summary>
		/// Vector equality
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Vector B</param>
		/// <returns>True if vectors are equal</returns>
		public static bool operator ==(Vector2 a, Vector2 b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (ReferenceEquals(a, null)) return false;
			if (ReferenceEquals(b, null)) return false;
			return a.Equals(b);
		}

		/// <summary>
		/// Vector inequality
		/// </summary>
		/// <param name="a">Vector A</param>
		/// <param name="b">Vector B</param>
		/// <returns>True if vectors are not equal</returns>
		public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

		/// <summary>
		/// X * Y
		/// </summary>
		public int Area => X * Y;

		/// <summary>
		/// Check vector equality
		/// </summary>
		/// <param name="obj">Vector to compare</param>
		/// <returns>True if vectors are equal</returns>
		public override bool Equals(object obj)
		{
			if ((obj == null) || GetType() != obj.GetType()) return false;
			var other = (Vector2)obj;
			return X == other.X && Y == other.Y;
		}

		/// <summary>
		/// Get hash code of vector
		/// </summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hash = (int)2166136261;
				hash = (hash * 16777619) ^ X.GetHashCode();
				hash = (hash * 16777619) ^ Y.GetHashCode();
				return hash;
			}
		}

		/// <summary>
		/// Get string representation of vector
		/// </summary>
		/// <returns>String representation</returns>
		public override string ToString()
		{
			return $"({X}, {Y})";
		}
	}
}
