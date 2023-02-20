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

	[Option('l', "loot-search", Required = false, HelpText = "Loot to search for in chests. Check the readme for details on usage.", Default = "sampo")]
	public string lootSearch { get; set; }

	[Option("greed", Required = false, HelpText = "Activate greed curse.", Default = false)]
	public bool greedCurse { get; set; }

	[Option("pedestals", Required = false, HelpText = "Check potion/item pedestals.", Default = false)]
	public bool checkItems { get; set; }

	[Option("potions", Required = false, HelpText = "Check potion material contents.", Default = false)]
	public bool potionContents { get; set; }

	[Option("spells", Required = false, HelpText = "Check random spell contents.", Default = false)]
	public bool spellContents { get; set; }

	[Option('a', "aggregate-items", Required = false, HelpText = "Expands multi-item search scope to the entire world instead of single chests.", Default = false)]
	public bool aggregate { get; set; }

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

	public uint currentSeed;
	public List<string> lootSeparated;

	public List<string> lootPositive = new();
	public List<string> lootNegative = new();
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

			opt.lootSeparated = opt.lootSearch.Split(" ").ToList();
			foreach(string s in opt.lootSeparated)
			{
				if(s.StartsWith("-")) opt.lootNegative.Add(s);
				else if(s != "") opt.lootPositive.Add(s);
			}
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
			if (opt.outputPath != "" && !opt.continueLast) File.Delete(opt.outputPath);
			while (true)
			{
				DateTime startTime = DateTime.Now; 
				List<string> seedText = new List<string>() { $"{opt.currentSeed}" };
				seedText.AddRange(args);
				File.WriteAllLines("seed.txt", seedText);

				if (opt.currentSeed >= opt.seedStart + opt.seedCount) break;

				if (opt.biome == "full")
				{
					List<string> biomes = STATICDATA.nameToColor.Keys.ToList();
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
				else if(STATICDATA.nameToColor.Keys.Any(s => opt.biome.Contains(s)))
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

