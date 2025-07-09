using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Menus;
using OP_Engine.Inputs;
using OP_Engine.Controls;
using OP_Engine.Characters;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Sounds;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Army : Menu
    {
        #region Variables

        int Top;
        bool Bottom;

        long SelectedSquad;
        List<Squad> SquadList = new List<Squad>();

        #endregion

        #region Constructors

        public Menu_Army(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Army";
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

                Army army = CharacterManager.GetArmy("Ally");
                if (army != null)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        foreach (Character character in squad.Characters)
                        {
                            if (character.Visible)
                            {
                                CharacterUtil.UpdateGear(character);
                            }
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

                Army army = CharacterManager.GetArmy("Ally");
                if (army != null)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        if (squad.Active)
                        {
                            foreach (Character character in squad.Characters)
                            {
                                if (character.Visible)
                                {
                                    Effect effect = AssetManager.Shaders["Grayscale"];
                                    effect.Parameters["percent"].SetValue(0f);

                                    spriteBatch.End();
                                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);

                                    CharacterUtil.DrawCharacter_Grayscale(spriteBatch, character, Color.White);

                                    spriteBatch.End();
                                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
                                }
                            }
                        }
                        else
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

            if (SelectedSquad == 0)
            {
                Army army = CharacterManager.GetArmy("Ally");
                if (army != null)
                {
                    for (int i = 0; i < army.Squads.Count; i++)
                    {
                        Squad squad = army.Squads[i];

                        foreach (Picture picture in Pictures)
                        {
                            if (picture.ID == squad.ID &&
                                picture.Name == squad.Name)
                            {
                                if (InputManager.MouseWithin(picture.Region.ToRectangle))
                                {
                                    found = true;

                                    if (!string.IsNullOrEmpty(squad.Name))
                                    {
                                        GameUtil.Examine(this, squad.Name);
                                    }

                                    Picture highlight = GetPicture("Highlight");
                                    highlight.Region = picture.Region;
                                    highlight.Visible = true;

                                    if (InputManager.Mouse_RB_Pressed) 
                                    {
                                        found = false;
                                        SelectSquad(squad);
                                        break;
                                    }
                                    else if (InputManager.Mouse_LB_Pressed &&
                                             !squad.Active) //Prevent removing deployed squad
                                    {
                                        SelectedSquad = squad.ID;

                                        if (i > 0) //Prevent removing last squad
                                        {
                                            GetButton("Remove").Enabled = true;
                                        }

                                        if (Handler.LocalMap &&
                                            squad.Characters.Any())
                                        {
                                            GetButton("Deploy").Enabled = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!found)
            {
                GetLabel("Examine").Visible = false;

                if (InputManager.Mouse_LB_Pressed ||
                    SelectedSquad == 0)
                {
                    SelectedSquad = 0;
                    GetButton("Remove").Enabled = false;
                    GetButton("Deploy").Enabled = false;
                }
            }

            if (!found &&
                SelectedSquad == 0)
            {
                GetPicture("Highlight").Visible = false;
            }

            if (InputManager.Mouse.ScrolledDown)
            {
                ScrollDown();
            }
            else if (InputManager.Mouse.ScrolledUp)
            {
                ScrollUp();
            }

            if (InputManager.KeyPressed("Esc"))
            {
                Back();
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            if (button.Name == "Back")
            {
                Back();
            }
            else if (button.Name == "Add")
            {
                Army army = CharacterManager.GetArmy("Ally");
                army.Squads.Add(ArmyUtil.NewSquad("Ally"));
                Load();
            }
            else if (button.Name == "Remove")
            {
                RemoveSquad();
                Load();
            }
            else if (button.Name == "Deploy")
            {
                DeploySquad();
            }
        }

        private void Back()
        {
            SelectedSquad = 0;
            TimeManager.Paused = false;
            SoundManager.AmbientPaused = false;

            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            MenuManager.ChangeMenu_Previous();
        }

        private void ScrollDown()
        {
            if (!Bottom)
            {
                Top += (Main.Game.MenuSize.Y / 2);
                ResizeArmy();
                GetPicture("Arrow_Up").Visible = true;
            }
        }

        private void ScrollUp()
        {
            Top -= (Main.Game.MenuSize.Y / 2);
            if (Top <= 0)
            {
                Top = 0;
                GetPicture("Arrow_Up").Visible = false;
            }

            ResizeArmy();
        }

        private void RemoveSquad()
        {
            Army ally_army = CharacterManager.GetArmy("Ally");
            Squad squad = ally_army.GetSquad(SelectedSquad);

            Army army_reserves = CharacterManager.GetArmy("Reserves");
            Squad reserves = army_reserves.Squads[0];

            for (int i = 0; i < squad.Characters.Count; i++)
            {
                Character character = squad.Characters[i];

                int x = 0;
                int y = 0;

                Character last_character = ArmyUtil.Get_LastCharacter(reserves);
                if (last_character != null)
                {
                    x = (int)last_character.Formation.X;
                    y = (int)last_character.Formation.Y;

                    x++;
                    if (x > 9)
                    {
                        y++;
                        x = 0;
                    }
                }
                
                character.Formation = new Vector2(x, y);
                reserves.Characters.Add(character);
                
                squad.Characters.Remove(character);
                i--;
            }

            Picture picture = GetPicture(squad.ID);
            if (picture != null)
            {
                Pictures.Remove(picture);
            }

            ally_army.Squads.Remove(squad);

            SelectedSquad = 0;
        }

        private void SelectSquad(Squad squad)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.Selected_Squad = squad.ID;

            if (squad.Active) 
            {
                //Prevent editing deployed squad
                Handler.ViewOnly_Squad = true;
                Handler.ViewOnly_Character = true;
                Handler.ViewOnly_Item = true;
            }
            else
            {
                Handler.ViewOnly_Squad = false;
                Handler.ViewOnly_Character = false;
                Handler.ViewOnly_Item = false;
            }

            InputManager.Mouse.Flush();
            MenuManager.ChangeMenu("Squad");
        }

        private void DeploySquad()
        {
            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(SelectedSquad);

            World world = SceneManager.GetScene("Localmap").World;
            Map map = world.Maps[Handler.Level];

            WorldUtil.AllyToken_Start(squad, map);

            GetButton("Deploy").Enabled = false;
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddPicture(Handler.GetID(), "Background", AssetManager.Textures["Black"], new Region(0, 0, 0, 0), Color.White * 0.6f, true);

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

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Add",
                hover_text = "Add Squad",
                texture = AssetManager.Textures["Button_Add"],
                texture_highlight = AssetManager.Textures["Button_Add_Hover"],
                texture_disabled = AssetManager.Textures["Button_Add_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Remove",
                hover_text = "Remove Squad",
                texture = AssetManager.Textures["Button_Remove"],
                texture_highlight = AssetManager.Textures["Button_Remove_Hover"],
                texture_disabled = AssetManager.Textures["Button_Remove_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Deploy",
                hover_text = "Deploy Squad",
                texture = AssetManager.Textures["Button_Next"],
                texture_highlight = AssetManager.Textures["Button_Next_Hover"],
                texture_disabled = AssetManager.Textures["Button_Next_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                visible = true
            });

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Squad"], new Region(0, 0, 0, 0), Color.White, false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        private void ResizeArmy()
        {
            Army army = CharacterManager.GetArmy("Ally");
            if (army != null)
            {
                int Y = (Main.Game.MenuSize.Y * 2) - Top;
                int X = Main.Game.MenuSize.X;

                int column = 0;
                int row = 0;

                int max_horizontal = (Main.Game.ScreenWidth / Main.Game.MenuSize.X) / 4;

                foreach (Squad squad in army.Squads)
                {
                    for (int i = 0; i < Pictures.Count; i++)
                    {
                        Picture picture = Pictures[i];
                        if (picture.ID == squad.ID)
                        {
                            if (picture.Name == squad.Name)
                            {
                                picture.Region = new Region(X, Y, Main.Game.MenuSize.X * 3, Main.Game.MenuSize.Y * 3);
                            }
                            else
                            {
                                for (int y = 0; y < 3; y++)
                                {
                                    for (int x = 0; x < 3; x++)
                                    {
                                        if (picture.Name == "x:" + x.ToString() + ",y:" + y.ToString())
                                        {
                                            picture.Region = new Region(X + (Main.Game.MenuSize.X * x), Y + (Main.Game.MenuSize.Y * y), Main.Game.MenuSize.X, Main.Game.MenuSize.Y);
                                        }

                                        Character character = squad.GetCharacter(new Vector2(x, y));
                                        if (character != null)
                                        {
                                            character.Region = new Region(X + (Main.Game.MenuSize.X * x), Y + (Main.Game.MenuSize.Y * y) - Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y + (Main.Game.MenuSize.Y / 2));
                                            CharacterUtil.ResizeBars(character);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Y >= Main.Game.ScreenHeight - (Main.Game.MenuSize.Y * 3))
                    {
                        GetPicture("Arrow_Down").Visible = true;
                        Bottom = false;
                    }
                    else
                    {
                        GetPicture("Arrow_Down").Visible = false;
                        Bottom = true;
                    }

                    X += (Main.Game.MenuSize.X * 4);
                    column++;

                    if (column >= max_horizontal)
                    {
                        column = 0;
                        X = Main.Game.MenuSize.X;

                        row++;
                        Y += (Main.Game.MenuSize.Y * 4);
                    }
                }
            }
        }

        private void ClearArmy()
        {
            for (int s = 0; s < SquadList.Count; s++)
            {
                Squad squad = SquadList[s];

                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {
                        foreach (Picture grid_picture in Pictures)
                        {
                            if (grid_picture.ID == squad.ID &&
                                grid_picture.Name == "x:" + x.ToString() + ",y:" + y.ToString())
                            {
                                Pictures.Remove(grid_picture);
                                break;
                            }
                        }

                        Army army = CharacterManager.GetArmy("Ally");
                        if (army != null)
                        {
                            foreach (Squad existing in army.Squads)
                            {
                                if (existing.ID == squad.ID)
                                {
                                    Character character = existing.GetCharacter(new Vector2(x, y));
                                    if (character != null)
                                    {
                                        character.Visible = false;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                Picture picture = GetPicture(squad.ID);
                if (picture != null)
                {
                    Pictures.Remove(picture);
                }

                SquadList.Remove(squad);
                s--;
            }
        }

        public override void Load()
        {
            ClearArmy();

            Army army = CharacterManager.GetArmy("Ally");
            if (army != null)
            {
                int Y = Main.Game.MenuSize.Y * 2;
                int X = Main.Game.MenuSize.X;

                int column = 0;
                int row = 0;

                int max_horizontal = (Main.Game.ScreenWidth / Main.Game.MenuSize.X) / 4;

                for (int s = 0; s < army.Squads.Count; s++)
                {
                    Squad squad = army.Squads[s];
                    AddPicture(squad.ID, squad.Name, AssetManager.Textures["Squad"],
                        new Region(X, Y, Main.Game.MenuSize.X * 3, Main.Game.MenuSize.Y * 3), Color.White, true);

                    SquadList.Add(squad);

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            AddPicture(squad.ID, "x:" + x.ToString() + ",y:" + y.ToString(), AssetManager.Textures["Grid"],
                                new Region(X + (Main.Game.MenuSize.X * x), Y + (Main.Game.MenuSize.Y * y), Main.Game.MenuSize.X, Main.Game.MenuSize.Y), Color.White, true);

                            Character character = squad.GetCharacter(new Vector2(x, y));
                            if (character != null)
                            {
                                character.Region = new Region(X + (Main.Game.MenuSize.X * x), Y + (Main.Game.MenuSize.Y * y) - Main.Game.MenuSize.Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y + (Main.Game.MenuSize.Y / 2));
                                character.Visible = true;
                                CharacterUtil.ResizeBars(character);
                            }
                        }
                    }

                    if (Y >= Main.Game.ScreenHeight - (Main.Game.MenuSize.Y * 2))
                    {
                        GetPicture("Arrow_Down").Visible = true;
                        Bottom = false;
                    }
                    else
                    {
                        GetPicture("Arrow_Down").Visible = false;
                        Bottom = true;
                    }

                    X += (Main.Game.MenuSize.X * 4);
                    column++;

                    if (column >= max_horizontal)
                    {
                        column = 0;
                        X = Main.Game.MenuSize.X;

                        row++;
                        Y += (Main.Game.MenuSize.Y * 4);
                    }
                }
            }
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            int X = 0;
            int Y = 0;

            GetPicture("Background").Region = new Region(0, 0, Main.Game.Resolution.X, Main.Game.Resolution.Y);

            GetButton("Back").Region = new Region(X, Y, width, height);
            GetButton("Add").Region = new Region(X + width, Y, width, height);
            GetButton("Remove").Region = new Region(X + (width * 2), Y, width, height);
            GetButton("Deploy").Region = new Region(X + (width * 3), Y, width, height);

            GetPicture("Arrow_Up").Region = new Region(X, height, width, height);
            GetPicture("Arrow_Down").Region = new Region(X, Main.Game.ScreenHeight - height, width, height);

            GetPicture("Highlight").Region = new Region(0, 0, 0, 0);
            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            if (Visible)
            {
                ResizeArmy();
            }
        }

        #endregion
    }
}
