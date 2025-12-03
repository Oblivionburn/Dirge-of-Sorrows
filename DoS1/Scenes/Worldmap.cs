using DoS1.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
using System.Linq;

namespace DoS1.Scenes
{
    public class Worldmap : Scene
    {
        #region Variables

        

        #endregion

        #region Constructors

        public Worldmap(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Worldmap";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible &&
                Active &&
                !TimeManager.Paused)
            {
                if (SoundManager.NeedMusic)
                {
                    SoundManager.MusicLooping = false;
                    AssetManager.PlayMusic_Random("Worldmap", true);
                }

                if (Handler.StoryStep == -1)
                {
                    WorldUtil.UpdateTime();
                    GameUtil.Alert_Tutorial();
                }
                else if (Handler.StoryStep == 0)
                {
                    StoryUtil.Alert_Story(Menu);
                }

                if (Handler.MovingGrid)
                {
                    WorldUtil.MoveGrid(World);

                    if (InputManager.Mouse_LB_Pressed)
                    {
                        Handler.MoveGridDelay = 0;
                        Handler.MovingGrid = false;
                    }
                }
                else if (Handler.AlertType == "Story" ||
                         Handler.AlertType == "Generic" ||
                         string.IsNullOrEmpty(Handler.AlertType))
                {
                    UpdateControls(World);
                }

                base.Update(gameRef, content);
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Menu.Pictures)
                {
                    picture.Draw(spriteBatch);
                }

                foreach (Label label in Menu.Labels)
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        private void UpdateControls(World world)
        {
            bool hovering_location = false;

            Map map = WorldUtil.GetMap(world);
            if (map != null)
            {
                hovering_location = HoveringLocation(map);
            }

            if (!hovering_location)
            {
                Menu.GetLabel("Examine").Visible = false;
                Menu.GetPicture("Highlight").Visible = false;
            }

            if (InputManager.Mouse_LB_Held &&
                InputManager.Mouse.Moved)
            {
                Handler.MoveGridDelay++;

                if (Handler.MoveGridDelay >= 4)
                {
                    WorldUtil.MoveGrid(world);
                }
            }

            if (InputManager.Mouse_ScrolledUp)
            {
                WorldUtil.ZoomIn();
            }
            else if (InputManager.Mouse_ScrolledDown)
            {
                WorldUtil.ZoomOut();
            }
        }

        private bool HoveringLocation(Map map)
        {
            bool found = false;

            Layer locations = map.GetLayer("Locations");
            for (int i = 0; i < locations.Tiles.Count; i++)
            {
                Tile location = locations.Tiles[i];

                if (location.Visible)
                {
                    if (InputManager.MouseWithin(location.Region.ToRectangle))
                    {
                        found = true;

                        ExamineLocation(location, i + 1);

                        Picture highlight = Menu.GetPicture("Highlight");
                        highlight.Region = location.Region;
                        highlight.Visible = true;

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            found = false;
                            CheckClick_Location(map, location, i);
                            break;
                        }
                    }
                }
            }

            return found;
        }

        private void CheckClick_Location(Map map, Tile tile, int location_num)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.MoveGridDelay = 0;
            Handler.Level = location_num;

            Army enemies = CharacterManager.GetArmy("Enemy");
            Army allies = CharacterManager.GetArmy("Ally");

            Layer ground = map.GetLayer("Ground");
            Tile ground_tile = ground.GetTile(new Vector2(tile.Location.X, tile.Location.Y));

            Map localmap;

            World world = SceneManager.GetScene("Localmap").World;
            if (world.Maps.Count >= Handler.Level + 1)
            {
                //Revisit map
                localmap = world.Maps[Handler.Level];
                localmap.Visible = true;
            }
            else
            {
                //Generate new map
                if (!world.Maps.Any())
                {
                    world = new World
                    {
                        ID = Handler.GetID(),
                        Visible = true,
                        DrawColor = Color.White
                    };

                    SceneManager.GetScene("Localmap").World = world;
                }

                localmap = WorldGen.GenLocalmap(world, ground_tile, Handler.Level);
                world.Maps.Add(localmap);

                //Generate enemies
                ArmyUtil.Gen_EnemySquads(enemies, Handler.Level);
            }

            //Hide other maps
            for (int i = 0; i < world.Maps.Count; i++)
            {
                Map existing = world.Maps[i];
                if (existing.ID != localmap.ID)
                {
                    existing.Visible = false;
                }
            }

            if (localmap != null)
            {
                //Set market inventories
                if (!Handler.MarketInventories.ContainsKey(Handler.Level))
                {
                    Handler.MarketInventories.Add(Handler.Level, InventoryUtil.Gen_Market(Handler.Level + 1));
                }
                Handler.TradingMarket = Handler.MarketInventories[Handler.Level];

                //Set academy units
                if (!Handler.AcademyRecruits.ContainsKey(Handler.Level))
                {
                    Handler.AcademyRecruits.Add(Handler.Level, ArmyUtil.Gen_Academy());
                }
                Handler.TradingAcademy = Handler.AcademyRecruits[Handler.Level];

                //Set weather
                if (ground_tile.Type.Contains("Snow") ||
                    ground_tile.Type.Contains("Ice"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Snow, WeatherType.Fog };
                }
                else if (!ground_tile.Type.Contains("Desert"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm, WeatherType.Fog };
                }

                Handler.LocalMap = true;

                Menu ui = MenuManager.GetMenu("UI");
                ui.GetButton("PlayPause").Enabled = true;
                ui.GetButton("Speed").Enabled = true;
                ui.GetLabel("Level").Text = "Level " + (Handler.Level + 1);

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

                if (tile.Type == "Base_Enemy")
                {
                    Handler.RevisitMap = false;

                    //Deploy all squads at bases
                    ArmyUtil.DeployArmy(enemies, localmap, "Enemy");
                    ArmyUtil.DeployArmy(allies, localmap, "Ally");
                }
                else
                {
                    Handler.RevisitMap = true;

                    //Deploy just hero squad at ally base
                    ArmyUtil.DeploySquad(allies.Squads[0].ID);
                }

                if (Handler.StoryStep == 0)
                {
                    MenuManager.GetMenu("Alerts").Visible = false;
                    Handler.StoryStep++;
                }

                //Switch to local map
                SceneManager.ChangeScene("Localmap");

                WorldUtil.Resize_OnStart(Menu, localmap);

                SoundManager.StopMusic();
                SoundManager.NeedMusic = true;
            }
        }

        private void ExamineLocation(Tile location, int location_num)
        {
            Label examine = Menu.GetLabel("Examine");
            examine.Text = location.Name + "\n(Level " + location_num + ")";

            int width = Main.Game.MenuSize.X * 4;
            int height = Main.Game.MenuSize.X + (Main.Game.MenuSize.X / 2);

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

        public override void Load(ContentManager content)
        {
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            Menu.AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);

            Menu.Visible = true;
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (World.Maps.Any())
            {
                WorldUtil.Resize_OnZoom(Menu, World.Maps[0], true);
            }
        }

        #endregion
    }
}
