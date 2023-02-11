using System.Drawing;
using System.Runtime.InteropServices;

namespace GCFinder;

public static class Wang
{
	//program segfaults on batch sizes smaller than this. Don't know why, don't care, we'll just cap the batch size sent to the kernel.
	const uint MAGIC_NUMBER = 724;

	static string threadlock = "bottom text";

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
		byte greedCurse,
		byte checkItems);

	[DllImport("WangTilerCUDA.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static unsafe extern void free_array(void* block);

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
			case 44:
				return "egg_purple";
			case 45:
				return "egg_slime";
			case 46:
				return "egg_monster";
			case 47:
				return "broken_wand";

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
		for (int i = 0; i < list.Count; i++) if (list[i] == sub || list[i].Contains("|") && list[i].Contains(sub)) return i;
		return -1;
	}

	static int CheckString(List<string> positiveList, List<string> negativeList, string s)
	{
		if (negativeList.Contains("-" + s)) return -1;

		int idx = ListIndexOfSubstring(positiveList, s);
		if (idx != -1)
		{
			positiveList.RemoveAt(idx);
			return 1;
		}
		else if (positiveList.Contains("*"))
		{
			positiveList.Remove("*");
			return 1;
		}
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

	public static unsafe List<Chest>[] ReadChestArray(byte* ptr, ConfigState o)
	{
		List<Chest>[] retArr = new List<Chest>[o.batch];
		Parallel.For(0, o.batch, i =>
		{
			List<Chest> ret = new List<Chest>();
			byte* chestBlock = ptr + i * ((9 + o.maxChestContents) * (2 * o.pwCount + 1) * o.maxChestsPerBiome + sizeof(uint)) + sizeof(uint);
			int count = *(((int*)chestBlock) - 1);

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

				ret.Add(retChest);
			}
			lock (retArr)
			{
				retArr[i] = ret;
			}
		});
		return retArr;
	}

	public static List<Chest> FilterChestList(List<Chest>[] lArr, ConfigState o)
	{
		List<Chest> ret = new List<Chest>();
		Parallel.For(0, o.batch, i =>
		{
			List<Chest> seedChests = lArr[i];

			List<string> aggregateSearch = new(o.lootPositive);
			List<Chest> aggregateChests = new();
			foreach (Chest c in seedChests)
			{
				if (o.loggingLevel >= 6) Console.WriteLine($"Chest {c.x} {c.y}: {c.contents.Count}");
				List<string> searchList = new(o.lootPositive);

				//Validate step
				bool failed = false;
				bool aggregatePassed = false;
				for (int k = 0; k < c.contents.Count; k++)
				{
					string s = c.contents[k];
					if (o.loggingLevel >= 6) Console.WriteLine($"  {s}");

					int checkResult = CheckString(searchList, o.lootNegative, s);
					int aggregateResult = CheckString(aggregateSearch, o.lootNegative, s);

					if (o.loggingLevel >= 6) Console.WriteLine($"     Result: {checkResult}, {searchList.Count}");
					if (!o.aggregate && checkResult == -1)
					{
						failed = true;
						break;
					}
					if(o.aggregate && aggregateResult == 1)
					{
						aggregatePassed = true;
					}
				}
				if (!o.aggregate && searchList.Count == 0 && !failed)
				{
					lock (ret)
					{
						ret.Add(c);
					}
				}
				if(o.aggregate && aggregatePassed)
				{
					aggregateChests.Add(c);
				}
			}
			if(o.aggregate && aggregateSearch.Count == 0)
			{
				lock (ret)
				{
					ret.AddRange(aggregateChests);
				}
			}
		});
		return ret;
	}

	public static unsafe List<Chest>[] GenerateMap(Image wang, uint tiles_w, uint tiles_h, uint map_w, uint map_h, bool isCoalMine, int worldX, int worldY, ConfigState o)
	{
		byte[] wangData = Helpers.ImageToByteArray(wang);
		GCHandle pinnedTileData = GCHandle.Alloc(wangData, GCHandleType.Pinned);

		DateTime lStartTime = DateTime.Now;
		IntPtr pointer = generate_block(wangData, tiles_w, tiles_h, map_w, map_h, isCoalMine, worldX, worldY, o.currentSeed + o.ngPlus, 
			Math.Max(o.batch, MAGIC_NUMBER), o.maxTries, o.pwCount, (byte)o.ngPlus, (byte)o.loggingLevel, o.maxChestContents, o.maxChestsPerBiome, 
			(byte)(o.greedCurse ? 1 : 0), (byte)(o.checkItems ? 1 : 0));

		DateTime lEndTime = DateTime.Now;
		TimeSpan lFullExec = lEndTime - lStartTime;
		if (o.loggingLevel >= 2) Console.WriteLine($"DLL time: {lFullExec.TotalSeconds} sec");

		pinnedTileData.Free();
		void* retPointers = pointer.ToPointer();
		byte* chestPtr = *(byte**)retPointers;
		byte* imgPtr = *((byte**)retPointers + 1);
		if (o.seedCount == 1)
		{
			Image i = Helpers.BytePtrToImage(imgPtr, (int)map_w, (int)map_h);
			i.Save($"{o.seedStart}_wang.png");
		}

		List<Chest>[] ret = ReadChestArray(chestPtr, o);
		free_array(chestPtr);
		free_array(imgPtr);
		return ret;
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
