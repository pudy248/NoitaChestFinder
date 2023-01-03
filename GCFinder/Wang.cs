using System.Drawing;
using System.Runtime.InteropServices;

namespace GCFinder;

public static class Wang
{
	//program segfaults on batch sizes smaller than this. Don't know why, don't care, we'll just cap the batch size sent to the kernel.
	const uint MAGIC_NUMBER = 726;

	const ulong COLOR_PURPLE = 0x7f007f;
	const ulong COLOR_BLACK = 0x000000;
	const ulong COLOR_WHITE = 0xffffff;
	const ulong COLOR_YELLOW = 0xffff00;
	const ulong COLOR_COFFEE = 0xc0ffee;
	const ulong COLOR_01CFEE = 0x01cfee;
	const bool DEBUG = false;

	static string threadlock = "bottom text";

	public static ulong createRGB(byte r, byte g, byte b)
	{
		return (ulong)(((r & 0xff) << 16) + ((g & 0xff) << 8) + (b & 0xff));
	}

	public static ulong getPos(uint w, byte f, uint x, uint y)
	{
		return w * y * f + f * x;
	}

	public static ulong getPixelColor(byte[] map, uint w, uint x, uint y)
	{
		ulong pos = getPos(w, 3, x, y);
		byte r = map[pos];
		byte g = map[pos + 1];
		byte b = map[pos + 2];
		return createRGB(r, g, b);
	}

	public static void setPixelColor(byte[] map, uint w, uint x, uint y, ulong color)
	{
		ulong pos = getPos(w, 3, x, y);
		byte r = (byte)((color >> 16) & 0xff);
		byte g = (byte)((color >> 8) & 0xff);
		byte b = (byte)((color) & 0xff);
		map[pos] = r;
		map[pos + 1] = g;
		map[pos + 2] = b;
	}

	static void floodFill(byte[] map, uint width, uint height, uint initialX, uint initialY, ulong fromColor, ulong toColor)
	{
		Stack<(uint, uint)> s = new();
		bool[] visited = new bool[width * height + 1];

		if (initialX < 0 || initialX >= width || initialY < 0 || initialY >= height)
		{
			return;
		}

		s.Push((initialX, initialY));
		visited[getPos(width, 1, initialX, initialY)] = true;

		int filled = 0;

		while (s.Count() > 0)
		{
			(uint, uint) pos = s.Pop();
			uint x = pos.Item1;
			uint y = pos.Item2;

			setPixelColor(map, width, x, y, toColor);
			filled++;
			{
				uint nx = x - 1;
				uint ny = y;
				if (nx < 0 || nx >= width || ny < 0 || ny >= height)
				{
					return;
				}

				ulong p = getPos(width, 1, nx, ny);
				if (visited[p] == true)
				{
					return;
				}

				ulong nc = getPixelColor(map, width, nx, ny);
				if (nc != fromColor || nc == toColor)
				{
					return;
				}

				visited[p] = true;
				s.Push((nx, ny));
			}
			{
				uint nx = x + 1;
				uint ny = y;
				if (nx < 0 || nx >= width || ny < 0 || ny >= height)
				{
					return;
				}

				ulong p = getPos(width, 1, nx, ny);
				if (visited[p] == true)
				{
					return;
				}

				ulong nc = getPixelColor(map, width, nx, ny);
				if (nc != fromColor || nc == toColor)
				{
					return;
				}

				visited[p] = true;
				s.Push((nx, ny));
			}
			{
				uint nx = x;
				uint ny = y - 1;
				if (nx < 0 || nx >= width || ny < 0 || ny >= height)
				{
					return;
				}

				ulong p = getPos(width, 1, nx, ny);
				if (visited[p] == true)
				{
					return;
				}

				ulong nc = getPixelColor(map, width, nx, ny);
				if (nc != fromColor || nc == toColor)
				{
					return;
				}

				visited[p] = true;
				s.Push((nx, ny));
			}
			{
				uint nx = x;
				uint ny = y + 1;
				if (nx < 0 || nx >= width || ny < 0 || ny >= height)
				{
					return;
				}

				ulong p = getPos(width, 1, nx, ny);
				if (visited[p] == true)
				{
					return;
				}

				ulong nc = getPixelColor(map, width, nx, ny);
				if (nc != fromColor || nc == toColor)
				{
					return;
				}

				visited[p] = true;
				s.Push((nx, ny));
			}
		}
	}

	public static NoitaRandom GetRNG(int map_w, uint world_seed)
	{
		NoitaRandom rng = new NoitaRandom(world_seed);
		rng.SetRandomFromWorldSeed();
		rng.Next();
		int length = (int)((ulong)((long)map_w * -0x2e8ba2e9) >> 0x20);
		int iters = (int)(((length >> 1) - (length >> 0x1f)) * 0xb + (world_seed / 0xc) * -0xc + world_seed) + map_w;
		if (0 < iters)
		{
			do
			{
				rng.Next();
				iters -= 1;
			} while (iters != 0);
		}
		return rng;
	}


	[DllImport("WangTilerCUDA.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern IntPtr generate_block(
		byte[] tiles_data,
		uint tiles_w,
		uint tiles_h,
		uint map_w,
		uint map_h,
		bool isCoalMine,
		int worldX,
		int worldY,
		uint worldSeedStart,
		uint worldSeedCount,
		uint maxTries,
		uint pwCount,
		byte ngPlus,
		byte loggingLevel,
		uint maxChestContents,
		uint maxChestsPerWorld,
		byte greedCurse);

	[DllImport("WangTilerCUDA.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern void free_array(IntPtr block);

	static string DecodeItem(byte item, bool greed)
	{
		switch(item)
		{
			case 0:
				return "gold_nuggets";
			case 1:
				return "chest_to_gold";
			case 2:
				return "rain_gold";
			case 3:
				return "bomb";
			case 4:
				return "powder";
			case 5:
				return "potion_normal";
			case 6:
				return "potion_secret";
			case 7:
				return "potion_random_material";
			case 8:
				return "potions_pps";
			case 9:
				return "potions_ssr";
			case 10:
				return "kammi";
			case 11:
				return "kuu";
			case 12:
				return "paha_silma";
			case 13:
				return greed ? "greed_die" : "chaos_die";
			case 14:
				return greed ? "greed_orb" : "shiny_orb";
			case 15:
				return "ukkoskivi";
			case 16:
				return "kiuaskivi";
			case 17:
				return "vuoksikivi";
			case 18:
				return "kakkakikkare";
			case 19:
				return "runestone_light";
			case 20:
				return "runestone_fire";
			case 21:
				return "runestone_magma";
			case 22:
				return "runestone_weight";
			case 23:
				return "runestone_emptiness";
			case 24:
				return "runestone_edges";
			case 25:
				return "runestone_metal";
			case 26:
				return "random_spell";
			case 27:
				return "spell_refresh";
			case 28:
				return "heart_normal";
			case 29:
				return "heart_mimic";
			case 30:
				return "large_heart";
			case 31:
				return "full_heal";
			case 32:
				return "wand_T1";
			case 33:
				return "wand_T1NS";
			case 34:
				return "wand_T2";
			case 35:
				return "wand_T2NS";
			case 36:
				return "wand_T3";
			case 37:
				return "wand_T3NS";
			case 38:
				return "wand_T4";
			case 39:
				return "wand_T4NS";
			case 40:
				return "wand_T5";
			case 41:
				return "wand_T5NS";
			case 42:
				return "wand_T6";
			case 43:
				return "wand_T6NS";

			case 254:
				return "sampo";
			case 255:
				return "true_orb";
			default:
				return "ERR";
		}
	}

	static int ListIndexOfSubstring(List<string> list, string sub)
	{
		for (int i = 0; i < list.Count; i++) if (list[i].Contains(sub)) return i;
		return -1;
	}

	static int CheckString(List<string> positiveList, List<string> negativeList, string s)
	{
		if (negativeList.Contains("-" + s)) return -1;

		int idx = ListIndexOfSubstring(positiveList, s);
		if (idx != -1) positiveList.RemoveAt(idx);
		else if (positiveList.Contains("*")) positiveList.Remove("*");
		else if (negativeList.Contains("-")) return -1;
		return 0;
	}

	static void ExpandAndAdd(Chest retChest, string s, bool potionContents)
	{
		if (s == "potions_pps")
		{
			ExpandAndAdd(retChest, "potion_normal", potionContents);
			ExpandAndAdd(retChest, "potion_normal", potionContents);
			ExpandAndAdd(retChest, "potion_secret", potionContents);
		}
		else if (s == "potions_ssr")
		{
			ExpandAndAdd(retChest, "potion_secret", potionContents);
			ExpandAndAdd(retChest, "potion_secret", potionContents);
			ExpandAndAdd(retChest, "potion_random_material", potionContents);
		}
		else if (potionContents && new string[] { "potion_normal", "potion_secret", "potion_random_material" }.Contains(s))
		{
			string contents = PotionLists.PotionContents(s, retChest.x, retChest.y, retChest.seed);
			ExpandAndAdd(retChest, "potion_" + contents, potionContents);
		}
		else
		{
			retChest.contents.Add(s);
		}
	}

	public static unsafe List<Chest> GenerateMap(Image wang, uint tiles_w, uint tiles_h, uint map_w, uint map_h, bool isCoalMine, int worldX, int worldY, ConfigState o)
	{
		byte[] wangData = Helpers.ImageToByteArray(wang);
		GCHandle pinnedTileData = GCHandle.Alloc(wangData, GCHandleType.Pinned);

		DateTime lStartTime = DateTime.Now;
		IntPtr pointer = generate_block(wangData, tiles_w, tiles_h, map_w, map_h, isCoalMine, worldX, worldY, o.currentSeed + o.ngPlus, Math.Max(o.batch, MAGIC_NUMBER), o.maxTries, o.pwCount, (byte)o.ngPlus, (byte)o.loggingLevel, o.maxChestContents, o.maxChestsPerBiome, (byte)(o.greedCurse ? 1 : 0));
		DateTime lEndTime = DateTime.Now;
		TimeSpan lFullExec = lEndTime - lStartTime;
		if (o.loggingLevel >= 2) Console.WriteLine($"DLL time: {lFullExec.TotalSeconds} sec");

		pinnedTileData.Free();
		byte* bytePtr = (byte*)pointer.ToPointer();
		List<Chest> ret = new();

		Parallel.For(0, o.batch, i =>
		{
			byte* chestBlock = bytePtr + i * ((9 + o.maxChestContents) * (2 * o.pwCount + 1) * o.maxChestsPerBiome + sizeof(uint)) + sizeof(uint);
			int count = *(((int*)chestBlock) - 1);
			if (o.loggingLevel >= 4) Console.WriteLine($"Chest count: {count}");
			for (int j = 0; j < count; j++)
			{
				byte* c = chestBlock + j * (9 + o.maxChestContents);
				int x = *(int*)c;
				int y = *(int*)(c + 4);
				byte contentsCount = *(c + 8);
				byte* contents = c + 9;
				if (o.loggingLevel >= 6) Console.WriteLine($"Chest {x} {y}: {contentsCount}");
				List<string> searchList = new(o.lootPositive);
				Chest retChest = new();
				retChest.x = x;
				retChest.y = y;
				retChest.seed = (uint)(o.currentSeed + i);
				retChest.contents = new(); 

				//Decode step
				for (int k = 0; k < contentsCount; k++)
				{
					byte b = contents[k];
					string s = DecodeItem(b, o.greedCurse);
					ExpandAndAdd(retChest, s, o.potionContents);
				}

				//Validate step
				bool failed = false;
				for (int k = 0; k < retChest.contents.Count; k++)
				{
					string s = retChest.contents[k];
					if (o.loggingLevel >= 6) Console.WriteLine($"  {s}");

					int checkResult = CheckString(searchList, o.lootNegative, s);

					if (o.loggingLevel >= 6) Console.WriteLine($"     Result: {checkResult}, {searchList.Count}");
					if (checkResult == -1)
					{
						failed = true;
						break;
					}
				}
				if (searchList.Count == 0 && !failed)
				{
					lock (threadlock)
					{
						ret.Add(retChest);
					}
				}
			}
		});
		free_array(pointer);

		return ret;
	}

	public static Image GeneratePathMap(Image map, int map_w, int map_h, int worldX, int worldY)
	{
		byte[] imgData = Helpers.ImageToByteArray(map);
		byte[] result = new byte[3 * map_w * map_h];

		bool mainPath = isMainPath(map_w, worldX);
		long malloc_amount = 3 * map_w * map_h;
		for (int i = 0; i < malloc_amount; i++)
		{
			result[i] = imgData[i];
		}
		uint path_start_x = 0x8e;
		floodFill(result, (uint)map_w, (uint)map_h, path_start_x, 1, COLOR_BLACK, COLOR_PURPLE);

		return Helpers.ByteArrayToImage(result, map_w, map_h);
	}

	const int BIOME_PATH_FIND_WORLD_POS_MIN_X = 159;
	const int BIOME_PATH_FIND_WORLD_POS_MAX_X = 223;
	const int WORLD_OFFSET_Y = 14;
	const int WORLD_OFFSET_X = 35;
	static bool isMainPath(int width, int worldX)
	{
		int fill_x_from = (int)(BIOME_PATH_FIND_WORLD_POS_MIN_X - (worldX - WORLD_OFFSET_X) * 512.0) / 10;
		int fill_x_to = fill_x_from + (BIOME_PATH_FIND_WORLD_POS_MAX_X - BIOME_PATH_FIND_WORLD_POS_MIN_X) / 10;
		return fill_x_to > 0 && fill_x_from > 0 && width > fill_x_from && fill_x_to < width + fill_x_from;
	}

	public static int GetWidthFromPix(int a, int b)
	{
		return ((b * 512) / 10 - (a * 512) / 10);
	}

	public static int GetGlobalPos(int a, int b, int c)
	{
		return ((b * 512) / 10 - (a * 512) / 10) * 10 + c;
	}

}
