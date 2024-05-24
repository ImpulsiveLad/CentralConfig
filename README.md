CentralConfig is a Lethal Company mod to help organize and set config values in one place.

The original idea was that it would be a back up for if/when LLL removes its config but there are plenty requested functions so I just added any reasonable ones.

I was also able to fix up a few misc issues with the base game and LLL along the way.

This intial release only adds moon and dungeon settings so far but I plan on adding settings for enemies, scrap, and adding scrap, enemies, and dungeons to moons based on tags.

# Usage

Upon opening the game with the mod, only the broad settings will be generated. You can blacklist or whitelist elements (moons/dungeons) from being modified by the mod here and choose what groups of settings you want to set.

After making your choice of settings, you will need to reboot the game ***and enter a lobby*** in order for the config section for each individual element to be generated and applied.

Because there is a gap between portions of config generation, you will notice orphaned entries if you close the game without opening a lobby. ***This is not a concern.*** They will be applied again when you open a lobby and the current values will remain unchanged.

The mod generates these config sections so late on purpose in order for it to pull current values from other configs and have "the final say."

## Moons config:
Enable General Overrides? - allows altering of some basic properties including the route price, risk level, and description for each moon.

Enable Scrap Overrides? - allows altering of the min/max scrap count, the list of scrap objects on each moon, and a multiplier for the individual scrap item's values.

Enable Enemy Overrides? - allows altering of the max power counts and lists for enemies on each moon (Interior, Nighttime, and Daytime).

Enable Trap Overrides? - allows altering of the min/max count for each trap on each moon. !!(Modded traps are not yet supported, I would need to hand add support using its SpawnableMapObject data.)!!

Enable Weather and Tag Overrides? - allows altering of the possible weathers and ***adding*** tags to each moon, not removing or editing.

Enable Misc Overrides? - allows altering of miscellaneous traits of moons such as hidden/unhidden status, locked/unlocked status, if time exists, the time speed multiplier, and if time should wait until the ship lands to begin moving.

## Dungeon config:
Enable Overhauled Dungeon Generation? - Enabled by default, this changes how dungeons are generated and *may* prevent loading failures. I have not found any issues in my own testing but this is a very complex mechanic.

Enable Dungeon Size Overrides? - allows altering of the min/max dungeon size multipliers, and the size scaler. This also allows you to set the dungeon size multiplier applied by the individual moons.

Enable Dungeon Selection Overrides? - allows altering of the dungeon selection settings (By moon name, route price range, and mod name.

# Misc fixes

Aforementioned dungeon generation changes, I'm not sure what I did but I am yet to get a tile placement failure from even extremely large dungeons (I had the vanilla facility and wesley's toystore running fine at 1000x size).
Dungeons still break if they are made too small to have a main entrance.

The days until quota deadline is now unaffected by the day speed (Yeah I have no idea why it was to begin with either).

!!!This heavily uses LLL so only moons and dungeons supported by LLL will function.
LethalLib scrap *does* load in, they just don't appear in the `scan` command.
