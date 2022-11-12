using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    public readonly struct SoftQuaternion
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
        /// The composite rotation of two quaternions.
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
        /// Rotating vector by the specified quaternion.
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
        /// Returns true if quaternions are equal, false otherwise.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(SoftQuaternion a, SoftQuaternion b) => ApproximatelyEqual(a, b, SoftFloat.Epsilon);

        /// <summary>
        /// Returns true if quaternions are not equal, false otherwise.
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
        /// Compares two quaternions with some epsilon and returns true if they are similar.
        /// </summary>
        public static bool ApproximatelyEqual(SoftQuaternion a, SoftQuaternion b, SoftFloat epsilon)
        {
            return Dot(a, b) > SoftFloat.One - epsilon;
        }
    }
}