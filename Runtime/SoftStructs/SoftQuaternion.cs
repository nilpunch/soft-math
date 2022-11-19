using System;
using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    /// <summary>
    /// Represent rotation. Normalized quaternion.
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
        private SoftQuaternion(SoftFloat x, SoftFloat y, SoftFloat z, SoftFloat w)
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
        public override bool Equals(object other) => other is SoftQuaternion softQuaternion && Equals(softQuaternion);

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
        public static SoftQuaternionGeneral operator +(SoftQuaternion a, SoftQuaternion b)
        {
            return new SoftQuaternionGeneral(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
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
        public static SoftQuaternionGeneral operator -(SoftQuaternion a, SoftQuaternion b)
        {
            return -b + a;
        }

        /// <summary>
        /// Returns the quaternions multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator *(SoftQuaternion a, SoftQuaternion b)
        {
            return EnsureNormalization(new SoftQuaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z));
        }

        /// <summary>
        /// Returns the componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternionGeneral operator *(SoftQuaternion a, SoftFloat b)
        {
            return new SoftQuaternionGeneral(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        /// <summary>
        /// Returns the vector rotated by the quaternion.
        /// Also known as sandwich product: q * vec * conj(q)
        /// </summary>
        public static SoftVector3 operator *(SoftQuaternion quaternion, SoftVector3 vector)
        {
            SoftFloat twoX = quaternion.X * (SoftFloat)2f;
            SoftFloat twoY = quaternion.Y * (SoftFloat)2f;
            SoftFloat twoZ = quaternion.Z * (SoftFloat)2f;
            SoftFloat xx2 = quaternion.X * twoX;
            SoftFloat yy2 = quaternion.Y * twoY;
            SoftFloat zz2 = quaternion.Z * twoZ;
            SoftFloat xy2 = quaternion.X * twoY;
            SoftFloat xz2 = quaternion.X * twoZ;
            SoftFloat yz2 = quaternion.Y * twoZ;
            SoftFloat wx2 = quaternion.W * twoX;
            SoftFloat wy2 = quaternion.W * twoY;
            SoftFloat wz2 = quaternion.W * twoZ;
            SoftVector3 result = new SoftVector3(
                (SoftFloat.One - (yy2 + zz2)) * vector.X + (xy2 - wz2) * vector.Y + (xz2 + wy2) * vector.Z,
                (xy2 + wz2) * vector.X + (SoftFloat.One - (xx2 + zz2)) * vector.Y + (yz2 - wx2) * vector.Z,
                (xz2 - wy2) * vector.X + (yz2 + wx2) * vector.Y + (SoftFloat.One - (xx2 + yy2)) * vector.Z);
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
        public static SoftQuaternionGeneral operator /(SoftQuaternion a, SoftFloat b)
        {
            return new SoftQuaternionGeneral(a.X / b, a.Y / b, a.Z / b, a.W / b);
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
            return Conjugate(a);
        }

        /// <summary>
        /// Returns a normalized version of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion Normalize(SoftQuaternionGeneral a)
        {
            return FromNormalized(SoftQuaternionGeneral.Normalize(a));
        }

        /// <summary>
        /// Returns a safe normalized version of a quaternion.
        /// Returns the given default value when quaternion length close to zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion NormalizeSafe(SoftQuaternionGeneral a,
            SoftQuaternion defaultValue = new SoftQuaternion())
        {
            SoftFloat sqrLength = SoftQuaternionGeneral.LengthSqr(a);
            if (sqrLength < SoftMath.CalculationsEpsilonSqr)
                return defaultValue;
            return FromNormalized(a / SoftMath.Sqrt(sqrLength));
        }

        /// <summary>
        /// Returns a spherical interpolation between two quaternions.
        /// Non-commutative, torque-minimal, constant velocity.
        /// </summary>
        public static SoftQuaternion Slerp(SoftQuaternion a, SoftQuaternion b, SoftFloat t, bool longPath = false)
        {
            // Calculate angle between them.
            SoftFloat cosHalfTheta = Dot(a, b);

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

            // If a = b or a = b then theta = 0 then we can return interpolation between a and b
            if (SoftMath.Abs(cosHalfTheta) > SoftFloat.One - SoftMath.CalculationsEpsilon)
            {
                return Nlerp(a, b, t, longPath);
            }

            SoftFloat halfTheta = SoftMath.Acos(cosHalfTheta);
            SoftFloat sinHalfTheta = SoftMath.Sin(halfTheta);

            SoftFloat influenceA = SoftMath.Sin((SoftFloat.One - t) * halfTheta) / sinHalfTheta;
            SoftFloat influenceB = SoftMath.Sin(t * halfTheta) / sinHalfTheta;

            return EnsureNormalization(FromNormalized(a * influenceA + b * influenceB));
        }

        /// <summary>
        /// Returns a normalized componentwise interpolation between two quaternions.
        /// Commutative, torque-minimal, non-constant velocity.
        /// </summary>
        public static SoftQuaternion Nlerp(SoftQuaternion a, SoftQuaternion b, SoftFloat t, bool longPath = false)
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

            SoftQuaternionGeneral generalA = SoftQuaternionGeneral.FromNormalized(a);
            SoftQuaternionGeneral generalB = SoftQuaternionGeneral.FromNormalized(b);

            return Normalize(generalA * (SoftFloat.One - t) + generalB * t);
        }

        /// <summary>
        /// Returns a quaternion rotation given a forward vector and up vector.
        /// The two input vectors are assumed to be not collinear.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion LookRotation(SoftVector3 forward, SoftVector3 up)
        {
            // Third column
            SoftVector3 lookAt = SoftVector3.Normalize(forward);
            // First column
            SoftVector3 sideAxis = SoftVector3.Normalize(SoftVector3.Cross(forward, up));
            // Second column
            SoftVector3 rotatedUp = SoftVector3.Cross(lookAt, sideAxis);

            // Sum of diagonal elements
            SoftFloat trace1 = SoftFloat.One + sideAxis.X - rotatedUp.Y - lookAt.Z;
            SoftFloat trace2 = SoftFloat.One - sideAxis.X + rotatedUp.Y - lookAt.Z;
            SoftFloat trace3 = SoftFloat.One - sideAxis.X - rotatedUp.Y + lookAt.Z;
            
            // Choose largest diagonal
            if (trace1 > trace2 && trace1 > trace3) 
            { 
                SoftFloat s = SoftMath.Sqrt(trace1) * (SoftFloat)2.0f;
                return new SoftQuaternion(
                    (SoftFloat)0.25f * s,
                    (rotatedUp.X + sideAxis.Y) / s,
                    (lookAt.X + sideAxis.Z) / s,
                    (rotatedUp.Z - lookAt.Y) / s);
            }
            else if (trace2 > trace1 && trace2 > trace3)
            { 
                SoftFloat s = SoftMath.Sqrt(trace2) * (SoftFloat)2.0f;
                return new SoftQuaternion(
                    (rotatedUp.X + sideAxis.Y) / s,
                    (SoftFloat)0.25f * s,
                    (lookAt.Y + rotatedUp.Z) / s,
                    (lookAt.X - sideAxis.Z) / s);
            }
            else
            { 
                SoftFloat s = SoftMath.Sqrt(trace3) * (SoftFloat)2.0f;
                return new SoftQuaternion(
                    (lookAt.X + sideAxis.Z) / s,
                    (lookAt.Y + rotatedUp.Z) / s,
                    (SoftFloat)0.25f * s,
                    (sideAxis.Y - rotatedUp.X) / s);
            }
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
            return SoftMath.Abs(Dot(a, b)) > SoftFloat.One - epsilon;
        }

        /// <summary>
        /// Check quaternion for normalization and re-normalize it if needed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion EnsureNormalization(SoftQuaternion a)
        {
            SoftFloat lengthSqr = LengthSqr(a);

            if (SoftMath.Abs(SoftFloat.One - lengthSqr) > SoftMath.CalculationsEpsilonSqr)
            {
                return FromNormalized(a / SoftMath.Sqrt(lengthSqr));
            }

            return a;
        }

        /// <summary>
        /// Constructs normalized quaternion from normalized general one.
        /// Does not check for normalization.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SoftQuaternion FromNormalized(SoftQuaternionGeneral quaternion)
        {
            return new SoftQuaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static class Radians
        {
            /// <summary>
            /// Returns a quaternion representing a rotation around a unit axis by an angle in radians.
            /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SoftQuaternion AxisAngle(SoftVector3 axis, SoftFloat angle)
            {
                axis = SoftVector3.Normalize(axis);
                SoftFloat sin = SoftMath.Sin((SoftFloat)0.5f * angle);
                SoftFloat cos = SoftMath.Cos((SoftFloat)0.5f * angle);
                return new SoftQuaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
            }
        }

        public static class Degrees
        {
            /// <summary>
            /// Returns a quaternion representing a rotation around a unit axis by an angle in degrees.
            /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SoftQuaternion AxisAngle(SoftVector3 axis, SoftFloat angle)
            {
                axis = SoftVector3.Normalize(axis);
                SoftFloat sin = SoftMath.Sin((SoftFloat)0.5f * angle * SoftMath.Deg2Rad);
                SoftFloat cos = SoftMath.Cos((SoftFloat)0.5f * angle * SoftMath.Deg2Rad);
                return new SoftQuaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
            }
        }
    }
}