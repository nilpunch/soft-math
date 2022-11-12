using System;
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
        public static SoftVector3 Zero => new SoftVector3(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero);

        /// <summary>
        /// Shorthand for writing SoftVector3(1, 1, 1).
        /// </summary>
        public static SoftVector3 One => new SoftVector3(SoftFloat.One, SoftFloat.One, SoftFloat.One);

        /// <summary>
        /// Shorthand for writing SoftVector3(1, 0, 0).
        /// </summary>
        public static SoftVector3 Right => new SoftVector3(SoftFloat.One, SoftFloat.Zero, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(-1, 0, 0).
        /// </summary>
        public static SoftVector3 Left => new SoftVector3(SoftFloat.MinusOne, SoftFloat.Zero, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, 1, 0).
        /// </summary>
        public static SoftVector3 Up => new SoftVector3(SoftFloat.Zero, SoftFloat.One, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, -1, 0).
        /// </summary>
        public static SoftVector3 Down => new SoftVector3(SoftFloat.Zero, SoftFloat.MinusOne, SoftFloat.Zero);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, 0, 1).
        /// </summary>
        public static SoftVector3 Forward => new SoftVector3(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.One);
            
        /// <summary>
        /// Shorthand for writing SoftVector3(0, 0, -1).
        /// </summary>
        public static SoftVector3 Backward => new SoftVector3(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.MinusOne);
        
        /// <summary>
        /// Returns the componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator +(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        
        /// <summary>
        /// Returns the componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator +(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X + b, a.Y + b, a.Z + b);
        }
        
        /// <summary>
        /// Returns the componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator +(SoftFloat a, SoftVector3 b)
        {
            return b + a;
        }
        
        /// <summary>
        /// Returns the componentwise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator -(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        
        /// <summary>
        /// Returns the componentwise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator -(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X - b, a.Y - b, a.Z - b);
        }
        
        /// <summary>
        /// Returns the componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator -(SoftFloat a, SoftVector3 b)
        {
            return b - a;
        }
        
        /// <summary>
        /// Returns the componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator *(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        
        /// <summary>
        /// Returns the componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator *(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X * b, a.Y * b, a.Z * b);
        }
        
        /// <summary>
        /// Returns the componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator *(SoftFloat a, SoftVector3 b)
        {
            return b * a;
        }

        /// <summary>
        /// Returns the componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator /(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
        }
        
        /// <summary>
        /// Returns the componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator /(SoftVector3 a, SoftFloat b)
        {
            return new SoftVector3(a.X / b, a.Y / b, a.Z / b);
        }
        
        /// <summary>
        /// Returns the componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 operator /(SoftFloat a, SoftVector3 b)
        {
            return b / a;
        }
        
        /// <summary>
        /// Returns true if vectors are equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SoftVector3 a, SoftVector3 b) => ApproximatelyEqual(a, b, SoftFloat.Epsilon);

        /// <summary>
        /// Returns true if vectors are not equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SoftVector3 a, SoftVector3 b) => !(a == b);
        
        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Dot(SoftVector3 a, SoftVector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
        
        /// <summary>
        /// Returns the cross product of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Cross(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        /// <summary>
        /// Returns the length of a vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Length(SoftVector3 a)
        {
            return SoftMath.Sqrt(LengthSqr(a));
        }

        /// <summary>
        /// Returns the squared length of a vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat LengthSqr(SoftVector3 a)
        {
            return Dot(a, a);
        }
        
        /// <summary>
        /// Returns the distance between a and b.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Distance(SoftVector3 a, SoftVector3 b)
        {
            return SoftMath.Sqrt(DistanceSqr(a, b));
        }
        
        /// <summary>
        /// Returns the squared distance between a and b.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat DistanceSqr(SoftVector3 a, SoftVector3 b)
        {
            SoftFloat deltaX = a.X - b.X;
            SoftFloat deltaY = a.Y - b.Y;
            SoftFloat deltaZ = a.Z - b.Z;
            return deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        }
        
        /// <summary>
        /// Returns a normalized version of a vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Normalize(SoftVector3 a)
        {
            SoftFloat length = Length(a);
            if (length < SoftFloat.Epsilon)
                throw new Exception("Vector length close to zero.");
            return a / length;
        }

        /// <summary>
        /// Returns a safe normalized version of a vector.
        /// Returns the given default value when 1 / length(x) does not produce a finite number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 NormalizeSafe(SoftVector3 a, SoftVector3 defaultValue = new SoftVector3())
        {
            SoftFloat lengthSqr = LengthSqr(a);
            if (lengthSqr < SoftFloat.Epsilon)
                return defaultValue;
            return a / SoftMath.Sqrt(lengthSqr);
        }

        /// <summary>
        /// Returns a vector that is made from the largest components of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Max(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(SoftMath.Max(a.X, b.X), SoftMath.Max(a.Y, b.Y), SoftMath.Max(a.Z, b.Z));
        }
        
        /// <summary>
        /// Returns a vector that is made from the smallest components of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Min(SoftVector3 a, SoftVector3 b)
        {
            return new SoftVector3(SoftMath.Min(a.X, b.X), SoftMath.Min(a.Y, b.Y), SoftMath.Min(a.Z, b.Z));
        }

        /// <summary>
        /// Returns the componentwise absolute value of a vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVector3 Abs(SoftVector3 a)
        {
            return new SoftVector3(SoftMath.Abs(a.X), SoftMath.Abs(a.Y), SoftMath.Abs(a.Z));
        }
        
        /// <summary>
        /// Compares two vectors with some epsilon and returns true if they are similar.
        /// </summary>
        public static bool ApproximatelyEqual(SoftVector3 a, SoftVector3 b, SoftFloat epsilon)
        {
            return DistanceSqr(a, b) < epsilon;
        }
    }
}