---
title: Design Doc
categories: [docs]
layout: custom
---
# Design Doc

I don't know how these are normally organized, but since this is a small project I'll throw everything here.

## Summary

A bug girl JRPG with programming-bug battle mechanics and speedrunner-bug power ups. 
A trash mob with no place in the current game revision leans into their buggy world to survive.

The game takes after Paper Mario in terms of a 2d overworld movement and colliding with enemies to engage in turn-based combat where the battle and overworld graphics are the same.
- No quick times, sorry

Using bug-based abilities trivializes encounters but causes destabilization of the game, raising the "bug-level" and generating aesthetic glitches and plot branching
- Plot not guaranteed for game jam

## Options

Resolution and full/window/borderless options
SFX and music volume

Everything else is probably a luxury
## Battles

Simple turned based battles.

Stats:
- Health
- Mana
- Attack Power
- Defense Power
- Speed
- Tags
- Level (fluff)

Actions:
- Attack
- Defend
- Ability
  - Traditional magic spells like "heal"
  - Bug abilities

Characters and targeted actions. Actions have effects which generally check stats + tags of characters involved.

Battle "bugs" take the form of battle mechanics + special abilities/options available to the combat actors.
- Over-healing past a certain number overflows back to negative and death
- Health values can be byte-swapped between little and big endian, transforming health values
- An ability to change what actor is being referenced by a targeting name (pointer)
- Items can be targets and special items can cause different effects when targeted (like "drop enemy table")

## Overworld movement

2D plane movement. Colliding with an enemy initiates a battle.

If enough content can be created, "bugged" movement abilities as unlockables for area progression. Otherwise, only walking.

Battles scripted as overworld enemies simply stand in doorways between rooms. Stretch goal of enemies dumbly walking towards you.

## Visuals

2d cutout animation in the vein of mobage like Princess Connect or Paper Mario.

At least 1 main character graphic and 3 enemies. 
- More is better as time allows

At least 1 "theme" for overworld/battle background (cave, forest, etc.)
- 1 far background image
- Foreground textured 3d stage
  - Edges of the map get a "floating island" appearance
  - Can make multi-height, but just have the main character move in the XZ axis and snap to the top Y axis
    - Jumping is unnecessary complication
- 2+ foreground decorations (i.e. trees + bushes)
  - Used throughout the stage, but also to separate fore and background

"Bug-level" tells the narrative as the world starts to break down and skip around as more bugs are used in combat
- Cute bluescreen ending where the main character pops out, waits our a reboot, and escapes to an internet browser
- Can just be a video

## Sound

Do SFX and music on my own if there's time at the end. Before the end, find free (monetary, copyright, etc.) SFX and music to use

Likely sfx:
- 1 attack sound per character
- Death sound
- Battle begin sound
- Battle win sound
- Walking sound

Likely music:
- Battle music
- Overworld music