![](/Assets/Title_Banner_Clear.png)
### This is the source code for a low-fantasy RPG/Strategy auto-battler game (e.g. like Ogre Battle) being built using [OP Game Engine](https://github.com/Oblivionburn/OP_Engine) in VS 2022.
#
# Current state: Playable Alpha
>[!NOTE]
>Releases will only be available up through Beta state of the game prior to actual release on Steam, but code changes will stay updated on here and remain available for those who wish to compile their own modded version. After release of the game, the game will need to be purchased on Steam to acquire media files for the Content folder (e.g. music, sounds, and textures).
#
### Features currently in the game:
- Auto-battler combat
  - 3x3 squad formations (position effects damage dealt and received)
  - No classes (characters are what they wear)
- Randomly generated Worldmap with 20 locations progressively unlocked (each location has a Local Map where battle takes place)
- Local Maps consist of:
  - 1 Player Base
  - 1 Enemy Base (capturing it completes a map)
  - At least 1 Market Town to purchase equipment
  - At least 1 Academy Town to recruit more characters
  - Neutral Towns to capture/liberate for passive gold income
  - Various types of terrain that effect movement speed
- Randomly generated enemy squads in every Local Map which increase in amount/difficulty/equipment across locations on the Worldmap
- Capturing/liberating towns yields 1 gold each, every in-game hour (stacks across every map)
- Individual character XP/Level progression with 4 combat stats:
  - Strength - increases melee/bow weapon damage
  - Intelligence - increases grimoire(magic) damage
  - Dexterity - increases hit chance with melee/bow weapons (magic always hits)
  - Agility - increases dodge chance
  - Note: stat increases per Level happen automatically based on equipped weapon
- Armor/Weapons that can be equipped on characters
- 14 types of Runes that can be attached to armor/weapons and paired for various effects (similar to FF7 materia system), which also have XP/Level progression to increase their effectiveness over time
- Intro of the story, which also serves as the tutorial for the game
#
### Planned features not yet implemented:
- More Story/Lore
- Side-quests for special armor/weapons
- Finding hidden items in the world
- More music for variety
- Hopefully some graphical improvements
