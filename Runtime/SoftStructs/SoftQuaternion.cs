using System;
using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    public readonly struct SoftQuaternion : IEquatable<SoftQuaternion>, IFormattable
    {
        public readonly SoftFloat X;
        public readonly SoftFloat Y;
        public readonly SoftFloat Z;
        public readonly SoftFloat W;

        /// <summary>
        /// Constructs a quaternion from four SoftFloat values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SoftQuaternion(SoftFloat x, SoftFloat y, SoftFloat z, SoftFloat w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        
        /// <summary>
        /// The identity rotation.
        /// </summary>
        public static SoftQuaternion Identity => new SoftQuaternion(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.One);

        /// <summary>
        /// Returns true if the given quaternion is exactly equal to this quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is SoftQuaternion softQuaternion && Equals(softQuaternion);

        /// <summary>
        /// Returns true if the given quaternion is exactly equal to this quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SoftQuaternion other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        public override string ToString() => ToString("F2", System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

        public string ToString(string format) => ToString(format, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        
        public string ToString(IFormatProvider provider) => ToString("F2", provider);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("({0}, {1}, {2}, {3})", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
        
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2 ^ W.GetHashCode() >> 1;
        
        /// <summary>
        /// Returns the componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator +(SoftQuaternion a, SoftQuaternion b)
        {
            return new SoftQuaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }
        
        /// <summary>
        /// Returns the componentwise negotiation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator -(SoftQuaternion a)
        {
            return new SoftQuaternion(-a.X, -a.Y, -a.Z, -a.W);
        }
        
        /// <summary>
        /// Returns the componentwise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator -(SoftQuaternion a, SoftQuaternion b)
        {
            return -b + a;
        }

        /// <summary>
        /// Returns the quaternions multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator *(SoftQuaternion a, SoftQuaternion b)
        {
            return new SoftQuaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
        }
        
        /// <summary>
        /// Returns the componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator *(SoftQuaternion a, SoftFloat b)
        {
            return new SoftQuaternion(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        /// <summary>
        /// Returns vector transformed by the quaternion.
        /// </summary>
        public static SoftVector3 operator *(SoftQuaternion rotation, SoftVector3 vector)
        {
            SoftFloat twoX = rotation.X * (SoftFloat)2f;
            SoftFloat twoY = rotation.Y * (SoftFloat)2f;
            SoftFloat twoZ = rotation.Z * (SoftFloat)2f;
            SoftFloat xx = rotation.X * twoX;
            SoftFloat yy = rotation.Y * twoY;
            SoftFloat zz = rotation.Z * twoZ;
            SoftFloat xy = rotation.X * twoY;
            SoftFloat xz = rotation.X * twoZ;
            SoftFloat yz = rotation.Y * twoZ;
            SoftFloat wx = rotation.W * twoX;
            SoftFloat wy = rotation.W * twoY;
            SoftFloat wz = rotation.W * twoZ;
            SoftVector3 result = new SoftVector3(
                (SoftFloat.One - (yy + zz)) * vector.X + (xy - wz) * vector.Y + (xz + wy) * vector.Z,
                (xy + wz) * vector.X + (SoftFloat.One - (xx + zz)) * vector.Y + (yz - wx) * vector.Z,
                (xz - wy) * vector.X + (yz + wx) * vector.Y + (SoftFloat.One - (xx + yy)) * vector.Z);
            return result;
        }

        /// <summary>
        /// Returns the quaternions division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator /(SoftQuaternion a, SoftQuaternion b)
        {
            return a * Inverse(b);
        }
        
        /// <summary>
        /// Returns the componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator /(SoftQuaternion a, SoftFloat b)
        {
            return new SoftQuaternion(a.X / b, a.Y / b, a.Z / b, a.W / b);
        }

        /// <summary>
        /// Returns true if quaternions are approximately equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SoftQuaternion a, SoftQuaternion b) => ApproximatelyEqual(a, b);

        /// <summary>
        /// Returns true if quaternions are not approximately equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SoftQuaternion a, SoftQuaternion b) => !(a == b);
        
        /// <summary>
        /// The dot product between two rotations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Dot(SoftQuaternion a, SoftQuaternion b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }
        
        /// <summary>
        /// Returns the length of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Length(SoftQuaternion a)
        {
            return SoftMath.Sqrt(LengthSqr(a));
        }

        /// <summary>
        /// Returns the squared length of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat LengthSqr(SoftQuaternion a)
        {
            return Dot(a, a);
        }
        
        /// <summary>
        /// Returns the conjugate of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion Conjugate(SoftQuaternion a)
        {
            return new SoftQuaternion(-a.X, -a.Y, -a.Z, a.W);
        }
        
        /// <summary>
        /// Returns the inverse of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion Inverse(SoftQuaternion a)
        {
            SoftFloat lengthSqr = LengthSqr(a);
            SoftQuaternion conjugation = Conjugate(a);
            return conjugation / lengthSqr;
        }
        
        /// <summary>
        /// Returns a normalized version of a quaternion.
        /// </summary>
        [MethodImpl((MethodImplOptions) 256)]
        public static SoftQuaternion Normalize(SoftQuaternion a)
        {
            SoftFloat length = Length(a);
            return a / length;
        }
        
        /// <summary>
        /// Returns a safe normalized version of a quaternion.
        /// Returns the given default value when quaternion scale close to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion NormalizeSafe(SoftQuaternion a, SoftQuaternion defaultValue = new SoftQuaternion())
        {
            SoftFloat sqrLength = LengthSqr(a);
            if (sqrLength < SoftFloat.Epsilon)
                return defaultValue;
            return a / SoftMath.Sqrt(sqrLength);
        }

        /// <summary>
        /// Compares two quaternions with <see cref="SoftFloat.Epsilon"/> and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftQuaternion a, SoftQuaternion b)
        {
            return ApproximatelyEqual(a, b, SoftFloat.Epsilon);
        }
        
        /// <summary>
        /// Compares two quaternions with some epsilon and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftQuaternion a, SoftQuaternion b, SoftFloat epsilon)
        {
            return Dot(a, b) > SoftFloat.One - epsilon;
        }
    }
}