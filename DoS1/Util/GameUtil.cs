using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
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
        public static void NewGame()
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

            TimeManager.Now.OnHoursChange -= HourChanged;
            TimeManager.Now.OnHoursChange += HourChanged;

            Main.Game.GameStarted = true;
            Toggle_MainMenu();

            SoundManager.StopMusic();
            SoundManager.MusicLooping = false;
            SoundManager.NeedMusic = true;

            Scene scene = WorldUtil.GetScene();
            SceneManager.ChangeScene(scene);

            MenuManager.GetMenu("UI").Visible = true;
            MenuManager.CurrentMenu_ID = MenuManager.GetMenu("UI").ID;

            if (scene.Name == "Worldmap")
            {
                while (!scene.World.Maps.Any())
                {
                    Thread.Sleep(100);
                }
            }

            Map map = WorldUtil.GetMap(scene.World);
            WorldUtil.Resize_OnStart(map);
        }

        public static void LoadGame()
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

            TimeManager.Now.OnSecondsChange -= SecondChanged;
            TimeManager.Now.OnSecondsChange += SecondChanged;

            TimeManager.Now.OnMinutesChange -= MinuteChanged;
            TimeManager.Now.OnMinutesChange += MinuteChanged;

            TimeManager.Now.OnHoursChange -= HourChanged;
            TimeManager.Now.OnHoursChange += HourChanged;

            Main.Game.GameStarted = true;
            Toggle_MainMenu();

            SoundManager.StopMusic();
            SoundManager.MusicLooping = false;
            SoundManager.NeedMusic = true;

            Scene scene = WorldUtil.GetScene();
            SceneManager.ChangeScene(scene);

            Menu ui = MenuManager.GetMenu("UI");
            ui.Visible = true;
            MenuManager.CurrentMenu_ID = ui.ID;

            Map map = WorldUtil.GetMap(scene.World);
            WorldUtil.Resize_OnStart(map);

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
        }

        public static void ReturnToTitle()
        {
            Main.Game.GameStarted = false;

            Handler.MarketInventories.Clear();
            Handler.TradingMarket = null;
            Handler.AcademyRecruits.Clear();
            Handler.TradingAcademy = null;
            Handler.LocalMap = false;
            Handler.LocalPause = false;
            Handler.StoryStep = -1;
            Handler.Gold = 1000;

            TimeManager.Paused = false;
            TimeManager.Interval = 0;

            MenuManager.PreviousMenus.Clear();

            Toggle_MainMenu();

            Menu main = MenuManager.GetMenu("Main");
            main.Visible = true;
            main.Active = true;

            Menu ui = MenuManager.GetMenu("UI");
            ui.GetButton("Worldmap").Visible = false;

            SceneManager.GetScene("Title").Menu.Visible = true;
            SceneManager.ChangeScene("Title");

            CharacterManager.Armies.Clear();
            SceneManager.GetScene("Worldmap").World.Maps.Clear();
            SceneManager.GetScene("Localmap").World.Maps.Clear();

            SoundManager.StopAll();
            SoundManager.MusicLooping = true;
            SoundManager.NeedMusic = true;
            SoundManager.AmbientFade = 1;
            SoundManager.AmbientPaused = false;

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

            Army army = CharacterManager.GetArmy("Ally");
            foreach (Squad squad in army.Squads)
            {
                squad.Active = false;

                foreach (Character character in squad.Characters)
                {
                    character.HealthBar.Value = character.HealthBar.Max_Value;
                    character.ManaBar.Value = character.ManaBar.Max_Value;
                }
            }

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
            int max_length = 50;

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

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            alerts.GetLabel("Dialogue_Name").Visible = false;

            Button alert = alerts.GetButton("Alert");
            alert.Selected = false;
            alert.Opacity = 1;
            alert.Visible = true;

            Label attacker = alerts.GetLabel("Combat_Attacker");
            attacker.Text = attacker_name;
            attacker.Region = new Region(alert.Region.X, alert.Region.Y, alert.Region.Width, height);
            attacker.Visible = true;

            Label vs = alerts.GetLabel("Combat_VS");
            vs.Region = new Region(alert.Region.X, alert.Region.Y + height, alert.Region.Width, height);
            vs.Visible = true;

            Label defender = alerts.GetLabel("Combat_Defender");
            defender.Text = defender_name;
            defender.Region = new Region(alert.Region.X, alert.Region.Y + (height * 2), alert.Region.Width, height);
            defender.Visible = true;

            Picture mouseClick = alerts.GetPicture("MouseClick");
            mouseClick.Region = new Region(alert.Region.X + alert.Region.Width, alert.Region.Y + alert.Region.Height - height, height, height);
            mouseClick.Image = new Rectangle(0, 0, mouseClick.Texture.Width / 4, mouseClick.Texture.Height);
            mouseClick.Visible = true;
        }

        public static void Alert_Location(Map map, Layer ground, Squad squad, Tile location)
        {
            Toggle_Pause(false);

            Squad hero_squad = ArmyUtil.Get_Squad(Handler.GetHero());

            Handler.Dialogue_Character2 = squad.GetLeader();
            Handler.AlertType = "Location";

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
            dialogue.Text = WrapText(message);

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
                    }
                    else
                    {
                        Button option1 = alerts.GetButton("Dialogue_Option1");
                        option1.Text = "[Enter Town]";
                        option1.Visible = true;
                    }

                    Button option2 = alerts.GetButton("Dialogue_Option2");
                    option2.Text = "[Continue]";
                    option2.Visible = true;
                }
                else
                {
                    if (!is_base)
                    {
                        Button option1 = alerts.GetButton("Dialogue_Option1");
                        option1.Text = "[Enter Town]";
                        option1.Visible = true;

                        Button option2 = alerts.GetButton("Dialogue_Option2");
                        option2.Text = "[Continue]";
                        option2.Visible = true;
                    }
                    else
                    {
                        Button option1 = alerts.GetButton("Dialogue_Option1");
                        option1.Text = "[Continue]";
                        option1.Visible = true;
                    }
                }
            }
            else
            {
                Button option1 = alerts.GetButton("Dialogue_Option1");
                option1.Visible = true;

                Menu ui = MenuManager.GetMenu("UI");
                Button worldmap = ui.GetButton("Worldmap");

                if (worldmap.Enabled)
                {
                    option1.Text = "[Continue]";
                }
                else
                {
                    option1.Text = "[Claim Region]";
                }
            }
        }

        public static void Alert_MoveFinished(Map map, Layer ground, Squad squad, Tile location)
        {
            Toggle_Pause(false);
            WorldUtil.CameraToTile(map, ground, location);

            Handler.Dialogue_Character2 = squad.GetLeader();
            Handler.AlertType = "MoveFinished";

            string message = "\"We have arrived at our destination.\"";

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue_name = alerts.GetLabel("Dialogue_Name");
            dialogue_name.Visible = true;
            dialogue_name.Text = Handler.Dialogue_Character2.Name;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText(message);

            Picture picture = alerts.GetPicture("Dialogue_Portrait2");
            picture.Visible = true;

            Button option1 = alerts.GetButton("Dialogue_Option1");
            option1.Text = "[Hold Position]";
            option1.Visible = true;

            Button option2 = alerts.GetButton("Dialogue_Option2");
            option2.Text = "[Continue Moving]";
            option2.Visible = true;
        }

        public static void Alert_Capture(Map map, Layer ground, Tile location)
        {
            LocalPause();

            Handler.AlertType = "Capture";

            WorldUtil.CameraToTile(map, ground, location);

            string message = "";

            if (location.Type == "Base_Ally")
            {
                message = "The enemy has captured our base!";
            }
            else
            {
                message = "The enemy has captured " + location.Name + "!";
            }

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText(message);

            Button option1 = alerts.GetButton("Dialogue_Option1");
            option1.Visible = true;

            if (location.Type == "Base_Ally")
            {
                option1.Text = "[Retreat to Worldmap]";
            }
            else
            {
                option1.Text = "[Continue]";
            }
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

        public static void Alert_Story()
        {
            Handler.AlertType = "Story";
            string message = "";

            Army ally = CharacterManager.GetArmy("Ally");
            Squad squad = ally.Squads[0];
            Character hero = squad.GetLeader();

            Character spouse = CharacterManager.GetArmy("Special").Squads[0].Characters[0];

            Menu alerts = MenuManager.GetMenu("Alerts");
            alerts.Visible = true;

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
                    " any squad positioned at a Town or your Base to edit them. Right-click your starting squad now to edit it.";
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
                message = "Left-click and drag " + friend.Name + " from your Reserves to a position in your squad's Formation.";
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
                    " to sell them. Purchase a Cloth Helm from the market.";
            }
            else if (Handler.StoryStep == 13)
            {
                dialogue_name.Text = "System";
                message = "Now left-click the Exit Market button in the bottom-center to leave the Market Menu.";
            }
            else if (Handler.StoryStep == 14)
            {
                LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click your squad while they're at a Town so you can edit it and equip your new helm.";
            }
            else if (Handler.StoryStep == 15)
            {
                LocalPause();

                dialogue_name.Text = "System";
                message = "Right-click your character in the formation to change their equipment.";
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
                message = "\"Go cry about it to your local lord, " + local_lord.Name + ", I'm just following orders.\"";

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
                message = "You're now free to do as you please, and your objective is clear: kill " + local_lord.Name + ".\n- If your HP is low, you can park your" +
                    " squad at a Town/Base to recover 1 HP/EP per minute.\n- You will also now gain 1 Gold per hour for each Town you have liberated/captured." +
                    "\n\nGood luck!";

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

            Label dialogue = alerts.GetLabel("Dialogue");
            dialogue.Visible = true;
            dialogue.Text = WrapText(message);
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

                menu.GetButton("SaveExit").Visible = true;
                menu.GetButton("Exit").Visible = false;
            }
            else
            {
                menu.GetButton("Back").Visible = false;
                menu.GetButton("Play").Visible = true;

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

        private static void HourChanged(object sender, EventArgs e)
        {
            if (Handler.StoryStep > 48)
            {
                WorldUtil.Collect_Tax();
            }
        }
    }
}
