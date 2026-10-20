# KarmaWarnings

A client-side Risk of Rain 2 mod that warns you **before** you do something that raises your Karma, so you can decide whether it's worth it.

**Status: in development.** The warnings below are written but haven't been tested in game yet, and there is no release.

[Karma](https://riskofrain2.wiki.gg/wiki/Karma) is a run setting that makes Mithrix harder. It goes up at fixed milestones and never goes down. Some milestones are decided long before they happen: once you take the Ritual Portal to Soulbound Threshold, you either beat the Anima Harvester or die. KarmaWarnings warns you at the earliest point where you can still back out.

## What you see

A short banner right above the crosshair: "Increases Karma", with a line under it saying what you're about to do. Examples of where it appears:

- **Entering a stage you can't leave without the milestone:** Void Fields, Gilded Coast, Bulwark's Ambry, Prime Meridian, Soulbound Threshold, Terminal Lucidity and the Solus fights. The Shrine of Descent and the teleporter it turns into a catastrophe teleporter warn about the +1 for surviving it, and the Ritual Portal warns separately about the +1 for killing the Anima Harvester.
- **Interacting:** the preplaced Legendary Chests in Abyssal Depths, Sundered Grove and Repurposed Crater, and a Halcyon Shrine, which also tells you when to cash out to avoid the Karma.
- **Fights where timing matters:** killing a teleporter boss at 10% charge or less, and hurting the Newt in the Bazaar or SPEX in the Computational Exchange.
- **Items:** a pickup, shop terminal or item choice that would reach 3 Lunar items, 3 Meal items, 5 Hallowed items, 50 items, an Elite Aspect, or Sautéed Worms.
- **Looping:** leaving the fifth stage of a loop, including for the moon.

[MESSAGES.md](https://github.com/jacob5567/KarmaWarnings/blob/main/MESSAGES.md) lists every message and when it appears.

Nothing is shown when Karma is turned off for the run, when you're already at 8 Karma, or when you've already earned that milestone and it only counts once. When the mod can't tell whether a milestone is earned, it still warns.

Some things can't be warned about in time: an Elite Aspect picked up by walking over it with an empty equipment slot, and damage to the Newt or SPEX that is already past the point of no return.

## Notes

- Warnings use the game's own rules, read from the game's code. A few things couldn't be confirmed without playing, so expect to see some mistakes: which green portal leads to Prime Meridian, the Solus fights' portals, the elemental Elder Lemurians' names, which way a client sees the Primordial Teleporter aligned, and the vulture egg count.
- Not warned about yet: an open item-choice panel (Command, Void potentials), a ready CHEF meal, and items still falling out of a chest.
- Not warned about on purpose: the Obelisk in A Moment, Fractured (the run ends right after), and item thresholds crossed by monsters or drones, such as with Artifact of Evolution.
- Item warnings at shop terminals only show when you can afford the item, and skip terminals paid for with items. A terminal showing "?" still warns, so the warning can hint at what's inside.

## Multiplayer

The mod never changes gameplay and never sends anything over the network. It works when you join a host who doesn't have it installed. As a client it can't see which one-time milestones the host has already counted, so it works that out from what it can see. The host reads the game's own completed flag. A client marks a stage milestone earned when Karma rises while it is in that stage (Void Fields, Gilded Coast, Prime Meridian, the Solus stages, the Reaper fight, Commencement), marks item milestones earned when any player's inventory meets them, and marks the Reaper beaten from the game's progression state. Anything it can't tell still warns.

## Configuration

Edit `BepInEx/config/sevenstack.KarmaWarnings.cfg`, or use the in-game settings menu if [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) is installed.

| Setting | Default | Description |
|---|---|---|
| `Enabled` | true | Show a warning above the crosshair before you do something that raises your Karma. |
| `DebugLogging` | false | Enable the dump key, which logs the Karma state and what the mod sees around you. |
| `DebugAlwaysShow` | false | Show a warning even when nothing would raise your Karma, for testing. |
| `DumpKey` | F8 | With `DebugLogging` on, press to log the Karma state, what the warning banner shows, the exit you're aiming at, and what the fight checks for this stage see. |

## Building

Requires the .NET 8 SDK, a local Risk of Rain 2 install, and [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) in the r2modman profile (it is only referenced for the settings menu). `dotnet build -c Release` builds the plugin and copies it into the r2modman Default profile; add `-p:CopyToProfile=false` to skip the copy. `./build.sh` also writes the Thunderstore zip to `dist/`. The default game and profile paths are in `Directory.Build.props`; override them on the command line or in a `Directory.Build.props.user` file.

The version lives in `thunderstore/manifest.json`. To release, bump it, run `./build.sh`, and publish a GitHub release tagged `v<version>` with the zip attached. The publish workflow then uploads that zip to Thunderstore.

## License

[GPL-3.0](https://github.com/jacob5567/KarmaWarnings/blob/main/LICENSE)
