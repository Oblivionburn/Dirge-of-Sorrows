using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Inventories;
using OP_Engine.Spells;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class InventoryUtil
    {
        public static void GenAssets()
        {
            for (int i = 0; i < InventoryManager.Inventories.Count; i++)
            {
                Inventory existing = InventoryManager.Inventories[i];
                if (existing.Name != "Assets")
                {
                    InventoryManager.Inventories.Remove(existing);
                    i--;
                }
            }

            Inventory assets = new Inventory
            {
                ID = Handler.GetID(),
                Name = "Assets"
            };
            assets.Items.AddRange(GenArmors());
            assets.Items.AddRange(GenHelms());
            assets.Items.AddRange(GenShields());
            assets.Items.AddRange(GenAxes());
            assets.Items.AddRange(GenMaces());
            assets.Items.AddRange(GenSwords());
            assets.Items.AddRange(GenBows());
            assets.Items.AddRange(GenGrimoires());
            assets.Items.AddRange(GenRunes());
            InventoryManager.Inventories.Add(assets);

            Inventory inventory = new Inventory
            {
                ID = Handler.GetID(),
                Name = "Ally"
            };
            InventoryManager.Inventories.Add(inventory);
        }

        public static List<Item> GenArmors()
        {
            string type = "Armor";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            string category = "Cloth";
            properties.Add(NewProperty("Physical", "Defense", 5, 0));
            properties.Add(NewProperty("Rune", "Slots", 8, 0));
            properties.Add(NewProperty("EP", "Cost", 1, 0));
            Item item = NewItem(type, category, "Cloth", properties);
            item.Buy_Price = 50;
            items.Add(item);

            category = "Leather";
            properties[0].Value = 10;
            properties[1].Value = 4;
            properties[2].Value = 2;
            item = NewItem(type, category, "Leather", properties);
            item.Buy_Price = 100;
            items.Add(item);

            category = "Chainmail";
            properties[0].Value = 15;
            properties[1].Value = 2;
            properties[2].Value = 3;
            item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 150;
            items.Add(item);

            properties[0].Value = 20;
            properties[2].Value = 4;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 200;
            items.Add(item);

            properties[0].Value = 25;
            properties[2].Value = 5;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 250;
            items.Add(item);

            properties[0].Value = 30;
            properties[2].Value = 6;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 300;
            items.Add(item);

            category = "Platemail";
            properties[0].Value = 35;
            properties[1].Value = 0;
            properties[2].Value = 7;
            item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 350;
            items.Add(item);

            properties[0].Value = 40;
            properties[2].Value = 8;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 400;
            items.Add(item);

            properties[0].Value = 45;
            properties[2].Value = 9;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 450;
            items.Add(item);

            properties[0].Value = 50;
            properties[2].Value = 10;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 500;
            items.Add(item);

            return items;
        }

        public static List<Item> GenHelms()
        {
            string type = "Helm";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            string category = "Cloth";
            properties.Add(NewProperty("Physical", "Defense", 1, 0));
            properties.Add(NewProperty("Rune", "Slots", 8, 0));
            properties.Add(NewProperty("EP", "Cost", 1, 0));
            Item item = NewItem(type, category, "Cloth", properties);
            item.Buy_Price = 10;
            items.Add(item);

            category = "Leather";
            properties[0].Value = 2;
            properties[1].Value = 4;
            properties[2].Value = 1;
            item = NewItem(type, category, "Leather", properties);
            item.Buy_Price = 20;
            items.Add(item);

            category = "Chainmail";
            properties[0].Value = 3;
            properties[1].Value = 2;
            properties[2].Value = 2;
            item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 30;
            items.Add(item);

            properties[0].Value = 4;
            properties[2].Value = 2;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 40;
            items.Add(item);

            properties[0].Value = 5;
            properties[2].Value = 3;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 50;
            items.Add(item);

            properties[0].Value = 6;
            properties[2].Value = 3;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 60;
            items.Add(item);

            category = "Platemail";
            properties[0].Value = 7;
            properties[1].Value = 0;
            properties[2].Value = 4;
            item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 70;
            items.Add(item);

            properties[0].Value = 8;
            properties[2].Value = 4;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 80;
            items.Add(item);

            properties[0].Value = 9;
            properties[2].Value = 5;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 90;
            items.Add(item);

            properties[0].Value = 10;
            properties[2].Value = 5;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 100;
            items.Add(item);

            return items;
        }

        public static List<Item> GenShields()
        {
            string type = "Shield";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            string category = "Round";
            properties.Add(NewProperty("Physical", "Defense", 5, 0));
            properties.Add(NewProperty("EP", "Cost", 1, 0));
            Item item = NewItem(type, category, "Wood", properties);
            item.Buy_Price = 20;
            items.Add(item);

            properties[0].Value = 10;
            properties[1].Value = 1;
            item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 40;
            items.Add(item);

            properties[0].Value = 15;
            properties[1].Value = 2;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 60;
            items.Add(item);

            properties[0].Value = 20;
            properties[1].Value = 2;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 80;
            items.Add(item);

            properties[0].Value = 25;
            properties[1].Value = 3;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 100;
            items.Add(item);

            category = "Kite";
            List<Something> special_properties = new List<Something>()
            {
                NewProperty("Physical", "Defense", 30, 0),
                NewProperty("Rune", "Slots", 4, 0),
                NewProperty("EP", "Cost", 3, 0)
            };
            item = NewItem(type, category, "Wood", properties);
            item.Buy_Price = 120;
            item.Sell_Price = 120;
            items.Add(item);

            properties[0].Value = 35;
            properties[1].Value = 4;
            item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 140;
            item.Sell_Price = 140;
            items.Add(item);

            properties[0].Value = 40;
            properties[1].Value = 4;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 160;
            item.Sell_Price = 160;
            items.Add(item);

            properties[0].Value = 45;
            properties[1].Value = 5;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 180;
            item.Sell_Price = 180;
            items.Add(item);

            properties[0].Value = 50;
            properties[1].Value = 5;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 200;
            item.Sell_Price = 200;
            items.Add(item);

            return items;
        }

        public static List<Item> GenAxes()
        {
            string type = "Weapon";
            string category = "Axe";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 20, 0));
            properties.Add(NewProperty("EP", "Cost", 4, 0));
            Item item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 100;
            items.Add(item);

            properties[0].Value = 40;
            properties[1].Value = 6;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 200;
            items.Add(item);

            properties[0].Value = 60;
            properties[1].Value = 8;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 300;
            items.Add(item);

            properties[0].Value = 80;
            properties[1].Value = 10;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 400;
            items.Add(item);

            return items;
        }

        public static List<Item> GenMaces()
        {
            string type = "Weapon";
            string category = "Mace";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 25, 0));
            properties.Add(NewProperty("Rune", "Slots", 3, 0));
            properties.Add(NewProperty("EP", "Cost", 1, 0));
            Item item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 125;
            items.Add(item);

            properties[0].Value = 50;
            properties[1].Value = 3;
            properties[2].Value = 2;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 250;
            items.Add(item);

            properties[0].Value = 75;
            properties[1].Value = 3;
            properties[2].Value = 3;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 375;
            items.Add(item);

            properties[0].Value = 100;
            properties[1].Value = 3;
            properties[2].Value = 4;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 500;
            items.Add(item);

            return items;
        }

        public static List<Item> GenSwords()
        {
            string type = "Weapon";
            string category = "Sword";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 30, 0));
            properties.Add(NewProperty("Rune", "Slots", 2, 0));
            properties.Add(NewProperty("EP", "Cost", 2, 0));
            Item item = NewItem(type, category, "Iron", properties);
            item.Buy_Price = 150;
            items.Add(item);

            properties[0].Value = 60;
            properties[1].Value = 2;
            properties[2].Value = 3;
            item = NewItem(type, category, "Copper", properties);
            item.Buy_Price = 300;
            items.Add(item);

            properties[0].Value = 90;
            properties[1].Value = 2;
            properties[2].Value = 4;
            item = NewItem(type, category, "Bronze", properties);
            item.Buy_Price = 450;
            items.Add(item);

            properties[0].Value = 120;
            properties[1].Value = 2;
            properties[2].Value = 5;
            item = NewItem(type, category, "Steel", properties);
            item.Buy_Price = 600;
            items.Add(item);

            return items;
        }

        public static List<Item> GenBows()
        {
            string type = "Weapon";
            string category = "Bow";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 35, 0));
            properties.Add(NewProperty("Rune", "Slots", 1, 0));
            properties.Add(NewProperty("EP", "Cost", 4, 0));
            Item item = NewItem(type, category, "Elm", properties);
            item.Buy_Price = 175;
            items.Add(item);

            properties[0].Value = 70;
            properties[1].Value = 1;
            properties[2].Value = 6;
            item = NewItem(type, category, "Cedar", properties);
            item.Buy_Price = 350;
            items.Add(item);

            properties[0].Value = 105;
            properties[1].Value = 1;
            properties[2].Value = 8;
            item = NewItem(type, category, "Oak", properties);
            item.Buy_Price = 525;
            items.Add(item);

            properties[0].Value = 140;
            properties[1].Value = 1;
            properties[2].Value = 10;
            item = NewItem(type, category, "Ebony", properties);
            item.Buy_Price = 700;
            items.Add(item);

            return items;
        }

        public static List<Item> GenGrimoires()
        {
            string type = "Weapon";
            string category = "Grimoire";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Rune", "Slots", 4, 0));
            Item item = NewItem(type, category, "Apprentice", properties);
            item.Buy_Price = 400;
            items.Add(item);

            properties[0].Value = 6;
            item = NewItem(type, category, "Novice", properties);
            item.Buy_Price = 600;
            items.Add(item);

            properties[0].Value = 8;
            item = NewItem(type, category, "Expert", properties);
            item.Buy_Price = 800;
            items.Add(item);

            properties[0].Value = 10;
            item = NewItem(type, category, "Master", properties);
            item.Buy_Price = 1000;
            items.Add(item);

            return items;
        }

        public static List<Item> GenRunes()
        {
            string type = "Rune";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("XP", "Value", 0, 10));
            properties.Add(NewProperty("Level", "Value", 1, 10));
            Item item = NewItem(type, "Area", "Area", properties);
            item.Buy_Price = 800;
            item.Description = "On Weapon: applies paired rune's On Weapon effect to whole enemy squad on attack.\nOn Armor: passively applies paired rune's On Armor effect to your whole squad.";
            items.Add(item);

            item = NewItem(type, "Counter", "Counter", properties);
            item.Buy_Price = 200;
            item.Description = "On Weapon: 10% chance per Level for damage to bypass all defenses.\nOn Armor: 10% chance per Level to counter attack with weapon when hit.\nStatus: Weak.";
            items.Add(item);

            item = NewItem(type, "Death", "Death", properties);
            item.Buy_Price = 600;
            item.Description = "On Weapon: 10% chance per Level to instant kill target.\nOn Armor: 10% chance per Level to resist instant kill.\nStatus: Cursed.";
            items.Add(item);

            item = NewItem(type, "Disarm", "Disarm", properties);
            item.Buy_Price = 400;
            item.Description = "On Weapon: 10% chance per Level to destroy target's weapon.\nOn Armor: 10% chance per Level to destroy attacker's weapon when hit.\nStatus: Melting.";
            items.Add(item);

            item = NewItem(type, "Drain", "Drain", properties);
            item.Buy_Price = 200;
            item.Description = "On Weapon: 10% chance per Level to restore HP by amount of damage inflicted with paired rune.\nOn Armor: 10% chance per Level to restore EP by amount of damage received.\nStatus: Poisoned.";
            items.Add(item);

            item = NewItem(type, "Earth", "Earth", properties);
            item.Buy_Price = 100;
            item.Description = "On Weapon: inflict " + Handler.Element_Multiplier + " Earth damage per Level.\nOn Armor: resist " + Handler.Element_Multiplier + " Earth damage per Level.\nStatus: Petrified.";
            items.Add(item);

            item = NewItem(type, "Effect", "Effect", properties);
            item.Buy_Price = 400;
            item.Description = "On Weapon: 10% chance per Level to apply paired rune's Status effect on target.\nOn Armor: 10% chance per Level to resist paired rune's Status effect.";
            items.Add(item);

            item = NewItem(type, "Fire", "Fire", properties);
            item.Buy_Price = 100;
            item.Description = "On Weapon: inflict " + Handler.Element_Multiplier + " Fire damage per Level.\nOn Armor: resist " + Handler.Element_Multiplier + " Fire damage per Level.\nStatus: Burning.";
            items.Add(item);

            item = NewItem(type, "Life", "Life", properties);
            item.Buy_Price = 200;
            item.Description = "On Weapon: restore " + Handler.Element_Multiplier + " HP per Level on ally with lowest HP.\nOn Armor: restore extra " + Handler.Element_Multiplier + " HP per Level when HP is restored.\nStatus: Regenerating.";
            items.Add(item);

            item = NewItem(type, "Mind", "Mind", properties);
            item.Buy_Price = 200;
            item.Description = "On Weapon: restore " + Handler.Element_Multiplier + " EP per Level on ally with lowest EP.\nOn Armor: restore extra " + Handler.Element_Multiplier + " EP per Level when EP is restored.\nStatus: Charging.";
            items.Add(item);

            item = NewItem(type, "Physical", "Physical", properties);
            item.Buy_Price = 100;
            item.Description = "On Weapon: inflict " + Handler.Element_Multiplier + " Physical damage per Level.\nOn Armor: resist " + Handler.Element_Multiplier + " Physical damage per Level.\nStatus: Stunned.";
            items.Add(item);

            item = NewItem(type, "Time", "Time", properties);
            item.Buy_Price = 200;
            item.Description = "On Weapon: 10% chance per Level to skip target's next turn.\nOn Armor: 10% chance per Level to attack twice.\nStatus: Slow.";
            items.Add(item);

            item = NewItem(type, "Ice", "Ice", properties);
            item.Buy_Price = 100;
            item.Description = "On Weapon: inflict " + Handler.Element_Multiplier + " Ice damage per Level.\nOn Armor: resist " + Handler.Element_Multiplier + " Ice damage per Level.\nStatus: Frozen.";
            items.Add(item);

            item = NewItem(type, "Lightning", "Lightning", properties);
            item.Buy_Price = 100;
            item.Description = "On Weapon: inflict " + Handler.Element_Multiplier + " Lightning damage per Level.\nOn Armor: resist " + Handler.Element_Multiplier + " Lightning damage per Level.\nStatus: Confused.";
            items.Add(item);

            return items;
        }

        public static Item NewItem(string type, string category, string material, List<Something> properties)
        {
            Item item = new Item();
            item.ID = Handler.GetID();
            item.Type = type;
            item.Materials.Add(material);
            item.Categories.Add(category);
            item.Location = new Location();

            if (material == category)
            {
                item.Name = category + " " + type;
                item.Icon = AssetManager.Textures[type + "_" + category];
            }
            else
            {
                if (type == "Weapon")
                {
                    item.Name = material + " " + category;
                }
                else
                {
                    item.Name = material + " " + category + " " + type;
                }
                    
                item.Icon = AssetManager.Textures[type + "_" + category + "_" + material];
            }

            item.Icon_Image = new Rectangle(0, 0, item.Icon.Width, item.Icon.Height);
            item.Icon_DrawColor = Color.White;

            foreach (Something existing in properties)
            {
                Something something = new Something();
                something.ID = existing.ID;
                something.Name = existing.Name;
                something.Type = existing.Type;
                something.Value = existing.Value;
                something.Max_Value = existing.Max_Value;
                something.Rate = existing.Rate;
                item.Properties.Add(something);
            }

            return item;
        }

        public static Something NewProperty(string type, string assignment, int value, int max_value)
        {
            Something property = new Something();
            property.ID = Handler.GetID();
            property.Name = type + " " + assignment;
            property.Value = value;
            property.Max_Value = max_value;
            property.Type = type;
            property.Assignment = assignment;
            return property;
        }

        public static void TestInventory()
        {
            Inventory assets = InventoryManager.GetInventory("Assets");
            if (assets != null)
            {
                Inventory ally = InventoryManager.GetInventory("Ally");
                if (ally != null)
                {
                    int x = 0;
                    int y = 0;

                    for (int i = 0; i < 300; i++)
                    {
                        CryptoRandom random = new CryptoRandom();
                        Item item = assets.Items[random.Next(0, assets.Items.Count)];
                        if (item != null)
                        {
                            Item new_item = CopyItem(item, true);
                            new_item.Location = new Location(x, y, 0);
                            ally.Items.Add(new_item);

                            x++;
                            if (x > 9)
                            {
                                x = 0;
                                y++;
                            }
                        }
                    }
                }
            }
        }

        public static void BeginningInventory()
        {
            Inventory ally = InventoryManager.GetInventory("Ally");
            if (ally != null)
            {
                AddItem(ally, "Iron", "Axe", "Weapon");
                AddItem(ally, "Iron", "Mace", "Weapon");
                AddItem(ally, "Elm", "Bow", "Weapon");
                AddItem(ally, "Iron", "Sword", "Weapon");
                AddItem(ally, "Wood", "Round", "Shield");
                AddItem(ally, "Apprentice", "Apprentice", "Grimoire");

                CryptoRandom random = new CryptoRandom();
                int choice = random.Next(0, 4);
                if (choice == 0)
                {
                    AddItem(ally, "Earth", "Earth", "Rune");
                }
                else if (choice == 1)
                {
                    AddItem(ally, "Lightning", "Lightning", "Rune");
                }
                else if (choice == 2)
                {
                    AddItem(ally, "Fire", "Fire", "Rune");
                }
                else if (choice == 3)
                {
                    AddItem(ally, "Ice", "Ice", "Rune");
                }
            }
        }

        public static void EquipItem(Character character, Item item)
        {
            item.Equipped = true;
            item.Visible = true;

            if (item.Type == "Helm" ||
                item.Type == "Shield")
            {
                item.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + item.Type + "_" + item.Categories[0] + "_" + item.Materials[0]];
            }
            else
            {
                item.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + item.Type + "_" + item.Categories[0] + "_" + item.Materials[0] + "_Idle"];
            }

            item.Image = new Rectangle(0, 0, item.Texture.Width / 4, item.Texture.Height);
            item.Region = character.Region;
        }

        public static void UnequipItem(Character character, Item item)
        {
            item.Equipped = false;
            item.Visible = false;
            item.Region = character.Region;
        }

        public static void AddItem(Inventory inventory, string material, string category, string type)
        {
            Inventory assets = InventoryManager.GetInventory("Assets");
            if (assets != null)
            {
                Item asset = null;

                if (type == "Weapon")
                {
                    asset = assets.GetItem(material + " " + category);
                }
                else if (material == category)
                {
                    asset = assets.GetItem(category + " " + type);
                }
                else
                {
                    asset = assets.GetItem(material + " " + category + " " + type);
                }

                if (asset != null)
                {
                    Item new_item = CopyItem(asset, true);

                    int x = 0;
                    int y = 0;

                    Item last_item = Get_LastItem(inventory);
                    if (last_item != null)
                    {
                        x = (int)last_item.Location.X;
                        y = (int)last_item.Location.Y;

                        x++;
                        if (x > 9)
                        {
                            x = 0;
                            y++;
                        }
                    }

                    new_item.Location = new Location(x, y, 0);
                    inventory.Items.Add(new_item);
                }
            }
        }

        public static Item CopyItem(Item original, bool new_item)
        {
            Item copy = new Item();

            if (new_item)
            {
                copy.ID = Handler.GetID();
                copy.Visible = false;
                copy.Amount = 1;
                copy.Spells = new List<Spell>();
            }
            else
            {
                copy.ID = original.ID;
                copy.Amount = original.Amount;
                copy.Visible = original.Visible;

                foreach (Spell existing in original.Spells)
                {
                    Spell spell = CopySpell(existing, new_item);
                    copy.Spells.Add(spell);
                }
            }

            copy.Name = original.Name;
            copy.Description = original.Description;
            copy.Categories.AddRange(original.Categories);
            copy.Type = original.Type;
            copy.Materials.AddRange(original.Materials);
            copy.Task = original.Task;
            copy.Time = original.Time;
            copy.Buy_Price = original.Buy_Price;

            foreach (Something property in original.Properties)
            {
                copy.Properties.Add(CopyProperty(property, new_item));
            }

            if (original.Attachments != null)
            {
                copy.Attachments = new List<Item>();

                foreach (Item item in original.Attachments)
                {
                    copy.Attachments.Add(CopyItem(item, new_item));
                }
            }

            copy.Texture = original.Texture;
            copy.Region = new Region(original.Region.X, original.Region.Y, original.Region.Width, original.Region.Height);
            copy.Image = new Rectangle(original.Image.X, original.Image.Y, original.Image.Width, original.Image.Height);
            copy.DrawColor = new Color(original.DrawColor.R, original.DrawColor.G, original.DrawColor.B, original.DrawColor.A);

            copy.Icon = original.Icon;

            if (original.Icon_Region != null)
            {
                copy.Icon_Region = new Region(original.Icon_Region.X, original.Icon_Region.Y, original.Icon_Region.Width, original.Icon_Region.Height);
            }
            
            copy.Icon_Image = new Rectangle(original.Icon_Image.X, original.Icon_Image.Y, original.Icon_Image.Width, original.Icon_Image.Height);
            copy.Icon_DrawColor = new Color(original.Icon_DrawColor.R, original.Icon_DrawColor.G, original.Icon_DrawColor.B, original.Icon_DrawColor.A);

            return copy;
        }

        public static Something CopyProperty(Something original, bool new_property)
        {
            Something property = new Something();

            if (new_property)
            {
                property.ID = Handler.GetID();
            }
            else
            {
                property.ID = original.ID;
            }

            property.Name = original.Name;
            property.Type = original.Type;
            property.Description = original.Description;
            property.Rate = original.Rate;
            property.Value = original.Value;
            property.Max_Value = original.Max_Value;

            return property;
        }

        public static Spell CopySpell(Spell original, bool new_spell)
        {
            Spell spell = new Spell();
            spell.Name = original.Name;

            if (new_spell)
            {
                spell.ID = Handler.GetID();
            }
            else
            {
                spell.ID = original.ID;
            }

            foreach (Something existing in original.Properties)
            {
                Something part = new Something();

                if (new_spell)
                {
                    part.ID = Handler.GetID();
                }
                else
                {
                    part.ID = existing.ID;
                }

                part.Max_Value = existing.Max_Value;
                part.Rate = existing.Rate;
                part.Name = existing.Name;
                part.Type = existing.Type;
                spell.Properties.Add(part);
            }

            return spell;
        }

        public static Inventory Gen_Shop(int depth)
        {
            Inventory inventory = new Inventory();

            CryptoRandom random = new CryptoRandom();
            int min_tier = (int)Math.Ceiling(depth / 2.5);
            int max_tier = (int)Math.Ceiling(depth / 1.5);
            if (max_tier > 10)
            {
                max_tier = 10;
            }

            for (int i = 0; i < 20; i++)
            {
                #region Helms

                random = new CryptoRandom();
                int helm_tier = random.Next(min_tier, max_tier + 1);
                switch (helm_tier)
                {
                    case 1:
                        AddItem(inventory, "Cloth", "Cloth", "Helm");
                        break;

                    case 2:
                        AddItem(inventory, "Leather", "Leather", "Helm");
                        break;

                    case 3:
                        AddItem(inventory, "Iron", "Chainmail", "Helm");
                        break;

                    case 4:
                        AddItem(inventory, "Copper", "Chainmail", "Helm");
                        break;

                    case 5:
                        AddItem(inventory, "Bronze", "Chainmail", "Helm");
                        break;

                    case 6:
                        AddItem(inventory, "Steel", "Chainmail", "Helm");
                        break;

                    case 7:
                        AddItem(inventory, "Iron", "Platemail", "Helm");
                        break;

                    case 8:
                        AddItem(inventory, "Copper", "Platemail", "Helm");
                        break;

                    case 9:
                        AddItem(inventory, "Bronze", "Platemail", "Helm");
                        break;

                    case 10:
                        AddItem(inventory, "Steel", "Platemail", "Helm");
                        break;
                }

                #endregion

                #region Armor

                random = new CryptoRandom();
                int armor_tier = random.Next(min_tier, max_tier + 1);
                switch (armor_tier)
                {
                    case 1:
                        AddItem(inventory, "Cloth", "Cloth", "Armor");
                        break;

                    case 2:
                        AddItem(inventory, "Leather", "Leather", "Armor");
                        break;

                    case 3:
                        AddItem(inventory, "Iron", "Chainmail", "Armor");
                        break;

                    case 4:
                        AddItem(inventory, "Copper", "Chainmail", "Armor");
                        break;

                    case 5:
                        AddItem(inventory, "Bronze", "Chainmail", "Armor");
                        break;

                    case 6:
                        AddItem(inventory, "Steel", "Chainmail", "Armor");
                        break;

                    case 7:
                        AddItem(inventory, "Iron", "Platemail", "Armor");
                        break;

                    case 8:
                        AddItem(inventory, "Copper", "Platemail", "Armor");
                        break;

                    case 9:
                        AddItem(inventory, "Bronze", "Platemail", "Armor");
                        break;

                    case 10:
                        AddItem(inventory, "Steel", "Platemail", "Armor");
                        break;
                }

                #endregion

                #region Shields

                random = new CryptoRandom();
                int shield_tier = random.Next(min_tier, max_tier + 1);
                switch (shield_tier)
                {
                    case 1:
                        AddItem(inventory, "Wood", "Round", "Shield");
                        break;

                    case 2:
                        AddItem(inventory, "Iron", "Round", "Shield");
                        break;

                    case 3:
                        AddItem(inventory, "Copper", "Round", "Shield");
                        break;

                    case 4:
                        AddItem(inventory, "Bronze", "Round", "Shield");
                        break;

                    case 5:
                        AddItem(inventory, "Steel", "Round", "Shield");
                        break;

                    case 6:
                        AddItem(inventory, "Wood", "Kite", "Shield");
                        break;

                    case 7:
                        AddItem(inventory, "Iron", "Kite", "Shield");
                        break;

                    case 8:
                        AddItem(inventory, "Copper", "Kite", "Shield");
                        break;

                    case 9:
                        AddItem(inventory, "Bronze", "Kite", "Shield");
                        break;

                    case 10:
                        AddItem(inventory, "Steel", "Kite", "Shield");
                        break;
                }

                #endregion

                #region Weapons

                string weapon_type = "";

                random = new CryptoRandom();
                int weapon_choice = random.Next(0, 5);
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

                    case 3:
                        weapon_type = "Bow";
                        break;

                    case 4:
                        weapon_type = "Grimoire";
                        break;
                }

                random = new CryptoRandom();
                int weapon_tier = random.Next(min_tier, max_tier + 1);

                if (weapon_type == "Sword" ||
                    weapon_type == "Mace" ||
                    weapon_type == "Axe")
                {
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            AddItem(inventory, "Iron", weapon_type, "Weapon");
                            break;

                        case 4:
                        case 5:
                            AddItem(inventory, "Copper", weapon_type, "Weapon");
                            break;

                        case 6:
                        case 7:
                        case 8:
                            AddItem(inventory, "Bronze", weapon_type, "Weapon");
                            break;

                        case 9:
                        case 10:
                            AddItem(inventory, "Steel", weapon_type, "Weapon");
                            break;
                    }
                }
                else if (weapon_type == "Bow")
                {
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            AddItem(inventory, "Elm", weapon_type, "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            AddItem(inventory, "Cedar", weapon_type, "Weapon");
                            break;

                        case 7:
                        case 8:
                            AddItem(inventory, "Oak", weapon_type, "Weapon");
                            break;

                        case 9:
                        case 10:
                            AddItem(inventory, "Ebony", weapon_type, "Weapon");
                            break;
                    }
                }
                else if (weapon_type == "Grimoire")
                {
                    switch (weapon_tier)
                    {
                        case 1:
                        case 2:
                        case 3:
                            AddItem(inventory, "Apprentice", weapon_type, "Weapon");
                            break;

                        case 4:
                        case 5:
                        case 6:
                            AddItem(inventory, "Novice", weapon_type, "Weapon");
                            break;

                        case 7:
                        case 8:
                            AddItem(inventory, "Expert", weapon_type, "Weapon");
                            break;

                        case 9:
                        case 10:
                            AddItem(inventory, "Master", weapon_type, "Weapon");
                            break;
                    }
                }

                #endregion

                #region Runes

                random = new CryptoRandom();
                int rune_choice = random.Next(0, 4);
                switch (rune_choice)
                {
                    case 0:
                        AddItem(inventory, "Earth", "Earth", "Rune");
                        break;

                    case 1:
                        AddItem(inventory, "Lightning", "Lightning", "Rune");
                        break;

                    case 2:
                        AddItem(inventory, "Ice", "Ice", "Rune");
                        break;

                    case 3:
                        AddItem(inventory, "Fire", "Fire", "Rune");
                        break;

                    case 4:
                        AddItem(inventory, "Drain", "Drain", "Rune");
                        break;

                    case 5:
                        AddItem(inventory, "Area", "Area", "Rune");
                        break;

                    case 6:
                        AddItem(inventory, "Effect", "Effect", "Rune");
                        break;

                    case 7:
                        AddItem(inventory, "Disarm", "Disarm", "Rune");
                        break;

                    case 8:
                        AddItem(inventory, "Life", "Life", "Rune");
                        break;

                    case 9:
                        AddItem(inventory, "Mind", "Mind", "Rune");
                        break;

                    case 10:
                        AddItem(inventory, "Physical", "Physical", "Rune");
                        break;

                    case 11:
                        AddItem(inventory, "Time", "Time", "Rune");
                        break;

                    case 12:
                        AddItem(inventory, "Death", "Death", "Rune");
                        break;

                    case 13:
                        AddItem(inventory, "Counter", "Counter", "Rune");
                        break;
                }

                #endregion
            }

            return inventory;
        }

        public static bool Element_IsDamage(string type)
        {
            if (type == "Fire" ||
                type == "Lightning" ||
                type == "Earth" ||
                type == "Ice")
            {
                return true;
            }

            return false;
        }

        public static bool Weapon_IsAoE(Item weapon)
        {
            if (weapon != null)
            {
                Item area_rune = null;

                for (int i = 0; i < weapon.Attachments.Count; i++)
                {
                    Item rune = weapon.Attachments[i];

                    if (rune.Categories.Contains("Area"))
                    {
                        area_rune = rune;
                        break;
                    }
                }

                if (area_rune != null)
                {
                    Item paired_rune = GetPairedRune(weapon, area_rune);
                    if (paired_rune != null &&
                        Element_IsDamage(paired_rune.Categories[0]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool Weapon_HasElement(Item weapon, string type)
        {
            foreach (Something property in weapon.Properties)
            {
                if (property.Name.Contains(type))
                {
                    return true;
                }
            }

            return false;
        }

        public static Item GetPairedRune(Item equipment, Item target_rune)
        {
            int index = -1;

            int count = equipment.Attachments.Count;
            for (int i = 0; i < count; i++)
            {
                Item rune = equipment.Attachments[i];
                if (rune.ID == target_rune.ID)
                {
                    index = i;
                }
                else if (index != -1)
                {
                    if ((index % 2 == 0 && i == index + 1) ||
                        (i == index - 1))
                    {
                        return rune;
                    }
                }
            }

            return null;
        }

        public static int Get_ItemIndex(Item item)
        {
            return (int)((item.Location.Y * 10) + item.Location.X);
        }

        public static int GetPrice(Item item)
        {
            Inventory assets = InventoryManager.GetInventory("Assets");
            if (assets != null)
            {
                Item asset = null;

                if (item.Type == "Weapon")
                {
                    asset = assets.GetItem(item.Materials[0] + " " + item.Categories[0]);
                }
                else if (item.Materials[0] == item.Categories[0])
                {
                    asset = assets.GetItem(item.Categories[0] + " " + item.Type);
                }
                else
                {
                    asset = assets.GetItem(item.Materials[0] + " " + item.Categories[0] + " " + item.Type);
                }

                if (asset != null)
                {
                    return (int)asset.Buy_Price;
                }
            }

            return 0;
        }

        public static Item Get_LastItem(Inventory inventory)
        {
            if (inventory != null)
            {
                if (inventory.Items.Count > 0)
                {
                    Item last = inventory.Items[0];
                    int last_index = Get_ItemIndex(last);

                    foreach (Item item in inventory.Items)
                    {
                        int current_index = Get_ItemIndex(item);
                        if (current_index > last_index)
                        {
                            last = item;
                            last_index = current_index;
                        }
                    }

                    return last;
                }
            }

            return null;
        }

        public static Item Get_EquippedItem(Character character, string type)
        {
            if (character != null)
            {
                Inventory inventory = character.Inventory;
                foreach (Item item in inventory.Items)
                {
                    if (item.Equipped &&
                        item.Type == type)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public static List<Item> SortItems(List<Item> items, string type)
        {
            for (int j = 0; j < items.Count - 1; j++)
            {
                for (int i = 0; i < items.Count - 1; i++)
                {
                    Item current = items[i];
                    Item next = items[i + 1];

                    int current_value = 0;
                    if (type == "Defense")
                    {
                        current_value = Get_TotalDefense(current);
                    }
                    else if (type == "Damage")
                    {
                        current_value = Get_TotalDamage(current);
                    }

                    float current_slots = 0;
                    Something property = current.GetProperty("Rune Slots");
                    if (property != null)
                    {
                        current_slots = property.Value;
                    }

                    int next_value = 0;
                    if (type == "Defense")
                    {
                        next_value = Get_TotalDefense(next);
                    }
                    else if (type == "Damage")
                    {
                        next_value = Get_TotalDamage(next);
                    }

                    float next_slots = 0;
                    property = next.GetProperty("Rune Slots");
                    if (property != null)
                    {
                        next_slots = property.Value;
                    }

                    bool next_is_better = false;

                    if (current_value > next_value)
                    {
                        next_is_better = true;
                    }
                    else if (current_value == next_value)
                    {
                        if (current_slots > next_slots)
                        {
                            next_is_better = true;
                        }
                        else if (current_slots == next_slots)
                        {
                            if (string.Compare(current.Name, next.Name) < 0)
                            {
                                next_is_better = true;
                            }
                        }
                    }

                    if (next_is_better)
                    {
                        Item item = next;
                        items[i + 1] = current;
                        items[i] = item;
                    }
                }
            }

            return items;
        }

        public static int Get_TotalDefense(Item item)
        {
            float total = 0;

            foreach (Something property in item.Properties)
            {
                if (property.Name.Contains("Defense"))
                {
                    total += property.Value;
                }
            }

            return (int)total;
        }

        public static int Get_TotalDefense(Item item, string type)
        {
            float total = 0;

            foreach (Something property in item.Properties)
            {
                if (property.Name.Contains(type) &&
                    property.Name.Contains("Defense"))
                {
                    total += property.Value;
                }
            }

            return (int)total;
        }

        public static int Get_TotalDamage(Item item)
        {
            float total = 0;

            foreach (Something property in item.Properties)
            {
                if (property.Name.Contains("Damage"))
                {
                    total += property.Value;
                }
            }

            return (int)total;
        }

        public static int Get_TotalDamage(Item item, string type)
        {
            float total = 0;

            foreach (Something property in item.Properties)
            {
                if (property.Name.Contains(type) && 
                    property.Name.Contains("Damage"))
                {
                    total += property.Value;
                }
            }

            return (int)total;
        }

        public static int Get_EP_Cost(Character character)
        {
            int total = 0;

            if (character != null)
            {
                Inventory inventory = character.Inventory;
                foreach (Item item in inventory.Items)
                {
                    if (item.Equipped)
                    {
                        Something ep_cost = item.GetProperty("EP Cost");
                        if (ep_cost != null)
                        {
                            total += (int)ep_cost.Value;
                        }
                    }
                }
            }

            return total;
        }
    }
}
