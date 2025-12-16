using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_UI : Menu
    {
        #region Variables



        #endregion

        #region Constructor

        public Menu_UI(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "UI";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Handler.Combat)
            {
                Active = false;
                Visible = false;
            }

            if (Visible &&
                !TimeManager.Paused)
            {
                if (!Handler.LocalPause)
                {
                    WorldUtil.UpdateTime();
                    UpdateGold();
                    UpdateAlerts();
                }

                if (Handler.StoryStep <= 0 ||
                    Handler.StoryStep == 5 ||
                    Handler.StoryStep == 9 ||
                    Handler.StoryStep == 10 ||
                    Handler.StoryStep == 14 ||
                    Handler.StoryStep == 20 ||
                    Handler.StoryStep == 21 ||
                    Handler.StoryStep == 28 ||
                    Handler.StoryStep > 48)
                {
                    if (Handler.AlertType == "Story" ||
                        Handler.AlertType == "Generic" ||
                        string.IsNullOrEmpty(Handler.AlertType))
                    {
                        UpdateControls();
                    }
                }

                base.Update(gameRef, content);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
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
            if (Handler.MovingGrid)
            {
                GetLabel("Examine").Visible = false;
                return;
            }

            if (!Handler.TokenMenu)
            {
                bool hovering_button = HoveringButton();

                if (!hovering_button)
                {
                    GetLabel("Examine").Visible = false;
                }

                if (InputManager.KeyPressed("Space"))
                {
                    GameUtil.Toggle_Pause(true);
                }
                else if (InputManager.KeyPressed("Esc"))
                {
                    TimeManager.Paused = true;

                    Active = false;
                    Visible = false;

                    MenuManager.ChangeMenu("Main");

                    InputManager.Mouse.Flush();
                    InputManager.Keyboard.Flush();
                }
                else if (InputManager.KeyPressed("Debug"))
                {
                    if (!Main.Game.Debugging)
                    {
                        Main.Game.Debugging = true;
                        Handler.Gold = 10000;
                        GetLabel("Debug").Visible = true;
                    }
                    else
                    {
                        Main.Game.Debugging = false;
                        GetLabel("Debug").Visible = false;
                    }
                }
            }
        }

        private bool HoveringButton()
        {
            if (Handler.Selected_Token != -1 ||
                Handler.StoryStep < 20)
            {
                return false;
            }

            if (Handler.LocalMap &&
                Handler.StoryStep >= 57 &&
                Handler.StoryStep <= 62)
            {
                return false;
            }

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
                            GetLabel("Examine").TextColor = Color.White;
                            GameUtil.Examine(this, button.HoverText);
                        }

                        button.Selected = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick(button);
                            button.Selected = false;
                            break;
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        button.Selected = false;
                    }
                }
            }

            return found;
        }

        private void UpdateAlerts()
        {
            Label alert = GetLabel("Alert");
            if (alert.Visible)
            {
                alert.Value--;
                alert.Opacity -= 0.01f;
                if (alert.Value <= 0)
                {
                    alert.Visible = false;
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            if (button.Name == "PlayPause")
            {
                GameUtil.Toggle_Pause(true);
            }
            else if (button.Name == "Speed")
            {
                GameUtil.ToggleSpeed();
            }
            else if (button.Name == "Worldmap")
            {
                Handler.Fireworks = false;

                Scene scene = WorldUtil.GetScene();
                for (int i = 0; i < scene.Menu.Pictures.Count; i++)
                {
                    Picture picture = scene.Menu.Pictures[i];
                    if (picture.Name == "Fireworks")
                    {
                        scene.Menu.Pictures.Remove(picture);
                        i--;
                    }
                }

                if (Handler.Selected_Token != -1)
                {
                    WorldUtil.DeselectToken();
                }

                if (Handler.StoryStep == 56)
                {
                    Handler.StoryStep++;
                }

                if (Handler.KingKilled)
                {
                    Active = false;
                    Visible = false;

                    Handler.LocalMap = false;
                    Handler.LocalPause = false;

                    SoundManager.StopAll();
                    SoundManager.NeedMusic = true;
                    SoundManager.MusicLooping = true;

                    Handler.StoryStep++;
                    SceneManager.ChangeScene("TheEnd");
                }
                else
                {
                    GameUtil.ReturnToWorldmap();
                }
            }
            else
            {
                TimeManager.Paused = true;

                Active = false;
                Visible = false;

                if (button.Name == "Army" &&
                    Handler.StoryStep == 63)
                {
                    MenuManager.GetMenu("Alerts").Visible = false;
                    Handler.StoryStep++;
                }

                MenuManager.ChangeMenu(button.Name);

                InputManager.Mouse.Flush();
                InputManager.Keyboard.Flush();
            }
        }

        private void UpdateGold()
        {
            Label gold = GetLabel("Gold");
            gold.Text = "Gold: " + Handler.Gold;
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Main",
                hover_text = "System",
                texture = AssetManager.Textures["Button_Menu"],
                texture_highlight = AssetManager.Textures["Button_Menu_Hover"],
                texture_disabled = AssetManager.Textures["Button_Menu_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Help",
                hover_text = "Help",
                texture = AssetManager.Textures["Button_Help"],
                texture_highlight = AssetManager.Textures["Button_Help_Hover"],
                texture_disabled = AssetManager.Textures["Button_Help_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Army",
                hover_text = "Army",
                texture = AssetManager.Textures["Button_Army"],
                texture_highlight = AssetManager.Textures["Button_Army_Hover"],
                texture_disabled = AssetManager.Textures["Button_Army_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Inventory",
                hover_text = "Inventory",
                texture = AssetManager.Textures["Button_Inventory"],
                texture_highlight = AssetManager.Textures["Button_Inventory_Hover"],
                texture_disabled = AssetManager.Textures["Button_Inventory_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Worldmap",
                hover_text = "Return to Worldmap",
                texture = AssetManager.Textures["Button_Exit"],
                texture_highlight = AssetManager.Textures["Button_Exit_Hover"],
                texture_disabled = AssetManager.Textures["Button_Exit_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = false
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "PlayPause",
                hover_text = "Pause",
                texture = AssetManager.Textures["Button_Pause"],
                texture_highlight = AssetManager.Textures["Button_Pause_Hover"],
                texture_disabled = AssetManager.Textures["Button_Pause_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Speed",
                hover_text = "Time x1",
                texture = AssetManager.Textures["Button_Speed1"],
                texture_highlight = AssetManager.Textures["Button_Speed1_Hover"],
                texture_disabled = AssetManager.Textures["Button_Speed1_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            Button speed_button = GetButton("Speed");
            switch (Handler.TimeSpeed)
            {
                case 4:
                    speed_button.HoverText = "Time x1";
                    speed_button.Texture = AssetManager.Textures["Button_Speed1"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
                    break;

                case 6:
                    speed_button.HoverText = "Time x2";
                    speed_button.Texture = AssetManager.Textures["Button_Speed2"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
                    break;

                case 8:
                    speed_button.HoverText = "Time x3";
                    speed_button.Texture = AssetManager.Textures["Button_Speed3"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
                    break;

                case 10:
                    speed_button.HoverText = "Time x4";
                    speed_button.Texture = AssetManager.Textures["Button_Speed4"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
                    break;
            }

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Level", "", Color.White, new Region(0, 0, 0, 0), true);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Debug", "Debugging", Color.White, new Region(0, 0, 0, 0), false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Date", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Date").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Time", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Time").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gold", "Gold: " + Handler.Gold, Color.Gold, AssetManager.Textures["Frame_Wide"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Gold").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Alert", "", Color.White, new Region(0, 0, 0, 0), false);
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            GetButton("PlayPause").Region = new Region(Main.Game.ScreenWidth - (width * 4), 0, width, height);
            GetButton("Speed").Region = new Region(Main.Game.ScreenWidth - (width * 3), 0, width, height);

            GetLabel("Date").Region = new Region(Main.Game.ScreenWidth - (width * 2), 0, width * 2, height / 2);
            GetLabel("Time").Region = new Region(Main.Game.ScreenWidth - (width * 2), height / 2, width * 2, height / 2);
            GetLabel("Level").Region = new Region(Main.Game.ScreenWidth - (width * 2), height, width * 2, height / 2);
            GetLabel("Debug").Region = new Region(Main.Game.ScreenWidth - (width * 2), height + (height / 2), width * 2, height / 2);
            GetLabel("Gold").Region = new Region((Main.Game.Resolution.X / 2) - (width * 5), 0, width * 10, height);
            GetLabel("Alert").Region = new Region((Main.Game.Resolution.X / 2) - (width * 5), height, width * 10, height);

            GetButton("Main").Region = new Region(0, 0, width, height);
            GetButton("Help").Region = new Region(0, height, width, height);
            GetButton("Army").Region = new Region(width, 0, width, height);
            GetButton("Inventory").Region = new Region(width * 2, 0, width, height);
            GetButton("Worldmap").Region = new Region(width * 3, 0, width, height);

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
