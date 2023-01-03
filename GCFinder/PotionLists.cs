namespace GCFinder;

public class PotionLists
{
	public static string[] materials_standard = new string[]
	{
		"lava",
		"water",
		"blood",
		"alcohol",
		"oil",
		"slime",
		"acid",
		"radioactive_liquid",
		"gunpowder_unstable",
		"liquid_fire",
		"blood_cold"
	};

	public static string[] materials_magic = new string[]
	{
		"magic_liquid_unstable_teleportation",
		"magic_liquid_polymorph",
		"magic_liquid_random_polymorph",
		"magic_liquid_berserk",
		"magic_liquid_charm",
		"magic_liquid_invisibility",
		"material_confusion",
		"magic_liquid_movement_faster",
		"magic_liquid_faster_levitation",
		"magic_liquid_worm_attractor",
		"magic_liquid_protection_all",
		"magic_liquid_mana_regeneration",
	};

	public static string[] materials_secret = new string[]
	{
		"magic_liquid_hp_regeneration_unstable",
		"blood_worm",
		"gold",
		"creepy_liquid",
		"glowshroom",
		"bush_seed",
		"cement",
		"salt",
		"sodium",
		"mushroom_seed",
		"plant_seed",
		"urine",
		"purifying_powder"
	};

	public static string[] materials_sands = new string[]
	{
		"mud",
		"concrete_sand",
		"sand",
		"bone",
		"soil",
		"sandstone",
		"fungisoil",
		"honey",
		"glue",
		"explosion_dirt",
		"snow",
		"snow_sticky",
		"rotten_meat",
		"meat_slime_sand",
		"rotten_meat_radioactive",
		"ice",
		"sand_herb",
		"wax",
		"gold",
		"silver",
		"copper",
		"brass",
		"diamond",
		"coal",
		"sulphur",
		"salt",
		"sodium_unstable",
		"gunpowder",
		"gunpowder_explosive",
		"gunpowder_tnt",
		"gunpowder_unstable",
		"gunpowder_unstable_big",
		"monster_powder_test",
		"rat_powder",
		"fungus_powder",
		"orb_powder",
		"gunpowder_unstable_boss_limbs",
		"plastic_red",
		"grass",
		"grass_ice",
		"grass_dry",
		"fungi",
		"spore",
		"moss",
		"plant_material",
		"plant_material_red",
		"ceiling_plant_material",
		"mushroom_seed",
		"plant_seed",
		"mushroom",
		"mushroom_giant_red",
		"mushroom_giant_blue",
		"glowshroom",
		"bush_seed",
		"poo",
		"glass_broken",
		"moss_rust",
		"fungi_creeping_secret",
		"fungi_creeping",
		"grass_dark",
		"fungi_green",
		"shock_powder",
		"fungus_powder_bad",
		"burning_powder",
		"purifying_powder",
		"sodium",
		"metal_sand",
		"steel_sand",
		"gold_radioactive",
		"endslime_blood",
		"sandstone_surface",
		"soil_dark",
		"soil_dead",
		"soil_lush_dark",
		"soil_lush",
		"sand_petrify",
		"lavasand",
		"sand_surface",
		"sand_blue",
		"plasma_fading_pink",
		"plasma_fading_green"
	};

	public static string[] materials_liquids = new string[]
	{
		"water",
		"water_temp",
		"water_ice",
		"water_swamp",
		"oil",
		"alcohol",
		"sima",
		"juhannussima",
		"magic_liquid",
		"material_confusion",
		"material_darkness",
		"material_rainbow",
		"magic_liquid_movement_faster",
		"magic_liquid_faster_levitation",
		"magic_liquid_faster_levitation_and_movement",
		"magic_liquid_worm_attractor",
		"magic_liquid_protection_all",
		"magic_liquid_mana_regeneration",
		"magic_liquid_unstable_teleportation",
		"magic_liquid_teleportation",
		"magic_liquid_hp_regeneration",
		"magic_liquid_hp_regeneration_unstable",
		"magic_liquid_polymorph",
		"magic_liquid_random_polymorph",
		"magic_liquid_unstable_polymorph",
		"magic_liquid_berserk",
		"magic_liquid_charm",
		"magic_liquid_invisibility",
		"cloud_radioactive",
		"cloud_blood",
		"cloud_slime",
		"swamp",
		"blood",
		"blood_fading",
		"blood_fungi",
		"blood_worm",
		"porridge",
		"blood_cold",
		"radioactive_liquid",
		"radioactive_liquid_fading",
		"plasma_fading",
		"gold_molten",
		"wax_molten",
		"silver_molten",
		"copper_molten",
		"brass_molten",
		"glass_molten",
		"glass_broken_molten",
		"steel_molten",
		"creepy_liquid",
		"cement",
		"slime",
		"slush",
		"vomit",
		"plastic_red_molten",
		"acid",
		"lava",
		"urine",
		"rocket_particles",
		"peat",
		"plastic_prop_molten",
		"plastic_molten",
		"slime_yellow",
		"slime_green",
		"aluminium_oxide_molten",
		"steel_rust_molten",
		"metal_prop_molten",
		"aluminium_robot_molten",
		"aluminium_molten",
		"metal_nohit_molten",
		"metal_rust_molten",
		"metal_molten",
		"metal_sand_molten",
		"steelsmoke_static_molten",
		"steelmoss_static_molten",
		"steelmoss_slanted_molten",
		"steel_static_molten",
		"plasma_fading_bright",
		"radioactive_liquid_yellow",
		"cursed_liquid",
		"poison",
		"blood_fading_slow",
		"midas",
		"midas_precursor",
		"liquid_fire_weak",
		"liquid_fire",
		"void_liquid",
		"water_salt",
		"water_fading",
		"pea_soup"
	};

	public static string random_from_array(NoitaRandom rnd, string[] arr)
	{
		int idx = rnd.Random(0, arr.Length - 1);
		return arr[idx];
	}

	public static string PotionContents(string potionType, int x, int y, uint seed)
	{
		NoitaRandom rnd = new NoitaRandom(seed);
		rnd.SetRandomSeed(x - 4.5, y - 4);
		string ret;
		if (potionType == "potion_normal") {
			if (rnd.Random(0, 100) <= 75)
			{
				if (rnd.Random(0, 100000) <= 50)
					ret = "magic_liquid_hp_regeneration";
				else if (rnd.Random(200, 100000) <= 250)
					ret = "purifying_powder";
				else
					ret = random_from_array(rnd, materials_magic);
			}
			else
				ret = random_from_array(rnd, materials_standard);
		}
		else if (potionType == "potion_secret") {
			ret = random_from_array(rnd, materials_secret);
		}
		else if (potionType == "potion_random_material") {
			if (rnd.Random(0, 100) <= 50)
				ret = random_from_array(rnd, materials_liquids);
			else
				ret = random_from_array(rnd, materials_sands);
		}
		else ret = "ERR";
		//Console.WriteLine($"PotionContents {seed} ({x}, {y}): {potionType} => {ret}");
		return ret;
	}
}