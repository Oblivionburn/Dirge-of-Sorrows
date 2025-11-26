using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Tiles;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Weathers;

namespace DoS1.Util
{
    public static class GameUtil
    {
        private static void ResetGame()
        {
            Main.Game.GameStarted = false;

            Handler.MarketInventories.Clear();
            Handler.TradingMarket = null;
            Handler.AcademyRecruits.Clear();
            Handler.TradingAcademy = null;
            Handler.LocalMap = false;
            Handler.LocalPause = false;
            Handler.Level = 0;
            Handler.StoryStep = -1;
            Handler.Gold = 1000;

            TimeManager.Paused = false;
            TimeManager.Interval = 0;

            MenuManager.PreviousMenus.Clear();

            Inventory inventory = InventoryManager.GetInventory("Ally");
            inventory.Items.Clear();

            SceneManager.GetScene("Worldmap").World.Maps.Clear();
            SceneManager.GetScene("Localmap").World.Maps.Clear();

            WeatherManager.Transitioning = false;
            WeatherManager.Lightning = false;
            WeatherManager.TransitionType = WeatherTransition.None;
            WeatherManager.CurrentWeather = WeatherType.Clear;

            foreach (Weather weather in WeatherManager.Weathers)
            {
                weather.TransitionTime = 0;
                weather.ParticleManager.Particles.Clear();
                weather.Visible = false;
            }

            TimeManager.Reset(TimeRate.Second, 1, 1, 1, 12);
            TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear };

            TimeManager.Now.OnSecondsChange -= SecondChanged;
            TimeManager.Now.OnSecondsChange += SecondChanged;

            TimeManager.Now.OnMinutesChange -= MinuteChanged;
            TimeManager.Now.OnMinutesChange += MinuteChanged;

            TimeManager.Now.OnHoursChange -= HourChanged;
            TimeManager.Now.OnHoursChange += HourChanged;

            TimeManager.Now.OnDaysChange -= DayChanged;
            TimeManager.Now.OnDaysChange += DayChanged;
        }

        public static void NewGame()
        {
            ResetGame();

            Task.Factory.StartNew(() => WorldGen.GenWorldmap());

            Main.Game.GameStarted = true;
            Toggle_MainMenu();

            Scene scene = WorldUtil.GetScene();
            SceneManager.ChangeScene(scene);
            scene.Active = true;

            SoundManager.StopAll();
            SoundManager.NeedMusic = true;
            SoundManager.MusicLooping = false;
            SoundManager.StopAmbient();
            SoundManager.AmbientFade = 1;
            SoundManager.AmbientPaused = false;

            Menu ui = MenuManager.GetMenu("UI");
            ui.Visible = true;
            MenuManager.CurrentMenu_ID = ui.ID;

            if (scene.Name == "Worldmap")
            {
                while (!scene.World.Maps.Any())
                {
                    Thread.Sleep(100);
                }
            }

            Main.Game.Zoom = 1.5f;

            Map map = WorldUtil.GetMap(scene.World);
            WorldUtil.Resize_OnStart(scene.Menu, map);

            Main.Game.ResolutionChange();
        }

        public static void LoadGame()
        {
            ResetGame();
            CharacterManager.Armies.Clear();

            LoadUtil.LoadGame();

            Scene scene = WorldUtil.GetScene();
            SceneManager.ChangeScene(scene);
            scene.Active = true;

            SoundManager.StopAll();
            SoundManager.NeedMusic = true;
            SoundManager.MusicLooping = false;
            SoundManager.StopAmbient();
            SoundManager.AmbientFade = 1;
            SoundManager.AmbientPaused = false;

            Menu ui = MenuManager.GetMenu("UI");
            ui.Visible = true;
            MenuManager.CurrentMenu_ID = ui.ID;

            Main.Game.Zoom = 1.5f;

            Map map = WorldUtil.GetMap(scene.World);
            WorldUtil.Resize_OnStart(scene.Menu, map);

            Main.Game.ResolutionChange();

            if (Handler.LocalMap)
            {
                ui.GetButton("PlayPause").Enabled = true;
                ui.GetButton("Speed").Enabled = true;

                //Set weather
                if (map.Type.Contains("Snow") ||
                    map.Type.Contains("Ice"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Snow };
                }
                else if (!map.Type.Contains("Desert"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm };
                }

                //Set "Return to Worldmap" button
                Button worldMap = ui.GetButton("Worldmap");
                worldMap.Visible = true;

                int maxLevelUnlocked = WorldUtil.MaxLevelUnlocked();
                if (maxLevelUnlocked > Handler.Level)
                {
                    worldMap.Enabled = true;
                }
                else
                {
                    worldMap.Enabled = false;
                }
            }

            Menu saveLoad = MenuManager.GetMenu("Save_Load");
            saveLoad.GetPicture("Loading").Visible = false;
            saveLoad.Visible = false;

            Main.Game.GameStarted = true;
            Toggle_MainMenu();
        }

        public static void ReturnToTitle()
        {
            ResetGame();
            CharacterManager.Armies.Clear();

            Toggle_MainMenu();

            Menu main = MenuManager.GetMenu("Main");
            main.Visible = true;
            main.Active = true;

            Menu ui = MenuManager.GetMenu("UI");
            ui.GetButton("Worldmap").Visible = false;

            SceneManager.GetScene("Title").Menu.Visible = true;
            SceneManager.ChangeScene("Title");

            SoundManager.StopAll();
            SoundManager.NeedMusic = true;
            SoundManager.MusicLooping = true;
            SoundManager.StopAmbient();
            SoundManager.AmbientFade = 1;
            SoundManager.AmbientPaused = false;

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

            TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear };

            Handler.LocalMap = false;
            Handler.LocalPause = false;

            Menu ui = MenuManager.GetMenu("UI");
            ui.GetButton("Worldmap").Visible = false;
            ui.GetButton("PlayPause").Enabled = false;

            if (!Handler.RevisitMap)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                foreach (Squad squad in enemy_army.Squads)
                {
                    foreach (Character character in squad.Characters)
                    {
                        character.Inventory.Items.Clear();
                    }
                    squad.Characters.Clear();
                }
                enemy_army.Squads.Clear();
            }

            Army ally_army = CharacterManager.GetArmy("Ally");
            foreach (Squad squad in ally_army.Squads)
            {
                squad.Active = false;
                squad.Visible = false;
                squad.Path.Clear();
                squad.Moving = false;

                foreach (Character character in squad.Characters)
                {
                    character.HealthBar.Value = character.HealthBar.Max_Value;
                    character.ManaBar.Value = character.ManaBar.Max_Value;
                }
            }

            Menu main = MenuManager.GetMenu("Main");
            main.GetButton("Save").Visible = true;

            Scene localmap = SceneManager.GetScene("Localmap");
            localmap.Active = false;

            Scene worldmap = SceneManager.GetScene("Worldmap");
            SceneManager.ChangeScene(worldmap);
            worldmap.Active = true;

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;
        }

        public static void RetreatToWorldmap()
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

            TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear };

            Handler.LocalMap = false;
            Handler.LocalPause = false;

            Menu ui = MenuManager.GetMenu("UI");
            ui.GetButton("Worldmap").Visible = false;
            ui.GetButton("PlayPause").Enabled = false;

            Army enemy_army = CharacterManager.GetArmy("Enemy");
            foreach (Squad squad in enemy_army.Squads)
            {
                squad.Active = false;
                squad.Visible = false;
                squad.Path.Clear();
                squad.Moving = false;

                foreach (Character character in squad.Characters)
                {
                    character.HealthBar.Value = character.HealthBar.Max_Value;
                    character.ManaBar.Value = character.ManaBar.Max_Value;
                }
            }

            Army ally_army = CharacterManager.GetArmy("Ally");
            foreach (Squad squad in ally_army.Squads)
            {
                squad.Active = false;
                squad.Visible = false;
                squad.Path.Clear();
                squad.Moving = false;

                foreach (Character character in squad.Characters)
                {
                    character.HealthBar.Value = character.HealthBar.Max_Value;
                    character.ManaBar.Value = character.ManaBar.Max_Value;
                }
            }

            Menu main = MenuManager.GetMenu("Main");
            main.GetButton("Save").Visible = true;

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

        public static string WrapText_Dialogue(string text)
        {
            string result = "";

            List<string> text_parts = new List<string>();
            int max_length = 49;

            string full_text = text;
            if (full_text.Length > max_length)
            {
                for (int m = 0; m < full_text.Length; m++)
                {
                    int index_break = 0;

                    string current_chunk = full_text.Substring(0, max_length);
                    if (current_chunk.Contains("\n"))
                    {
                        index_break = full_text.IndexOf("\n") + 1;

                        string text_part = full_text.Substring(0, index_break);
                        text_parts.Add(text_part);

                        full_text = full_text.Remove(0, index_break);
                        m = 0;
                    }
                    else
                    {
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

                        full_text = full_text.Remove(0, index_break + 1);
                        m = 0;
                    }

                    if (full_text.Length <= max_length)
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
                string text_part = text_parts[i];
                if (text_part[text_part.Length - 1] == '\n')
                {
                    result += text_part;
                }
                else
                {
                    result += text_part + "\n";
                }
            }

            return result;
        }

        public static string WrapText_Help(string text, int max_length)
        {
            string result = "";

            List<string> text_parts = new List<string>();

            string full_text = text;
            if (full_text.Length > max_length)
            {
                for (int m = 0; m < full_text.Length; m++)
                {
                    int index_break = 0;

                    string current_chunk = full_text.Substring(0, max_length);
                    if (current_chunk.Contains("\n"))
                    {
                        index_break = full_text.IndexOf("\n") + 1;

                        string text_part = full_text.Substring(0, index_break);
                        text_parts.Add(text_part);

                        full_text = full_text.Remove(0, index_break);
                        m = 0;
                    }
                    else
                    {
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

                        full_text = full_text.Remove(0, index_break + 1);
                        m = 0;
                    }

                    if (full_text.Length <= max_length)
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
                string text_part = text_parts[i];
                if (text_part[text_part.Length - 1] == '\n')
                {
                    result += text_part;
                }
                else
                {
                    result += text_part + "\n";
                }
            }

            return result;
        }

        public static string HisHer(string gender)
        {
            if (gender == "Male")
            {
                return "his";
            }

            return "her";
        }

        public static string HimHer(string gender)
        {
            if (gender == "Male")
            {
                return "him";
            }

            return "her";
        }

        public static string HeShe(string gender)
        {
            if (gender == "Male")
            {
                return "he";
            }

            return "she";
        }

        public static Color Get_EffectColor(string effect_name)
        {
            if (effect_name == "Weak")
            {
                return new Color(86, 127, 93, 255);
            }
            else if (effect_name == "Cursed")
            {
                return new Color(0, 0, 0, 255);
            }
            else if (effect_name == "Melting")
            {
                return new Color(125, 115, 62, 255);
            }
            else if (effect_name == "Poisoned")
            {
                return new Color(0, 255, 0, 255);
            }
            else if (effect_name == "Petrified")
            {
                return new Color(125, 125, 125, 255);
            }
            else if (effect_name == "Burning")
            {
                return new Color(255, 0, 0, 255);
            }
            else if (effect_name == "Regenerating")
            {
                return new Color(255, 255, 255, 255);
            }
            else if (effect_name == "Charging")
            {
                return new Color(255, 255, 0, 255);
            }
            else if (effect_name == "Stunned")
            {
                return new Color(81, 68, 47, 255);
            }
            else if (effect_name == "Slow")
            {
                return new Color(255, 140, 20, 255);
            }
            else if (effect_name == "Frozen")
            {
                return new Color(0, 0, 255, 255);
            }
            else if (effect_name == "Shocked")
            {
                return new Color(0, 255, 255, 255);
            }

            return new Color(0, 0, 0, 0);
        }

        public static Texture2D CopyTexture_NewColor(Texture2D original, Color new_color)
        {
            Texture2D texture = new Texture2D(Main.Game.GraphicsManager.GraphicsDevice, original.Width, original.Height);

            Color[] colors = new Color[original.Width * original.Height];
            original.GetData(colors);

            int count = colors.Length;
            for (int i = 0; i < count; i++)
            {
                Color color = colors[i];
                if (color.R == 0 &&
                    color.G == 0 &&
                    color.B == 0 &&
                    color.A == 0)
                {
                    //Ignore transparent pixels
                }
                else
                {
                    colors[i] = new_color;
                }
            }

            texture.SetData(colors);
            texture.Name = texture.Name;

            return texture;
        }

        public static Texture2D CopyTexture_NewColor(GraphicsDevice graphicsDevice, Texture2D texture, Color new_color)
        {
            int width = texture.Width;
            int height = texture.Height;

            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, width, height)
            {
                Name = texture.Name
            };

            while (Main.Drawing)
            {
                Thread.Sleep(1);
            }

            if (!Main.Drawing)
            {
                Handler.PauseDrawing = true;

                graphicsDevice.SetRenderTarget(renderTarget);
                graphicsDevice.Clear(Color.Transparent);

                Main.Game.SpriteBatch.Begin();
                Main.Game.SpriteBatch.Draw(texture, Vector2.Zero, new_color);
                Main.Game.SpriteBatch.End();

                graphicsDevice.SetRenderTarget(null);

                Handler.PauseDrawing = false;
            }

            return renderTarget;
        }

        public static void Alert_Generic(string message, Color color)
        {
            Menu ui = MenuManager.GetMenu("UI");

            Handler.AlertType = "Generic";

            Label alert = ui.GetLabel("Alert");
            alert.Opacity = 1;
            alert.TextColor = color;
            alert.Text = message;
            alert.Value = 100;
            alert.Visible = true;
        }

        public static void Alert_Combat(Squad attacker, Squad defender)
        {
            Handler.LocalPause = true;
            SoundManager.AmbientPaused = true;

            Menu ui = MenuManager.GetMenu("UI");

            Label examine = ui.GetLabel("Examine");
            examine.Visible = false;

            Button button = ui.GetButton("PlayPause");
            button.Value = 1;
            button.HoverText = "Play";
            button.Texture = AssetManager.Textures["Button_Play"];
            button.Texture_Highlight = AssetManager.Textures["Button_Play_Hover"];
            button.Texture_Disabled = AssetManager.Textures["Button_Play_Disabled"];

            Handler.AlertType = "Combat";

            int height = Main.Game.MenuSize.X;

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            alerts.GetLabel("Dialogue_Name").Visible = false;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Text = "";
            dialogue.Visible = true;

            float Y = dialogue.Region.Y + (height / 2);

            Label combat_attacker = alerts.GetLabel("Combat_Attacker");
            combat_attacker.Text = attacker.Name;
            combat_attacker.Region = new Region(dialogue.Region.X, Y, dialogue.Region.Width, height);
            combat_attacker.Visible = true;

            if (attacker.Type == "Enemy")
            {
                combat_attacker.TextColor = Color.Red;
            }
            else
            {
                combat_attacker.TextColor = Color.Blue;
            }

            Label vs = alerts.GetLabel("Combat_VS");
            vs.Region = new Region(dialogue.Region.X, Y + height, dialogue.Region.Width, height);
            vs.Visible = true;

            Label combat_defender = alerts.GetLabel("Combat_Defender");
            combat_defender.Text = defender.Name;
            combat_defender.Region = new Region(dialogue.Region.X, Y + (height * 2), dialogue.Region.Width, height);
            combat_defender.Visible = true;

            if (defender.Type == "Enemy")
            {
                combat_defender.TextColor = Color.Red;
            }
            else
            {
                combat_defender.TextColor = Color.Blue;
            }

            Button option1 = alerts.GetButton("Dialogue_Option1");
            option1.Text = "Fight!";
            option1.Visible = true;

            Button option2 = alerts.GetButton("Dialogue_Option2");
            option2.Visible = false;
        }

        public static void Alert_Location(Map map, Layer ground, Squad squad, Tile location)
        {
            Toggle_Pause(false);

            Squad hero_squad = ArmyUtil.Get_Squad(Handler.GetHero());

            Handler.Dialogue_Character2 = squad.GetLeader();
            Handler.AlertType = "Location";

            Menu ui = MenuManager.GetMenu("UI");
            Label examine = ui.GetLabel("Examine");
            examine.Visible = false;

            bool captured = false;
            bool liberated = false;
            bool is_market = false;
            bool is_academy = false;
            bool is_base = false;
            bool captured_enemy_base = false;

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
            else if (location.Type.Contains("Market"))
            {
                is_market = true;
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

            string message;
            if (liberated)
            {
                message = "\"We liberated " + location.Name + "!";
            }
            else if (captured)
            {
                message = "\"We captured " + location.Name + ".";
            }
            else
            {
                message = "\"We arrived at " + location.Name + ".";
            }

            if (is_academy)
            {
                message += " There's an academy here we could recruit some people from.";
            }
            else if (is_market)
            {
                message += " There's a market here we could buy some equipment from.";
            }
            else if (captured_enemy_base)
            {
                message += " The enemy will no longer hold control over this region... it's ours now!";
            }
            else if (is_base)
            {
                if (hero_squad.ID != squad.ID)
                {
                    message += " This is our current base of operations. We could retreat inside to deploy again later.";
                }
                else
                {
                    message += " This is our current base of operations.";
                }
            }

            message += "\"";

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = true;
            dialogue_name.Text = Handler.Dialogue_Character2.Name;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText_Dialogue(message);

            Picture picture = alerts.GetPicture("Dialogue_Portrait2");
            picture.Visible = true;

            if (!captured_enemy_base)
            {
                if (hero_squad.ID != squad.ID)
                {
                    if (is_base)
                    {
                        Button option1 = alerts.GetButton("Dialogue_Option1");
                        option1.Text = "[Retreat]";
                        option1.Visible = true;

                        Button option2 = alerts.GetButton("Dialogue_Option2");
                        option2.Text = "[Hold Position]";
                        option2.Visible = true;

                        Button option3 = alerts.GetButton("Dialogue_Option3");
                        option3.Text = "[Continue Moving]";
                        option3.Visible = true;
                    }
                    else if (is_market ||
                             is_academy)
                    {
                        Button option1 = alerts.GetButton("Dialogue_Option1");
                        option1.Text = "[Enter Town]";
                        option1.Visible = true;

                        Button option2 = alerts.GetButton("Dialogue_Option2");
                        option2.Text = "[Hold Position]";
                        option2.Visible = true;

                        Button option3 = alerts.GetButton("Dialogue_Option3");
                        option3.Text = "[Continue Moving]";
                        option3.Visible = true;
                    }
                    else
                    {
                        Button option1 = alerts.GetButton("Dialogue_Option1");
                        option1.Text = "[Hold Position]";
                        option1.Visible = true;

                        Button option2 = alerts.GetButton("Dialogue_Option2");
                        option2.Text = "[Continue Moving]";
                        option2.Visible = true;
                    }
                }
                else
                {
                    if (!is_base)
                    {
                        if (is_market ||
                            is_academy)
                        {
                            Button option1 = alerts.GetButton("Dialogue_Option1");
                            option1.Text = "[Enter Town]";
                            option1.Visible = true;

                            Button option2 = alerts.GetButton("Dialogue_Option2");
                            option2.Text = "[Hold Position]";
                            option2.Visible = true;

                            Button option3 = alerts.GetButton("Dialogue_Option3");
                            option3.Text = "[Continue Moving]";
                            option3.Visible = true;
                        }
                        else
                        {
                            Button option1 = alerts.GetButton("Dialogue_Option1");
                            option1.Text = "[Hold Position]";
                            option1.Visible = true;

                            Button option2 = alerts.GetButton("Dialogue_Option2");
                            option2.Text = "[Continue Moving]";
                            option2.Visible = true;
                        }
                    }
                    else
                    {
                        Button option1 = alerts.GetButton("Dialogue_Option1");
                        option1.Text = "[Hold Position]";
                        option1.Visible = true;

                        Button option2 = alerts.GetButton("Dialogue_Option2");
                        option2.Text = "[Continue Moving]";
                        option2.Visible = true;
                    }
                }
            }
            else
            {
                Button worldmap = ui.GetButton("Worldmap");

                if (worldmap.Enabled)
                {
                    Button option1 = alerts.GetButton("Dialogue_Option1");
                    option1.Text = "[Hold Position]";
                    option1.Visible = true;

                    Button option2 = alerts.GetButton("Dialogue_Option2");
                    option2.Text = "[Continue Moving]";
                    option2.Visible = true;
                }
                else
                {
                    Button option1 = alerts.GetButton("Dialogue_Option1");
                    option1.Text = "[Claim Region]";
                    option1.Visible = true;
                }
            }
        }

        public static void Alert_MoveFinished(Menu menu, Map map, Layer ground, Squad squad, Tile location)
        {
            Toggle_Pause(false);
            WorldUtil.CameraToTile(menu, map, ground, location);

            Handler.Dialogue_Character2 = squad.GetLeader();
            Handler.AlertType = "MoveFinished";

            Menu ui = MenuManager.GetMenu("UI");
            Label examine = ui.GetLabel("Examine");
            examine.Visible = false;

            string message = "\"We have arrived at our destination.\"";

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = true;
            dialogue_name.Text = Handler.Dialogue_Character2.Name;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText_Dialogue(message);

            Picture picture = alerts.GetPicture("Dialogue_Portrait2");
            picture.Visible = true;

            Button option1 = alerts.GetButton("Dialogue_Option1");
            option1.Text = "[Hold Position]";
            option1.Visible = true;

            Button option2 = alerts.GetButton("Dialogue_Option2");
            option2.Text = "[Continue Moving]";
            option2.Visible = true;
        }

        public static void Alert_BaseCaptured(Menu menu, Map map, Layer ground, Tile location)
        {
            LocalPause();

            Handler.AlertType = "Capture";
            Handler.Selected_Token = -1;

            Menu ui = MenuManager.GetMenu("UI");
            ui.GetPicture("Select").Visible = false;

            Layer pathing = map.GetLayer("Pathing");
            pathing.Visible = false;

            WorldUtil.CameraToTile(menu, map, ground, location);

            string message = "The enemy has captured our base!";

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = false;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText_Dialogue(message);

            Button option1 = alerts.GetButton("Dialogue_Option1");
            option1.Text = "[Retreat to Worldmap]";
            option1.Visible = true;
        }

        public static void Alert_Tutorial()
        {
            LocalPause();

            Handler.AlertType = "Tutorial";

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = true;
            dialogue_name.Text = "System";

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = "Enable Tutorial?";

            Button option1 = alerts.GetButton("Dialogue_Option1");
            option1.Text = "[Yes - Start Tutorial]";
            option1.Visible = true;

            Button option2 = alerts.GetButton("Dialogue_Option2");
            option2.Text = "[No - Skip Tutorial]";
            option2.Visible = true;
        }

        public static void Alert_Story(Menu menu)
        {
            Handler.AlertType = "Story";
            string message = "";

            Army ally = CharacterManager.GetArmy("Ally");
            Squad squad = ally.Squads[0];
            Character hero = squad.GetLeader();

            Army special = CharacterManager.GetArmy("Special");

            Character spouse = special.Squads[0].Characters[0];

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = true;

            if (Handler.StoryStep == 0)
            {
                LocalPause();

                dialogue_name.Text = "System";
                message = "Left-click the red castle on the Worldmap to enter the Local Map of that location." +
                    "\n\nIf the red castle is not visible, left-click and drag the map until you can see it.";
            }
            else if (Handler.StoryStep == 1)
            {
                Scene scene = WorldUtil.GetScene();
                Map map = scene.World.Maps[Handler.Level];
                Layer ground = map.GetLayer("Ground");
                Tile tile = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));

                WorldUtil.CameraToTile(menu, map, ground, tile);
                LocalPause();

                dialogue_name.Text = "Narrator";

                if (hero.Gender == "Male")
                {
                    message = "\"Your wife approaches as you're tilling the soil of your farm's grain field...\"";
                }
                else
                {
                    message = "\"Your husband takes a break from tilling the soil of your farm's grain field and comes into the house for some water...\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 2)
            {
                LocalPause();

                Handler.Dialogue_Character2 = spouse;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = spouse.Name;

                if (hero.Gender == "Male")
                {
                    message = "\"I think we're short on grain seed this season. You should take a break from the field and head to the market to buy some more.\"";
                }
                else
                {
                    message = "\"I think we're short on grain seed this season. Could you head to the market to buy some more?\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 3)
            {
                LocalPause();

                Squad reserves = CharacterManager.GetArmy("Reserves").Squads[0];

                Character friend = null;
                if (reserves.Characters.Any())
                {
                    friend = reserves.Characters[0];
                }

                dialogue_name.Text = "Narrator";

                if (hero.Gender == "Male")
                {
                    message = "\"You follow your wife back to the house to clean yourself up before heading to the market. Just as you're finishing, you hear a knock" +
                        " on the door and can tell from the friendly greeting that it's your best friend, " + friend.Name + ".\"";
                }
                else
                {
                    message = "\"You start to clean yourself up for heading to the market. Just as you're finishing, you hear someone enter the house and peek out to" +
                        " see it's your best friend, " + friend.Name + ".\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 4)
            {
                LocalPause();

                Squad reserves = CharacterManager.GetArmy("Reserves").Squads[0];

                Character friend = null;
                if (reserves.Characters.Any())
                {
                    friend = reserves.Characters[0];
                }

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;

                if (hero.Gender == "Male")
                {
                    message = "\"Your wife tells me you're heading to the market for some grain. I need to get a few things from the market as well... mind if I tag along?\"";
                }
                else
                {
                    message = "\"Your husband told me you're heading to the market for some grain. I need to get a few things from the market as well... mind if I tag along?\"";
                }

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 5)
            {
                LocalPause();

                dialogue_name.Text = "System";
                message = "Your starting squad, represented by a blue token on the map, will always be deployed at your Base when entering a Local Map. You can right-click" +
                    " any squad positioned at a Town or your Base to edit them.\n\nRight-click your starting squad now to edit it.";
            }
            else if (Handler.StoryStep == 6)
            {
                LocalPause();

                Squad reserves = CharacterManager.GetArmy("Reserves").Squads[0];

                Character friend = null;
                if (reserves.Characters.Any())
                {
                    friend = reserves.Characters[0];
                }

                dialogue_name.Text = "System";
                message = "Left-click and drag " + friend.Name + " from your Reserves (on the right) to a position in your squad's Formation (on the left).";
            }
            else if (Handler.StoryStep == 7)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Squad Menu.";
            }
            else if (Handler.StoryStep == 8)
            {
                LocalPause();

                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;

                Tile market = WorldUtil.GetMarket();
                Direction direction = WorldUtil.Get_Direction(new Vector2(squad.Location.X, squad.Location.Y), new Vector2(market.Location.X, market.Location.Y));

                message = "\"The nearest market is at " + market.Name + ", which is " + direction.ToString() + " of here. I'm ready when you are!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 9)
            {
                Tile market = WorldUtil.GetMarket();

                dialogue_name.Text = "System";
                message = "Any Town on the map with a gold outline will possess a market. Left-click your squad to begin moving it, and then left-click " + market.Name +
                    " to set that as your destination.\n\nYou can zoom in/out the map with the Scroll Wheel.";
            }
            else if (Handler.StoryStep == 10)
            {
                Character friend = squad.Characters[1];

                dialogue_name.Text = "Narrator";
                message = "\"Upon entering the market, you quickly found the seeds you were needing. You and " + friend.Name + " continued looking around to see" +
                    " what else was being sold...\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 11)
            {
                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Did you ever get that Cloth Helm you were wanting for protecting your face from mosquitoes out in the field? Maybe you should buy" +
                    " one while we're here.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 12)
            {
                dialogue_name.Text = "System";
                message = "Items in the market (on the right) can be right-clicked to purchase them. Items in your inventory (on the left) can be right-clicked" +
                    " to sell them.\n\nPurchase a Cloth Helm from the market.";
            }
            else if (Handler.StoryStep == 13)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Exit Market button in the bottom-center to leave the Market Menu.";
            }
            else if (Handler.StoryStep == 14)
            {
                Scene scene = WorldUtil.GetScene();
                Map map = scene.World.Maps[Handler.Level];
                Layer ground = map.GetLayer("Ground");
                Tile tile = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));

                WorldUtil.CameraToTile(menu, map, ground, tile);
                LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click your squad while they're at a Town so you can edit it and equip your new helm.";
            }
            else if (Handler.StoryStep == 15)
            {
                LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click " + hero.Name + " in the formation to change their equipment.";
            }
            else if (Handler.StoryStep == 16)
            {
                dialogue_name.Text = "System";
                message = "Left-click and drag the Cloth Helm from your inventory to the Helm Slot in order to equip it.";
            }
            else if (Handler.StoryStep == 17)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Character Menu.";
            }
            else if (Handler.StoryStep == 19)
            {
                LocalPause();

                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                Scene scene = WorldUtil.GetScene();
                Map map = WorldUtil.GetMap(scene.World);
                Tile ally_base = WorldUtil.Get_Base(map, "Ally");

                dialogue_name.Text = friend.Name;
                message = "\"Let's head back to " + ally_base.Name + " to show " + spouse.Name + " your new helm... I can't wait to see the look on " + HisHer(spouse.Gender) + 
                    " face. I'm sure " + HeShe(spouse.Gender) + " will love it!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 22)
            {
                LocalPause();

                dialogue_name.Text = "Narrator";
                message = "\"As you approach your fields, you see smoke rising in the distance down the road.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 23)
            {
                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Where's all that smoke coming from? Oh no... your house is on fire! " + spouse.Name + "!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 24)
            {
                Character friend = squad.Characters[1];

                dialogue_name.Text = "Narrator";
                message = "\"You and " + friend.Name + " run down the road to find " + spouse.Name + ", but as you approach your burning home you can see " + HimHer(spouse.Gender) + 
                    " laying dead on the road in a pool of blood.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 25)
            {
                dialogue_name.Text = "Narrator";
                message = "\"With tears streaming down your face, you bend down to " + HisHer(spouse.Gender) + " body and see many long gashes across " + HisHer(spouse.Gender) + 
                    " torso. Someone had slashed " + HimHer(spouse.Gender) + " repeatedly with a sword.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 26)
            {
                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "(heavy sigh) \"" + hero.Name + "... I'm so sorry, but we need to arm ourselves while we can and find who did this. " + spouse.Name + 
                    " deserves justice.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 27)
            {
                Character friend = squad.Characters[1];

                dialogue_name.Text = "Narrator";
                message = "\"You and " + friend.Name + " enter a nearby shed to retrieve some weapons.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 28)
            {
                dialogue_name.Text = "System";
                message = "Right-click your squad to equip your characters with a weapon.";
            }
            else if (Handler.StoryStep == 29)
            {
                LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click a character to equip them with a weapon.";
            }
            else if (Handler.StoryStep == 30)
            {
                int width = Main.Game.MenuSize.X;
                int height = Main.Game.MenuSize.X;
                int Y = Main.Game.ScreenHeight - (height * 6);

                dialogue.Region = new Region((Main.Game.ScreenWidth / 2) - (width * 5), Y, width * 10, height * 4);

                int name_height = (int)(dialogue.Region.Height / 6);
                dialogue_name.Region = new Region(dialogue.Region.X + (width * 3), dialogue.Region.Y - name_height, dialogue.Region.Width - (width * 6), name_height);

                dialogue_name.Text = "System";
                message = "Above your inventory on the right are buttons to filter which items are visible in the grid. Click the Weapons filter button to view" +
                    " your weapons.";
            }
            else if (Handler.StoryStep == 31)
            {
                dialogue_name.Text = "System";
                message = "Left-click and drag a weapon to the character's Weapon Slot.";
            }
            else if (Handler.StoryStep == 32)
            {
                dialogue_name.Text = "System";
                message = "Right-click the weapon you just equipped to attach a rune to it for extra damage.";
            }
            else if (Handler.StoryStep == 33)
            {
                dialogue_name.Text = "System";
                message = "The grid on the right displays all runes in your inventory. Left-click and drag a rune to a Rune Slot under your weapon to attach it.";
            }
            else if (Handler.StoryStep == 34)
            {
                int width = Main.Game.MenuSize.X;
                int height = Main.Game.MenuSize.X;
                int Y = Main.Game.ScreenHeight - (height * 7);

                dialogue.Region = new Region((Main.Game.ScreenWidth / 2) - (width * 5), Y, width * 10, height * 4);

                int name_height = (int)(dialogue.Region.Height / 6);
                dialogue_name.Region = new Region(dialogue.Region.X + (width * 3), dialogue.Region.Y - name_height, dialogue.Region.Width - (width * 6), name_height);

                dialogue_name.Text = "System";
                message = "Now left-click the Back button in the upper-left corner to leave the Item Menu.";
            }
            else if (Handler.StoryStep == 35)
            {
                dialogue_name.Text = "System";
                message = "Equip both characters with a weapon before leaving the Squad Menu.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 36)
            {
                LocalPause();

                dialogue_name.Text = "Narrator";
                message = "\"As you exit the shed, a heavily armed person appears from the back of your house carrying bags of items they must've stolen after" +
                    " murdering " + spouse.Name + ".\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 37)
            {
                Character friend = squad.Characters[1];

                dialogue_name.Text = friend.Name;
                message = "(yells angrily) \"There they are! Get them!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 38)
            {
                Army army = CharacterManager.GetArmy("Special");
                Squad enemy_squad = army.Squads[1];
                Character enemy = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = enemy;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = enemy.Name;
                message = "\"I see you found my warning. Don't worry, " + HeShe(spouse.Gender) + " didn't scream... much.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 39)
            {
                Army army = CharacterManager.GetArmy("Special");
                Squad enemy_squad = army.Squads[1];
                Character enemy = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = enemy;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = enemy.Name;
                message = "(laughing) \"Perhaps you should've paid your taxes on time?\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 40)
            {
                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"We've had enough of your warnings! Every month you ask for more taxes than the last! It's thievery and extortion!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 41)
            {
                Army special_army = CharacterManager.GetArmy("Special");
                Squad special_squad = special_army.Squads[1];
                Character enemy = special_squad.Characters[0];

                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = enemy;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = enemy.Name;
                message = "\"Go cry about it to your local lord, " + local_lord.Name + ", I'm just following " + HisHer(local_lord.Gender) + " orders.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 42)
            {
                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"This ends here!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 43)
            {
                dialogue_name.Text = "System";
                message = "Combat is automatic and death is permanent. Prepare your equipment prior to combat for the best chance of success.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 44)
            {
                dialogue_name.Text = "System";
                message = "Excluding story events, if a character's death is imminent you can click the Retreat button to end combat early and live to fight another day.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 45)
            {
                LocalPause();

                Character friend = squad.Characters[1];

                dialogue_name.Text = "Narrator";
                message = "\"With the murder of " + spouse.Name + " now avenged, " + friend.Name + " helped you bury " + HisHer(spouse.Gender) + " body.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 46)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"We must put a stop to " + local_lord.Name + "... " + HeShe(local_lord.Gender) + "'s taken too much from us, and I fear this madness will" +
                    " never end until we stop " + HimHer(local_lord.Gender) + " for good.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 47)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Character friend = squad.Characters[1];

                Handler.Dialogue_Character2 = friend;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = friend.Name;
                message = "\"Many friends and neighbors in the area have also suffered under " + HisHer(local_lord.Gender) + " tyranny as you have." +
                    " I'm sure they would aid us in the coming battle if we went to the Academy and plead our case to bolster our Reserves. We could also go to the" +
                    " Market for more equipment. Whichever you decide, I'll follow your lead.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 48)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                dialogue_name.Text = "System";
                message = "You're now free to do as you please, and your objective is clear: kill " + local_lord.Name + ".\n\n- If your HP/EP is low, you can park your" +
                    " squad at a Town/Base to recover 1 HP/EP per minute.\n- You will now gain 1 Gold per Town you control at the start of every day.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 50)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = local_lord;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = local_lord.Name;
                message = "\"I don't recall having any appointments today... what do you want?\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 51)
            {
                Handler.Dialogue_Character2 = hero;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = hero.Name;
                message = "\"We're here to put an end to your tyranny and extortion!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 52)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = local_lord;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = local_lord.Name;
                message = "\"My... extortion? You simple fool. It's not I extorting you, it's your King! I just gather the gold he requests, by whatever means necessary," +
                    " else it's my own head going in the gutter and I can't abide that... you see, I'm rather attached to it.\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 53)
            {
                Handler.Dialogue_Character2 = hero;

                Picture picture = alerts.GetPicture("Dialogue_Portrait2");
                picture.Visible = true;

                dialogue_name.Text = hero.Name;
                message = "\"Then you're as guilty as the King for being complicit... and we'll tolerate no more of this! We will cut you all down like the snakes you" +
                    " are, until the very head of your King is rolling at our feet! You will know our suffering, until there is none left to fear us!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 54)
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                Squad enemy_squad = enemy_army.Squads[0];
                Character local_lord = enemy_squad.Characters[0];

                Handler.Dialogue_Character1 = local_lord;

                Picture picture = alerts.GetPicture("Dialogue_Portrait1");
                picture.Visible = true;

                dialogue_name.Text = local_lord.Name;
                message = "\"I see. Well, I guess you leave me no choice... and I'm not going down without a fight!\"";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }
            else if (Handler.StoryStep == 55)
            {
                LocalPause();

                dialogue_name.Text = "System";
                message = "Well done! To finish this tutorial map, and every subsequent map, just park your squad at the enemy's Base to capture it and enable the" +
                    " \"Return to Worldmap\" button in the upper-left corner of the screen which will let you proceed to the next map.";

                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Text = "[Click here to continue]";
                option1.Visible = true;
            }

            dialogue.Text = WrapText_Dialogue(message);
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
            Handler.Retreating = false;
            Handler.LocalPause = false;
            SoundManager.AmbientPaused = false;

            Button button = MenuManager.GetMenu("UI").GetButton("PlayPause");
            button.HoverText = "Pause";
            button.Texture = AssetManager.Textures["Button_Pause"];
            button.Texture_Highlight = AssetManager.Textures["Button_Pause_Hover"];
            button.Texture_Disabled = AssetManager.Textures["Button_Pause_Disabled"];
        }

        public static void Toggle_Pause_Combat(bool manual)
        {
            if (Handler.CombatPause)
            {
                if (manual)
                {
                    Handler.ManualPause = false;
                    CombatUnpause();
                }
                else if (!Handler.ManualPause)
                {
                    //Don't auto-unpause if player had intentionally paused
                    CombatUnpause();
                }
            }
            else
            {
                if (manual)
                {
                    Handler.ManualPause = true;
                }

                CombatPause();
            }
        }

        public static void CombatPause()
        {
            Handler.CombatTimer.Stop();

            Handler.CombatPause = true;
            SoundManager.AmbientPaused = true;

            Button button = SceneManager.GetScene("Combat").Menu.GetButton("PlayPause");
            button.HoverText = "Play";
            button.Texture = AssetManager.Textures["Button_Play"];
            button.Texture_Highlight = AssetManager.Textures["Button_Play_Hover"];
            button.Texture_Disabled = AssetManager.Textures["Button_Play_Disabled"];
        }

        public static void CombatUnpause()
        {
            Handler.CombatTimer.Start();

            Handler.CombatPause = false;
            SoundManager.AmbientPaused = false;

            Button button = SceneManager.GetScene("Combat").Menu.GetButton("PlayPause");
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
                menu.GetButton("Save").Visible = true;
                menu.GetButton("Load").Visible = true;
                menu.GetButton("SaveExit").Visible = true;
                menu.GetButton("Exit").Visible = false;
            }
            else
            {
                menu.GetButton("Back").Visible = false;
                menu.GetButton("Play").Visible = true;
                menu.GetButton("Save").Visible = false;
                menu.GetButton("Load").Visible = false;
                menu.GetButton("SaveExit").Visible = false;
                menu.GetButton("Exit").Visible = true;
            }
        }

        public static void SecondChanged(object sender, EventArgs e)
        {
            if (Handler.LocalMap)
            {
                WorldUtil.MoveSquads();
            }
        }

        public static void MinuteChanged(object sender, EventArgs e)
        {
            WorldUtil.AnimateTiles();

            if (Handler.LocalMap)
            {
                WorldUtil.RestSquads();
            }
        }

        public static void HourChanged(object sender, EventArgs e)
        {
            if (Handler.AutoSave &&
                TimeManager.Now.Hours == 12 &&
                Handler.LocalMap)
            {
                Handler.Selected_Save = "AutoSave";
                SaveUtil.SaveGame();

                Main.Portrait = null;
                Main.SavePortrait = true;

                while (Main.Portrait == null)
                {
                    Thread.Sleep(1);
                }

                string saveDir = Path.Combine(AssetManager.Directories["Saves"], Handler.Selected_Save);
                string portraitFile = Path.Combine(saveDir, "portrait.png");
                using (FileStream stream = File.OpenWrite(portraitFile))
                {
                    Main.Portrait.SaveAsPng(stream, Main.Game.MenuSize.X * 2, Main.Game.MenuSize.Y * 2);
                }

                Handler.Selected_Save = Handler.GetHero().Name;
                Alert_Generic("Game saved!", Color.LimeGreen);
            }
        }

        private static void DayChanged(object sender, EventArgs e)
        {
            if (Handler.StoryStep > 48)
            {
                WorldUtil.Collect_Tax();
            }
        }
    }
}
