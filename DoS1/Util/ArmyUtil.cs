using System;
using System.Linq;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Menus;

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
            army.Squads.Add(ally_squad);

            //Reserves
            Army reserves = new Army
            {
                ID = Handler.GetID(),
                Name = "Reserves"
            };
            CharacterManager.Armies.Add(reserves);

            Squad reserves_squad = NewSquad("Reserves");
            reserves.Squads.Add(reserves_squad);

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
                Type = type
            };

            if (type == "Ally")
            {
                squad.Texture = AssetManager.Textures["Token_Ally"];
            }
            else if (type == "Enemy")
            {
                squad.Texture = AssetManager.Textures["Token_Enemy"];
            }

            if (squad.Texture != null)
            {
                squad.Image = new Rectangle(0, 0, squad.Texture.Width, squad.Texture.Height);
            }

            return squad;
        }

        public static void Gen_EnemySquads(Army army, int depth)
        {
            army.Squads.Clear();

            int squads = 1;
            for (int i = 0; i < depth; i++)
            {
                if (i == 1)
                {
                    squads = 2;
                }
                else if (i % 2 == 0)
                {
                    squads++;
                }
            }

            for (int i = 1; i <= squads; i++)
            {
                Squad enemy_squad = NewSquad("Enemy");
                if (i == 1)
                {
                    enemy_squad.Assignment = "Guard Base";
                }
                else if (i == 2)
                {
                    enemy_squad.Assignment = "Attack Base";
                }
                else
                {
                    CryptoRandom random = new CryptoRandom();
                    int task = random.Next(0, 4);
                    if (task == 0)
                    {
                        enemy_squad.Assignment = "Attack Base";
                    }
                    else if (task == 1)
                    {
                        enemy_squad.Assignment = "Capture Nearest Town";
                    }
                    else if (task == 2)
                    {
                        enemy_squad.Assignment = "Guard Nearest Town";
                    }
                    else if (task == 3)
                    {
                        enemy_squad.Assignment = "Attack Nearest Squad";
                    }
                }

                Gen_EnemySquad(enemy_squad, depth + 1);
                army.Squads.Add(enemy_squad);
            }
        }

        public static void Gen_EnemySquad(Squad squad, int depth)
        {
            int min_chars = (int)Math.Ceiling((double)depth / 5);
            int max_chars = (int)Math.Ceiling((double)depth / 2);
            if (max_chars > 5)
            {
                max_chars = 5;
            }

            CryptoRandom random = new CryptoRandom();
            int chars = random.Next(min_chars, max_chars + 1);

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

                string name = "";
                
                random = new CryptoRandom();
                int gender = random.Next(0, 2);
                if (gender == 0)
                {
                    name = CharacterManager.FirstNames_Male[random.Next(0, CharacterManager.FirstNames_Male.Count)];
                }
                else
                {
                    name = CharacterManager.FirstNames_Female[random.Next(0, CharacterManager.FirstNames_Female.Count)];
                }
                //name += " " + CharacterManager.LastNames[random.Next(0, CharacterManager.LastNames.Count)];

                Character character = CharacterUtil.NewCharacter_Random(name, formation, true);
                if (gender == 0)
                {
                    character.Gender = "Male";
                }
                else
                {
                    character.Gender = "Female";
                }

                random = new CryptoRandom();
                int min_tier = (int)Math.Ceiling(depth / 2.5);
                int max_tier = (int)Math.Ceiling(depth / 1.5);
                if (max_tier > 10)
                {
                    max_tier = 10;
                }

                random = new CryptoRandom();
                int class_type = random.Next(0, 2);
                if (class_type == 0)
                {
                    #region Warrior Gear

                    random = new CryptoRandom();
                    int armor_tier = random.Next(min_tier, max_tier + 1);
                    switch (armor_tier)
                    {
                        case 1:
                            InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 2:
                            InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 3:
                            InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 4:
                            InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 5:
                            InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Armor");
                            break;

                        case 6:
                            InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Armor");
                            break;

                        case 7:
                            InventoryUtil.AddItem(character.Inventory, "Iron", "Platemail", "Armor");
                            break;

                        case 8:
                            InventoryUtil.AddItem(character.Inventory, "Copper", "Platemail", "Armor");
                            break;

                        case 9:
                            InventoryUtil.AddItem(character.Inventory, "Bronze", "Platemail", "Armor");
                            break;

                        case 10:
                            InventoryUtil.AddItem(character.Inventory, "Steel", "Platemail", "Armor");
                            break;
                    }
                    InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);

                    random = new CryptoRandom();
                    int helm_chance = random.Next(min_tier, max_tier + 1);
                    if (helm_chance >= (int)Math.Ceiling((double)(min_tier + max_tier) / 2))
                    {
                        random = new CryptoRandom();
                        int helm_tier = random.Next(min_tier, max_tier + 1);
                        switch (helm_tier)
                        {
                            case 1:
                                InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Helm");
                                break;

                            case 2:
                                InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 3:
                                InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 4:
                                InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 5:
                                InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Helm");
                                break;

                            case 6:
                                InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Helm");
                                break;

                            case 7:
                                InventoryUtil.AddItem(character.Inventory, "Iron", "Platemail", "Helm");
                                break;

                            case 8:
                                InventoryUtil.AddItem(character.Inventory, "Copper", "Platemail", "Helm");
                                break;

                            case 9:
                                InventoryUtil.AddItem(character.Inventory, "Bronze", "Platemail", "Helm");
                                break;

                            case 10:
                                InventoryUtil.AddItem(character.Inventory, "Steel", "Platemail", "Helm");
                                break;
                        }
                        InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);
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
                            InventoryUtil.AddItem(character.Inventory, "Iron", weapon_type, "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            InventoryUtil.AddItem(character.Inventory, "Copper", weapon_type, "Weapon");
                            break;
                        
                        case 7:
                        case 8:
                            InventoryUtil.AddItem(character.Inventory, "Bronze", weapon_type, "Weapon");
                            break;

                        case 9:
                        case 10:
                            InventoryUtil.AddItem(character.Inventory, "Steel", weapon_type, "Weapon");
                            break;
                    }
                    InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);

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
                                    InventoryUtil.AddItem(character.Inventory, "Wood", "Round", "Shield");
                                    break;

                                case 2:
                                    InventoryUtil.AddItem(character.Inventory, "Iron", "Round", "Shield");
                                    break;

                                case 3:
                                    InventoryUtil.AddItem(character.Inventory, "Copper", "Round", "Shield");
                                    break;

                                case 4:
                                    InventoryUtil.AddItem(character.Inventory, "Bronze", "Round", "Shield");
                                    break;

                                case 5:
                                    InventoryUtil.AddItem(character.Inventory, "Steel", "Round", "Shield");
                                    break;

                                case 6:
                                    InventoryUtil.AddItem(character.Inventory, "Wood", "Kite", "Shield");
                                    break;

                                case 7:
                                    InventoryUtil.AddItem(character.Inventory, "Iron", "Kite", "Shield");
                                    break;

                                case 8:
                                    InventoryUtil.AddItem(character.Inventory, "Copper", "Kite", "Shield");
                                    break;

                                case 9:
                                    InventoryUtil.AddItem(character.Inventory, "Bronze", "Kite", "Shield");
                                    break;

                                case 10:
                                    InventoryUtil.AddItem(character.Inventory, "Steel", "Kite", "Shield");
                                    break;
                            }
                            InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);
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
                            InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 2:
                            InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                            break;

                        case 3:
                            InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 4:
                            InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Armor");
                            break;

                        case 5:
                            InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 6:
                            InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Armor");
                            break;

                        case 7:
                            InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 8:
                            InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Armor");
                            break;

                        case 9:
                            InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Armor");
                            break;

                        case 10:
                            InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Armor");
                            break;
                    }
                    InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);

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
                                InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Helm");
                                break;

                            case 3:
                                InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 4:
                                InventoryUtil.AddItem(character.Inventory, "Leather", "Leather", "Helm");
                                break;

                            case 5:
                                InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 6:
                                InventoryUtil.AddItem(character.Inventory, "Iron", "Chainmail", "Helm");
                                break;

                            case 7:
                                InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 8:
                                InventoryUtil.AddItem(character.Inventory, "Copper", "Chainmail", "Helm");
                                break;

                            case 9:
                                InventoryUtil.AddItem(character.Inventory, "Bronze", "Chainmail", "Helm");
                                break;

                            case 10:
                                InventoryUtil.AddItem(character.Inventory, "Steel", "Chainmail", "Helm");
                                break;
                        }

                        if (helm_tier > 1)
                        {
                            InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);
                        }
                    }

                    random = new CryptoRandom();
                    int weapon_tier = random.Next(min_tier, max_tier + 1);
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            InventoryUtil.AddItem(character.Inventory, "Elm", "Bow", "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            InventoryUtil.AddItem(character.Inventory, "Cedar", "Bow", "Weapon");
                            break;
                        
                        case 7:
                        case 8:
                            InventoryUtil.AddItem(character.Inventory, "Oak", "Bow", "Weapon");
                            break;

                        case 9:
                        case 10:
                            InventoryUtil.AddItem(character.Inventory, "Ebony", "Bow", "Weapon");
                            break;
                    }
                    InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);

                    #endregion
                }
                else if (class_type == 2)
                {
                    #region Mage Gear



                    #endregion
                }

                squad.Characters.Add(character);

                if (i == 0)
                {
                    //Set Leader
                    squad.Leader_ID = character.ID;
                    squad.Name = character.Name;
                }
            }
        }

        public static Squad Gen_Academy()
        {
            Squad squad = new Squad();

            CryptoRandom random;
            int x = 0;
            int y = 0;

            for (int i = 0; i < 20; i++)
            {
                string name = "";

                random = new CryptoRandom();
                int gender = random.Next(0, 2);
                if (gender == 0)
                {
                    name = CharacterManager.FirstNames_Male[random.Next(0, CharacterManager.FirstNames_Male.Count)];
                }
                else
                {
                    name = CharacterManager.FirstNames_Female[random.Next(0, CharacterManager.FirstNames_Female.Count)];
                }
                //name += " " + CharacterManager.LastNames[random.Next(0, CharacterManager.LastNames.Count)];

                Character character = CharacterUtil.NewCharacter_Random(name, new Vector2(x, y), false);
                if (gender == 0)
                {
                    character.Gender = "Male";
                }
                else
                {
                    character.Gender = "Female";
                }

                InventoryUtil.AddItem(character.Inventory, "Cloth", "Cloth", "Armor");
                InventoryUtil.EquipItem(character, character.Inventory.Items[character.Inventory.Items.Count - 1]);

                squad.Characters.Add(character);

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

        public static Squad Get_Squad(Character leader)
        {
            foreach (Army army in CharacterManager.Armies)
            {
                foreach (Squad squad in army.Squads)
                {
                    Character squadLeader = squad.GetLeader();
                    if (squadLeader != null)
                    {
                        if (squadLeader.ID == leader.ID)
                        {
                            return squad;
                        }
                    }
                }
            }

            return null;
        }

        public static Squad Get_Squad(long character_id)
        {
            foreach (Army army in CharacterManager.Armies)
            {
                foreach (Squad squad in army.Squads)
                {
                    foreach (Character character in squad.Characters)
                    {
                        if (character.ID == character_id)
                        {
                            return squad;
                        }
                    }
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
                            (destination.Location.X == enemy_squad.Destination.X &&
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

                Handler.Selected_Token = -1;

                menu.GetPicture("Select").Visible = false;
                menu.GetPicture("Highlight").Visible = false;
                menu.GetPicture("Highlight").DrawColor = new Color(255, 255, 255, 255);

                pathing.Visible = false;
            }
        }

        #endregion
    }
}
