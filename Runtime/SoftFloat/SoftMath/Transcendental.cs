namespace GameLibrary.Mathematics
{
	public static partial class SoftMath
	{
		private const uint RawE = 0x402df854;
		private const uint RawLog2E = 0x3fb8aa3b;
		private const uint RawLog10E = 0x3ede5bd9;
		private const uint RawLn2 = 0x3f317218;
		private const uint RawLn10 = 0x40135d8e;

		/// <summary>
		/// The mathematical constant e also known as Euler's number. Approximately 2.718281...
		/// </summary>
		public static SoftFloat E => SoftFloat.FromRaw(RawE);

		/// <summary>
		/// The base 2 logarithm of e. Approximately 1.44...
		/// </summary>
		public static SoftFloat Log2E => SoftFloat.FromRaw(RawLog2E);

		/// <summary>
		/// The base 10 logarithm of e. Approximately 0.43...
		/// </summary>
		public static SoftFloat Log10E => SoftFloat.FromRaw(RawLog10E);

		/// <summary>
		/// The natural logarithm of 2. Approximately 0.69...
		/// </summary>
		public static SoftFloat Ln2 => SoftFloat.FromRaw(RawLn2);

		/// <summary>
		/// The natural logarithm of 10. Approximately 2.30...
		/// </summary>
		public static SoftFloat Ln10 => SoftFloat.FromRaw(RawLn10);
		
		/// <summary>
		/// Returns e raised to the power x (e ~= 2.71828182845904523536).
		/// </summary>
		public static SoftFloat Exp(SoftFloat x)
		{
			const uint ln2HiU32 = 0x3f317200; // 6.9314575195e-01
			const uint ln2Lou32 = 0x35bfbe8e; // 1.4286067653e-06
			const uint invLN2U32 = 0x3fb8aa3b; // 1.4426950216e+00

			const uint p1U32 = 0x3e2aaa8f; // 1.6666625440e-1 /*  0xaaaa8f.0p-26 */
			const uint p2U32 = 0xbb355215; // -2.7667332906e-3 /* -0xb55215.0p-32 */

			SoftFloat x1P127 = SoftFloat.FromRaw(0x7f000000); // 0x1p127f === 2 ^ 127
			SoftFloat
				x1P126 = SoftFloat.FromRaw(0x800000); // 0x1p-126f === 2 ^ -126  /*original 0x1p-149f    ??????????? */
			uint hx = x.RawValue;
			int sign = (int)(hx >> 31); /* sign bit of x */
			bool signb = sign != 0;
			hx &= 0x7fffffff; /* high word of |x| */

			/* special cases */
			if (hx >= 0x42aeac50)
			{
				/* if |x| >= -87.33655f or NaN */
				if (hx > 0x7f800000)
				{
					/* NaN */
					return x;
				}

				if (hx >= 0x42b17218 && !signb)
				{
					/* x >= 88.722839f */
					/* overflow */
					x *= x1P127;
					return x;
				}

				if (signb)
				{
					/* underflow */
					//force_eval!(-x1p_126 / x);

					if (hx >= 0x42cff1b5)
					{
						/* x <= -103.972084f */
						return SoftFloat.Zero;
					}
				}
			}

			/* argument reduction */
			int k;
			SoftFloat hi;
			SoftFloat lo;
			if (hx > 0x3eb17218)
			{
				/* if |x| > 0.5 ln2 */
				if (hx > 0x3f851592)
				{
					/* if |x| > 1.5 ln2 */
					k = (int)(SoftFloat.FromRaw(invLN2U32) * x + (signb ? (SoftFloat)0.5f : (SoftFloat)(-0.5f)));
				}
				else
				{
					k = 1 - sign - sign;
				}

				SoftFloat kf = (SoftFloat)k;
				hi = x - kf * SoftFloat.FromRaw(ln2HiU32); /* k*ln2hi is exact here */
				lo = kf * SoftFloat.FromRaw(ln2Lou32);
				x = hi - lo;
			}
			else if (hx > 0x39000000)
			{
				/* |x| > 2**-14 */
				k = 0;
				hi = x;
				lo = SoftFloat.Zero;
			}
			else
			{
				/* raise inexact */
				//force_eval!(x1p127 + x);
				return SoftFloat.One + x;
			}

			/* x is now in primary range */
			SoftFloat xx = x * x;
			SoftFloat c = x - xx * (SoftFloat.FromRaw(p1U32) + xx * SoftFloat.FromRaw(p2U32));
			SoftFloat y = SoftFloat.One + (x * c / ((SoftFloat)2.0f - c) - lo + hi);
			return k == 0 ? y : Scalbn(y, k);
		}

		/// <summary>
		/// Returns the natural logarithm (base e) of x.
		/// </summary>
		public static SoftFloat Log(SoftFloat x)
		{
			const uint ln2HiU32 = 0x3f317180; // 6.9313812256e-01
			const uint ln2Lou32 = 0x3717f7d1; // 9.0580006145e-06

			/* |(log(1+s)-log(1-s))/s - Lg(s)| < 2**-34.24 (~[-4.95e-11, 4.97e-11]). */
			const uint lg1U32 = 0x3f2aaaaa; // 0.66666662693 /*  0xaaaaaa.0p-24*/
			const uint lg2U32 = 0x3eccce13; // 0.40000972152 /*  0xccce13.0p-25 */
			const uint lg3U32 = 0x3e91e9ee; // 0.28498786688 /*  0x91e9ee.0p-25 */
			const uint lg4U32 = 0x3e789e26; // 0.24279078841 /*  0xf89e26.0p-26 */

			uint ix = x.RawValue;
			int k = 0;

			if ((ix < 0x00800000) || ((ix >> 31) != 0))
			{
				/* x < 2**-126  */
				if (ix << 1 == 0)
				{
					//return -1. / (x * x); /* log(+-0)=-inf */
					return SoftFloat.NegativeInfinity;
				}

				if ((ix >> 31) != 0)
				{
					//return (x - x) / 0.; /* log(-#) = NaN */
					return SoftFloat.NaN;
				}

				/* subnormal number, scale up x */
				SoftFloat x1P25 = SoftFloat.FromRaw(0x4c000000); // 0x1p25f === 2 ^ 25
				k -= 25;
				x *= x1P25;
				ix = x.RawValue;
			}
			else if (ix >= 0x7f800000)
			{
				return x;
			}
			else if (ix == 0x3f800000)
			{
				return SoftFloat.Zero;
			}

			/* reduce x into [sqrt(2)/2, sqrt(2)] */
			ix += 0x3f800000 - 0x3f3504f3;
			k += ((int)(ix >> 23)) - 0x7f;
			ix = (ix & 0x007fffff) + 0x3f3504f3;
			x = SoftFloat.FromRaw(ix);

			SoftFloat f = x - SoftFloat.One;
			SoftFloat s = f / ((SoftFloat)2.0f + f);
			SoftFloat z = s * s;
			SoftFloat w = z * z;
			SoftFloat t1 = w * (SoftFloat.FromRaw(lg2U32) + w * SoftFloat.FromRaw(lg4U32));
			SoftFloat t2 = z * (SoftFloat.FromRaw(lg1U32) + w * SoftFloat.FromRaw(lg3U32));
			SoftFloat r = t2 + t1;
			SoftFloat hfsq = (SoftFloat)0.5f * f * f;
			SoftFloat dk = (SoftFloat)k;

			return s * (hfsq + r) + dk * SoftFloat.FromRaw(ln2Lou32) - hfsq + f + dk * SoftFloat.FromRaw(ln2HiU32);
		}

		/// <summary>
		/// Returns the base 2 logarithm of x.
		/// </summary>
		public static SoftFloat Log2(SoftFloat x)
		{
			const uint ivln2HiU32 = 0x3fb8b000; // 1.4428710938e+00
			const uint ivln2Lou32 = 0xb9389ad4; // -1.7605285393e-04

			/* |(log(1+s)-log(1-s))/s - Lg(s)| < 2**-34.24 (~[-4.95e-11, 4.97e-11]). */
			const uint lg1U32 = 0x3f2aaaaa; // 0.66666662693 /*  0xaaaaaa.0p-24*/
			const uint lg2U32 = 0x3eccce13; // 0.40000972152 /*  0xccce13.0p-25 */
			const uint lg3U32 = 0x3e91e9ee; // 0.28498786688 /*  0x91e9ee.0p-25 */
			const uint lg4U32 = 0x3e789e26; // 0.24279078841 /*  0xf89e26.0p-26 */

			SoftFloat x1P25F = SoftFloat.FromRaw(0x4c000000); // 0x1p25f === 2 ^ 25

			uint ui = x.RawValue;
			SoftFloat hfsq;
			SoftFloat f;
			SoftFloat s;
			SoftFloat z;
			SoftFloat r;
			SoftFloat w;
			SoftFloat t1;
			SoftFloat t2;
			SoftFloat hi;
			SoftFloat lo;
			uint ix;
			int k;

			ix = ui;
			k = 0;
			if (ix < 0x00800000 || (ix >> 31) > 0)
			{
				/* x < 2**-126  */
				if (ix << 1 == 0)
				{
					//return -1. / (x * x); /* log(+-0)=-inf */
					return SoftFloat.NegativeInfinity;
				}

				if ((ix >> 31) > 0)
				{
					//return (x - x) / 0.0; /* log(-#) = NaN */
					return SoftFloat.NaN;
				}

				/* subnormal number, scale up x */
				k -= 25;
				x *= x1P25F;
				ui = x.RawValue;
				ix = ui;
			}
			else if (ix >= 0x7f800000)
			{
				return x;
			}
			else if (ix == 0x3f800000)
			{
				return SoftFloat.Zero;
			}

			/* reduce x into [sqrt(2)/2, sqrt(2)] */
			ix += 0x3f800000 - 0x3f3504f3;
			k += (int)(ix >> 23) - 0x7f;
			ix = (ix & 0x007fffff) + 0x3f3504f3;
			ui = ix;
			x = SoftFloat.FromRaw(ui);

			f = x - SoftFloat.One;
			s = f / ((SoftFloat)2.0f + f);
			z = s * s;
			w = z * z;
			t1 = w * (SoftFloat.FromRaw(lg2U32) + w * SoftFloat.FromRaw(lg4U32));
			t2 = z * (SoftFloat.FromRaw(lg1U32) + w * SoftFloat.FromRaw(lg3U32));
			r = t2 + t1;
			hfsq = (SoftFloat)0.5f * f * f;

			hi = f - hfsq;
			ui = hi.RawValue;
			ui &= 0xfffff000;
			hi = SoftFloat.FromRaw(ui);
			lo = f - hi - hfsq + s * (hfsq + r);
			return (lo + hi) * SoftFloat.FromRaw(ivln2Lou32) + lo * SoftFloat.FromRaw(ivln2HiU32) +
			       hi * SoftFloat.FromRaw(ivln2HiU32) + (SoftFloat)k;
		}

		/// <summary>
		/// Returns x raised to the power y.
		/// </summary>
		public static SoftFloat Pow(SoftFloat x, SoftFloat y)
		{
			const uint bp0U32 = 0x3f800000; /* 1.0 */
			const uint bp1U32 = 0x3fc00000; /* 1.5 */
			const uint dpH0U32 = 0x00000000; /* 0.0 */
			const uint dpH1U32 = 0x3f15c000; /* 5.84960938e-01 */
			const uint dpL0U32 = 0x00000000; /* 0.0 */
			const uint dpL1U32 = 0x35d1cfdc; /* 1.56322085e-06 */
			const uint two24U32 = 0x4b800000; /* 16777216.0 */
			const uint hugeU32 = 0x7149f2ca; /* 1.0e30 */
			const uint tinyU32 = 0x0da24260; /* 1.0e-30 */
			const uint l1U32 = 0x3f19999a; /* 6.0000002384e-01 */
			const uint l2U32 = 0x3edb6db7; /* 4.2857143283e-01 */
			const uint l3U32 = 0x3eaaaaab; /* 3.3333334327e-01 */
			const uint l4U32 = 0x3e8ba305; /* 2.7272811532e-01 */
			const uint l5U32 = 0x3e6c3255; /* 2.3066075146e-01 */
			const uint l6U32 = 0x3e53f142; /* 2.0697501302e-01 */
			const uint p1U32 = 0x3e2aaaab; /* 1.6666667163e-01 */
			const uint p2U32 = 0xbb360b61; /* -2.7777778450e-03 */
			const uint p3U32 = 0x388ab355; /* 6.6137559770e-05 */
			const uint p4U32 = 0xb5ddea0e; /* -1.6533901999e-06 */
			const uint p5U32 = 0x3331bb4c; /* 4.1381369442e-08 */
			const uint lg2U32 = 0x3f317218; /* 6.9314718246e-01 */
			const uint lg2Hu32 = 0x3f317200; /* 6.93145752e-01 */
			const uint lg2Lu32 = 0x35bfbe8c; /* 1.42860654e-06 */
			const uint ovtU32 = 0x3338aa3c; /* 4.2995665694e-08 =-(128-log2(ovfl+.5ulp)) */
			const uint cpU32 = 0x3f76384f; /* 9.6179670095e-01 =2/(3ln2) */
			const uint cpHu32 = 0x3f764000; /* 9.6191406250e-01 =12b cp */
			const uint cpLu32 = 0xb8f623c6; /* -1.1736857402e-04 =tail of cp_h */
			const uint ivln2U32 = 0x3fb8aa3b; /* 1.4426950216e+00 */
			const uint ivln2Hu32 = 0x3fb8aa00; /* 1.4426879883e+00 */
			const uint ivln2Lu32 = 0x36eca570; /* 7.0526075433e-06 */

			SoftFloat z;
			SoftFloat ax;
			SoftFloat zH;
			SoftFloat zL;
			SoftFloat pH;
			SoftFloat pL;
			SoftFloat y1;
			SoftFloat t1;
			SoftFloat t2;
			SoftFloat r;
			SoftFloat s;
			SoftFloat sn;
			SoftFloat t;
			SoftFloat u;
			SoftFloat v;
			SoftFloat w;
			int i;
			int j;
			int k;
			int yisint;
			int n;
			int hx;
			int hy;
			int ix;
			int iy;
			int iS;

			hx = (int)x.RawValue;
			hy = (int)y.RawValue;

			ix = hx & 0x7fffffff;
			iy = hy & 0x7fffffff;

			/* x**0 = 1, even if x is NaN */
			if (iy == 0)
			{
				return SoftFloat.One;
			}

			/* 1**y = 1, even if y is NaN */
			if (hx == 0x3f800000)
			{
				return SoftFloat.One;
			}

			/* NaN if either arg is NaN */
			if (ix > 0x7f800000 || iy > 0x7f800000)
			{
				return SoftFloat.NaN;
			}

			/* determine if y is an odd int when x < 0
			 * yisint = 0       ... y is not an integer
			 * yisint = 1       ... y is an odd int
			 * yisint = 2       ... y is an even int
			 */
			yisint = 0;
			if (hx < 0)
			{
				if (iy >= 0x4b800000)
				{
					yisint = 2; /* even integer y */
				}
				else if (iy >= 0x3f800000)
				{
					k = (iy >> 23) - 0x7f; /* exponent */
					j = iy >> (23 - k);
					if ((j << (23 - k)) == iy)
					{
						yisint = 2 - (j & 1);
					}
				}
			}

			/* special value of y */
			if (iy == 0x7f800000)
			{
				/* y is +-inf */
				if (ix == 0x3f800000)
				{
					/* (-1)**+-inf is 1 */
					return SoftFloat.One;
				}
				else if (ix > 0x3f800000)
				{
					/* (|x|>1)**+-inf = inf,0 */
					return hy >= 0 ? y : SoftFloat.Zero;
				}
				else
				{
					/* (|x|<1)**+-inf = 0,inf */
					return hy >= 0 ? SoftFloat.Zero : -y;
				}
			}

			if (iy == 0x3f800000)
			{
				/* y is +-1 */
				return hy >= 0 ? x : SoftFloat.One / x;
			}

			if (hy == 0x40000000)
			{
				/* y is 2 */
				return x * x;
			}

			if (hy == 0x3f000000
			    /* y is  0.5 */
			    && hx >= 0)
			{
				/* x >= +0 */
				return Sqrt(x);
			}

			ax = Abs(x);
			/* special value of x */
			if (ix == 0x7f800000 || ix == 0 || ix == 0x3f800000)
			{
				/* x is +-0,+-inf,+-1 */
				z = ax;
				if (hy < 0)
				{
					/* z = (1/|x|) */
					z = SoftFloat.One / z;
				}

				if (hx < 0)
				{
					if (((ix - 0x3f800000) | yisint) == 0)
					{
						z = (z - z) / (z - z); /* (-1)**non-int is NaN */
					}
					else if (yisint == 1)
					{
						z = -z; /* (x<0)**odd = -(|x|**odd) */
					}
				}

				return z;
			}

			sn = SoftFloat.One; /* sign of result */
			if (hx < 0)
			{
				if (yisint == 0)
				{
					/* (x<0)**(non-int) is NaN */
					//return (x - x) / (x - x);
					return SoftFloat.NaN;
				}

				if (yisint == 1)
				{
					/* (x<0)**(odd int) */
					sn = -SoftFloat.One;
				}
			}

			/* |y| is HUGE */
			if (iy > 0x4d000000)
			{
				/* if |y| > 2**27 */
				/* over/underflow if x is not close to one */
				if (ix < 0x3f7ffff8)
				{
					return hy < 0
						? sn * SoftFloat.FromRaw(hugeU32) * SoftFloat.FromRaw(hugeU32)
						: sn * SoftFloat.FromRaw(tinyU32) * SoftFloat.FromRaw(tinyU32);
				}

				if (ix > 0x3f800007)
				{
					return hy > 0
						? sn * SoftFloat.FromRaw(hugeU32) * SoftFloat.FromRaw(hugeU32)
						: sn * SoftFloat.FromRaw(tinyU32) * SoftFloat.FromRaw(tinyU32);
				}

				/* now |1-x| is TINY <= 2**-20, suffice to compute
				log(x) by x-x^2/2+x^3/3-x^4/4 */
				t = ax - SoftFloat.One; /* t has 20 trailing zeros */
				w = (t * t) * (SoftFloat.FromRaw(0x3f000000) -
				               t * (SoftFloat.FromRaw(0x3eaaaaab) - t * SoftFloat.FromRaw(0x3e800000)));
				u = SoftFloat.FromRaw(ivln2Hu32) * t; /* IVLN2_H has 16 sig. bits */
				v = t * SoftFloat.FromRaw(ivln2Lu32) - w * SoftFloat.FromRaw(ivln2U32);
				t1 = u + v;
				iS = (int)t1.RawValue;
				t1 = SoftFloat.FromRaw((uint)iS & 0xfffff000);
				t2 = v - (t1 - u);
			}
			else
			{
				SoftFloat s2;
				SoftFloat sH;
				SoftFloat sL;
				SoftFloat tH;
				SoftFloat tL;

				n = 0;
				/* take care subnormal number */
				if (ix < 0x00800000)
				{
					ax *= SoftFloat.FromRaw(two24U32);
					n -= 24;
					ix = (int)ax.RawValue;
				}

				n += ((ix) >> 23) - 0x7f;
				j = ix & 0x007fffff;
				/* determine interval */
				ix = j | 0x3f800000; /* normalize ix */
				if (j <= 0x1cc471)
				{
					/* |x|<sqrt(3/2) */
					k = 0;
				}
				else if (j < 0x5db3d7)
				{
					/* |x|<sqrt(3)   */
					k = 1;
				}
				else
				{
					k = 0;
					n += 1;
					ix -= 0x00800000;
				}

				ax = SoftFloat.FromRaw((uint)ix);

				/* compute s = s_h+s_l = (x-1)/(x+1) or (x-1.5)/(x+1.5) */
				u = ax - SoftFloat.FromRaw(k == 0 ? bp0U32 : bp1U32); /* bp[0]=1.0, bp[1]=1.5 */
				v = SoftFloat.One / (ax + SoftFloat.FromRaw(k == 0 ? bp0U32 : bp1U32));
				s = u * v;
				sH = s;
				iS = (int)sH.RawValue;
				sH = SoftFloat.FromRaw((uint)iS & 0xfffff000);

				/* t_h=ax+bp[k] High */
				iS = (int)((((uint)ix >> 1) & 0xfffff000) | 0x20000000);
				tH = SoftFloat.FromRaw((uint)iS + 0x00400000 + (((uint)k) << 21));
				tL = ax - (tH - SoftFloat.FromRaw(k == 0 ? bp0U32 : bp1U32));
				sL = v * ((u - sH * tH) - sH * tL);

				/* compute log(ax) */
				s2 = s * s;
				r = s2 * s2 * (SoftFloat.FromRaw(l1U32) + s2 * (SoftFloat.FromRaw(l2U32) +
				                                                s2 * (SoftFloat.FromRaw(l3U32) +
				                                                      s2 * (SoftFloat.FromRaw(l4U32) +
				                                                            s2 * (SoftFloat.FromRaw(l5U32) +
					                                                            s2 * SoftFloat.FromRaw(l6U32))))));
				r += sL * (sH + s);
				s2 = sH * sH;
				tH = SoftFloat.FromRaw(0x40400000) + s2 + r;
				iS = (int)tH.RawValue;
				tH = SoftFloat.FromRaw((uint)iS & 0xfffff000);
				tL = r - ((tH - SoftFloat.FromRaw(0x40400000)) - s2);

				/* u+v = s*(1+...) */
				u = sH * tH;
				v = sL * tH + tL * s;

				/* 2/(3log2)*(s+...) */
				pH = u + v;
				iS = (int)pH.RawValue;
				pH = SoftFloat.FromRaw((uint)iS & 0xfffff000);
				pL = v - (pH - u);
				zH = SoftFloat.FromRaw(cpHu32) * pH; /* cp_h+cp_l = 2/(3*log2) */
				zL = SoftFloat.FromRaw(cpLu32) * pH + pL * SoftFloat.FromRaw(cpU32) +
				     SoftFloat.FromRaw(k == 0 ? dpL0U32 : dpL1U32);

				/* log2(ax) = (s+..)*2/(3*log2) = n + dp_h + z_h + z_l */
				t = (SoftFloat)n;
				t1 = ((zH + zL) + SoftFloat.FromRaw(k == 0 ? dpH0U32 : dpH1U32)) + t;
				iS = (int)t1.RawValue;
				t1 = SoftFloat.FromRaw((uint)iS & 0xfffff000);
				t2 = zL - (((t1 - t) - SoftFloat.FromRaw(k == 0 ? dpH0U32 : dpH1U32)) - zH);
			}

            /* split up y into y1+y2 and compute (y1+y2)*(t1+t2) */
			iS = (int)y.RawValue;
			y1 = SoftFloat.FromRaw((uint)iS & 0xfffff000);
			pL = (y - y1) * t1 + y * t2;
			pH = y1 * t1;
			z = pL + pH;
			j = (int)z.RawValue;
			if (j > 0x43000000)
			{
				/* if z > 128 */
				return sn * SoftFloat.FromRaw(hugeU32) * SoftFloat.FromRaw(hugeU32); /* overflow */
			}
			else if (j == 0x43000000)
			{
				/* if z == 128 */
				if (pL + SoftFloat.FromRaw(ovtU32) > z - pH)
				{
					return sn * SoftFloat.FromRaw(hugeU32) * SoftFloat.FromRaw(hugeU32); /* overflow */
				}
			}
			else if ((j & 0x7fffffff) > 0x43160000)
			{
				/* z < -150 */
				// FIXME: check should be  (uint32_t)j > 0xc3160000
				return sn * SoftFloat.FromRaw(tinyU32) * SoftFloat.FromRaw(tinyU32); /* underflow */
			}
			else if ((uint)j == 0xc3160000
			         /* z == -150 */
			         && pL <= z - pH)
			{
				return sn * SoftFloat.FromRaw(tinyU32) * SoftFloat.FromRaw(tinyU32); /* underflow */
			}

			/*
			 * compute 2**(p_h+p_l)
			 */
			i = j & 0x7fffffff;
			k = (i >> 23) - 0x7f;
			n = 0;
			if (i > 0x3f000000)
			{
				/* if |z| > 0.5, set n = [z+0.5] */
				n = j + (0x00800000 >> (k + 1));
				k = ((n & 0x7fffffff) >> 23) - 0x7f; /* new k for n */
				t = SoftFloat.FromRaw((uint)n & ~(0x007fffffu >> k));
				n = ((n & 0x007fffff) | 0x00800000) >> (23 - k);
				if (j < 0)
				{
					n = -n;
				}

				pH -= t;
			}

			t = pL + pH;
			iS = (int)t.RawValue;
			t = SoftFloat.FromRaw((uint)iS & 0xffff8000);
			u = t * SoftFloat.FromRaw(lg2Hu32);
			v = (pL - (t - pH)) * SoftFloat.FromRaw(lg2U32) + t * SoftFloat.FromRaw(lg2Lu32);
			z = u + v;
			w = v - (z - u);
			t = z * z;
			t1 = z - t * (SoftFloat.FromRaw(p1U32) + t * (SoftFloat.FromRaw(p2U32) +
			                                              t * (SoftFloat.FromRaw(p3U32) +
			                                                   t * (SoftFloat.FromRaw(p4U32) +
			                                                        t * SoftFloat.FromRaw(p5U32)))));
			r = (z * t1) / (t1 - SoftFloat.FromRaw(0x40000000)) - (w + z * w);
			z = SoftFloat.One - (r - z);
			j = (int)z.RawValue;
			j += n << 23;
			if ((j >> 23) <= 0)
			{
				/* subnormal output */
				z = Scalbn(z, n);
			}
			else
			{
				z = SoftFloat.FromRaw((uint)j);
			}

			return sn * z;
		}

		private static SoftFloat Scalbn(SoftFloat x, int n)
		{
			SoftFloat x1P127 = SoftFloat.FromRaw(0x7f000000); // 0x1p127f === 2 ^ 127
			SoftFloat x1P126 = SoftFloat.FromRaw(0x800000); // 0x1p-126f === 2 ^ -126
			SoftFloat x1P24 = SoftFloat.FromRaw(0x4b800000); // 0x1p24f === 2 ^ 24

			if (n > 127)
			{
				x *= x1P127;
				n -= 127;
				if (n > 127)
				{
					x *= x1P127;
					n -= 127;
					if (n > 127)
					{
						n = 127;
					}
				}
			}
			else if (n < -126)
			{
				x *= x1P126 * x1P24;
				n += 126 - 24;
				if (n < -126)
				{
					x *= x1P126 * x1P24;
					n += 126 - 24;
					if (n < -126)
					{
						n = -126;
					}
				}
			}

			return x * SoftFloat.FromRaw(((uint)(0x7f + n)) << 23);
		}
	}
}
