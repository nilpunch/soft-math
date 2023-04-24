using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
    public static class SoftMath
    {
        /// <summary>
        /// The square root 2. Approximately 1.414213...
        /// </summary>
        public static SoftFloat Sqrt2 => SoftFloat.FromRaw(SoftMathArithmetic.RawSqrt2);

        /// <summary>
        /// The mathematical constant e also known as Euler's number. Approximately 2.718281...
        /// </summary>
        public static SoftFloat E => SoftFloat.FromRaw(SoftMathTranscendental.RawE);

        /// <summary>
        /// The base 2 logarithm of e. Approximately 1.44...
        /// </summary>
        public static SoftFloat Log2E => SoftFloat.FromRaw(SoftMathTranscendental.RawLog2E);

        /// <summary>
        /// The base 10 logarithm of e. Approximately 0.43...
        /// </summary>
        public static SoftFloat Log10E => SoftFloat.FromRaw(SoftMathTranscendental.RawLog10E);

        /// <summary>
        /// The natural logarithm of 2. Approximately 0.69...
        /// </summary>
        public static SoftFloat Ln2 => SoftFloat.FromRaw(SoftMathTranscendental.RawLn2);

        /// <summary>
        /// The natural logarithm of 10. Approximately 2.30...
        /// </summary>
        public static SoftFloat Ln10 => SoftFloat.FromRaw(SoftMathTranscendental.RawLn10);

        /// <summary>
        /// The ratio of a circle's circumference to its diameter. Approximately 3.141592...
        /// </summary>
        public static SoftFloat PI => SoftFloat.FromRaw(SoftMathTrigonometry.RawPI);

        /// <summary>
        /// PI / 2. Approximately 1.570798...
        /// </summary>
        public static SoftFloat HalfPI => SoftFloat.FromRaw(SoftMathTrigonometry.RawHalfPI);

        /// <summary>
        /// PI / 4. Approximately 0.785398...
        /// </summary>
        public static SoftFloat PIOver4 => SoftFloat.FromRaw(SoftMathTrigonometry.RawPIOver4);

        /// <summary>
        /// PI * 2. Approximately 6.283185...
        /// </summary>
        public static SoftFloat TwoPI => SoftFloat.FromRaw(SoftMathTrigonometry.RawTwoPI);

        /// <summary>
        /// Degrees-to-radians conversion constant.
        /// </summary>
        public static SoftFloat Deg2Rad => SoftFloat.FromRaw(SoftMathTrigonometry.RawDeg2Rad);

        /// <summary>
        /// Radians-to-degrees conversion constant.
        /// </summary>
        public static SoftFloat Rad2Deg => SoftFloat.FromRaw(SoftMathTrigonometry.RawRad2Deg);

        /// <summary>
        /// Epsilon for linear comparison operations. Approximately 1e-6.
        /// </summary>
        public static SoftFloat CalculationsEpsilon => SoftFloat.FromRaw(0x358637bd);

        /// <summary>
        /// More precise epsilon for operations with squares involved. Approximately 1e-12.
        /// </summary>
        public static SoftFloat CalculationsEpsilonSqr => SoftFloat.FromRaw(0x2b8cbccc);

        /// <summary>
        /// Returns 1, -1 depending on sign of the given soft float number.
        /// </summary>
        public static SoftFloat Sign(SoftFloat x)
        {
            if (SoftFloat.IsPositive(x))
            {
                return SoftFloat.One;
            }

            return SoftFloat.MinusOne;
        }

        /// <summary>
        /// Returns 1, -1 or 0 depending on sign and magnitude of the given soft float number. For NaN and zero returns 0.
        /// </summary>
        public static SoftFloat SignWithZero(SoftFloat x)
        {
            if (SoftFloat.IsNaN(x) || SoftFloat.IsZero(x))
            {
                return SoftFloat.Zero;
            }

            if (SoftFloat.IsPositive(x))
            {
                return SoftFloat.One;
            }

            return SoftFloat.MinusOne;
        }

        /// <summary>
        /// Returns value x with sign of y. Also applies sign from NaN and Zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat CopySign(SoftFloat x, SoftFloat y)
        {
            return SoftMathArithmetic.CopySign(x, y);
        }

        /// <summary>
        /// Returns the absolute value of the given soft float number. Leaves NaN untouched.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Abs(SoftFloat x)
        {
            return SoftMathArithmetic.Abs(x);
        }

        /// <summary>
        /// Returns the maximum of the two given soft float values. Returns NaN if either argument is NaN.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Max(SoftFloat x, SoftFloat y)
        {
            if (x > y || SoftFloat.IsNaN(x))
            {
                return x;
            }

            return y;
        }

        /// <summary>
        /// Returns the minimum of the two given soft float values. Returns NaN if either argument is NaN.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Min(SoftFloat x, SoftFloat y)
        {
            if (x < y || SoftFloat.IsNaN(x))
            {
                return x;
            }

            return y;
        }

        /// <summary>
        /// Returns the clamped value of the two given soft floats.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Clamp(SoftFloat x, SoftFloat min, SoftFloat max)
        {
            if (x < min)
            {
                return min;
            }

            if (x > max)
            {
                return max;
            }

            return x;
        }

        /// <summary>
        /// Compares two soft floats with <see cref="SoftMath.CalculationsEpsilonSqr"/> and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftFloat x, SoftFloat y)
        {
            return ApproximatelyEqual(x, y, CalculationsEpsilonSqr);
        }

        /// <summary>
        /// Compares two soft floats with some epsilon and returns true if they are similar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ApproximatelyEqual(SoftFloat x, SoftFloat y, SoftFloat epsilon)
        {
            var difference = Abs(x - y);
            return difference <= epsilon || difference <= Max(Abs(x), Abs(y)) * epsilon;
        }

        /// <summary>
        /// Returns x modulo y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Fmod(SoftFloat x, SoftFloat y)
        {
            return SoftMathArithmetic.Fmod(x, y);
        }

        /// <summary>
        /// Rounds x to the nearest integer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Round(SoftFloat x)
        {
            return SoftMathArithmetic.Round(x);
        }

        /// <summary>
        /// Rounds x down to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Floor(SoftFloat x)
        {
            return SoftMathArithmetic.Floor(x);
        }

        /// <summary>
        /// Rounds x up to the nearest integer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Ceil(SoftFloat x)
        {
            return SoftMathArithmetic.Ceil(x);
        }

        /// <summary>
        /// Truncates x, removing its fractional parts.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Trunc(SoftFloat x)
        {
            return SoftMathArithmetic.Trunc(x);
        }

        /// <summary>
        /// Returns the reverse square root of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Rsqrt(SoftFloat x)
        {
            return SoftFloat.One / SoftMathArithmetic.Sqrt(x);
        }

        /// <summary>
        /// Returns the square root of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Sqrt(SoftFloat x)
        {
            return SoftMathArithmetic.Sqrt(x);
        }

        /// <summary>
        /// Returns the remainder when dividing x by y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Remainder(SoftFloat x, SoftFloat y)
        {
            ReminderQuotient(x, y, out SoftFloat remainder, out _);
            return remainder;
        }

        /// <summary>
        /// Returns the remainder and the quotient when dividing x by y, so that x == y * quotient + remainder.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReminderQuotient(SoftFloat x, SoftFloat y, out SoftFloat remainder, out int quotient)
        {
            SoftMathArithmetic.ReminderQuotient(x, y, out remainder, out quotient);
        }

        /// <summary>
        /// Returns e raised to the power x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Exp(SoftFloat x)
        {
            return SoftMathTranscendental.Exp(x);
        }

        /// <summary>
        /// Returns the natural logarithm of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Log(SoftFloat x)
        {
            return SoftMathTranscendental.Log(x);
        }

        /// <summary>
        /// Returns the base 2 logarithm of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Log2(SoftFloat x)
        {
            return SoftMathTranscendental.Log2(x);
        }

        /// <summary>
        /// Returns x raised to the power y.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Pow(SoftFloat x, SoftFloat y)
        {
            return SoftMathTranscendental.Pow(x, y);
        }

        /// <summary>
        /// Returns the sine of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Sin(SoftFloat x)
        {
            return SoftMathTrigonometry.Sin(x);
        }

        /// <summary>
        /// Returns the cosine of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Cos(SoftFloat x)
        {
            return Sin(x + HalfPI);
        }

        /// <summary>
        /// Returns the tangent of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Tan(SoftFloat x)
        {
            return Sin(x) / Cos(x);
        }

        /// <summary>
        /// Returns the square root of (x*x + y*y).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Hypot(SoftFloat x, SoftFloat y)
        {
            return SoftMathTrigonometry.Hypot(x, y);
        }

        /// <summary>
        /// Returns the arctangent of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Atan(SoftFloat x)
        {
            return SoftMathTrigonometry.Atan(x);
        }

        /// <summary>
        /// Returns the signed angle between the positive x axis, and the direction (x, y).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Atan2(SoftFloat y, SoftFloat x)
        {
            return SoftMathTrigonometry.Atan2(y, x);
        }

        /// <summary>
        /// Returns the arccosine of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Acos(SoftFloat x)
        {
            return SoftMathTrigonometry.Acos(x);
        }

        /// <summary>
        /// Returns the arcsine of x.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat Asin(SoftFloat x)
        {
            return HalfPI - Acos(x);
        }
    }
}
