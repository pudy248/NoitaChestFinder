using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace GCFinder;

public static class Wang
{
	[DllImport("WangTilerCUDA.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern IntPtr generate_block(
		byte[] tiles_data,
		uint[] seeds,
		uint tiles_w,
		uint tiles_h,
		uint map_w,
		uint map_h,
		bool isCoalMine,
		byte biomeIndex,
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
		byte expandSpells,
		byte checkWands);

	[DllImport("WangTilerCUDA.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static extern IntPtr search_eoe(
		int originX,
		int originY,
		uint radius,
		uint _worldSeed,
		byte _loggingLevel,
		uint _maxChestContents,
		byte checkItems,
		byte expandSpells,
		byte tinyMode);

	[DllImport("WangTilerCUDA.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	public static unsafe extern void free_array(void* block);

	public static string DecodeItem(byte item, bool greed, uint seed, int x, int y)
	{
		//we have to expand spells here because the byte contains some data in the 2nd-7th bits
		if((item & 0x80) > 0 && (item & 1) == 0)
		{
			byte rndCount = (byte)((item >> 1) & 0x3F);
			NoitaRandom rnd = new NoitaRandom(seed);
			rnd.SetRandomSeed(x+509.7, y+683.1);
			for (int i = 0; i < rndCount; i++) rnd.Next();
			WandGen.Spell res = WandGen.all_spells[0];
			bool valid = false;
			while (!valid)
			{
				int itemno = rnd.Random(0, 392);
				WandGen.Spell spell = WandGen.all_spells[itemno];
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

			case 64:
				return "wand_T10NS";
			case 65:
				return "wand_T1B";
			case 66:
				return "wand_T2B";
			case 67:
				return "wand_T3B";
			case 68:
				return "wand_T4B";
			case 69:
				return "wand_T5B";
			case 70:
				return "wand_T6B";
			case 71:
				return "unknown_wand";

			case 253:
				return "sampo";
			case 255:
				return "true_orb";
			default:
				return "ERR";
		}
	}

	//returns 1 if found, -1 if blacklisted, 0 if ignored, 2 if wand found
	public static int CheckString(List<string> positiveList, List<string> negativeList, List<WandCheck> wandChecks, string s)
	{
		if (negativeList.Contains("-" + s)) return -1;

		if(s.StartsWith("wand") && !s.StartsWith("wand_T"))
		{
			bool passed = WandGen.WandChecksPassed(s, wandChecks);
			if(passed) return 2;
			else return 0;
		}

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

	public static void ExpandAndAdd(Chest retChest, string s, ConfigState o)
	{
		if (s == "potions_pps")
		{
			ExpandAndAdd(retChest, "potion_normal", o);
			ExpandAndAdd(retChest, "potion_normal", o);
			ExpandAndAdd(retChest, "potion_secret", o);
		}
		else if (s == "potions_ssr")
		{
			ExpandAndAdd(retChest, "potion_secret", o);
			ExpandAndAdd(retChest, "potion_secret", o);
			ExpandAndAdd(retChest, "potion_random_material", o);
		}
		else if (o.checkPotions && new string[] { "potion_normal", "potion_secret", "potion_random_material" }.Contains(s))
		{
			string contents = PotionLists.PotionContents(s, retChest.x, retChest.y, retChest.seed);
			ExpandAndAdd(retChest, "potion_" + contents, o);
		}
		else if (o.checkWands && s.StartsWith("wand_T"))
		{
			bool nonshuffle = s.EndsWith("NS");
			bool better = s.EndsWith("B");
			int index = s.ToCharArray().ToList().IndexOf('T');
			int tier;
			if (s.Length > index + 2 && s.ToCharArray()[index + 2] == '0')
				tier = 10;
			else tier = int.Parse(s.ToCharArray()[index + 1].ToString());

			WandGen.Wand w = WandGen.GetWandWithLevel(retChest.seed, retChest.x, retChest.y, tier, nonshuffle, better);
			string[] strs = WandGen.ExpandWand(w);
			foreach(string str in strs)
				ExpandAndAdd(retChest, str.Trim(), o);
		}
		else
		{
			retChest.contents.Add(s);
		}
	}

	public static unsafe List<Chest>[] ReadChestArray(byte* ptr, uint[] seeds, ConfigState o)
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
				Chest retChest = new();
				retChest.x = x;
				retChest.y = y;
				retChest.seed = seeds[i];
				retChest.contents = new();

				//Decode step
				for (int k = 0; k < contentsCount; k++)
				{
					byte b = contents[k];
					string s = DecodeItem(b, o.greedCurse, retChest.seed, x, y);
					ExpandAndAdd(retChest, s, o);
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

	public static unsafe List<Chest> ReadEOEChestArray(byte* ptr, ConfigState o)
	{
		Chest[] retArr = new Chest[4 * o.EOE_radius * o.EOE_radius];
		Parallel.For(0, 4 * o.EOE_radius * o.EOE_radius, i =>
		{
			byte* c = ptr + i * (9 + o.maxChestContents);
			int x = *(int*)c;
			int y = *(int*)(c + 4);
			byte contentsCount = *(c + 8);
			byte* contents = c + 9;
			if (o.loggingLevel >= 6) Console.WriteLine($"Chest {x} {y}: {contentsCount}");
			List<string> searchList = new(o.lootPositive);
			Chest retChest = new();
			retChest.x = x;
			retChest.y = y;
			retChest.seed = o.currentSeed;
			retChest.contents = new();

			//Decode step
			for (int k = 0; k < contentsCount; k++)
			{
				byte b = contents[k];
				string s = DecodeItem(b, o.greedCurse, retChest.seed, x, y);
				ExpandAndAdd(retChest, s, o);
			}
			retArr[i] = retChest;
		});
		return retArr.ToList();
	}

	public static List<Chest> FilterChestList(List<Chest>[,] listArray, ConfigState o, List<string> biomes)
	{
		List<Chest> ret = new List<Chest>();
		Parallel.For(0, o.batch, j =>
		{
			for (int i = 0; i < biomes.Count; i++)
			{
				List<Chest> seedChests = listArray[i, j];


				List<string> aggregateSearch = new(o.lootPositive);
				List<Chest> aggregateChests = new();
				foreach (Chest c in seedChests)
				{
					if (o.loggingLevel >= 6) Console.WriteLine($"Chest {c.x} {c.y}: {c.contents.Count}");
					List<string> searchList = new(o.lootPositive);

					//check if valid spawn
					if (o.minX != -1 && c.x < o.minX)
						continue;
					if (o.maxX != -1 && c.x > o.maxX)
						continue;
					if (o.minY != -1 && c.y < o.minY)
						continue;
					if (o.maxY != -1 && c.y > o.maxY)
						continue;

					if (o.startingFlask != "" && PotionLists.StartingFlask(c.seed) != o.startingFlask)
						continue;

					(int, int) chunkCoords = Wang.GetChunkPos(c.x, c.y);
					(int, int) pwChunkCoords = ((chunkCoords.Item1 % 70 + 70) % 70, (chunkCoords.Item2 % 48 + 48) % 48);
					string color = Helpers.ToHex(BiomeData.biomeMap[pwChunkCoords.Item1, pwChunkCoords.Item2]);
					if (color != BiomeData.nameToColor[biomes[i]])
					{
						continue;
					}


					//check contents
					bool failed = false;
					bool aggregatePassed = false;
					bool wandPassed = false;
					for (int k = 0; k < c.contents.Count; k++)
					{
						string s = c.contents[k];
						if (o.loggingLevel >= 6) Console.WriteLine($"  {s}");

						int checkResult = CheckString(searchList, o.lootNegative, o.wandChecks, s);
						int aggregateResult = CheckString(aggregateSearch, o.lootNegative, o.wandChecks, s);

						if (o.loggingLevel >= 6) Console.WriteLine($"     Result: {checkResult}, {searchList.Count}");
						if (!o.aggregate && checkResult == -1)
						{
							failed = true;
							break;
						}
						if (o.aggregate && aggregateResult == 1)
						{
							aggregatePassed = true;
						}
						if (checkResult == 2) wandPassed = true;
					}

					if (!failed && (o.wandAndLoot && wandPassed && searchList.Count == 0 || !o.wandAndLoot && (!o.aggregate && searchList.Count == 0 || wandPassed)))
					{
						lock (ret)
						{
							ret.Add(c);
						}
					}
					if (o.aggregate && aggregatePassed)
					{
						aggregateChests.Add(c);
					}
				}
				if (o.aggregate && aggregateSearch.Count == 0)
				{
					lock (ret)
					{
						ret.AddRange(aggregateChests);
					}
				}
			}
		});
		return ret;
	}

	public static List<Chest> FilterEOEChestList(List<Chest> list, ConfigState o)
	{
		List<Chest> ret = new();
		Parallel.For(0, 4 * o.EOE_radius * o.EOE_radius, i =>
		{
			Chest c = list[(int)i];
			if (o.loggingLevel >= 6) Console.WriteLine($"Chest {c.x} {c.y}: {c.contents.Count}");
			List<string> searchList = new(o.lootPositive);

			//Validate step
			bool failed = false;
			bool wandPassed = false;
			for (int k = 0; k < c.contents.Count; k++)
			{
				string s = c.contents[k];
				if (o.loggingLevel >= 6) Console.WriteLine($"  {s}");

				int checkResult = CheckString(searchList, o.lootNegative, o.wandChecks, s);

				if (o.loggingLevel >= 6) Console.WriteLine($"     Result: {checkResult}, {searchList.Count}");
				if (checkResult == -1)
				{
					failed = true;
					break;
				}
				if (checkResult == 2) wandPassed = true;
			}
			if (!failed && (searchList.Count == 0 || wandPassed))
			{
				lock (ret)
				{
					ret.Add(c);
				}
			}
		});
		return ret;
	}
	
	public static unsafe List<Chest>[] GenerateMap(Image<Rgb24> wang, uint tiles_w, uint tiles_h, uint map_w, uint map_h, bool isCoalMine, byte biomeIndex, int worldX, int worldY, ConfigState o, uint[] seeds)
	{
		byte[] wangData = Helpers.ImageToByteArray(wang);
		GCHandle pinnedTileData = GCHandle.Alloc(wangData, GCHandleType.Pinned);
		GCHandle pinnedSeeds = GCHandle.Alloc(seeds, GCHandleType.Pinned);

		DateTime lStartTime = DateTime.Now;
		IntPtr pointer = generate_block(wangData, seeds, tiles_w, tiles_h, map_w, map_h, isCoalMine, biomeIndex, worldX, worldY, o.currentSeed + o.ngPlus, 
			o.batch, o.maxTries, o.pwCount, (byte)o.ngPlus, (byte)o.loggingLevel, o.maxChestContents, o.maxChestsPerBiome, 
			(byte)(o.greedCurse ? 1 : 0), (byte)(o.checkItems ? 1 : 0), (byte)(o.checkSpells ? 1 : 0), (byte)(o.checkWands ? 1 : 1));

		DateTime lEndTime = DateTime.Now;
		TimeSpan lFullExec = lEndTime - lStartTime;
		if (o.loggingLevel >= 2) Console.WriteLine($"DLL time: {lFullExec.TotalSeconds} sec");

		pinnedTileData.Free();
		pinnedSeeds.Free();
		void* retPointers = pointer.ToPointer();
		byte* chestPtr = *(byte**)retPointers;
		byte* imgPtr = *((byte**)retPointers + 1);
		if (o.seedCount == 1)
		{
			Image i = Helpers.BytePtrToImage(imgPtr, (int)map_w, (int)map_h);
			if(!Directory.Exists("wang_outputs")) Directory.CreateDirectory("wang_outputs");
			i.Save($"wang_outputs/{o.currentSeed}.png");
		}

		List<Chest>[] ret = ReadChestArray(chestPtr, seeds, o);
		free_array(chestPtr);
		free_array(imgPtr);
		return ret;
	}

	public static unsafe List<Chest> GenerateEOEChests(ConfigState o)
	{
		DateTime lStartTime = DateTime.Now;
		IntPtr pointer = search_eoe(o.EOE_x, o.EOE_y, (uint)o.EOE_radius, o.currentSeed, (byte)o.loggingLevel, o.maxChestContents, (byte)(o.checkItems ? 1 : 0), (byte)(o.checkSpells ? 1 : 0), (byte)(o.EOE_tinymode ? 1 : 0));
		byte* retPointer = (byte*)pointer.ToPointer();
		List<Chest> ret = ReadEOEChestArray(retPointer, o);
		free_array(retPointer);
		return ret;
	}

	public static int GetWidthFromPix(int a, int b)
	{
		return ((b * 512) / 10 - (a * 512) / 10);
	}
	public static (int, int) GetChunkPos(int gx, int gy)
	{
		int x = (int)Math.Round(((gx + 15) / 512.0) + 35);
		int y = (int)Math.Round(((gy + 3) / 512.0) + 14);
		return (x, y);
	}
	public static (int, int) GetGlobalPos(int x, int y)
	{
		int gx = (int)(((x - 35) * 512) / 10) * 10 - 15;
		int gy = (int)(((y - 14) * 512) / 10) * 10 - 3;
		return (gx, gy);
	}
}
