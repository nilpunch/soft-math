namespace GameLibrary.Mathematics
{
	/// <summary>
    /// This is bitwise implementation of float operations. Use <see cref="SoftMath"/> instead.
    /// </summary>
	public static class SoftMathTrigonometry
	{
		public const uint RawPI = 0x40490fdb; // 3.1415926535897932384626433832795
    	public const uint RawHalfPI = 0x3fc90fdb; // 1.5707963267948966192313216916398
    	public const uint RawTwoPI = 0x40c90fdb; // 6.283185307179586476925286766559
    	public const uint RawPIOver4 = 0x3f490fdb; // 0.78539816339744830961566084581988
    	public const uint RawPITimes3Over4 = 0x4016cbe4; // 2.3561944901923449288469825374596
    	public const uint RawDeg2Rad = 0x3c8efa35; // 0.017453292
    	public const uint RawRad2Deg = 0x42652ee1; // // 57.29578049

		/// <summary>
		/// Returns the sine of x.
		/// </summary>
		public static SoftFloat Sin(SoftFloat x)
		{
			const uint piSquaredTimesFive = 0x42456460; // 49.348022005446793094172454999381

			// https://en.wikipedia.org/wiki/Bhaskara_I%27s_sine_approximation_formula
			// sin(x) ~= (16x * (pi - x)) / (5pi^2 - 4x * (pi - x)) if 0 <= x <= pi

			// move x into range
			x %= SoftFloat.FromRaw(RawTwoPI);
			if (SoftFloat.IsNegative(x))
			{
				x += SoftFloat.FromRaw(RawTwoPI);
			}

			bool negate;
			if (x > SoftFloat.FromRaw(RawPI))
			{
				// pi < x <= 2pi, we need to move x to the 0 <= x <= pi range
				// also, we need to negate the result before returning it
				x = SoftFloat.FromRaw(RawTwoPI) - x;
				negate = true;
			}
			else
			{
				negate = false;
			}

			SoftFloat piMinusX = SoftFloat.FromRaw(RawPI) - x;
			SoftFloat result = ((SoftFloat)16.0f * x * piMinusX) /
			                   (SoftFloat.FromRaw(piSquaredTimesFive) - (SoftFloat)4.0f * x * piMinusX);
			return negate ? -result : result;
		}

		/// <summary>
		/// Returns the cosine of x.
		/// </summary>
		public static SoftFloat Cos(SoftFloat x) => Sin(x + SoftFloat.FromRaw(RawHalfPI));

		/// <summary>
		/// Returns the tangent of x.
		/// </summary>
		public static SoftFloat Tan(SoftFloat x) => Sin(x) / Cos(x);

		/// <summary>
		/// Returns the square root of (x*x + y*y).
		/// </summary>
		public static SoftFloat Hypot(SoftFloat x, SoftFloat y)
		{
			SoftFloat w;

			int ha = (int)x.RawValue;
			ha &= 0x7fffffff;

			int hb = (int)y.RawValue;
			hb &= 0x7fffffff;

			if (hb > ha)
			{
				int temp = ha;
				ha = hb;
				hb = temp;
			}

			SoftFloat a = SoftFloat.FromRaw((uint)ha); /* a <- |a| */
			SoftFloat b = SoftFloat.FromRaw((uint)hb); /* b <- |b| */

			if (ha - hb > 0xf000000)
			{
				return a + b;
			} /* x/y > 2**30 */

			uint k = 0;
			if (ha > 0x58800000)
			{
				/* a>2**50 */
				if (ha >= 0x7f800000)
				{
					/* Inf or NaN */
					w = a + b; /* for sNaN */
					if (ha == 0x7f800000)
					{
						w = a;
					}

					if (hb == 0x7f800000)
					{
						w = b;
					}

					return w;
				}

				/* scale a and b by 2**-60 */
				ha -= 0x5d800000;
				hb -= 0x5d800000;
				k += 60;
				a = SoftFloat.FromRaw((uint)ha);
				b = SoftFloat.FromRaw((uint)hb);
			}

			if (hb < 0x26800000)
			{
				/* b < 2**-50 */
				if (hb <= 0x007fffff)
				{
					/* subnormal b or 0 */
					if (hb == 0)
					{
						return a;
					}

					SoftFloat t1 = SoftFloat.FromRaw(0x3f000000); /* t1=2^126 */
					b *= t1;
					a *= t1;
					k -= 126;
				}
				else
				{
					/* scale a and b by 2^60 */
					ha += 0x5d800000; /* a *= 2^60 */
					hb += 0x5d800000; /* b *= 2^60 */
					k -= 60;
					a = SoftFloat.FromRaw((uint)ha);
					b = SoftFloat.FromRaw((uint)hb);
				}
			}

			/* medium size a and b */
			w = a - b;
			if (w > b)
			{
				SoftFloat t1 = SoftFloat.FromRaw(((uint)ha) & 0xfffff000);
				SoftFloat t2 = a - t1;
				w = SoftMathArithmetic.Sqrt(t1 * t1 - (b * (-b) - t2 * (a + t1)));
			}
			else
			{
				a += a;
				SoftFloat y1 = SoftFloat.FromRaw(((uint)hb) & 0xfffff000);
				SoftFloat y2 = b - y1;
				SoftFloat t1 = SoftFloat.FromRaw(((uint)ha) + 0x00800000);
				SoftFloat t2 = a - t1;
				w = SoftMathArithmetic.Sqrt(t1 * y1 - (w * (-w) - (t1 * y2 + t2 * b)));
			}

			if (k != 0)
			{
				SoftFloat t1 = SoftFloat.FromRaw(0x3f800000 + (k << 23));
				return t1 * w;
			}
			else
			{
				return w;
			}
        }


		private static readonly uint[] s_atanHi = new uint[4]
		{
			0x3eed6338, // 4.6364760399e-01, /* atan(0.5)hi */
			0x3f490fda, // 7.8539812565e-01, /* atan(1.0)hi */
			0x3f7b985e, // 9.8279368877e-01, /* atan(1.5)hi */
			0x3fc90fda, // 1.5707962513e+00, /* atan(inf)hi */
		};

		private static readonly uint[] s_atanLO = new uint[4]
		{
			0x31ac3769, // 5.0121582440e-09, /* atan(0.5)lo */
			0x33222168, // 3.7748947079e-08, /* atan(1.0)lo */
			0x33140fb4, // 3.4473217170e-08, /* atan(1.5)lo */
			0x33a22168, // 7.5497894159e-08, /* atan(inf)lo */
		};

		private static readonly uint[] s_aT = new uint[5]
		{
			0x3eaaaaa9, // 3.3333328366e-01
			0xbe4cca98, // -1.9999158382e-01
			0x3e11f50d, // 1.4253635705e-01
			0xbdda1247, // -1.0648017377e-01
			0x3d7cac25 // 6.1687607318e-02
		};

		/// <summary>
		/// Returns the arctangent of x.
		/// </summary>
		public static SoftFloat Atan(SoftFloat x)
		{
			SoftFloat z;

			uint ix = x.RawValue;
			bool sign = (ix >> 31) != 0;
			ix &= 0x7fffffff;

			if (ix >= 0x4c800000)
			{
				/* if |x| >= 2**26 */
				if (SoftFloat.IsNaN(x))
				{
					return x;
				}

				SoftFloat x1P120 = SoftFloat.FromRaw(0x03800000); // 0x1p-120 === 2 ^ (-120)
				z = SoftFloat.FromRaw(s_atanHi[3]) + x1P120;
				return sign ? -z : z;
			}

			int id;
			if (ix < 0x3ee00000)
			{
				/* |x| < 0.4375 */
				if (ix < 0x39800000)
				{
					/* |x| < 2**-12 */
					//if (ix < 0x00800000)
					//{
					//    /* raise underflow for subnormal x */
					//    force_eval!(x * x);
					//}
					return x;
				}

				id = -1;
			}
			else
			{
				x = SoftMathArithmetic.Abs(x);
				if (ix < 0x3f980000)
				{
					/* |x| < 1.1875 */
					if (ix < 0x3f300000)
					{
						/*  7/16 <= |x| < 11/16 */
						x = ((SoftFloat)2.0f * x - (SoftFloat)1.0f) / ((SoftFloat)2.0f + x);
						id = 0;
					}
					else
					{
						/* 11/16 <= |x| < 19/16 */
						x = (x - (SoftFloat)1.0f) / (x + (SoftFloat)1.0f);
						id = 1;
					}
				}
				else if (ix < 0x401c0000)
				{
					/* |x| < 2.4375 */
					x = (x - (SoftFloat)1.5f) / ((SoftFloat)1.0f + (SoftFloat)1.5f * x);
					id = 2;
				}
				else
				{
					/* 2.4375 <= |x| < 2**26 */
					x = (SoftFloat)(-1.0f) / x;
					id = 3;
				}
			}

			;

			/* end of argument reduction */
			z = x * x;
			SoftFloat w = z * z;

			/* break sum from i=0 to 10 aT[i]z**(i+1) into odd and even poly */
			SoftFloat s1 = z * (SoftFloat.FromRaw(s_aT[0]) +
			                    w * (SoftFloat.FromRaw(s_aT[2]) + w * SoftFloat.FromRaw(s_aT[4])));
			SoftFloat s2 = w * (SoftFloat.FromRaw(s_aT[1]) + w * SoftFloat.FromRaw(s_aT[3]));
			if (id < 0)
			{
				return x - x * (s1 + s2);
			}

			z = SoftFloat.FromRaw(s_atanHi[id]) - ((x * (s1 + s2) - SoftFloat.FromRaw(s_atanLO[id])) - x);
			return sign ? -z : z;
		}

		/// <summary>
		/// Returns the signed angle between the positive x axis, and the direction (x, y).
		/// </summary>
		public static SoftFloat Atan2(SoftFloat y, SoftFloat x)
		{
			if (SoftFloat.IsNaN(x) || SoftFloat.IsNaN(y))
			{
				return x + y;
			}

			uint ix = x.RawValue;
			uint iy = y.RawValue;

			if (ix == 0x3f800000)
			{
				/* x=1.0 */
				return Atan(y);
			}

			uint m = ((iy >> 31) & 1) | ((ix >> 30) & 2); /* 2*sign(x)+sign(y) */
			ix &= 0x7fffffff;
			iy &= 0x7fffffff;

			const uint piLou32 = 0xb3bbbd2e; // -8.7422776573e-08

			/* when y = 0 */
			if (iy == 0)
			{
				switch (m)
				{
					case 0:
					case 1:
						return y; /* atan(+-0,+anything)=+-0 */
					case 2:
						return SoftFloat.FromRaw(RawPI); /* atan(+0,-anything) = pi */
					case 3:
					default:
						return -SoftFloat.FromRaw(RawPI); /* atan(-0,-anything) =-pi */
				}
			}

			/* when x = 0 */
			if (ix == 0)
			{
				return (m & 1) != 0 ? -SoftFloat.FromRaw(RawHalfPI) : SoftFloat.FromRaw(RawHalfPI);
			}

			/* when x is INF */
			if (ix == 0x7f800000)
			{
				if (iy == 0x7f800000)
				{
					switch (m)
					{
						case 0:
							return SoftFloat.FromRaw(RawPIOver4); /* atan(+INF,+INF) */
						case 1:
							return -SoftFloat.FromRaw(RawPIOver4); /* atan(-INF,+INF) */
						case 2:
							return SoftFloat.FromRaw(RawPITimes3Over4); /* atan(+INF,-INF)*/
						case 3:
						default:
							return -SoftFloat.FromRaw(RawPITimes3Over4); /* atan(-INF,-INF)*/
					}
				}
				else
				{
					switch (m)
					{
						case 0:
							return SoftFloat.Zero; /* atan(+...,+INF) */
						case 1:
							return -SoftFloat.Zero; /* atan(-...,+INF) */
						case 2:
							return SoftFloat.FromRaw(RawPI); /* atan(+...,-INF) */
						case 3:
						default:
							return -SoftFloat.FromRaw(RawPI); /* atan(-...,-INF) */
					}
				}
			}

			/* |y/x| > 0x1p26 */
			if (ix + (26 << 23) < iy || iy == 0x7f800000)
			{
				return (m & 1) != 0 ? -SoftFloat.FromRaw(RawHalfPI) : SoftFloat.FromRaw(RawHalfPI);
			}

			/* z = atan(|y/x|) with correct underflow */
			SoftFloat z = (m & 2) != 0 && iy + (26 << 23) < ix
				? SoftFloat.Zero /*|y/x| < 0x1p-26, x < 0 */
				: Atan(SoftMathArithmetic.Abs(y / x));

			switch (m)
			{
				case 0:
					return z; /* atan(+,+) */
				case 1:
					return -z; /* atan(-,+) */
				case 2:
					return SoftFloat.FromRaw(RawPI) - (z - SoftFloat.FromRaw(piLou32)); /* atan(+,-) */
				case 3:
				default:
					return (z - SoftFloat.FromRaw(piLou32)) - SoftFloat.FromRaw(RawPI); /* atan(-,-) */
			}
		}

		/// <summary>
		/// Returns the arccosine of x.
		/// </summary>
		public static SoftFloat Acos(SoftFloat x)
		{
			const uint pio2HiU32 = 0x3fc90fda; // 1.5707962513e+00
			const uint pio2Lou32 = 0x33a22168; // 7.5497894159e-08
			const uint pS0U32 = 0x3e2aaa75; // 1.6666586697e-01
			const uint pS1U32 = 0xbd2f13ba; // -4.2743422091e-02
			const uint pS2U32 = 0xbc0dd36b; // -8.6563630030e-03
			const uint qS1U32 = 0xbf34e5ae; // - 7.0662963390e-01

			static SoftFloat R(SoftFloat z)
			{
				SoftFloat p = z * (SoftFloat.FromRaw(pS0U32) +
				                   z * (SoftFloat.FromRaw(pS1U32) + z * SoftFloat.FromRaw(pS2U32)));
				SoftFloat q = (SoftFloat)1.0f + z * SoftFloat.FromRaw(qS1U32);
				return p / q;
			}

			SoftFloat x1P120 = SoftFloat.FromRaw(0x03800000); // 0x1p-120 === 2 ^ (-120)

			SoftFloat z;
			SoftFloat w;
			SoftFloat s;

			uint hx = x.RawValue;
			uint ix = hx & 0x7fffffff;

			/* |x| >= 1 or nan */
			if (ix >= 0x3f800000)
			{
				if (ix == 0x3f800000)
				{
					if ((hx >> 31) != 0)
					{
						return (SoftFloat)2.0f * SoftFloat.FromRaw(pio2HiU32) + x1P120;
					}

					return SoftFloat.Zero;
				}

				return SoftFloat.NaN;
			}

			/* |x| < 0.5 */
			if (ix < 0x3f000000)
			{
				if (ix <= 0x32800000)
				{
					/* |x| < 2**-26 */
					return SoftFloat.FromRaw(pio2HiU32) + x1P120;
				}

				return SoftFloat.FromRaw(pio2HiU32) - (x - (SoftFloat.FromRaw(pio2Lou32) - x * R(x * x)));
			}

			/* x < -0.5 */
			if ((hx >> 31) != 0)
			{
				z = ((SoftFloat)1.0f + x) * (SoftFloat)0.5f;
				s = SoftMathArithmetic.Sqrt(z);
				w = R(z) * s - SoftFloat.FromRaw(pio2Lou32);
				return (SoftFloat)2.0 * (SoftFloat.FromRaw(pio2HiU32) - (s + w));
			}

			/* x > 0.5 */
			z = ((SoftFloat)1.0f - x) * (SoftFloat)0.5f;
			s = SoftMathArithmetic.Sqrt(z);
			hx = s.RawValue;
			SoftFloat df = SoftFloat.FromRaw(hx & 0xfffff000);
			SoftFloat c = (z - df * df) / (s + df);
			w = R(z) * s + c;
			return (SoftFloat)2.0f * (df + w);
		}

		/// <summary>
		/// Returns the arcsine of x.
		/// </summary>
		public static SoftFloat Asin(SoftFloat x)
		{
			return SoftFloat.FromRaw(RawHalfPI) - Acos(x);
		}
	}
}
