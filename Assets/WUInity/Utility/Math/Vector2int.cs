// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace WUIEngine
{
    // Representation of 2D vectors and points.
    [System.Serializable]
    public struct Vector2int : IEquatable<Vector2int>, IFormattable
    {
        public int x
        {
            get { return m_X; }

            set { m_X = value; }
        }


        public int y
        {
            get { return m_Y; }

            set { m_Y = value; }
        }

        private int m_X;
        private int m_Y;

        public Vector2int(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }

        // Set x and y components of an existing Vector.
        public void Set(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }

        // Access the /x/ or /y/ component using [0] or [1] respectively.
        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    default:
                        throw new IndexOutOfRangeException(String.Format("Invalid Vector2int index addressed: {0}!", index));
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    default:
                        throw new IndexOutOfRangeException(String.Format("Invalid Vector2int index addressed: {0}!", index));
                }
            }
        }

        // Returns the length of this vector (RO).
        public float magnitude { get { return Mathf.Sqrt((float)(x * x + y * y)); } }

        // Returns the squared length of this vector (RO).
        public int sqrMagnitude { get { return x * x + y * y; } }

        // Returns the distance between /a/ and /b/.
        public static float Distance(Vector2int a, Vector2int b)
        {
            float diff_x = a.x - b.x;
            float diff_y = a.y - b.y;

            return (float)Math.Sqrt(diff_x * diff_x + diff_y * diff_y);
        }

        // Returns a vector that is made from the smallest components of two vectors.
        public static Vector2int Min(Vector2int lhs, Vector2int rhs) { return new Vector2int(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y)); }

        // Returns a vector that is made from the largest components of two vectors.
        public static Vector2int Max(Vector2int lhs, Vector2int rhs) { return new Vector2int(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y)); }

        // Multiplies two vectors component-wise.
        public static Vector2int Scale(Vector2int a, Vector2int b) { return new Vector2int(a.x * b.x, a.y * b.y); }

        // Multiplies every component of this vector by the same component of /scale/.
        public void Scale(Vector2int scale) { x *= scale.x; y *= scale.y; }

        public void Clamp(Vector2int min, Vector2int max)
        {
            x = Math.Max(min.x, x);
            x = Math.Min(max.x, x);
            y = Math.Max(min.y, y);
            y = Math.Min(max.y, y);
        }

        // Converts a Vector2int to a [[Vector2]].
        public static implicit operator Vector2(Vector2int v)
        {
            return new Vector2(v.x, v.y);
        }

        // Converts a Vector2int to a [[Vector3Int]].
        /*public static explicit operator Vector3Int(Vector2int v)
        {
            return new Vector3Int(v.x, v.y, 0);
        }*/

        public static Vector2int FloorToInt(Vector2 v)
        {
            return new Vector2int(
                Mathf.FloorToInt(v.X),
                Mathf.FloorToInt(v.Y)
            );
        }

        public static Vector2int CeilToInt(Vector2 v)
        {
            return new Vector2int(
                Mathf.CeilToInt(v.X),
                Mathf.CeilToInt(v.Y)
            );
        }

        public static Vector2int RoundToInt(Vector2 v)
        {
            return new Vector2int(
                Mathf.RoundToInt(v.X),
                Mathf.RoundToInt(v.Y)
            );
        }

        public static Vector2int operator -(Vector2int v)
        {
            return new Vector2int(-v.x, -v.y);
        }

        public static Vector2int operator +(Vector2int a, Vector2int b)
        {
            return new Vector2int(a.x + b.x, a.y + b.y);
        }

        public static Vector2int operator -(Vector2int a, Vector2int b)
        {
            return new Vector2int(a.x - b.x, a.y - b.y);
        }

        public static Vector2int operator *(Vector2int a, Vector2int b)
        {
            return new Vector2int(a.x * b.x, a.y * b.y);
        }

        public static Vector2int operator *(int a, Vector2int b)
        {
            return new Vector2int(a * b.x, a * b.y);
        }

        public static Vector2int operator *(Vector2int a, int b)
        {
            return new Vector2int(a.x * b, a.y * b);
        }

        public static Vector2int operator /(Vector2int a, int b)
        {
            return new Vector2int(a.x / b, a.y / b);
        }

        public static bool operator ==(Vector2int lhs, Vector2int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Vector2int lhs, Vector2int rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector2int)) return false;

            return Equals((Vector2int)other);
        }

        public bool Equals(Vector2int other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

        /// *listonly*
        public override string ToString()
        {
            return ToString(null, null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider == null)
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            return String.Format("({0}, {1})", x.ToString(format, formatProvider), y.ToString(format, formatProvider));
        }

        public static Vector2int zero { get { return s_Zero; } }
        public static Vector2int one { get { return s_One; } }
        public static Vector2int up { get { return s_Up; } }
        public static Vector2int down { get { return s_Down; } }
        public static Vector2int left { get { return s_Left; } }
        public static Vector2int right { get { return s_Right; } }

        private static readonly Vector2int s_Zero = new Vector2int(0, 0);
        private static readonly Vector2int s_One = new Vector2int(1, 1);
        private static readonly Vector2int s_Up = new Vector2int(0, 1);
        private static readonly Vector2int s_Down = new Vector2int(0, -1);
        private static readonly Vector2int s_Left = new Vector2int(-1, 0);
        private static readonly Vector2int s_Right = new Vector2int(1, 0);
    }
}