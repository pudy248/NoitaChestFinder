using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GCFinder.WandGen;

namespace GCFinder
{
	public class ChestPosPrechecker
	{
		public ConfigState o;

		int MakeRandomCard(ref NoitaRandom random)
		{
			int res = 0;
			bool valid = false;
			while (!valid)
			{
				int itemno = random.Random(0, 392);
				Spell item = all_spells[itemno];
				double sum = 0;
				for (int i = 0; i < item.spawn_probabilities.Length; i++) sum += item.spawn_probabilities[i];
				if (sum > 0)
				{
					valid = true;
					res = itemno;
				}
			}
			return res;
		}

		int roundRNGPos(int num)
		{
			if (-1000000 < num && num < 1000000) return num;
			else if (-10000000 < num && num < 10000000) return num - (num % 10) + (num % 10 >= 5 ? 10 : 0);
			else if (-100000000 < num && num < 100000000) return num - (num % 100) + (num % 100 >= 50 ? 100 : 0);
			return num;
		}
		
		unsafe void CheckNormalChestLoot(int x, int y, uint worldSeed, byte expandSpells, byte* writeLoc)
		{
			*(int*)writeLoc = x;
			*(int*)(writeLoc + 4) = y;
			byte* contents = writeLoc + 9;
			NoitaRandom random = new NoitaRandom(worldSeed);
			random.SetRandomSeed(roundRNGPos(x) + 509.7, y + 683.1);

			int idx = 0;
			int count = 1;
			while (count > 0)
			{
				if (idx >= o.maxChestContents)
				{
					if (o.loggingLevel >= 3) Console.WriteLine($"Prechecker: Chest contents overflow in seed {worldSeed}");
					break;
				}
				count--;
				int rnd = random.Random(1, 100);
				if (rnd <= 7) contents[idx++] = 3;
				else if (rnd <= 40)
				{
					rnd = random.Random(0, 100);

					rnd = random.Random(0, 100);
					if (rnd > 99)
					{
						int tamount = random.Random(1, 3);
						for (int i = 0; i < tamount; i++)
						{
							random.Random(-10, 10);
							random.Random(-10, 5);
						}

						if (random.Random(0, 100) > 50)
						{
							tamount = random.Random(1, 3);
							for (int i = 0; i < tamount; i++)
							{
								random.Random(-10, 10);
								random.Random(-10, 5);
							}
						}
						if (random.Random(0, 100) > 80)
						{
							tamount = random.Random(1, 3);
							for (int i = 0; i < tamount; i++)
							{
								random.Random(-10, 10);
								random.Random(-10, 5);
							}
						}
					}
					else
					{
						random.Random(-10, 10);
						random.Random(-10, 5);
					}
					contents[idx++] = 0;
				}
				else if (rnd <= 50)
				{
					rnd = random.Random(1, 100);
					if (rnd <= 94) contents[idx++] = 5;
					else if (rnd <= 98) contents[idx++] = 4;
					else
					{
						rnd = random.Random(0, 100);
						if (rnd <= 98) contents[idx++] = 6;
						else contents[idx++] = 7;
					}
				}
				else if (rnd <= 54) contents[idx++] = 27;
				else if (rnd <= 60)
				{
					byte[] opts = new byte[] { 10, 11, 15, 12, 16, 127, 13, 14 };
					rnd = random.Random(0, 7);
					byte opt = opts[rnd];
					if (opt == 127)
					{
						byte[] r_opts = new byte[] { 19, 20, 21, 22, 23, 24, 25 };
						rnd = random.Random(0, 6);
						byte r_opt = r_opts[rnd];
						contents[idx++] = r_opt;
					}
					else
					{
						contents[idx++] = opt;
					}
				}
				else if (rnd <= 65)
				{
					int amount = 1;
					int rnd2 = random.Random(0, 100);
					if (rnd2 <= 50) amount = 1;
					else if (rnd2 <= 70) amount += 1;
					else if (rnd2 <= 80) amount += 2;
					else if (rnd2 <= 90) amount += 3;
					else amount += 4;

					for (int i = 0; i < amount; i++)
					{
						random.Random(0, 1);
						if (expandSpells > 0)
						{
							int randCTR = random.randomCTR;
							contents[idx++] = (byte)((randCTR << 1) | 0x80);
						}
						MakeRandomCard(ref random);
					}

					if (expandSpells == 0)
						contents[idx++] = 26;
				}
				else if (rnd <= 84)
				{
					rnd = random.Random(0, 100);
					if (rnd <= 25) contents[idx++] = 32;
					else if (rnd <= 50) contents[idx++] = 33;
					else if (rnd <= 75) contents[idx++] = 34;
					else if (rnd <= 90) contents[idx++] = 35;
					else if (rnd <= 96) contents[idx++] = 36;
					else if (rnd <= 98) contents[idx++] = 37;
					else if (rnd <= 99) contents[idx++] = 38;
					else contents[idx++] = 39;
				}
				else if (rnd <= 95)
				{
					rnd = random.Random(0, 100);
					if (rnd <= 88) contents[idx++] = 28;
					else if (rnd <= 89) contents[idx++] = 29;
					else if (rnd <= 99) contents[idx++] = 30;
					else contents[idx++] = 31;
				}
				else if (rnd <= 98) contents[idx++] = 1;
				else if (rnd <= 99) count += 2;
				else count += 3;
			}

			*(writeLoc + 8) = (byte)idx;
		}

		unsafe void CheckGreatChestLoot(int x, int y, uint worldSeed, byte* writeLoc)
		{
			*(int*)writeLoc = x;
			*(int*)(writeLoc + 4) = y;
			byte* contents = writeLoc + 9;
			NoitaRandom random = new NoitaRandom(worldSeed);
			random.SetRandomSeed(roundRNGPos(x), y);

			int idx = 0;
			int count = 1;

			if (random.Random(0, 100000) >= 100000)
			{
				count = 0;
				if (random.Random(0, 1000) == 999) contents[idx++] = 255;
				else contents[idx++] = 253;
			}

			while (count != 0)
			{
				if (idx >= o.maxChestContents)
				{
					if (o.loggingLevel >= 3) Console.WriteLine($"Prechecker: Chest contents overflow in seed {worldSeed}");
					break;
				}
				count--;
				int rnd = random.Random(1, 100);

				if (rnd <= 30)
				{
					rnd = random.Random(0, 100);
					if (rnd <= 30)
						contents[idx++] = 8;
					else
						contents[idx++] = 9;
				}
				else if (rnd <= 33)
				{
					contents[idx++] = 2;
				}
				else if (rnd <= 38)
				{
					rnd = random.Random(1, 30);
					if (rnd == 30)
					{
						contents[idx++] = 18;
					}
					else contents[idx++] = 17;
				}
				else if (rnd <= 39)
				{
					rnd = random.Random(0, 100);
					if (rnd <= 25) contents[idx++] = 36;
					else if (rnd <= 50) contents[idx++] = 37;
					else if (rnd <= 75) contents[idx++] = 38;
					else if (rnd <= 90) contents[idx++] = 39;
					else if (rnd <= 96) contents[idx++] = 40;
					else if (rnd <= 98) contents[idx++] = 41;
					else if (rnd <= 99) contents[idx++] = 42;
					else contents[idx++] = 43;
				}
				else if (rnd <= 60)
				{
					rnd = random.Random(0, 100);
					if (rnd <= 89) contents[idx++] = 28;
					else if (rnd <= 99) contents[idx++] = 30;
					else contents[idx++] = 31;
				}
				else if (rnd <= 99) count += 2;
				else count += 3;
			}
			*(writeLoc + 8) = (byte)idx;
		}

		unsafe void spawnChest(int x, int y, uint seed, byte greedCurse, byte expandSpells, byte* writeLoc)
		{
			NoitaRandom random = new NoitaRandom(seed);
			random.SetRandomSeed(x, y);
			int super_chest_spawn_rate = greedCurse > 0 ? 100 : 2000;
			int rnd = random.Random(1, super_chest_spawn_rate);

			if (rnd >= super_chest_spawn_rate - 1)
				CheckGreatChestLoot(x, y, seed, writeLoc);
			else
				CheckNormalChestLoot(x, y, seed, expandSpells, writeLoc);
		}

		unsafe Chest DecodeChestBytes(byte* chest, uint seed)
		{
			int x = *(int*)chest;
			int y = *(int*)(chest + 4);
			byte contentsCount = *(chest + 8);
			byte* contents = chest + 9;
			if (o.loggingLevel >= 6) Console.WriteLine($"Precheck chest {x} {y}: {contentsCount}");
			Chest retChest = new();
			retChest.x = x;
			retChest.y = y;
			retChest.seed = seed;
			retChest.contents = new();

			for (int k = 0; k < contentsCount; k++)
			{
				byte b = contents[k];
				string s = Wang.DecodeItem(b, o.greedCurse, retChest.seed, x, y);
				Wang.ExpandAndAdd(retChest, s, o);
			}

			return retChest;
		}

		bool SingleChestPassed(Chest c)
		{
			if (o.loggingLevel >= 6) Console.WriteLine($"Chest {c.seed}, {c.x} {c.y}: {c.contents.Count}");
			List<string> searchList = new(o.lootPositive);

			if (o.startingFlask != "" && PotionLists.StartingFlask(c.seed) != o.startingFlask)
				return false;

			//check contents
			bool failed = false;
			bool wandPassed = false;
			for (int k = 0; k < c.contents.Count; k++)
			{
				string s = c.contents[k];
				if (o.loggingLevel >= 6) Console.WriteLine($"  {s}");

				int checkResult = Wang.CheckString(searchList, o.lootNegative, o.wandChecks, s);

				if (o.loggingLevel >= 6) Console.WriteLine($"     Result: {checkResult}, {searchList.Count}");
				if (checkResult == -1)
				{
					failed = true;
					break;
				}
				if (checkResult == 2) wandPassed = true;
			}

			if (!failed && (o.wandAndLoot && wandPassed && searchList.Count == 0 || !o.wandAndLoot && (!o.aggregate && searchList.Count == 0 || wandPassed)))
			{
				return true;
			}
			return false;
		}
	
		public static unsafe bool PrecheckSeed(uint seed, ConfigState o)
		{
			ChestPosPrechecker checker = new() { o = o };
			byte* pointer = (byte*)Marshal.AllocHGlobal((int)(9 + o.maxChestContents));
			checker.spawnChest(315, 17, seed, (byte)(o.greedCurse ? 1 : 0), (byte)(o.checkSpells ? 1 : 0), pointer);
			Chest c = checker.DecodeChestBytes(pointer, seed);
			bool filter = checker.SingleChestPassed(c);
			Marshal.FreeHGlobal((IntPtr)pointer);
			return filter;
		}
	}
}
