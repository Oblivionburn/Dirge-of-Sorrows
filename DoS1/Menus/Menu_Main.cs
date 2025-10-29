﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Inputs;
using OP_Engine.Controls;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Sounds;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Main : Menu
    {
        #region Variables

        

        #endregion

        #region Constructor

        public Menu_Main(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Main";
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

            if (!found)
            {
                GetLabel("Examine").Visible = false;
            }

            if (InputManager.KeyPressed("Esc"))
            {
                Close();
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                Close();
            }
            else if (button.Name == "Play")
            {
                Active = false;
                Visible = false;
                InputManager.Mouse.Flush();

                SceneManager.GetScene("Title").Menu.Visible = false;

                if (Handler.Loaded)
                {
                    if (Handler.Saves.Count > 0)
                    {
                        MenuManager.ChangeMenu("Save_Load");
                    }
                    else
                    {
                        MenuManager.ChangeMenu("CharGen");
                    }
                }
                else
                {
                    MenuManager.ChangeMenu("Loading");
                }
            }
            else if (button.Name == "Save")
            {
                Save.SaveGame();
                Close();
                GameUtil.Alert_Generic("Game saved!", Color.LimeGreen);
            }
            else if (button.Name == "SaveExit")
            {
                Save.SaveGame();
                GameUtil.ReturnToTitle();
            }
            else if (button.Name == "Options")
            {
                InputManager.Mouse.Flush();
                MenuManager.ChangeMenu("Options");
            }
            else if (button.Name == "Exit")
            {
                Main.Game.Quit = true;
            }
        }

        public override void Close()
        {
            TimeManager.Paused = false;
            SoundManager.AmbientPaused = false;

            if (Handler.Combat)
            {
                Handler.CombatTimer.Start();
            }

            InputManager.Mouse.Flush();
            InputManager.Keyboard.Flush();

            MenuManager.ChangeMenu_Previous();
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Back",
                hover_text = "Resume",
                texture = AssetManager.Textures["Button_Back"],
                texture_highlight = AssetManager.Textures["Button_Back_Hover"],
                texture_disabled = AssetManager.Textures["Button_Back_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = false
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Play",
                hover_text = "Play",
                texture = AssetManager.Textures["Button_Play"],
                texture_highlight = AssetManager.Textures["Button_Play_Hover"],
                texture_disabled = AssetManager.Textures["Button_Play_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Save",
                hover_text = "Save",
                texture = AssetManager.Textures["Button_Save"],
                texture_highlight = AssetManager.Textures["Button_Save_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = false
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Options",
                hover_text = "Options",
                texture = AssetManager.Textures["Button_Options"],
                texture_highlight = AssetManager.Textures["Button_Options_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "SaveExit",
                hover_text = "Save & Quit",
                texture = AssetManager.Textures["Button_Main"],
                texture_highlight = AssetManager.Textures["Button_Main_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = false
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Exit",
                hover_text = "Exit",
                texture = AssetManager.Textures["Button_Exit"],
                texture_highlight = AssetManager.Textures["Button_Exit_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White,
                enabled = true,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Version", "v" + Main.Version, Color.White, new Region(0, 0, 0, 0), true);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize.Y * 2);

            Y++;
            Button back = GetButton("Back");
            back.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Button play = GetButton("Play");
            play.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Y++;
            Button save = GetButton("Save");
            save.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Y++;
            Button options = GetButton("Options");
            options.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Y++;
            Button main = GetButton("SaveExit");
            main.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Button exit = GetButton("Exit");
            exit.Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Label version = GetLabel("Version");
            version.Region = new Region(Main.Game.ScreenWidth - (Main.Game.MenuSize.X * 2) - 16, Main.Game.ScreenHeight - Main.Game.MenuSize.X, Main.Game.MenuSize.X * 2, Main.Game.MenuSize.X);

            Label examine = GetLabel("Examine");
            examine.Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
