namespace GCFinder;

public static class WandGen
{
	public enum ACTION_TYPE
	{
		PROJECTILE = 0,
		STATIC_PROJECTILE = 1,
		MODIFIER = 2,
		DRAW_MANY = 3,
		MATERIAL = 4,
		OTHER = 5,
		UTILITY = 6,
		PASSIVE = 7
	}

	public struct Wand
	{
		public int level;
		public bool isBetter;

		public float cost;
		public float capacity;
		public int multicast;
		public int mana;
		public int regen;
		public int delay;
		public int reload;
		public float speed;
		public int spread;
		public bool shuffle;

		public float prob_unshuffle;
		public float prob_draw_many;
		public bool force_unshuffle;
		public bool is_rare;

		public Spell? alwaysCast;
		public List<Spell> spells;
	}

	public struct Spell
	{
		public string id;
		public ACTION_TYPE type;
		public double[] spawn_probabilities;

		public Spell(string _id, ACTION_TYPE _type, double[] _spawn_probabilities)
		{
			this.id = _id;
			this.type = _type;
			this.spawn_probabilities = _spawn_probabilities;
		}
	}
	
	public static string[] ExpandWand(Wand w)
	{
		List<string> strs = new List<string>()
		{
			$"wand_{w.capacity.ToString("F0")}_{w.multicast}_{(w.delay / 60f).ToString("F2")}_{(w.reload / 60f).ToString("F2")}_{w.mana}_{w.regen}_{w.spread}_{w.speed.ToString("F3")}_{(w.shuffle ? "shuffle" : "nonshuffle")}"
		};
		if (w.alwaysCast.HasValue) strs.Add($"ac_spell_{w.alwaysCast.Value.id.ToLower()}");
		strs.AddRange(w.spells.Select(x => "spell_" + x.id.ToLower()));
		return strs.ToArray();
	}

	public static bool WandChecksPassed(string wandStr, List<WandCheck> checks)
	{
		string[] stats = wandStr.Split('_');
		bool anyFailed = false;
		foreach(WandCheck check in checks)
		{
			string stat = "0";
			switch(check.stat)
			{
				case "capacity":
					stat = stats[1];
					break;
				case "multicast":
					stat = stats[2];
					break;
				case "delay":
					stat = stats[3];
					break;
				case "reload":
					stat = stats[4];
					break;
				case "mana":
					stat = stats[5];
					break;
				case "regen":
					stat = stats[6];
					break;
				case "spread":
					stat = stats[7];
					break;
				case "speed":
					stat = stats[8];
					break;
				case "shuffle":
					stat = stats[9] == "shuffle" ? "1" : "0";
					break;
				default:
					Console.WriteLine($"Unrecognized wand stat: {check.stat}");
					break;
			}
			float val = float.Parse(stat);
			bool passed = false;
			switch (check.comparison)
			{
				case WandCheck.Comparison.Greater:
					passed = val > check.value;
					break;
				case WandCheck.Comparison.Geq:
					passed = val >= check.value;
					break;
				case WandCheck.Comparison.Equal:
					passed = val == check.value;
					break;
				case WandCheck.Comparison.Leq:
					passed = val <= check.value;
					break;
				case WandCheck.Comparison.Less:
					passed = val < check.value;
					break;
			}
			if(!passed) anyFailed = true;
		}
		return !anyFailed;
	}

	public static Wand GetWandWithLevel(uint seed, double x, double y, int level, bool nonshuffle, bool better)
	{
		if(nonshuffle)
			switch(level)
			{
				case 1:
					return GetWand(seed, x, y, 25, 1, true);
				case 2:
					return GetWand(seed, x, y, 40, 2, true);
				case 3:
					return GetWand(seed, x, y, 60, 3, true);
				case 4:
					return GetWand(seed, x, y, 80, 4, true);
				case 5:
					return GetWand(seed, x, y, 100, 5, true);
				case 6:
					return GetWand(seed, x, y, 120, 6, true);
				default:
					return GetWand(seed, x, y, 180, 11, true);
			}
		else if(better)
			switch(level)
			{
				case 1:
					return GetWandBetter(seed, x, y, 30, 1);
				case 2:
					return GetWandBetter(seed, x, y, 40, 2);
				case 3:
					return GetWandBetter(seed, x, y, 60, 3);
				case 4:
					return GetWandBetter(seed, x, y, 80, 4);
				case 5:
					return GetWandBetter(seed, x, y, 100, 5);
				case 6:
					return GetWandBetter(seed, x, y, 120, 6);
			}
		else
			switch (level)
			{
				case 1:
					return GetWand(seed, x, y, 30, 1, false);
				case 2:
					return GetWand(seed, x, y, 40, 2, false);
				case 3:
					return GetWand(seed, x, y, 60, 3, false);
				case 4:
					return GetWand(seed, x, y, 80, 4, false);
				case 5:
					return GetWand(seed, x, y, 100, 5, false);
				case 6:
					return GetWand(seed, x, y, 120, 6, false);
			}
		return GetWand(seed, x, y, 10, 1, false);
	}

	public static Wand GetWand(uint seed, double x, double y, int cost, int level, bool force_unshuffle)
	{
		NoitaRandom random = new(seed);
		random.SetRandomSeed(x, y);
		Wand wand = GetWandStats(cost, level, force_unshuffle, ref random);
		AddRandomCards(ref wand, seed, x, y, level, ref random);

		return wand;
	}

	public static Wand GetWandBetter(uint seed, double x, double y, int cost, int level)
	{
		NoitaRandom random = new(seed);
		random.SetRandomSeed(x, y);
		Wand wand = GetWandStatsBetter(cost, level, ref random);
		AddRandomCardsBetter(ref wand, seed, x, y, level, ref random);

		return wand;
	}

	static Wand GetWandStats(int _cost, int level, bool force_unshuffle, ref NoitaRandom random)
	{
		Wand gun = new() { level = level };
		int cost = _cost;

		if (level == 1 && random.Random(0, 100) < 50)
			cost += 5;

		cost += random.Random(-3, 3);
		gun.cost = cost;
		gun.capacity = 0;
		gun.multicast = 0;
		gun.reload = 0;
		gun.shuffle = true;
		gun.delay = 0;
		gun.spread = 0;
		gun.speed = 0;
		gun.prob_unshuffle = 0.1f;
		gun.prob_draw_many = 0.15f;
		gun.regen = 50 * level + random.Random(-5, 5 * level);
		gun.mana = 50 + (150 * level) + random.Random(-5, 5) * 10;
		gun.force_unshuffle = false;
		gun.is_rare = false;

		int p = random.Random(0, 100);
		if (p < 20)
		{
			gun.regen = (50 * level + random.Random(-5, 5 * level)) / 5;
			gun.mana = (50 + (150 * level) + random.Random(5, 5) * 10) * 3;
		}

		p = random.Random(0, 100);
		if (p < 15)
		{
			gun.regen = (50 * level + random.Random(-5, 5 * level)) * 5;
			gun.mana = (50 + (150 * level) + random.Random(-5, 5) * 10) / 3;
		}

		if (gun.mana < 50) gun.mana = 50;
		if (gun.regen < 10) gun.regen = 10;

		p = random.Random(0, 100);
		if (p < 15 + level * 6)
			gun.force_unshuffle = true;

		p = random.Random(0, 100);
		if (p < 5)
		{
			gun.is_rare = true;
			gun.cost += 65;
		}

		string[] variables_01 = new string[] { "reload_time", "fire_rate_wait", "spread_degrees", "speed_multiplier" };
		string[] variables_02 = new string[] { "deck_capacity" };
		string[] variables_03 = new string[] { "shuffle_deck_when_empty", "actions_per_round" };

		shuffleTable(ref variables_01, ref random);
		if (!gun.force_unshuffle) shuffleTable(ref variables_03, ref random);

		foreach (string s in variables_01)
			applyRandomVariable(ref gun, s, statProbabilities, ref random);
		foreach (string s in variables_02)
			applyRandomVariable(ref gun, s, statProbabilities, ref random);
		foreach (string s in variables_03)
			applyRandomVariable(ref gun, s, statProbabilities, ref random);

		if (gun.cost > 5 && random.Random(0, 1000) < 995)
		{
			if (gun.shuffle)
				gun.capacity += (gun.cost / 5f);
			else
				gun.capacity += (gun.cost / 10f);
			gun.cost = 0;
		}
		gun.capacity = (float)Math.Floor(gun.capacity - 0.1f);

		if (force_unshuffle) gun.shuffle = false;
		if(random.Random(0, 10000) <= 9999)
		{
			gun.capacity = Math.Clamp(gun.capacity, 2, 26);
		}

		gun.capacity = Math.Max(gun.capacity, 2);

		if (gun.reload >= 60)
		{
			int rnd = 0;
			while(rnd < 70)
			{
				gun.multicast++;
				rnd = random.Random(0, 100);
			}

			if(random.Random(0, 100) < 50)
			{
				int new_multicast = (int)gun.capacity;
				for(int i = 1; i <= 6; i++)
				{
					int temp = random.Random(gun.multicast, (int)gun.capacity);
					if(temp < new_multicast)
						new_multicast = temp;
				}
				gun.multicast = new_multicast;
			}
		}

		gun.multicast = Math.Clamp(gun.multicast, 1, (int)gun.capacity);

		return gun;
	}

	static Wand GetWandStatsBetter(int _cost, int level, ref NoitaRandom random)
	{
		Wand gun = new() { level = level, isBetter = true };
		int cost = _cost;

		if (level == 1 && random.Random(0, 100) < 50)
			cost += 5;

		cost += random.Random(-3, 3);
		gun.cost = cost;
		gun.capacity = 0;
		gun.multicast = 0;
		gun.reload = 0;
		gun.shuffle = true;
		gun.delay = 0;
		gun.spread = 0;
		gun.speed = 0;
		gun.prob_unshuffle = 0.1f;
		gun.prob_draw_many = 0.15f;
		gun.regen = 50 * level + random.Random(-5, 5 * level);
		gun.mana = 50 + (150 * level) + random.Random(-5, 5) * 10;
		gun.force_unshuffle = false;
		gun.is_rare = false;

		int p = random.Random(0, 100);
		if (p < 20)
		{
			gun.regen = (50 * level + random.Random(-5, 5 * level)) / 5;
			gun.mana = (50 + (150 * level) + random.Random(5, 5) * 10) * 3;

			if (gun.mana < 50) gun.mana = 50;
			if (gun.regen < 10) gun.regen = 10;
		}

		p = random.Random(0, 100);
		if (p < 15 + level * 6)
			gun.force_unshuffle = true;

		p = random.Random(0, 100);
		if (p < 5)
		{
			gun.is_rare = true;
			gun.cost += 65;
		}

		string[] variables_01 = new string[] { "reload_time", "fire_rate_wait", "spread_degrees", "speed_multiplier" };
		string[] variables_02 = new string[] { "deck_capacity" };
		string[] variables_03 = new string[] { "shuffle_deck_when_empty", "actions_per_round" };

		shuffleTable(ref variables_01, ref random);
		if (!gun.force_unshuffle) shuffleTable(ref variables_03, ref random);

		foreach (string s in variables_01)
			applyRandomVariable(ref gun, s, statProbabilitiesBetter, ref random);
		foreach (string s in variables_02)
			applyRandomVariable(ref gun, s, statProbabilitiesBetter, ref random);
		foreach (string s in variables_03)
			applyRandomVariable(ref gun, s, statProbabilitiesBetter, ref random);

		if (gun.cost > 5 && random.Random(0, 1000) < 995)
		{
			if (gun.shuffle)
				gun.capacity += (gun.cost / 5f);
			else
				gun.capacity += (gun.cost / 10f);
			gun.cost = 0;
		}
		gun.capacity = (float)Math.Floor(gun.capacity - 0.1f);

		if (random.Random(0, 10000) <= 9999)
		{
			gun.capacity = Math.Clamp(gun.capacity, 2, 26);
		}

		gun.capacity = Math.Max(gun.capacity, 2);

		if (gun.reload >= 60)
		{
			int rnd = 0;
			while (rnd < 70)
			{
				gun.multicast++;
				rnd = random.Random(0, 100);
			}

			if (random.Random(0, 100) < 50)
			{
				int new_multicast = (int)gun.capacity;
				for (int i = 1; i < 6; i++)
				{
					int temp = random.Random(gun.multicast, (int)gun.capacity);
					if (temp < new_multicast)
						new_multicast = temp;
				}
				gun.multicast = new_multicast;
			}
		}

		gun.multicast = Math.Clamp(gun.multicast, 1, (int)gun.capacity);

		return gun;
	}

	static void AddRandomCards(ref Wand gun, uint seed, double x, double y, int _level, ref NoitaRandom random)
	{
		gun.spells = new();

		bool is_rare = gun.is_rare;
		int goodCards = 5;
		if (random.Random(0, 100) < 7) goodCards = random.Random(20, 50);
		if (is_rare) goodCards *= 2;

		int orig_level = _level;
		int level = _level - 1;
		int capacity = (int)gun.capacity;
		int multicast = gun.multicast;
		int cardCount = random.Random(1, 3);
		Spell bulletCard = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.PROJECTILE, 0);
		Spell card = spell_empty;
		int randomBullets = 0;
		int good_card_count = 0;

		if (random.Random(0, 100) < 50 && cardCount < 3) cardCount++;
		if (random.Random(0, 100) < 10 || is_rare) cardCount += random.Random(1, 2);

		goodCards = random.Random(5, 45);
		cardCount = random.Random ((int)(0.51f * capacity), capacity);
		cardCount = Math.Clamp(cardCount, 1, capacity - 1);

		if (random.Random(0, 100) < (orig_level * 10) - 5) randomBullets = 1;

		if (random.Random(0, 100) < 4 || is_rare)
		{
			int p = random.Random(0, 100);
			if (p < 77)
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.MODIFIER, 666);
			else if(p < 85)
			{
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.MODIFIER, 666);
				good_card_count++;
			}
			else if(p < 93)
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.STATIC_PROJECTILE, 666);
			else
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.PROJECTILE, 666);
			gun.alwaysCast = card;
		}

		if (random.Random(0, 100) < 50)
		{
			int extraLevel = level;
			while(random.Random(1, 10) == 10)
			{
				extraLevel++;
				bulletCard = GetRandomActionWithType(seed, x, y, extraLevel, ACTION_TYPE.PROJECTILE, 0);
			}

			if(cardCount < 3)
			{
				if(cardCount < 1 && random.Random(0, 100) < 20)
				{
					card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.MODIFIER, 2);
					gun.spells.Add(card);
					cardCount--;
				}

				for(int i = 0; i < cardCount; i++)
					gun.spells.Add(bulletCard);
			}
			else
			{
				if(random.Random(0, 100) < 40) 
				{
					card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.DRAW_MANY, 1);
					gun.spells.Add(card);
					cardCount--;
				}
				if (cardCount > 3 && random.Random(0, 100) < 40)
				{
					card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.DRAW_MANY, 1);
					gun.spells.Add(card);
					cardCount--;
				}
				if (random.Random(0, 100) < 80)
				{
					card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.MODIFIER, 2);
					gun.spells.Add(card);
					cardCount--;
				}

				for (int i = 0; i < cardCount; i++)
					gun.spells.Add(bulletCard);
			}
		}
		else
		{
			for(int i = 0; i < cardCount; i++)
			{
				if(random.Random(0, 100) < goodCards && cardCount > 2)
				{
					if (good_card_count == 0 && multicast == 1)
					{
						card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.DRAW_MANY, i + 1);
						good_card_count++;
					}
					else
					{
						if (random.Random(0, 100) < 83)
							card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.MODIFIER, i + 1);
						else
							card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.DRAW_MANY, i + 1);
					}

					gun.spells.Add(card);
				}
				else
				{
					gun.spells.Add(bulletCard);
					if(randomBullets == 1)
					{
						bulletCard = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.PROJECTILE, i + 1);
					}
				}
			}
		}
	}

	static void AddRandomCardsBetter(ref Wand gun, uint seed, double x, double y, int _level, ref NoitaRandom random)
	{
		gun.spells = new();

		bool is_rare = gun.is_rare;
		int goodCards = 5;
		if (random.Random(0, 100) < 7) goodCards = random.Random(20, 50);
		if (is_rare) goodCards *= 2;

		int orig_level = _level;
		int level = _level - 1;
		int capacity = (int)gun.capacity;
		int multicast = gun.multicast;
		int cardCount = random.Random(1, 3);
		Spell bulletCard = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.PROJECTILE, 0);
		Spell card = spell_empty;
		int randomBullets = 0;
		int good_card_count = 0;

		if (random.Random(0, 100) < 50 && cardCount < 3) cardCount++;
		if (random.Random(0, 100) < 10 || is_rare) cardCount += random.Random(1, 2);

		goodCards = random.Random(5, 45);
		cardCount = random.Random((int)(0.51f * capacity), capacity);
		cardCount = Math.Clamp(cardCount, 1, capacity - 1);

		if (random.Random(0, 100) < (orig_level * 10) - 5) randomBullets = 1;

		if (random.Random(0, 100) < 4 || is_rare)
		{
			int p = random.Random(0, 100);
			if (p < 77)
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.MODIFIER, 666);
			else if (p < 85)
			{
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.MODIFIER, 666);
				good_card_count++;
			}
			else if (p < 93)
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.STATIC_PROJECTILE, 666);
			else
				card = GetRandomActionWithType(seed, x, y, level + 1, ACTION_TYPE.PROJECTILE, 666);
			gun.alwaysCast = card;
		}

		if (cardCount < 3)
		{
			if (cardCount < 1 && random.Random(0, 100) < 20)
			{
				card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.MODIFIER, 2);
				gun.spells.Add(card);
				cardCount--;
			}

			for (int i = 0; i < cardCount; i++)
				gun.spells.Add(bulletCard);
		}
		else
		{
			if (random.Random(0, 100) < 40)
			{
				card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.DRAW_MANY, 1);
				gun.spells.Add(card);
				cardCount--;
			}
			if (cardCount > 3 && random.Random(0, 100) < 40)
			{
				card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.DRAW_MANY, 1);
				gun.spells.Add(card);
				cardCount--;
			}
			if (random.Random(0, 100) < 80)
			{
				card = GetRandomActionWithType(seed, x, y, level, ACTION_TYPE.MODIFIER, 2);
				gun.spells.Add(card);
				cardCount--;
			}

			for (int i = 0; i < cardCount; i++)
				gun.spells.Add(bulletCard);
		}
	}

	static void shuffleTable(ref string[] table, ref NoitaRandom random)
	{
		for (int i = table.Length - 1; i >= 1; i--)
		{
			int j = random.Random(0, i);
			string temp = table[i];
			table[i] = table[j];
			table[j] = temp;
		}
	}

	static void applyRandomVariable(ref Wand gun, string s, Dictionary<string, StatProb[]> dict, ref NoitaRandom random)
	{
		float cost = gun.cost;
		StatProb prob = getGunProbs(s, dict, ref random);
		float min, max;
		switch (s)
		{
			case "reload_time":
				min = Math.Clamp(60 - (cost * 5), 1, 240);
				max = 1024;
				gun.reload = (int)Math.Clamp(random.RandomDistribution(prob.min, prob.max, prob.mean, prob.sharpness), min, max);
				gun.cost -= (60 - gun.reload) / 5;
				//Console.WriteLine("reload");
				//Console.WriteLine(gun.cost);
				return;
			case "fire_rate_wait":
				min = Math.Clamp(16 - cost, -50, 50);
				max = 50;
				gun.delay = (int)Math.Clamp(random.RandomDistribution(prob.min, prob.max, prob.mean, prob.sharpness), min, max);
				gun.cost -= 16 - gun.delay;
				//Console.WriteLine("delay");
				//Console.WriteLine(gun.cost);
				return;
			case "spread_degrees":
				min = Math.Clamp(cost / -1.5f, -35, 35);
				max = 35;
				gun.spread = (int)Math.Clamp(random.RandomDistribution(prob.min, prob.max, prob.mean, prob.sharpness), min, max);
				gun.cost -= 16 - gun.spread;
				//Console.WriteLine("spread");
				//Console.WriteLine(gun.cost);
				return;
			case "speed_multiplier":
				gun.speed = random.RandomDistributionf(prob.min, prob.max, prob.mean, prob.sharpness);
				//Console.WriteLine("speed");
				//Console.WriteLine(gun.cost);
				return;
			case "deck_capacity":
				min = 1;
				max = Math.Clamp((cost / 5) + 6, 1, 20);
				if (gun.force_unshuffle)
				{
					max = (cost - 15) / 5;
					if (max > 6)
						max = 6 + (cost - 45) / 10;
				}

				max = Math.Clamp(max, 1, 20);

				gun.capacity = (Math.Clamp(random.RandomDistribution(prob.min, prob.max, prob.mean, prob.sharpness), min, max));
				gun.cost -= (gun.capacity - 6) * 5;
				//Console.WriteLine("capacity");
				//Console.WriteLine(gun.cost);
				return;
			case "shuffle_deck_when_empty":
				int rnd = random.Random(0, 1);
				if (gun.force_unshuffle)
					rnd = 1;
				if (rnd == 1 && cost >= (15 + gun.capacity * 5) && gun.capacity <= 9)
				{
					gun.shuffle = false;
					gun.cost -= 15 + gun.capacity * 5;
				}
				//Console.WriteLine("shuffle");
				//Console.WriteLine(gun.cost);
				return;
			case "actions_per_round":
				float[] actionCosts = new float[]
				{
					0,
					5+(gun.capacity*2),
					15+(gun.capacity*3.5f),
					35+(gun.capacity*5),
					45+(gun.capacity*gun.capacity)
				};

				min = 1;
				max = 1;
				for (int i = 0; i < actionCosts.Length; i++)
				{
					if (actionCosts[i] <= cost) max = actionCosts[i];
				}
				max = Math.Clamp(max, 1, gun.capacity);

				gun.multicast = (int)Math.Floor(Math.Clamp(random.RandomDistribution(prob.min, prob.max, prob.mean, prob.sharpness), min, max));
				float temp_cost = actionCosts[Math.Clamp(gun.multicast, 1, actionCosts.Length) - 1];
				gun.cost -= temp_cost;
				//Console.WriteLine("multicast");
				//Console.WriteLine(gun.cost);
				return;
			default:
				return;
		}
	}

	static StatProb getGunProbs(string s, Dictionary<string, StatProb[]> dict, ref NoitaRandom random)
	{
		StatProb[] probs = dict[s];
		if (probs.Length == 0) return new();
		float sum = 0;
		foreach (StatProb prob in probs) sum += prob.prob;
		float rnd = (float)random.Next() * sum;
		for(int i = 0; i < probs.Length; i++)
		{
			if(rnd < probs[i].prob) return probs[i];
			rnd -= probs[i].prob;
		}
		//unreachable
		return new();
	}

	static Spell GetRandomActionWithType(uint seed, double x, double y, int level, ACTION_TYPE type, int offset)
	{
		NoitaRandom random = new NoitaRandom((uint)(seed + offset));
		random.SetRandomSeed(x, y);
		double sum = 0;
		level = Math.Min(level, 10);
		Spell[] spellsOfType = spellsByType[type];
		// all_spells length is 393
		for (int i = 0; i < spellsOfType.Length; i++)
		{
			if(level < spellsOfType[i].spawn_probabilities.Length)
				sum += spellsOfType[i].spawn_probabilities[level];
		}

		double multiplier = random.Next();
		double accumulated = sum * multiplier;

		for (int i = 0; i < spellsOfType.Length; i++)
		{
			Spell spell2 = spellsOfType[i];

			double probability = 0;
			if(level < spell2.spawn_probabilities.Length) 
				probability = spell2.spawn_probabilities[level];
			if (probability > 0.0 && probability >= accumulated)
			{
				return spell2;
			}
			accumulated -= probability;
		}
		int rand = (int)(random.Next() * 393);
		for (int j = 0; j < 393; j++)
		{
			Spell spell = all_spells[(j + rand) % 393];
			if (spell.type == type && level < spell.spawn_probabilities.Length && spell.spawn_probabilities[level] > 0.0)
			{
				return spell;
			}
			j++;
		}
		return spell_empty;
	}
	public static void SortSpells()
	{
		spellsByType = new();
		Dictionary<ACTION_TYPE, List<Spell>> intermediate = new();
		foreach(ACTION_TYPE type in Enum.GetValues(typeof(ACTION_TYPE)))
			intermediate.Add(type, new());

		foreach(Spell spell in all_spells)
			intermediate[spell.type].Add(spell);

		foreach(KeyValuePair<ACTION_TYPE, List<Spell>> entry in intermediate)
			spellsByType.Add(entry.Key, entry.Value.ToArray());
	}

	struct StatProb
	{
		public float prob;
		public float min;
		public float max;
		public float mean;
		public float sharpness;
		public Action<Wand> extra;
		public StatProb(float _prob, float _min, float _max, float _mean, float _sharpness, Action<Wand> _extra = null)
		{
			prob = _prob;
			min = _min;
			max = _max;
			mean = _mean;
			sharpness = _sharpness;
			extra = _extra;
		}
	}

	static Dictionary<string, StatProb[]> statProbabilities = new()
	{
		{
			"deck_capacity",
			new StatProb[]
			{
				new(1, 3, 10, 6, 2),
				new(0.1f, 2, 7, 4, 4, g => { g.prob_unshuffle += 0.8f; }),
				new(0.05f, 1, 5, 3, 4, g => { g.prob_unshuffle += 0.8f; }),
				new(0.15f, 5, 11, 8, 2),
				new(0.12f, 2, 20, 8, 4),
				new(0.15f, 3, 12, 6, 6, g => { g.prob_unshuffle += 0.8f; }),
				new(1, 1, 20, 6, 0)
			}
		},
		{
			"reload_time",
			new StatProb[]
			{
				new(1, 5, 60, 30, 2),
				new(0.5f, 1, 100, 40, 2),
				new(0.02f, 1, 100, 40, 0),
				new(0.35f, 1, 240, 40, 0, g => { g.prob_unshuffle += 0.5f; })
			}
		},
		{
			"fire_rate_wait",
			new StatProb[]
			{
				new(1, 1, 30, 5, 2),
				new(0.1f, 1, 50, 15, 3),
				new(0.1f, -15, 15, 0, 3),
				new(0.45f, 0, 35, 12, 0)
			}
		},
		{
			"spread_degrees",
			new StatProb[]
			{
				new(1, -5, 10, 0, 3),
				new(0.1f, -35, 35, 0, 0)
			}
		},
		{
			"speed_multiplier",
			new StatProb[]
			{
				new(1, 0.8f, 1.2f, 1, 6),
				new(0.05f, 1, 2, 1.1f, 3),
				new(0.05f, 0.5f, 1, 0.9f, 3),
				new(1, 0.8f, 1.2f, 1, 0),
				new(0.001f, 1, 10, 5, 2)
			}
		},
		{
			"actions_per_round",
			new StatProb[]
			{
				new(1, 1, 3, 1, 3),
				new(0.2f, 2, 4, 2, 8),
				new(0.05f, 1, 5, 2, 2),
				new(1, 1, 5, 2, 0)
			}
		},
		{
			"shuffle_deck_when_empty",
			new StatProb[] { }
		}
	};
	static Dictionary<string, StatProb[]> statProbabilitiesBetter = new()
	{
		{
			"deck_capacity",
			new StatProb[]
			{
				new(1, 5, 13, 8, 2),
			}
		},
		{
			"reload_time",
			new StatProb[]
			{
				new(1, 5, 40, 20, 2),
			}
		},
		{
			"fire_rate_wait",
			new StatProb[]
			{
				new(1, 1, 35, 5, 2),
			}
		},
		{
			"spread_degrees",
			new StatProb[]
			{
				new(1, -1, 2, 0, 3),
			}
		},
		{
			"speed_multiplier",
			new StatProb[]
			{
				new(1, 0.8f, 1.2f, 1, 6),
			}
		},
		{
			"actions_per_round",
			new StatProb[]
			{
				new(1, 1, 3, 1, 3),
			}
		},
		{
			"shuffle_deck_when_empty",
			new StatProb[] { }
		}
	};

	public static Spell spell_empty = new Spell("NULL", ACTION_TYPE.PROJECTILE, new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

	public static Spell[] all_spells = new Spell[] {
		new Spell("BOMB", ACTION_TYPE.PROJECTILE, new double[] {1, 1, 1, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("LIGHT_BULLET", ACTION_TYPE.PROJECTILE, new double[] {2, 1, 0.5, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("LIGHT_BULLET_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {1, 0.5, 0.5, 0.5, 0, 0, 0, 0, 0, 0}),
		new Spell("LIGHT_BULLET_TRIGGER_2", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 0.5, 0, 1, 1, 0, 0, 0, 0.2}),
		new Spell("LIGHT_BULLET_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 0.5, 0.5, 0, 0, 0, 0, 0, 0}),
		new Spell("BULLET", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 1, 1, 1, 0, 0, 0, 0}),
		new Spell("BULLET_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0, 0}),
		new Spell("BULLET_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("HEAVY_BULLET", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 1, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("HEAVY_BULLET_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("HEAVY_BULLET_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("AIR_BULLET", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("SLOW_BULLET", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("SLOW_BULLET_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 0.5, 0.5, 0.5, 1, 0, 0, 0, 0}),
		new Spell("SLOW_BULLET_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 0.5, 0.5, 0.5, 1, 1, 0, 0, 0}),
		new Spell("BLACK_HOLE", ACTION_TYPE.PROJECTILE, new double[] {0.8, 0, 0.8, 0, 0.8, 0.8, 0, 0, 0, 0}),
		new Spell("BLACK_HOLE_DEATH_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 0, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("BLACK_HOLE_BIG", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.8, 0, 0.8, 0, 0.8, 0.8, 0, 0, 0, 0.5}),
		new Spell("BLACK_HOLE_GIGA", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("TENTACLE_PORTAL", ACTION_TYPE.PROJECTILE, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0, 0.2}),
		new Spell("SPITTER", ACTION_TYPE.PROJECTILE, new double[] {1, 1, 1, 0.5, 0, 0, 0, 0, 0, 0}),
		new Spell("SPITTER_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0.5, 0.5, 0.5, 1, 0, 0, 0, 0, 0, 0}),
		new Spell("SPITTER_TIER_2", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 1, 0.5, 0, 0, 0, 0}),
		new Spell("SPITTER_TIER_2_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 0.5, 0.5, 1, 0, 0, 0, 0}),
		new Spell("SPITTER_TIER_3", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0.8, 0.8, 1, 1, 0, 0, 0}),
		new Spell("SPITTER_TIER_3_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("BUBBLESHOT", ACTION_TYPE.PROJECTILE, new double[] {1, 1, 1, 0.5, 0, 0, 0, 0, 0, 0}),
		new Spell("BUBBLESHOT_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 0.5, 1, 0, 0, 0, 0, 0, 0}),
		new Spell("DISC_BULLET", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 1, 0, 1, 0, 0, 0, 0, 0}),
		new Spell("DISC_BULLET_BIG", ACTION_TYPE.PROJECTILE, new double[] {0.6, 0, 0.6, 0, 0.6, 0, 0, 0, 0, 0, 0.1}),
		new Spell("DISC_BULLET_BIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.1, 0.6, 0, 1, 0, 0, 0, 0, 0.1}),
		new Spell("BOUNCY_ORB", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 1, 0, 1, 0, 0, 0, 0, 0}),
		new Spell("BOUNCY_ORB_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0.5, 0, 0.5, 0, 0.5, 0, 0, 0, 0, 0}),
		new Spell("RUBBER_BALL", ACTION_TYPE.PROJECTILE, new double[] {1, 1, 0, 0, 0, 0, 1, 0, 0, 0}),
		new Spell("ARROW", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 0, 1, 1, 0, 0, 0, 0}),
		new Spell("POLLEN", ACTION_TYPE.PROJECTILE, new double[] {0.6, 1, 0, 1, 0.8, 0, 0, 0, 0, 0}),
		new Spell("LANCE", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 0, 0, 1, 1, 0, 0, 0}),
		new Spell("ROCKET", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 1, 0.5, 0.5, 0, 0, 0, 0}),
		new Spell("ROCKET_TIER_2", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("ROCKET_TIER_3", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.25, 0.5, 1, 1, 1, 0, 0, 0}),
		new Spell("GRENADE", ACTION_TYPE.PROJECTILE, new double[] {1, 1, 0.5, 0.25, 0.25, 0, 0, 0, 0, 0}),
		new Spell("GRENADE_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0.5, 0.5, 0.5, 0.5, 0.5, 1, 0, 0, 0, 0}),
		new Spell("GRENADE_TIER_2", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 1, 1, 1, 1, 0, 0, 0, 0}),
		new Spell("GRENADE_TIER_3", ACTION_TYPE.PROJECTILE, new double[] {0, 0.25, 0.5, 0.75, 1, 1, 0, 0, 0, 0}),
		new Spell("GRENADE_ANTI", ACTION_TYPE.PROJECTILE, new double[] {0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("GRENADE_LARGE", ACTION_TYPE.PROJECTILE, new double[] {0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("MINE", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 0, 1, 1, 0, 1, 0, 0, 0}),
		new Spell("MINE_DEATH_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 0, 0, 0, 1, 0, 0, 0}),
		new Spell("PIPE_BOMB", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("PIPE_BOMB_DEATH_TRIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 1, 1, 0, 0, 0, 0}),
		new Spell("EXPLODING_DEER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0.6, 0.6, 0.6, 0, 0, 0, 0}),
		new Spell("EXPLODING_DUCKS", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0.6, 0.8, 0.6, 0, 0, 0, 0}),
		new Spell("WORM_SHOT", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0.6, 0.8, 0.6, 0, 0, 0, 0}),
		new Spell("BOMB_DETONATOR", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 1, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("LASER", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 0, 1, 0, 0, 0, 0, 0}),
		new Spell("MEGALASER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0.6, 0.6, 0.6, 0.6, 0, 0, 0, 0.1}),
		new Spell("LIGHTNING", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 0, 0, 1, 1, 0, 0, 0}),
		new Spell("BALL_LIGHTNING", ACTION_TYPE.PROJECTILE, new double[] {0, 0.2, 0.2, 0, 1, 1, 0, 0, 0, 0}),
		new Spell("LASER_EMITTER", ACTION_TYPE.PROJECTILE, new double[] {0, 0.2, 1, 1, 0.5, 0, 0, 0, 0, 0}),
		new Spell("LASER_EMITTER_FOUR", ACTION_TYPE.PROJECTILE, new double[] {0, 0.2, 1, 0.2, 0.5, 1, 0, 0, 0, 0}),
		new Spell("LASER_EMITTER_CUTTER", ACTION_TYPE.PROJECTILE, new double[] {0.2, 0.3, 1, 0.5, 0.1, 0, 0, 0, 0, 0}),
		new Spell("DIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 0.5, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("POWERDIGGER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("CHAINSAW", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 1, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("LUMINOUS_DRILL", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 1, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("LASER_LUMINOUS_DRILL", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 1, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("TENTACLE", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("TENTACLE_TIMER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("HEAL_BULLET", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("SPIRAL_SHOT", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0, 1, 1, 1, 0, 0, 0}),
		new Spell("MAGIC_SHIELD", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 0, 0.5, 1, 1, 0, 0, 0}),
		new Spell("BIG_MAGIC_SHIELD", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.2, 0, 0.2, 0.5, 0.5, 0, 0, 0, 0.1}),
		new Spell("CHAIN_BOLT", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 0, 0, 1, 1, 1, 0, 0, 0}),
		new Spell("FIREBALL", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 0, 1, 1, 0, 1, 0, 0, 0}),
		new Spell("METEOR", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0, 0.6, 0.6, 0.6, 0, 0, 0, 0.5}),
		new Spell("FLAMETHROWER", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 0, 0, 1, 0, 0, 0}),
		new Spell("ICEBALL", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 1, 0, 1, 0, 0, 0}),
		new Spell("SLIMEBALL", ACTION_TYPE.PROJECTILE, new double[] {1, 0, 0, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("DARKFLAME", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 1, 0, 1, 1, 0, 0, 0}),
		new Spell("MISSILE", ACTION_TYPE.PROJECTILE, new double[] {0, 0.5, 0.5, 1, 0, 1, 0, 0, 0, 0}),
		new Spell("FUNKY_SPELL", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0, 0, 0, 0.1, 0, 0, 0, 0.1}),
		new Spell("PEBBLE", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 0, 1, 0, 1, 0, 0, 0}),
		new Spell("DYNAMITE", ACTION_TYPE.PROJECTILE, new double[] {1, 1, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("GLITTER_BOMB", ACTION_TYPE.PROJECTILE, new double[] {0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0, 0, 0}),
		new Spell("BUCKSHOT", ACTION_TYPE.PROJECTILE, new double[] {1, 1, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("FREEZING_GAZE", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("GLOWING_BOLT", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0.1}),
		new Spell("SPORE_POD", ACTION_TYPE.PROJECTILE, new double[] {0, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0, 0}),
		new Spell("GLUE_SHOT", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.6, 0.2, 0.2, 0.6, 0, 0, 0, 0}),
		new Spell("BOMB_HOLY", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.2, 0.2, 0.2, 0.2, 0.2, 0, 0, 0, 0.5}),
		new Spell("BOMB_HOLY_GIGA", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("PROPANE_TANK", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("BOMB_CART", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("CURSED_ORB", ACTION_TYPE.PROJECTILE, new double[] {0, 0.3, 0.2, 0.1, 0, 0, 0, 0, 0, 0}),
		new Spell("EXPANDING_ORB", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.5, 0.5, 1, 1, 1, 0, 0, 0}),
		new Spell("CRUMBLING_EARTH", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("SUMMON_ROCK", ACTION_TYPE.PROJECTILE, new double[] {0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0}),
		new Spell("SUMMON_EGG", ACTION_TYPE.PROJECTILE, new double[] {0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0}),
		new Spell("SUMMON_HOLLOW_EGG", ACTION_TYPE.PROJECTILE, new double[] {0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0}),
		new Spell("TNTBOX", ACTION_TYPE.PROJECTILE, new double[] {0, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0, 0}),
		new Spell("TNTBOX_BIG", ACTION_TYPE.PROJECTILE, new double[] {0, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0, 0}),
		new Spell("SWARM_FLY", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.2, 0, 0.2, 0.5, 0.5, 0, 0, 0}),
		new Spell("SWARM_FIREBUG", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.2, 0, 0.2, 0.5, 0.5, 0, 0, 0}),
		new Spell("SWARM_WASP", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.2, 0, 0.2, 0.5, 0.5, 0, 0, 0}),
		new Spell("FRIEND_FLY", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0, 0, 0.2, 0.5, 0.5, 0, 0, 0}),
		new Spell("ACIDSHOT", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("THUNDERBALL", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0.2}),
		new Spell("FIREBOMB", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 1, 0, 0, 0, 0, 0, 0}),
		new Spell("SOILBALL", ACTION_TYPE.MATERIAL, new double[] {0, 1, 1, 1, 0, 1, 0, 0, 0, 0}),
		new Spell("DEATH_CROSS", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("DEATH_CROSS_BIG", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0.2}),
		new Spell("INFESTATION", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.1, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("WALL_HORIZONTAL", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.4, 0.4, 0.4, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("WALL_VERTICAL", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.4, 0.4, 0.4, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("WALL_SQUARE", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.4, 0.4, 0.4, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("TEMPORARY_WALL", ACTION_TYPE.UTILITY, new double[] {0.1, 0.1, 0.3, 0, 0.4, 0.2, 0.1, 0, 0, 0}),
		new Spell("TEMPORARY_PLATFORM", ACTION_TYPE.UTILITY, new double[] {0.1, 0.1, 0.3, 0, 0.4, 0.2, 0.1, 0, 0, 0}),
		new Spell("PURPLE_EXPLOSION_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {1, 1, 1, 0, 1, 1, 1, 0, 0, 0}),
		new Spell("DELAYED_SPELL", ACTION_TYPE.STATIC_PROJECTILE, new double[] {1, 1, 1, 0, 1, 1, 1, 0, 0, 0}),
		new Spell("LONG_DISTANCE_CAST", ACTION_TYPE.UTILITY, new double[] {0.6, 0.6, 0.6, 0, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("TELEPORT_CAST", ACTION_TYPE.UTILITY, new double[] {0.6, 0.6, 0.6, 0, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("SUPER_TELEPORT_CAST", ACTION_TYPE.UTILITY, new double[] {0.2, 0.2, 0.2, 0, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("MIST_RADIOACTIVE", ACTION_TYPE.PROJECTILE, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("MIST_ALCOHOL", ACTION_TYPE.PROJECTILE, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("MIST_SLIME", ACTION_TYPE.PROJECTILE, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("MIST_BLOOD", ACTION_TYPE.PROJECTILE, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("CIRCLE_FIRE", ACTION_TYPE.MATERIAL, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("CIRCLE_ACID", ACTION_TYPE.MATERIAL, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("CIRCLE_OIL", ACTION_TYPE.MATERIAL, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("CIRCLE_WATER", ACTION_TYPE.MATERIAL, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("MATERIAL_WATER", ACTION_TYPE.MATERIAL, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("MATERIAL_OIL", ACTION_TYPE.MATERIAL, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("MATERIAL_BLOOD", ACTION_TYPE.MATERIAL, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("MATERIAL_ACID", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("MATERIAL_CEMENT", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("TELEPORT_PROJECTILE", ACTION_TYPE.PROJECTILE, new double[] {0.6, 0.6, 0.6, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("TELEPORT_PROJECTILE_SHORT", ACTION_TYPE.PROJECTILE, new double[] {0.6, 0.6, 0.6, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("TELEPORT_PROJECTILE_STATIC", ACTION_TYPE.PROJECTILE, new double[] {0.6, 0.6, 0.6, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("SWAPPER_PROJECTILE", ACTION_TYPE.PROJECTILE, new double[] {0.05, 0.05, 0.05, 0, 0.05, 0.05, 0.05, 0, 0, 0}),
		new Spell("TELEPORT_PROJECTILE_CLOSER", ACTION_TYPE.PROJECTILE, new double[] {0.6, 0.6, 0.6, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("NUKE", ACTION_TYPE.PROJECTILE, new double[] {0, 0.3, 0, 0, 0, 1, 1, 0, 0, 0, 0.2}),
		new Spell("NUKE_GIGA", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("FIREWORK", ACTION_TYPE.PROJECTILE, new double[] {0, 1, 1, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("SUMMON_WANDGHOST", ACTION_TYPE.UTILITY, new double[] {0, 0, 0.08, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0.1}),
		new Spell("TOUCH_GOLD", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0.5}),
		new Spell("TOUCH_WATER", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0.1}),
		new Spell("TOUCH_OIL", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0.1}),
		new Spell("TOUCH_ALCOHOL", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0.1}),
		new Spell("TOUCH_BLOOD", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0.5}),
		new Spell("TOUCH_SMOKE", ACTION_TYPE.MATERIAL, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0.1}),
		new Spell("DESTRUCTION", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("BURST_2", ACTION_TYPE.DRAW_MANY, new double[] {0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0}),
		new Spell("BURST_3", ACTION_TYPE.DRAW_MANY, new double[] {0, 0.7, 0.7, 0.7, 0.7, 0.7, 0.7, 0, 0, 0}),
		new Spell("BURST_4", ACTION_TYPE.DRAW_MANY, new double[] {0, 0, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("BURST_8", ACTION_TYPE.DRAW_MANY, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 0.5}),
		new Spell("BURST_X", ACTION_TYPE.DRAW_MANY, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 0.5}),
		new Spell("SCATTER_2", ACTION_TYPE.DRAW_MANY, new double[] {0.8, 0.8, 0.8, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("SCATTER_3", ACTION_TYPE.DRAW_MANY, new double[] {0.7, 0.7, 0.7, 0.8, 0, 0, 0, 0, 0, 0}),
		new Spell("SCATTER_4", ACTION_TYPE.DRAW_MANY, new double[] {0, 0.6, 0.6, 0.7, 0.8, 0.8, 0.8, 0, 0, 0}),
		new Spell("I_SHAPE", ACTION_TYPE.DRAW_MANY, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("Y_SHAPE", ACTION_TYPE.DRAW_MANY, new double[] {0.8, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("T_SHAPE", ACTION_TYPE.DRAW_MANY, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("W_SHAPE", ACTION_TYPE.DRAW_MANY, new double[] {0, 0, 0.4, 0.3, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("CIRCLE_SHAPE", ACTION_TYPE.DRAW_MANY, new double[] {0, 0.1, 0.2, 0.3, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("PENTAGRAM_SHAPE", ACTION_TYPE.DRAW_MANY, new double[] {0, 0.4, 0.4, 0.3, 0.2, 0.1, 0, 0, 0, 0}),
		new Spell("SPREAD_REDUCE", ACTION_TYPE.MODIFIER, new double[] {0, 0.8, 0.8, 0.8, 0.8, 0.8, 0.8, 0, 0, 0}),
		new Spell("HEAVY_SPREAD", ACTION_TYPE.MODIFIER, new double[] {0.8, 0.8, 0.8, 0, 0.8, 0.8, 0.8, 0, 0, 0}),
		new Spell("RECHARGE", ACTION_TYPE.MODIFIER, new double[] {0, 1, 1, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("LIFETIME", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.5, 0.5, 0.5, 0.5, 0, 0, 0, 0.1}),
		new Spell("LIFETIME_DOWN", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.5, 0.5, 0.5, 0.5, 0, 0, 0, 0.1}),
		new Spell("NOLLA", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0, 0.2, 0.5, 0.5, 0, 0, 0, 1}),
		new Spell("SLOW_BUT_STEADY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.1, 0.2, 0.3, 0.4, 0, 0, 0, 0.4}),
		new Spell("EXPLOSION_REMOVE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0, 0.6, 0.7, 0.2, 0, 0, 0}),
		new Spell("EXPLOSION_TINY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0, 0.6, 0.7, 0.2, 0, 0, 0}),
		new Spell("LASER_EMITTER_WIDER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("MANA_REDUCE", ACTION_TYPE.MODIFIER, new double[] {0, 1, 1, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("BLOOD_MAGIC", ACTION_TYPE.UTILITY, new double[] {0, 0, 0, 0, 0, 0.1, 0.7, 0, 0, 0, 0.5}),
		new Spell("MONEY_MAGIC", ACTION_TYPE.UTILITY, new double[] {0, 0, 0, 0.2, 0, 0.8, 0.1, 0, 0, 0, 0.5}),
		new Spell("BLOOD_TO_POWER", ACTION_TYPE.UTILITY, new double[] {0, 0, 0.2, 0, 0, 0.8, 0.1, 0, 0, 0, 0.5}),
		new Spell("DUPLICATE", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("QUANTUM_SPLIT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 1, 0, 0, 0}),
		new Spell("GRAVITY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("GRAVITY_ANTI", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("SINEWAVE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0}),
		new Spell("CHAOTIC_ARC", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0, 0}),
		new Spell("PINGPONG_PATH", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0, 0}),
		new Spell("AVOIDING_ARC", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0}),
		new Spell("FLOATING_ARC", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0, 0}),
		new Spell("FLY_DOWNWARDS", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0, 0}),
		new Spell("FLY_UPWARDS", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0}),
		new Spell("HORIZONTAL_ARC", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0, 0}),
		new Spell("LINE_ARC", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0, 0.4, 0, 0.4, 0, 0, 0, 0}),
		new Spell("ORBIT_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0.3, 0.4, 0.1, 0, 0, 0, 0, 0}),
		new Spell("SPIRALING_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0.3, 0.4, 0.1, 0, 0, 0, 0, 0}),
		new Spell("PHASING_ARC", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.3, 0.6, 0.1, 0, 0, 0, 0}),
		new Spell("BOUNCE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 1, 1, 0.4, 0.2, 0.2, 0, 0, 0}),
		new Spell("REMOVE_BOUNCE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.2, 1, 1, 1, 0, 0, 0}),
		new Spell("HOMING", ACTION_TYPE.MODIFIER, new double[] {0, 0.1, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("HOMING_SHORT", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0.8, 1, 0.4, 0.1, 0.1, 0, 0, 0}),
		new Spell("HOMING_ROTATE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("HOMING_SHOOTER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.2, 0.2, 0.2, 0.2, 0, 0, 0}),
		new Spell("AUTOAIM", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("HOMING_ACCELERATING", ACTION_TYPE.MODIFIER, new double[] {0, 0.1, 0.3, 0.3, 0.5, 0, 0, 0, 0, 0}),
		new Spell("HOMING_CURSOR", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.7, 0.7, 0.4, 0.4, 1, 0, 0, 0}),
		new Spell("HOMING_AREA", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.4, 0.6, 0.7, 0.4, 0, 0, 0}),
		new Spell("PIERCING_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("CLIPPING_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("DAMAGE", ACTION_TYPE.MODIFIER, new double[] {0, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0, 0}),
		new Spell("DAMAGE_RANDOM", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.6, 0.6, 0.6, 0, 0, 0, 0}),
		new Spell("BLOODLUST", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.3, 0.6, 0.6, 0.3, 0, 0, 0}),
		new Spell("DAMAGE_FOREVER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.4, 0.6, 0.4, 0.2, 0, 0, 0}),
		new Spell("CRITICAL_HIT", ACTION_TYPE.MODIFIER, new double[] {0, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0, 0}),
		new Spell("AREA_DAMAGE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0}),
		new Spell("SPELLS_TO_POWER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.5, 0.5, 0.5, 0.5, 0.5, 0, 0, 0, 0.1}),
		new Spell("ESSENCE_TO_POWER", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0.5, 0.5, 0, 0, 0, 0, 0, 0, 0.1}),
		new Spell("HEAVY_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("LIGHT_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0.4, 0.4, 0, 0, 0, 0, 0}),
		new Spell("KNOCKBACK", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.6, 0, 0.6, 0, 0, 0, 0}),
		new Spell("RECOIL", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.6, 0, 0.6, 0, 0, 0, 0, 0}),
		new Spell("RECOIL_DAMPER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.6, 0, 0, 0.6, 0, 0, 0}),
		new Spell("SPEED", ACTION_TYPE.MODIFIER, new double[] {0, 1, 0.5, 0.5, 0, 0, 0, 0, 0, 0}),
		new Spell("ACCELERATING_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.5, 0.5, 1, 0, 0, 0, 0, 0}),
		new Spell("DECELERATING_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.5, 0, 0, 0, 0, 0}),
		new Spell("EXPLOSIVE_PROJECTILE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("WATER_TO_POISON", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("BLOOD_TO_ACID", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("LAVA_TO_BLOOD", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("LIQUID_TO_EXPLOSION", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("TOXIC_TO_ACID", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("STATIC_TO_SAND", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("TRANSMUTATION", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0, 0.2}),
		new Spell("RANDOM_EXPLOSION", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.3, 0, 0.6, 1, 0, 0, 0}),
		new Spell("NECROMANCY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.6, 0.6, 0.6, 0.6, 0, 0, 0, 0}),
		new Spell("LIGHT", ACTION_TYPE.MODIFIER, new double[] {1, 0.8, 0.6, 0.4, 0.2, 0, 0, 0, 0, 0}),
		new Spell("EXPLOSION", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.5, 0, 1, 0, 1, 1, 0, 0, 0, 0}),
		new Spell("EXPLOSION_LIGHT", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.5, 1, 0, 1, 1, 0, 0, 0}),
		new Spell("FIRE_BLAST", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.5, 0.5, 0, 0.6, 0, 0.6, 0, 0, 0, 0}),
		new Spell("POISON_BLAST", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.5, 0.6, 0, 0.6, 0, 0.5, 0, 0, 0}),
		new Spell("ALCOHOL_BLAST", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.5, 0.6, 0, 0.6, 0, 0.5, 0, 0, 0}),
		new Spell("THUNDER_BLAST", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.5, 0, 0.6, 0, 0.6, 0.5, 0, 0, 0, 0.1}),
		new Spell("BERSERK_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.3, 0.6, 0.3, 0, 0, 0, 0, 0}),
		new Spell("POLYMORPH_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.3, 0.3, 0.3, 0.8, 0.8, 0.3, 0.3, 0, 0, 0}),
		new Spell("CHAOS_POLYMORPH_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.3, 0.3, 0.5, 0.6, 0.3, 0.3, 0, 0, 0}),
		new Spell("ELECTROCUTION_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.3, 0, 0.6, 0, 0.8, 0.3, 0, 0, 0}),
		new Spell("FREEZE_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.3, 0, 0.6, 0, 0.7, 0.3, 0, 0, 0, 0}),
		new Spell("REGENERATION_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.3, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("TELEPORTATION_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.3, 0.6, 0.3, 0.3, 0.6, 0.3, 0, 0, 0, 0}),
		new Spell("LEVITATION_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0.3, 0.6, 0.6, 0.3, 0, 0, 0, 0, 0}),
		new Spell("SHIELD_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("PROJECTILE_TRANSMUTATION_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("PROJECTILE_THUNDER_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0, 0.3, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("PROJECTILE_GRAVITY_FIELD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.3, 0, 0, 0.3, 0.3, 0, 0, 0}),
		new Spell("VACUUM_POWDER", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.3, 1, 0, 0.3, 0.3, 0, 0, 0}),
		new Spell("VACUUM_LIQUID", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.3, 1, 0, 0.3, 0.3, 0, 0, 0}),
		new Spell("VACUUM_ENTITIES", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0.3, 1, 0, 0.3, 0.3, 0, 0, 0}),
		new Spell("SEA_LAVA", ACTION_TYPE.MATERIAL, new double[] {0.2, 0, 0, 0, 0.2, 0.2, 0.2, 0, 0, 0}),
		new Spell("SEA_ALCOHOL", ACTION_TYPE.MATERIAL, new double[] {0.3, 0, 0, 0, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("SEA_OIL", ACTION_TYPE.MATERIAL, new double[] {0.3, 0, 0, 0, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("SEA_WATER", ACTION_TYPE.MATERIAL, new double[] {0.4, 0, 0, 0, 0.4, 0.4, 0.4, 0, 0, 0}),
		new Spell("SEA_ACID", ACTION_TYPE.MATERIAL, new double[] {0.2, 0, 0, 0, 0.2, 0.2, 0.2, 0, 0, 0}),
		new Spell("SEA_ACID_GAS", ACTION_TYPE.MATERIAL, new double[] {0.3, 0, 0, 0, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("CLOUD_WATER", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("CLOUD_OIL", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("CLOUD_BLOOD", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0, 0}),
		new Spell("CLOUD_ACID", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("CLOUD_THUNDER", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0, 0}),
		new Spell("ELECTRIC_CHARGE", ACTION_TYPE.MODIFIER, new double[] {0, 1, 1, 0, 1, 1, 0, 0, 0, 0}),
		new Spell("MATTER_EATER", ACTION_TYPE.MODIFIER, new double[] {0, 0.1, 1, 0, 0.1, 0.1, 0, 0, 0, 0, 0.2}),
		new Spell("FREEZE", ACTION_TYPE.MODIFIER, new double[] {0, 1, 0, 1, 1, 1, 0, 0, 0, 0}),
		new Spell("HITFX_BURNING_CRITICAL_HIT", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("HITFX_CRITICAL_WATER", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("HITFX_CRITICAL_OIL", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("HITFX_CRITICAL_BLOOD", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("HITFX_TOXIC_CHARM", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("HITFX_EXPLOSION_SLIME", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("HITFX_EXPLOSION_SLIME_GIGA", ACTION_TYPE.MODIFIER, new double[] {0, 0.1, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0}),
		new Spell("HITFX_EXPLOSION_ALCOHOL", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0.2, 0.2, 0.2, 0, 0, 0, 0}),
		new Spell("HITFX_EXPLOSION_ALCOHOL_GIGA", ACTION_TYPE.MODIFIER, new double[] {0, 0.1, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0}),
		new Spell("HITFX_PETRIFY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.2, 0, 0.2, 0.2, 0, 0, 0}),
		new Spell("ROCKET_DOWNWARDS", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 1, 1, 1, 0, 0, 0, 0, 0}),
		new Spell("ROCKET_OCTAGON", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.5, 0.5, 0.5, 0, 0, 0, 0, 0}),
		new Spell("FIZZLE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0}),
		new Spell("BOUNCE_EXPLOSION", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.6, 0.8, 0.8, 0, 0, 0, 0}),
		new Spell("BOUNCE_SPARK", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0.6, 0.6, 0.6, 0, 0, 0, 0, 0}),
		new Spell("BOUNCE_LASER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.4, 0.8, 0.4, 0, 0, 0, 0}),
		new Spell("BOUNCE_LASER_EMITTER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.4, 0.8, 0.4, 0, 0, 0, 0}),
		new Spell("BOUNCE_LARPA", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0, 0.4, 0.6, 0.4, 0, 0, 0}),
		new Spell("FIREBALL_RAY", ACTION_TYPE.MODIFIER, new double[] {0, 0.6, 0.6, 0, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("LIGHTNING_RAY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("TENTACLE_RAY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("LASER_EMITTER_RAY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("FIREBALL_RAY_LINE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.6, 0.4, 0.4, 0.4, 1, 0, 0, 0}),
		new Spell("FIREBALL_RAY_ENEMY", ACTION_TYPE.MODIFIER, new double[] {0, 0.6, 0.6, 0, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("LIGHTNING_RAY_ENEMY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("TENTACLE_RAY_ENEMY", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("GRAVITY_FIELD_ENEMY", ACTION_TYPE.MODIFIER, new double[] {0, 0.6, 0.6, 0, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("CURSE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.6, 0.8, 0, 0.4, 0, 0, 0, 0}),
		new Spell("CURSE_WITHER_PROJECTILE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.2, 0.4, 0.9, 0.9, 0, 0, 0}),
		new Spell("CURSE_WITHER_EXPLOSION", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.2, 0.4, 0.9, 0.9, 0, 0, 0, 0}),
		new Spell("CURSE_WITHER_MELEE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.2, 0.4, 0.9, 0.9, 0, 0, 0}),
		new Spell("CURSE_WITHER_ELECTRICITY", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0, 0, 0.4, 0.9, 0.9, 0, 0, 0}),
		new Spell("ORBIT_DISCS", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0.8, 0, 0.4, 0.2, 0, 0, 0, 0}),
		new Spell("ORBIT_FIREBALLS", ACTION_TYPE.MODIFIER, new double[] {0.5, 0.2, 0.8, 0, 0.4, 0.2, 0, 0, 0, 0}),
		new Spell("ORBIT_NUKES", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0, 0.1, 0.1, 0.2, 0, 0, 0, 1}),
		new Spell("ORBIT_LASERS", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0.8, 0, 0.4, 0.2, 0, 0, 0, 0, 0.2}),
		new Spell("ORBIT_LARPA", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.2, 0.2, 0, 0.8, 0, 0, 0, 0.1}),
		new Spell("CHAIN_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0, 0.6, 0.8, 0, 0, 0, 0}),
		new Spell("ARC_ELECTRIC", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.4, 0.4, 0.4, 0.4, 0.8, 0, 0, 0}),
		new Spell("ARC_FIRE", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("ARC_GUNPOWDER", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("ARC_POISON", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("CRUMBLING_EARTH_PROJECTILE", ACTION_TYPE.MODIFIER, new double[] {0, 0.4, 0.4, 0.4, 0.4, 0.4, 0, 0, 0, 0}),
		new Spell("X_RAY", ACTION_TYPE.UTILITY, new double[] {0.8, 1, 1, 0.8, 0.6, 0.4, 0.2, 0, 0, 0}),
		new Spell("UNSTABLE_GUNPOWDER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("ACID_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0, 0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0, 0}),
		new Spell("POISON_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("OIL_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("WATER_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0, 0.3, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("GUNPOWDER_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("FIRE_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0, 0, 0}),
		new Spell("BURN_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0.3, 0.3, 0.3, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("TORCH", ACTION_TYPE.PASSIVE, new double[] {1, 1, 1, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("TORCH_ELECTRIC", ACTION_TYPE.PASSIVE, new double[] {1, 1, 1, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("ENERGY_SHIELD", ACTION_TYPE.PASSIVE, new double[] {0, 0.05, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0}),
		new Spell("ENERGY_SHIELD_SECTOR", ACTION_TYPE.PASSIVE, new double[] {0.05, 0.6, 0.6, 0.6, 0.6, 0.6, 0, 0, 0, 0}),
		new Spell("ENERGY_SHIELD_SHOT", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.3, 0.3, 0.3, 0.3, 0.3, 0, 0, 0}),
		new Spell("TINY_GHOST", ACTION_TYPE.PASSIVE, new double[] {0, 0.1, 0.5, 1, 1, 1, 1, 0, 0, 0}),
		new Spell("OCARINA_A", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("OCARINA_B", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("OCARINA_C", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("OCARINA_D", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("OCARINA_E", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("OCARINA_F", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("OCARINA_GSHARP", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("OCARINA_A2", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("KANTELE_A", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("KANTELE_D", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("KANTELE_DIS", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("KANTELE_E", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("KANTELE_G", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("RANDOM_SPELL", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0.2, 0.3, 0.1, 0.1, 0, 0, 0, 0.5}),
		new Spell("RANDOM_PROJECTILE", ACTION_TYPE.PROJECTILE, new double[] {0, 0, 0.2, 0, 0.4, 0.1, 0.1, 0, 0, 0, 0.5}),
		new Spell("RANDOM_MODIFIER", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0, 0.3, 0.1, 0.1, 0, 0, 0, 0.5}),
		new Spell("RANDOM_STATIC_PROJECTILE", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0, 0.2, 0, 0.1, 0.1, 0, 0, 0, 0.5}),
		new Spell("DRAW_RANDOM", ACTION_TYPE.OTHER, new double[] {0, 0, 0.3, 0.2, 0.2, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("DRAW_RANDOM_X3", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0.1, 0.3, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("DRAW_3_RANDOM", ACTION_TYPE.OTHER, new double[] {0, 0, 0.1, 0.2, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("ALL_NUKES", ACTION_TYPE.UTILITY, new double[] {0, 0, 0, 0, 0, 0, 0.1, 0, 0, 0, 1}),
		new Spell("ALL_DISCS", ACTION_TYPE.UTILITY, new double[] {0.1, 0, 0, 0, 0, 0, 0.05, 0, 0, 0, 1}),
		new Spell("ALL_ROCKETS", ACTION_TYPE.UTILITY, new double[] {0, 0.1, 0, 0, 0, 0, 0.05, 0, 0, 0, 1}),
		new Spell("ALL_DEATHCROSSES", ACTION_TYPE.UTILITY, new double[] {0, 0, 0.1, 0, 0, 0, 0.05, 0, 0, 0, 1}),
		new Spell("ALL_BLACKHOLES", ACTION_TYPE.UTILITY, new double[] {0, 0, 0, 0.1, 0, 0, 0.05, 0, 0, 0, 1}),
		new Spell("ALL_ACID", ACTION_TYPE.UTILITY, new double[] {0, 0, 0, 0, 0.1, 0, 0.05, 0, 0, 0, 1}),
		new Spell("ALL_SPELLS", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("SUMMON_PORTAL", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}),
		new Spell("ADD_TRIGGER", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0.3, 0.6, 0.6, 0, 0, 0, 0, 1}),
		new Spell("ADD_TIMER", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0.3, 0.6, 0.6, 0, 0, 0, 0, 1}),
		new Spell("ADD_DEATH_TRIGGER", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0.3, 0.6, 0.6, 0, 0, 0, 0, 1}),
		new Spell("LARPA_CHAOS", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.2, 0.3, 0.4, 0, 0, 0, 0, 0.2}),
		new Spell("LARPA_DOWNWARDS", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.3, 0.2, 0.2, 0, 0, 0, 0, 0.2}),
		new Spell("LARPA_UPWARDS", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.2, 0.4, 0, 0, 0, 0, 0.2}),
		new Spell("LARPA_CHAOS_2", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0.1, 0, 0.4, 0, 0, 0, 0, 0.1}),
		new Spell("LARPA_DEATH", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.3, 0.2, 0, 0, 0, 0, 0.2}),
		new Spell("ALPHA", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("GAMMA", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("TAU", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("OMEGA", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("MU", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("PHI", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("SIGMA", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 0, 1}),
		new Spell("ZETA", ACTION_TYPE.OTHER, new double[] {0, 0.2, 0.8, 0.6, 0, 0, 0, 0, 0, 0, 0.1}),
		new Spell("DIVIDE_2", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0.2, 0, 0.3, 0.2, 0, 0, 0, 1}),
		new Spell("DIVIDE_3", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0.1, 0.1, 0.2, 0, 0, 0, 1}),
		new Spell("DIVIDE_4", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0.1, 0.1, 0, 0, 0, 1}),
		new Spell("DIVIDE_10", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("METEOR_RAIN", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0, 0, 0, 0, 0.1, 0, 0, 0, 1}),
		new Spell("WORM_RAIN", ACTION_TYPE.STATIC_PROJECTILE, new double[] {0, 0, 0, 0, 0, 0, 0.1, 0, 0, 0, 1}),
		new Spell("RESET", ACTION_TYPE.UTILITY, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("IF_ENEMY", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("IF_PROJECTILE", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("IF_HP", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("IF_HALF", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("IF_END", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("IF_ELSE", ACTION_TYPE.OTHER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}),
		new Spell("COLOUR_RED", ACTION_TYPE.MODIFIER, new double[] {0, 0.2, 0.2, 0.2, 0.2, 0.2, 0.2, 0, 0, 0}),
		new Spell("COLOUR_ORANGE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0, 0}),
		new Spell("COLOUR_GREEN", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0, 0}),
		new Spell("COLOUR_YELLOW", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0, 0}),
		new Spell("COLOUR_PURPLE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0, 0}),
		new Spell("COLOUR_BLUE", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0, 0}),
		new Spell("COLOUR_RAINBOW", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0, 0}),
		new Spell("COLOUR_INVIS", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0.1, 0.1, 0.1, 0, 0, 0, 0, 0}),
		new Spell("RAINBOW_TRAIL", ACTION_TYPE.MODIFIER, new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0})
	};
	static Dictionary<ACTION_TYPE, Spell[]> spellsByType;
}