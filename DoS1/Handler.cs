using System;
using System.IO;
using System.Timers;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using OP_Engine.Characters;
using OP_Engine.Sounds;
using OP_Engine.Utility;
using OP_Engine.Weathers;
using OP_Engine.Inputs;
using OP_Engine.Time;
using OP_Engine.Inventories;

using DoS1.Util;

namespace DoS1
{
    public class Handler : GameComponent
    {
        #region Variables

        public static long ID;
        public static int Gold = 1000;
        public static long MainCharacter_ID;
        public static string AlertType;

        public static bool LocalMap;
        public static int Level;
        public static bool MovingGrid;
        public static int MoveGridDelay;
        public static bool MovingSquads = false;
        public static bool RestingSquads = false;

        public static bool ManualPause;
        public static bool LocalPause;

        public static long Selected_Token = -1;

        public static Squad Hovering_Squad = null;
        public static long Selected_Squad;
        public static bool ViewOnly_Squad = false;

        public static long Selected_Character;
        public static bool ViewOnly_Character = false;

        public static long Selected_Item;
        public static bool ViewOnly_Item = false;

        public static string ItemFilter;
        public static Dictionary<int, Inventory> ShopInventories = new Dictionary<int, Inventory>();
        public static Inventory TradingShop;

        public static int RecruitPrice = 100;
        public static Dictionary<int, Squad> AcademyRecruits = new Dictionary<int, Squad>();
        public static Squad TradingAcademy;

        public static Timer CombatTimer = new Timer(1);
        public static bool Combat;
        public static int Element_Multiplier = 8;
        public static string Combat_Terrain;
        public static bool Combat_Ally_Base;
        public static bool Combat_Enemy_Base;
        public static long Combat_Ally_Squad = -1;
        public static long Combat_Enemy_Squad = -1;

        public static Character Dialogue_Character1;
        public static Character Dialogue_Character2;

        public static string[] SkinTones = new string[] { "Light", "Tan", "Dark", "Darkest" };
        public static string[] HeadStyles = new string[] { "Head1", "Head2", "Head3", "Head4" };
        public static string[] HairStyles = new string[] { "Style1", "Style2", "Style3", "Style4", "Style5", "Style6", "Bald"};

        public static Dictionary<string, Color> HairColors = new Dictionary<string, Color>()
        {
            { "Brown", new Color(120, 90, 80, 255) },
            { "Red", new Color(230, 30, 10, 255) },
            { "Blonde", new Color(230, 230, 170, 255) },
            { "Black", new Color(32, 32, 32, 255) },
            { "Gray", new Color(160, 160, 160, 255) },
            { "White", new Color(240, 240, 240, 255) },
            { "Purple", new Color(120, 0, 160, 255) },
            { "Blue", new Color(0, 0, 240, 255) },
            { "Cyan", new Color(0, 190, 200, 255) },
            { "Green", new Color(0, 200, 0, 255) },
            { "Pink", new Color(240, 64, 200, 255) }
        };

        public static Dictionary<string, Color> EyeColors = new Dictionary<string, Color>()
        {
            { "Green", new Color(0, 160, 0, 255) },
            { "Brown", new Color(100, 70, 60, 255) },
            { "Blue", new Color(0, 0, 160, 255) },
            { "Cyan", new Color(0, 180, 200, 255) },
            { "Purple", new Color(120, 0, 160, 255) },
            { "Gold", new Color(200, 200, 140, 255) },
            { "Red", new Color(160, 0, 0, 255) },
            { "Black", new Color(32, 32, 32, 255) },
            { "Gray", new Color(160, 160, 160, 255) }
        };

        public static List<Something> light_maps = new List<Something>();
        public static int light_tile_distance = 1;

        public static List<string> Saves = new List<string>();
        public static string Selected_Save;

        public static oPathing Pathing = new oPathing();

        #endregion

        #region Constructors

        public Handler(Game game) : base(game)
        {

        }

        #endregion

        #region Methods

        #region Init

        public static void Init(Game game)
        {
            AssetManager.Init(game, Environment.CurrentDirectory + @"\Content");

            Load_Init();

            TimeManager.Init();
            TimeManager.Reset(TimeRate.Second, 1, 1, 1, 12);
            TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm, WeatherType.Snow };

            string saves = AssetManager.Directories["Saves"];
            if (!Directory.Exists(saves))
            {
                Directory.CreateDirectory(saves);
            }
            else
            {
                //Load Saves
                DirectoryInfo savesDir = new DirectoryInfo(AssetManager.Directories["Saves"]);
                foreach (DirectoryInfo saveDir in savesDir.GetDirectories())
                {
                    Saves.Add(saveDir.Name);
                }
            }

            string config = AssetManager.Files["Config"];
            if (!File.Exists(config))
            {
                File.Create(config).Close();
                Save.ExportINI();
            }
            else
            {
                Load.ParseINI(config);
            }
        }

        public static void Load_Init()
        {
            AssetManager.Directories.Add("Saves", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Dirge of Sorrows"));
            AssetManager.Files.Add("Config", Path.Combine(AssetManager.Directories["Saves"], "config.ini"));

            AssetManager.LoadFonts();
            AssetManager.LoadShaders(Main.Game.GraphicsManager.GraphicsDevice);

            //Textures
            string[] textures =
            {
                "Armors",
                "Bodies",
                "Controls",
                "Effects",
                "Eyes",
                "Hairs",
                "Heads",
                "Helms",
                "Icons",
                "Particles",
                "Screens",
                "Shaders",
                "Shields",
                "Tiles",
                "Weapons"
            };
            foreach (string dir in textures)
            {
                AssetManager.LoadTextures(Main.Game.GraphicsManager.GraphicsDevice, dir);
            }

            //Sounds
            AssetManager.LoadSounds();

            string[] sounds =
            {
                "Bow",
                "Cast",
                "Click",
                "Death",
                "Earth",
                "Equip",
                "Fire",
                "Heal",
                "Ice",
                "IronSword",
                "Leech",
                "Poison",
                "Punch",
                "Purchase",
                "Shock",
                "Siphon",
                "SteelSword",
                "Swing",
                "Thump",
                "Thunder",
                "Water",
                "Wind",
                "WoodSword"
            };
            foreach (string dir in sounds)
            {
                AssetManager.LoadSounds(dir);
            }
            SoundManager.SoundVolume = 1;
            SoundManager.SoundEnabled = true;

            //Ambient
            AssetManager.LoadAmbient();
            SoundManager.AmbientFade = 1;
            SoundManager.AmbientVolume = 0.8f;
            SoundManager.AmbientEnabled = true;

            //Music
            string[] music =
            {
                "Combat",
                "Desert",
                "GameOver",
                "Plains",
                "Snowy",
                "Title",
                "Worldmap"
            };
            foreach (string dir in music)
            {
                AssetManager.LoadMusic(dir);
            }
            SoundManager.MusicVolume = 0.9f;
            SoundManager.MusicEnabled = true;

            LoadControls();

            WeatherManager.Load(new List<Texture2D>
            {
                AssetManager.Textures["Rain"],
                AssetManager.Textures["Snow"],
                AssetManager.Textures["Storm"]
            });
        }

        #endregion

        #region Load Stuff

        private static void LoadControls()
        {
            InputManager.Keyboard.KeysMapped.Add("Debug", Keys.OemTilde);
            InputManager.Keyboard.KeysMapped.Add("Esc", Keys.Escape);
            InputManager.Keyboard.KeysMapped.Add("Space", Keys.Space);
            InputManager.Keyboard.KeysMapped.Add("Backspace", Keys.Back);
            InputManager.Keyboard.KeysMapped.Add("LeftShift", Keys.LeftShift);
            InputManager.Keyboard.KeysMapped.Add("RightShift", Keys.RightShift);

            for (char c = 'A'; c <= 'Z'; c++)
            {
                Keys key = InputManager.GetKey(c.ToString());
                InputManager.Keyboard.KeysMapped.Add(c.ToString(), key);
            }

            for (int i = 0; i < 10; i++)
            {
                Keys key = InputManager.GetKey("D" + i);
                InputManager.Keyboard.KeysMapped.Add(i.ToString(), key);
            }
        }

        #endregion

        #region Get Stuff

        public static long GetID()
        {
            ID++;
            return ID;
        }

        #endregion

        #endregion
    }
}
