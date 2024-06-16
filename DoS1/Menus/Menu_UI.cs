using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

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
            if (Handler.Combat)
            {
                Active = false;
                Visible = false;
            }

            if (Visible &&
                !TimeManager.Paused)
            {
                Scene scene = SceneManager.GetScene("Worldmap");
                if (Handler.LocalMap)
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Pictures)
                {
                    if (picture.Name != "Highlight" &&
                        picture.Name != "Select")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Highlight")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Pictures)
                {
                    if (picture.Name == "Select")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button button in Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Slider slider in Sliders)
                {
                    slider.Draw(spriteBatch, slider.DrawColor);
                }

                foreach (InputBox input in Inputs)
                {
                    input.Draw(spriteBatch);
                }

                foreach (ProgressBar progressBar in ProgressBars)
                {
                    progressBar.Draw(spriteBatch);
                }

                foreach (Label label in Labels)
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        private void UpdateControls(World world)
        {
            if (!Handler.MovingGrid)
            {
                bool hovering_button = HoveringButton();
                bool hovering_squad = false;
                bool hovering_location = false;
                bool hovering_selection = false;

                if (!hovering_button &&
                    string.IsNullOrEmpty(Handler.AlertType))
                {
                    Map map = null;
                    if (world.Maps.Any())
                    {
                        map = world.Maps[0];
                    }

                    if (Handler.Selected_Token != -1)
                    {
                        hovering_selection = HoveringSelection(world, map);
                        if (hovering_selection)
                        {
                            GetPicture("Select").Visible = true;
                        }
                    }

                    Layer pathing = null;
                    if (map != null)
                    {
                        pathing = map.GetLayer("Pathing");
                    }

                    hovering_squad = HoveringSquad(world);
                    if (!hovering_squad)
                    {
                        if (Handler.Selected_Token == -1 &&
                            pathing != null)
                        {
                            pathing.Visible = false;
                        }

                        hovering_location = HoveringLocation(map);
                    }

                    if (!hovering_selection &&
                        !hovering_squad &&
                        !hovering_location)
                    {
                        GetPicture("Select").Visible = false;
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
                    Handler.MoveGridDelay = 0;
                    Handler.MovingGrid = false;
                }
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
                GameUtil.Toggle_Pause();
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

                        if (button.Name == "Alert")
                        {
                            if (Handler.AlertType == "Combat")
                            {
                                GetLabel("Combat_Attacker").TextColor = button.TextColor_Selected;
                                GetLabel("Combat_VS").TextColor = button.TextColor_Selected;
                                GetLabel("Combat_Defender").TextColor = button.TextColor_Selected;
                            }
                        }

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

                        if (button.Name == "Alert")
                        {
                            if (Handler.AlertType == "Combat")
                            {
                                GetLabel("Combat_Attacker").TextColor = button.TextColor;
                                GetLabel("Combat_VS").TextColor = button.TextColor;
                                GetLabel("Combat_Defender").TextColor = button.TextColor;
                            }
                        }
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
                        Picture select = GetPicture("Select");

                        if (InputManager.MouseWithin(squad.Region.ToRectangle))
                        {
                            hovered_squad = true;

                            Map map = null;

                            if (world.Maps.Any())
                            {
                                map = world.Maps[0];
                            }
                            
                            GameUtil.Examine(this, squad.Name);

                            if (Handler.Selected_Token == -1)
                            {
                                highlight.Region = squad.Region;
                                highlight.Visible = true;

                                if (squad.Type == "Ally")
                                {
                                    highlight.DrawColor = new Color(0, 0, 255, 255);
                                }
                                else if (squad.Type == "Enemy")
                                {
                                    highlight.DrawColor = new Color(255, 0, 0, 255);
                                }
                            }
                            else if (Handler.Selected_Token == squad.ID)
                            {
                                select.Region = squad.Region;
                                select.Visible = true;
                                select.DrawColor = new Color(0, 255, 255, 255);
                            }
                            else if (squad.Type == "Enemy")
                            {
                                select.Region = squad.Region;
                                select.Visible = true;
                                select.DrawColor = new Color(255, 0, 0, 255);
                            }

                            if (InputManager.Mouse_LB_Pressed)
                            {
                                hovered_squad = false;

                                if (Handler.Selected_Token == squad.ID)
                                {
                                    WorldUtil.DeselectToken(map, this);
                                }
                                else if (squad.Type == "Ally" &&
                                         Handler.Selected_Token == -1)
                                {
                                    AssetManager.PlaySound_Random("Click");
                                    Handler.Selected_Token = squad.ID;
                                }
                                else if (squad.Type == "Enemy")
                                {
                                    Squad ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Selected_Token);
                                    if (ally_squad != null)
                                    {
                                        Character leader = ally_squad.GetLeader();
                                        leader.Target_ID = squad.ID;
                                    }
                                }

                                InputManager.Mouse.Flush();

                                break;
                            }
                            else if (Handler.Selected_Token == -1)
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
                    select.DrawColor = new Color(0, 255, 0, 255);

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

            Handler.MoveGridDelay = 0;

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

            Handler.LocalMap = true;

            GetButton("Worldmap").Visible = true;
            GetButton("PlayPause").Enabled = true;
            GetButton("Speed").Enabled = true;

            Army allies = CharacterManager.GetArmy("Ally");
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

            Handler.MoveGridDelay = 0;

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
                GameUtil.Toggle_Pause();
            }
            else if (button.Name == "Speed")
            {
                SpeedToggle();
            }
            else if (button.Name == "Worldmap")
            {
                BackToWorldmap();
            }
            else if (button.Name == "Alert")
            {
                if (Handler.AlertType == "Combat")
                {
                    button.Visible = false;
                    GetLabel("Combat_Attacker").Visible = false;
                    GetLabel("Combat_VS").Visible = false;
                    GetLabel("Combat_Defender").Visible = false;

                    Active = false;
                    Visible = false;

                    SoundManager.StopMusic();
                    SoundManager.NeedMusic = true;

                    Scene combat = SceneManager.GetScene("Combat");
                    combat.Load();

                    SceneManager.ChangeScene(combat);

                    Handler.Combat = true;
                    Handler.AlertType = "";
                    SoundManager.AmbientPaused = false;
                }
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

            Handler.LocalMap = false;

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

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Main",
                hover_text = "System",
                texture = AssetManager.Textures["Button_Menu"],
                texture_highlight = AssetManager.Textures["Button_Menu_Hover"],
                texture_disabled = AssetManager.Textures["Button_Menu_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Army",
                hover_text = "Army",
                texture = AssetManager.Textures["Button_Army"],
                texture_highlight = AssetManager.Textures["Button_Army_Hover"],
                texture_disabled = AssetManager.Textures["Button_Army_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Inventory",
                hover_text = "Inventory",
                texture = AssetManager.Textures["Button_Inventory"],
                texture_highlight = AssetManager.Textures["Button_Inventory_Hover"],
                texture_disabled = AssetManager.Textures["Button_Inventory_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Worldmap",
                hover_text = "Return to Worldmap",
                texture = AssetManager.Textures["Button_Exit"],
                texture_highlight = AssetManager.Textures["Button_Exit_Hover"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "PlayPause",
                hover_text = "Pause",
                texture = AssetManager.Textures["Button_Pause"],
                texture_highlight = AssetManager.Textures["Button_Pause_Hover"],
                texture_disabled = AssetManager.Textures["Button_Pause_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Speed",
                hover_text = "x1",
                texture = AssetManager.Textures["Button_Speed1"],
                texture_highlight = AssetManager.Textures["Button_Speed1_Hover"],
                texture_disabled = AssetManager.Textures["Button_Speed1_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = false,
                visible = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Date", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Date").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Time", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Time").Opacity = 0.8f;

            AddButton(new ButtonOptions
            {
                id = Handler.GetID(),
                name = "Alert",
                texture = AssetManager.Textures["TextFrame"],
                texture_highlight = AssetManager.Textures["TextFrame"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                draw_color_selected = Color.Red,
                text_color = Color.Red,
                text_selected_color = Color.White,
                enabled = true
            });

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat_Attacker", "", Color.Red, new Region(0, 0, 0, 0), false);
            GetLabel("Combat_Attacker").Alignment_Horizontal = Alignment.Center;
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat_VS", "VS", Color.Red, new Region(0, 0, 0, 0), false);
            GetLabel("Combat_VS").Alignment_Horizontal = Alignment.Center;
            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Combat_Defender", "", Color.Red, new Region(0, 0, 0, 0), false);
            GetLabel("Combat_Defender").Alignment_Horizontal = Alignment.Center;

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

            GetButton("Alert").Region = new Region((Main.Game.ScreenWidth / 2) - (width * 4), Main.Game.ScreenHeight - (height * 5), width * 8, height * 3);

            GetButton("Main").Region = new Region(0, 0, width, height);
            GetButton("Army").Region = new Region(width, 0, width, height);
            GetButton("Inventory").Region = new Region(width * 2, 0, width, height);
            GetButton("Worldmap").Region = new Region(width * 3, 0, width, height);

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
