using DoS1.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Time;
using OP_Engine.Utility;
using System.Collections.Generic;
using System.Linq;

namespace DoS1.Menus
{
    public class Menu_Academy : Menu
    {
        #region Variables

        int Top;
        List<Picture> ReservesList = new List<Picture>();
        List<Character> CharacterList_Reserves = new List<Character>();

        int Top_Academy;
        List<Picture> AcademyList = new List<Picture>();
        List<Character> CharacterList_Academy = new List<Character>();

        int width;
        int height;
        int starting_Y;
        int starting_X;
        int other_starting_x;

        #endregion

        #region Constructors

        public Menu_Academy(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Academy";
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

                Army reserve_army = CharacterManager.GetArmy("Reserves");
                if (reserve_army != null &&
                    reserve_army.Squads.Any())
                {
                    Squad squad = reserve_army.Squads[0];
                    foreach (Character character in squad.Characters)
                    {
                        if (character.Visible)
                        {
                            CharacterUtil.UpdateGear(character);
                        }
                    }
                }

                if (Handler.TradingAcademy != null)
                {
                    foreach (Character character in Handler.TradingAcademy.Characters)
                    {
                        if (character.Visible)
                        {
                            CharacterUtil.UpdateGear(character);
                        }
                    }
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

                Army reserve_army = CharacterManager.GetArmy("Reserves");
                if (reserve_army != null)
                {
                    foreach (Squad squad in reserve_army.Squads)
                    {
                        foreach (Character character in squad.Characters)
                        {
                            if (character.Visible)
                            {
                                CharacterUtil.DrawCharacter(spriteBatch, character, Color.White);
                            }
                        }
                    }
                }

                if (Handler.TradingAcademy != null)
                {
                    foreach (Character character in Handler.TradingAcademy.Characters)
                    {
                        if (character.Visible)
                        {
                            CharacterUtil.DrawCharacter(spriteBatch, character, Color.White);
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
            bool found_character = HoveringCharacter();

            if (!found_button &&
                !found_character)
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

            foreach (Picture grid in ReservesList)
            {
                if (InputManager.MouseWithin(grid.Region.ToRectangle))
                {
                    found = true;

                    Picture highlight = GetPicture("Highlight");
                    highlight.Region = grid.Region;
                    highlight.Visible = true;

                    if (InputManager.Mouse_ScrolledDown)
                    {
                        Army army = CharacterManager.GetArmy("Reserves");
                        if (army != null)
                        {
                            Character last_character = null;
                            if (CharacterList_Reserves.Count > 0)
                            {
                                last_character = CharacterList_Reserves[CharacterList_Reserves.Count - 1];
                            }

                            if (last_character != null)
                            {
                                Top++;

                                if (Top > last_character.Formation.Y)
                                {
                                    Top = (int)last_character.Formation.Y;
                                }
                            }

                            ResizeReserves();
                        }
                    }
                    else if (InputManager.Mouse_ScrolledUp)
                    {
                        Top--;
                        if (Top <= 0)
                        {
                            Top = 0;
                        }

                        ResizeReserves();
                    }

                    break;
                }
            }

            if (!found)
            {
                foreach (Picture grid in AcademyList)
                {
                    if (InputManager.MouseWithin(grid.Region.ToRectangle))
                    {
                        found = true;

                        Picture highlight = GetPicture("Highlight");
                        highlight.Region = grid.Region;
                        highlight.Visible = true;

                        if (InputManager.Mouse_ScrolledDown)
                        {
                            if (Handler.TradingAcademy != null)
                            {
                                Character last_character = null;
                                if (CharacterList_Academy.Count > 0)
                                {
                                    last_character = CharacterList_Academy[CharacterList_Academy.Count - 1];
                                }

                                if (last_character != null)
                                {
                                    Top_Academy++;

                                    if (Top_Academy > last_character.Formation.Y)
                                    {
                                        Top_Academy = (int)last_character.Formation.Y;
                                    }
                                }

                                ResizeAcademy();
                            }
                        }
                        else if (InputManager.Mouse_ScrolledUp)
                        {
                            Top_Academy--;
                            if (Top_Academy <= 0)
                            {
                                Top_Academy = 0;
                            }

                            ResizeAcademy();
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

        private bool HoveringCharacter()
        {
            bool found = false;

            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null &&
                army.Squads.Any())
            {
                Squad squad = army.Squads[0];
                foreach (Character character in squad.Characters)
                {
                    if (character.Visible)
                    {
                        if (InputManager.MouseWithin(character.Region.ToRectangle))
                        {
                            found = true;

                            Examine(character.Name);

                            if (InputManager.Mouse_RB_Pressed)
                            {
                                SellReserves(character);
                            }

                            break;
                        }
                    }
                }
            }

            if (!found &&
                Handler.TradingAcademy != null)
            {
                foreach (Character character in Handler.TradingAcademy.Characters)
                {
                    if (character.Visible)
                    {
                        if (InputManager.MouseWithin(character.Region.ToRectangle))
                        {
                            found = true;

                            Examine(character.Name);

                            if (InputManager.Mouse_RB_Pressed)
                            {
                                BuyRecruits(character);
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
        }

        private void Back()
        {
            TimeManager.Paused = false;
            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();
            MenuManager.ChangeMenu_Previous();
            GameUtil.Toggle_Pause(false);
            SceneManager.GetScene("Localmap").Active = true;
        }

        private void BuyRecruits(Character character)
        {
            if (Handler.Gold >= Handler.RecruitPrice)
            {
                AssetManager.PlaySound_Random("Purchase");

                Army reserve_army = CharacterManager.GetArmy("Reserves");
                if (reserve_army != null)
                {
                    Squad squad = reserve_army.Squads[0];
                    squad.AddCharacter(character);
                }

                Handler.TradingAcademy.Characters.Remove(character);

                Handler.Gold -= Handler.RecruitPrice;
                GetLabel("Gold").Text = "Gold: " + Handler.Gold;

                Load();
            }
        }

        private void SellReserves(Character character)
        {
            AssetManager.PlaySound_Random("Purchase");

            Army reserve_army = CharacterManager.GetArmy("Reserves");
            if (reserve_army != null)
            {
                Squad squad = reserve_army.Squads[0];
                squad.Characters.Remove(character);
            }

            Handler.TradingAcademy.AddCharacter(character);

            Handler.Gold += Handler.RecruitPrice;
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

        private void Examine(string text)
        {
            Label examine = GetLabel("Examine");
            examine.Text = text + "\n\nPrice: " + Handler.RecruitPrice;

            int width = Main.Game.MenuSize.X * 5;
            int height = Main.Game.MenuSize.Y * 2;

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

        private void LoadCharacters()
        {
            Top = 0;
            Top_Academy = 0;

            ResizeGrids();

            CharacterList_Reserves.Clear();
            CharacterList_Academy.Clear();

            GetPicture("Arrow_Up").Visible = false;
            GetPicture("Arrow_Down").Visible = false;
            GetPicture("Arrow_Up_Academy").Visible = false;
            GetPicture("Arrow_Down_Academy").Visible = false;

            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null &&
                army.Squads.Any())
            {
                Squad squad = army.Squads[0];
                foreach (Character character in squad.Characters)
                {
                    CharacterList_Reserves.Add(character);
                }

                for (int i = 0; i < CharacterList_Reserves.Count; i++)
                {
                    Character character = CharacterList_Reserves[i];

                    if (i < ReservesList.Count)
                    {
                        Picture grid = ReservesList[i];
                        character.Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                        character.Formation = new Vector2(grid.Location.X, grid.Location.Y);
                        character.Visible = true;
                    }
                }
            }

            if (Handler.TradingAcademy != null)
            {
                foreach (Character character in Handler.TradingAcademy.Characters)
                {
                    character.Visible = false;
                    CharacterList_Academy.Add(character);
                }

                for (int i = 0; i < CharacterList_Academy.Count; i++)
                {
                    Character character = CharacterList_Academy[i];

                    if (i < AcademyList.Count)
                    {
                        Picture grid = AcademyList[i];
                        character.Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                        character.Formation = new Vector2(grid.Location.X, grid.Location.Y);
                        character.Visible = true;
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

            AddPicture(Handler.GetID(), "Arrow_Up_Academy", AssetManager.Textures["ArrowIcon_Up"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Arrow_Down_Academy", AssetManager.Textures["ArrowIcon_Down"], new Region(0, 0, 0, 0), Color.White, false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gold", "Gold: 0", Color.Gold, new Region(0, 0, 0, 0), true);

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Exit Academy",
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

            Resize(Main.Game.Resolution);
        }

        private void ResizeGrids()
        {
            ResetPos();

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture grid = GetPicture("Reserves,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Region = new Region(starting_X + (width * x), starting_Y + (height * y), width, height);
                        grid.Location = new Location(x, y + Top, 0);
                    }

                    grid = GetPicture("Academy,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Region = new Region(other_starting_x + (width * x), starting_Y + (height * y), width, height);
                        grid.Location = new Location(x, y + Top, 0);
                    }
                }
            }

            GetLabel("Gold").Region = new Region(starting_X, starting_Y - height, width * 10, height);

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);

            GetPicture("Arrow_Up_Academy").Region = new Region(other_starting_x + (width * 10), starting_Y, width, height);
            GetPicture("Arrow_Down_Academy").Region = new Region(other_starting_x + (width * 10), starting_Y + (height * 9), width, height);
        }

        private void ResizeReserves()
        {
            ResizeGrids();

            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null)
            {
                Squad squad = army.Squads[0];
                if (squad != null)
                {
                    foreach (Character character in squad.Characters)
                    {
                        character.Visible = false;
                    }

                    foreach (Character character in CharacterList_Reserves)
                    {
                        foreach (Picture grid in ReservesList)
                        {
                            if (character.Formation.X == grid.Location.X &&
                                character.Formation.Y == grid.Location.Y)
                            {
                                character.Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                                character.Visible = true;
                                break;
                            }
                        }
                    }

                    DisplayArrows_Reserves();
                }
            }
        }

        private void ResizeAcademy()
        {
            ResizeGrids();

            if (Handler.TradingAcademy != null)
            {
                foreach (Character character in Handler.TradingAcademy.Characters)
                {
                    character.Visible = false;
                }

                foreach (Character character in CharacterList_Academy)
                {
                    foreach (Picture grid in AcademyList)
                    {
                        if (character.Formation.X == grid.Location.X &&
                            character.Formation.Y == grid.Location.Y)
                        {
                            character.Region = new Region(grid.Region.X, grid.Region.Y, grid.Region.Width, grid.Region.Height);
                            character.Visible = true;
                            break;
                        }
                    }
                }

                DisplayArrows_Academy();
            }
        }

        private void DisplayArrows_Reserves()
        {
            Army army = CharacterManager.GetArmy("Reserves");
            if (army != null)
            {
                Picture arrow_down = GetPicture("Arrow_Down");

                bool down_visible = true;
                int bottom_row = 9 + Top;

                Character last_character = null;
                if (CharacterList_Reserves.Count > 0)
                {
                    last_character = CharacterList_Reserves[CharacterList_Reserves.Count - 1];
                }

                if (last_character != null)
                {
                    if (last_character.Visible &&
                       (last_character.Region.Y <= arrow_down.Region.Y))
                    {
                        down_visible = false;
                    }
                    else if (!last_character.Visible &&
                              last_character.Formation.Y <= bottom_row)
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

        private void DisplayArrows_Academy()
        {
            if (Handler.TradingAcademy != null)
            {
                Picture arrow_down = GetPicture("Arrow_Down_Academy");

                bool down_visible = true;
                int bottom_row = 9 + Top;

                Character last_character = null;
                if (CharacterList_Academy.Count > 0)
                {
                    last_character = CharacterList_Academy[CharacterList_Academy.Count - 1];
                }

                if (last_character != null)
                {
                    if (last_character.Visible &&
                       (last_character.Region.Y <= arrow_down.Region.Y))
                    {
                        down_visible = false;
                    }
                    else if (!last_character.Visible &&
                              last_character.Formation.Y <= bottom_row)
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

            Picture arrow_up = GetPicture("Arrow_Up_Academy");
            if (Top_Academy == 0)
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
            LoadCharacters();
            ResizeReserves();
            ResizeAcademy();
        }

        private void ClearGrids()
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Picture reserves_grid = GetPicture("Reserves,x:" + x.ToString() + ",y:" + y.ToString());
                    if (reserves_grid != null)
                    {
                        Pictures.Remove(reserves_grid);
                        ReservesList.Remove(reserves_grid);
                    }

                    Picture academy_grid = GetPicture("Academy,x:" + x.ToString() + ",y:" + y.ToString());
                    if (academy_grid != null)
                    {
                        Pictures.Remove(academy_grid);
                        AcademyList.Remove(academy_grid);
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
                    AddPicture(Handler.GetID(), "Reserves,x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(starting_X + (width * x), starting_Y + (height * y), width, height), Color.White, true);

                    Picture grid = GetPicture("Reserves,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Location = new Location(x, y, 0);
                        ReservesList.Add(grid);
                    }

                    AddPicture(Handler.GetID(), "Academy,x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                        new Region(other_starting_x + (width * x), starting_Y + (height * y), width, height), Color.White, true);

                    grid = GetPicture("Academy,x:" + x.ToString() + ",y:" + y.ToString());
                    if (grid != null)
                    {
                        grid.Location = new Location(x, y, 0);
                        AcademyList.Add(grid);
                    }
                }
            }

            GetLabel("Gold").Text = "Gold: " + Handler.Gold;

            GetPicture("Arrow_Up").Region = new Region(starting_X - width, starting_Y, width, height);
            GetPicture("Arrow_Down").Region = new Region(starting_X - width, starting_Y + (height * 9), width, height);

            GetPicture("Arrow_Up_Academy").Region = new Region(other_starting_x + (width * 10), starting_Y, width, height);
            GetPicture("Arrow_Down_Academy").Region = new Region(other_starting_x + (width * 10), starting_Y + (height * 9), width, height);
        }

        public override void Resize(Point point)
        {
            ResetPos();

            if (Visible)
            {
                ResizeReserves();
                ResizeAcademy();
            }

            GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);
            GetLabel("Gold").Region = new Region(starting_X, starting_Y - height, width * 10, height);

            Button back = GetButton("Back");
            back.Region = new Region((Main.Game.ScreenWidth / 2) - (width / 2), starting_Y + (height * 11), width, height);

            GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
