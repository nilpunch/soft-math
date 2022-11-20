using System;
using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    /// <summary>
    /// Unit length quaternion. Represent rotation.
    /// </summary>
    public readonly struct SoftVersor : IEquatable<SoftVersor>, IFormattable
    {
        public readonly SoftFloat X;
        public readonly SoftFloat Y;
        public readonly SoftFloat Z;
        public readonly SoftFloat W;

        /// <summary>
        /// Constructs a versor from four SoftFloat values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SoftVersor(SoftFloat x, SoftFloat y, SoftFloat z, SoftFloat w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// The identity rotation.
        /// </summary>
        public static SoftVersor Identity =>
            new SoftVersor(SoftFloat.Zero, SoftFloat.Zero, SoftFloat.Zero, SoftFloat.One);

        /// <summary>
        /// Returns true if the given versor is exactly equal to this versor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is SoftVersor softQuaternion && Equals(softQuaternion);

        /// <summary>
        /// Returns true if the given versor is exactly equal to this versor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SoftVersor other)
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
        public static SoftQuaternion operator +(SoftVersor a, SoftVersor b)
        {
            return new SoftQuaternion(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        /// <summary>
        /// The componentwise negotiation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor operator -(SoftVersor a)
        {
            return new SoftVersor(-a.X, -a.Y, -a.Z, -a.W);
        }

        /// <summary>
        /// The componentwise subtraction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator -(SoftVersor a, SoftVersor b)
        {
            return -b + a;
        }

        /// <summary>
        /// The versors multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor operator *(SoftVersor a, SoftVersor b)
        {
            return EnsureNormalization(new SoftVersor(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y + a.Y * b.W + a.Z * b.X - a.X * b.Z,
                a.W * b.Z + a.Z * b.W + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z));
        }

        /// <summary>
        /// The componentwise multiplication.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator *(SoftVersor a, SoftFloat b)
        {
            return new SoftQuaternion(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        /// <summary>
        /// Rotate vector by the versor.
        /// </summary>
        public static SoftVector3 operator *(SoftVersor versor, SoftVector3 vector)
        {
            SoftFloat twoX = versor.X * (SoftFloat)2f;
            SoftFloat twoY = versor.Y * (SoftFloat)2f;
            SoftFloat twoZ = versor.Z * (SoftFloat)2f;
            SoftFloat xx2 = versor.X * twoX;
            SoftFloat yy2 = versor.Y * twoY;
            SoftFloat zz2 = versor.Z * twoZ;
            SoftFloat xy2 = versor.X * twoY;
            SoftFloat xz2 = versor.X * twoZ;
            SoftFloat yz2 = versor.Y * twoZ;
            SoftFloat wx2 = versor.W * twoX;
            SoftFloat wy2 = versor.W * twoY;
            SoftFloat wz2 = versor.W * twoZ;
            SoftVector3 result = new SoftVector3(
                (SoftFloat.One - (yy2 + zz2)) * vector.X + (xy2 - wz2) * vector.Y + (xz2 + wy2) * vector.Z,
                (xy2 + wz2) * vector.X + (SoftFloat.One - (xx2 + zz2)) * vector.Y + (yz2 - wx2) * vector.Z,
                (xz2 - wy2) * vector.X + (yz2 + wx2) * vector.Y + (SoftFloat.One - (xx2 + yy2)) * vector.Z);
            return result;
        }

        /// <summary>
        /// The versors division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor operator /(SoftVersor a, SoftVersor b)
        {
            return a * Inverse(b);
        }

        /// <summary>
        /// The componentwise division.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftQuaternion operator /(SoftVersor a, SoftFloat b)
        {
            return new SoftQuaternion(a.X / b, a.Y / b, a.Z / b, a.W / b);
        }

        /// <summary>
        /// Returns true if versors are approximately equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SoftVersor a, SoftVersor b) => ApproximatelyEqual(a, b);

        /// <summary>
        /// Returns true if versors are not approximately equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(SoftVersor a, SoftVersor b) => !(a == b);

        /// <summary>
        /// Normalize quaternion into versor. If input quaternion is zero, then returns identity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor FromQuaternion(SoftQuaternion a)
        {
            return FromNormalizedQuaternion(SoftQuaternion.NormalizeSafe(a, SoftQuaternion.Identity));
        }
        
        /// <summary>
        /// The dot product between two rotations.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Dot(SoftVersor a, SoftVersor b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        /// <summary>
        /// The length of a versor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Length(SoftVersor a)
        {
            return SoftMath.Sqrt(LengthSqr(a));
        }

        /// <summary>
        /// The squared length of a versor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat LengthSqr(SoftVersor a)
        {
            return Dot(a, a);
        }

        /// <summary>
        /// The conjugate of a versor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor Conjugate(SoftVersor a)
        {
            return new SoftVersor(-a.X, -a.Y, -a.Z, a.W);
        }

        /// <summary>
        /// The inverse of a versor.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor Inverse(SoftVersor a)
        {
            return Conjugate(a);
        }
        
        /// <summary>
        /// Returns a spherical interpolation between two versors.
        /// Non-commutative, torque-minimal, constant velocity.
        /// </summary>
        public static SoftVersor Slerp(SoftVersor a, SoftVersor b, SoftFloat t, bool longPath = false)
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

            return EnsureNormalization(FromNormalizedQuaternion(a * influenceA + b * influenceB));
        }

        /// <summary>
        /// Returns a normalized componentwise interpolation between two versors.
        /// Commutative, torque-minimal, non-constant velocity.
        /// </summary>
        public static SoftVersor Nlerp(SoftVersor a, SoftVersor b, SoftFloat t, bool longPath = false)
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

            return FromQuaternion(a * (SoftFloat.One - t) + b * t);
        }

        /// <summary>
        /// Returns a rotation with the specified forward and up directions.
        /// If inputs are zero length or collinear or have some other weirdness,
        /// then rotation result will be some mix of <see cref="SoftVector3.Forward"/> and <see cref="SoftVector3.Up"/> vectors.
        /// </summary>
        public static SoftVersor LookRotation(SoftVector3 forward, SoftVector3 up)
        {
            // Third matrix column
            SoftVector3 lookAt = SoftVector3.NormalizeSafe(forward, SoftVector3.Forward);
            // First matrix column
            SoftVector3 sideAxis = SoftVector3.NormalizeSafe(SoftVector3.Cross(up, lookAt), SoftVector3.Orthonormalized(lookAt));
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
                return new SoftVersor(
                    (SoftFloat)0.25f * s,
                    (rotatedUp.X + sideAxis.Y) / s,
                    (lookAt.X + sideAxis.Z) / s,
                    (rotatedUp.Z - lookAt.Y) / s);
            }
            else if (trace2 + SoftMath.CalculationsEpsilon > trace1 && trace2 + SoftMath.CalculationsEpsilon > trace3)
            {
                SoftFloat s = SoftMath.Sqrt(trace2) * (SoftFloat)2.0f;
                return new SoftVersor(
                    (rotatedUp.X + sideAxis.Y) / s,
                    (SoftFloat)0.25f * s,
                    (lookAt.Y + rotatedUp.Z) / s,
                    (lookAt.X - sideAxis.Z) / s);
            }
            else
            {
                SoftFloat s = SoftMath.Sqrt(trace3) * (SoftFloat)2.0f;
                return new SoftVersor(
                    (lookAt.X + sideAxis.Z) / s,
                    (lookAt.Y + rotatedUp.Z) / s,
                    (SoftFloat)0.25f * s,
                    (sideAxis.Y - rotatedUp.X) / s);
            }
        }
        
        /// <summary>
        /// Returns a versor representing a rotation around a unit axis by an angle in radians.
        /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
        /// If input vector is zero length then rotation will be around forward axis.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor AxisAngleRadians(SoftVector3 axis, SoftFloat angle)
        {
            axis = SoftVector3.NormalizeSafe(axis, SoftVector3.Forward);
            SoftFloat sin = SoftMath.Sin((SoftFloat)0.5f * angle);
            SoftFloat cos = SoftMath.Cos((SoftFloat)0.5f * angle);
            return new SoftVersor(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
        }
        
        /// <summary>
        /// Returns a versor representing a rotation around a unit axis by an angle in degrees.
        /// The rotation direction is clockwise when looking along the rotation axis towards the origin.
        /// /// If input vector is zero length then rotation will be around forward axis.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor AxisAngleDegrees(SoftVector3 axis, SoftFloat angle)
        {
            axis = SoftVector3.NormalizeSafe(axis, SoftVector3.Forward);
            SoftFloat sin = SoftMath.Sin((SoftFloat)0.5f * angle * SoftMath.Deg2Rad);
            SoftFloat cos = SoftMath.Cos((SoftFloat)0.5f * angle * SoftMath.Deg2Rad);
            return new SoftVersor(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
        }

        /// <summary>
        /// Compares two versors with <see cref="SoftMath.CalculationsEpsilonSqr"/> and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftVersor a, SoftVersor b)
        {
            return ApproximatelyEqual(a, b, SoftMath.CalculationsEpsilonSqr);
        }

        /// <summary>
        /// Compares two versors with some epsilon and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftVersor a, SoftVersor b, SoftFloat epsilon)
        {
            return SoftMath.Abs(Dot(a, b)) > SoftFloat.One - epsilon;
        }

        /// <summary>
        /// Check versor for normalization precision error and re-normalize it if needed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftVersor EnsureNormalization(SoftVersor a)
        {
            SoftFloat lengthSqr = LengthSqr(a);

            if (SoftMath.Abs(SoftFloat.One - lengthSqr) > SoftMath.CalculationsEpsilonSqr)
            {
                return FromNormalizedQuaternion(a / SoftMath.Sqrt(lengthSqr));
            }

            return a;
        }

        /// <summary>
        /// Constructs versor from normalized quaternion.
        /// Does not check for normalization.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SoftVersor FromNormalizedQuaternion(SoftQuaternion versor)
        {
            return new SoftVersor(versor.X, versor.Y, versor.Z, versor.W);
        }
    }
}