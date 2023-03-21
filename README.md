# pudy248's Chest Loot Searcher
##OUT OF DATE! Use the rewrite <a href="https://github.com/pudy248/NoitaSeedSearcherCUDA/">here</a>

DISCLAIMER: There are a few bugs in the Wang tiler worldgen, which makes this program only correct 50-90% of the time, depending on the biome. If a seed doesn't have an item at the reported position, try another seed. Sometimes it can take a few attempts.

## Installation
Simply download and extract the latest release and open a command prompt in the folder. Make sure CUDA and .NET are installed on your system. Then, in a command prompt, type in `GCFinder` (or `./GCFinder` on Linux) followed by your desired options.

For advanced users, you can also compile this program from scratch if you want to tinker with things. Visual Studio is recommended for Windows users, but on Linux you can use `dotnet build` for the .NET portion and `nvcc -o WangTilerCUDA.dll -Xcompiler -fPIC --shared wang.cu` for the CUDA portion. You'll need the .NET 6.0 SDK and the CUDA toolkit.

## Usage
This program won't do very much if you just run the EXE. All options are available in the form of command-line arguments. Run `GCFinder --help` in a command promt for a short summary of each one. The option you'll use most is `-l`, as that is the loot filter for chests. A list of valid items can be found in matchlist.txt, and advanced syntax is documented in the option documentation below. `-c` is helpful for continuing long searches, as it copies every argument from the last search performed but continues from the last seed searched.

## FAQ
Q: I got an error related to CUDA memory allocation or invalid addresses!

A: You don't have enough VRAM for the requested query. The easiest way to fix this is by decreasing batch size with -z. Other options that increase VRAM usage are max items per chest and chests per biome, try and keep these as low as you can without getting overflow warnings. More parallel worlds also use more VRAM.

Q: I got warnings related to chest density!

A: You haven't allocated enough space for chests in each biome, so extra chests are being ignored. Increase --max-chests-per-biome until the warnings disappear.

Q: The program said there was a chest where there wasn't one!

A: Read the disclaimer at the top of this readme. The program is occasionally wrong, but it's right often enough to be useful. I will try and solve the accuracy issue as soon as I find out what's causing it.

Q: How quickly does this run?

A: It depends on your hardware. Most graphics cards should be able to check at least 500 seeds per second, but better ones can easily reach 2-3000 seeds a second. On my machine, with an RTX 2060, I can go through roughly 30 million seeds an hour.

Q: There aren't any results for my search!

A: Make sure there aren't any typos in your search list! This is the most common cause of empty searches. Also, make sure you aren't searching for something too rare! Having multiple items in a chest is rare in the first place, and searching for 3 or more specific items (ex. 3 paha silmas) in a normal chest will likely give you no results, as these chests are often less than 1 in 100 million! Also, double check that you aren't searching for greed-only items without `--greed`.

## Search string syntax
The filter string functionality is relatively versatile. Here are all of its features, in order of complexity.
- Search for chests containing at least a single specified item by entering its name. All searches except those with '-' can include extra items besides the searched ones. Ex. `-l "kiuaskivi"` returns all chests in a seed with at least one kiuaskivi.

- Search for multiple items (AND) in a single chest by entering their names separated by spaces. Ex. `-l "kiuaskivi kiuaskivi"` returns all chests in a seed with two kiuaskivis in the same chest.

- Aggregate searches between all chests in a seed by adding the option -a. Ex. `-l "ukkoskivi ukkoskivi" -a` will return all seeds where two ukkoskivis appear in any chests, and returns the position of both chests.

- Search for multiple possible items (OR) in a single chest by entering their names separated by '|'. Ex. `-l "vuoksikivi|kakkakikkare"` will return all chests with either a vuoksikivi or a kakkakikkare.

- Search for chests containing at minimum a specific number of items with the wildcard '\*' by itself. Ex. `-l "* * *"` will return all chests containing at least three items. `-l "chaos_die *"` will return all chests containing a chaos die and at least one other item.

- Search for potions by type, like `potion_normal`, `potion_secret`, and `potion_random_material` by entering their names like a normal item. Ex. `-l "potion_random_material"` will return all chests containing random material potions, regardless of what the actual contents of the potion are.

- Specific potion contents can be searched by enabling potion contents search with the `--potions` flag and searching `potion_` followed by the material's LUA name. Ex. `-l "potion_urine" --potions` for urine jars or `-l "potion_magic_liquid_hp_regeneration_unstable" --potions` for lively concoction (what a mouthful!).

- Search item pedestals as well with `--pedestals`. Eggs and broken wands can only spawn on pedestals, so use this flag when searching for them. Ex. `-l "egg_purple" --pedestals` will return all item pedestals with purple eggs on them. Aggregate searches are recommended for pedestals, since pedestals cannot have multiple items.

- Calculate spell drops with `--spells`. Spells are searched by `spell_` followed by their LUA name. Ex. `-l "spell_regeneration_field" --spells` will return all chests containing a circle of vigor.

- All searches support UNIX-like wildcards as well. Ex. `-l "potion_magic_liquid_*" --potions` returns all potions containing materials that start with `magic_liquid`.

- Blacklist items from returned chests by prefixing their names with '-'. Ex. `-l "* * -gold_nuggets"` will return all chests containing at least 2 items and no gold nuggets.

- Search for exact matches by including '-' on its own in a query. Ex. `-l "wand_T4NS -"` will return chests containing a tier 4 non-shuffle wand and no additional items.

## Wands
Wand search can be enabled with `--wands`. This includes wand altars in the search scope automatically. Filtering of chests with wands is slightly different than looking for straight items, depending on the objective.

- Wands themselves are represented as (somewhat difficult to read) strings in the output. Their format is `wand_[capacity]_[multicast]_[cast delay (sec)]_[reload (sec)]_[max mana]_[mana regen]_[spread (deg)]_[speed multiplier]_[shuffle]`. This way, every stat of the wand can be seen at a glance. **Don't search for wands with certain stats using `-l`! There is a separate option for wand stat filtering.**

- Wand spells are automatically included separately in the chest, and can be searched for just like normal spell card drops from chests. Always casts are prefixed with `ac_`, so a query like `-l "ac_spell_regeneration_field" --wands` can locate Always Cast circles of vigor. For non-always casts, the program doesn't differentiate between spells on a wand and random card drops, but searching without `--wands` or without `--spells` will allow only one of them to be generated if desired.

- Wand stats can be filtered with `-w`. This accepts a series of space-separated filters for stats, in the form `[stat][operator][number]`. For instance, `-w "capacity>26 multicast=1 shuffle=0"` filters for non-shuffle wands with more than 26 slots and no multicast. Allowed operators are >, >=, =, <=, and <. Allowed stats are `capacity`, `multicast`, `delay`, `reload`, `mana`, `regen`, `spread`, `speed`, and `shuffle`. Shuffle is 1 if the wand is a shuffle wand, 0 otherwise. The other stats are self-explanatory.

- By default, wand stat search returns all chests/wands that have a wand passing the wand filter **OR** loot passing the loot filter. This can be configured to only return chests/wands with both the requisite stats and the searched loot with `--wand-and-loot`. For instance, searching for a wand with specific stats and a desired always cast should be done with `-w "delay<0.20 mana>1000" -l "ac_mana_reduce" --wands --wand-and-loot`.

## End of Everything / TinyMode
EOE mode contains a few important differences from normal search. First, **only the starting seed is searched!** Batch size and seed count are ignored with this option. The search checks square blocks in a spiral from the defined center. `--EOE-radius` is used in place of batch size to control the size of these squares, and does roughly the same thing as batch size. The search will spiral outwards indefinitely on a given seed, until stopped. Otherwise, the chest search syntax is identical to normal, except that `--pedestals` does nothing.

TinyMode is identical to EOE mode, except instead of generating the loot of a greater treasure chest on every pixel, it generates a single tier 10 non-shuffle wand. Killing Tiny with a certain offset relative to TinyMode results will drop the searched wand. The feasibility of killing Tiny on specific pixels is debatable, but if you manage to do it you can get arbitrarily powerful tier 10 wands.

## Other biomes
Currently only the mines are fully supported. Every main path biome should work with similar accuracy, but side biomes are by no means guaranteed to function at as high of an accuracy or even at all. All biomes use their LUA names. Also supported are "mainpath", "tower", and "full", whose scopes are hopefully obvious.

## Other options
- Batch size controls how many seeds are computed at once. You should adjust this number to use close to as much VRAM as you have available, since larger batches run significantly faster per-seed.
- Max items controls how many items can be recorded in a single chest. Chests that exceed this number will still behave fine, but items over the limit will be removed from the contents. If you're searching for just one item, setting this to 1 may improve performance slightly, since chests with many items are not of use for such searches.
- Max chests controls the maximum number of chests that can be stored per biome. Extra chests will be completely ignored! You will get warnings if this is too low, so if you see messages about chest overflow, consider increasing this. Note that this is per-world, so there is no need to increase it for parallel world searches. Larger biomes obviously have more chests.
- Max tries dictates how many tries the generator will attempt to make for world generation. Use logging level 3 to see how many invalid maps are left on each try. Each try takes quite a bit of time, so running 5 tries but only being able to check 70% of maps is significantly faster than running 50 tries and checking 99.99% of them. There isn't much reason you should have to mess with this setting.
- Debug logging level does what it says on the tin. There are 7 logging levels, from 0 to 6, but anything above 4 will spam the console with a LOT of information, most of which is probably not useful except for debugging.
 
 ## For developers
 Do not look into this mess of a codebase if you value your sanity. If you do, and you make some improvements, please submit a pull request! I have no idea how to put 2 different languages in a VS project, so go <a href="https://github.com/pudy248/NoitaChestFinderCUDA/">here</a> for the main portion of this codebase.
