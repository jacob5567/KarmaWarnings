# Warning messages

Every warning is a banner right above the crosshair. It has an **Increases Karma** headline and, under it, one of the messages below. Each message finishes the sentence the headline starts, in the present tense: "Increases Karma *when you kill the Anima Harvester*". The one exception is noted in the Interactables table.

A warning only appears when all of these are true:

- `Enabled` is on.
- You have a living body.
- Karma is on for the run and below 8.
- The milestone is in this run (its expansion is enabled).
- It isn't a one-time milestone you've already earned.

"Aim" means the object the game is outlining for interaction. When several warnings apply at once, the one earlier in the order below wins: stages and portals, then interactables, then combat, then items, then the new-loop warning, and the debug test message last.

## Stages and portals

You aim at the portal. Where a portal is matched by the stage it leads to, any exit to that stage counts.

| Message | Scenario |
|---|---|
| when you start and finish the Void cells | The Null Portal |
| when you kill Aurelionite | The Gold Portal |
| when you finish the Artifact Trial | The Ambry portal, which leads to Bulwark's Ambry or one of its Seekers of the Storm variants (Aphelian, Tar or Wisp Research). The mod also checks the portal's destination whenever it can be read. |
| when you kill the Anima Harvester | The Ritual Portal. Not shown once the Reaper is beaten. |
| when you kill the secret boss | The Veil of Memory portal |
| when you kill the Solus Heart | The Decrypted Portal (DLC3) |
| when you kill the Solus Wing | Any exit whose prefab leads to Solutional Haunt (DLC3) |
| when you kill False Son | A green portal whose stage for the next stage number is Prime Meridian: the one on Treeborn Colony or Golden Dieback |
| when you start a new loop | On stage 5, when none of the messages above applies: a scene exit, or a teleporter that hasn't been started yet or is fully charged. Not while a teleporter charges, not on the moon, and not in Prismatic Trials. |
| when you go to the moon | The same, at a Primordial Teleporter aligned with the moon. Leaving stage 5 for the moon also starts a new loop. |

## Interactables

You aim at the object.

| Message | Scenario |
|---|---|
| when you survive the catastrophe teleporter | An unused Shrine of Descent, or a teleporter that is about to be a catastrophe teleporter (the shrine has been used and it isn't charged yet). Also shown after the Reaper is beaten. |
| when you open this Legendary Chest | An unopened preplaced Legendary Chest in Abyssal Depths, Sundered Grove or Repurposed Crater. A chest the game spawns elsewhere doesn't count. |
| when you kill the Halcyonite | A Halcyon Shrine that hasn't started draining (DLC2) |
| Interact again before it fills to avoid Karma | A Halcyon Shrine that is draining. This is the only message shown **without** the "Increases Karma" headline, because it's advice. |

## Combat

No aiming needed unless stated.

| Message | Scenario |
|---|---|
| when you kill the boss under 10% charge | The teleporter isn't charged, it is at 10% charge or less, and the boss is at 50% health or lower |
| when the Newt kicks you out | In the Bazaar, the Newt is at full health and you're aiming at it, or it's between 99% and 100% health |
| when SPEX kicks you out | SPEX is awake, and either at full health with you aiming at it, or between 90% and 100% health (DLC3) |
| when you kill both elemental Elder Lemurians | In Abandoned Aqueduct, while one of the hidden chamber's Fire or Ice Elder Lemurians is alive |
| when you kill the Alloy Worship Unit | In Siren's Call, while the Alloy Worship Unit is alive |
| when you kill the Alloy Worship Unit that six eggs summon | In Siren's Call, before it appears, while you're aiming at a Vulture Egg |

## Items

The message is **when you take this and have …** followed by the phrases that apply, joined with "and", for example "when you take this and have 3 Lunar items and Beads of Fealty". It appears when taking the item would push you over a threshold, in any of these cases:

- You're aiming at a pickup on the ground.
- You're aiming at a shop terminal that is available, is paid for with money or lunar coins, and that you can afford right now. A terminal showing "?" counts too.
- A pickup within 5 m would be taken just by walking over it.

| Phrase | Taking the item would give you |
|---|---|
| 3 Lunar items | 3 Lunar items in total. A Lunar equipment counts as one, and temporary and rental copies count. |
| 3 Meal items | 3 Meal items (DLC3) |
| Sautéed Worms | Sautéed Worms (DLC3), which counts for the Meal milestone on its own |
| 5 Hallowed items | 5 Hallowed items (DLC4) |
| 50 items | 50 permanent items. Rentals, temporary items, and hidden or consumed ones don't count. |
| an Elite Aspect | An Elite Aspect equipment |
| Beads of Fealty | Beads of Fealty, but not a rental |

## Debug

| Message | Scenario |
|---|---|
| when DebugAlwaysShow is on | `DebugAlwaysShow` is on and no other warning applies. It ignores the Karma gates, so it shows with Karma off or at 8. |

## Not warned about

- The Altar of Gold, and the portal dialer on Sky Meadow and Helminth Hatchery (the Compound Generator and its laptop): using them only opens a portal, and the portal is what commits you.
- The Obelisk in A Moment, Fractured: the run ends right after.
- Item thresholds crossed by monsters or drones, such as with Artifact of Evolution.
- An open item-choice panel (Command, Void potentials), a ready CHEF meal, and items still falling out of a chest. These aren't covered yet.
