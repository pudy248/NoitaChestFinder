using GCFinder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CommandLine;
using System.Diagnostics;
public class ConfigState

{
	[Option('z', "batch-size", Required = false, HelpText = "Batch size for searches.", Default = 10000U)]
	public uint batch { get; set; }

	[Option('s', "start-seed", Required = false, HelpText = "Starting seed for searches.", Default = 1U)]
	public uint seedStart { get; set; }

	[Option('i', "seed-count", Required = false, HelpText = "Total number of seeds to search.", Default = 2147483647U)]
	public uint seedCount { get; set; }

	[Option('c', "continue", Required = false, HelpText = "Continue from where the last execution ended.", Default = false)]
	public bool continueLast { get; set; }

	[Option("biome", Required = false, HelpText = "Biome to search. Check the readme for details on usage.", Default = "coalmine")]
	public string biome { get; set; }

	[Option('p', "parallel-worlds", Required = false, HelpText = "Number of parallel worlds in either direction to search. 0 only searches the main world.", Default = 0U)]
	public uint pwCount { get; set; }

	//[Option('n', "ng-plus", Required = false, HelpText = "NG+ number to search. Currently only supports NG.", Default = 0U)]
	public uint ngPlus { get; set; }

	[Option('l', "loot-search", Required = false, HelpText = "Loot to search for in chests. Check the readme for details on usage.", Default = "null")]
	public string lootSearch { get; set; }

	[Option('w', "wand-stats", Required = false, HelpText = "Conditions to return wands. Check the readme for details on usage.", Default = "capacity>26")]
	public string wandStats { get; set; }

	[Option("greed", Required = false, HelpText = "Activate greed curse.", Default = false)]
	public bool greedCurse { get; set; }

	[Option("pedestals", Required = false, HelpText = "Check potion/item pedestals.", Default = false)]
	public bool checkItems { get; set; }

	[Option("potions", Required = false, HelpText = "Check potion material contents.", Default = false)]
	public bool checkPotions { get; set; }

	[Option("spells", Required = false, HelpText = "Check random spell contents.", Default = false)]
	public bool checkSpells { get; set; }

	[Option("wands", Required = false, HelpText = "Check wand stats and contents.", Default = false)]
	public bool checkWands { get; set; }

	[Option('a', "aggregate-items", Required = false, HelpText = "Expands multi-item search scope to the entire world instead of single chests.", Default = false)]
	public bool aggregate { get; set; }

	[Option("wand-and-loot", Required = false, HelpText = "Display only chests which have a wand that passes the wand filter AND loot that passes the loot filter.", Default = false)]
	public bool wandAndLoot { get; set; }

	[Option("output-path", Required = false, HelpText = "File to write outputs to. Leave blank to only log to the console.", Default = "out.txt")]
	public string outputPath { get; set; }

	[Option("max-items", Required = false, HelpText = "Maximum number of items per chest to store. Overflow items will not be included in search. Increases VRAM usage.", Default = 25U)]
	public uint maxChestContents { get; set; }

	[Option("max-chests", Required = false, HelpText = "Maximum number of chests per biome to store. Increases VRAM usage.", Default = 25U)]
	public uint maxChestsPerBiome { get; set; }

	[Option('d', "debug-logging-level", Required = false, HelpText = "Debug logging level.", Default = 1U)]
	public uint loggingLevel { get; set; }

	[Option("tries", Required = false, HelpText = "Maximum generation attempts. Noita uses 100.", Default = 10U)]
	public uint maxTries { get; set; }

	[Option("min-x", Required = false, HelpText = "Minimum X position.", Default = -1)]
	public int minX { get; set; }

	[Option("min-y", Required = false, HelpText = "Minimum Y position.", Default = -1)]
	public int minY { get; set; }

	[Option("max-x", Required = false, HelpText = "Maximum X position.", Default = -1)]
	public int maxX { get; set; }

	[Option("max-y", Required = false, HelpText = "Maximum Y position.", Default = -1)]
	public int maxY { get; set; }


	[Option("EOE", Required = false, HelpText = "End of Everything mode. Ignores aggregate search. Only searches the given input seed.", Default = false)]
	public bool EOE { get; set; }

	[Option("EOE-originX", Required = false, HelpText = "Center of search in EOE mode.", Default = 0)]
	public int EOE_originX { get; set; }

	[Option("EOE-originY", Required = false, HelpText = "Center of search in EOE mode.", Default = 0)]
	public int EOE_originY { get; set; }

	[Option("EOE-radius", Required = false, HelpText = "Radius of search in EOE mode.", Default = 1000)]
	public int EOE_radius { get; set; }

	[Option("EOE-tinymode", Required = false, HelpText = "Check T10 wand drops instead of great chests. How you kill tiny on those pixels is your problem.", Default = false)]
	public bool EOE_tinymode { get; set; }

	public int EOE_x;
	public int EOE_y;

	public uint currentSeed;

	public List<string> lootPositive = new();
	public List<string> lootNegative = new();

	public List<WandCheck> wandChecks = new();
}

public struct WandCheck
{
	public enum Comparison
	{
		Greater,
		Geq,
		Equal,
		Leq,
		Less
	}

	public Comparison comparison;
	public string stat;
	public float value;
}

public class Program
{
	static void Main(string[] args)
	{
		//idk if this does anything but it should prevent system lag and just slow down the search instead of taking more resources
		using (Process p = Process.GetCurrentProcess())
			p.PriorityClass = ProcessPriorityClass.BelowNormal;

		DateTime lStartTime = DateTime.Now;

		Parser.Default.ParseArguments<ConfigState>(args).WithParsed(opt =>
		{
			if (opt.continueLast && File.Exists("seed.txt"))
			{
				string[] file = File.ReadAllLines("seed.txt");
				List<string> intermediateArgs = file.Skip(1).Where(s => s != "-c").ToList();
				for(int j = 0; j < intermediateArgs.Count; j++)
				{
					if(intermediateArgs[j] == "-s")
					{
						intermediateArgs.RemoveAt(j);
						intermediateArgs.RemoveAt(j);
					}
				}
				intermediateArgs.Add("-s");
				intermediateArgs.Add(file[0]);
				Main(intermediateArgs.ToArray());
				return;
			}

			opt.batch = Math.Min(opt.batch, opt.seedCount);
			opt.currentSeed = opt.seedStart;

			List<string> lootSeparated = opt.lootSearch.Split(" ").ToList();
			foreach(string s in lootSeparated)
			{
				if(s.StartsWith("-")) opt.lootNegative.Add(s);
				else if(s != "") opt.lootPositive.Add(s);
			}

			List<string> wandStatsSeparated = opt.wandStats.Split(" ").ToList();
			foreach (string s in wandStatsSeparated)
			{
				WandCheck check = new();
				string[] halves;
				if (s.Contains("<="))
				{
					halves = s.Split("<=");
					check.comparison = WandCheck.Comparison.Leq;
				}
				else if (s.Contains("<"))
				{
					halves = s.Split("<");
					check.comparison = WandCheck.Comparison.Less;
				}
				else if (s.Contains(">="))
				{
					halves = s.Split(">=");
					check.comparison = WandCheck.Comparison.Geq;
				}
				else if (s.Contains(">"))
				{
					halves = s.Split(">");
					check.comparison = WandCheck.Comparison.Greater;
				}
				else if (s.Contains("="))
				{
					halves = s.Split("=");
					check.comparison = WandCheck.Comparison.Equal;
				}
				else continue;
				check.stat = halves[0];
				check.value = float.Parse(halves[1]);

				opt.wandChecks.Add(check);
			}
			WandGen.SortSpells();

			if (opt.loggingLevel >= 3)
			{
				Console.Write("Positive: ");
				foreach (string s in opt.lootPositive) Console.Write($"{{{s}}} ");
				Console.WriteLine("");
				Console.Write("Negative: ");
				foreach (string s in opt.lootNegative) Console.Write($"{{{s}}} ");
				Console.WriteLine("");
			}

			int i = 0;

			int x = 0;
			int y = 0;
			int rad = 1;
			int xIncr = 1;
			int yIncr = 0;
			opt.EOE_x = opt.EOE_originX;
			opt.EOE_y = opt.EOE_originY;

			if (opt.outputPath != "" && !opt.continueLast) File.WriteAllText(opt.outputPath, "");
			if (opt.EOE) while (true)
				{
					DateTime startTime = DateTime.Now;
					List<string> seedText = new List<string>() { $"{opt.currentSeed}" };
					seedText.AddRange(args);
					File.WriteAllLines("seed.txt", seedText);

					MapGenerator gen = new MapGenerator();
					if (opt.loggingLevel >= 2) Console.WriteLine($"Batch {i}: EOE local coords are ({x}, {y})");
					gen.SearchEOE(opt);
					
					//bad spiral code
					if (x >= rad && xIncr == 1)
					{
						yIncr = 1;
						xIncr = 0;
					}
					else if (y >= rad && yIncr == 1)
					{
						xIncr = -1;
						yIncr = 0;
					}
					else if (x <= -rad && xIncr == -1)
					{
						yIncr = -1;
						xIncr = 0;
					}
					else if (y <= -rad && yIncr == -1)
					{
						xIncr = 1;
						yIncr = 0;
						rad++;
					}
					x += xIncr;
					y += yIncr;
					opt.EOE_x = opt.EOE_originX + 2 * opt.EOE_radius * x;
					opt.EOE_y = opt.EOE_originY + 2 * opt.EOE_radius * y;
					i++;

					DateTime endTime = DateTime.Now;
					TimeSpan fullExec = endTime - startTime;
					if (opt.loggingLevel >= 1) Console.WriteLine($"Batch {i}: {fullExec.TotalSeconds} sec");
			}
			else while (true)
			{
				DateTime startTime = DateTime.Now; 
				List<string> seedText = new List<string>() { $"{opt.currentSeed}" };
				seedText.AddRange(args);
				File.WriteAllLines("seed.txt", seedText);

				if (opt.currentSeed >= opt.seedStart + opt.seedCount) break;

				if (opt.biome == "full")
				{
					List<string> biomes = BiomeData.nameToColor.Keys.ToList();
					MapGenerator gen = new MapGenerator();
					gen.ProvideMap(biomes, opt);
				}
				else if (opt.biome == "mainpath")
				{
					List<string> biomes = new List<String>() { "coalmine", "excavationsite", "snowcave", "snowcastle", "rainforest", "rainforest_open", "vault", "crypt" };
					MapGenerator gen = new MapGenerator();
					gen.ProvideMap(biomes, opt);
				}
				else if(opt.biome == "tower")
				{
					List<string> biomes = new List<String>() { "solid_wall_tower_1", "solid_wall_tower_2", "solid_wall_tower_3", "solid_wall_tower_4", "solid_wall_tower_5", "solid_wall_tower_6", "solid_wall_tower_7", "solid_wall_tower_8" };
					MapGenerator gen = new MapGenerator();
					gen.ProvideMap(biomes, opt);
				}
				else if(BiomeData.nameToColor.Keys.Any(s => opt.biome.Contains(s)))
				{
					MapGenerator gen = new MapGenerator();
					gen.ProvideMap(opt.biome.Split(' ', StringSplitOptions.TrimEntries).ToList(), opt);

				}

				opt.currentSeed += opt.batch;
				i++;
				DateTime endTime = DateTime.Now;
				TimeSpan fullExec = endTime - startTime;
				if (opt.loggingLevel >= 1) Console.WriteLine($"Batch {i}: {fullExec.TotalSeconds} sec");
			}
		});
		DateTime lEndTime = DateTime.Now;
		TimeSpan lFullExec = lEndTime - lStartTime;
		Console.WriteLine($"Full search: {lFullExec.TotalSeconds} sec");
	}
}

