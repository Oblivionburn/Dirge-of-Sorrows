using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Controls;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Inventories;
using OP_Engine.Inputs;
using OP_Engine.Time;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Market : Menu
    {
        #region Variables

        int Top;
        List<Picture> GridList = new List<Picture>();
        List<Item> ItemList = new List<Item>();

        int Top_Market;
        List<Picture> GridList_Market = new List<Picture>();
        List<Item> ItemList_Market = new List<Item>();

        int width;
        int height;
        int starting_Y;
        int starting_X;
        int other_starting_x;

        string current_filter = "Helms";

        #endregion

        #region Constructors

        public Menu_Market(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Market";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible ||
                Active)
            {
                if (Handler.StoryStep >= 12)
                {
                    UpdateControls();
                }

                if (Handler.StoryStep >= 10 &&
                    Handler.StoryStep <= 13)
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

                Inventory ally_inventory = InventoryManager.GetInventory("Ally");
                if (ally_inventory != null)
                {
                    foreach (Item item in ally_inventory.Items)
                    {
                        if (item.Icon_Visible)
                        {
                            item.Draw(spriteBatch, Main.Game.Resolution, Color.White);
                        }
                    }
                }

                if (Handler.TradingMarket != null)
                {
                    foreach (Item item in Handler.TradingMarket.Items)
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

            if (InputManager.KeyPressed("Esc"))
            {
                Back();
            }
        }

        private bool HoveringButton()
        {
            bool found = false;

            if (Handler.StoryStep > 12)
            {
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

                    break;
                }
            }

            if (!found)
            {
                foreach (Picture grid in GridList_Market)
                {
                    if (InputManager.MouseWithin(grid.Region.ToRectangle))
                    {
                        found = true;

                        Picture highlight = GetPicture("Highlight");
                        highlight.Region = grid.Region;
                        highlight.Visible = true;

                        if (InputManager.Mouse_ScrolledDown)
                        {
                            if (Handler.TradingMarket != null)
                            {
                                Item last_item = null;
                                if (ItemList_Market.Count > 0)
                                {
                                    last_item = ItemList_Market[ItemList_Market.Count - 1];
                                }

                                if (last_item != null)
                                {
                                    Top_Market++;

                                    if (Top_Market > last_item.Location.Y)
                                    {
                                        Top_Market = (int)last_item.Location.Y;
                                    }
                                }

                                ResizeMarket();
                            }
                        }
                        else if (InputManager.Mouse_ScrolledUp)
                        {
                            Top_Market--;
                            if (Top_Market <= 0)
                            {
                                Top_Market = 0;
                            }

                            ResizeMarket();
                        }

                        break;
                    }
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

                            if (InputManager.Mouse_RB_Pressed)
                            {
                                SellItem(item);
                            }

                            break;
                        }
                    }
                }
            }

            if (!found &&
                Handler.TradingMarket != null)
            {
                foreach (Item item in Handler.TradingMarket.Items)
                {
                    if (item.Icon_Visible)
                    {
                        if (InputManager.MouseWithin(item.Icon_Region.ToRectangle))
                        {
                            found = true;

                            ExamineItem(item);

                            if (InputManager.Mouse_RB_Pressed)
                            {
                                BuyItem(item);
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
                if (Handler.StoryStep > 13)
                {
                    current_filter = button.Name;
                    Filter();
                }
            }
        }

        private void Back()
        {
            Inventory inventory = InventoryManager.GetInventory("Ally");
            if (inventory != null)
            {
                foreach (Item item in inventory.Items)
                {
                    item.Icon_Visible = false;
                }
            }

            TimeManager.Paused = false;
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            if (Handler.StoryStep == 13)
            {
                MenuManager.GetMenu("Alerts").Visible = false;
                Handler.StoryStep++;
            }

            MenuManager.ChangeMenu_Previous();
            GameUtil.Toggle_Pause(false);
        }

        private void BuyItem(Item item)
        {
            if (Handler.Gold >= item.Buy_Price)
            {
                if ((Handler.StoryStep == 12 && item.Name == "Cloth Helm") ||
                    Handler.StoryStep != 12)
                {
                    AssetManager.PlaySound_Random("Purchase");

                    Inventory inventory = InventoryManager.GetInventory("Ally");
                    inventory.Items.Add(item);

                    Handler.TradingMarket.Items.Remove(item);

                    Handler.Gold -= (int)item.Buy_Price;
                    GetLabel("Gold").Text = "Gold: " + Handler.Gold;

                    if (Handler.StoryStep == 12)
                    {
                        MenuManager.GetMenu("Alerts").Visible = false;
                        Handler.StoryStep++;
                    }

                    Load();
                }
            }
        }

        private void SellItem(Item item)
        {
            AssetManager.PlaySound_Random("Purchase");

            Inventory inventory = InventoryManager.GetInventory("Ally");
            inventory.Items.Remove(item);

            int count = item.Attachments.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    Item attachment = item.Attachments[i];

                    inventory.Items.Add(attachment);

                    item.Attachments.Remove(attachment);
                    i--;
                }
            }

            Handler.TradingMarket.Items.Add(item);

            Handler.Gold += (int)item.Buy_Price;
            GetLabel("Gold").Text = "Gold: " + Handler.Gold;

            Load();
        }

        private void ResetPos()
        {
            width = Main.Game.MenuSize.X;
            height = Main.Game.MenuSize.Y;
            starting_Y = (Main.Game.ScreenHeight / 2) - (height * 5);
            starting_X = (Main.Game.ScreenWidth / 2) - (width / 2) - (width * 10);
            other_starting_x = (Main.Game.ScreenWidth / 2) + (width / 2);
        }

        private void ExamineItem(Item item)
        {
            int width = (Main.Game.MenuSize.X * 5) + (Main.Game.MenuSize.X / 2);
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

                    if (!string.IsNullOrEmpty(item.Description))
                    {
                        text += item.Description + "\n";
                        height += Main.Game.MenuSize.Y + (Main.Game.MenuSize.Y / 2);
                    }
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

            text += "\n\nPrice: " + item.Buy_Price;
            height += Main.Game.MenuSize.Y + (Main.Game.MenuSize.Y / 2);

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

        private void Filter()
        {
            Top = 0;
            Top_Market = 0;

            ResizeGrids();

            ItemList.Clear();
            ItemList_Market.Clear();

            string type = current_filter.Substring(0, current_filter.Length - 1);

            string sort_type = "Defense";
            if (current_filter == "Weapons")
            {
                sort_type = "Damage";
            }

            GetPicture("Arrow_Up").Visible = false;
            GetPicture("Arrow_Down").Visible = false;
            GetPicture("Arrow_Up_Market").Visible = false;
            GetPicture("Arrow_Down_Market").Visible = false;

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

            if (Handler.TradingMarket != null)
            {
                foreach (Item item in Handler.TradingMarket.Items)
                {
                    item.Icon_Visible = false;
                }

                foreach (Item item in Handler.TradingMarket.Items)
                {
                    if (item.Type == type)
                    {
                        ItemList_Market.Add(item);
                    }
                }

                ItemList_Market = InventoryUtil.SortItems(ItemList_Market, sort_type);

                for (int i = 0; i < ItemList_Market.Count; i++)
                {
                    Item item = ItemList_Market[i];

                    if (i < GridList_Market.Count)
                    {
                        Picture grid = GridList_Market[i];
                        item.Icon_Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                        item.Location = new Location(grid.Location.X, grid.Location.Y, 0);
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

            AddPicture(Handler.GetID(), "Arrow_Up_Market", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Arrow_Down_Market", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gold", "Gold: 0", Color.Gold, new Region(0, 0, 0, 0), true);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Exit Market",
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

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Runes",
                hover_text = "Runes",
                texture = AssetManager.Textures["Button_Runes"],
                texture_highlight = AssetManager.Textures["Button_Runes_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        private void ResizeGrids()
        {
            ResetPos();

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture grid = GetPicture("Inventory,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Region = new Region(starting_X + (width * x), starting_Y + (height * y), width, height);
                        grid.Location = new Location(x, y + Top, 0);
                    }

                    grid = GetPicture("Market,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Region = new Region(other_starting_x + (width * x), starting_Y + (height * y), width, height);
                        grid.Location = new Location(x, y + Top, 0);
                    }
                }
            }

            GetLabel("Gold").Region = new Region(starting_X, starting_Y - height, width * 10, height);

            int X = (Main.Game.ScreenWidth / 2) - (width / 2);
            int Y = starting_Y - (height * 2);
            GetButton("Helms").Region = new Region(X - (width * 2), Y, width, height);
            GetButton("Armors").Region = new Region(X - width, Y, width, height);
            GetButton("Shields").Region = new Region(X, Y, width, height);
            GetButton("Weapons").Region = new Region(X + width, Y, width, height);
            GetButton("Runes").Region = new Region(X + (width * 2), Y, width, height);

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);

            GetPicture("Arrow_Up_Market").Region = new Region(other_starting_x + (width * 10), starting_Y, width, height);
            GetPicture("Arrow_Down_Market").Region = new Region(other_starting_x + (width * 10), starting_Y + (height * 9), width, height);
        }

        private void ResizeInventory()
        {
            ResizeGrids();

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

                DisplayArrows_Inventory();
            }
        }

        private void ResizeMarket()
        {
            ResizeGrids();

            if (Handler.TradingMarket != null)
            {
                foreach (Item item in Handler.TradingMarket.Items)
                {
                    item.Icon_Visible = false;
                }

                foreach (Item item in ItemList_Market)
                {
                    foreach (Picture grid in GridList_Market)
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

                DisplayArrows_Market();
            }
        }

        private void DisplayArrows_Inventory()
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

        private void DisplayArrows_Market()
        {
            if (Handler.TradingMarket != null)
            {
                Picture arrow_down = GetPicture("Arrow_Down_Market");

                bool down_visible = true;
                int bottom_row = 9 + Top_Market;

                Item last_item = null;
                if (ItemList_Market.Count > 0)
                {
                    last_item = ItemList_Market[ItemList_Market.Count - 1];
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

            Picture arrow_up = GetPicture("Arrow_Up_Market");
            if (Top_Market == 0)
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
            LoadGrids();
            Filter();
            ResizeInventory();
            ResizeMarket();
        }

        private void ClearGrids()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture inventory_grid = GetPicture("Inventory,x:" + x.ToString() + ",y:" + y.ToString());
                    if (inventory_grid != null)
                    {
                        Pictures.Remove(inventory_grid);
                        GridList.Remove(inventory_grid);
                    }

                    Picture market_grid = GetPicture("Market,x:" + x.ToString() + ",y:" + y.ToString());
                    if (market_grid != null)
                    {
                        Pictures.Remove(market_grid);
                        GridList_Market.Remove(market_grid);
                    }
                }
            }
        }

        private void LoadGrids()
        {
            ClearGrids();
            ResetPos();

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    AddPicture(Handler.GetID(), "Inventory,x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(starting_X + (width * x), starting_Y + (height * y), width, height), Color.White, true);

                    Picture grid = GetPicture("Inventory,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Location = new Location(x, y, 0);
                        GridList.Add(grid);
                    }

                    AddPicture(Handler.GetID(), "Market,x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(other_starting_x + (width * x), starting_Y + (height * y), width, height), Color.White, true);

                    grid = GetPicture("Market,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Location = new Location(x, y, 0);
                        GridList_Market.Add(grid);
                    }
                }
            }

            GetLabel("Gold").Text = "Gold: " + Handler.Gold;

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);

            GetPicture("Arrow_Up_Market").Region = new Region(other_starting_x + (width * 10), starting_Y, width, height);
            GetPicture("Arrow_Down_Market").Region = new Region(other_starting_x + (width * 10), starting_Y + (height * 9), width, height);
        }

        public override void Resize(Point point)
        {
            ResetPos();

            if (Visible)
            {
                ResizeInventory();
                ResizeMarket();
            }

            GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            GetLabel("Gold").Region = new Region(starting_X, starting_Y - height, width * 10, height);

            Button back = GetButton("Back");
            back.Region = new Region((Main.Game.ScreenWidth / 2) - (width / 2), starting_Y + (height * 11) + (height / 2), width, height);

            GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            int X = (Main.Game.ScreenWidth / 2) - (width / 2);
            int Y = starting_Y - (height * 2);
            GetButton("Helms").Region = new Region(X - (width * 2), Y, width, height);
            GetButton("Armors").Region = new Region(X - width, Y, width, height);
            GetButton("Shields").Region = new Region(X, Y, width, height);
            GetButton("Weapons").Region = new Region(X + width, Y, width, height);
            GetButton("Runes").Region = new Region(X + (width * 2), Y, width, height);
        }

        #endregion
    }
}
