using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace GCFinder;
public static class BiomeData
{
	public static Dictionary<string, string> nameToColor = new()
	{
		{ "coalmine", "d57917" },
		{ "coalmine_alt", "d56517" },
		{ "excavationsite", "124445" },
		{ "fungicave", "e861f0" },
		{ "snowcave", "1775d5" },
		{ "snowcastle", "0046ff" },
		{ "rainforest", "808000" },
		{ "rainforest_open", "a08400" },
		{ "rainforest_dark", "375c00" },
		{ "vault", "008000" },
		{ "crypt", "786c42" },
		{ "wandcave", "006c42" },
		{ "vault_frozen", "0080a8" },
		//{ "the_end", "3c0f0a" },
		//{ "the_sky", "d3e6f0" },
		{ "wizardcave", "726186" },
		{ "sandcave", "e1cd32" },
		//{ "pyramid", "967f11" },
		{ "fungiforest", "a861ff" },
		{ "solid_wall_tower_1", "3d3e37" },
		{ "solid_wall_tower_2", "3d3e38" },
		{ "solid_wall_tower_3", "3d3e39" },
		{ "solid_wall_tower_4", "3d3e3a" },
		{ "solid_wall_tower_5", "3d3e3b" },
		{ "solid_wall_tower_6", "3d3e3c" },
		{ "solid_wall_tower_7", "3d3e3d" },
		{ "solid_wall_tower_8", "3d3e3e" },
		{ "solid_wall_tower_9", "3d3e3f" },
		{ "robobase", "4e5267" },
	};

	public static Dictionary<string, byte> biomeIndices = new()
	{
		{ "coalmine", 0 },
		{ "coalmine_alt", 1 },
		{ "excavationsite", 2 },
		{ "fungicave", 3 },
		{ "snowcave", 4 },
		{ "snowcastle", 5 },
		{ "rainforest", 6 },
		{ "rainforest_open", 7 },
		{ "rainforest_dark", 8 },
		{ "vault", 9 },
		{ "crypt", 10 },
		{ "wandcave", 11 },
		{ "vault_frozen", 12 },
		//{ "the_end", "3c0f0a" },
		//{ "the_sky", "d3e6f0" },
		{ "wizardcave", 13 },
		{ "sandcave", 14 },
		//{ "pyramid", "967f11" },
		{ "fungiforest", 15 },
		{ "solid_wall_tower_1", 16 },
		{ "solid_wall_tower_2", 17 },
		{ "solid_wall_tower_3", 18 },
		{ "solid_wall_tower_4", 19 },
		{ "solid_wall_tower_5", 20 },
		{ "solid_wall_tower_6", 21 },
		{ "solid_wall_tower_7", 22 },
		{ "solid_wall_tower_8", 23 },
		{ "solid_wall_tower_9", 24 },
		{ "robobase", 25 },
	};

	public static Dictionary<string, Image<Rgb24>> colorToWang = new()
	{
		{ "d57917", Image.Load<Rgb24>("wang_tiles/coalmine.png") },
		{ "d56517", Image.Load<Rgb24>("wang_tiles/coalmine_alt.png") },
		{ "124445", Image.Load<Rgb24>("wang_tiles/excavationsite.png") },
		{ "e861f0", Image.Load<Rgb24>("wang_tiles/fungicave.png") },
		{ "1775d5", Image.Load<Rgb24>("wang_tiles/snowcave.png") },
		{ "0046ff", Image.Load<Rgb24>("wang_tiles/snowcastle.png") },
		{ "808000", Image.Load<Rgb24>("wang_tiles/rainforest.png") },
		{ "a08400", Image.Load<Rgb24>("wang_tiles/rainforest_open.png") },
		{ "375c00", Image.Load<Rgb24>("wang_tiles/rainforest_dark.png") },
		{ "008000", Image.Load<Rgb24>("wang_tiles/vault.png") },
		{ "786c42", Image.Load<Rgb24>("wang_tiles/crypt.png") },
		{ "006c42", Image.Load<Rgb24>("wang_tiles/wand.png") },
		{ "0080a8", Image.Load<Rgb24>("wang_tiles/vault_frozen.png") },
		//{ "3c0f0a", Image.Load<Rgb24>("wang_tiles/the_end.png") },
		//{ "d3e6f0", Image.Load<Rgb24>("wang_tiles/the_sky.png") },
		{ "726186", Image.Load<Rgb24>("wang_tiles/wizardcave.png") },
		{ "e1cd32", Image.Load<Rgb24>("wang_tiles/sandcave.png") },
		//{ "967f11", Image.Load<Rgb24>("wang_tiles/pyramid.png") },
		{ "a861ff", Image.Load<Rgb24>("wang_tiles/fungiforest.png") },
		{ "3d3e37", Image.Load<Rgb24>("wang_tiles/coalmine.png") },
		{ "3d3e38", Image.Load<Rgb24>("wang_tiles/excavationsite.png") },
		{ "3d3e39", Image.Load<Rgb24>("wang_tiles/snowcave.png") },
		{ "3d3e3a", Image.Load<Rgb24>("wang_tiles/snowcastle.png") },
		{ "3d3e3b", Image.Load<Rgb24>("wang_tiles/fungicave.png") },
		{ "3d3e3c", Image.Load<Rgb24>("wang_tiles/rainforest.png") },
		{ "3d3e3d", Image.Load<Rgb24>("wang_tiles/vault.png") },
		{ "3d3e3e", Image.Load<Rgb24>("wang_tiles/crypt.png") },
		{ "3d3e3f", Image.Load<Rgb24>("wang_tiles/the_end.png") },
		{ "4e5267", Image.Load<Rgb24>("wang_tiles/robobase.png") },
	};

	public static Dictionary<string, List<MapArea>> colorToArea = new()
	{
		{ "d57917", new() { new() { x1 = 34, y1 = 14, w = 5, h = 2 } } },
		{ "d56517", new() { new() { x1 = 32, y1 = 15, w = 2, h = 1 } } },
		{ "124445", new() { new() { x1 = 31, y1 = 17, w = 8, h = 2 } } },
		{ "e861f0", new() { new() { x1 = 28, y1 = 17, w = 3, h = 1 } } },
		{ "1775d5", new() { new() { x1 = 30, y1 = 20, w = 10, h = 3 } } },
		{ "0046ff", new() { new() { x1 = 31, y1 = 24, w = 7, h = 2 } } },
		{ "808000", new() { new() { x1 = 30, y1 = 27, w = 9, h = 2 } } },
		{ "a08400", new() { new() { x1 = 30, y1 = 28, w = 9, h = 2 } } },
		{ "375c00", new() { new() { x1 = 25, y1 = 26, w = 5, h = 8 } } },
		{ "008000", new() { new() { x1 = 29, y1 = 31, w = 11, h = 3 } } },
		{ "786c42", new() { new() { x1 = 26, y1 = 35, w = 14, h = 4 } } },
		{ "006c42", new() { new() { x1 = 27, y1 = 21, w = 3, h = 1 }, new() { x1 = 41, y1 = 36, w = 6, h = 1 }, new() { x1 = 47, y1 = 35, w = 4, h = 4 }, new() { x1 = 53, y1 = 36, w = 2, h = 2 }, new() { x1 = 53, y1 = 39, w = 5, h = 1 } } },
		{ "0080a8", new() { new() { x1 = 12, y1 = 15, w = 7, h = 5 } } },
		//{ "3c0f0a", new() { new() { x1 = 25, y1 = 43, w = 18, h = 5 } } },
		//{ "d3e6f0", new() { new() { x1 = 27, y1 = 0, w = 15, h = 1 } } },
		{ "726186", new() { new() { x1 = 23, y1 = 25, w = 3, h = 2 }, new() { x1 = 47, y1 = 36, w = 2, h = 2 }, new() { x1 = 51, y1 = 36, w = 8, h = 3 }, new() { x1 = 53, y1 = 40, w = 6, h = 6 } } },
		{ "e1cd32", new() { new() { x1 = 51, y1 = 15, w = 7, h = 5 }, new() { x1 = 59, y1 = 24, w = 7, h = 6 } } },
		//{ "967f11", new() { new() { x1 = 52, y1 = 12, w = 5, h = 3 } } },
		{ "a861ff", new() { new() { x1 = 58, y1 = 35, w = 4, h = 6 }, new() { x1 = 59, y1 = 16, w = 7, h = 9 } } },

		{ "3d3e37", new() { new() { x1 = 53, y1 = 31, w = 3, h = 1 } } },
		{ "3d3e38", new() { new() { x1 = 53, y1 = 30, w = 3, h = 1 } } },
		{ "3d3e39", new() { new() { x1 = 53, y1 = 29, w = 3, h = 1 } } },
		{ "3d3e3a", new() { new() { x1 = 53, y1 = 28, w = 3, h = 1 } } },
		{ "3d3e3b", new() { new() { x1 = 53, y1 = 27, w = 3, h = 1 } } },
		{ "3d3e3c", new() { new() { x1 = 53, y1 = 26, w = 3, h = 1 } } },
		{ "3d3e3d", new() { new() { x1 = 53, y1 = 25, w = 3, h = 1 } } },
		{ "3d3e3e", new() { new() { x1 = 53, y1 = 24, w = 3, h = 1 } } },
		{ "3d3e3f", new() { new() { x1 = 53, y1 = 23, w = 3, h = 1 } } },
		{ "4e5267", new() { new() { x1 = 59, y1 = 29, w = 7, h = 9 } } },
	};

	public static Image<Rgb24> biomeMap = Image.Load<Rgb24>("biome_map.png");
}

public class MapArea
{
	public int x1;
	public int y1;
	public int w;
	public int h;


	public int ng_x1;
	public int ng_y1;
	public int ng_w;
	public int ng_h;
}

public class Chest
{
	public uint seed;
	public int x;
	public int y;
	public List<string> contents;
}

public class MapGenerator
{
	public ConfigState options;
	
	public void SearchEOE(ConfigState o)
	{
		options = o;
		List<Chest> chests = Wang.GenerateEOEChests(o);
		List<Chest> filteredChests = Wang.FilterEOEChestList(chests, o);
		SortAndWriteResults(filteredChests);
	}

	public void ProvideMap(List<string> biomes, ConfigState o)
	{
		options = o;

		List<Chest>[,] allChests = new List<Chest>[biomes.Count, o.batch];
		for(int i = 0; i < biomes.Count; i++)
		{
			string biome = biomes[i];
			List<Chest>[] biomeChests = ProvideBlock(biome);
			Parallel.For(0, options.batch, j =>
			{
				allChests[i, j] = new();
				allChests[i,j].AddRange(biomeChests[j]);
			});
		}

		List<Chest> filteredChests = Wang.FilterChestList(allChests, o, biomes);
		SortAndWriteResults(filteredChests);
	}

	public List<Chest>[] ProvideBlock(string biome)
	{
		if(!BiomeData.nameToColor.ContainsKey(biome))
		{
			Console.WriteLine($"Invalid biome: {biome}");
			return new List<Chest>[0];
		}
		string color = BiomeData.nameToColor[biome];
		List<MapArea> area = BiomeData.colorToArea[color];
		Image<Rgb24> wangMap = BiomeData.colorToWang[color];
		List<Chest>[] aggregateRet = new List<Chest>[options.batch];
		for (int i = 0; i < options.batch; i++) aggregateRet[i] = new();

		for (int i = 0; i < area.Count; i++)
		{
			int x1 = options.ngPlus > 0 ? area[i].ng_x1 : area[i].x1;
			int y1 = options.ngPlus > 0 ? area[i].ng_y1 : area[i].y1;
			int w = options.ngPlus > 0 ? area[i].ng_w : area[i].w;
			int h = options.ngPlus > 0 ? area[i].ng_h : area[i].h;

			if (w == 0 || h == 0) continue;

			int map_w = Wang.GetWidthFromPix(x1, x1 + w);
			int map_h = Wang.GetWidthFromPix(y1, y1 + h);

			List<Chest>[] chests = Wang.GenerateMap(
				wangMap,
				(uint)wangMap.Width,
				(uint)wangMap.Height,
				(uint)map_w,
				(uint)map_h,
				biome == "coalmine",
				BiomeData.biomeIndices[biome],
				x1,
				y1,
				options
			);
			Parallel.For(0, options.batch, j =>
			{
				aggregateRet[j].AddRange(chests[j]);
			});
		}
		return aggregateRet;
	}

	public void SortAndWriteResults(List<Chest> chests)
	{
		chests = chests.OrderBy(c => c.seed).ToList();

		StreamWriter file = null;
		if (options.outputPath != "") file = new(options.outputPath, true);

		for (int j = 0; j < chests.Count; j++)
		{
			string output = $"{chests[j].seed} at ({chests[j].x}, {chests[j].y}) contains a ";
			for (int k = 0; k < chests[j].contents.Count; k++)
			{
				output += chests[j].contents[k] + ", ";
			}
			output = output.Substring(0, output.Length - 2) + ".";
			Console.WriteLine(output);
			if (options.outputPath != "") file.WriteLine(output);
		}
		if (options.outputPath != "") file.Close();
	}
}