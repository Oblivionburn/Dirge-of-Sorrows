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
                Save.ExportINI();
                InputManager.Mouse.Flush();
                MenuManager.ChangeMenu_Previous();
            }
            else if (button.Name == "FullscreenOn")
            {
                Main.Game.GraphicsManager.IsFullScreen = false;
                Main.Game.ResetScreen();

                button.Name = "FullscreenOff";
                button.Texture = AssetManager.Textures["Button_FullscreenOff"];
                button.Texture_Highlight = AssetManager.Textures["Button_FullscreenOff_Hover"];
            }
            else if (button.Name == "FullscreenOff")
            {
                Main.Game.GraphicsManager.IsFullScreen = true;
                Main.Game.ResetScreen();

                button.Name = "FullscreenOn";
                button.Texture = AssetManager.Textures["Button_FullscreenOn"];
                button.Texture_Highlight = AssetManager.Textures["Button_FullscreenOn_Hover"];
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
                SoundManager.AmbientChannel.setVolume(SoundManager.AmbientVolume);

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

            AddButton(Handler.GetID(), "Back", AssetManager.Textures["Button_Back"], AssetManager.Textures["Button_Back_Hover"], AssetManager.Textures["Button_Back_Disabled"],
                new Region(0, 0, 0, 0), Color.White, true);
            GetButton("Back").HoverText = "Back";

            if (Main.Game.GraphicsManager.IsFullScreen)
            {
                AddButton(Handler.GetID(), "FullscreenOn", AssetManager.Textures["Button_FullscreenOn"], AssetManager.Textures["Button_FullscreenOn_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("FullscreenOn").HoverText = "Toggle Fullscreen";
                GetButton("FullscreenOn").Value = 1;
            }
            else
            {
                AddButton(Handler.GetID(), "FullscreenOff", AssetManager.Textures["Button_FullscreenOff"], AssetManager.Textures["Button_FullscreenOff_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("FullscreenOff").HoverText = "Toggle Fullscreen";
            }

            if (SoundManager.MusicEnabled)
            {
                AddButton(Handler.GetID(), "MusicOn", AssetManager.Textures["Button_MusicOn"], AssetManager.Textures["Button_MusicOn_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("MusicOn").HoverText = "Toggle Music";
                GetButton("MusicOn").Value = 1;

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(0, 200, 200, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(Handler.GetID(), "MusicOff", AssetManager.Textures["Button_MusicOff"], AssetManager.Textures["Button_MusicOff_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("MusicOff").HoverText = "Toggle Music";

                AddProgressBar(Handler.GetID(), "Music", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(0, 200, 200, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Music", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            GetVolume(GetProgressBar("Music"));

            if (SoundManager.AmbientEnabled)
            {
                AddButton(Handler.GetID(), "AmbienceOn", AssetManager.Textures["Button_AmbienceOn"], AssetManager.Textures["Button_AmbienceOn_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("AmbienceOn").HoverText = "Toggle Ambience";
                GetButton("AmbienceOn").Value = 1;

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(0, 200, 200, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(Handler.GetID(), "AmbienceOff", AssetManager.Textures["Button_AmbienceOff"], AssetManager.Textures["Button_AmbienceOff_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("AmbienceOff").HoverText = "Toggle Ambience";

                AddProgressBar(Handler.GetID(), "Ambience", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(0, 200, 200, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Ambience", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            GetVolume(GetProgressBar("Ambience"));

            if (SoundManager.SoundEnabled)
            {
                AddButton(Handler.GetID(), "SoundOn", AssetManager.Textures["Button_SoundOn"], AssetManager.Textures["Button_SoundOn_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("SoundOn").HoverText = "Toggle Sound";
                GetButton("SoundOn").Value = 1;

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(0, 200, 200, 255), true);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            else
            {
                AddButton(Handler.GetID(), "SoundOff", AssetManager.Textures["Button_SoundOff"], AssetManager.Textures["Button_SoundOff_Hover"], null,
                    new Region(0, 0, 0, 0), Color.White, true);
                GetButton("SoundOff").HoverText = "Toggle Sound";

                AddProgressBar(Handler.GetID(), "Sound", 100, 100, 1, AssetManager.Textures["ProgressBase"], AssetManager.Textures["ProgressBar"],
                    new Region(0, 0, 0, 0), new Color(0, 200, 200, 255), false);
                AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Sound", "", Color.White, new Region(0, 0, 0, 0), true);
            }
            GetVolume(GetProgressBar("Sound"));

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int Y = Main.Game.ScreenHeight / (Main.Game.MenuSize.Y * 2);

            GetButton("Back").Region = new Region((Main.Game.ScreenWidth / 2) - (Main.Game.MenuSize.X / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Y += 2;
            Button fullscreen = GetButton("FullscreenOn");
            if (fullscreen == null)
            {
                fullscreen = GetButton("FullscreenOff");
            }
            fullscreen.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize.X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            Y += 1;
            Button music = GetButton("MusicOn");
            if (music == null)
            {
                music = GetButton("MusicOff");
            }
            music.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize.X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            GetProgressBar("Music").Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X * 4, Main.Game.MenuSize.Y);
            GetLabel("Music").Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X * 4, Main.Game.MenuSize.Y);

            Y += 1;
            Button ambience = GetButton("AmbienceOn");
            if (ambience == null)
            {
                ambience = GetButton("AmbienceOff");
            }
            ambience.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize.X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            GetProgressBar("Ambience").Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X * 4, Main.Game.MenuSize.Y);
            GetLabel("Ambience").Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X * 4, Main.Game.MenuSize.Y);

            Y += 1;
            Button sound = GetButton("SoundOn");
            if (sound == null)
            {
                sound = GetButton("SoundOff");
            }
            sound.Region = new Region((Main.Game.ScreenWidth / 2) - Main.Game.MenuSize.X, Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X, Main.Game.MenuSize.Y);

            GetProgressBar("Sound").Base_Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X * 4, Main.Game.MenuSize.Y);
            GetLabel("Sound").Region = new Region((Main.Game.ScreenWidth / 2), Main.Game.MenuSize.Y * Y, Main.Game.MenuSize.X * 4, Main.Game.MenuSize.Y);
        }

        #endregion
    }
}
