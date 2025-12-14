using System;
using System.Linq;
using Microsoft.Xna.Framework;
using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class ArmyUtil
    {
        #region Variables



        #endregion

        #region Constructors

        

        #endregion

        #region Methods

        public static void InitArmies()
        {
            CharacterManager.Armies.Clear();

            //Ally army
            Army army = new Army
            {
                ID = Handler.GetID(),
                Name = "Ally"
            };
            CharacterManager.Armies.Add(army);

            Squad ally_squad = NewSquad("Ally");
            army.AddSquad(ally_squad);

            //Reserves
            Army reserves = new Army
            {
                ID = Handler.GetID(),
                Name = "Reserves"
            };
            CharacterManager.Armies.Add(reserves);

            Squad reserves_squad = NewSquad("Reserves");
            reserves.AddSquad(reserves_squad);

            //Special army
            Army special = new Army
            {
                ID = Handler.GetID(),
                Name = "Special"
            };
            CharacterManager.Armies.Add(special);

            Squad special_squad = NewSquad("Special");
            special.AddSquad(special_squad);

            //Enemy
            Army enemies = new Army
            {
                ID = Handler.GetID(),
                Name = "Enemy"
            };
            CharacterManager.Armies.Add(enemies);
        }

        public static Squad NewSquad(string type)
        {
            Squad squad = new Squad
            {
                ID = Handler.GetID(),
                Location = new Location(),
                Type = type
            };

            if (type == "Ally")
            {
                squad.Texture = Handler.GetTexture("Token_Ally");
            }
            else if (type == "Enemy")
            {
                squad.Texture = Handler.GetTexture("Token_Enemy");
            }

            if (squad.Texture != null)
            {
                squad.Image = new Rectangle(0, 0, squad.Texture.Width, squad.Texture.Height);
            }

            return squad;
        }

        public static void Gen_EnemySquads(Army army, int map_level)
        {
            army.Squads.Clear();

            int squads = 1;
            for (int i = 0; i < map_level; i++)
            {
                if (i == 1)
                {
                    squads = 2;
                }
                else if (i % 2 == 0)
                {
                    //+9 squads at level 20
                    squads++;
                }
            }

            for (int i = 1; i <= squads; i++)
            {
                Squad enemy_squad = NewSquad("Enemy");
                army.AddSquad(enemy_squad);

                if (i == 1)
                {
                    enemy_squad.Assignment = "Guard Base";
                }
                else
                {
                    enemy_squad.Assignment = "Guard Nearest Town";
                    AI_Util.RandomAssignment(enemy_squad);
                }

                Gen_EnemySquad(enemy_squad, map_level + 1, -1, -1, -1, -1);
            }
        }

        public static void Gen_EnemySquad(Squad squad, int map_level, int chars_override, int class_type_override, int min_tier_override, int max_tier_override)
        {
            //Check if first squad in the army
            bool first_squad = squad.Army.Squads.Count == 1;

            int min_chars = 1;
            if (map_level >= 2 && map_level < 10)
            {
                min_chars = 2;
            }
            else if (map_level >= 10 && map_level < 15)
            {
                min_chars = 3;
            }
            else if (map_level >= 15 && map_level < 20)
            {
                min_chars = 4;
            }
            else if (map_level >= 20)
            {
                min_chars = 5;
            }

            int max_chars = 1;
            if (map_level >= 2 && map_level < 4)
            {
                max_chars = 2;
            }
            else if (map_level >= 4 && map_level < 7)
            {
                max_chars = 3;
            }
            else if (map_level >= 7 && map_level < 13)
            {
                max_chars = 4;
            }
            else if (map_level >= 13)
            {
                max_chars = 5;
            }

            CryptoRandom random = new CryptoRandom();
            int chars = random.Next(min_chars, max_chars + 1);

            if (chars_override != -1)
            {
                chars = chars_override;
            }

            for (int i = 0; i < chars; i++)
            {
                Vector2 formation = new Vector2(-1, -1);

                for (int z = 0; z < 80; z++)
                {
                    bool found = false;

                    random = new CryptoRandom();
                    int x = random.Next(0, 3);
                    int y = random.Next(0, 3);

                    for (int j = 0; j < squad.Characters.Count; j++)
                    {
                        Character existing = squad.Characters[j];
                        if (existing.Formation.X == x &&
                            existing.Formation.Y == y)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        formation = new Vector2(x, y);
                        break;
                    }
                }

                if (formation.X == -1)
                {
                    continue;
                }

                Character character;

                if (i == 0) //First character in the squad
                {
                    if (first_squad) //First squad in the army
                    {
                        if (map_level == 20)
                        {
                            character = CharacterUtil.NewCharacter_Random(formation, true, 0);
                            character.Name = "King " + character.Name;

                            class_type_override = 0;
                        }
                        else
                        {
                            int gender = random.Next(0, 2);
                            character = CharacterUtil.NewCharacter_Random(formation, true, gender);

                            if (gender == 0)
                            {
                                if (map_level >= 18)
                                {
                                    character.Name = "Duke " + character.Name;
                                }
                                else if (map_level >= 14)
                                {
                                    character.Name = "Count " + character.Name;
                                }
                                else if (map_level >= 10)
                                {
                                    character.Name = "Baron " + character.Name;
                                }
                                else
                                {
                                    character.Name = "Lord " + character.Name;
                                }
                            }
                            else
                            {
                                if (map_level >= 18)
                                {
                                    character.Name = "Duchess " + character.Name;
                                }
                                else if (map_level >= 14)
                                {
                                    character.Name = "Countess " + character.Name;
                                }
                                else if (map_level >= 10)
                                {
                                    character.Name = "Baroness " + character.Name;
                                }
                                else
                                {
                                    character.Name = "Lady " + character.Name;
                                }
                            }
                        }
                    }
                    else
                    {
                        character = CharacterUtil.NewCharacter_Random(formation, true, random.Next(0, 2));
                    }

                    squad.Leader_ID = character.ID;
                    squad.Name = character.Name;
                }
                else
                {
                    character = CharacterUtil.NewCharacter_Random(formation, true, random.Next(0, 2));
                }

                //Max is 1 at Map Level 1
                //Max is 3 at Map Level 5
                //Max is 6 at Map Level 10
                //Max is 8 at Map Level 15
                //Max is 10 at Map Level 18+
                int max_tier = (int)Math.Ceiling(map_level / 2f) + 1;
                if (max_tier_override != -1)
                {
                    max_tier = max_tier_override;
                }
                if (max_tier > 10)
                {
                    max_tier = 10;
                }
                else if (max_tier < 1)
                {
                    max_tier = 1;
                }

                //Min is 1 at Map Level 1
                //Min is 2 at Map Level 5
                //Min is 4 at Map Level 10
                //Min is 6 at Map Level 15
                //Min is 8 at Map Level 20
                int min_tier = (int)Math.Ceiling(map_level / 2.5f);
                if (min_tier_override != -1)
                {
                    min_tier = min_tier_override;
                }
                if (min_tier < 1)
                {
                    min_tier = 1;
                }

                if (first_squad)
                {
                    //Set min to max tier if first/boss squad 
                    min_tier = max_tier;
                }

                random = new CryptoRandom();
                int class_type = random.Next(0, 4);

                if (class_type_override != -1)
                {
                    class_type = class_type_override;
                }

                Item armor = null;
                Item helm = null;
                Item shield = null;
                Item weapon = null;

                if (class_type == 0)
                {
                    #region Warrior Gear

                    random = new CryptoRandom();
                    int armor_tier = random.Next(min_tier, max_tier + 1);
                    switch (armor_tier)
                    {
                        case 1:
                            armor = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 2:
                            armor = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 3:
                            armor = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 4:
                            armor = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 5:
                            armor = InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Armor");
                            break;

                        case 6:
                            armor = InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Armor");
                            break;

                        case 7:
                            armor = InventoryUtil.AddItem(character.Inventory, "Iron", "Platemail", "Armor");
                            break;

                        case 8:
                            armor = InventoryUtil.AddItem(character.Inventory, "Copper", "Platemail", "Armor");
                            break;

                        case 9:
                            armor = InventoryUtil.AddItem(character.Inventory, "Bronze", "Platemail", "Armor");
                            break;

                        case 10:
                            armor = InventoryUtil.AddItem(character.Inventory, "Steel", "Platemail", "Armor");
                            break;
                    }
                    InventoryUtil.EquipItem(character, armor);

                    random = new CryptoRandom();
                    int helm_chance = random.Next(min_tier, max_tier + 1);
                    if (helm_chance >= (int)Math.Ceiling((double)(min_tier + max_tier) / 2))
                    {
                        random = new CryptoRandom();
                        int helm_tier = random.Next(min_tier, max_tier + 1);
                        switch (helm_tier)
                        {
                            case 1:
                                helm = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Helm");
                                break;

                            case 2:
                                helm = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 3:
                                helm = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 4:
                                helm = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 5:
                                helm = InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Helm");
                                break;

                            case 6:
                                helm = InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Helm");
                                break;

                            case 7:
                                helm = InventoryUtil.AddItem(character.Inventory, "Iron", "Platemail", "Helm");
                                break;

                            case 8:
                                helm = InventoryUtil.AddItem(character.Inventory, "Copper", "Platemail", "Helm");
                                break;

                            case 9:
                                helm = InventoryUtil.AddItem(character.Inventory, "Bronze", "Platemail", "Helm");
                                break;

                            case 10:
                                helm = InventoryUtil.AddItem(character.Inventory, "Steel", "Platemail", "Helm");
                                break;
                        }
                        InventoryUtil.EquipItem(character, helm);
                    }

                    string weapon_type = "";

                    random = new CryptoRandom();
                    int weapon_choice = random.Next(0, 3);
                    switch (weapon_choice)
                    {
                        case 0:
                            weapon_type = "Sword";
                            break;

                        case 1:
                            weapon_type = "Mace";
                            break;

                        case 2:
                            weapon_type = "Axe";
                            break;
                    }

                    random = new CryptoRandom();
                    int weapon_tier = random.Next(min_tier, max_tier + 1);
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Iron", weapon_type, "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Copper", weapon_type, "Weapon");
                            break;
                        
                        case 7:
                        case 8:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Bronze", weapon_type, "Weapon");
                            break;

                        case 9:
                        case 10:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Steel", weapon_type, "Weapon");
                            break;
                    }
                    InventoryUtil.EquipItem(character, weapon);

                    if (weapon_type != "Axe")
                    {
                        random = new CryptoRandom();
                        int shield_chance = random.Next(min_tier, max_tier + 1);
                        if (shield_chance >= (int)Math.Ceiling((double)(min_tier + max_tier) / 2))
                        {
                            random = new CryptoRandom();
                            int shield_tier = random.Next(min_tier, max_tier + 1);
                            switch (shield_tier)
                            {
                                case 1:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Wood", "Round", "Shield");
                                    break;

                                case 2:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Iron", "Round", "Shield");
                                    break;

                                case 3:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Copper", "Round", "Shield");
                                    break;

                                case 4:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Bronze", "Round", "Shield");
                                    break;

                                case 5:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Steel", "Round", "Shield");
                                    break;

                                case 6:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Wood", "Kite", "Shield");
                                    break;

                                case 7:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Iron", "Kite", "Shield");
                                    break;

                                case 8:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Copper", "Kite", "Shield");
                                    break;

                                case 9:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Bronze", "Kite", "Shield");
                                    break;

                                case 10:
                                    shield = InventoryUtil.AddItem(character.Inventory, "Steel", "Kite", "Shield");
                                    break;
                            }
                            InventoryUtil.EquipItem(character, shield);
                        }
                    }

                    #endregion
                }
                else if (class_type == 1)
                {
                    #region Ranger Gear

                    random = new CryptoRandom();
                    int armor_tier = random.Next(min_tier, max_tier + 1);
                    switch (armor_tier)
                    {
                        case 1:
                            armor = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 2:
                            armor = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 3:
                            armor = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 4:
                            armor = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 5:
                            armor = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 6:
                            armor = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 7:
                            armor = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 8:
                            armor = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 9:
                            armor = InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Armor");
                            break;

                        case 10:
                            armor = InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Armor");
                            break;
                    }
                    InventoryUtil.EquipItem(character, armor);

                    random = new CryptoRandom();
                    int helm_chance = random.Next(min_tier, max_tier + 1);
                    if (helm_chance >= (int)Math.Ceiling((double)(min_tier + max_tier) / 2))
                    {
                        random = new CryptoRandom();
                        int helm_tier = random.Next(min_tier, max_tier + 1);
                        switch (helm_tier)
                        {
                            case 1:
                                //No helm
                                break;

                            case 2:
                                helm = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Helm");
                                break;

                            case 3:
                                helm = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 4:
                                helm = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 5:
                                helm = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 6:
                                helm = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 7:
                                helm = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 8:
                                helm = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 9:
                                helm = InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Helm");
                                break;

                            case 10:
                                helm = InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Helm");
                                break;
                        }
                        InventoryUtil.EquipItem(character, helm);
                    }

                    random = new CryptoRandom();
                    int weapon_tier = random.Next(min_tier, max_tier + 1);
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Elm", "Bow", "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Cedar", "Bow", "Weapon");
                            break;
                        
                        case 7:
                        case 8:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Oak", "Bow", "Weapon");
                            break;

                        case 9:
                        case 10:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Ebony", "Bow", "Weapon");
                            break;
                    }
                    InventoryUtil.EquipItem(character, weapon);

                    #endregion
                }
                else if (class_type == 2)
                {
                    #region Mage Gear

                    armor = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                    InventoryUtil.EquipItem(character, armor);

                    random = new CryptoRandom();
                    int helm_chance = random.Next(min_tier, max_tier + 1);
                    if (helm_chance >= (int)Math.Ceiling((double)(min_tier + max_tier) / 2))
                    {
                        random = new CryptoRandom();
                        int helm_tier = random.Next(min_tier, max_tier + 1);
                        switch (helm_tier)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                //No helm
                                break;

                            case 6:
                            case 7:
                            case 8:
                            case 9:
                            case 10:
                                helm = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Helm");
                                InventoryUtil.EquipItem(character, helm);
                                break;
                        }
                    }

                    random = new CryptoRandom();
                    int weapon_tier = random.Next(min_tier, max_tier + 1);
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Apprentice", "Grimoire", "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Novice", "Grimoire", "Weapon");
                            break;

                        case 7:
                        case 8:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Expert", "Grimoire", "Weapon");
                            break;

                        case 9:
                        case 10:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Master", "Grimoire", "Weapon");
                            break;
                    }
                    InventoryUtil.EquipItem(character, weapon);

                    #endregion
                }
                else if (class_type == 3)
                {
                    #region Cleric

                    random = new CryptoRandom();
                    int armor_tier = random.Next(min_tier, max_tier + 1);
                    switch (armor_tier)
                    {
                        case 1:
                            armor = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 2:
                            armor = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 3:
                            armor = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 4:
                            armor = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 5:
                            armor = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 6:
                            armor = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 7:
                            armor = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 8:
                            armor = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 9:
                            armor = InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Armor");
                            break;

                        case 10:
                            armor = InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Armor");
                            break;
                    }
                    InventoryUtil.EquipItem(character, armor);

                    random = new CryptoRandom();
                    int helm_chance = random.Next(min_tier, max_tier + 1);
                    if (helm_chance >= (int)Math.Ceiling((double)(min_tier + max_tier) / 2))
                    {
                        random = new CryptoRandom();
                        int helm_tier = random.Next(min_tier, max_tier + 1);
                        switch (helm_tier)
                        {
                            case 1:
                                //No helm
                                break;

                            case 2:
                                helm = InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Helm");
                                break;

                            case 3:
                                helm = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 4:
                                helm = InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 5:
                                helm = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 6:
                                helm = InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 7:
                                helm = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 8:
                                helm = InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 9:
                                helm = InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Helm");
                                break;

                            case 10:
                                helm = InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Helm");
                                break;
                        }
                        InventoryUtil.EquipItem(character, helm);
                    }

                    random = new CryptoRandom();
                    int weapon_tier = random.Next(min_tier, max_tier + 1);
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Iron", "Mace", "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Copper", "Mace", "Weapon");
                            break;

                        case 7:
                        case 8:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Bronze", "Mace", "Weapon");
                            break;

                        case 9:
                        case 10:
                            weapon = InventoryUtil.AddItem(character.Inventory, "Steel", "Mace", "Weapon");
                            break;
                    }
                    InventoryUtil.EquipItem(character, weapon);

                    random = new CryptoRandom();
                    int shield_tier = random.Next(min_tier, max_tier + 1);
                    switch (shield_tier)
                    {
                        case 1:
                            shield = InventoryUtil.AddItem(character.Inventory, "Wood", "Round", "Shield");
                            break;

                        case 2:
                            shield = InventoryUtil.AddItem(character.Inventory, "Iron", "Round", "Shield");
                            break;

                        case 3:
                            shield = InventoryUtil.AddItem(character.Inventory, "Copper", "Round", "Shield");
                            break;

                        case 4:
                            shield = InventoryUtil.AddItem(character.Inventory, "Bronze", "Round", "Shield");
                            break;

                        case 5:
                            shield = InventoryUtil.AddItem(character.Inventory, "Steel", "Round", "Shield");
                            break;

                        case 6:
                            shield = InventoryUtil.AddItem(character.Inventory, "Wood", "Kite", "Shield");
                            break;

                        case 7:
                            shield = InventoryUtil.AddItem(character.Inventory, "Iron", "Kite", "Shield");
                            break;

                        case 8:
                            shield = InventoryUtil.AddItem(character.Inventory, "Copper", "Kite", "Shield");
                            break;

                        case 9:
                            shield = InventoryUtil.AddItem(character.Inventory, "Bronze", "Kite", "Shield");
                            break;

                        case 10:
                            shield = InventoryUtil.AddItem(character.Inventory, "Steel", "Kite", "Shield");
                            break;
                    }
                    InventoryUtil.EquipItem(character, shield);

                    #endregion
                }

                if (class_type == 0)
                {
                    #region Warrior Runes

                    //Max of 1 rune at Map Level 10
                    //Max of 2 runes at Map Level 15+
                    random = new CryptoRandom();
                    int runes_amount = (int)Math.Floor((double)random.Next(min_tier, max_tier + 1) / 4);

                    int leftover = InventoryUtil.AddRunes(armor, runes_amount);

                    if (helm != null)
                    {
                        leftover = InventoryUtil.AddRunes(helm, leftover);
                    }

                    if (shield != null)
                    {
                        leftover = InventoryUtil.AddRunes(shield, leftover);
                    }

                    InventoryUtil.AddRunes(weapon, leftover);

                    #endregion
                }
                else if (class_type == 1)
                {
                    #region Ranger Runes

                    //Max of 1 rune at Map Level 5
                    //Max of 2 runes at Map Level 10
                    //Max of 3 runes at Map Level 15+
                    random = new CryptoRandom();
                    int runes_amount = (int)Math.Floor((double)random.Next(min_tier, max_tier + 1) / 3);
                    int leftover = InventoryUtil.AddRunes(armor, runes_amount);

                    if (helm != null)
                    {
                        leftover = InventoryUtil.AddRunes(helm, leftover);
                    }

                    InventoryUtil.AddRunes(weapon, leftover);

                    #endregion
                }
                else if (class_type == 2)
                {
                    #region Mage Runes

                    //Max of 1 rune at Map Level 5
                    //Max of 2 runes at Map Level 10
                    //Max of 4 runes at Map Level 15
                    //Max of 5 runes at Map Level 18+
                    random = new CryptoRandom();
                    int runes_amount = random.Next(min_tier, max_tier + 1) / 2;

                    int leftover = InventoryUtil.AddRunes(armor, runes_amount);

                    if (helm != null)
                    {
                        leftover = InventoryUtil.AddRunes(helm, leftover);
                    }

                    //Always at least 1 offense on weapon
                    InventoryUtil.AddRune_Elemental(weapon);

                    InventoryUtil.AddRunes(weapon, leftover);

                    #endregion
                }
                else if (class_type == 3)
                {
                    #region Cleric Runes

                    //Max of 1 rune at Map Level 5
                    //Max of 2 runes at Map Level 10
                    //Max of 4 runes at Map Level 15
                    //Max of 5 runes at Map Level 18+
                    random = new CryptoRandom();
                    int runes_amount = random.Next(min_tier, max_tier + 1) / 2;

                    int leftover = InventoryUtil.AddRunes(armor, runes_amount);

                    if (helm != null)
                    {
                        leftover = InventoryUtil.AddRunes(helm, leftover);
                    }

                    if (shield != null)
                    {
                        leftover = InventoryUtil.AddRunes(shield, leftover);
                    }

                    InventoryUtil.AddRunes(weapon, leftover);

                    #endregion
                }

                random = new CryptoRandom();
                int level = random.Next(min_tier - 1, max_tier + 1);
                for (int l = 1; l <= level; l++)
                {
                    if (l > 1)
                    {
                        CharacterUtil.Increase_Level(character);
                    }
                }

                squad.AddCharacter(character);
            }
        }

        public static Squad Gen_Academy()
        {
            Squad squad = new Squad();

            int x = 0;
            int y = 0;

            for (int i = 0; i < 20; i++)
            {
                CryptoRandom random = new CryptoRandom();
                Character character = CharacterUtil.NewCharacter_Random(new Vector2(x, y), false, random.Next(0, 2));

                InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);

                squad.AddCharacter(character);

                x++;
                if (x >= 10)
                {
                    x = 0;
                    y++;
                }
            }

            return squad;
        }

        public static Squad Get_TargetSquad(long target_id)
        {
            foreach (Army army in CharacterManager.Armies)
            {
                foreach (Squad squad in army.Squads)
                {
                    if (squad.ID == target_id)
                    {
                        return squad;
                    }
                }
            }

            return null;
        }

        public static Squad Get_Squad(Map map, Army army, Location location)
        {
            Layer ground = map.GetLayer("Ground");
            Tile tile = ground.GetTile(new Vector2(location.X, location.Y));

            foreach (Squad squad in army.Squads)
            {
                if (squad.Location.X == tile.Location.X &&
                    squad.Location.Y == tile.Location.Y)
                {
                    return squad;
                }
            }

            return null;
        }

        public static int Get_CharacterIndex(Character character)
        {
            return (int)((character.Formation.Y * 10) + character.Formation.X);
        }

        public static Character Get_LastCharacter(Squad squad)
        {
            if (squad != null)
            {
                if (squad.Characters.Count > 0)
                {
                    Character last = squad.Characters[0];
                    int last_index = Get_CharacterIndex(last);

                    foreach (Character existing in squad.Characters)
                    {
                        int current_index = Get_CharacterIndex(existing);
                        if (current_index > last_index)
                        {
                            last = existing;
                            last_index = current_index;
                        }
                    }

                    return last;
                }
            }

            return null;
        }

        public static void SetPath(Map map, Squad squad, Tile destination)
        {
            if (map != null &&
                squad != null &&
                squad.Region != null &&
                destination != null)
            {
                Layer ground = map.GetLayer("Ground");
                Layer roads = map.GetLayer("Roads");

                ALocation remaining = null;

                if (squad.Path.Any())
                {
                    remaining = new ALocation(squad.Path[0].X, squad.Path[0].Y);
                }

                squad.Path = Handler.Pathing.Get_Path(ground, roads, squad, destination, ground.Columns * ground.Rows, false);
                if (squad.Path.Any())
                {
                    squad.Path.Reverse();

                    if (remaining != null &&
                        squad.Moved > 0)
                    {
                        if (remaining.X == squad.Path[1].X &&
                            remaining.Y == squad.Path[1].Y)
                        {
                            //Exclude starting location if already moving to next location
                            squad.Path.RemoveAt(0);
                        }
                        else
                        {
                            //Reverse direction towards new starting location
                            squad.Location = new Location(remaining.X, remaining.Y, 0);
                            squad.Moved = squad.Move_TotalDistance - squad.Moved;
                        }
                    }

                    ALocation start = squad.Path[0];
                    Tile start_tile = ground.GetTile(new Vector2(start.X, start.Y));

                    if (squad.Region.X == start_tile.Region.X &&
                        squad.Region.Y == start_tile.Region.Y)
                    {
                        //Exclude starting location if already there
                        squad.Path.RemoveAt(0);
                        squad.Moved = 0;
                    }

                    long target_id = squad.GetLeader().Target_ID;
                    if (target_id != 0)
                    {
                        bool found = false;
                        foreach (Army army in CharacterManager.Armies)
                        {
                            foreach (Squad existing in army.Squads)
                            {
                                if (target_id == existing.ID)
                                {
                                    found = true;
                                    squad.Coordinates = existing.Location;
                                    break;
                                }
                            }

                            if (found)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        squad.Coordinates = destination.Location;
                    }
                }
                else if (remaining != null &&
                         squad.Moved > 0)
                {
                    //Reverse direction back towards current location if already moved away from it
                    squad.Path.Insert(0, new ALocation((int)squad.Location.X, (int)squad.Location.Y));
                    squad.Location = new Location(remaining.X, remaining.Y, 0);
                    squad.Moved = squad.Move_TotalDistance - squad.Moved;
                }
            }
        }

        public static void SetPath(Menu menu, Map map, Tile destination)
        {
            Layer ground = map.GetLayer("Ground");
            Layer roads = map.GetLayer("Roads");

            Layer pathing = map.GetLayer("Pathing");
            pathing.Visible = false;
            pathing.Tiles.Clear();

            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(Handler.Selected_Token);
            if (squad != null)
            {
                ALocation remaining = null;

                if (squad.Path.Any())
                {
                    remaining = new ALocation(squad.Path[0].X, squad.Path[0].Y);
                }

                squad.Path = Handler.Pathing.Get_Path(ground, roads, squad, destination, ground.Columns * ground.Rows, false);
                if (squad.Path != null)
                {
                    if (squad.Path.Any())
                    {
                        squad.Path.Reverse();

                        if (remaining != null)
                        {
                            if (remaining.X == squad.Path[1].X &&
                                remaining.Y == squad.Path[1].Y &&
                                squad.Moved > 0)
                            {
                                //Exclude starting location if already moving to next location
                                squad.Path.RemoveAt(0);
                            }
                            else
                            {
                                //Reverse direction towards new starting location
                                squad.Location = new Location(remaining.X, remaining.Y, 0);
                                squad.Moved = squad.Move_TotalDistance - squad.Moved;
                            }
                        }

                        ALocation start = squad.Path[0];
                        Tile start_tile = ground.GetTile(new Vector2(start.X, start.Y));

                        if (squad.Region.X == start_tile.Region.X &&
                            squad.Region.Y == start_tile.Region.Y)
                        {
                            //Exclude starting location if already there
                            squad.Path.RemoveAt(0);
                            squad.Moved = 0;
                        }

                        bool enemy_targeted = false;
                        Army enemy_army = CharacterManager.GetArmy("Enemy");
                        foreach (Squad enemy_squad in enemy_army.Squads)
                        {
                            if ((destination.Location.X == enemy_squad.Location.X &&
                                 destination.Location.Y == enemy_squad.Location.Y) ||
                                (enemy_squad.Moving &&
                                 destination.Location.X == enemy_squad.Destination.X &&
                                 destination.Location.Y == enemy_squad.Destination.Y))
                            {
                                enemy_targeted = true;
                                squad.GetLeader().Target_ID = enemy_squad.ID;
                                squad.Coordinates = enemy_squad.Location;
                                break;
                            }
                        }

                        if (!enemy_targeted)
                        {
                            squad.GetLeader().Target_ID = 0;
                            squad.Coordinates = destination.Location;
                        }
                    }
                    else if (remaining != null &&
                             squad.Moved > 0)
                    {
                        //Reverse direction back towards current location if already moved away from it
                        squad.Path.Insert(0, new ALocation((int)squad.Location.X, (int)squad.Location.Y));
                        squad.Location = new Location(remaining.X, remaining.Y, 0);
                        squad.Moved = squad.Move_TotalDistance - squad.Moved;
                    }
                }

                Handler.Selected_Token = -1;

                menu.GetPicture("Select").Visible = false;
                menu.GetPicture("Highlight").Visible = false;
                menu.GetPicture("Highlight").DrawColor = new Color(255, 255, 255, 255);
            }
        }

        public static void DeployArmy(Army army, Map map, string type)
        {
            Tile armyBase = WorldUtil.Get_Base(map, type);
            if (armyBase != null)
            {
                foreach (Squad squad in army.Squads)
                {
                    squad.Location = new Location(armyBase.Location.X, armyBase.Location.Y, armyBase.Location.Z);
                    squad.Region = new Region(armyBase.Region.X, armyBase.Region.Y, armyBase.Region.Width, armyBase.Region.Height);
                    squad.Visible = true;
                    squad.Active = true;
                }
            }
        }

        public static void DeploySquad(long squadID)
        {
            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(squadID);

            World world = SceneManager.GetScene("Localmap").World;
            Map map = world.Maps[Handler.Level];

            Tile ally_base = WorldUtil.Get_Base(map, "Ally");
            if (ally_base != null)
            {
                squad.Location = new Location(ally_base.Location.X, ally_base.Location.Y, ally_base.Location.Z);
                squad.Region = new Region(ally_base.Region.X, ally_base.Region.Y, ally_base.Region.Width, ally_base.Region.Height);
                squad.Visible = true;
                squad.Active = true;
            }

            Menu armyMenu = MenuManager.GetMenu("Army");
            armyMenu.GetButton("Deploy").Enabled = false;
        }

        public static void Undeploy()
        {
            Squad squad = Handler.Dialogue_Character2.Squad;
            if (squad != null)
            {
                squad.Visible = false;
                squad.Active = false;
            }
        }

        #endregion
    }
}
