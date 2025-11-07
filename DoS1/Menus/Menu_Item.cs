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
    public class Menu_Item : Menu
    {
        #region Variables

        bool ControlsLoaded;

        int Top;
        List<Picture> GridList = new List<Picture>();
        List<Item> ItemList = new List<Item>();

        bool moving;
        bool attachment;
        int slot;
        Rectangle starting_pos;

        int width;
        int height;
        int starting_Y;
        int starting_X;

        Item selectedItem = null;
        Item movingItem = null;

        #endregion

        #region Constructors

        public Menu_Item(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Item";
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
                UpdateControls();

                if (Handler.StoryStep == 33 ||
                    Handler.StoryStep == 34)
                {
                    GameUtil.Alert_Story();
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
                    if (!picture.Name.Contains("Link") &&
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

                if (selectedItem != null)
                {
                    if (selectedItem.Attachments != null)
                    {
                        foreach (Item item in selectedItem.Attachments)
                        {
                            if (item.Icon_Visible)
                            {
                                item.Draw(spriteBatch, Main.Game.Resolution, Color.White);
                            }
                        }
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name.Contains("Link"))
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    if (label.Name != "Examine")
                    {
                        label.Draw(spriteBatch);
                    }
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

                foreach (Label label in Labels)
                {
                    if (label.Name == "Examine")
                    {
                        label.Draw(spriteBatch);
                        break;
                    }
                }

                if (movingItem != null)
                {
                    movingItem.Draw(spriteBatch, Main.Game.Resolution, Color.White);
                }
            }
        }

        private void UpdateControls()
        {
            bool found_slot = HoveringSlot();

            bool found_grid = false;
            if (!Handler.ViewOnly_Item)
            {
                found_grid = HoveringGrid();
            }

            bool found_button = false;
            if (!moving)
            {
                found_button = HoveringButton();
            }

            bool found_item = false;
            if (!moving)
            {
                found_item = HoveringItem();
            }

            if (!found_grid &&
                !found_slot)
            {
                GetPicture("Highlight").Visible = false;
            }

            if (!found_button &&
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

            if (!found_button &&
                !found_grid &&
                !found_item &&
                !found_slot &&
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
                    if (item.Icon_Visible &&
                        InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                    {
                        found = true;

                        ExamineItem(item);

                        if (InputManager.Mouse_LB_Held &&
                            InputManager.Mouse.Moved &&
                            !Handler.ViewOnly_Item)
                        {
                            found = false;
                            attachment = false;

                            starting_pos = item.Icon_Region.ToRectangle;

                            moving = true;
                            movingItem = item;

                            break;
                        }

                        break;
                    }
                }
            }

            if (!found)
            {
                if (selectedItem?.Attachments != null)
                {
                    foreach (Item item in selectedItem.Attachments)
                    {
                        if (InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(item);

                            if (InputManager.Mouse_LB_Held &&
                                InputManager.Mouse.Moved &&
                                !Handler.ViewOnly_Item)
                            {
                                found = false;
                                attachment = true;

                                starting_pos = item.Icon_Region.ToRectangle;

                                moving = true;
                                movingItem = item;

                                break;
                            }

                            break;
                        }
                    }
                }
            }

            return found;
        }

        private bool HoveringSlot()
        {
            bool found = false;

            if (selectedItem != null)
            {
                Something slots = selectedItem.GetProperty("Rune Slots");
                if (slots != null)
                {
                    for (int i = 0; i < slots.Value; i++)
                    {
                        Picture picture = GetPicture("Slot" + i.ToString());
                        if (picture != null)
                        {
                            if (InputManager.MouseWithin(picture.Region.ToRectangle))
                            {
                                found = true;
                                slot = i;

                                Picture highlight = GetPicture("Highlight");
                                highlight.Region = picture.Region;
                                highlight.Visible = true;
                            }
                        }
                    }
                }
            }
            
            return found;
        }

        private void MoveItem()
        {
            if (movingItem != null)
            {
                bool found_grid = HoveringGrid();
                bool found_slot = HoveringSlot();

                if (InputManager.Mouse_LB_Held)
                {
                    movingItem.Icon_Region = new Region(InputManager.Mouse.X - (width / 2), InputManager.Mouse.Y - (height / 2), width, height);
                }
                else
                {
                    moving = false;

                    bool reset = true;

                    if (attachment)
                    {
                        if (found_grid)
                        {
                            reset = false;
                            Detach(movingItem);
                            movingItem = null;
                        }
                        else
                        {
                            if (found_slot)
                            {
                                reset = false;
                                Swap(movingItem);
                                movingItem = null;
                            }
                        }
                    }
                    else
                    {
                        if (found_slot)
                        {
                            reset = false;
                            Attach(movingItem);
                            movingItem = null;
                        }
                    }

                    if (reset)
                    {
                        movingItem = null;
                    }

                    Filter("Runes");
                    ResizeInventory();
                    ResizeSlots();
                }

                if (!found_grid &&
                    !found_slot)
                {
                    GetPicture("Highlight").Visible = false;
                }
            }
        }

        private void Attach(Item item)
        {
            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                AssetManager.PlaySound_Random("Equip");

                foreach (Item attachment in selectedItem.Attachments)
                {
                    if (attachment.Location.X == slot)
                    {
                        inventory.Items.Add(attachment);
                        selectedItem.Attachments.Remove(attachment);
                        break;
                    }
                }

                inventory.Items.Remove(item);
                selectedItem.Attachments.Add(item);

                item.Location.X = slot;

                InventoryUtil.UpdateItem(selectedItem);

                if (Handler.StoryStep == 33)
                {
                    Handler.StoryStep++;
                }
            }
        }

        private void Detach(Item item)
        {
            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                inventory.Items.Add(item);
                selectedItem.Attachments.Remove(item);

                InventoryUtil.UpdateItem(selectedItem);
            }
        }

        private void Swap(Item item)
        {
            AssetManager.PlaySound_Random("Equip");

            foreach (Item attachment in selectedItem.Attachments)
            {
                if (attachment.Location.X == slot)
                {
                    attachment.Location.X = item.Location.X;
                    break;
                }
            }

            item.Location.X = slot;

            InventoryUtil.UpdateItem(selectedItem);
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back" &&
                Handler.StoryStep >= 34)
            {
                Back();
            }
        }

        private void Back()
        {
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            if (Handler.StoryStep == 34)
            {
                MenuManager.GetMenu("Alerts").Visible = false;
                Handler.StoryStep++;
            }

            MenuManager.ChangeMenu_Previous();
        }

        private void ResetPos()
        {
            width = Main.Game.MenuSize.X;
            height = Main.Game.MenuSize.Y;
            starting_Y = (Main.Game.ScreenHeight / 2) - (height * 5);

            if (!Handler.ViewOnly_Item)
            {
                starting_X = (Main.Game.ScreenWidth / 2) + width;
            }
            else
            {
                starting_X = (Main.Game.ScreenWidth / 2) + (width * 9) + (width / 2);
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

            for (int i = 0; i < item.Properties.Count; i++)
            {
                Something property = item.Properties[i];

                if (property.Name.Contains("RP"))
                {
                    text += "RP: " + property.Value + "/" + property.Max_Value;
                }
                else if (property.Name.Contains("Level"))
                {
                    text += "Level: " + property.Value + "/" + property.Max_Value;
                }
                else
                {
                    text += property.Name + ": " + property.Value;
                }

                if (i < item.Properties.Count - 1)
                {
                    text += "\n";
                    height += (Main.Game.MenuSize.Y / 2);
                }
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

        private void Filter(string filter)
        {
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

                ItemList = InventoryUtil.SortItems(ItemList, null);

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

        private void GetSelectedItem()
        {
            selectedItem = null;

            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                foreach (Item existing in inventory.Items)
                {
                    if (existing.ID == Handler.Selected_Item)
                    {
                        selectedItem = existing;
                        break;
                    }
                }
            }

            if (selectedItem == null)
            {
                Character character = null;

                Army ally_army = CharacterManager.GetArmy("Ally");
                if (ally_army != null)
                {
                    foreach (Squad squad in ally_army.Squads)
                    {
                        character = squad.GetCharacter(Handler.Selected_Character);
                        if (character != null)
                        {
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
                                break;
                            }
                        }
                    }
                }

                if (character != null)
                {
                    foreach (Item existing in character.Inventory.Items)
                    {
                        if (existing.ID == Handler.Selected_Item)
                        {
                            selectedItem = existing;
                            break;
                        }
                    }
                }
            }

            if (selectedItem != null)
            {
                if (selectedItem.Attachments == null)
                {
                    selectedItem.Attachments = new List<Item>();
                }

                Picture selectedItemImage = GetPicture("SelectedItem");
                selectedItemImage.Region = new Region(starting_X - (width * 12), starting_Y, width * 5, height * 5);
                selectedItemImage.Texture = selectedItem.Icon;
                selectedItemImage.Image = new Rectangle(0, 0, selectedItemImage.Texture.Width, selectedItemImage.Texture.Height);

                GetPicture("SelectedItem_Spot").Region = selectedItemImage.Region;

                Label item_name = GetLabel("Item_Name");
                item_name.Region = new Region(starting_X - (width * 12), starting_Y - height, width * 5, height);
                item_name.Text = selectedItem.Name;

                if (selectedItem.Type == "Weapon" &&
                    InventoryUtil.Weapon_Is2H(selectedItem))
                {
                    item_name.Text = selectedItem.Name + " (2H)";
                }

                int Y = starting_Y + (height * 6) + (height / 2);
                GetLabel("Properties").Region = new Region(starting_X - (width * 12), Y, width * 7, height);
                GetPicture("Properties_Underline").Region = new Region(starting_X - (width * 12), Y + (height / 2), width * 7, height);

                Label item_properties = GetLabel("Item_Properties");
                item_properties.Region = new Region(starting_X - (width * 12), Y + height, width * 7, height);
                item_properties.Text = "";

                if (selectedItem.Type == "Weapon")
                {
                    List<Something> properties = new List<Something>();

                    //List damage properties first
                    for (int i = 0; i < selectedItem.Properties.Count; i++)
                    {
                        Something property = selectedItem.Properties[i];
                        if (property.Name.Contains("Damage"))
                        {
                            properties.Add(property);
                        }
                    }

                    //List non-damage properties
                    for (int i = 0; i < selectedItem.Properties.Count; i++)
                    {
                        Something property = selectedItem.Properties[i];
                        if (!property.Name.Contains("Damage") &&
                            !property.Name.Contains("Slots") &&
                            !property.Name.Contains("Cost"))
                        {
                            properties.Add(property);
                        }
                    }

                    //List cost last
                    for (int i = 0; i < selectedItem.Properties.Count; i++)
                    {
                        Something property = selectedItem.Properties[i];
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
                                item_properties.Text += property.Name + " Chance: " + property.Value + "%";
                            }
                            else
                            {
                                item_properties.Text += property.Name + ": " + property.Value + "%";
                            }
                        }
                        else
                        {
                            item_properties.Text += property.Name + ": " + property.Value;
                        }

                        if (i < properties.Count - 1)
                        {
                            item_properties.Text += "\n";
                            item_properties.Region.Height += (Main.Game.MenuSize.Y / 2);
                        }
                    }

                    properties.Clear();
                }
                else
                {
                    List<Something> properties = new List<Something>();

                    //List defense properties first
                    for (int i = 0; i < selectedItem.Properties.Count; i++)
                    {
                        Something property = selectedItem.Properties[i];
                        if (property.Name.Contains("Defense"))
                        {
                            properties.Add(property);
                        }
                    }

                    //List non-defense properties
                    for (int i = 0; i < selectedItem.Properties.Count; i++)
                    {
                        Something property = selectedItem.Properties[i];
                        if (!property.Name.Contains("Defense") &&
                            !property.Name.Contains("Slots") &&
                            !property.Name.Contains("Cost"))
                        {
                            properties.Add(property);
                        }
                    }

                    //List cost last
                    for (int i = 0; i < selectedItem.Properties.Count; i++)
                    {
                        Something property = selectedItem.Properties[i];
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
                                item_properties.Text += property.Name + " Chance: " + property.Value + "%";
                            }
                            else
                            {
                                item_properties.Text += property.Name + ": " + property.Value + "%";
                            }
                        }
                        else
                        {
                            item_properties.Text += property.Name + ": " + property.Value;
                        }

                        if (i < properties.Count - 1)
                        {
                            item_properties.Text += "\n";
                            item_properties.Region.Height += (Main.Game.MenuSize.Y / 2);
                        }
                    }

                    properties.Clear();
                }

                LoadSlots();
            }
        }
        
        private void ClearSlots()
        {
            for (int i = 0; i < Pictures.Count; i++)
            {
                Picture picture = Pictures[i];
                if (picture.Name.Contains("Slot") ||
                    picture.Name.Contains("Link"))
                {
                    Pictures.Remove(picture);
                    i--;
                }
            }
        }

        private void LoadSlots()
        {
            ClearSlots();

            Something slots = selectedItem.GetProperty("Rune Slots");
            if (slots != null)
            {
                Picture item = GetPicture("SelectedItem");
                int X = (int)item.Region.X;
                int Y = (int)item.Region.Y + (height * 5);

                bool add_link = false;

                for (int i = 0; i < slots.Value; i++)
                {
                    AddPicture(Handler.GetID(), "Slot" + i.ToString(), AssetManager.Textures["Rune"],
                        new Region(X, Y, width, height), Color.White * 0.8f, true);

                    if (i <= slots.Value - 1)
                    {
                        if (!add_link)
                        {
                            add_link = true;
                        }
                        else
                        {
                            AddPicture(Handler.GetID(), "Link" + i.ToString(), AssetManager.Textures["Link"],
                                new Region(X + (width / 2), Y, width, height), Color.White * 0.8f, true);
                            add_link = false;
                        }
                    }

                    X += width;

                    if (selectedItem.Attachments != null)
                    {
                        foreach (Item rune in selectedItem.Attachments)
                        {
                            if (rune.Location.X == i)
                            {
                                rune.Icon_Region = GetPicture("Slot" + i.ToString()).Region;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void ResizeSlots()
        {
            ResetPos();
            GetSelectedItem();

            Something slots = selectedItem.GetProperty("Rune Slots");
            if (slots != null)
            {
                Picture item = GetPicture("SelectedItem");
                int X = (int)item.Region.X;
                int Y = (int)item.Region.Y + (height * 5);

                bool add_link = false;

                for (int i = 0; i < slots.Value; i++)
                {
                    Picture slot = GetPicture("Slot" + i.ToString());
                    if (slot != null)
                    {
                        slot.Region = new Region(X, Y, width, height);

                        if (i <= slots.Value - 1)
                        {
                            if (!add_link)
                            {
                                add_link = true;
                            }
                            else
                            {
                                Picture link = GetPicture("Link" + i.ToString());
                                if (link != null)
                                {
                                    link.Region = new Region(X - (width / 2), Y, width, height);
                                }

                                add_link = false;
                            }
                        }

                        X += width;

                        if (selectedItem.Attachments != null)
                        {
                            foreach (Item rune in selectedItem.Attachments)
                            {
                                if (rune.Location.X == i)
                                {
                                    rune.Icon_Region = slot.Region;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadControls()
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddPicture(Handler.GetID(), "SelectedItem_Spot", AssetManager.Textures["Grid"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "SelectedItem", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White, true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Runes", "Runes", Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Item_Name", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Item_Properties", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Properties", "Properties", Color.White, new Region(0, 0, 0, 0), true);
            AddPicture(Handler.GetID(), "Properties_Underline", AssetManager.Textures["Underline"], new Region(0, 0, 0, 0), Color.White, true);

            AddPicture(Handler.GetID(), "Arrow_Up", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Arrow_Down", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, false);

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

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            ControlsLoaded = true;
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

            GetLabel("Runes").Region = new Region(starting_X, starting_Y - height, width * 10, height);

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

        public override void Load()
        {
            LoadControls();

            if (Handler.ViewOnly_Item)
            {
                ClearGrid();
                GetLabel("Runes").Visible = false;
            }
            else
            {
                LoadGrid();
                Filter("Runes");
                ResizeInventory();
                GetLabel("Runes").Visible = true;
            }

            ResizeSlots();
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (ControlsLoaded)
            {
                GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

                ResetPos();

                if (Visible)
                {
                    ResizeSlots();
                    ResizeInventory();
                }

                GetButton("Back").Region = new Region(0, 0, width, height);

                GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
                GetLabel("Examine").Region = new Region(0, 0, 0, 0);
            }
        }

        #endregion
    }
}
