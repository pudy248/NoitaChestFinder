using System.Runtime.InteropServices;

namespace GCFinder;

public class NoitaRandom
{
	public NoitaRandom(uint worldSeed)
	{
		SetWorldSeed(worldSeed);
	}

	[StructLayout(LayoutKind.Explicit)]
	struct DLUnion
	{
		[FieldOffset(0)]
		public ulong Long;
		[FieldOffset(0)]
		public double Double;


		public static implicit operator DLUnion(ulong input)
		{
			DLUnion ret = new DLUnion
			{
				Long = input
			};
			return ret;
		}

		public static implicit operator DLUnion(double input)
		{
			DLUnion ret = new DLUnion
			{
				Double = input
			};
			return ret;
		}

		public static implicit operator ulong(DLUnion input)
		{
			return input.Long;
		}

		public static implicit operator double(DLUnion input)
		{
			return input.Double;
		}
	}

	public uint world_seed = 0;

	ulong SetRandomSeedHelper(double r)
	{
		ulong e = (DLUnion)r;

		if (((e >> 0x20 & 0x7fffffff) < 0x7ff00000) && (-9.223372036854776e+18 <= r) && (r < 9.223372036854776e+18))
		{
			e <<= 1;
			e >>= 1;
			double s = (DLUnion)e;
			ulong i = 0;
			if (s != 0.0)
			{
				ulong f = (e & 0xfffffffffffff) | 0x0010000000000000;
				ulong g = 0x433 - (e >> 0x34);
				ulong h = f >> (int)g;

				uint j = ~(uint)(0x433 < (((e >> 0x20) & 0xffffffff) >> 0x14) ? 1 : 0) + 1;
				i = (ulong)j << 0x20 | j;
				i = ~i & h | f << (((int)s >> 0x34) - 0x433) & i;
				i = ~(~(uint)(r == s ? 1 : 0) + 1) & (~i + 1) | i & (~(uint)(r == s ? 1 : 0) + 1);
			}
			return i & 0xffffffff;
		}

		// error!
		throw new Exception("Error in SetRandomSeedHelper");
	}

	uint SetRandomSeedHelper2(uint a, uint b, uint ws)
	{
		uint uVar1;
		uint uVar2;
		uint uVar3;

		uVar2 = (a - b) - ws ^ ws >> 0xd;
		uVar1 = (b - uVar2) - ws ^ uVar2 << 8;
		uVar3 = (ws - uVar2) - uVar1 ^ uVar1 >> 0xd;
		uVar2 = (uVar2 - uVar1) - uVar3 ^ uVar3 >> 0xc;
		uVar1 = (uVar1 - uVar2) - uVar3 ^ uVar2 << 0x10;
		uVar3 = (uVar3 - uVar2) - uVar1 ^ uVar1 >> 5;
		uVar2 = (uVar2 - uVar1) - uVar3 ^ uVar3 >> 3;
		uVar1 = (uVar1 - uVar2) - uVar3 ^ uVar2 << 10;
		return (uVar3 - uVar2) - uVar1 ^ uVar1 >> 0xf;
	}

	public double Seed;

	public uint H2(uint a, uint b, uint ws)
	{
		uint v3;
		uint v4;
		uint v5;
		int v6;
		uint v7;
		uint v8;
		int v9;

		v3 = (ws >> 13) ^ (b - a - ws);
		v4 = (v3 << 8) ^ (a - v3 - ws);
		v5 = (v4 >> 13) ^ (ws - v3 - v4);
		v6 = (int)((v5 >> 12) ^ (v3 - v4 - v5));
		v7 = (uint)(v6 << 16) ^ (uint)(v4 - v6 - v5);
		v8 = (v7 >> 5) ^ (uint)(v5 - v6 - v7);
		v9 = (int)((v8 >> 3) ^ (uint)(v6 - v7 - v8));
		return (((uint)(v9 << 10) ^ (uint)(v7 - v9 - v8)) >> 15) ^ (uint)(v8 - v9 - ((uint)(v9 << 10) ^ (uint)(v7 - v9 - v8)));
	}

	public void SetRandomFromWorldSeed()
	{
		Seed = world_seed;
		if (2147483647.0 <= Seed)
		{
			Seed = world_seed * 0.5;
		}
	}

	public void SetRandomSeed(double x, double y)
	{
		uint ws = world_seed;
		uint a = ws ^ 0x93262e6f;
		uint b = a & 0xfff;
		uint c = (a >> 0xc) & 0xfff;

		double x_ = x + b;

		double y_ = y + c;

		double r = x_ * 134217727.0;
		ulong e = SetRandomSeedHelper(r);

		ulong _x = (ulong)(DLUnion)x_ & 0x7fffffffffffffff;
		ulong _y = (ulong)(DLUnion)y_ & 0x7fffffffffffffff;
		if (102400.0 <= (double)(DLUnion)_y || (double)(DLUnion)_x <= 1.0)
		{
			r = y_ * 134217727.0;
		}
		else
		{
			double y__ = y_ * 3483.328;
			double t = e;
			y__ += t;
			y_ *= y__;
			r = y_;
		}

		ulong f = SetRandomSeedHelper(r);

		uint g = SetRandomSeedHelper2((uint)e, (uint)f, ws);
		double s = g;
		s /= 4294967295.0;
		s *= 2147483639.0;
		s += 1.0;

		if (2147483647.0 <= s)
		{
			s *= 0.5;
		}

		Seed = s;

		Next();

		uint h = ws & 3;
		while (h > 0)
		{
			Next();
			h--;
		}
	}

	public uint NextU()
	{
		Next();
		return (uint)((Seed * 4.656612875e-10) * 2147483645.0);
	}

	public double Next()
	{
		int v4 = (int)Seed * 0x41a7 + ((int)Seed / 0x1f31d) * -0x7fffffff;
		if (v4 < 0)
		{
			v4 += 0x7fffffff;
		}
		Seed = v4;
		return Seed / 0x7fffffff;
	}

	public int Random(int a, int b)
	{
		return a + (int)((b + 1 - a) * Next());
	}

	public void SetWorldSeed(uint worldseed)
	{
		world_seed = worldseed;
	}

	public float GetDistribution(float mean, float sharpness, float baseline)
	{
		int i = 0;
		do
		{
			float r1 = (float)Next();
			float r2 = (float)Next();
			float div = MathF.Abs(r1 - mean);
			if (r2 < ((1.0 - div) * baseline))
			{
				return r1;
			}
			if (div < 0.5)
			{
				// double v11 = sin(((0.5f - mean) + r1) * M_PI);
				float v11 = MathF.Sin(((0.5f - mean) + r1) * 3.1415f);
				float v12 = MathF.Pow(v11, sharpness);
				if (v12 > r2)
				{
					return r1;
				}
			}
			i++;
		} while (i < 100);
		return (float)Next();
	}

	public int RandomDistribution(int min, int max, int mean, float sharpness)
	{
		if (sharpness == 0)
		{
			return Random(min, max);
		}

		float adjMean = (mean - min) / (float)(max - min);
		float v7 = GetDistribution(adjMean, sharpness, 0.005f); // Baseline is always this
		int d = (int)MathF.Round((max - min) * v7);
		return min + d;
	}

	public int RandomDistribution(float min, float max, float mean, float sharpness)
	{
		return RandomDistribution((int)min, (int)max, (int)mean, (int)sharpness);
	}

	public float RandomDistributionf(float min, float max, float mean, float sharpness)
	{
		if (sharpness == 0.0)
		{
			float r = (float)Next();
			return (r * (max - min)) + min;
		}
		float adjMean = (mean - min) / (max - min);
		return min + (max - min) * GetDistribution(adjMean, sharpness, 0.005f); // Baseline is always this
	}
	
	public float ProceduralRandomf(double x, double y, double a, double b)
	{
		SetRandomSeed(x, y);
		return (float)(a + ((b - a) * Next()));
	}

	public int ProceduralRandomi(double x, double y, double a, double b)
	{
		SetRandomSeed(x, y);
		return Random((int)a, (int)b);
	}
}
