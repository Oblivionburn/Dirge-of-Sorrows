using System.Linq;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using System;

namespace DoS1.Util
{
    public static class ArmyUtil
    {
        #region Variables



        #endregion

        #region Constructors

        

        #endregion

        #region Methods

        public static void Gen_StartingArmy()
        {
            CharacterManager.Armies.Clear();

            //Player army
            Army army = new Army
            {
                ID = Handler.GetID(),
                Name = "Player"
            };
            CharacterManager.Armies.Add(army);

            Squad player_squad = NewSquad("Player");
            army.Squads.Add(player_squad);

            //Reserves
            Army reserves = new Army
            {
                ID = Handler.GetID(),
                Name = "Reserves"
            };
            CharacterManager.Armies.Add(reserves);

            Squad reserves_squad = NewSquad("Reserves");
            reserves.Squads.Add(reserves_squad);

            //Academy
            Army academy = new Army
            {
                ID = Handler.GetID(),
                Name = "Academy"
            };
            CharacterManager.Armies.Add(academy);

            Squad academy_squad = NewSquad("Academy");
            academy.Squads.Add(academy_squad);

            //Enemy
            Army enemies = new Army
            {
                ID = Handler.GetID(),
                Name = "Enemy"
            };
            CharacterManager.Armies.Add(enemies);

            Squad enemy_squad = NewSquad("Enemy");
            Gen_EnemySquad(enemy_squad, 1);
            enemies.Squads.Add(enemy_squad);
        }

        public static Squad NewSquad(string type)
        {
            Squad squad = new Squad
            {
                ID = Handler.GetID(),
                Type = type
            };

            if (type == "Player")
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

        public static Character NewCharacter(string name, Vector2 formation, string hairStyle, string hairColor, string headStyle, string eyeColor, string skinColor, bool enemy)
        {
            Character character = new Character();
            character.ID = Handler.GetID();
            character.Name = name;
            character.Formation = new Vector2(formation.X, formation.Y);

            if (enemy)
            {
                character.Direction = Direction.Right;
            }
            else
            {
                character.Direction = Direction.Left;
            }

            character.Texture = AssetManager.Textures[character.Direction.ToString() + "_Body_" + skinColor + "_Idle"];
            character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height);

            //Add head
            Item item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Head";
            item.Type = "Head";
            item.Equipped = true;
            item.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + skinColor + "_" + headStyle];
            item.Image = character.Image;
            item.Visible = true;
            character.Inventory.Items.Add(item);

            //Add eyes
            item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Eyes";
            item.Type = "Eyes";
            item.Equipped = true;
            item.Texture = AssetManager.Textures[character.Direction.ToString() + "_Eye"];
            item.Image = character.Image;
            item.DrawColor = Handler.EyeColors[eyeColor];
            item.Visible = true;
            character.Inventory.Items.Add(item);

            //Add hair
            if (hairStyle != "Bald")
            {
                item = new Item();
                item.ID = Handler.GetID();
                item.Name = "Hair";
                item.Type = "Hair";
                item.Equipped = true;
                item.DrawColor = Handler.HairColors[hairColor];
                item.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + hairStyle];
                item.Image = character.Image;
                item.Visible = true;
                character.Inventory.Items.Add(item);
            }

            return character;
        }

        public static Character NewCharacter_Random(string name, Vector2 formation, bool enemy)
        {
            CryptoRandom random = new CryptoRandom();

            Character character = new Character();
            character.ID = Handler.GetID();
            character.Name = name;
            character.Formation = new Vector2(formation.X, formation.Y);

            if (enemy)
            {
                character.Direction = Direction.Right;
            }
            else
            {
                character.Direction = Direction.Left;
            }

            character.Texture = AssetManager.Textures[character.Direction.ToString() + "_Body_" + Handler.SkinTones[random.Next(0, Handler.SkinTones.Length)] + "_Idle"];
            character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height);

            //Add head
            Item item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Head";
            item.Type = "Head";
            item.Equipped = true;
            item.Texture = AssetManager.Textures[character.Direction.ToString() + 
                Handler.SkinTones[random.Next(0, Handler.SkinTones.Length)] + "_" + 
                Handler.HeadStyles[random.Next(0, Handler.HeadStyles.Length)]];
            item.Image = character.Image;
            item.Visible = true;
            character.Inventory.Items.Add(item);

            //Add eyes
            item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Eyes";
            item.Type = "Eyes";
            item.Equipped = true;
            item.Texture = AssetManager.Textures[character.Direction.ToString() + "_Eye"];
            item.Image = character.Image;
            item.DrawColor = Handler.EyeColors.ElementAt(random.Next(0, Handler.EyeColors.Count)).Value;
            item.Visible = true;
            character.Inventory.Items.Add(item);

            string hairStyle = Handler.HairStyles[random.Next(0, Handler.HairStyles.Length)];
            if (hairStyle != "Bald")
            {
                //Add hair
                item = new Item();
                item.ID = Handler.GetID();
                item.Name = "Hair";
                item.Type = "Hair";
                item.Equipped = true;
                item.DrawColor = Handler.HairColors.ElementAt(random.Next(0, 6)).Value;
                item.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + hairStyle];
                item.Image = character.Image;
                item.Visible = true;
                character.Inventory.Items.Add(item);
            }

            return character;
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
                name += " " + CharacterManager.LastNames[random.Next(0, CharacterManager.LastNames.Count)];

                Character character = NewCharacter_Random(name, formation, true);
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
                int class_type = 0;
                //int class_type = random.Next(0, 3);
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
                            InventoryUtil.AddItem(character.Inventory, "Copper", weapon_type, "Weapon");
                            break;

                        case 6:
                        case 7:
                        case 8:
                            InventoryUtil.AddItem(character.Inventory, "Bronze", weapon_type, "Weapon");
                            break;

                        case 9:
                        case 10:
                            InventoryUtil.AddItem(character.Inventory, "Steel", weapon_type, "Weapon");
                            break;
                    }

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
                        }
                    }

                    #endregion
                }
                else if (class_type == 1)
                {
                    #region Ranger Gear



                    #endregion
                }
                else if (class_type == 2)
                {
                    #region Mage Gear



                    #endregion
                }

                squad.Characters.Add(character);
            }
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

        public static void SetPath(Menu menu, Map map, Tile destination)
        {
            Layer ground = map.GetLayer("Ground");
            Layer roads = map.GetLayer("Roads");

            Layer pathing = map.GetLayer("Pathing");
            pathing.Visible = false;
            pathing.Tiles.Clear();

            Army army = CharacterManager.GetArmy("Player");
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
                        //Finish movement to current destination
                        squad.Path.Insert(0, remaining);
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

                    squad.Coordinates = new Vector2(destination.Location.X, destination.Location.Y);
                }
                
                Handler.Selected_Token = -1;

                menu.GetPicture("Select").Visible = false;
                menu.GetPicture("Highlight").Visible = false;
                menu.GetPicture("Highlight").DrawColor = new Color(255, 255, 255, 255);

                pathing.Visible = false;
            }
        }

        public static void MoveSquads()
        {
            Scene localmap = SceneManager.GetScene("Localmap");
            if (localmap.World.Maps.Any())
            {
                Map map = localmap.World.Maps[0];
                Layer ground = map.GetLayer("Ground");

                foreach (Army army in CharacterManager.Armies)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        if (squad.Path.Any())
                        {
                            ALocation path = squad.Path[0];

                            Vector2 path_location = new Vector2(path.X, path.Y);
                            Vector2 squad_location = new Vector2(squad.Location.X, squad.Location.Y);

                            Tile destination = ground.GetTile(path_location);

                            squad.Move_TotalDistance = Main.Game.TileSize.X;
                            squad.Moving = true;
                            squad.Speed = WorldUtil.Get_TerrainSpeed(map, squad_location);
                            squad.Destination = new Vector3(path_location.X, path_location.Y, 0);
                            squad.Update();

                            if (squad.Location.X == squad.Destination.X &&
                                squad.Location.Y == squad.Destination.Y)
                            {
                                squad.Region = new Region(destination.Region.X, destination.Region.Y, destination.Region.Width, destination.Region.Height);
                                squad.Path.Remove(path);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
