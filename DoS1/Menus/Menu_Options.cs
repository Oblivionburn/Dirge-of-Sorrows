using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Sounds;
using OP_Engine.Weathers;
using OP_Engine.Utility;
using OP_Engine.Enums;
using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_Options : Menu
    {
        #region Variables



        #endregion

        #region Constructors

        public Menu_Options(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Options";
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
                    picture.Draw(spriteBatch);
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (ProgressBar bar in ProgressBars)
                {
                    bar.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    if (label.Name != "Examine")
                    {
                        label.Draw(spriteBatch);
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

            foreach (ProgressBar bar in ProgressBars)
            {
                if (bar.Visible)
                {
                    if (InputManager.MouseWithin(bar.Base_Region.ToRectangle))
                    {
                        bar.Opacity = 1;

                        if (InputManager.Mouse_LB_Held)
                        {
                            SetVolume(bar);
                        }
                    }
                    else if (InputManager.Mouse.Moved)
                    {
                        bar.Opacity = 0.8f;
                    }
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            button.Opacity = 0.8f;
            button.Selected = false;

            if (button.Name == "Back")
            {
                SaveUtil.ExportINI();
                InputManager.Mouse.Flush();
                MenuManager.ChangeMenu("Main");
                MenuManager.PreviousMenus.Clear();
            }
            else if (button.Name == "AutoSaveOn")
            {
                Handler.AutoSave = false;
                button.Name = "AutoSaveOff";
                GetLabel("AutoSave").Text = "AutoSave Off";
            }
            else if (button.Name == "AutoSaveOff")
            {
                Handler.AutoSave = true;
                button.Name = "AutoSaveOn";
                GetLabel("AutoSave").Text = "AutoSave On";
            }
            else if (button.Name == "FullscreenOn")
            {
                Main.Game.ScreenType = ScreenType.Windowed;
                Main.Game.ResetScreen();

                button.Name = "FullscreenOff";
                button.Texture = AssetManager.Textures["Button_FullscreenOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_FullscreenOff_Hover"];

                GetLabel("Fullscreen").Text = "Fullscreen Off";
            }
            else if (button.Name == "FullscreenOff")
            {
                Main.Game.ScreenType = ScreenType.BorderlessFullscreen;
                Main.Game.ResetScreen();

                button.Name = "FullscreenOn";
                button.Texture = AssetManager.Textures["Button_FullscreenOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_FullscreenOn_Hover"];

                GetLabel("Fullscreen").Text = "Fullscreen On";
            }
            else if (button.Name == "MusicOn")
            {
                GetProgressBar("Music").Visible = false;
                GetLabel("Music").Visible = false;

                SoundManager.StopMusic();
                SoundManager.MusicEnabled = false;

                button.Name = "MusicOff";
                button.Texture = AssetManager.Textures["Button_MusicOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_MusicOff_Hover"];
            }
            else if (button.Name == "MusicOff")
            {
                GetProgressBar("Music").Visible = true;
                GetLabel("Music").Visible = true;

                SoundManager.MusicEnabled = true;

                if (Main.Game.GameStarted)
                {
                    SoundManager.NeedMusic = true;
                }
                else
                {
                    SoundManager.MusicLooping = true;
                    AssetManager.PlayMusic_Random("Title", true);
                }

                button.Name = "MusicOn";
                button.Texture = AssetManager.Textures["Button_MusicOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_MusicOn_Hover"];
            }
            else if (button.Name == "SoundOn")
            {
                GetProgressBar("Sound").Visible = false;
                GetLabel("Sound").Visible = false;

                SoundManager.SoundEnabled = false;

                try
                {
                    SoundManager.StopSound();
                }
                catch (Exception e)
                {
                    string ignore_me = e.Message;
                }

                button.Name = "SoundOff";
                button.Texture = AssetManager.Textures["Button_SoundOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_SoundOff_Hover"];
            }
            else if (button.Name == "SoundOff")
            {
                GetProgressBar("Sound").Visible = true;
                GetLabel("Sound").Visible = true;

                SoundManager.SoundEnabled = true;

                button.Name = "SoundOn";
                button.Texture = AssetManager.Textures["Button_SoundOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_SoundOn_Hover"];
            }
            else if (button.Name == "AmbienceOn")
            {
                GetProgressBar("Ambience").Visible = false;
                GetLabel("Ambience").Visible = false;

                SoundManager.StopAmbient();
                SoundManager.AmbientEnabled = false;

                button.Name = "AmbienceOff";
                button.Texture = AssetManager.Textures["Button_AmbienceOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_AmbienceOff_Hover"];
            }
            else if (button.Name == "AmbienceOff")
            {
                GetProgressBar("Ambience").Visible = true;
                GetLabel("Ambience").Visible = true;

                SoundManager.AmbientEnabled = true;

                if (WeatherManager.CurrentWeather == WeatherType.Rain)
                {
                    AssetManager.PlayAmbient("Rain", true);
                }
                else if (WeatherManager.CurrentWeather == WeatherType.Fog)
                {
                    AssetManager.PlayAmbient("Wind", true);
                }
                else if (WeatherManager.CurrentWeather == WeatherType.Storm)
                {
                    AssetManager.PlayAmbient("Storm", true);
                }

                button.Name = "AmbienceOn";
                button.Texture = AssetManager.Textures["Button_AmbienceOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_AmbienceOn_Hover"];
            }
        }

        private void SetVolume(ProgressBar bar)
        {
            bar.Bar_Region.Width = InputManager.Mouse.X - bar.Base_Region.X;
            float volume = ((bar.Bar_Region.Width * 100) / (float)bar.Base_Region.Width) + 1;

            float CurrentVal = ((volume - 1) * bar.Bar_Texture.Width) / 100;
            bar.Bar_Image.Width = (int)CurrentVal;

            bar.Value = (int)volume;

            if (bar.Name == "Music")
            {
                SoundManager.MusicVolume = volume / 100;
                SoundManager.MusicChannel.setVolume(SoundManager.MusicVolume);

                GetLabel("Music").Text = ((int)volume).ToString() + @"%";
            }
            else if (bar.Name == "Sound")
            {
                SoundManager.SoundVolume = volume / 100;
                GetLabel("Sound").Text = ((int)volume).ToString() + @"%";
            }
            else if (bar.Name == "Ambience")
            {
                SoundManager.AmbientVolume = volume / 100;
                GetLabel("Ambience").Text = ((int)volume).ToString() + @"%";
            }
        }

        private void GetVolume(ProgressBar bar)
        {
            if (bar.Name == "Music")
            {
                bar.Value = (int)(SoundManager.MusicVolume * 100);

                GetLabel("Music").Text = bar.Value.ToString() + @"%";
                GetLabel("Music").Visible = SoundManager.MusicEnabled;
            }
            else if (bar.Name == "Sound")
            {
                bar.Value = (int)(SoundManager.SoundVolume * 100);

                GetLabel("Sound").Text = bar.Value.ToString() + @"%";
                GetLabel("Sound").Visible = SoundManager.SoundEnabled;
            }
            else if (bar.Name == "Ambience")
            {
                bar.Value = (int)(SoundManager.AmbientVolume * 100);

                GetLabel("Ambience").Text = bar.Value.ToString() + @"%";
                GetLabel("Ambience").Visible = SoundManager.AmbientEnabled;
            }
        }

        public override void Load(ContentManager content)
        {
            Clear();

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

            if (Handler.AutoSave)
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "AutoSaveOn",
                    hover_text = "Toggle AutoSave",
                    texture = AssetManager.Textures["Button_Save"],
                    texture_highlight = AssetManager.Textures["Button_Save_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                GetButton("AutoSaveOn").Value = 1;
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AutoSave", "AutoSave On", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "AutoSaveOff",
                    hover_text = "Toggle AutoSave",
                    texture = AssetManager.Textures["Button_Save"],
                    texture_highlight = AssetManager.Textures["Button_Save_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "AutoSave", "AutoSave Off", Color.White, new Region(0, 0, 0, 0), true);
            }

            if (Main.Game.GraphicsManager.IsFullScreen)
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "FullscreenOn",
                    hover_text = "Toggle Fullscreen",
                    texture = AssetManager.Textures["Button_FullscreenOn"],
                    texture_highlight = AssetManager.Textures["Button_FullscreenOn_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                GetButton("FullscreenOn").Value = 1;
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Fullscreen", "Fullscreen On", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "FullscreenOff",
                    hover_text = "Toggle Fullscreen",
                    texture = AssetManager.Textures["Button_FullscreenOff"],
                    texture_highlight = AssetManager.Textures["Button_FullscreenOff_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Fullscreen", "Fullscreen Off", Color.White, new Region(0, 0, 0, 0), true);
            }

            if (SoundManager.MusicEnabled)
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "MusicOn",
                    hover_text = "Toggle Music",
                    texture = AssetManager.Textures["Button_MusicOn"],
                    texture_highlight = AssetManager.Textures["Button_MusicOn_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                GetButton("MusicOn").Value = 1;

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(150, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "MusicOff",
                    hover_text = "Toggle Music",
                    texture = AssetManager.Textures["Button_MusicOff"],
                    texture_highlight = AssetManager.Textures["Button_MusicOff_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(150, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            GetVolume(GetProgressBar("Music"));

            if (SoundManager.AmbientEnabled)
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "AmbienceOn",
                    hover_text = "Toggle Ambience",
                    texture = AssetManager.Textures["Button_AmbienceOn"],
                    texture_highlight = AssetManager.Textures["Button_AmbienceOn_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                GetButton("AmbienceOn").Value = 1;

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(150, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "AmbienceOff",
                    hover_text = "Toggle Ambience",
                    texture = AssetManager.Textures["Button_AmbienceOff"],
                    texture_highlight = AssetManager.Textures["Button_AmbienceOff_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(150, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            GetVolume(GetProgressBar("Ambience"));

            if (SoundManager.SoundEnabled)
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "SoundOn",
                    hover_text = "Toggle Sound",
                    texture = AssetManager.Textures["Button_SoundOn"],
                    texture_highlight = AssetManager.Textures["Button_SoundOn_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                GetButton("SoundOn").Value = 1;

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(150, 0, 0, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(new ButtonOptions
                {
                    id = Handler.GetID(),
                    name = "SoundOff",
                    hover_text = "Toggle Sound",
                    texture = AssetManager.Textures["Button_SoundOff"],
                    texture_highlight = AssetManager.Textures["Button_SoundOff_Hover"],
                    region = new Region(0, 0, 0, 0),
                    draw_color = Color.White,
                    enabled = true,
                    visible = true
                });

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(150, 0, 0, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            GetVolume(GetProgressBar("Sound"));

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize.Y * 2);
            int X = (Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X * 2) - (Main.Game.MenuSize.X / 2);
            int width = Main.Game.MenuSize.X * 4;
            int height = Main.Game.MenuSize.Y;

            GetButton("Back").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, height);

            Y += 2;
            Button autosave = GetButton("AutoSaveOn");
            if (autosave == null)
            {
                autosave = GetButton("AutoSaveOff");
            }
            autosave.Region = new Region(X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, height);
            GetLabel("AutoSave").Region = new Region(autosave.Region.X + autosave.Region.Width, autosave.Region.Y, width, height);

            Y += 1;
            Button fullscreen = GetButton("FullscreenOn");
            if (fullscreen == null)
            {
                fullscreen = GetButton("FullscreenOff");
            }
            fullscreen.Region = new Region(X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, height);
            GetLabel("Fullscreen").Region = new Region(fullscreen.Region.X + fullscreen.Region.Width, fullscreen.Region.Y, width, height);

            Y += 1;
            Button music = GetButton("MusicOn");
            if (music == null)
            {
                music = GetButton("MusicOff");
            }
            music.Region = new Region(X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, height);

            GetProgressBar("Music").Base_Region = new Region(music.Region.X + music.Region.Width, music.Region.Y, width, height);
            GetLabel("Music").Region = new Region(music.Region.X + music.Region.Width, music.Region.Y, width, height);

            Y += 1;
            Button ambience = GetButton("AmbienceOn");
            if (ambience == null)
            {
                ambience = GetButton("AmbienceOff");
            }
            ambience.Region = new Region(X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, height);

            GetProgressBar("Ambience").Base_Region = new Region(ambience.Region.X + ambience.Region.Width, ambience.Region.Y, width, height);
            GetLabel("Ambience").Region = new Region(ambience.Region.X + ambience.Region.Width, ambience.Region.Y, width, height);

            Y += 1;
            Button sound = GetButton("SoundOn");
            if (sound == null)
            {
                sound = GetButton("SoundOff");
            }
            sound.Region = new Region(X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, height);

            GetProgressBar("Sound").Base_Region = new Region(sound.Region.X + sound.Region.Width, sound.Region.Y, width, height);
            GetLabel("Sound").Region = new Region(sound.Region.X + sound.Region.Width, sound.Region.Y, width, height);
        }

        #endregion
    }
}
