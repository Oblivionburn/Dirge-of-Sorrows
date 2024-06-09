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
                Name = "Player"
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
            items.Add(NewItem(type, category, "Cloth", properties));

            category = "Leather";
            properties[0].Value = 10;
            properties[1].Value = 4;
            properties[2].Value = 2;
            items.Add(NewItem(type, category, "Leather", properties));

            category = "Chainmail";
            properties[0].Value = 15;
            properties[1].Value = 2;
            properties[2].Value = 3;
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 20;
            properties[2].Value = 4;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 25;
            properties[2].Value = 5;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 30;
            properties[2].Value = 6;
            items.Add(NewItem(type, category, "Steel", properties));

            category = "Platemail";
            properties[0].Value = 35;
            properties[1].Value = 0;
            properties[2].Value = 7;
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 40;
            properties[2].Value = 8;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 45;
            properties[2].Value = 9;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 50;
            properties[2].Value = 10;
            items.Add(NewItem(type, category, "Steel", properties));

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
            items.Add(NewItem(type, category, "Cloth", properties));

            category = "Leather";
            properties[0].Value = 2;
            properties[1].Value = 4;
            properties[2].Value = 1;
            items.Add(NewItem(type, category, "Leather", properties));

            category = "Chainmail";
            properties[0].Value = 3;
            properties[1].Value = 2;
            properties[2].Value = 2;
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 4;
            properties[2].Value = 2;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 5;
            properties[2].Value = 3;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 6;
            properties[2].Value = 3;
            items.Add(NewItem(type, category, "Steel", properties));

            category = "Platemail";
            properties[0].Value = 7;
            properties[1].Value = 0;
            properties[2].Value = 4;
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 8;
            properties[2].Value = 4;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 9;
            properties[2].Value = 5;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 10;
            properties[2].Value = 5;
            items.Add(NewItem(type, category, "Steel", properties));

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
            items.Add(NewItem(type, category, "Wood", properties));

            properties[0].Value = 10;
            properties[1].Value = 1;
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 15;
            properties[1].Value = 2;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 20;
            properties[1].Value = 2;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 25;
            properties[1].Value = 3;
            items.Add(NewItem(type, category, "Steel", properties));

            category = "Kite";
            List<Something> special_properties = new List<Something>()
            {
                NewProperty("Physical", "Defense", 30, 0),
                NewProperty("Rune", "Slots", 4, 0),
                NewProperty("EP", "Cost", 3, 0)
            };
            items.Add(NewItem(type, category, "Wood", special_properties));

            properties[0].Value = 35;
            properties[1].Value = 4;
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 40;
            properties[1].Value = 4;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 45;
            properties[1].Value = 5;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 50;
            properties[1].Value = 5;
            items.Add(NewItem(type, category, "Steel", properties));

            return items;
        }

        public static List<Item> GenAxes()
        {
            string type = "Weapon";
            string category = "Axe";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 35, 0));
            properties.Add(NewProperty("EP", "Cost", 4, 0));
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 70;
            properties[1].Value = 6;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 105;
            properties[1].Value = 8;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 140;
            properties[1].Value = 10;
            items.Add(NewItem(type, category, "Steel", properties));

            return items;
        }

        public static List<Item> GenMaces()
        {
            string type = "Weapon";
            string category = "Mace";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 20, 0));
            properties.Add(NewProperty("Rune", "Slots", 3, 0));
            properties.Add(NewProperty("EP", "Cost", 1, 0));
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 40;
            properties[1].Value = 3;
            properties[2].Value = 2;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 60;
            properties[1].Value = 3;
            properties[2].Value = 3;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 80;
            properties[1].Value = 3;
            properties[2].Value = 4;
            items.Add(NewItem(type, category, "Steel", properties));

            return items;
        }

        public static List<Item> GenSwords()
        {
            string type = "Weapon";
            string category = "Sword";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 30, 0));
            properties.Add(NewProperty("Rune", "Slots", 1, 0));
            properties.Add(NewProperty("EP", "Cost", 2, 0));
            items.Add(NewItem(type, category, "Iron", properties));

            properties[0].Value = 60;
            properties[1].Value = 1;
            properties[2].Value = 3;
            items.Add(NewItem(type, category, "Copper", properties));

            properties[0].Value = 90;
            properties[1].Value = 1;
            properties[2].Value = 4;
            items.Add(NewItem(type, category, "Bronze", properties));

            properties[0].Value = 120;
            properties[1].Value = 1;
            properties[2].Value = 5;
            items.Add(NewItem(type, category, "Steel", properties));

            return items;
        }

        public static List<Item> GenBows()
        {
            string type = "Weapon";
            string category = "Bow";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Physical", "Damage", 25, 0));
            properties.Add(NewProperty("Rune", "Slots", 2, 0));
            properties.Add(NewProperty("EP", "Cost", 4, 0));
            items.Add(NewItem(type, category, "Elm", properties));

            properties[0].Value = 50;
            properties[1].Value = 2;
            properties[2].Value = 6;
            items.Add(NewItem(type, category, "Cedar", properties));

            properties[0].Value = 75;
            properties[1].Value = 2;
            properties[2].Value = 8;
            items.Add(NewItem(type, category, "Oak", properties));

            properties[0].Value = 100;
            properties[1].Value = 2;
            properties[2].Value = 10;
            items.Add(NewItem(type, category, "Ebony", properties));

            return items;
        }

        public static List<Item> GenGrimoires()
        {
            string type = "Weapon";
            string category = "Grimoire";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("Rune", "Slots", 4, 0));
            items.Add(NewItem(type, category, "Apprentice", properties));

            properties[0].Value = 6;
            items.Add(NewItem(type, category, "Novice", properties));

            properties[0].Value = 8;
            items.Add(NewItem(type, category, "Expert", properties));

            properties[0].Value = 10;
            items.Add(NewItem(type, category, "Master", properties));

            return items;
        }

        public static List<Item> GenRunes()
        {
            string type = "Rune";

            List<Item> items = new List<Item>();
            List<Something> properties = new List<Something>();

            properties.Add(NewProperty("XP", "Value", 0, 10));
            properties.Add(NewProperty("Level", "Value", 1, 10));

            items.Add(NewItem(type, "Area", "Area", properties));
            items.Add(NewItem(type, "Counter", "Counter", properties));
            items.Add(NewItem(type, "Death", "Death", properties));
            items.Add(NewItem(type, "Disarm", "Disarm", properties));
            items.Add(NewItem(type, "Drain", "Drain", properties));
            items.Add(NewItem(type, "Earth", "Earth", properties));
            items.Add(NewItem(type, "Effect", "Effect", properties));
            items.Add(NewItem(type, "Fire", "Fire", properties));
            items.Add(NewItem(type, "Life", "Life", properties));
            items.Add(NewItem(type, "Mind", "Mind", properties));
            items.Add(NewItem(type, "Physical", "Physical", properties));
            items.Add(NewItem(type, "Time", "Time", properties));
            items.Add(NewItem(type, "Water", "Water", properties));
            items.Add(NewItem(type, "Wind", "Wind", properties));

            return items;
        }

        public static Item NewItem(string type, string category, string material, List<Something> properties)
        {
            Item item = new Item();
            item.ID = Handler.GetID();
            item.Type = type;
            item.Materials.Add(material);
            item.Categories.Add(category);

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
                Inventory player = InventoryManager.GetInventory("Player");
                if (player != null)
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
                            new_item.Location = new Vector3(x, y, 0);
                            player.Items.Add(new_item);

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
            Inventory player = InventoryManager.GetInventory("Player");
            if (player != null)
            {
                AddItem(player, "Iron", "Axe", "Weapon");
                AddItem(player, "Iron", "Mace", "Weapon");
                AddItem(player, "Elm", "Bow", "Weapon");
                AddItem(player, "Iron", "Sword", "Weapon");
                AddItem(player, "Wood", "Round", "Shield");
                AddItem(player, "Apprentice", "Apprentice", "Grimoire");

                CryptoRandom random = new CryptoRandom();
                int choice = random.Next(0, 4);
                if (choice == 0)
                {
                    AddItem(player, "Earth", "Earth", "Rune");
                }
                else if (choice == 1)
                {
                    AddItem(player, "Wind", "Wind", "Rune");
                }
                else if (choice == 2)
                {
                    AddItem(player, "Fire", "Fire", "Rune");
                }
                else if (choice == 3)
                {
                    AddItem(player, "Water", "Water", "Rune");
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

                    new_item.Location = new Vector3(x, y, 0);
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
            copy.Categories.AddRange(original.Categories);
            copy.Type = original.Type;
            copy.Materials.AddRange(original.Materials);
            copy.Task = original.Task;
            copy.Time = original.Time;

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

        public static bool Element_IsDamage(string type)
        {
            if (type == "Fire" ||
                type == "Wind" ||
                type == "Earth" ||
                type == "Water")
            {
                return true;
            }

            return false;
        }

        public static int Get_ItemIndex(Item item)
        {
            return (int)((item.Location.Y * 10) + item.Location.X);
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
    }
}
