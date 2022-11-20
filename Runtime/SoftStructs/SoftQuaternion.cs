using System;
using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    /// <summary>
    /// General quaternion. Represent rotation and scaling.
    /// </summary>
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
        public static SoftQuaternion Identity =>
            new SoftQuaternion(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.One);

        /// <summary>
        /// Returns true if the given quaternion is exactly equal to this quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) =>
            other is SoftQuaternion softQuaternion && Equals(softQuaternion);

        /// <summary>
        /// Returns true if the given quaternion is exactly equal to this quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SoftQuaternion other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        public override string ToString() =>
            ToString("F2", System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

        public string ToString(string format) =>
            ToString(format, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

        public string ToString(IFormatProvider provider) => ToString("F2", provider);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("({0}, {1}, {2}, {3})", X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider), Z.ToString(format, formatProvider),
                Y.ToString(format, formatProvider));
        }

        public override int GetHashCode() =>
            X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2 ^ W.GetHashCode() >> 1;

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
        /// Returns the vector transformed by the quaternion, including scale and rotation.
        /// Also known as sandwich product: q * vec * conj(q)
        /// </summary>
        public static SoftVector3 operator *(SoftQuaternion quaternion, SoftVector3 vector)
        {
            SoftFloat twoX = quaternion.X * (SoftFloat)2f;
            SoftFloat twoY = quaternion.Y * (SoftFloat)2f;
            SoftFloat twoZ = quaternion.Z * (SoftFloat)2f;
            SoftFloat xx = quaternion.X * quaternion.X;
            SoftFloat yy = quaternion.Y * quaternion.Y;
            SoftFloat zz = quaternion.Z * quaternion.Z;
            SoftFloat ww = quaternion.W * quaternion.W;
            SoftFloat xy2 = quaternion.X * twoY;
            SoftFloat xz2 = quaternion.X * twoZ;
            SoftFloat yz2 = quaternion.Y * twoZ;
            SoftFloat wx2 = quaternion.W * twoX;
            SoftFloat wy2 = quaternion.W * twoY;
            SoftFloat wz2 = quaternion.W * twoZ;
            SoftVector3 result = new SoftVector3(
                (ww + xx - yy - zz) * vector.X + (xy2 - wz2) * vector.Y + (xz2 + wy2) * vector.Z,
                (xy2 + wz2) * vector.X + (ww - xx + yy - zz) * vector.Y + (yz2 - wx2) * vector.Z,
                (xz2 - wy2) * vector.X + (yz2 + wx2) * vector.Y + (ww - xx - yy + zz) * vector.Z);
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
        /// Constructs quaternion from normalized one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion FromVersor(SoftVersor versor)
        {
            return new SoftQuaternion(versor.X, versor.Y, versor.Z, versor.W);
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion Normalize(SoftQuaternion a)
        {
            SoftFloat length = Length(a);
            return a / length;
        }

        /// <summary>
        /// Returns a safe normalized version of a quaternion.
        /// Returns the given default value when quaternion length close to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion NormalizeSafe(SoftQuaternion a,
            SoftQuaternion defaultValue = new SoftQuaternion())
        {
            SoftFloat sqrLength = LengthSqr(a);
            if (sqrLength < SoftMath.CalculationsEpsilonSqr)
                return defaultValue;
            return a / SoftMath.Sqrt(sqrLength);
        }

        /// <summary>
        /// Returns a spherical interpolation between two quaternions.
        /// Non-commutative, torque-minimal, constant velocity.
        /// Preserve it's properties when quaternions have identical length. 
        /// </summary>
        public static SoftQuaternion Slerp(SoftQuaternion a, SoftQuaternion b, SoftFloat t,
            bool longPath = false)
        {
            // Calculate angle between them.
            SoftFloat cosHalfTheta = Dot(Normalize(a), Normalize(b));

            if (longPath)
            {
                if (cosHalfTheta > SoftFloat.Zero)
                {
                    b = -b;
                    cosHalfTheta = -cosHalfTheta;
                }
            }
            else
            {
                if (cosHalfTheta < SoftFloat.Zero)
                {
                    b = -b;
                    cosHalfTheta = -cosHalfTheta;
                }
            }

            // If a = b or a = b then theta = 0 and we can return interpolation between a and b
            if (SoftMath.Abs(cosHalfTheta) > SoftFloat.One - SoftMath.CalculationsEpsilon)
            {
                return Lerp(a, b, t, longPath);
            }

            SoftFloat halfTheta = SoftMath.Acos(cosHalfTheta);
            SoftFloat sinHalfTheta = SoftMath.Sin(halfTheta);

            SoftFloat influenceA = SoftMath.Sin((SoftFloat.One - t) * halfTheta) / sinHalfTheta;
            SoftFloat influenceB = SoftMath.Sin(t * halfTheta) / sinHalfTheta;

            return a * influenceA + b * influenceB;
        }

        /// <summary>
        /// Returns a normalized componentwise interpolation between two quaternions.
        /// Commutative, torque-minimal, non-constant velocity.
        /// Preserve it's properties when quaternions have identical length.
        /// </summary>
        public static SoftQuaternion Nlerp(SoftQuaternion a, SoftQuaternion b, SoftFloat t,
            bool longPath = false)
        {
            SoftFloat dot = Dot(a, b);

            if (longPath)
            {
                if (dot > SoftFloat.Zero)
                {
                    b = -b;
                }
            }
            else
            {
                if (dot < SoftFloat.Zero)
                {
                    b = -b;
                }
            }

            SoftFloat lenght = Length(a) * (SoftFloat.One - t) + Length(b) * t;
            SoftQuaternion normalized = Normalize(a * (SoftFloat.One - t) + b * t);

            return normalized * lenght;
        }

        /// <summary>
        /// Returns a componentwise interpolation between two quaternions.
        /// </summary>
        public static SoftQuaternion Lerp(SoftQuaternion a, SoftQuaternion b, SoftFloat t,
            bool longPath = false)
        {
            SoftFloat dot = Dot(a, b);

            if (longPath)
            {
                if (dot > SoftFloat.Zero)
                {
                    b = -b;
                }
            }
            else
            {
                if (dot < SoftFloat.Zero)
                {
                    b = -b;
                }
            }

            return a * (SoftFloat.One - t) + b * t;
        }

        /// <summary>
        /// Compares two quaternions with <see cref="SoftMath.CalculationsEpsilonSqr"/> and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftQuaternion a, SoftQuaternion b)
        {
            return ApproximatelyEqual(a, b, SoftMath.CalculationsEpsilonSqr);
        }

        /// <summary>
        /// Compares two quaternions with some epsilon and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftQuaternion a, SoftQuaternion b, SoftFloat epsilon)
        {
            return SoftMath.Abs(SoftFloat.One - SoftMath.Abs(Dot(a, b))) < epsilon;
        }
    }
}