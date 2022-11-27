using System;
using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    /// <summary>
    /// Normalized quaternion with unit length. Represent rotation.
    /// </summary>
    public readonly struct SoftUnitQuaternion : IEquatable<SoftUnitQuaternion>, IFormattable
    {
        public readonly SoftFloat X;
        public readonly SoftFloat Y;
        public readonly SoftFloat Z;
        public readonly SoftFloat W;

        // Constructors should be made private to maintain unit length invariant,
        // but they are made public since we can't prevent constructing quaternion with default constructor,
        // that forces length 0.
        
        /// <summary>
        /// Constructs a unit quaternion from four SoftFloat values. Use if you know what you are doing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SoftUnitQuaternion(SoftFloat x, SoftFloat y, SoftFloat z, SoftFloat w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        
        /// <summary>
        /// Constructs a unit quaternion from general quaternion. Use if you know what you are doing.
        /// Does not ensure normalization. For normalizing see <see cref="SoftUnitQuaternion.NormalizeToUnit"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SoftUnitQuaternion(SoftQuaternion quaternion)
        {
            X = quaternion.X;
            Y = quaternion.Y;
            Z = quaternion.Z;
            W = quaternion.W;
        }

        /// <summary>
        /// The identity rotation.
        /// </summary>
        public static SoftUnitQuaternion Identity => new SoftUnitQuaternion(SoftQuaternion.Identity);

        /// <summary>
        /// Returns true if the given quaternion is exactly equal to this quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is SoftUnitQuaternion softQuaternion && Equals(softQuaternion);

        /// <summary>
        /// Returns true if the given quaternion is exactly equal to this quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SoftUnitQuaternion other)
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
        /// The componentwise addition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator +(SoftUnitQuaternion a, SoftUnitQuaternion b)
        {
            return new SoftQuaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        /// <summary>
        /// The componentwise negotiation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion operator -(SoftUnitQuaternion a)
        {
            return new SoftUnitQuaternion(-a.X, -a.Y, -a.Z, -a.W);
        }

        /// <summary>
        /// The componentwise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator -(SoftUnitQuaternion a, SoftUnitQuaternion b)
        {
            return -b + a;
        }

        /// <summary>
        /// The quaternions multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion operator *(SoftUnitQuaternion a, SoftUnitQuaternion b)
        {
            return EnsureNormalization(new SoftUnitQuaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z));
        }

        /// <summary>
        /// The componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator *(SoftUnitQuaternion a, SoftFloat b)
        {
            return new SoftQuaternion(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        /// <summary>
        /// Rotate vector by the quaternion.
        /// </summary>
        public static SoftVector3 operator *(SoftUnitQuaternion unitQuaternion, SoftVector3 vector)
        {
            SoftFloat twoX = unitQuaternion.X * (SoftFloat)2f;
            SoftFloat twoY = unitQuaternion.Y * (SoftFloat)2f;
            SoftFloat twoZ = unitQuaternion.Z * (SoftFloat)2f;
            SoftFloat xx2 = unitQuaternion.X * twoX;
            SoftFloat yy2 = unitQuaternion.Y * twoY;
            SoftFloat zz2 = unitQuaternion.Z * twoZ;
            SoftFloat xy2 = unitQuaternion.X * twoY;
            SoftFloat xz2 = unitQuaternion.X * twoZ;
            SoftFloat yz2 = unitQuaternion.Y * twoZ;
            SoftFloat wx2 = unitQuaternion.W * twoX;
            SoftFloat wy2 = unitQuaternion.W * twoY;
            SoftFloat wz2 = unitQuaternion.W * twoZ;
            SoftVector3 result = new SoftVector3(
                (SoftFloat.One - (yy2 + zz2)) * vector.X + (xy2 - wz2) * vector.Y + (xz2 + wy2) * vector.Z,
                (xy2 + wz2) * vector.X + (SoftFloat.One - (xx2 + zz2)) * vector.Y + (yz2 - wx2) * vector.Z,
                (xz2 - wy2) * vector.X + (yz2 + wx2) * vector.Y + (SoftFloat.One - (xx2 + yy2)) * vector.Z);
            return result;
        }

        /// <summary>
        /// The quaternions division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion operator /(SoftUnitQuaternion a, SoftUnitQuaternion b)
        {
            return a * Inverse(b);
        }

        /// <summary>
        /// The componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator /(SoftUnitQuaternion a, SoftFloat b)
        {
            return new SoftQuaternion(a.X / b, a.Y / b, a.Z / b, a.W / b);
        }

        /// <summary>
        /// Returns true if quaternions are approximately equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SoftUnitQuaternion a, SoftUnitQuaternion b) => ApproximatelyEqual(a, b);

        /// <summary>
        /// Returns true if quaternions are not approximately equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SoftUnitQuaternion a, SoftUnitQuaternion b) => !(a == b);

        /// <summary>
        /// The dot product between two rotations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Dot(SoftUnitQuaternion a, SoftUnitQuaternion b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        /// <summary>
        /// The length of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Length(SoftUnitQuaternion a)
        {
            return SoftMath.Sqrt(LengthSqr(a));
        }

        /// <summary>
        /// The squared length of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat LengthSqr(SoftUnitQuaternion a)
        {
            return Dot(a, a);
        }

        /// <summary>
        /// The conjugate of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion Conjugate(SoftUnitQuaternion a)
        {
            return new SoftUnitQuaternion(-a.X, -a.Y, -a.Z, a.W);
        }

        /// <summary>
        /// The inverse of a quaternion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion Inverse(SoftUnitQuaternion a)
        {
            return Conjugate(a);
        }

        /// <summary>
        /// Normalize quaternion to unit quaternion. If input quaternion is zero, then returns identity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion NormalizeToUnit(SoftQuaternion a)
        {
            return new SoftUnitQuaternion(SoftQuaternion.NormalizeSafe(a, SoftQuaternion.Identity));
        }

        /// <summary>
        /// Returns a spherical interpolation between two quaternions.
        /// Non-commutative, torque-minimal, constant velocity.
        /// </summary>
        public static SoftUnitQuaternion Slerp(SoftUnitQuaternion a, SoftUnitQuaternion b, SoftFloat t, bool longPath = false)
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

            return EnsureNormalization(new SoftUnitQuaternion(a * influenceA + b * influenceB));
        }

        /// <summary>
        /// Returns a normalized componentwise interpolation between two quaternions.
        /// Commutative, torque-minimal, non-constant velocity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion Nlerp(SoftUnitQuaternion a, SoftUnitQuaternion b, SoftFloat t, bool longPath = false)
        {
            return NormalizeToUnit(SoftQuaternion.Lerp(new SoftQuaternion(a), new SoftQuaternion(b), t, longPath));
        }

        /// <summary>
        /// Returns a rotation with the specified forward and up directions.
        /// If inputs are zero length or collinear or have some other weirdness,
        /// then rotation result will be some mix of <see cref="SoftVector3.Forward"/> and <see cref="SoftVector3.Up"/> vectors.
        /// </summary>
        public static SoftUnitQuaternion LookRotation(SoftVector3 forward, SoftVector3 up)
        {
            // Third matrix column
            SoftVector3 lookAt = SoftVector3.NormalizeSafe(forward, SoftVector3.Forward);
            // First matrix column
            SoftVector3 sideAxis = SoftVector3.NormalizeSafe(SoftVector3.Cross(up, lookAt), SoftVector3.Orthonormal(lookAt));
            // Second matrix column
            SoftVector3 rotatedUp = SoftVector3.Cross(lookAt, sideAxis);

            // Sums of matrix main diagonal elements
            SoftFloat trace1 = SoftFloat.One + sideAxis.X - rotatedUp.Y - lookAt.Z;
            SoftFloat trace2 = SoftFloat.One - sideAxis.X + rotatedUp.Y - lookAt.Z;
            SoftFloat trace3 = SoftFloat.One - sideAxis.X - rotatedUp.Y + lookAt.Z;

            // If orthonormal vectors forms identity matrix, then return identity rotation
            if (trace1 + trace2 + trace3 < SoftMath.CalculationsEpsilon)
            {
                return Identity;
            }

            // Choose largest diagonal
            if (trace1 + SoftMath.CalculationsEpsilon > trace2 && trace1 + SoftMath.CalculationsEpsilon > trace3)
            {
                SoftFloat s = SoftMath.Sqrt(trace1) * (SoftFloat)2.0f;
                return new SoftUnitQuaternion(
                    (SoftFloat)0.25f * s,
                    (rotatedUp.X + sideAxis.Y) / s,
                    (lookAt.X + sideAxis.Z) / s,
                    (rotatedUp.Z - lookAt.Y) / s);
            }
            else if (trace2 + SoftMath.CalculationsEpsilon > trace1 && trace2 + SoftMath.CalculationsEpsilon > trace3)
            {
                SoftFloat s = SoftMath.Sqrt(trace2) * (SoftFloat)2.0f;
                return new SoftUnitQuaternion(
                    (rotatedUp.X + sideAxis.Y) / s,
                    (SoftFloat)0.25f * s,
                    (lookAt.Y + rotatedUp.Z) / s,
                    (lookAt.X - sideAxis.Z) / s);
            }
            else
            {
                SoftFloat s = SoftMath.Sqrt(trace3) * (SoftFloat)2.0f;
                return new SoftUnitQuaternion(
                    (lookAt.X + sideAxis.Z) / s,
                    (lookAt.Y + rotatedUp.Z) / s,
                    (SoftFloat)0.25f * s,
                    (sideAxis.Y - rotatedUp.X) / s);
            }
        }
        
        /// <summary>
        /// Returns a quaternion representing a rotation around a unit axis by an angle in radians.
        /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
        /// If input vector is zero length then rotation will be around forward axis.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion AxisAngleRadians(SoftVector3 axis, SoftFloat angle)
        {
            axis = SoftVector3.NormalizeSafe(axis, SoftVector3.Forward);
            SoftFloat sin = SoftMath.Sin((SoftFloat)0.5f * angle);
            SoftFloat cos = SoftMath.Cos((SoftFloat)0.5f * angle);
            return new SoftUnitQuaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
        }
        
        /// <summary>
        /// Returns a quaternion representing a rotation around a unit axis by an angle in degrees.
        /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
        /// /// If input vector is zero length then rotation will be around forward axis.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion AxisAngleDegrees(SoftVector3 axis, SoftFloat angle)
        {
            axis = SoftVector3.NormalizeSafe(axis, SoftVector3.Forward);
            SoftFloat sin = SoftMath.Sin((SoftFloat)0.5f * angle * SoftMath.Deg2Rad);
            SoftFloat cos = SoftMath.Cos((SoftFloat)0.5f * angle * SoftMath.Deg2Rad);
            return new SoftUnitQuaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
        }

        /// <summary>
        /// Compares two quaternions with <see cref="SoftMath.CalculationsEpsilonSqr"/> and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftUnitQuaternion a, SoftUnitQuaternion b)
        {
            return ApproximatelyEqual(a, b, SoftMath.CalculationsEpsilonSqr);
        }

        /// <summary>
        /// Compares two quaternions with some epsilon and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftUnitQuaternion a, SoftUnitQuaternion b, SoftFloat epsilon)
        {
            return SoftMath.Abs(Dot(a, b)) > SoftFloat.One - epsilon;
        }

        /// <summary>
        /// Check quaternion for normalization precision error and re-normalize it if needed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftUnitQuaternion EnsureNormalization(SoftUnitQuaternion a)
        {
            SoftFloat lengthSqr = LengthSqr(a);

            if (SoftMath.Abs(SoftFloat.One - lengthSqr) > SoftMath.CalculationsEpsilonSqr)
            {
                return new SoftUnitQuaternion(a / SoftMath.Sqrt(lengthSqr));
            }

            return a;
        }
    }
}