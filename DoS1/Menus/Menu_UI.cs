using System.Linq;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

using OP_Engine.Scenes;
using OP_Engine.Menus;
using OP_Engine.Controls;
using OP_Engine.Sounds;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Inputs;
using OP_Engine.Tiles;
using OP_Engine.Characters;
using OP_Engine.Weathers;

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_UI : Menu
    {
        #region Variables

        

        #endregion

        #region Constructor

        public Menu_UI(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "UI";
            Load(content);
        }

        #endregion

        #region Methods

        public override void Update(Game gameRef, ContentManager content)
        {
            if (Visible &&
                !TimeManager.Paused)
            {
                Scene scene = SceneManager.GetScene("Worldmap");
                if (Main.LocalMap)
                {
                    scene = SceneManager.GetScene("Localmap");
                }

                UpdateControls(scene.World);

                if (!Main.LocalPause)
                {
                    UpdateTime();
                }

                base.Update(gameRef, content);
            }
        }

        private void UpdateControls(World world)
        {
            if (!Handler.MoveGridDelay)
            {
                bool hovering_button = HoveringButton();
                bool hovering_squad = false;
                bool hovering_location = false;
                bool hovering_selection = false;

                if (!hovering_button)
                {
                    Map map = null;
                    if (world.Maps.Any())
                    {
                        map = world.Maps[0];
                    }

                    Layer pathing = null;
                    if (map != null)
                    {
                        pathing = map.GetLayer("Pathing");
                    }

                    hovering_squad = HoveringSquad(world);

                    if (!hovering_squad)
                    {
                        if (pathing != null)
                        {
                            pathing.Visible = false;
                        }
                        
                        GetPicture("Select").Visible = false;

                        hovering_location = HoveringLocation(map);

                        if (Handler.Selected_Token != -1)
                        {
                            hovering_selection = HoveringSelection(world, map);
                        }
                    }

                    if (hovering_squad)
                    {
                        if (Handler.Selected_Token != -1)
                        {
                            //Hide selection box while hovering a token
                            if (pathing != null)
                            {
                                pathing.Visible = false;
                            }
                            
                            GetPicture("Select").Visible = false;
                        }
                    }
                    else if (hovering_selection)
                    {
                        if (pathing != null)
                        {
                            pathing.Visible = true;
                        }

                        GetPicture("Select").Visible = true;
                    }
                }

                if (!hovering_button &&
                    !hovering_squad &&
                    !hovering_location)
                {
                    GetLabel("Examine").Visible = false;

                    if (Handler.Selected_Token == -1)
                    {
                        GetPicture("Highlight").Visible = false;
                    }
                }
            }
            else
            {
                GetLabel("Examine").Visible = false;

                if (InputManager.Mouse_LB_Pressed)
                {
                    Handler.MoveGridDelay = false;
                }
            }

            if (InputManager.Mouse_LB_Held &&
                InputManager.Mouse.Moved)
            {
                WorldUtil.MoveGrid(world);
            }
            else if (InputManager.Mouse_ScrolledUp)
            {
                WorldUtil.ZoomIn();
            }
            else if (InputManager.Mouse_ScrolledDown)
            {
                WorldUtil.ZoomOut();
            }

            if (InputManager.KeyPressed("Space"))
            {
                LocalPauseToggle();
            }
            else if (InputManager.KeyPressed("Esc"))
            {
                TimeManager.Paused = true;
                Active = false;
                Visible = false;
                MenuManager.ChangeMenu("Main");
            }
        }

        private bool HoveringButton()
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

            return found;
        }

        private bool HoveringSquad(World world)
        {
            bool hovered_squad = false;

            foreach (Army army in CharacterManager.Armies)
            {
                foreach (Squad squad in army.Squads)
                {
                    if (squad.Visible)
                    {
                        Picture highlight = GetPicture("Highlight");

                        if (InputManager.MouseWithin(squad.Region.ToRectangle))
                        {
                            hovered_squad = true;

                            Map map = null;

                            if (world.Maps.Any())
                            {
                                map = world.Maps[0];
                            }
                            
                            GameUtil.Examine(this, squad.Name);

                            highlight.Region = squad.Region;
                            highlight.Visible = true;

                            if (squad.Type == "Player")
                            {
                                highlight.DrawColor = new Color(0, 0, 255, 255);
                            }
                            else if (squad.Type == "Enemy")
                            {
                                highlight.DrawColor = new Color(255, 0, 0, 255);
                            }

                            if (InputManager.Mouse_LB_Pressed)
                            {
                                AssetManager.PlaySound_Random("Click");

                                hovered_squad = false;

                                if (Handler.Selected_Token == squad.ID)
                                {
                                    WorldUtil.DeselectToken(map, this);
                                }
                                else
                                {
                                    //Select new token
                                    Handler.Selected_Token = squad.ID;
                                }

                                InputManager.Mouse.Flush();

                                break;
                            }
                            else if (Handler.Selected_Token == squad.ID)
                            {
                                highlight.DrawColor = new Color(0, 255, 255, 255);
                            }
                            else
                            {
                                WorldUtil.DisplayPath_Squad(this, map, squad);
                            }
                        }
                        else if (Handler.Selected_Token == squad.ID)
                        {
                            highlight.DrawColor = new Color(0, 0, 255, 255);
                        }
                    }
                }
            }

            return hovered_squad;
        }

        private bool HoveringLocation(Map map)
        {
            bool found = false;

            if (map == null)
            {
                return false;
            }

            Layer locations = map.GetLayer("Locations");
            for (int i = 0; i < locations.Tiles.Count; i++)
            {
                Tile location = locations.Tiles[i];

                if (location.Visible)
                {
                    if (InputManager.MouseWithin(location.Region.ToRectangle))
                    {
                        found = true;

                        GameUtil.Examine(this, location.Name);

                        if (Handler.Selected_Token == -1)
                        {
                            Picture highlight = GetPicture("Highlight");
                            highlight.Region = location.Region;
                            highlight.Visible = true;
                            highlight.DrawColor = new Color(255, 255, 255, 255);
                        }

                        if (InputManager.Mouse_LB_Pressed)
                        {
                            if (map.Name == "World Map")
                            {
                                found = false;
                                CheckClick_Worldmap_Location(map, location, i);
                            }
                            
                            break;
                        }
                    }
                }
            }

            return found;
        }

        private bool HoveringSelection(World world, Map map)
        {
            bool found = false;

            if (map == null)
            {
                return false;
            }

            Layer ground = map.GetLayer("Ground");
            foreach (Tile tile in ground.Tiles)
            {
                if (InputManager.MouseWithin(tile.Region.ToRectangle))
                {
                    found = true;

                    Picture select = GetPicture("Select");
                    select.Region = tile.Region;

                    if (InputManager.Mouse_LB_Pressed)
                    {
                        found = false;
                        CheckClick_Selection(world, tile);
                        break;
                    }
                    else if (InputManager.Mouse_RB_Pressed)
                    {
                        WorldUtil.DeselectToken(map, this);
                    }
                    else
                    {
                        WorldUtil.DisplayPath_Temp(map, tile);
                    }
                }
            }

            return found;
        }

        private void CheckClick_Worldmap_Location(Map map, Tile tile, int location_num)
        {
            AssetManager.PlaySound_Random("Click");

            Layer ground = map.GetLayer("Ground");
            Tile ground_tile = ground.GetTile(new Vector2(tile.Location.X, tile.Location.Y));

            WorldGen.GenLocalmap(ground_tile, location_num);

            World world = SceneManager.GetScene("Localmap").World;
            Map localmap = world.Maps[0];

            if (tile.Type.Contains("Snow") ||
                tile.Type.Contains("Ice"))
            {
                TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Snow };
            }
            else if (!tile.Type.Contains("Desert"))
            {
                TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm };
            }

            Main.LocalMap = true;

            GetButton("Worldmap").Visible = true;
            GetButton("PlayPause").Enabled = true;
            GetButton("Speed").Enabled = true;

            Army allies = CharacterManager.GetArmy("Player");
            Squad ally_squad = allies.Squads[0];

            Army enemies = CharacterManager.GetArmy("Enemy");
            Squad enemy_squad = enemies.Squads[0];

            WorldUtil.AllyToken_Start(ally_squad, localmap);
            WorldUtil.EnemyToken_Start(enemy_squad, localmap);

            SceneManager.ChangeScene("Localmap");
            WorldUtil.Resize_OnStart(localmap);

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;
        }

        private void CheckClick_Selection(World world, Tile tile)
        {
            AssetManager.PlaySound_Random("Click");

            //Select tile in local map for pathing
            Map map = world.Maps[0];
            ArmyUtil.SetPath(this, map, tile);
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            if (button.Name == "PlayPause")
            {
                LocalPauseToggle();
            }
            else if (button.Name == "Speed")
            {
                SpeedToggle();
            }
            else if (button.Name == "Worldmap")
            {
                BackToWorldmap();
            }
            else
            {
                TimeManager.Paused = true;
                Active = false;
                Visible = false;
                MenuManager.ChangeMenu(button.Name);

                InputManager.Mouse.Flush();
                InputManager.Keyboard.Flush();
            }
        }

        private void UpdateTime()
        {
            long NewHours = TimeManager.Now.Hours;
            string hours;
            string minutes;

            bool pm = false;

            if (NewHours > 12)
            {
                NewHours = NewHours - 12;
                pm = true;
            }
            else if (NewHours == 0)
            {
                NewHours = 12;
            }
            else if (NewHours == 12)
            {
                pm = true;
            }

            if (NewHours < 10)
            {
                hours = "0" + NewHours.ToString();
            }
            else
            {
                hours = NewHours.ToString();
            }

            if (TimeManager.Now.Minutes < 10)
            {
                minutes = "0" + TimeManager.Now.Minutes.ToString();
            }
            else
            {
                minutes = TimeManager.Now.Minutes.ToString();
            }

            Label time = GetLabel("Time");
            Label date = GetLabel("Date");

            if (pm == false)
            {
                time.Text = hours + ":" + minutes + " AM";
            }
            else
            {
                time.Text = hours + ":" + minutes + " PM";
            }

            date.Text = "Day " + TimeManager.Now.Days.ToString();
        }

        private void LocalPauseToggle()
        {
            Button button = GetButton("PlayPause");

            if (button.Value == 0)
            {
                Main.LocalPause = true;
                SoundManager.AmbientPaused = true;
                button.Value = 1;
                button.HoverText = "Play";
                button.Texture = AssetManager.Textures["Button_Play"];
                button.Texture_Highlight = AssetManager.Textures["Button_Play_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Play_Disabled"];
            }
            else if (button.Value == 1)
            {
                Main.LocalPause = false;
                SoundManager.AmbientPaused = false;
                button.Value = 0;
                button.HoverText = "Pause";
                button.Texture = AssetManager.Textures["Button_Pause"];
                button.Texture_Highlight = AssetManager.Textures["Button_Pause_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Pause_Disabled"];
            }
        }

        private void SpeedToggle()
        {
            Button button = GetButton("Speed");

            if (button.Value == 0)
            {
                Main.TimeSpeed = 2;
                button.Value = 1;
                button.HoverText = "x2";
                button.Texture = AssetManager.Textures["Button_Speed2"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
            }
            else if (button.Value == 1)
            {
                Main.TimeSpeed = 3;
                button.Value = 2;
                button.HoverText = "x3";
                button.Texture = AssetManager.Textures["Button_Speed3"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
            }
            else if (button.Value == 2)
            {
                Main.TimeSpeed = 4;
                button.Value = 3;
                button.HoverText = "x4";
                button.Texture = AssetManager.Textures["Button_Speed4"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
            }
            else if (button.Value == 3)
            {
                Main.TimeSpeed = 1;
                button.Value = 0;
                button.HoverText = "x1";
                button.Texture = AssetManager.Textures["Button_Speed1"];
                button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
            }
        }

        private void BackToWorldmap()
        {
            TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear };

            Main.LocalMap = false;

            GetButton("Worldmap").Visible = false;
            GetButton("PlayPause").Enabled = false;
            GetButton("Speed").Enabled = false;

            SceneManager.ChangeScene("Worldmap");

            SoundManager.StopMusic();
            SoundManager.NeedMusic = true;
        }

        public override void Load(ContentManager content)
        {
            Clear();

            AddButton(Handler.GetID(), "Main", AssetManager.Textures["Button_Menu"], AssetManager.Textures["Button_Menu_Hover"], AssetManager.Textures["Button_Menu_Disabled"],
                new Region(0, 0, 0, 0), Color.White * 0.8f, true);
            GetButton("Main").HoverText = "System";

            AddButton(Handler.GetID(), "Army", AssetManager.Textures["Button_Army"], AssetManager.Textures["Button_Army_Hover"], AssetManager.Textures["Button_Army_Disabled"],
                new Region(0, 0, 0, 0), Color.White * 0.8f, true);
            GetButton("Army").HoverText = "Army";

            AddButton(Handler.GetID(), "Inventory", AssetManager.Textures["Button_Inventory"], AssetManager.Textures["Button_Inventory_Hover"], AssetManager.Textures["Button_Inventory_Disabled"],
                new Region(0, 0, 0, 0), Color.White * 0.8f, true);
            GetButton("Inventory").HoverText = "Inventory";

            AddButton(Handler.GetID(), "Worldmap", AssetManager.Textures["Button_Exit"], AssetManager.Textures["Button_Exit_Hover"], null,
                new Region(0, 0, 0, 0), Color.White * 0.8f, false);
            GetButton("Worldmap").HoverText = "Back to Worldmap";

            AddButton(Handler.GetID(), "PlayPause", AssetManager.Textures["Button_Pause"], AssetManager.Textures["Button_Pause_Hover"], AssetManager.Textures["Button_Pause_Disabled"],
                new Region(0, 0, 0, 0), Color.White * 0.8f, true);
            GetButton("PlayPause").HoverText = "Pause";
            GetButton("PlayPause").Enabled = false;

            AddButton(Handler.GetID(), "Speed", AssetManager.Textures["Button_Speed1"], AssetManager.Textures["Button_Speed1_Hover"], AssetManager.Textures["Button_Speed1_Disabled"],
                new Region(0, 0, 0, 0), Color.White * 0.8f, true);
            GetButton("Speed").HoverText = "x1";
            GetButton("Speed").Enabled = false;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Date", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Date").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Time", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Time").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["Frame"],
                new Region(0, 0, 0, 0), false);

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Highlight"], new Region(0, 0, 0, 0), new Color(255, 255, 255, 255), false);
            AddPicture(Handler.GetID(), "Select", AssetManager.Textures["Highlight"], new Region(0, 0, 0, 0), new Color(0, 255, 0, 255), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            GetButton("PlayPause").Region = new Region(Main.Game.ScreenWidth - (width * 4), 0, width, height);
            GetButton("Speed").Region = new Region(Main.Game.ScreenWidth - (width * 3), 0, width, height);

            GetLabel("Date").Region = new Region(Main.Game.ScreenWidth - (width * 2), 0, width * 2, height / 2);
            GetLabel("Time").Region = new Region(Main.Game.ScreenWidth - (width * 2), width / 2, width * 2, height / 2);

            GetButton("Main").Region = new Region(0, 0, width, height);
            GetButton("Army").Region = new Region(width, 0, width, height);
            GetButton("Inventory").Region = new Region(width * 2, 0, width, height);
            GetButton("Worldmap").Region = new Region(width * 3, 0, width, height);

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
