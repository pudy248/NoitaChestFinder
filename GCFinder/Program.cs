using GCFinder;
using CommandLine;
public class ConfigState

{
	[Option('z', "batch-size", Required = false, HelpText = "Batch size for searches.", Default = 10000U)]
	public uint batch { get; set; }

	[Option('s', "start-seed", Required = false, HelpText = "Starting seed for searches.", Default = 1U)]
	public uint seedStart { get; set; }

	[Option('i', "seed-count", Required = false, HelpText = "Total number of seeds to search.", Default = 2147483647U)]
	public uint seedCount { get; set; }

	[Option('c', "continue", Required = false, HelpText = "Continue from where the last execution ended.", Default = false)]
	public bool fromLastSeed { get; set; }

	[Option('b', "biome", Required = false, HelpText = "Biome to search. Check the readme for details on usage.", Default = "coalmine")]
	public string biome { get; set; }

	[Option('p', "parallel-worlds", Required = false, HelpText = "Number of parallel worlds in either direction to search. 0 only searches the main world.", Default = 0U)]
	public uint pwCount { get; set; }

	[Option('n', "ng-plus", Required = false, HelpText = "NG+ number to search. Currently only supports NG.", Default = 0U)]
	public uint ngPlus { get; set; }

	[Option('l', "loot-search", Required = false, HelpText = "Loot to search for in chests. Check the readme for details on usage.", Default = "sampo")]
	public string lootSearch { get; set; }

	[Option('g', "greed-curse", Required = false, HelpText = "Is the greed curse active?", Default = false)]
	public bool greedCurse { get; set; }

	[Option('e', "search-potions", Required = false, HelpText = "Should potion contents be computed?", Default = false)]
	public bool potionContents { get; set; }

	[Option('o', "output-path", Required = false, HelpText = "File to write outputs to. Leave blank to only log to the console.", Default = "out.txt")]
	public string outputPath { get; set; }

	[Option("max-items-per-chest", Required = false, HelpText = "Maximum number of items per chest to store. Overflow items will not be included in search. Increases VRAM usage.", Default = 25U)]
	public uint maxChestContents { get; set; }

	[Option("max-chests-per-biome", Required = false, HelpText = "Maximum number of chests per biome to store. Increases VRAM usage.", Default = 10U)]
	public uint maxChestsPerBiome { get; set; }

	[Option('d', "debug-logging-level", Required = false, HelpText = "Debug logging level.", Default = 1U)]
	public uint loggingLevel { get; set; }

	[Option('t', "max-tries", Required = false, HelpText = "Maximum generation attempts.", Default = 10U)]
	public uint maxTries { get; set; }

	public uint currentSeed;
	public List<string> lootSeparated;

	public List<string> lootPositive = new();
	public List<string> lootNegative = new();
}

public class Program
{
	static void Main(string[] args)
	{
		DateTime lStartTime = DateTime.Now;

		Parser.Default.ParseArguments<ConfigState>(args).WithParsed(opt =>
		{
			opt.lootSeparated = opt.lootSearch.Split(" ").ToList();
			foreach(string s in opt.lootSeparated)
			{
				if(s.StartsWith("-")) opt.lootNegative.Add(s);
				else opt.lootPositive.Add(s);
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

			if (!opt.fromLastSeed) File.WriteAllText("seed.txt", $"{opt.seedStart}");
			if (opt.outputPath != "" && (!opt.fromLastSeed || !File.Exists("seed.txt"))) File.Delete(opt.outputPath);
			while (true)
			{
				DateTime startTime = DateTime.Now;
				if (!File.Exists("seed.txt")) File.WriteAllText("seed.txt", opt.seedStart.ToString());
				string currentFile = File.ReadAllText("seed.txt");
				uint currentSeed = uint.Parse(currentFile);

				if (currentSeed >= opt.seedStart + opt.seedCount) break;
				opt.currentSeed = currentSeed;

				if (opt.biome == "full")
				{
					foreach(string biome in STATICDATA.nameToColor.Keys)
					{
						DateTime bStartTime = DateTime.Now;
						MapGenerator gen = new MapGenerator();
						gen.ProvideBlock(biome, opt);
						DateTime bEndTime = DateTime.Now;
						TimeSpan bFullExec = bEndTime - bStartTime;
						if (opt.loggingLevel >= 1) Console.WriteLine($"Batch {i} ({biome}): {bFullExec.TotalSeconds} sec");
					}
				}
				else if (opt.biome == "mainpath")
				{
					foreach (string biome in new string[] {"coalmine", "excavationsite", "snowcave", "snowcastle", "rainforest", "rainforest_open", "vault", "crypt"})
					{
						DateTime bStartTime = DateTime.Now;
						MapGenerator gen = new MapGenerator();
						gen.ProvideBlock(biome, opt);
						DateTime bEndTime = DateTime.Now;
						TimeSpan bFullExec = bEndTime - bStartTime;
						if (opt.loggingLevel >= 1) Console.WriteLine($"Batch {i} ({biome}): {bFullExec.TotalSeconds} sec");
					}
				}
				else
				{
					MapGenerator gen = new MapGenerator();
					gen.ProvideBlock(opt.biome, opt);

				}

				currentSeed += opt.batch;
				File.WriteAllText("seed.txt", $"{currentSeed}");
				i++;
				DateTime endTime = DateTime.Now;
				TimeSpan fullExec = endTime - startTime;
				if (opt.loggingLevel >= 1) Console.WriteLine($"Batch {i}: {fullExec.TotalSeconds} sec");
			}
		});
		DateTime lEndTime = DateTime.Now;
		TimeSpan lFullExec = lEndTime - lStartTime;
		Console.WriteLine($"Full search: {lFullExec.TotalSeconds} sec");


		/*
		Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
		{
			GCFinder.MapDatatypes.STATICDATA.READ_ALL();
			MapGenerator.DO_IMPLEMENTATION_LOGS = false;

			if (o.mode == 0)
			{
				DateTime lStartTime = DateTime.Now;
				int i = 0;
				while (i < o.loop / o.batch)
				{
					DateTime startTime = DateTime.Now;
					try
					{
						uint currentSeedNum = o.seed;
						int currentChestNum = 0;
						if (o.fromLastSeed)
						{
							if (!File.Exists("seed.txt")) File.WriteAllText("seed.txt", "0,0");
							string currentFile = File.ReadAllText("seed.txt");
							currentSeedNum = uint.Parse(currentFile.Split(",")[0]);
							currentChestNum = int.Parse(currentFile.Split(",")[1]);
						}

						MapGenerator gen = new MapGenerator();
						if (o.fullMap) gen.CheckFullMap((uint)j, 0, o);
						else gen.Provide(36, 14, (uint)j, 0, 1, o);
						currentChestNum += gen.chestCounter;

						currentSeedNum += (uint)o.batch;
						File.WriteAllText("seed.txt", $"{currentSeedNum},{currentChestNum}");
						i++;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}

					DateTime endTime = DateTime.Now;
					TimeSpan fullExec = endTime - startTime;
					Console.WriteLine($"Batch {i}: {fullExec.TotalSeconds} sec");
				}
				DateTime lEndTime = DateTime.Now;
				TimeSpan lFullExec = lEndTime - lStartTime;
				Console.WriteLine($"Full search: {lFullExec.TotalSeconds} sec");
			}
			else if (o.mode == 1)
			{
				DateTime startTime = DateTime.Now;

				uint currentSeedNum = o.seed;
				int currentChestNum = 0;
				if (o.fromLastSeed)
				{
					if (!File.Exists("seed.txt")) File.WriteAllText("seed.txt", "0,0");
					string currentFile = File.ReadAllText("seed.txt");
					currentSeedNum = uint.Parse(currentFile.Split(",")[0]);
					currentChestNum = int.Parse(currentFile.Split(",")[1]);
				}

				for(uint i = currentSeedNum; i < currentSeedNum + o.loop; i++)
				{
					MapGenerator gen = new MapGenerator();
					if (o.fullMap) gen.CheckFullMap(i, 0, o);
					else gen.Provide(36, 14, i, 0, 1, o);
					currentChestNum += gen.chestCounter;
				}

				currentSeedNum += (uint)o.batch;
				File.WriteAllText("seed.txt", $"{currentSeedNum},{currentChestNum}");

				DateTime endTime = DateTime.Now;
				TimeSpan fullExec = endTime - startTime;
				Console.WriteLine($"Completed in {fullExec.TotalSeconds} sec");
			}
			else if (o.mode == 2)
			{
				DateTime startTime = DateTime.Now;

				uint currentSeedNum = o.seed;

				MapGenerator gen = new MapGenerator();
				if (o.fullMap) gen.CheckFullMap(currentSeedNum, 0, o);
				else gen.Provide(36, 14, currentSeedNum, 0, 1, o);

				DateTime endTime = DateTime.Now;
				TimeSpan fullExec = endTime - startTime;
				Console.WriteLine($"Completed in {fullExec.TotalSeconds} sec");
			}
		});*/
	}
}

