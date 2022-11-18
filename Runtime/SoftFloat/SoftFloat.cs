using System;
using System.Runtime.CompilerServices;

namespace GameLibrary.Mathematics
{
	/// <summary>
	/// Software floating point implementation.
	/// Internal representation is identical to IEEE 754 binary32 floating point numbers.
	/// </summary>
	public readonly struct SoftFloat : IEquatable<SoftFloat>, IComparable<SoftFloat>, IComparable, IFormattable
	{
        private const uint SignMask = 0x80000000;
        private const int MantissaBits = 23;
        private const int ExponentBits = 8;
        private const int ExponentBias = 127;

        private const uint RawZero = 0;
        private const uint RawNaN = 0xFFC00000;
        private const uint RawPositiveInfinity = 0x7F800000;
        private const uint RawNegativeInfinity = RawPositiveInfinity ^ SignMask;
        private const uint RawOne = 0x3F800000;
        private const uint RawMinusOne = RawOne ^ SignMask;
        private const uint RawMaxValue = 0x7F7FFFFF;
        private const uint RawMinValue = 0x7F7FFFFF ^ SignMask;
        private const uint RawAbsoluteEpsilon = 0x00000001;
        private const uint RawNormalEpsilon = 0x00800000;
        private const uint RawLog2OfE = 0;

        private static readonly sbyte[] s_msb = new sbyte[256]
        {
            -1, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
            5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
            6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
            6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
        };

        private static readonly int[] s_normalizeAmounts = new int[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0,
            8, 8, 8, 8, 8, 8, 8, 8, 16,
            16, 16, 16, 16, 16, 16, 16,
            24, 24, 24, 24, 24, 24, 24
        };

        private static readonly int[] s_debruijn32_int = new int[64]
        {
            32, 8, 17, -1, -1, 14, -1, -1, -1, 20, -1, -1, -1, 28, -1, 18,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 26, 25, 24,
            4, 11, 23, 31, 3, 7, 10, 16, 22, 30, -1, -1, 2, 6, 13, 9,
            -1, 15, -1, 21, -1, 29, 19, -1, -1, -1, -1, -1, 1, 27, 5, 12
        };
        
        private static readonly int[] s_debruijn32_uint = new int[32]
        {
	        0, 31, 9, 30, 3, 8, 13, 29, 2, 5, 7, 21, 12, 24, 28, 19,
	        1, 10, 4, 14, 6, 22, 25, 20, 11, 15, 23, 26, 16, 27, 17, 18
        };

        /// <summary>
		/// Raw byte representation of an signed float number.
		/// </summary>
		public readonly uint RawValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		private SoftFloat(uint raw)
		{
			RawValue = raw;
		}

        public static SoftFloat Zero => new SoftFloat(RawZero);

        public static SoftFloat PositiveInfinity => new SoftFloat(RawPositiveInfinity);

        public static SoftFloat NegativeInfinity => new SoftFloat(RawNegativeInfinity);

        public static SoftFloat NaN => new SoftFloat(RawNaN);

        public static SoftFloat One => new SoftFloat(RawOne);

        public static SoftFloat MinusOne => new SoftFloat(RawMinusOne);

        public static SoftFloat MaxValue => new SoftFloat(RawMaxValue);

        public static SoftFloat MinValue => new SoftFloat(RawMinValue);

        /// <summary>
        /// The smallest positive number that can be distinguished from zero. This is the absolute lower limit of the format.
        /// </summary>
        /// <seealso cref="SoftFloat.Epsilon"/>
        public static SoftFloat AbsoluteEpsilon => new SoftFloat(RawAbsoluteEpsilon);
        
        /// <summary>
        /// The smallest positive value that can be represented as a normalized number in the format.
        /// Numbers smaller than this can be stored as subnormals, but are not held with as much precision.
        /// </summary>
        /// <seealso cref="SoftFloat.AbsoluteEpsilon"/>
        public static SoftFloat Epsilon => new SoftFloat(RawNormalEpsilon);

        private uint RawMantissa => RawValue & 0x7FFFFF;

        private int Mantissa
		{
			get
			{
				if (RawExponent != 0)
				{
					uint sign = (uint)((int)RawValue >> 31);
					return (int)(((RawMantissa | 0x800000) ^ sign) - sign);
				}
				else
				{
					uint sign = (uint)((int)RawValue >> 31);
					return (int)(((RawMantissa) ^ sign) - sign);
				}
			}
		}

        private sbyte Exponent => (sbyte)(RawExponent - ExponentBias);

        private byte RawExponent => (byte)(RawValue >> MantissaBits);


        public string ToString(string format, IFormatProvider formatProvider) =>
            ((float)this).ToString(format, formatProvider);


        public string ToString(string format) => ((float)this).ToString(format);

        public string ToString(IFormatProvider provider) => ((float)this).ToString(provider);

        public override string ToString() => ((float)this).ToString(System.Globalization.CultureInfo.InvariantCulture);

        public bool Equals(SoftFloat other)
        {
            if (RawExponent != 255)
            {
                // 0 == -0
                return (RawValue == other.RawValue) ||
                       ((RawValue & 0x7FFFFFFF) == 0) && ((other.RawValue & 0x7FFFFFFF) == 0);
            }

            if (RawMantissa == 0)
            {
                // Infinities
                return RawValue == other.RawValue;
            }

            // NaNs are equal for `Equals` (as opposed to the == operator)
            return other.RawMantissa != 0;
        }

        public override bool Equals(object obj) => obj != null && GetType() == obj.GetType() && Equals((SoftFloat)obj);

        public int CompareTo(SoftFloat other)
        {
            if (IsNaN(this) && IsNaN(other))
            {
                return 0;
            }

            uint sign1 = (uint)((int)RawValue >> 31);
            int val1 = (int)(((RawValue) ^ (sign1 & 0x7FFFFFFF)) - sign1);

            uint sign2 = (uint)((int)other.RawValue >> 31);
            int val2 = (int)(((other.RawValue) ^ (sign2 & 0x7FFFFFFF)) - sign2);
            return val1.CompareTo(val2);
        }

        public int CompareTo(object obj) => obj is SoftFloat f ? CompareTo(f) : throw new ArgumentException("obj");

        public override int GetHashCode()
        {
            if (RawValue == SignMask)
            {
                // +0 equals -0
                return 0;
            }

            if (!IsNaN(this))
            {
                return (int)RawValue;
            }

            // All NaNs are equal
            return unchecked((int)RawNaN);
        }

        /// <summary>
        /// Creates an soft float number from a float value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator SoftFloat(float f)
        {
	        uint raw = ReinterpretFloatToInt32(f);
	        return new SoftFloat(raw);
        }

        /// <summary>
        /// Converts an soft float number to a float value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float(SoftFloat f)
        {
	        uint raw = f.RawValue;
	        return ReinterpretIntToFloat32(raw);
        }

        /// <summary>
        /// Creates an soft float number from an integer.
        /// </summary>
        public static explicit operator SoftFloat(int value)
        {
	        if (value == 0)
	        {
		        return Zero;
	        }

	        if (value == int.MinValue)
	        {
		        // Special case
		        return FromRaw(0xcf000000);
	        }

	        bool negative = value < 0;
	        int u = Math.Abs(value);

	        int shifts;

	        int lzcnt = LeadingZeroesCount(u);
	        if (lzcnt < 8)
	        {
		        int count = 8 - lzcnt;
		        u >>= count;
		        shifts = -count;
	        }
	        else
	        {
		        int count = lzcnt - 8;
		        u <<= count;
		        shifts = count;
	        }

	        uint exponent = (uint)(ExponentBias + MantissaBits - shifts);
	        return FromParts(negative, exponent, (uint)u);
        }

        /// <summary>
        /// Converts an soft float number to an integer.
        /// </summary>
        public static explicit operator int(SoftFloat f)
        {
	        if (f.Exponent < 0)
	        {
		        return 0;
	        }

	        int shift = MantissaBits - f.Exponent;
	        var mantissa = (int)(f.RawMantissa | (1 << MantissaBits));
	        int value = shift < 0 ? mantissa << -shift : mantissa >> shift;
	        return IsPositive(f) ? value : -value;
        }
        
        /// <summary>
        /// Creates an soft float number from unsigned integer.
        /// </summary>
        public static explicit operator SoftFloat(uint value)
        {
	        if (value == 0)
	        {
		        return Zero;
	        }

	        int shifts;

	        int lzcnt = LeadingZeroesCount(value);
	        if (lzcnt < 8)
	        {
		        int count = 8 - lzcnt;
		        value >>= count;
		        shifts = -count;
	        }
	        else
	        {
		        int count = lzcnt - 8;
		        value <<= count;
		        shifts = count;
	        }

	        uint exponent = (uint)(ExponentBias + MantissaBits - shifts);
	        return FromParts(false, exponent, value);
        }

        /// <summary>
        /// Converts an soft float number to unsigned integer.
        /// </summary>
        public static explicit operator uint(SoftFloat f)
        {
	        if (f.Exponent < 0)
	        {
		        return 0;
	        }

	        int shift = MantissaBits - f.Exponent;
	        uint mantissa = f.RawMantissa | (1 << MantissaBits);
	        uint value = shift < 0 ? mantissa << -shift : mantissa >> shift;
	        return value;
        }

        /// <summary>
		/// Creates an soft float number from its parts: sign, exponent, mantissa.
		/// </summary>
		/// <param name="sign">Sign of the number: false = the number is positive, true = the number is negative.</param>
		/// <param name="exponent">Exponent of the number.</param>
		/// <param name="mantissa">Mantissa (significand) of the number.</param>
		/// <returns></returns>
		public static SoftFloat FromParts(bool sign, uint exponent, uint mantissa)
		{
			return FromRaw((sign ? SignMask : 0) | ((exponent & 0xff) << MantissaBits) |
			               (mantissa & ((1 << MantissaBits) - 1)));
		}

        /// <summary>
        /// Creates an soft float number from its raw byte representation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoftFloat FromRaw(uint raw)
        {
            return new SoftFloat(raw);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoftFloat operator -(SoftFloat f) => new SoftFloat(f.RawValue ^ 0x80000000);

        public static SoftFloat operator +(SoftFloat f1, SoftFloat f2)
		{
			return f1.RawExponent - f2.RawExponent >= 0 ? InternalAdd(f1, f2) : InternalAdd(f2, f1);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoftFloat operator -(SoftFloat f1, SoftFloat f2) => f1 + (-f2);

        public static SoftFloat operator *(SoftFloat f1, SoftFloat f2)
		{
			int man1;
			int rawExp1 = f1.RawExponent;
			uint sign1;
			uint sign2;
			if (rawExp1 == 0)
			{
				// SubNorm
				sign1 = (uint)((int)f1.RawValue >> 31);
				int rawMan1 = (int)f1.RawMantissa;
				if (rawMan1 == 0)
                {
                    if (IsFinite(f2))
					{
						// 0 * f2
						return new SoftFloat((f1.RawValue ^ f2.RawValue) & SignMask);
					}

                    // 0 * Infinity
                    // 0 * NaN
                    return NaN;
                }

				int shift = LeadingZeroesCount(rawMan1 & 0x00ffffff) - 8;
				rawMan1 <<= shift;
				rawExp1 = 1 - shift;

				man1 = (int)((rawMan1 ^ sign1) - sign1);
			}
			else if (rawExp1 != 255)
			{
				// Norm
				sign1 = (uint)((int)f1.RawValue >> 31);
				man1 = (int)(((f1.RawMantissa | 0x800000) ^ sign1) - sign1);
			}
			else
            {
                // Non finite
				if (f1.RawValue == RawPositiveInfinity)
				{
					if (IsZero(f2))
					{
						// Infinity * 0
						return NaN;
					}

					if (IsNaN(f2))
					{
						// Infinity * NaN
						return NaN;
					}

					if ((int)f2.RawValue >= 0)
					{
						// Infinity * f
						return PositiveInfinity;
					}

                    // Infinity * -f
                    return NegativeInfinity;
                }

                if (f1.RawValue == RawNegativeInfinity)
                {
                    if (IsZero(f2) || IsNaN(f2))
                    {
                        // -Infinity * 0
                        // -Infinity * NaN
                        return NaN;
                    }

                    if ((int)f2.RawValue < 0)
                    {
                        // -Infinity * -f
                        return PositiveInfinity;
                    }

                    // -Infinity * f
                    return NegativeInfinity;
                }
                return f1;
            }

			int man2;
			int rawExp2 = f2.RawExponent;
			if (rawExp2 == 0)
			{
				// SubNorm
				sign2 = (uint)((int)f2.RawValue >> 31);
				int rawMan2 = (int)f2.RawMantissa;
				if (rawMan2 == 0)
                {
                    if (IsFinite(f1))
					{
						// f1 * 0
						return new SoftFloat((f1.RawValue ^ f2.RawValue) & SignMask);
					}

                    // Infinity * 0
                    // NaN * 0
                    return NaN;
                }

				int shift = LeadingZeroesCount(rawMan2 & 0x00ffffff) - 8;
				rawMan2 <<= shift;
				rawExp2 = 1 - shift;

				man2 = (int)((rawMan2 ^ sign2) - sign2);
			}
			else if (rawExp2 != 255)
			{
				// Norm
				sign2 = (uint)((int)f2.RawValue >> 31);
				man2 = (int)(((f2.RawMantissa | 0x800000) ^ sign2) - sign2);
			}
			else
            {
                // Non finite
				if (f2.RawValue == RawPositiveInfinity)
				{
					if (IsZero(f1))
					{
						// 0 * Infinity
						return NaN;
					}

					if ((int)f1.RawValue >= 0)
					{
						// f * Infinity
						return PositiveInfinity;
					}

                    // -f * Infinity
                    return NegativeInfinity;
                }

                if (f2.RawValue == RawNegativeInfinity)
                {
                    if (IsZero(f1))
                    {
                        // 0 * -Infinity
                        return NaN;
                    }

                    if ((int)f1.RawValue < 0)
                    {
                        // -f * -Infinity
                        return PositiveInfinity;
                    }

                    // f * -Infinity
                    return NegativeInfinity;
                }
                return f2;
            }

			long longMan = man1 * (long)man2;
			int man = (int)(longMan >> MantissaBits);
            uint absMan = (uint)Math.Abs(man);
			int rawExp = rawExp1 + rawExp2 - ExponentBias;
			uint sign = (uint)man & 0x80000000;
			if ((absMan & 0x1000000) != 0)
			{
				absMan >>= 1;
				rawExp++;
			}

			if (rawExp >= 255)
			{
				// Overflow
				return new SoftFloat(sign ^ RawPositiveInfinity);
			}

			if (rawExp <= 0)
			{
				// Subnorms/Underflow
				if (rawExp <= -24)
				{
					return new SoftFloat(sign);
				}

				absMan >>= -rawExp + 1;
				rawExp = 0;
			}

			uint raw = sign | (uint)rawExp << MantissaBits | absMan & 0x7FFFFF;
			return new SoftFloat(raw);
		}

        public static SoftFloat operator /(SoftFloat f1, SoftFloat f2)
		{
			if (IsNaN(f1) || IsNaN(f2))
			{
				return NaN;
			}

			int man1;
			int rawExp1 = f1.RawExponent;
			uint sign1;
			uint sign2;
			if (rawExp1 == 0)
			{
				// SubNorm
				sign1 = (uint)((int)f1.RawValue >> 31);
				int rawMan1 = (int)f1.RawMantissa;
				if (rawMan1 == 0)
                {
                    if (IsZero(f2))
					{
						// 0 / 0
						return NaN;
					}

                    // 0 / f
                    return new SoftFloat((f1.RawValue ^ f2.RawValue) & SignMask);
                }

				int shift = LeadingZeroesCount(rawMan1 & 0x00ffffff) - 8;
				rawMan1 <<= shift;
				rawExp1 = 1 - shift;

				man1 = (int)((rawMan1 ^ sign1) - sign1);
			}
			else if (rawExp1 != 255)
			{
				// Norm
				sign1 = (uint)((int)f1.RawValue >> 31);
				man1 = (int)(((f1.RawMantissa | 0x800000) ^ sign1) - sign1);
			}
			else
            {
                // Non finite
				if (f1.RawValue == RawPositiveInfinity)
				{
					if (IsZero(f2))
					{
						// Infinity / 0
						return PositiveInfinity;
					}

					// +-Infinity / Infinity
					return NaN;
				}

                if (f1.RawValue == RawNegativeInfinity)
                {
                    if (IsZero(f2))
                    {
                        // -Infinity / 0
                        return NegativeInfinity;
                    }

                    // -Infinity / +-Infinity
                    return NaN;
                }
                // NaN
                return f1;
            }

			int man2;
			int rawExp2 = f2.RawExponent;
			if (rawExp2 == 0)
			{
				// SubNorm
				sign2 = (uint)((int)f2.RawValue >> 31);
				int rawMan2 = (int)f2.RawMantissa;
				if (rawMan2 == 0)
				{
					// f / 0
					return new SoftFloat(((f1.RawValue ^ f2.RawValue) & SignMask) | RawPositiveInfinity);
				}

				int shift = LeadingZeroesCount(rawMan2 & 0x00ffffff) - 8;
				rawMan2 <<= shift;
				rawExp2 = 1 - shift;

				man2 = (int)((rawMan2 ^ sign2) - sign2);
			}
			else if (rawExp2 != 255)
			{
				// Norm
				sign2 = (uint)((int)f2.RawValue >> 31);
				man2 = (int)(((f2.RawMantissa | 0x800000) ^ sign2) - sign2);
			}
			else
            {
                // Non finite
				if (f2.RawValue == RawPositiveInfinity)
				{
					if (IsZero(f1))
					{
						// 0 / Infinity
						return Zero;
					}

					if ((int)f1.RawValue >= 0)
					{
						// f / Infinity
						return PositiveInfinity;
					}

                    // -f / Infinity
                    return NegativeInfinity;
                }

                if (f2.RawValue == RawNegativeInfinity)
                {
                    if (IsZero(f1))
                    {
                        // 0 / -Infinity
                        return new SoftFloat(SignMask);
                    }

                    if ((int)f1.RawValue < 0)
                    {
                        // -f / -Infinity
                        return PositiveInfinity;
                    }

                    // f / -Infinity
                    return NegativeInfinity;
                }
                // NaN
                return f2;
            }

			long longMan = ((long)man1 << MantissaBits) / man2;
			int man = (int)longMan;
			uint absMan = (uint)Math.Abs(man);
			int rawExp = rawExp1 - rawExp2 + ExponentBias;
			uint sign = (uint)man & 0x80000000;

			if ((absMan & 0x800000) == 0)
			{
				absMan <<= 1;
				--rawExp;
			}

			if (rawExp >= 255)
			{
				// Overflow
				return new SoftFloat(sign ^ RawPositiveInfinity);
			}

			if (rawExp <= 0)
			{
				// Subnorms/Underflow
				if (rawExp <= -24)
				{
					return new SoftFloat(sign);
				}

				absMan >>= -rawExp + 1;
				rawExp = 0;
			}

			uint raw = sign | (uint)rawExp << MantissaBits | absMan & 0x7FFFFF;
			return new SoftFloat(raw);
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SoftFloat operator %(SoftFloat f1, SoftFloat f2) => SoftMath.Fmod(f1, f2);

        public static bool operator ==(SoftFloat f1, SoftFloat f2)
        {
            if (f1.RawExponent != 255)
			{
				// 0 == -0
				return f1.RawValue == f2.RawValue || (f1.RawValue & 0x7FFFFFFF) == 0 && (f2.RawValue & 0x7FFFFFFF) == 0;
			}

            if (f1.RawMantissa == 0)
            {
                // Infinities
                return f1.RawValue == f2.RawValue;
            }

            // NaNs
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SoftFloat f1, SoftFloat f2) => !(f1 == f2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(SoftFloat f1, SoftFloat f2) => !IsNaN(f1) && !IsNaN(f2) && f1.CompareTo(f2) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(SoftFloat f1, SoftFloat f2) => !IsNaN(f1) && !IsNaN(f2) && f1.CompareTo(f2) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(SoftFloat f1, SoftFloat f2) => !IsNaN(f1) && !IsNaN(f2) && f1.CompareTo(f2) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(SoftFloat f1, SoftFloat f2) => !IsNaN(f1) && !IsNaN(f2) && f1.CompareTo(f2) >= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInfinity(SoftFloat softFloat) => (softFloat.RawValue & 0x7FFFFFFF) == 0x7F800000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNegativeInfinity(SoftFloat softFloat) => softFloat.RawValue == RawNegativeInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPositiveInfinity(SoftFloat softFloat) => softFloat.RawValue == RawPositiveInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNaN(SoftFloat softFloat) => (softFloat.RawExponent == 255) && !IsInfinity(softFloat);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFinite(SoftFloat softFloat) => softFloat.RawExponent != 255;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsZero(SoftFloat softFloat) => (softFloat.RawValue & 0x7FFFFFFF) == 0;

        /// <summary>
        /// Returns true if the soft float number has a positive sign.
        /// </summary>
        /// <param name="softFloat"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPositive(SoftFloat softFloat) => (softFloat.RawValue & SignMask) == 0;

        /// <summary>
        /// Returns true if the soft float number has a negative sign.
        /// </summary>
        /// <param name="softFloat"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNegative(SoftFloat softFloat) => (softFloat.RawValue & SignMask) != 0;

        private static SoftFloat InternalAdd(SoftFloat f1, SoftFloat f2)
        {
            byte rawExp1 = f1.RawExponent;
            byte rawExp2 = f2.RawExponent;
            int deltaExp = rawExp1 - rawExp2;

            if (rawExp1 != 255)
            {
                //Finite
                if (deltaExp > 25)
                {
                    return f1;
                }

                int man1;
                int man2;
                if (rawExp2 != 0)
                {
                    // man1 = f1.Mantissa
                    // http://graphics.stanford.edu/~seander/bithacks.html#ConditionalNegate
                    uint sign1 = (uint)((int)f1.RawValue >> 31);
                    man1 = (int)(((f1.RawMantissa | 0x800000) ^ sign1) - sign1);
                    // man2 = f2.Mantissa
                    uint sign2 = (uint)((int)f2.RawValue >> 31);
                    man2 = (int)(((f2.RawMantissa | 0x800000) ^ sign2) - sign2);
                }
                else
                {
                    // Subnorm
                    // man2 = f2.Mantissa
                    uint sign2 = (uint)((int)f2.RawValue >> 31);
                    man2 = (int)((f2.RawMantissa ^ sign2) - sign2);

                    man1 = f1.Mantissa;

                    rawExp2 = 1;
                    if (rawExp1 == 0)
                    {
                        rawExp1 = 1;
                    }

                    deltaExp = rawExp1 - rawExp2;
                }

                int man = (man1 << 6) + ((man2 << 6) >> deltaExp);
                int absMan = Math.Abs(man);
                if (absMan == 0)
                {
                    return Zero;
                }

                int rawExp = rawExp1 - 6;

                int amount = s_normalizeAmounts[LeadingZeroesCount(absMan)];
                rawExp -= amount;
                absMan <<= amount;

                int msbIndex = BitScanReverse8(absMan >> MantissaBits);
                rawExp += msbIndex;
                absMan >>= msbIndex;
                if ((uint)(rawExp - 1) < 254)
                {
                    uint raw = (uint)man & 0x80000000 | (uint)rawExp << MantissaBits | ((uint)absMan & 0x7FFFFF);
                    return new SoftFloat(raw);
                }

                if (rawExp >= 255)
                {
                    // Overflow
                    return man >= 0 ? PositiveInfinity : NegativeInfinity;
                }

                if (rawExp >= -24)
                {
                    uint raw = (uint)man & 0x80000000 | (uint)(absMan >> (-rawExp + 1));
                    return new SoftFloat(raw);
                }

                return Zero;
            }
            
            // Special
            if (rawExp2 != 255)
            {
                // f1 is NaN, +Inf, -Inf and f2 is finite
                return f1;
            }

            // Both not finite
            return f1.RawValue == f2.RawValue ? f1 : NaN;
        }

        /// <summary>
        /// Returns the leading zero count of the given 32-bit integer.
        /// </summary>
        /// [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LeadingZeroesCount(int x)
        {
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;

            return s_debruijn32_int[(uint)x * 0x8c0b2891u >> 26];
        }
        
        /// <summary>Returns number of leading zeros in the binary representations of a uint value.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LeadingZeroesCount(uint x)
        {
	        if (x == 0)
	        {
		        return 32;
	        }

	        x |= x >> 1;
	        x |= x >> 2;
	        x |= x >> 4;
	        x |= x >> 8;
	        x |= x >> 16;
	        x++;

	        return s_debruijn32_uint[x * 0x076be629 >> 27];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int BitScanReverse8(int b) => s_msb[b];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint ReinterpretFloatToInt32(float f) => *(uint*)&f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe float ReinterpretIntToFloat32(uint i) => *(float*)&i;
    }
}
