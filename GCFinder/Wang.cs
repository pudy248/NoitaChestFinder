using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace GCFinder;

public static class Wang
{
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
		byte checkItems,
		byte expandSpells);

	[DllImport("WangTilerCUDA.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static unsafe extern void free_array(void* block);

	static string DecodeItem(byte item, bool greed, uint seed, int x, int y)
	{
		if((item & 0x80) > 0 && (item & 1) == 0)
		{
			byte rndCount = (byte)((item >> 1) & 0x3F);
			NoitaRandom rnd = new NoitaRandom(seed);
			rnd.SetRandomSeed(x, y);
			for (int i = 0; i < rndCount; i++) rnd.Next();
			SpellLists.Spell res = SpellLists.all_spells[0];
			bool valid = false;
			while (!valid)
			{
				int itemno = rnd.Random(0, 392);
				SpellLists.Spell spell = SpellLists.all_spells[itemno];
				double sum = 0;
				for (int i = 0; i < spell.spawn_probabilities.Length; i++) sum += spell.spawn_probabilities[i];
				if (sum > 0)
				{
					valid = true;
					res = spell;
				}
			}
			return $"spell_{res.id.ToLower()}";
		}
		else switch(item)
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

			case 253:
				return "sampo";
			case 255:
				return "true_orb";
			default:
				return "ERR";
		}
	}

	//returns 1 if found, -1 if blacklisted, 0 if ignored
	static int CheckString(List<string> positiveList, List<string> negativeList, string s)
	{
		if (negativeList.Contains("-" + s)) return -1;

		bool found = false;
		for(int i = 0; i < positiveList.Count; i++)
		{
			string pStr = positiveList[i];
			if (s == pStr) { positiveList.RemoveAt(i); return 1; }
			else if (pStr.Contains('|'))
			{
				string[] orList = pStr.Split('|');
				for (int j = 0; j < orList.Length; j++)
				{
					string orStr = orList[j];
					if (s == orStr) { positiveList.RemoveAt(i); return 1; }
					else if (orStr.Contains("*"))
					{
						if (orStr == "*") { positiveList.RemoveAt(i); return 1; }
						string wildcard = orStr.Substring(0, orStr.IndexOf('*'));
						if (s.StartsWith(wildcard)) { positiveList.RemoveAt(i); return 1; }
					}
				}
			}
			else if (pStr.Contains("*"))
			{
				if (pStr == "*") { positiveList.RemoveAt(i); return 1; }
				string wildcard = pStr.Substring(0, pStr.IndexOf('*'));
				if(s.StartsWith(wildcard)) { positiveList.RemoveAt(i); return 1; }
			}
		}
		return 0;
	}

	static void ExpandAndAdd(Chest retChest, string s, bool potionContents, bool spellContents)
	{
		if (s == "potions_pps")
		{
			ExpandAndAdd(retChest, "potion_normal", potionContents, spellContents);
			ExpandAndAdd(retChest, "potion_normal", potionContents, spellContents);
			ExpandAndAdd(retChest, "potion_secret", potionContents, spellContents);
		}
		else if (s == "potions_ssr")
		{
			ExpandAndAdd(retChest, "potion_secret", potionContents, spellContents);
			ExpandAndAdd(retChest, "potion_secret", potionContents, spellContents);
			ExpandAndAdd(retChest, "potion_random_material", potionContents, spellContents);
		}
		else if (potionContents && new string[] { "potion_normal", "potion_secret", "potion_random_material" }.Contains(s))
		{
			string contents = PotionLists.PotionContents(s, retChest.x, retChest.y, retChest.seed);
			ExpandAndAdd(retChest, "potion_" + contents, potionContents, spellContents);
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
			uint chestSize = 9 + o.maxChestContents;
			uint chestSegmentSize = sizeof(uint) + o.maxChestsPerBiome * (2 * o.pwCount + 1) * chestSize;
			List<Chest> ret = new List<Chest>();
			byte* chestBlock = ptr + i * chestSegmentSize;
			int count = *(int*)chestBlock;
			if (o.loggingLevel >= 5) Console.WriteLine($"{o.currentSeed + i} chest count: {count}");

			for (int j = 0; j < count; j++)
			{
				byte* c = chestBlock + 4 + j * chestSize;
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
					string s = DecodeItem(b, o.greedCurse, retChest.seed, x, y);
					ExpandAndAdd(retChest, s, o.potionContents, true);
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

	public static unsafe List<Chest>[] GenerateMap(Image<Rgb24> wang, uint tiles_w, uint tiles_h, uint map_w, uint map_h, bool isCoalMine, int worldX, int worldY, ConfigState o)
	{
		byte[] wangData = Helpers.ImageToByteArray(wang);
		GCHandle pinnedTileData = GCHandle.Alloc(wangData, GCHandleType.Pinned);

		DateTime lStartTime = DateTime.Now;
		IntPtr pointer = generate_block(wangData, tiles_w, tiles_h, map_w, map_h, isCoalMine, worldX, worldY, o.currentSeed + o.ngPlus, 
			o.batch, o.maxTries, o.pwCount, (byte)o.ngPlus, (byte)o.loggingLevel, o.maxChestContents, o.maxChestsPerBiome, 
			(byte)(o.greedCurse ? 1 : 0), (byte)(o.checkItems ? 1 : 0), (byte)(o.spellContents ? 1 : 0));

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
			if(!Directory.Exists("wang_outputs")) Directory.CreateDirectory("wang_outputs");
			i.Save($"wang_outputs/{o.currentSeed}.png");
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
