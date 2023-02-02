# pudy248's Chest Loot Searcher
DISCLAIMER: There are a few bugs in the Wang tiler worldgen, which makes this program only correct ~60-70% of the time. If a seed doesn't have a chest at the reported position, try another seed. Sometimes it can take a few attempts.

## Installation
Simply download and extract the latest release and open a command prompt in the folder. Make sure CUDA and .NET are installed on your system. Then, in a command prompt, type in GCFinder followed by your desired options.

For advanced users, you can also compile this program from scratch if you want to tinker with things. The Visual Studio solution should have everything configured properly.

## Usage
This program won't do very much if you just run the EXE. All options are available in the form of command-line arguments. Run GCFinder --help in CMD for a short summary of each one. The option you'll use most is -l, as that is the loot filter for chests. A list of valid items can be found in matchlist.txt, and advanced syntax is documented in the option documentation below.

## FAQ
Q: I got an error related to CUDA memory allocation!

A: You don't have enough VRAM for the requested query. The easiest way to fix this is by decreasing batch size with -z. Other options that increase VRAM usage are max items per chest and chests per biome, try and keep these as low as you can without getting overflow warnings. More parallel worlds also use more VRAM.

Q: I got warnings related to chest density!

A: You haven't allocated enough space for chests in each biome, so extra chests are being ignored. Increase --max-chests-per-biome until the warnings disappear.

Q: The program said there was a chest where there wasn't one!

A: Read the disclaimer at the top of this readme. The program is occasionally wrong, but it's right often enough to be useful. I will try and solve the accuracy issue as soon as I find out what's causing it.

Q: How quickly does this run?

A: It depends on your hardware. Most graphics cards should be able to check at least 500 seeds per second, but better ones can easily reach 2-3000 seeds a second. On my machine, with an RTX 2060, I can go through roughly 30 million seeds an hour.

Q: There aren't any results for my search!

A: Make sure there aren't any typos in your search list! This is the most common cause of empty searches. Also, make sure you aren't searching for something too rare! Having multiple items in a chest is rare in the first place, and searching for 3 or more specific items (ex. 3 paha silmas) in a normal chest will likely give you no results, as these chests are often less than 1 in 100 million! Also, double check that you aren't searching for greed-only items without -g or non-greed items with -g.

## Search string syntax
The filter string functionality is relatively versatile. Here are all of its features, in order of complexity.
- Search for chests containing at least a single specified item by entering its name. All searches except those with '-' can include extra items besides the searched ones. Ex. `-l "kiuaskivi"` returns all chests in a seed with at least one kiuaskivi.

- Search for multiple items (AND) in a single chest by entering their names separated by spaces. Ex. `-l "kiuaskivi kiuaskivi"` returns all chests in a seed with two kiuaskivis in the same chest.

- Aggregate searches between all chests in a seed by adding the option -a. Ex. `-l "ukkoskivi ukkoskivi" -a` will return all seeds where two ukkoskivis appear in any chests, and returns the position of both chests.

- Search for multiple possible items (OR) in a single chest by entering their names separated by '|'. Ex. `-l "vuoksikivi|kakkakikkare"` will return all chests with either a vuoksikivi or a kakkakikkare.

- Search for chests containing at minimum a specific number of items with the wildcard '\*' Ex. `-l "* * *"` will return all chests containing at least three items. `-l "chaos_die *"` will return all chests containing a chaos die and at least one other item.

- Search for potions by type, like `potion_normal`, `potion_secret`, and `potion_random_material` by entering their names like a normal item. Ex. `-l "potion_random_material"` will return all chests containing random material potions, regardless of what the actual contents of the potion are.

- Specific potion contents can be searched by enabling potion contents search with the -e flag and searching "potion_" followed by the material's name in the game code. Ex. `-l "potion_urine" -e` for urine jars or `-l "potion_magic_liquid_hp_regeneration_unstable" -e` for lively concoction (what a mouthful!).

- Search item pedestals as well with -k. Eggs and broken wands can only spawn on pedestals, so use this flag when searching for them. Ex. `-l "egg_purple" -k` will return all item pedestals with purple eggs on them. Aggregate searches are recommended for pedestals, since they cannot have multiple items per pedestal.

- Blacklist items from returned chests by prefixing their names with '-'. Ex. `-l "* * -gold_nuggets"` will return all chests containing at least 2 items and no gold nuggets.

- Search for exact matches by including '-' on its own in a query. Ex. `-l "wand_T4NS -"` will return chests containing a tier 4 non-shuffle wand and no additional items.

## Other biomes
Currently only the mines are fully supported. Every main path biome should work with similar accuracy, but side biomes are by no means guaranteed to function at as high of an accuracy or even at all. All biomes use their LUA names.

## Other options
- Batch size controls how many seeds are computed at once. You should adjust this number to use close to as much VRAM as you have available, since larger batches run significantly faster per-seed.
- Max items per chest controls how many items can be recorded in a single chest. Chests that exceed this number will still behave fine, but items over the limit will be removed from the contents. If you're searching for just one item, setting this to 1 may improve performance slightly, since chests with many items are not of use for such searches.
- Max chests per biome controls the maximum number of chests that can be stored per biome. Extra chests will be completely ignored! You will get warnings if this is too low, so if you see messages about chest overflow, consider increasing this. Note that this is per-world, so there is no need to increase it for parallel world searches. Larger biomes obviously have more chests.
- Max tries dictates how many tries the generator will attempt to make for world generation. Use logging level 3 to see how many invalid maps are left on each try. Each try takes quite a bit of time, so running 5 tries but only being able to check 70% of maps is significantly faster than running 50 tries and checking 99.99% of them. There isn't much reason you should have to mess with this setting.
- Debug logging level does what it says on the tin. There are 7 logging levels, from 0 to 6, but anything above 4 will spam the console with a LOT of information, most of which is probably not useful except for debugging.
 
 ## For developers
 Do not look into this mess of a codebase if you value your sanity. If you do, and you make some improvements, please submit a pull request! I have no idea how to put 2 different languages in a VS project, so go <a href="https://github.com/pudy248/NoitaChestFinderCUDA/">here</a> for the main portion of this codebase.
