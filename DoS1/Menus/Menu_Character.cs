using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Utility;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Character : Menu
    {
        #region Variables

        bool ControlsLoaded;
        Character character = null;

        int Top;
        List<Picture> GridList = new List<Picture>();
        List<Item> ItemList = new List<Item>();

        bool moving;
        Rectangle starting_pos;

        int width;
        int height;
        int starting_Y;
        int starting_X;

        #endregion

        #region Constructors

        public Menu_Character(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Character";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (moving)
                {
                    MoveItem();
                }
                else if (string.IsNullOrEmpty(Handler.AlertType) ||
                         Handler.AlertType == "Generic")
                {
                    UpdateControls();
                }

                if (character != null)
                {
                    CharacterUtil.UpdateGear(character);
                }

                if (Handler.Tutorials &&
                    !Handler.Tutorial_Character &&
                    !Handler.ViewOnly_Character)
                {
                    Handler.TutorialType = "Character";
                    GameUtil.Alert_Tutorial();
                }

                base.Update(gameRef, content);
            } 
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Background")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name != "Window_Top" &&
                        picture.Name != "Highlight" &&
                        picture.Name != "Background")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Highlight")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                if (character != null)
                {
                    CharacterUtil.DrawCharacter(spriteBatch, character, Color.White);
                }

                Inventory inventory = InventoryManager.GetInventory("Ally");
                if (inventory != null)
                {
                    foreach (Item item in inventory.Items)
                    {
                        if (item.Icon_Visible)
                        {
                            item.Draw(spriteBatch, Main.Game.Resolution, Color.White);
                        }
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Window_Top")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        private void UpdateControls()
        {
            bool found_button = HoveringButton();
            bool found_stat = HoveringStat();
            bool found_grid = HoveringGrid();
            bool found_item = HoveringItem();

            bool found_helm = HoveringSlot("Helm");
            bool found_armor = HoveringSlot("Armor");
            bool found_shield = HoveringSlot("Shield");
            bool found_weapon = HoveringSlot("Weapon");

            if (!found_grid &&
                !found_helm &&
                !found_armor &&
                !found_shield &&
                !found_weapon)
            {
                GetPicture("Highlight").Visible = false;
            }

            if (!found_button &&
                !found_stat &&
                !found_item)
            {
                GetLabel("Examine").Visible = false;
            }

            if (InputManager.Mouse_ScrolledDown)
            {
                Inventory inventory = InventoryManager.GetInventory("Ally");
                if (inventory != null)
                {
                    Item last_item = null;
                    if (ItemList.Count > 0)
                    {
                        last_item = ItemList[ItemList.Count - 1];
                    }

                    if (last_item != null)
                    {
                        Top++;

                        if (Top > last_item.Location.Y)
                        {
                            Top = (int)last_item.Location.Y;
                        }
                    }

                    ResizeInventory();
                }
            }
            else if (InputManager.Mouse_ScrolledUp)
            {
                Top--;
                if (Top <= 0)
                {
                    Top = 0;
                }

                ResizeInventory();
            }

            if (!found_helm &&
                !found_armor &&
                !found_shield &&
                !found_weapon &&
                !found_button &&
                !found_stat &&
                !found_grid &&
                !found_item &&
                InputManager.Mouse_RB_Pressed)
            {
                Back();
            }

            if (InputManager.KeyPressed("Esc"))
            {
                Back();
            }
        }

        private bool HoveringButton()
        {
            bool found = false;

            foreach (Button button in Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;

                        if (button.HoverText != null)
                        {
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Opacity = 1;
                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick(button);

                            button.Opacity = 0.8f;
                            button.Selected = false;

                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        button.Opacity = 0.8f;
                        button.Selected = false;
                    }
                }
            }

            return found;
        }

        private bool HoveringGrid()
        {
            bool found = false;

            foreach (Picture grid in GridList)
            {
                if (InputManager.MouseWithin(grid.Region.ToRectangle))
                {
                    found = true;

                    Picture highlight = GetPicture("Highlight");
                    highlight.Region = grid.Region;
                    highlight.Visible = true;

                    break;
                }
            }

            return found;
        }

        private bool HoveringItem()
        {
            bool found = false;

            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                foreach (Item item in inventory.Items)
                {
                    if (item.Icon_Visible)
                    {
                        if (InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(item);

                            if (InputManager.Mouse_LB_Held &&
                                InputManager.Mouse.Moved)
                            {
                                found = false;
                                moving = true;
                                starting_pos = item.Icon_Region.ToRectangle;
                                Handler.Selected_Item = item.ID;
                                break;
                            }
                            else if (InputManager.Mouse_RB_Pressed)
                            {
                                Something slots = item.GetProperty("Rune Slots");
                                if (slots != null)
                                {
                                    if (slots.Value > 0)
                                    {
                                        found = false;
                                        SelectItem(item.ID);
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }

            if (character != null)
            {
                foreach (Item item in character.Inventory.Items)
                {
                    if (item.Icon_Visible &&
                        item.Equipped)
                    {
                        if (InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(item);

                            if (InputManager.Mouse_LB_Held &&
                                InputManager.Mouse.Moved &&
                                !Handler.ViewOnly_Character)
                            {
                                found = false;
                                moving = true;
                                starting_pos = item.Icon_Region.ToRectangle;
                                Handler.Selected_Item = item.ID;
                                break;
                            }
                            else if (InputManager.Mouse_RB_Pressed)
                            {
                                Something slots = item.GetProperty("Rune Slots");
                                if (slots != null)
                                {
                                    if (slots.Value > 0)
                                    {
                                        found = false;
                                        SelectItem(item.ID);
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }

            return found;
        }

        private bool HoveringStat()
        {
            foreach (Label label in Labels)
            {
                if (label.Name == "Level" ||
                    label.Name == "XP" ||
                    label.Name == "STR" ||
                    label.Name == "INT" ||
                    label.Name == "DEX" ||
                    label.Name == "AGI")
                {
                    if (InputManager.MouseWithin(label.Region.ToRectangle))
                    {
                        ExamineStat(label.Name);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HoveringSlot(string type)
        {
            bool found = false;

            Picture slot = GetPicture(type);
            if (InputManager.MouseWithin(slot.Region.ToRectangle))
            {
                found = true;
                Picture highlight = GetPicture("Highlight");
                highlight.Region = slot.Region;
                highlight.Visible = true;
            }

            return found;
        }

        private void MoveItem()
        {
            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                Item item = null;
                bool equipped = false;

                foreach (Item existing in inventory.Items)
                {
                    if (existing.ID == Handler.Selected_Item)
                    {
                        item = existing;
                        break;
                    }
                }

                if (item == null)
                {
                    foreach (Item existing in character.Inventory.Items)
                    {
                        if (existing.ID == Handler.Selected_Item &&
                            existing.Equipped)
                        {
                            equipped = true;
                            item = existing;
                            break;
                        }
                    }
                }

                if (item != null)
                {
                    if (InputManager.Mouse_LB_Held)
                    {
                        item.Icon_Region = new Region(InputManager.Mouse.X - (width / 2), InputManager.Mouse.Y - (height / 2), width, height);
                    }
                    else
                    {
                        moving = false;

                        if (!equipped)
                        {
                            bool found_helm = HoveringSlot("Helm");
                            bool found_armor = HoveringSlot("Armor");
                            bool found_shield = HoveringSlot("Shield");
                            bool found_weapon = HoveringSlot("Weapon");

                            if (!found_helm &&
                                !found_armor &&
                                !found_shield &&
                                !found_weapon)
                            {
                                item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                            }
                            else if ((found_helm && item.Type == "Helm") ||
                                     (found_armor && item.Type == "Armor") ||
                                     (found_shield && item.Type == "Shield") ||
                                     (found_weapon && item.Type == "Weapon"))
                            {
                                Item equipped_item = InventoryUtil.Get_EquippedItem(character, item.Type);
                                if (equipped_item != null)
                                {
                                    SwapItems(inventory, item, equipped_item);
                                }
                                else
                                {
                                    EquipItem(inventory, item);
                                }
                            }
                        }
                        else
                        {
                            bool found_grid = HoveringGrid();
                            if (!found_grid)
                            {
                                item.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                            }
                            else
                            {
                                UnequipItem(inventory, item);
                            }
                        }

                        Filter(Handler.ItemFilter);
                        LoadCharacter();
                        ResizeInventory();
                    }
                }
            }
        }

        private void SwapItems(Inventory main_inventory, Item moving_item, Item existing_item)
        {
            UnequipItem(main_inventory, existing_item);
            EquipItem(main_inventory, moving_item);
        }

        private void EquipItem(Inventory main_inventory, Item item)
        {
            AssetManager.PlaySound_Random("Equip");

            InventoryUtil.EquipItem(character, item);
            character.Inventory.Items.Add(item);
            main_inventory.Items.Remove(item);
            ItemList.Remove(item);

            if (item.Type == "Weapon" &&
                InventoryUtil.Weapon_Is2H(item))
            {
                foreach (Item existing in character.Inventory.Items)
                {
                    if (existing.Equipped &&
                        existing.Type == "Shield")
                    {
                        UnequipItem(main_inventory, existing);
                        break;
                    }
                }
            }
            else if (item.Type == "Shield")
            {
                foreach (Item existing in character.Inventory.Items)
                {
                    if (existing.Equipped &&
                        existing.Type == "Weapon")
                    {
                        if (InventoryUtil.Weapon_Is2H(existing))
                        {
                            UnequipItem(main_inventory, existing);
                            break;
                        }
                    }
                }
            }
        }

        private void UnequipItem(Inventory main_inventory, Item item)
        {
            InventoryUtil.UnequipItem(character, item);
            main_inventory.Items.Add(item);
            character.Inventory.Items.Remove(item);
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                Back();
            }
            else if (button.Name == "Edit")
            {
                Edit();
            }
            else if (button.Name == "Helms" ||
                     button.Name == "Armors" ||
                     button.Name == "Shields" ||
                     button.Name == "Weapons")
            {
                Filter(button.Name);
            }
        }

        private void Back()
        {
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                foreach (Item item in inventory.Items)
                {
                    item.Icon_Visible = false;
                }
            }

            foreach (Item item in character.Inventory.Items)
            {
                item.Icon_Visible = false;
            }

            character.HealthBar.Visible = false;
            character.ManaBar.Visible = false;

            MenuManager.ChangeMenu_Previous();
        }

        private void Edit()
        {
            InputManager.Mouse.Flush();
            MenuManager.ChangeMenu("CharEdit");
        }

        private void SelectItem(long id)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.Selected_Item = id;

            InputManager.Mouse.Flush();
            MenuManager.ChangeMenu("Item");
        }

        private void ResetPos()
        {
            width = Main.Game.MenuSize.X;
            height = Main.Game.MenuSize.Y;
            starting_Y = (Main.Game.ScreenHeight / 2) - (height * 5);

            if (!Handler.ViewOnly_Character)
            {
                starting_X = (Main.Game.ScreenWidth / 2) + width;
            }
            else
            {
                starting_X = (Main.Game.ScreenWidth / 2) + (width * 5);
            }
        }

        private void ExamineItem(Item item)
        {
            int width = (Main.Game.MenuSize.X * 4) + (Main.Game.MenuSize.X / 2);
            if (item.Type == "Rune")
            {
                width = item.Description.Length * (Main.Game.MenuSize.X / 10);
            }
            else if (item.Type == "Weapon" &&
                     item.Categories.Contains("Grimoire"))
            {
                width = (Main.Game.MenuSize.X * 5) + (Main.Game.MenuSize.X / 2);
            }

            int height = Main.Game.MenuSize.Y + (Main.Game.MenuSize.Y / 2);

            string text = item.Name + "\n\n";

            if (!string.IsNullOrEmpty(item.Description))
            {
                text += item.Description + "\n";
                height += Main.Game.MenuSize.Y + (Main.Game.MenuSize.Y / 2);
            }

            if (item.Type == "Weapon")
            {
                if (InventoryUtil.Weapon_Is2H(item))
                {
                    text = item.Name + " (2H)\n\n";
                }

                List<Something> properties = new List<Something>();

                //List damage properties first
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (property.Name.Contains("Damage"))
                    {
                        properties.Add(property);
                    }
                }

                //List non-damage properties
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (!property.Name.Contains("Damage") &&
                        !property.Name.Contains("Slots") &&
                        !property.Name.Contains("Cost"))
                    {
                        properties.Add(property);
                    }
                }

                //List slots
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (property.Name.Contains("Slots"))
                    {
                        properties.Add(property);
                        break;
                    }
                }

                //List cost last
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (property.Name.Contains("Cost"))
                    {
                        properties.Add(property);
                        break;
                    }
                }

                for (int i = 0; i < properties.Count; i++)
                {
                    Something property = properties[i];

                    if (property.Name.Contains("Area") ||
                        property.Name.Contains("Chance") ||
                        property.Name.Contains("Status") ||
                        property.Name.Contains("Drain") ||
                        property.Name.Contains("Resist") ||
                        property.Name.Contains("Haste") ||
                        property.Name.Contains("Dodge") ||
                        property.Name.Contains("Pierce") ||
                        property.Name.Contains("Counter") ||
                        property.Name.Contains("Disarm"))
                    {
                        text += property.Name + ": " + property.Value + "%";
                    }
                    else
                    {
                        text += property.Name + ": " + property.Value;
                    }

                    if (i < properties.Count - 1)
                    {
                        text += "\n";
                        height += (Main.Game.MenuSize.Y / 2);
                    }
                }

                properties.Clear();
            }
            else
            {
                List<Something> properties = new List<Something>();

                //List defense properties first
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (property.Name.Contains("Defense"))
                    {
                        properties.Add(property);
                    }
                }

                //List non-defense properties
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (!property.Name.Contains("Defense") &&
                        !property.Name.Contains("Slots") &&
                        !property.Name.Contains("Cost"))
                    {
                        properties.Add(property);
                    }
                }

                //List slots
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (property.Name.Contains("Slots"))
                    {
                        properties.Add(property);
                        break;
                    }
                }

                //List cost last
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (property.Name.Contains("Cost"))
                    {
                        properties.Add(property);
                        break;
                    }
                }

                for (int i = 0; i < properties.Count; i++)
                {
                    Something property = properties[i];

                    if (property.Name.Contains("Area") ||
                        property.Name.Contains("Chance") ||
                        property.Name.Contains("Status") ||
                        property.Name.Contains("Drain") ||
                        property.Name.Contains("Resist") ||
                        property.Name.Contains("Haste") ||
                        property.Name.Contains("Dodge") ||
                        property.Name.Contains("Pierce") ||
                        property.Name.Contains("Counter") ||
                        property.Name.Contains("Disarm"))
                    {
                        text += property.Name + ": " + property.Value + "%";
                    }
                    else
                    {
                        text += property.Name + ": " + property.Value;
                    }

                    if (i < properties.Count - 1)
                    {
                        text += "\n";
                        height += (Main.Game.MenuSize.Y / 2);
                    }
                }

                properties.Clear();
            }

            Label examine = GetLabel("Examine");
            examine.Text = text;

            int X = InputManager.Mouse.X - (width / 2);
            if (X < 0)
            {
                X = 0;
            }
            else if (X > Main.Game.Resolution.X - width)
            {
                X = Main.Game.Resolution.X - width;
            }

            int Y = InputManager.Mouse.Y + 20;
            if (Y < 0)
            {
                Y = 0;
            }
            else if (Y > Main.Game.Resolution.Y - height)
            {
                Y = Main.Game.Resolution.Y - height;
            }

            examine.Region = new Region(X, Y, width, height);
            examine.Visible = true;
        }

        private void ExamineStat(string name)
        {
            Label examine = GetLabel("Examine");

            switch (name)
            {
                case "Level":
                    examine.Text = "Level (LVL):\nThe progress the character has made.";
                    break;

                case "XP":
                    examine.Text = "Experience (EXP):\nThe character progress to the next Level.";
                    break;

                case "STR":
                    examine.Text = "Strength (STR):\nAffects melee and ranged attack power.";
                    break;

                case "INT":
                    examine.Text = "Intelligence (INT):\nAffects magic attack power.";
                    break;

                case "DEX":
                    examine.Text = "Dexterity (DEX):\nAffects melee and ranged accuracy.";
                    break;

                case "AGI":
                    examine.Text = "Agility (AGI):\nAffects ability to dodge attacks.";
                    break;
            }

            int X = InputManager.Mouse.X - (width / 2);
            if (X < 0)
            {
                X = 0;
            }
            else if (X > Main.Game.Resolution.X - width)
            {
                X = Main.Game.Resolution.X - width;
            }

            int Y = InputManager.Mouse.Y + 20;
            if (Y < 0)
            {
                Y = 0;
            }
            else if (Y > Main.Game.Resolution.Y - height)
            {
                Y = Main.Game.Resolution.Y - height;
            }

            if (name == "INT" ||
                name == "AGI")
            {
                examine.Region = new Region(X, Y, width * 8, height + (height / 2));
            }
            else
            {
                examine.Region = new Region(X, Y, width * 10, height + (height / 2));
            }
                
            examine.Visible = true;
        }

        private void Filter(string filter)
        {
            Handler.ItemFilter = filter;

            Top = 0;
            ResizeGrid();

            ItemList.Clear();

            string type = filter.Substring(0, filter.Length - 1);

            GetPicture("Arrow_Up").Visible = false;
            GetPicture("Arrow_Down").Visible = false;

            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                foreach (Item item in inventory.Items)
                {
                    item.Icon_Visible = false;
                }

                foreach (Item item in inventory.Items)
                {
                    if (item.Type == type)
                    {
                        ItemList.Add(item);
                    }
                }

                string sort_type = "Defense";
                if (filter == "Weapons")
                {
                    sort_type = "Damage";
                }
                ItemList = InventoryUtil.SortItems(ItemList, sort_type);

                for (int i = 0; i < ItemList.Count; i++)
                {
                    Item item = ItemList[i];

                    if (i < GridList.Count)
                    {
                        Picture grid = GridList[i];
                        item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                        item.Location = new Location(grid.Location.X, grid.Location.Y, 0);
                        item.Icon_Visible = true;
                    }
                }
            }
        }

        private Picture GetGrid(Vector2 location)
        {
            foreach (Picture picture in GridList)
            {
                if (picture.Location.X == location.X &&
                    picture.Location.Y == location.Y)
                {
                    return picture;
                }
            }

            return null;
        }

        private void LoadControls()
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddPicture(Handler.GetID(), "Arrow_Up", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Arrow_Down", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Name", "", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Stats", "Stats", Color.IndianRed, new Region(0, 0, 0, 0), true);
            AddPicture(Handler.GetID(), "Stats_Underline", AssetManager.Textures["Path_WE"], new Region(0, 0, 0, 0), Color.IndianRed, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Level", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "XP", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "STR", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "INT", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "DEX", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AGI", "", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Equipment", "Equipment", Color.Gold, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Equipment_Properties", "", Color.White, new Region(0, 0, 0, 0), true);
            AddPicture(Handler.GetID(), "Equipment_Underline", AssetManager.Textures["Path_WE"], new Region(0, 0, 0, 0), Color.Gold, true);

            AddPicture(Handler.GetID(), "Character", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White, false);

            Color equip_color = new Color(96, 96, 96, 255) * 0.8f;
            AddPicture(Handler.GetID(), "Helm", AssetManager.Textures["Helm"], new Region(0, 0, 0, 0), equip_color, true);
            AddPicture(Handler.GetID(), "Armor", AssetManager.Textures["Armor"], new Region(0, 0, 0, 0), equip_color, true);
            AddPicture(Handler.GetID(), "Shield", AssetManager.Textures["Shield"], new Region(0, 0, 0, 0), equip_color, true);
            AddPicture(Handler.GetID(), "Weapon", AssetManager.Textures["Weapon"], new Region(0, 0, 0, 0), equip_color, true);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Back",
                texture = AssetManager.Textures["Button_Back"],
                texture_highlight = AssetManager.Textures["Button_Back_Hover"],
                texture_disabled = AssetManager.Textures["Button_Back_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Edit",
                hover_text = "Edit Character",
                texture = AssetManager.Textures["Button_Edit"],
                texture_highlight = AssetManager.Textures["Button_Edit_Hover"],
                texture_disabled = AssetManager.Textures["Button_Edit_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Helms",
                hover_text = "Helms",
                texture = AssetManager.Textures["Button_Helms"],
                texture_highlight = AssetManager.Textures["Button_Helms_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Armors",
                hover_text = "Armors",
                texture = AssetManager.Textures["Button_Armors"],
                texture_highlight = AssetManager.Textures["Button_Armors_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Shields",
                hover_text = "Shields",
                texture = AssetManager.Textures["Button_Shields"],
                texture_highlight = AssetManager.Textures["Button_Shields_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Weapons",
                hover_text = "Weapons",
                texture = AssetManager.Textures["Button_Weapons"],
                texture_highlight = AssetManager.Textures["Button_Weapons_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            ControlsLoaded = true;
        }

        private void LoadCharacter()
        {
            ResetPos();
            character = null;

            Army ally_army = CharacterManager.GetArmy("Ally");
            if (ally_army != null)
            {
                foreach (Squad squad in ally_army.Squads)
                {
                    character = squad.GetCharacter(Handler.Selected_Character);
                    if (character != null)
                    {
                        GetButton("Edit").Visible = true;
                        break;
                    }
                }
            }

            if (character == null)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                if (enemy_army != null)
                {
                    foreach (Squad squad in enemy_army.Squads)
                    {
                        character = squad.GetCharacter(Handler.Selected_Character);
                        if (character != null)
                        {
                            GetButton("Edit").Visible = false;
                            break;
                        }
                    }
                }
            }

            if (character == null)
            {
                Army reserve_army = CharacterManager.GetArmy("Reserves");
                if (reserve_army != null)
                {
                    foreach (Squad squad in reserve_army.Squads)
                    {
                        character = squad.GetCharacter(Handler.Selected_Character);
                        if (character != null)
                        {
                            GetButton("Edit").Visible = true;
                            break;
                        }
                    }
                }
            }

            if (character != null)
            {
                Picture char_pic = GetPicture("Character");
                char_pic.Region = new Region(starting_X - (width * 7), starting_Y + (height * 2), (width * 4), (height * 6));

                character.Region = new Region(char_pic.Region.X, char_pic.Region.Y, char_pic.Region.Width, char_pic.Region.Height);
                character.Visible = true;

                GetLabel("Name").Region = new Region(char_pic.Region.X, char_pic.Region.Y - height, char_pic.Region.Width, height);

                float X = char_pic.Region.X + char_pic.Region.Width;
                float Y = char_pic.Region.Y + height;

                GetPicture("Helm").Region = new Region(X, Y, width, height);
                GetPicture("Armor").Region = new Region(X, Y + height, width, height);
                GetPicture("Shield").Region = new Region(X, Y + (height * 2), width, height);
                GetPicture("Weapon").Region = new Region(X, Y + (height * 3), width, height);

                CharacterUtil.ResizeBars(character);
                GetLabel("Name").Text = character.Name;

                GetButton("Edit").Region = new Region(char_pic.Region.X + (char_pic.Region.Width / 2) - (Main.Game.MenuSize.X / 2), 
                    char_pic.Region.Y + char_pic.Region.Height + Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

                Item equip = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (equip != null)
                {
                    Picture helm = GetPicture("Helm");
                    equip.Icon_Region = new Region(helm.Region.X, helm.Region.Y, helm.Region.Width, helm.Region.Height);
                    equip.Icon_Image = new Rectangle(0, 0, equip.Icon.Width, equip.Icon.Height);
                    equip.Icon_Visible = true;
                }

                equip = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (equip != null)
                {
                    Picture armor = GetPicture("Armor");
                    equip.Icon_Region = new Region(armor.Region.X, armor.Region.Y, armor.Region.Width, armor.Region.Height);
                    equip.Icon_Image = new Rectangle(0, 0, equip.Icon.Width, equip.Icon.Height);
                    equip.Icon_Visible = true;
                }

                equip = InventoryUtil.Get_EquippedItem(character, "Shield");
                if (equip != null)
                {
                    Picture shield = GetPicture("Shield");
                    equip.Icon_Region = new Region(shield.Region.X, shield.Region.Y, shield.Region.Width, shield.Region.Height);
                    equip.Icon_Image = new Rectangle(0, 0, equip.Icon.Width, equip.Icon.Height);
                    equip.Icon_Visible = true;
                }

                equip = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (equip != null)
                {
                    Picture weapon = GetPicture("Weapon");
                    equip.Icon_Region = new Region(weapon.Region.X, weapon.Region.Y, weapon.Region.Width, weapon.Region.Height);
                    equip.Icon_Image = new Rectangle(0, 0, equip.Icon.Width, equip.Icon.Height);
                    equip.Icon_Visible = true;
                }

                // ===============
                // Character Stats
                // ===============
                X = char_pic.Region.X - (width * 7);
                Y = char_pic.Region.Y - height;

                GetLabel("Stats").Region = new Region(X, Y, (width * 7), height);
                GetPicture("Stats_Underline").Region = new Region(X, Y + (height / 2), (width * 7), height);

                Y += height;
                Label level = GetLabel("Level");
                level.Region = new Region(X, Y, (width * 7), (Main.Game.MenuSize.Y / 4) * 3);
                level.Text = "LVL: " + character.Level + "/100";

                Y += (Main.Game.MenuSize.Y / 4) * 3;
                Label xp = GetLabel("XP");
                xp.Region = new Region(X, Y, (width * 7), (Main.Game.MenuSize.Y / 4) * 3);
                xp.Text = "EXP: " + character.XP + "/" + character.XP_Needed_ForLevels[character.Level + 1];

                Y += (Main.Game.MenuSize.Y / 4) * 3;
                Label strength = GetLabel("STR");
                strength.Region = new Region(X, Y, (width * 7), (Main.Game.MenuSize.Y / 4) * 3);
                strength.Text = "STR: 10/100";

                Y += (Main.Game.MenuSize.Y / 4) * 3;
                Label intelligence = GetLabel("INT");
                intelligence.Region = new Region(X, Y, (width * 7), (Main.Game.MenuSize.Y / 4) * 3);
                intelligence.Text = "INT: 10/100";

                Y += (Main.Game.MenuSize.Y / 4) * 3;
                Label dexterity = GetLabel("DEX");
                dexterity.Region = new Region(X, Y, (width * 7), (Main.Game.MenuSize.Y / 4) * 3);
                dexterity.Text = "DEX: 10/100";

                Y += (Main.Game.MenuSize.Y / 4) * 3;
                Label agility = GetLabel("AGI");
                agility.Region = new Region(X, Y, (width * 7), (Main.Game.MenuSize.Y / 4) * 3);
                agility.Text = "AGI: 10/100";

                // ===============
                // Equipment Stats
                // ===============
                X = char_pic.Region.X - (width * 7);
                Y = char_pic.Region.Y + char_pic.Region.Height - (char_pic.Region.Height / 2) + (height * 2);

                GetLabel("Equipment").Region = new Region(X, Y, (width * 7), height);
                GetPicture("Equipment_Underline").Region = new Region(X, Y + (height / 2), (width * 7), height);

                Label equipment = GetLabel("Equipment_Properties");
                equipment.Region = new Region(X, Y + height, (width * 7), height);
                equipment.Text = "";

                List<Something> Properties = new List<Something>();

                //List defense properties
                foreach (Item item in character.Inventory.Items)
                {
                    if (item.Type != "Weapon")
                    {
                        foreach (Something property in item.Properties)
                        {
                            if (!property.Name.Contains("Slots"))
                            {
                                Something existing_property = null;
                                foreach (Something existing in Properties)
                                {
                                    if (existing.Name == property.Name)
                                    {
                                        existing_property = existing;
                                        break;
                                    }
                                }

                                if (existing_property == null)
                                {
                                    Properties.Add(new Something
                                    {
                                        Name = property.Name,
                                        Value = property.Value
                                    });
                                }
                                else
                                {
                                    existing_property.Value += property.Value;
                                }
                            }
                        }
                    }
                }

                //List damage properties
                foreach (Item item in character.Inventory.Items)
                {
                    if (item.Type == "Weapon")
                    {
                        foreach (Something property in item.Properties)
                        {
                            if (!property.Name.Contains("Slots"))
                            {
                                Something existing_property = null;
                                foreach (Something existing in Properties)
                                {
                                    if (existing.Name == property.Name)
                                    {
                                        existing_property = existing;
                                        break;
                                    }
                                }

                                if (existing_property == null)
                                {
                                    Properties.Add(new Something
                                    {
                                        Name = property.Name,
                                        Value = property.Value
                                    });
                                }
                                else
                                {
                                    existing_property.Value += property.Value;
                                }
                            }
                        }
                        break;
                    }
                }

                for (int i = 0; i < Properties.Count; i++)
                {
                    Something property = Properties[i];

                    if (!property.Name.Contains("Cost"))
                    {
                        if (property.Name.Contains("Area") ||
                            property.Name.Contains("Death") ||
                            property.Name.Contains("Status") ||
                            property.Name.Contains("Drain") ||
                            property.Name.Contains("Resist") ||
                            property.Name.Contains("Haste") ||
                            property.Name.Contains("Dodge") ||
                            property.Name.Contains("Pierce") ||
                            property.Name.Contains("Counter") ||
                            property.Name.Contains("Disarm"))
                        {
                            if (!property.Name.Contains("Chance"))
                            {
                                equipment.Text += property.Name + " Chance: " + property.Value + "%";
                            }
                            else
                            {
                                equipment.Text += property.Name + ": " + property.Value + "%";
                            }
                        }
                        else if (property.Name.Contains("XP"))
                        {
                            equipment.Text += "XP: " + property.Value + "/" + property.Max_Value;
                        }
                        else if (property.Name.Contains("Level"))
                        {
                            equipment.Text += "Level: " + property.Value + "/" + property.Max_Value;
                        }
                        else
                        {
                            equipment.Text += property.Name + ": " + property.Value;
                        }

                        equipment.Text += "\n";
                        equipment.Region.Height += (Main.Game.MenuSize.Y / 2);
                    }
                }

                for (int i = 0; i < Properties.Count; i++)
                {
                    Something property = Properties[i];

                    if (property.Name.Contains("Cost"))
                    {
                        equipment.Text += property.Name + ": " + property.Value;
                        break;
                    }
                }
            }
        }

        private void ResizeGrid()
        {
            ResetPos();

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture grid = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Region = new Region(starting_X + (width * x), starting_Y + (height * y), width, height);
                        grid.Location = new Location(x, y + Top, 0);
                    }
                }
            }

            int X = starting_X + (width * 2);
            int Y = starting_Y - height;

            GetButton("Helms").Region = new Region(X + width, Y, width, height);
            GetButton("Armors").Region = new Region(X + (width * 2), Y, width, height);
            GetButton("Shields").Region = new Region(X + (width * 3), Y, width, height);
            GetButton("Weapons").Region = new Region(X + (width * 4), Y, width, height);

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);
        }

        private void ResizeInventory()
        {
            ResizeGrid();

            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                foreach (Item item in inventory.Items)
                {
                    item.Icon_Visible = false;
                }

                foreach (Item item in ItemList)
                {
                    foreach (Picture grid in GridList)
                    {
                        if (item.Location.X == grid.Location.X &&
                            item.Location.Y == grid.Location.Y)
                        {
                            item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                            item.Icon_Visible = true;
                            break;
                        }
                    }
                }

                DisplayArrows();
            }
        }

        private void DisplayArrows()
        {
            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                Picture arrow_down = GetPicture("Arrow_Down");

                bool down_visible = true;
                int bottom_row = 9 + Top;

                Item last_item = null;
                if (ItemList.Count > 0)
                {
                    last_item = ItemList[ItemList.Count - 1];
                }

                if (last_item != null)
                {
                    if (last_item.Icon_Visible &&
                       (last_item.Icon_Region.Y <= arrow_down.Region.Y))
                    {
                        down_visible = false;
                    }
                    else if (!last_item.Icon_Visible &&
                              last_item.Location.Y <= bottom_row)
                    {
                        down_visible = false;
                    }
                }
                else
                {
                    down_visible = false;
                }

                arrow_down.Visible = down_visible;
            }

            Picture arrow_up = GetPicture("Arrow_Up");
            if (Top == 0)
            {
                arrow_up.Visible = false;
            }
            else
            {
                arrow_up.Visible = true;
            }
        }

        public override void Load()
        {
            LoadControls();
            LoadCharacter();

            if (!Handler.ViewOnly_Character)
            {
                LoadGrid();

                if (string.IsNullOrEmpty(Handler.ItemFilter))
                {
                    Handler.ItemFilter = "Helms";
                }

                Filter(Handler.ItemFilter);

                ResizeInventory();

                GetButton("Helms").Visible = true;
                GetButton("Armors").Visible = true;
                GetButton("Shields").Visible = true;
                GetButton("Weapons").Visible = true;
            }
            else
            {
                ClearGrid();

                GetButton("Helms").Visible = false;
                GetButton("Armors").Visible = false;
                GetButton("Shields").Visible = false;
                GetButton("Weapons").Visible = false;
            }

            Resize(Main.Game.Resolution);
        }

        private void ClearGrid()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture existing = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                    if (existing != null)
                    {
                        Pictures.Remove(existing);

                        foreach (Picture grid in GridList)
                        {
                            if (grid.ID == existing.ID)
                            {
                                GridList.Remove(existing);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void LoadGrid()
        {
            ClearGrid();
            ResetPos();

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture existing = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                    if (existing != null)
                    {
                        Pictures.Remove(existing);

                        Picture existing_grid = GetGrid(new Vector2(x, y));
                        if (existing_grid != null)
                        {
                            GridList.Remove(existing_grid);
                        }
                    }

                    AddPicture(Handler.GetID(), "x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(starting_X + (width * x), starting_Y + (height * y), width, height), Color.White, true);

                    Picture grid = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Location = new Location(x, y, 0);
                        GridList.Add(grid);
                    }
                }
            }

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);
        }

        public override void Resize(Point point)
        {
            if (ControlsLoaded)
            {
                if (Visible)
                {
                    LoadCharacter();
                    ResizeInventory();
                }

                GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                GetButton("Back").Region = new Region(0, 0, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

                GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
                GetLabel("Examine").Region = new Region(0, 0, 0, 0);
            }
        }

        #endregion
    }
}
