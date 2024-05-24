CentralConfig is a mod for Lethal Company designed to help organize and set config values in one place. Initially intended as a backup for when/if LLL removes its config, it now includes numerous requested functions and miscellaneous fixes for the base game and LLL.

The mod currently focuses on moon and dungeon settings but I have plans to add settings for enemies, scrap, and added scrap, enemies, and dungeons based on the moon's tags.

# Usage
When you open the game with the mod, only the broad settings will be generated. Here’s how to use the mod effectively:

1. Initial Setup: You can blacklist or whitelist elements (moons/dungeons) from being modified by the mod and choose which groups of settings you want to configure.

2. Reboot Required: After choosing your settings, reboot the game and enter a lobby. This will generate and apply the config section for each individual element.

If you close the game without opening a lobby, you may notice orphaned entries. ***This is not a concern***; they will be applied when you next open a lobby, and current values will remain unchanged.

The mod generates these config sections late to pull current values from other configs and have "the final say."
# Configuration Options
## Moons Config
**Enable General Overrides?**
Allows altering basic properties like route price, risk level, and description for each moon.

**Enable Scrap Overrides?**
Allows altering the min/max scrap count, the list of scrap objects on each moon, and a multiplier for individual scrap item values.

**Enable Enemy Overrides?**
Allows altering the max power counts and lists for enemies on each moon (Interior, Nighttime, and Daytime).

**Enable Trap Overrides?**
Allows altering the min/max count for each trap on each moon. (I have to add support for modded traps manually.)

Note: Modded traps are not yet supported and require manual addition using SpawnableMapObject data.
Enable Weather and Tag Overrides?
Allows altering possible weathers and adding (not removing or editing) tags to each moon.

**Enable Misc Overrides?**
Allows altering miscellaneous traits such as hidden/unhidden status, locked/unlocked status, time existence, time speed multiplier, and if time should wait until the ship lands to begin moving.
## Dungeon Config
**Enable Overhauled Dungeon Generation?**
Enabled by default; changes dungeon generation to hopefully prevent any loading failures caused by a large multiplier. Testing has shown no issues, but this is a complex mechanic.

**Enable Dungeon Size Overrides?**
Allows altering the min/max dungeon size multipliers and the size scaler. Also allows setting the dungeon size multiplier applied by individual moons.

**Enable Dungeon Selection Overrides?**
Allows altering the dungeon selection settings by moon name, route price range, and mod name.
# Miscellaneous Fixes
Dungeon Generation: Enhanced dungeon generation to prevent tile placement failures, even with extremely large dungeons (e.g., vanilla facility and Wesley's toystore at 1000x size).
Note: Dungeons still break if too small to have a main entrance.

Quota Deadline: Days until the quota deadline are now unaffected by the day speed.
# Notes
This mod heavily relies on LLL, so only moons and dungeons supported by LLL will function.

LethalLib scrap loads but doesn't appear in the `scan` command.
