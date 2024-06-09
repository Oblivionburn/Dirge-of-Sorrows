using System;
using System.Linq;
using System.Threading;

using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Scenes;
using OP_Engine.Sounds;
using OP_Engine.Weathers;
using OP_Engine.Utility;
using OP_Engine.Time;
using OP_Engine.Tiles;

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
            ArmyUtil.MoveSquads();
        }

        public static void MinuteChanged(object sender, EventArgs e)
        {
            WorldUtil.AnimateTiles();
        }
    }
}
