# Warning messages

Every warning is a banner right above the crosshair. It has an **Increases Karma** headline and, under it, one of the messages below. The one exception is noted in the Interactables table.

A warning only appears when all of these are true:

- `Enabled` is on.
- You have a living body.
- Karma is on for the run and below 8.
- The milestone is in this run (its expansion is enabled).
- It isn't a one-time milestone you've already earned.

"Aim" means the object the game is outlining for interaction. When several warnings apply at once, the one earlier in the order below wins: stages and portals, then interactables, then combat, then items, then the new-loop warning.

## Stages and portals

You aim at the portal. Where a portal is matched by the stage it leads to, any exit to that stage counts.

| Message | Scenario |
|---|---|
| Void Fields: starting and finishing the cells | The Null Portal |
| Gilded Coast: can't leave until Aurelionite dies | The Gold Portal |
| Bulwark's Ambry: can't leave until the trial ends | The Ambry portal, which leads to Bulwark's Ambry or one of its Seekers of the Storm variants (Aphelian, Tar or Wisp Research). The mod also checks the portal's destination whenever it can be read. |
| Soulbound Threshold: kill the Anima Harvester or die to leave | The Ritual Portal. Not shown once the Reaper is beaten. |
| Terminal Lucidity: killing the Echo of Providence | The Veil of Memory portal |
| Neural Sanctum: killing the Solus Heart | The Decrypted Portal (DLC3) |
| Solutional Haunt: killing the Solus Wing | Any exit whose prefab leads to Solutional Haunt (DLC3) |
| Prime Meridian: killing False Son | A green portal whose stage for the next stage number is Prime Meridian |
| Starting a new loop | A scene exit or a charged teleporter on stage 5, when none of the messages above applies. Not on the moon, and not in Prismatic Trials. |

## Interactables

You aim at the object.

| Message | Scenario |
|---|---|
| Surviving the catastrophe teleporter | An unused Shrine of Descent, or a teleporter that is about to be a catastrophe teleporter (the shrine has been used and it isn't charged yet). Also shown after the Reaper is beaten. |
| Opening this Legendary Chest | An unopened preplaced Legendary Chest in Abyssal Depths, Sundered Grove or Repurposed Crater. A chest the game spawns elsewhere doesn't count. |
| Fully draining this shrine and killing the Halcyonite | A Halcyon Shrine that hasn't started draining (DLC2) |
| Interact again before it fills to avoid Karma | A Halcyon Shrine that is draining. This is the only message shown **without** the "Increases Karma" headline, because it's advice. |

## Combat

No aiming needed unless stated.

| Message | Scenario |
|---|---|
| Killing the boss under 10% teleporter charge | The teleporter isn't charged, it is at 10% charge or less, and the boss is at 50% health or lower |
| Hurting the Newt gets you kicked out | In the Bazaar, the Newt is at full health and you're aiming at it, or it's between 99% and 100% health |
| The Newt will kick you out | In the Bazaar, the Newt is at 99% health or lower. Shown whether or not you aim at it. |
| Hurting SPEX gets you kicked out | SPEX is awake, and either at full health with you aiming at it, or between 90% and 100% health (DLC3) |
| SPEX will kick you out | SPEX is awake and at 90% health or lower |
| Killing both elemental Elder Lemurians | In Abandoned Aqueduct, while one of the hidden chamber's Fire or Ice Elder Lemurians is alive |
| Killing the Alloy Worship Unit | In Siren's Call, while the Alloy Worship Unit is alive |
| Six Vulture Eggs summon the Alloy Worship Unit | In Siren's Call, before it appears, while you're aiming at a Vulture Egg |

## Items

The message is **Gives you …** followed by the phrases that apply, joined with "and", for example "Gives you 3 Lunar items and Beads of Fealty". It appears when taking the item would push you over a threshold, in any of these cases:

- You're aiming at a pickup on the ground.
- You're aiming at a shop terminal that is available, isn't showing "?", and is paid for with money or lunar coins.
- A pickup within 3 m would be taken just by walking over it.

| Phrase | Taking the item would give you |
|---|---|
| 3 Lunar items | 3 Lunar items in total. A Lunar equipment counts as one, and temporary and rental copies count. |
| 3 Meal items | 3 Meal items, or any Wyrm-on-hit item (DLC3) |
| 5 Hallowed items | 5 Hallowed items (DLC4) |
| 50 items | 50 permanent items. Rentals, temporary items, and hidden or consumed ones don't count. |
| an Elite Aspect | An Elite Aspect equipment |
| Beads of Fealty | Beads of Fealty, but not a rental |

## Debug

| Message | Scenario |
|---|---|
| Debug: test warning | `DebugAlwaysShow` is on. It overrides everything else and ignores the Karma gates. |

## Not warned about

- The Altar of Gold, and the portal dialer on Sky Meadow and Helminth Hatchery (the Compound Generator and its laptop): using them only opens a portal, and the portal is what commits you.
- The Obelisk in A Moment, Fractured: the run ends right after.
- Item thresholds crossed by monsters or drones, such as with Artifact of Evolution.
- An open item-choice panel (Command, Void potentials), a ready CHEF meal, and items still falling out of a chest. These aren't covered yet.
