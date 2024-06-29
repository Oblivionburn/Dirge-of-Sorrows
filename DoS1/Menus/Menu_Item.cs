using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Inventories;
using OP_Engine.Utility;
using OP_Engine.Characters;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Item : Menu
    {
        #region Variables

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
                else
                {
                    UpdateControls();
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
                            if (item.Icon_Visible &&
                                (movingItem == null ||
                                 item.ID != movingItem.ID))
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

                if (selectedItem != null)
                {
                    if (selectedItem.Attachments != null)
                    {
                        foreach (Item item in selectedItem.Attachments)
                        {
                            if (item.Icon_Visible &&
                                movingItem != null &&
                                item.ID == movingItem.ID)
                            {
                                item.Draw(spriteBatch, Main.Game.Resolution, Color.White);
                                break;
                            }
                        }
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
            bool found_grid = HoveringGrid();
            bool found_item = HoveringItem();
            bool found_slot = HoveringSlot();

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
                            InputManager.Mouse.Moved)
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
                                InputManager.Mouse.Moved)
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
                            Unattach(movingItem);
                            movingItem = null;
                        }
                        else
                        {
                            if (found_slot)
                            {
                                reset = false;
                                movingItem.Location.X = slot;
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
                            movingItem.Location.X = slot;
                            movingItem = null;
                        }
                    }

                    if (reset)
                    {
                        movingItem.Icon_Region = new Region(starting_pos.X, starting_pos.Y, starting_pos.Width, starting_pos.Height);
                    }

                    Filter("Runes");
                    ResizeInventory();
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

                inventory.Items.Remove(item);
                selectedItem.Attachments.Add(item);

                string element = item.Categories[0];
                string effect = "";

                bool damage = InventoryUtil.Element_IsDamage(element);
                if (damage)
                {
                    if (selectedItem.Type == "Weapon")
                    {
                        effect = "Damage";
                    }
                    else if (selectedItem.Type == "Shield" ||
                             selectedItem.Type == "Armor")
                    {
                        effect = "Defense";
                    }
                }

                Something level = item.GetProperty("Level Value");

                Something property = selectedItem.GetProperty(element + " " + effect);
                if (property != null)
                {
                    property.Value += level.Value * Handler.Element_Multiplier;
                }
                else
                {
                    selectedItem.Properties.Add(new Something
                    {
                        Name = element + " " + effect,
                        Type = element,
                        Assignment = effect,
                        Value = level.Value * Handler.Element_Multiplier
                    });
                }

                Something cost = selectedItem.GetProperty("EP Cost");
                if (cost != null)
                {
                    cost.Value += level.Value;
                }
                else
                {
                    selectedItem.Properties.Add(new Something
                    {
                        Name = "EP Cost",
                        Type = "EP",
                        Assignment = "Cost",
                        Value = level.Value
                    });
                }

                selectedItem.Buy_Price += InventoryUtil.GetPrice(item);
            }
        }

        private void Unattach(Item item)
        {
            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                inventory.Items.Add(item);
                selectedItem.Attachments.Remove(item);

                string element = item.Categories[0];
                string effect = "";

                bool damage = InventoryUtil.Element_IsDamage(element);
                if (damage)
                {
                    if (selectedItem.Type == "Weapon")
                    {
                        effect = "Damage";
                    }
                    else if (selectedItem.Type == "Shield" ||
                             selectedItem.Type == "Armor")
                    {
                        effect = "Defense";
                    }
                }

                Something level = item.GetProperty("Level Value");

                Something property = selectedItem.GetProperty(element + " " + effect);
                if (property != null)
                {
                    property.Value -= level.Value * Handler.Element_Multiplier;
                    if (property.Value <= 0)
                    {
                        selectedItem.Properties.Remove(property);
                    }
                }

                Something cost = selectedItem.GetProperty("EP Cost");
                if (cost != null)
                {
                    cost.Value -= level.Value;
                    if (cost.Value <= 0)
                    {
                        selectedItem.Properties.Remove(cost);
                    }
                }

                selectedItem.Buy_Price -= InventoryUtil.GetPrice(item);
            }
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
        }

        private void Back()
        {
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();
            MenuManager.ChangeMenu_Previous();
        }

        private void ResetPos()
        {
            width = Main.Game.MenuSize.X;
            height = Main.Game.MenuSize.Y;
            starting_Y = (Main.Game.ScreenHeight / 2) - (height * 5);
            starting_X = (Main.Game.ScreenWidth / 2) + width;
        }

        private void ExamineItem(Item item)
        {
            int width = (Main.Game.MenuSize.X * 4) + (Main.Game.MenuSize.X / 2);
            if (item.Type == "Rune")
            {
                width = (Main.Game.MenuSize.X * 10) + (Main.Game.MenuSize.X / 2);
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
                text += property.Name + ": " + property.Value;

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

                Army army = CharacterManager.GetArmy("Ally");
                if (army != null)
                {
                    foreach (Squad squad in army.Squads)
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

                Picture item = GetPicture("SelectedItem");
                item.Texture = selectedItem.Icon;
                item.Image = new Rectangle(0, 0, item.Texture.Width, item.Texture.Height);

                AddSlots();
            }
        }

        private void AddSlots()
        {
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

        public override void Load(ContentManager content)
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddPicture(Handler.GetID(), "SelectedItem_Spot", AssetManager.Textures["Grid"], new Region(0, 0, 0, 0), Color.White, true);
            AddPicture(Handler.GetID(), "SelectedItem", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White, true);

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
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
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
            ResizeSlots();

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

            GetSelectedItem();

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
            LoadGrid();
            Filter("Runes");
            ResizeInventory();
        }

        public override void Resize(Point point)
        {
            GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            ResetPos();

            Picture item = GetPicture("SelectedItem");
            item.Region = new Region((Main.Game.ScreenWidth / 2) - (width * 11), starting_Y, width * 5, height * 5);
            GetPicture("SelectedItem_Spot").Region = item.Region;

            if (Visible)
            {
                ResizeInventory();
            }

            GetButton("Back").Region = new Region(0, 0, width, height);

            GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
