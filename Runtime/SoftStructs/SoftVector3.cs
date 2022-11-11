using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    public readonly struct SoftVector3
    {
        public readonly SoftFloat X;
        public readonly SoftFloat Y;
        public readonly SoftFloat Z;

        /// <summary>
        /// Constructs a vector from three SoftFloat values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SoftVector3(SoftFloat x, SoftFloat y, SoftFloat z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        /// <summary>
        /// Shorthand for writing SoftVector3(0, 0, 0).
        /// </summary>
        public static readonly SoftVector3 Zero = new SoftVector3(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);

        /// <summary>
        /// Shorthand for writing SoftVector3(1, 1, 1).
        /// </summary>
        public static readonly SoftVector3 One = new SoftVector3(SoftFloat.One, SoftFloat.One, SoftFloat.One);

        /// <summary>
        /// Shorthand for writing SoftVector3(1, 0, 0).
        /// </summary>
        public static readonly SoftVector3 Right = new SoftVector3(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(-1, 0, 0).
        /// </summary>
        public static readonly SoftVector3 Left = new SoftVector3(SoftFloat.MinusOne, SoftFloat.Zero, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, 1, 0).
        /// </summary>
        public static readonly SoftVector3 Up = new SoftVector3(SoftFloat.Zero, SoftFloat.One, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, -1, 0).
        /// </summary>
        public static readonly SoftVector3 Down = new SoftVector3(SoftFloat.Zero, SoftFloat.MinusOne, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, 0, 1).
        /// </summary>
        public static readonly SoftVector3 Forward = new SoftVector3(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.One);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, 0, -1).
        /// </summary>
        public static readonly SoftVector3 Backward = new SoftVector3(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.MinusOne);
        
        /// <summary>
        /// The componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator +(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        
        /// <summary>
        /// The componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator +(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X + b, a.Y + b, a.Z + b);
        }
        
        /// <summary>
        /// The componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator +(SoftFloat a, SoftVector3 b)
        {
            return b + a;
        }
        
        /// <summary>
        /// The componentwise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator -(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        
        /// <summary>
        /// The componentwise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator -(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X - b, a.Y - b, a.Z - b);
        }
        
        /// <summary>
        /// The componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator -(SoftFloat a, SoftVector3 b)
        {
            return b - a;
        }
        
        /// <summary>
        /// The componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator *(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        
        /// <summary>
        /// The componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator *(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X * b, a.Y * b, a.Z * b);
        }
        
        /// <summary>
        /// The componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator *(SoftFloat a, SoftVector3 b)
        {
            return b * a;
        }

        /// <summary>
        /// The componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator /(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }
        
        /// <summary>
        /// The componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator /(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X / b, a.Y / b, a.Z / b);
        }
        
        /// <summary>
        /// The componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator /(SoftFloat a, SoftVector3 b)
        {
            return b / a;
        }
        
        /// <summary>
        /// The dot product of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Dot(SoftVector3 a, SoftVector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
        
        /// <summary>
        /// The cross product of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Cross(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        /// <summary>
        /// The componentwise maximum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Max(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(SoftMath.Max(a.X, b.X), SoftMath.Max(a.Y, b.Y), SoftMath.Max(a.Z, b.Z));
        }

        /// <summary>
        /// The length of a vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Length(SoftVector3 a)
        {
            return SoftMath.Sqrt(Dot(a, a));
        }

        /// <summary>
        /// The squared length of a vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat LengthSqr(SoftVector3 a)
        {
            return Dot(a, a);
        }

        /// <summary>
        /// The componentwise absolute value of a vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Abs(SoftVector3 a)
        {
            return new SoftVector3(SoftMath.Abs(a.X), SoftMath.Abs(a.Y), SoftMath.Abs(a.Z));
        }
    }
}