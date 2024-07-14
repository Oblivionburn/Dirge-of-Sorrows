using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Weathers;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Tiles;

using OP_Engine.Characters;

namespace DoS1.Util
{
    public static class GameUtil
    {
        public static void Start()
        {
            SoundManager.AmbientFade = 1;
            SoundManager.StopAmbient();

            foreach (Weather weather in WeatherManager.Weathers)
            {
                weather.TransitionTime = 0;
                weather.ParticleManager.Particles.Clear();
                weather.Visible = false;
            }

            WeatherManager.Transitioning = false;
            WeatherManager.Lightning = false;
            WeatherManager.TransitionType = WeatherTransition.None;
            WeatherManager.CurrentWeather = WeatherType.Clear;

            TimeManager.Reset(TimeRate.Second, 1, 1, 1, 12);
            TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear };

            TimeManager.Now.OnSecondsChange -= SecondChanged;
            TimeManager.Now.OnSecondsChange += SecondChanged;

            TimeManager.Now.OnMinutesChange -= MinuteChanged;
            TimeManager.Now.OnMinutesChange += MinuteChanged;

            Main.Game.GameStarted = true;
            Toggle_MainMenu();

            SoundManager.StopMusic();
            SoundManager.MusicLooping = false;
            SoundManager.NeedMusic = true;

            Scene worldmap = SceneManager.GetScene("Worldmap");
            SceneManager.ChangeScene(worldmap);

            MenuManager.GetMenu("UI").Visible = true;
            MenuManager.CurrentMenu_ID = MenuManager.GetMenu("UI").ID;

            while (!worldmap.World.Maps.Any())
            {
                Thread.Sleep(100);
            }

            Map map = worldmap.World.Maps[0];
            WorldUtil.Resize_OnStart(map);
        }

        public static void ReturnToTitle()
        {
            Main.Game.GameStarted = false;
            TimeManager.Paused = false;
            TimeManager.Interval = 0;

            Toggle_MainMenu();

            Menu main = MenuManager.GetMenu("Main");
            main.Visible = true;
            main.Active = true;

            SceneManager.GetScene("Title").Menu.Visible = true;
            SceneManager.ChangeScene("Title");

            SoundManager.StopAll();
            SoundManager.MusicLooping = true;
            SoundManager.NeedMusic = true;
            SoundManager.AmbientFade = 1;

            foreach (Weather weather in WeatherManager.Weathers)
            {
                weather.TransitionTime = 0;
                weather.ParticleManager.Particles.Clear();
                weather.Visible = false;
            }

            WeatherManager.Transitioning = false;
            WeatherManager.Lightning = false;
            WeatherManager.TransitionType = WeatherTransition.None;
            WeatherManager.CurrentWeather = WeatherType.Clear;

            CryptoRandom random = new CryptoRandom();
            int weather_choice = random.Next(0, 3);
            if (weather_choice == 0)
            {
                WeatherManager.ChangeWeather(WeatherType.Rain);
            }
            else if (weather_choice == 1)
            {
                WeatherManager.ChangeWeather(WeatherType.Storm);
            }
            else if (weather_choice == 2)
            {
                WeatherManager.ChangeWeather(WeatherType.Snow);
            }
        }

        public static void ReturnToWorldmap()
        {
            Handler.TradingShop.Items.Clear();
            Handler.TradingShop = null;

            TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear };

            Handler.LocalMap = false;

            Menu ui = MenuManager.GetMenu("UI");
            ui.GetButton("Worldmap").Visible = false;
            ui.GetButton("PlayPause").Enabled = false;
            ui.GetButton("Speed").Enabled = false;

            Main.TimeSpeed = 1;

            Button speed = ui.GetButton("Speed");
            speed.Value = 0;
            speed.HoverText = "x1";
            speed.Texture = AssetManager.Textures["Button_Speed1"];
            speed.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
            speed.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];

            SceneManager.ChangeScene("Worldmap");

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;
        }

        public static void Examine(Menu menu, string text)
        {
            Label examine = menu.GetLabel("Examine");
            examine.Text = text;

            int width = Main.Game.MenuSize.X * 4;
            int height = Main.Game.MenuSize.X;

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

        public static string WrapText(string text)
        {
            string result = "";

            List<string> text_parts = new List<string>();
            int max_length = 39;

            string full_text = text;
            if (full_text.Length > max_length)
            {
                for (int m = 0; m < full_text.Length; m++)
                {
                    int index_break = 0;
                    for (int i = max_length; i > 0; i--)
                    {
                        if (full_text[i] == ' ')
                        {
                            index_break = i;
                            break;
                        }
                    }

                    string text_part = full_text.Substring(0, index_break);
                    text_parts.Add(text_part);

                    full_text = full_text.Remove(0, index_break);
                    m = 0;

                    if (full_text.Length < max_length)
                    {
                        text_parts.Add(full_text);
                        break;
                    }
                }
            }
            else
            {
                text_parts.Add(full_text);
            }

            for (int i = 0; i < text_parts.Count; i++)
            {
                result += text_parts[i] + "\n";
            }

            return result;
        }

        public static void Alert_Combat(string attacker_name, string defender_name)
        {
            Handler.LocalPause = true;
            SoundManager.AmbientPaused = true;

            Menu ui = MenuManager.GetMenu("UI");

            Button button = ui.GetButton("PlayPause");
            button.Value = 1;
            button.HoverText = "Play";
            button.Texture = AssetManager.Textures["Button_Play"];
            button.Texture_Highlight = AssetManager.Textures["Button_Play_Hover"];
            button.Texture_Disabled = AssetManager.Textures["Button_Play_Disabled"];

            Handler.AlertType = "Combat";

            int height = Main.Game.MenuSize.X;

            Button alert = ui.GetButton("Alert");
            alert.Selected = false;
            alert.Opacity = 1;
            alert.Visible = true;

            Label attacker = ui.GetLabel("Combat_Attacker");
            attacker.Text = attacker_name;
            attacker.Region = new Region(alert.Region.X, alert.Region.Y, alert.Region.Width, height);
            attacker.Visible = true;

            Label vs = ui.GetLabel("Combat_VS");
            vs.Region = new Region(alert.Region.X, alert.Region.Y + height, alert.Region.Width, height);
            vs.Visible = true;

            Label defender = ui.GetLabel("Combat_Defender");
            defender.Text = defender_name;
            defender.Region = new Region(alert.Region.X, alert.Region.Y + (height * 2), alert.Region.Width, height);
            defender.Visible = true;

            Picture mouseClick = ui.GetPicture("MouseClick");
            mouseClick.Region = new Region(alert.Region.X + alert.Region.Width, alert.Region.Y + alert.Region.Height - height, height, height);
            mouseClick.Image = new Rectangle(0, 0, mouseClick.Texture.Width / 4, mouseClick.Texture.Height);
            mouseClick.Visible = true;
        }

        public static void Alert_Location(Squad squad, Tile tile)
        {
            LocalPause();

            Handler.Dialogue_Character1 = squad.GetLeader();
            Handler.AlertType = "Location";

            bool captured = false;
            bool liberated = false;
            bool is_shop = false;
            bool is_academy = false;
            bool is_base = false;
            bool captured_enemy_base = false;

            Scene scene = SceneManager.GetScene("Localmap");
            if (scene.World.Maps.Any())
            {
                Map map = scene.World.Maps[Handler.Level];
                if (map != null)
                {
                    Layer ground = map.GetLayer("Ground");
                    Layer locations = map.GetLayer("Locations");

                    foreach (Tile location in locations.Tiles)
                    {
                        if (squad.Location.X == location.Location.X &&
                            squad.Location.Y == location.Location.Y)
                        {
                            if (location.Type.Contains("Enemy"))
                            {
                                liberated = true;
                            }
                            else if (location.Type.Contains("Neutral"))
                            {
                                captured = true;
                            }

                            if (location.Type.Contains("Academy"))
                            {
                                is_academy = true;
                            }
                            else if (location.Type.Contains("Shop"))
                            {
                                is_shop = true;
                            }
                            else if (location.Type.Contains("Base"))
                            {
                                is_base = true;
                            }

                            if (liberated &&
                                is_base)
                            {
                                captured_enemy_base = true;
                            }

                            if (!is_base)
                            {
                                WorldUtil.ChangeLocation(location, squad);
                            }
                            
                            WorldUtil.CameraToTile(map, ground, location);
                            break;
                        }
                    }
                }
            }

            string message;
            if (liberated)
            {
                message = squad.Name + ": \"We liberated " + tile.Name + "!";
            }
            else if (captured)
            {
                message = squad.Name + ": \"We captured " + tile.Name + ".";
            }
            else
            {
                message = squad.Name + ": \"We arrived at " + tile.Name + ".";
            }

            if (is_academy)
            {
                message += " There's an academy here we could recruit some people from.";
            }
            else if (is_shop)
            {
                message += " There's a shop here we could buy some equipment from.";
            }
            else if (captured_enemy_base)
            {
                message += " The enemy will no longer hold control over this region... it's ours now!";
            }

            message += "\"";

            Menu ui = MenuManager.GetMenu("UI");

            Label dialogue = ui.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText(message);

            int height = Main.Game.MenuSize.X;

            Picture picture = ui.GetPicture("Dialogue_Portrait1");
            picture.Region = new Region(dialogue.Region.X + dialogue.Region.Width, dialogue.Region.Y - (height * 2), height * 3, height * 3);
            picture.Visible = true;

            if (!captured_enemy_base)
            {
                Button option1 = ui.GetButton("Dialogue_Option1");
                option1.Text = "Enter Town";
                option1.Region = new Region(dialogue.Region.X, dialogue.Region.Y + dialogue.Region.Height - (height * 2), dialogue.Region.Width, height);
                option1.Visible = true;

                Button option2 = ui.GetButton("Dialogue_Option2");
                option2.Text = "Move Out";
                option2.Region = new Region(dialogue.Region.X, dialogue.Region.Y + dialogue.Region.Height - height, dialogue.Region.Width, height);
                option2.Visible = true;
            }
            else
            {
                Button option1 = ui.GetButton("Dialogue_Option1");
                option1.Text = "Claim Region";
                option1.Region = new Region(dialogue.Region.X, dialogue.Region.Y + dialogue.Region.Height - height, dialogue.Region.Width, height);
                option1.Visible = true;
            }
        }

        public static void Alert_MoveFinished(Squad squad)
        {
            LocalPause();

            Handler.Dialogue_Character1 = squad.GetLeader();
            Handler.AlertType = "MoveFinished";

            string message = squad.Name + ": \"We have arrived at our destination.\"";

            Menu ui = MenuManager.GetMenu("UI");

            Label dialogue = ui.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText(message);

            int height = Main.Game.MenuSize.X;

            Picture picture = ui.GetPicture("Dialogue_Portrait1");
            picture.Region = new Region(dialogue.Region.X + dialogue.Region.Width, dialogue.Region.Y - (height * 2), height * 3, height * 3);
            picture.Visible = true;

            Button option1 = ui.GetButton("Dialogue_Option1");
            option1.Text = "(Continue)";
            option1.Region = new Region(dialogue.Region.X, dialogue.Region.Y + dialogue.Region.Height - height, dialogue.Region.Width, height);
            option1.Visible = true;
        }

        public static void Toggle_Pause(bool manual)
        {
            if (Handler.LocalPause)
            {
                if (manual)
                {
                    Handler.ManualPause = false;
                    LocalUnpause();
                }
                else if (!Handler.ManualPause)
                {
                    //Don't auto-unpause if player had intentionally paused
                    LocalUnpause();
                }
            }
            else
            {
                if (manual)
                {
                    Handler.ManualPause = true;
                }

                LocalPause();
            }
        }

        public static void LocalPause()
        {
            Handler.LocalPause = true;
            SoundManager.AmbientPaused = true;

            Button button = MenuManager.GetMenu("UI").GetButton("PlayPause");
            button.HoverText = "Play";
            button.Texture = AssetManager.Textures["Button_Play"];
            button.Texture_Highlight = AssetManager.Textures["Button_Play_Hover"];
            button.Texture_Disabled = AssetManager.Textures["Button_Play_Disabled"];
        }

        public static void LocalUnpause()
        {
            Handler.LocalPause = false;
            SoundManager.AmbientPaused = false;

            Button button = MenuManager.GetMenu("UI").GetButton("PlayPause");
            button.HoverText = "Pause";
            button.Texture = AssetManager.Textures["Button_Pause"];
            button.Texture_Highlight = AssetManager.Textures["Button_Pause_Hover"];
            button.Texture_Disabled = AssetManager.Textures["Button_Pause_Disabled"];
        }

        public static void Toggle_MainMenu()
        {
            Menu menu = MenuManager.GetMenu("Main");

            if (Main.Game.GameStarted)
            {
                menu.GetButton("Back").Visible = true;
                menu.GetButton("Play").Visible = false;

                Button save = menu.GetButton("Save");
                save.Visible = true;

                Button options = menu.GetButton("Options");
                options.Region = new Region(options.Region.X, save.Region.Y + Main.Game.MenuSize.Y, options.Region.Width, options.Region.Height);

                Button main = menu.GetButton("Main");
                main.Visible = true;
                main.Region = new Region(main.Region.X, options.Region.Y + Main.Game.MenuSize.Y, main.Region.Width, main.Region.Height);

                menu.GetButton("Exit").Visible = false;
            }
            else
            {
                menu.GetButton("Back").Visible = false;

                Button play = menu.GetButton("Play");
                play.Visible = true;

                menu.GetButton("Save").Visible = false;
                menu.GetButton("Main").Visible = false;

                Button options = menu.GetButton("Options");
                options.Region = new Region(options.Region.X, play.Region.Y + (Main.Game.MenuSize.Y * 2), options.Region.Width, options.Region.Height);

                Button exit = menu.GetButton("Exit");
                exit.Visible = true;
                exit.Region = new Region(exit.Region.X, options.Region.Y + Main.Game.MenuSize.Y, exit.Region.Width, exit.Region.Height);
            }
        }

        public static void SecondChanged(object sender, EventArgs e)
        {
            WorldUtil.MoveSquads();
        }

        public static void MinuteChanged(object sender, EventArgs e)
        {
            WorldUtil.AnimateTiles();
        }
    }
}
