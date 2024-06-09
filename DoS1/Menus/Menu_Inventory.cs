using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Inventories;
using OP_Engine.Utility;
using OP_Engine.Time;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Inventory : Menu
    {
        #region Variables

        int Top;
        List<Picture> GridList = new List<Picture>();
        List<Item> ItemList = new List<Item>();

        int width;
        int height;
        int starting_Y;
        int starting_X;

        string current_filter = "Helms";

        #endregion

        #region Constructors

        public Menu_Inventory(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Inventory";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                UpdateControls();

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

                Inventory inventory = InventoryManager.GetInventory("Player");
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
            bool found_grid = HoveringGrid();
            bool found_item = HoveringItem();

            if (!found_button &&
                !found_item)
            {
                GetLabel("Examine").Visible = false;
            }

            if (InputManager.Mouse_ScrolledDown)
            {
                Inventory inventory = InventoryManager.GetInventory("Player");
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

            if (!found)
            {
                GetPicture("Highlight").Visible = false;
            }

            return found;
        }

        private bool HoveringItem()
        {
            bool found = false;

            Inventory inventory = InventoryManager.GetInventory("Player");
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

                            if (InputManager.Mouse_RB_Pressed)
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

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                Back();
            }
            else if (button.Name == "Helms" ||
                     button.Name == "Armors" ||
                     button.Name == "Shields" ||
                     button.Name == "Weapons" ||
                     button.Name == "Runes")
            {
                Filter(button.Name);
            }
        }

        private void Back()
        {
            TimeManager.Paused = false;
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();
            MenuManager.ChangeMenu_Previous();
        }

        private void SelectItem(long id)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.Selected_Item = id;

            MenuManager.ChangeMenu("Item");
        }

        private void ResetPos()
        {
            width = Main.Game.MenuSize.X;
            height = Main.Game.MenuSize.Y;
            starting_Y = (Main.Game.ScreenHeight / 2) - (height * 5);
            starting_X = (Main.Game.ScreenWidth / 2) - (width * 5);
        }

        private void ExamineItem(Item item)
        {
            int width = (Main.Game.MenuSize.X * 4) + (Main.Game.MenuSize.X / 2);
            int height = Main.Game.MenuSize.Y + (Main.Game.MenuSize.Y / 2);

            string text = item.Name + "\n";

            if (item.Type == "Weapon")
            {
                if (item.Categories.Contains("Axe") ||
                    item.Categories.Contains("Bow") ||
                    item.Categories.Contains("Grimoire"))
                {
                    text = item.Name + " (2H)\n";
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

                //List non-damage properties last
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (!property.Name.Contains("Damage"))
                    {
                        properties.Add(property);
                    }
                }

                for (int i = 0; i < properties.Count; i++)
                {
                    Something property = properties[i];

                    text += property.Name + ": " + property.Value;

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

                //List non-defense properties last
                for (int i = 0; i < item.Properties.Count; i++)
                {
                    Something property = item.Properties[i];
                    if (!property.Name.Contains("Defense"))
                    {
                        properties.Add(property);
                    }
                }

                for (int i = 0; i < properties.Count; i++)
                {
                    Something property = properties[i];

                    text += property.Name + ": " + property.Value;

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

        private void Filter(string filter)
        {
            current_filter = filter;

            Top = 0;
            ResizeGrid();

            ItemList.Clear();

            string type = filter.Substring(0, filter.Length - 1);

            GetPicture("Arrow_Up").Visible = false;
            GetPicture("Arrow_Down").Visible = false;

            Inventory inventory = InventoryManager.GetInventory("Player");
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
                        item.Location = new Vector3(grid.Location.X, grid.Location.Y, 0);
                        item.Icon_Visible = true;
                    }
                }
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

            AddPicture(Handler.GetID(), "Arrow_Up", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Arrow_Down", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, false);

            AddButton(Handler.GetID(), "Back", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Back").HoverText = "Back";

            AddButton(Handler.GetID(), "Helms", AssetManager.Textures["Button_Helms"], AssetManager.Textures["Button_Helms_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Helms").HoverText = "Helms";

            AddButton(Handler.GetID(), "Armors", AssetManager.Textures["Button_Armors"], AssetManager.Textures["Button_Armors_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Armors").HoverText = "Armors";

            AddButton(Handler.GetID(), "Shields", AssetManager.Textures["Button_Shields"], AssetManager.Textures["Button_Shields_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Shields").HoverText = "Shields";

            AddButton(Handler.GetID(), "Weapons", AssetManager.Textures["Button_Weapons"], AssetManager.Textures["Button_Weapons_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Weapons").HoverText = "Weapons";

            AddButton(Handler.GetID(), "Runes", AssetManager.Textures["Button_Runes"], AssetManager.Textures["Button_Runes_Hover"], null,
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Runes").HoverText = "Runes";

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
                        grid.Location = new Vector3(x, y + Top, 0);
                    }
                }
            }

            int X = starting_X + (width * 2) - (width / 2);
            int Y = starting_Y - height;

            GetButton("Helms").Region = new Region(X + width, Y, width, height);
            GetButton("Armors").Region = new Region(X + (width * 2), Y, width, height);
            GetButton("Shields").Region = new Region(X + (width * 3), Y, width, height);
            GetButton("Weapons").Region = new Region(X + (width * 4), Y, width, height);
            GetButton("Runes").Region = new Region(X + (width * 5), Y, width, height);

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);
        }

        private void ResizeInventory()
        {
            ResizeGrid();

            Inventory inventory = InventoryManager.GetInventory("Player");
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
            Inventory inventory = InventoryManager.GetInventory("Player");
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
            LoadGrid();
            Filter("Helms");
            ResizeInventory();
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
                    AddPicture(Handler.GetID(), "x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(starting_X + (width * x), starting_Y + (height * y), width, height), Color.White, true);

                    Picture grid = GetPicture("x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Location = new Vector3(x, y, 0);
                        GridList.Add(grid);
                    }
                }
            }

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);
        }

        public override void Resize(Point point)
        {
            if (Visible)
            {
                ResizeInventory();
            }

            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            int X = 0;
            int Y = 0;

            GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            GetButton("Back").Region = new Region(X, Y, width, height);

            GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            ResetPos();

            X = (Main.Game.ScreenWidth / 2) - (width * 3);
            Y = starting_Y - height;
            GetButton("Helms").Region = new Region(X + width, Y, width, height);
            GetButton("Armors").Region = new Region(X + (width * 2), Y, width, height);
            GetButton("Shields").Region = new Region(X + (width * 3), Y, width, height);
            GetButton("Weapons").Region = new Region(X + (width * 4), Y, width, height);
            GetButton("Runes").Region = new Region(X + (width * 5), Y, width, height);
        }

        #endregion
    }
}
