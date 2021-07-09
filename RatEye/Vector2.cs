using System;

namespace RatEye
{
	public class Vector2
	{
		public int X;
		public int Y;

		public Vector2(System.Drawing.Point point) : this(point.X, point.Y) { }

		public Vector2(OpenCvSharp.Point point) : this(point.X, point.Y) { }

		public Vector2(System.Drawing.Size size) : this(size.Width, size.Height) { }

		public Vector2(OpenCvSharp.Size size) : this(size.Width, size.Height) { }

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

		public static implicit operator System.Drawing.Point(Vector2 vec)
		{
			return new System.Drawing.Point(vec.X, vec.Y);
		}

		public static implicit operator OpenCvSharp.Point(Vector2 vec)
		{
			return new OpenCvSharp.Point(vec.X, vec.Y);
		}

		public static implicit operator System.Drawing.Size(Vector2 vec)
		{
			return new System.Drawing.Size(vec.X, vec.Y);
		}

		public static implicit operator OpenCvSharp.Size(Vector2 vec)
		{
			return new OpenCvSharp.Size(vec.X, vec.Y);
		}

		public static Vector2 operator +(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X + b.X, a.Y + b.Y);
		}

		public static Vector2 operator +(Vector2 a, int b)
		{
			return new Vector2(a.X + b, a.Y + b);
		}

		public static Vector2 operator -(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X - b.X, a.Y - b.Y);
		}

		public static Vector2 operator -(Vector2 a, int b)
		{
			return new Vector2(a.X - b, a.Y - b);
		}

		public static Vector2 operator *(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X * b.X, a.Y * b.Y);
		}

		public static Vector2 operator *(Vector2 a, int b)
		{
			return new Vector2(a.X * b, a.Y * b);
		}

		public static Vector2 operator /(Vector2 a, Vector2 b)
		{
			return new Vector2(a.X / b.X, a.Y / b.Y);
		}

		public static Vector2 operator /(Vector2 a, int b)
		{
			return new Vector2(a.X / b, a.Y / b);
		}

		public static bool operator ==(Vector2 a, Vector2 b)
		{
			if (ReferenceEquals(a, b)) return true;
			if (ReferenceEquals(a, null)) return false;
			if (ReferenceEquals(b, null)) return false;
			return a.Equals(b);
		}

		public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

		public override bool Equals(object obj)
		{
			if ((obj == null) || GetType() != obj.GetType()) return false;
			var other = (Vector2)obj;
			return X == other.X && Y == other.Y;
		}

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

		public override string ToString()
		{
			return $"({X}, {Y})";
		}
	}
}
