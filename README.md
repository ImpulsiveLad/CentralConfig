CentralConfig is a mod for Lethal Company designed to help organize and set config values in one place. Initially intended as a backup for when/if LLL removes its config, it now includes numerous requested functions and miscellaneous fixes and tweaks for both the base game and LLL.

This mod purposefully avoids heavier settings that could lead to bugs being birthed. I.E. how other mods may allow players to change enemy mortality which is bound to cause problems.
CentralConfig is design with compatibility in mind and doesn't affect these settings that can break things with ease. Such specific settings would have to be made by the original developer.

# Usage
When you open the game with the mod, only the broad settings will be generated (no settings for specific moons/dungeons/tags/weathers/etc). This is a two step process:

You will start by blacklisting or whitelisting moons/dungeons/tags/weathers/etc from being modified by the mod and choose which broader settings you want to configure for each (Modify this after first launch).

Now you must reboot the game and enter a lobby. This will generate and apply the config settings for each individual element (You can now access and set the other settings after exiting out).

(I have not checked myself but this process is reportedly smoother in the Gale launcher due to the config regeneration button.)

If you close the game without opening a lobby, you may notice orphaned (darkened out) entries. ***This is not a concern***; they will be generated again when you next open a lobby, and their current values will remain unchanged.

I strongly recommend players to keep a copy of their config file somewhere safe in case an entry (or entries) undergo a name change and regenerates because of it.

The mod generates these config sections during runtime in order to pull its current config values from other mod's configs and have "the final say" in what this settings will be in game.
# Configuration Options
## Dungeons
**Enable Dungeon Size Overrides?** -
Allows altering various dungeon size related numbers. This includes the moon-tied size multipliers, the Dungeon's Map Tile Size (which is divided from the moon's size), a Dungeon specific min/max size clamp applied after the MTS is factored in, the scaler to determine how strict the size clamp should be, and a Dungeon specific min/max random size multiplier applied after the clamping.

**Enable Dungeon Selection Overrides?** -
Allows altering the dungeon selection pool tied to the dungeon by moon name, level tags, route price range, and mod name.

**Enable Enemy Injection by Current Dungeon?** - 
Allows adding/replacing enemies on moons based on the current dungeon (inside, day, and night).

**Enable Scrap Injectiom by Current Dungeon?** -
Allows adding scrap to moons based on the current dungeon.
## Enemies
**Free Them?** -
Extends the 20 inside/day/night enemy caps to the maximum and sets the interior enemy spawn waves to be hourly instead of every other hour.

**Scale Enemy Spawn Rate?** -
This setting adjusts the enemy spawn rate to match the moon's new max enemy counts (double the original will make twice as many enemies spawn).

**Remove Duplicate Enemies?** - 
Only keeps the highest rarity of a specific enemy in a spawnlist. This runs after tags, the current dungeon, and the current weather adds/replaces enemies.

**Keep Smallest Rarity?** -
The lowest rarity of a given enemy with be kept instead of its highest rarity (For setting above).

**Always Keep Zeros?** -
Any enemies with that have an entry of 0 rarity will be kept regardless, use this to prevent specific enemies from spawning under certain conditions (For the setting above above).

**Log Current Enemy Tables?** -
This setting is mostly for information or debugging, this will log the full indoor, day, and night enemy pools 10 seconds into the match.
## Moons
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

**Enable Time Settings?** - 
Allows altering time settings such as time existence, time speed multiplier, and if time should wait until the ship lands to begin moving.

**Enable Misc Overrides?** -
Allows altering miscellaneous traits such as hidden/unhidden status, and locked/unlocked status.

**Rename Celest?** -
Changes the name of `Celest` to `Celeste`, fixing the age old string confusion. If this is true, you will have to use `Celeste` for any config settings you want to apply to it.
## Tags
**Enable Enemy Injection by Tag?** -
Allows adding/replacing enemies on moons based on matching tags (inside, day, and night).

**Enable Scrap Injection by Tag?** -
Allows adding scrap to moons based on matching tags.
## Weather
**Enable Enemy Injection by Current Weather?** - 
Allows adding/replacing enemies on moons based on the current weather (inside, day, and night).

**Enable Scrap Injection by Current Weather?** - 
Allows adding scrap to moons based on the current weather as well as multipliers to the scrap amount and individual scrap values.
## Misc
**Accurate Clock?** - 
Causes the clock to update when time moves as opposed to every 3 seconds.

**Keep Orphaned Entries?** -
Prevents the config from 'cleaning' itself of removed entries, use to prevent accidental config entry loss. This will result in a more crowded and messy file if left true for long.

**Starting Seed** - 
Sets the `randonMapSeed`, leave at -1 to randomize. This is usually not set until loading into a level which causes weathers, the store, and I certain other things to always be the same on Day 1.

**Enable Fine Overrides?** -
Allows altering the fine for dead players, the reduction to the fine for recovering bodies, and if you want the fine value to be proportional to the players (if half the lobby is dead, you will be fined half of the fine amount). The company has its own fine values (No fines there by default).

**Enable Scan Node Extensions?** - 
Allows you to set the min/max ranges for the scan nodes on the ship, main entrance, and fire exits (if you have ScannableFireExit installed).
# Miscellaneous Fixes
**Dungeon Generation:** Optional setting that enacts safeguards for the dungeon generation process.

- Extends the retry cap and makes it dynamic, instead relying on a set loop of reattempts.
- If the dungeon fails to generate with the current size multiplier, it will reduce the multiplier by a percentage every retry until the value is low and it instead subtracts by a set value.
- The first time it reaches 0 or less, it will set the size back up (in case the intial input value was small such as 0.1x).
- If the dungeon completely fails to generate, then the game will generate the dungeon as the default facility (Preferable to getting softlocked).
- This allows the gameplay to continue seemlessly in the rare event of failure.

The dungeon should almost never fail on default settings from developers, it is far more likely to fail if you significantly change its calculate size, often by making it smaller than intended.

You can set both the # of unmodified attempts and the # of modified attempts seperated by size brackets (default is 20 and 25 respectively).

**Quota Deadline:** Days until the quota deadline are now unaffected by the day speed.
# Notes
This mod heavily relies on LLL, so only moons and dungeons supported by LLL will function, scrap and enemies that use LLL OR LL are compatible.

The mod uses Csync for all entries, this should hopefully prevent config desync but I highly advice sharing config files.

If you find any bugs it will be faster to tell me on discord tbh.



*Tips are always appreciated* <3 <3 <3 <3 <3
https://ko-fi.com/impulsivelass
