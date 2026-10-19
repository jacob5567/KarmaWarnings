# KarmaWarnings

A client-side Risk of Rain 2 mod that warns you **before** you do something that raises your Karma, so you can decide whether it's worth it.

**Status: planned, not built yet.** There is no release. This README describes what the mod is meant to do.

[Karma](https://riskofrain2.wiki.gg/wiki/Karma) is a run setting that makes Mithrix harder. It goes up at fixed milestones and never goes down. Some milestones are decided long before they happen: once you take the Ritual Portal to Soulbound Threshold, you either beat the Anima Harvester or die. KarmaWarnings warns you at the earliest point where you can still back out.

## What you see

A short banner right above the crosshair, saying what you're about to do and how much Karma it adds. Examples of where it appears:

- **Entering a stage you can't leave without the milestone:** Void Fields, Gilded Coast, Bulwark's Ambry, Prime Meridian, Soulbound Threshold, Terminal Lucidity and the Solus fights. For Soulbound Threshold the warning starts at the Shrine of Descent.
- **Interacting:** the preplaced Legendary Chests in Abyssal Depths, Sundered Grove and Repurposed Crater, and a Halcyon Shrine that is close to filling.
- **Fights where timing matters:** killing a teleporter boss at 10% charge or less, and hurting the Newt in the Bazaar or SPEX in the Computational Exchange.
- **Items:** a pickup, shop terminal or item choice that would reach 3 Lunar items, 3 Meal items, 5 Hallowed items, 50 items, or an Elite Aspect.
- **Looping:** leaving the fifth stage of a loop.

Nothing is shown when Karma is turned off for the run, when you're already at 8 Karma, or when you've already earned that milestone and it only counts once. If you join a run in progress and the mod can't tell, it still warns.

Some things can't be warned about in time: an Elite Aspect picked up by walking over it with an empty equipment slot, and damage to the Newt or SPEX that is already past the point of no return.

## Multiplayer

The mod never changes gameplay and never sends anything over the network. It works when you join a host who doesn't have it installed. As a client it can't see which one-time milestones the host has already counted, so it works that out from what it can see.

## Configuration

A single setting, `Enabled`, in `BepInEx/config/sevenstack.KarmaWarnings.cfg`, or in the in-game settings menu if [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) is installed.

## Building

Requires the .NET 8 SDK, a local Risk of Rain 2 install, and [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) in the r2modman profile (it is only referenced for the settings menu). `dotnet build -c Release` builds the plugin and copies it into the r2modman Default profile; add `-p:CopyToProfile=false` to skip the copy. `./build.sh` also writes the Thunderstore zip to `dist/`. The default game and profile paths are in `Directory.Build.props`; override them on the command line or in a `Directory.Build.props.user` file.

## License

[GPL-3.0](LICENSE)
