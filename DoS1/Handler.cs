using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using OP_Engine.Characters;
using OP_Engine.Inputs;
using OP_Engine.Inventories;
using OP_Engine.Sounds;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Weathers;

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
        public static int StoryStep = -1;
        public static bool AutoSave;

        public static bool Loaded;
        public static int Loading_Step;
        public static string Loading_Message;
        public static int Loading_IconCount;
        public static int Loading_Current;
        public static float Loading_Percent;
        public static CancellationTokenSource Loading_TokenSource;
        public static Task Loading_Task;

        public static bool RevisitMap;
        public static bool LocalMap;
        public static int Level;
        public static bool MovingGrid;
        public static int MoveGridDelay;
        public static bool TokenMenu;
        public static bool MovingSquads = false;
        public static bool RestingSquads = false;

        public static bool ManualPause;
        public static bool LocalPause;
        public static bool CombatPause;
        public static bool PauseDrawing;

        public static long Selected_Token = -1;

        public static Squad Hovering_Squad = null;
        public static long Selected_Squad;
        public static bool ViewOnly_Squad = false;

        public static long Selected_Character;
        public static bool ViewOnly_Character = false;

        public static long Selected_Item;
        public static bool ViewOnly_Item = false;

        public static string ItemFilter;
        public static Dictionary<int, Inventory> MarketInventories = new Dictionary<int, Inventory>();
        public static Inventory TradingMarket;

        public static int RecruitPrice = 100;
        public static Dictionary<int, Squad> AcademyRecruits = new Dictionary<int, Squad>();
        public static Squad TradingAcademy;

        public static System.Timers.Timer CombatTimer = new System.Timers.Timer(1);
        public static bool Combat;
        public static bool CombatFinishing;
        public static bool Retreating;
        public static int Element_Multiplier = 4;
        public static string Combat_Terrain;
        public static bool Combat_Ally_Base;
        public static bool Combat_Enemy_Base;
        public static long Combat_Ally_Squad = -1;
        public static long Combat_Enemy_Squad = -1;

        public static Character Dialogue_Character1;
        public static Character Dialogue_Character2;

        public static string[] SkinTones = new string[] { "Light", "Tan", "Dark", "Darkest" };
        public static string[] HeadStyles_Male = new string[] { "Head1", "Head2", "Head3" };
        public static string[] HeadStyles_Female = new string[] { "Head1", "Head2", "Head3" };
        public static string[] HairStyles_Male = new string[] { "Style1", "Style2", "Style3", "Bald"};
        public static string[] HairStyles_Female = new string[] { "Style1", "Style2", "Style3" };

        public static Dictionary<string, Color> HairColors = new Dictionary<string, Color>()
        {
            { "Brown", new Color(120, 90, 80, 255) },
            { "Red", new Color(230, 30, 10, 255) },
            { "Blonde", new Color(230, 230, 170, 255) },
            { "Black", new Color(32, 32, 32, 255) },
            { "Gray", new Color(160, 160, 160, 255) },
            { "White", new Color(240, 240, 240, 255) }
        };

        public static Dictionary<string, Color> EyeColors = new Dictionary<string, Color>()
        {
            { "Green", new Color(0, 160, 0, 255) },
            { "Brown", new Color(100, 70, 60, 255) },
            { "Blue", new Color(0, 0, 160, 255) },
            { "Cyan", new Color(0, 180, 200, 255) },
            { "Purple", new Color(120, 0, 160, 255) },
            { "Gold", new Color(200, 200, 140, 255) },
            { "Gray", new Color(160, 160, 160, 255) }
        };

        public static List<Something> light_maps = new List<Something>();
        public static int light_tile_distance = 1;

        public static List<string> Saves = new List<string>();
        public static string Selected_Save;

        public static oPathing Pathing = new oPathing();
        public static Dictionary<int, int> XP_Needed_ForLevels = new Dictionary<int, int>();

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
            int xp = 10;
            for (int i = 2; i <= 100; i++)
            {
                XP_Needed_ForLevels.Add(i, xp);
                xp += 2;
            }

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

                SortSaves();
            }

            string config = AssetManager.Files["Config"];
            if (!File.Exists(config))
            {
                File.Create(config).Close();
                SaveUtil.ExportINI();
            }
            else
            {
                LoadUtil.ParseINI(config);
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
                "Controls",
                "Particles",
                "Screens",
                "Shaders",
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

        public static void SortSaves()
        {
            DirectoryInfo saveDirs = new DirectoryInfo(AssetManager.Directories["Saves"]);

            for (int i = 0; i < Saves.Count; i++)
            {
                for (int j = 0; j < Saves.Count - 1; j++)
                {
                    DirectoryInfo saveDir_a = null;
                    DirectoryInfo saveDir_b = null;

                    foreach (DirectoryInfo existing in saveDirs.GetDirectories())
                    {
                        if (existing.Name == Saves[j])
                        {
                            saveDir_a = existing;
                        }
                        else if (existing.Name == Saves[j + 1])
                        {
                            saveDir_b = existing;
                        }
                    }

                    int day_a = saveDir_a.LastWriteTime.Day;
                    int hour_a = saveDir_a.LastWriteTime.Hour;
                    int minute_a = saveDir_a.LastWriteTime.Minute;
                    int second_a = saveDir_a.LastWriteTime.Second;
                    int total_a = (((day_a * 24) * 60) * 60) + ((hour_a * 60) * 60) + (minute_a * 60) + second_a;

                    int day_b = saveDir_b.LastWriteTime.Day;
                    int hour_b = saveDir_b.LastWriteTime.Hour;
                    int minute_b = saveDir_b.LastWriteTime.Minute;
                    int second_b = saveDir_b.LastWriteTime.Second;
                    int total_b = (((day_b * 24) * 60) * 60) + ((hour_b * 60) * 60) + (minute_b * 60) + second_b;

                    if (total_a < total_b)
                    {
                        string temp = Saves[j];
                        Saves[j] = Saves[j + 1];
                        Saves[j + 1] = temp;
                    }
                }
            }
        }

        #endregion

        #region Load Stuff

        private static void LoadControls()
        {
            InputManager.Keyboard.KeysMapped.Add("Debug", Keys.OemTilde);
            InputManager.Keyboard.KeysMapped.Add("Esc", Keys.Escape);
            InputManager.Keyboard.KeysMapped.Add("Enter", Keys.Enter);
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

        public static void LoadTextures()
        {
            if (Main.Game.GraphicsManager.GraphicsDevice != null)
            {
                Loading_Current = 0;
                Loading_IconCount = 0;
                Loading_Message = "Loading Textures";

                string[] dirs = { "Armors", "Bodies", "Effects", "Eyes", "Hairs", "Heads", "Helms", "Icons", "Shields", "Tiles", "Weapons" };

                int total = 0;

                DirectoryInfo dir = new DirectoryInfo(AssetManager.Directories["Textures"]);

                foreach (var sub_dir in dir.GetDirectories())
                {
                    if (dirs.Contains(sub_dir.Name))
                    {
                        foreach (var file in sub_dir.GetFiles("*.png"))
                        {
                            total++;
                        }
                    }
                }

                foreach (var sub_dir in dir.GetDirectories())
                {
                    if (dirs.Contains(sub_dir.Name))
                    {
                        FileInfo[] files = sub_dir.GetFiles("*.png");

                        int fileCount = files.Length;
                        for (int i = 0; i < fileCount; i++)
                        {
                            FileInfo file = files[i];

                            var name = Path.GetFileNameWithoutExtension(file.Name);
                            if (!AssetManager.Textures.ContainsKey(name))
                            {
                                using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
                                {
                                    if (Main.Game.GraphicsManager.GraphicsDevice != null)
                                    {
                                        Texture2D texture = Texture2D.FromStream(Main.Game.GraphicsManager.GraphicsDevice, fileStream);
                                        texture.Name = name;
                                        AssetManager.Textures.Add(name, texture);

                                        Loading_Current++;
                                        Loading_Percent = (Loading_Current * 100) / total;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Get Stuff

        public static long GetID()
        {
            ID++;
            return ID;
        }

        public static Character GetHero()
        {
            return CharacterManager.GetArmy("Ally").Squads[0].GetLeader();
        }

        #endregion

        #endregion
    }
}
