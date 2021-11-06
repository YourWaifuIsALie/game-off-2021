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

## Mechanics

### Battles

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


## Aesthetics

2d cutout animation in the vein of mobage like Princess Connect