CentralConfig is a mod for Lethal Company designed to help organize and set config values in one place. Initially intended as a backup for when/if LLL removes its config, it now includes numerous requested functions and miscellaneous fixes for the base game and LLL.

The mod currently focuses on moon and dungeon settings but I have plans to add settings for enemies, scrap, and adding enemies, scrap, and dungeons to moons based on the moon's tags.

# Usage
When you open the game with the mod, only the broad settings will be generated. This is a two step process:

1. Initial Setup: You can blacklist or whitelist elements (moons/dungeons) from being modified by the mod and choose which groups of settings you want to configure. (Modify this after first launch)

2. Reboot Required: After choosing your settings, reboot the game and enter a lobby. This will generate and apply the config section for each individual element. (You can now set the rest of the config when you close out)

If you close the game without opening a lobby, you may notice orphaned entries. ***This is not a concern***; they will be applied when you next open a lobby, and current values will remain unchanged.

The mod generates these config sections late to pull current values from other configs and have "the final say."
# Configuration Options
## Moon Config
**Enable General Overrides?** -
Allows altering basic properties like route price, risk level, and description for each moon.

**Enable Scrap Overrides?** -
Allows altering the min/max scrap count, the list of scrap objects on each moon, and a multiplier for individual scrap item values.

**Enable Enemy Overrides?** -
Allows altering the max power counts and lists for enemies on each moon (Interior, Nighttime, and Daytime).

**Big Enemy Lists?** -
Lets you set all enemy spawn lists from just three strings (Helpful for spreadsheet junkies).

**Enable Trap Overrides?** -
Allows altering the min/max count for each trap on each moon. (I have to add support for modded traps manually.)

**Enable Weather Overrides?** -
Allows altering possible weathers for moons. **DO NOT USE WITH WEATHER REGISTRY INSTALLED !!!**

** Enable Time Settings?** - 
Allows altering time settings such as time existence, time speed multiplier, and if time should wait until the ship lands to begin moving.

**Enable Misc Overrides?** -
Allows altering miscellaneous traits such as hidden/unhidden status, and locked/unlocked status.
## Dungeon Config
**Enable Overhauled Dungeon Generation?** -
Enabled by default; changes dungeon generation to hopefully prevent any loading failures caused by a large multiplier. Testing has shown no issues, but this is a complex mechanic.

**Enable Dungeon Size Overrides?** -
Allows altering the min/max dungeon size multipliers and the size scaler. Also allows setting the dungeon size multiplier applied by individual moons.

**Enable Dungeon Selection Overrides?** -
Allows altering the dungeon selection settings by moon name, route price range, and mod name.

**Enable Enemy Injection by Current Dungeon?** - 
Allows adding/replacing enemies on levels based on the current dungeon (inside, day, and night.)

**Enable Scrap Injectiom by Current Dungeon?** -
Allows adding scrap to levels based on matching tags.
## Tags
**Enable Enemy Injection by Tag?** -
Allows adding enemies to levels based on matching tags (inside, day, and night).

Also allows you to replace enemies using "EnemyName1:EnemyName2,EnemyName1:EnemyName2" where the first name is the replacee and the second is the replacement (this would mostly be for biomatic enemies).

**Enable Scrap Injection by Tag?** -
Allows adding scrap to levels based on matching tags.
## Weather
**Enable Enemy Injection by Current Weather?** - 
Allows adding/replacing enemies on levels based on the current weather (inside, day, and night).

**Enable Scrap Injection by Current Weather?** - 
Allows adding scrap to levels based on the current weather as well as multipliers to the scrap amount and individiual scrap values.
## Enemies
**Free Them?** -
Extends the 20 inside/day/night enemy caps to the maximum and sets the interior enemy spawn waves to be hourly instead of every other hour.

**Scale Enemy Spawn Rate?** -
This setting adjusts the enemy spawn rate to match the new enemy powers.

**Remove Duplicate Enemies?** - 
Only keeps the highest rarity of a specific enemy in a spawnlist. This runs after tags and weather adds/replaces enemies.

**Keep Smallest Rarity?** -
The lowest rarity of a given enemy with be kept instead of its highest rarity (For setting above).

**Always Keep Zeros?** -
Any enemies with that have an entry of 0 rarity will be kept regardless (For the setting above above).
## Misc
**Accurate Clock?** - 
Causes the clock to update when time moves as opposed to every 3 seconds.

**Keep Orphaned Entries?** -
Prevents the config from 'cleaning' itself of removed entries, use to prevent accidental config entry loss. This will result in a more crowded and messy file if left true for long.

**Starting Seed** - 
Used in some bits of code, leave at -1 to randomize.

**Enable Fine Overrides?** -
Allows altering the fine for dead players, the reduction to the fine for recovering bodies, and if you want the fine value to be proportional to the players (if half the lobby is dead, you will be fined half of the fine amount). The company has its own fine values (No fines there by default).

**Enable Scan Node Extensions?** - 
Allows you to set the min/max ranges for the scan nodes on the ship, main entrance, and fire exits (if you have ScannableFireExit installed).
# Miscellaneous Fixes
**Dungeon Generation:** Optional safeguards for dungeon generation.

- Removes the retry hard cap. Instead relying on a set process of reattempts.
- If the dungeon fails to generate with the current size multiplier, it will reduce the multiplier by 10% every retry until it is below 1, in which it will reduce by 0.05 every attempt.
- The first time it reaches 0 or less, it will set the size back up (in case the input value was small such as 0.1).
- If the dungeon doesn't generate at any attempted sizes, it will log the event then generate the dungeon as the facility with the moons unmodified size multiplier.
- This allows the gameplay to continue seemlessly in the rare event of failure.

This process is faster than it sounds, even with a test size of 10000, I was able to have it find a valid generation size in 6 seconds.
This feature should only be able to fail if the dungeon is unable to generate at any positive size multiplier.

**Quota Deadline:** Days until the quota deadline are now unaffected by the day speed.
# Notes
This mod heavily relies on LLL, so only moons and dungeons supported by LLL will function, scrap and enemies that use LLL work.

LethalLib scrap loads but doesn't appear in the `scan` command, LL enemies work perfectly.

The mod uses Csync for all entries, this should hopefully prevent config desync.

If you find any bugs it will be faster to tell me on discord tbh.



*Tips are always appreciated* <3 <3 <3 <3 <3
https://ko-fi.com/impulsivelass
