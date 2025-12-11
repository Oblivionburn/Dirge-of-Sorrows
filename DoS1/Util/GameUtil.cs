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
using OP_Engine.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            SoundManager.StopAll();
            SoundManager.NeedMusic = true;
            SoundManager.MusicLooping = false;

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

            SoundManager.StopAll();
            SoundManager.NeedMusic = true;
            SoundManager.MusicLooping = false;

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
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Snow, WeatherType.Fog };
                }
                else if (!map.Type.Contains("Desert"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm, WeatherType.Fog };
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
            ui.GetLabel("Level").Text = "";

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

            SceneManager.ChangeScene("Worldmap");

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;
        }

        public static void RetreatToWorldmap()
        {
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
            ui.GetLabel("Level").Text = "";

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

            Squad hero_squad = Handler.GetHero().Squad;

            Handler.Dialogue_Character2 = squad.GetLeader();
            Handler.AlertType = "Location";

            Menu ui = MenuManager.GetMenu("UI");

            Button worldmap = ui.GetButton("Worldmap");

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

                if (worldmap.Enabled)
                {
                    message = "\"We arrived at " + location.Name + ".";
                }
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

            if (captured_enemy_base &&
                !worldmap.Enabled)
            {
                int gold_level = ((Handler.Level + 1) * 1000) / 2;
                int min_gold = gold_level / 4;
                int max_gold = gold_level / 2;

                CryptoRandom random = new CryptoRandom();
                int gold_amount = random.Next(min_gold, max_gold + 1);

                Handler.Gold += gold_amount;

                message += "\n\n" + gold_amount + " Gold looted!";
            }

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

            Button button = SceneManager.GetScene("Combat").Menu.GetButton("PlayPause");
            if (button != null)
            {
                button.HoverText = "Play";
                button.Texture = AssetManager.Textures["Button_Play"];
                button.Texture_Highlight = AssetManager.Textures["Button_Play_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Play_Disabled"];
            }
        }

        public static void CombatUnpause()
        {
            Handler.CombatTimer.Start();
            Handler.CombatPause = false;

            Button button = SceneManager.GetScene("Combat").Menu.GetButton("PlayPause");
            button.HoverText = "Pause";
            button.Texture = AssetManager.Textures["Button_Pause"];
            button.Texture_Highlight = AssetManager.Textures["Button_Pause_Hover"];
            button.Texture_Disabled = AssetManager.Textures["Button_Pause_Disabled"];
        }

        public static void ToggleSpeed()
        {
            Handler.TimeSpeed += 2;
            if (Handler.TimeSpeed > 10)
            {
                Handler.TimeSpeed = 4;
            }

            UpdateSpeed();

            SaveUtil.ExportINI();
        }

        public static void UpdateSpeed()
        {
            Menu ui = MenuManager.GetMenu("UI");
            Button button = ui.GetButton("Speed");
            switch (Handler.TimeSpeed)
            {
                case 4:
                    button.HoverText = "Time x1";
                    button.Texture = AssetManager.Textures["Button_Speed1"];
                    button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                    button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
                    break;

                case 6:
                    button.HoverText = "Time x2";
                    button.Texture = AssetManager.Textures["Button_Speed2"];
                    button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                    button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
                    break;

                case 8:
                    button.HoverText = "Time x3";
                    button.Texture = AssetManager.Textures["Button_Speed3"];
                    button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                    button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
                    break;

                case 10:
                    button.HoverText = "Time x4";
                    button.Texture = AssetManager.Textures["Button_Speed4"];
                    button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                    button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
                    break;
            }
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

            if (Handler.StoryStep == 59)
            {
                Handler.StoryStep++;
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
