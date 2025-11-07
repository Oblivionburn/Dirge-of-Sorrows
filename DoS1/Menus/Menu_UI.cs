using System.Linq;
using System.Collections.Generic;

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

using DoS1.Util;

namespace DoS1.Menus
{
    public class Menu_UI : Menu
    {
        #region Variables

        private bool SelectingMultiple = false;
        private bool SelectingTown = false;

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
                if (!Handler.LocalPause)
                {
                    UpdateTime();
                    UpdateGold();
                    UpdateAlerts();
                }

                if (Handler.StoryStep == -1)
                {
                    GameUtil.Alert_Tutorial();
                }
                else if (Handler.StoryStep == 0)
                {
                    GameUtil.Alert_Story();
                }

                Scene scene = WorldUtil.GetScene();

                if (Handler.StoryStep <= 0 ||
                    Handler.StoryStep == 5 ||
                    Handler.StoryStep == 9 ||
                    Handler.StoryStep == 10 ||
                    Handler.StoryStep == 14 ||
                    Handler.StoryStep == 20 ||
                    Handler.StoryStep == 21 ||
                    Handler.StoryStep == 28 ||
                    Handler.StoryStep > 48)
                {
                    if (Handler.AlertType == "Story" ||
                        Handler.AlertType == "Generic" ||
                        string.IsNullOrEmpty(Handler.AlertType))
                    {
                        UpdateControls(scene.World);
                    }
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
            if (!Handler.MovingGrid &&
                !SelectingMultiple &&
                !SelectingTown)
            {
                bool hovering_button = HoveringButton();

                Handler.Hovering_Squad = null;
                bool hovering_location = false;
                bool hovering_selection = false;

                if (!hovering_button)
                {
                    Map map = WorldUtil.GetMap(world);

                    Layer pathing = null;
                    if (map != null)
                    {
                        pathing = map.GetLayer("Pathing");
                    }

                    Handler.Hovering_Squad = HoveringSquad(world);
                    if (Handler.Hovering_Squad == null)
                    {
                        if (Handler.Selected_Token == -1 &&
                            pathing != null)
                        {
                            pathing.Visible = false;
                        }

                        hovering_location = HoveringLocation(map);
                    }
                    else if (Handler.Hovering_Squad.Type != "Enemy" &&
                             (Handler.Selected_Token != -1 &&
                             Handler.Hovering_Squad.ID != Handler.Selected_Token))
                    {
                        hovering_location = HoveringLocation(map);
                    }

                    if (Handler.Selected_Token != -1)
                    {
                        hovering_selection = HoveringSelection(world, map);
                        if (hovering_selection)
                        {
                            GetPicture("Select").Visible = true;
                        }
                    }

                    if (!hovering_selection &&
                        !hovering_location &&
                        Handler.Hovering_Squad == null)
                    {
                        GetPicture("Select").Visible = false;
                    }
                }

                if (!hovering_button &&
                    !hovering_location &&
                    Handler.Hovering_Squad == null)
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

                if (SelectingMultiple)
                {
                    bool hovering_button = HoveringButton();

                    if (!hovering_button &&
                        InputManager.Mouse_LB_Pressed)
                    {
                        SelectingMultiple = false;
                        ClearButtons_Squad();
                    }
                }
                else if (SelectingTown)
                {
                    bool hovering_button = HoveringButton();

                    if (!hovering_button &&
                        InputManager.Mouse_LB_Pressed)
                    {
                        SelectingTown = false;
                        ClearButtons_Squad();
                        GameUtil.Toggle_Pause(false);
                    }
                }

                if (InputManager.Mouse_LB_Pressed)
                {
                    Handler.MoveGridDelay = 0;
                    Handler.MovingGrid = false;
                }
            }

            if (!Handler.TokenMenu)
            {
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

                if (InputManager.KeyPressed("Space"))
                {
                    GameUtil.Toggle_Pause(true);
                }
                else if (InputManager.KeyPressed("Esc"))
                {
                    TimeManager.Paused = true;
                    Active = false;
                    Visible = false;
                    MenuManager.ChangeMenu("Main");
                }
                else if (InputManager.KeyPressed("Debug"))
                {
                    if (!Main.Game.Debugging)
                    {
                        Main.Game.Debugging = true;
                        Handler.Gold = 10000;
                        GetLabel("Debug").Visible = true;
                    }
                    else
                    {
                        Main.Game.Debugging = false;
                        GetLabel("Debug").Visible = false;
                    }
                }
            }
        }

        private bool HoveringButton()
        {
            bool found = false;

            if (Handler.StoryStep >= 20 &&
                Handler.Selected_Token == -1)
            {
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
                                GetLabel("Examine").TextColor = Color.White;
                                GameUtil.Examine(this, button.HoverText);
                            }

                            button.Selected = true;

                            if (InputManager.Mouse_LB_Pressed)
                            {
                                found = false;

                                CheckClick(button);

                                button.Selected = false;

                                break;
                            }
                        }
                        else if (InputManager.Mouse.Moved)
                        {
                            button.Selected = false;
                        }
                    }
                }
            }

            return found;
        }

        private Squad HoveringSquad(World world)
        {
            Squad hovered_squad = null;

            if (Handler.LocalMap)
            {
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
                                hovered_squad = squad;

                                Label examine = GetLabel("Examine");
                                if (squad.Type == "Enemy")
                                {
                                    examine.TextColor = Color.Red;
                                }
                                else
                                {
                                    examine.TextColor = Color.Blue;
                                }

                                Map map = WorldUtil.GetMap(world);

                                if (Handler.Selected_Token == -1)
                                {
                                    highlight.Region = squad.Region;
                                    highlight.Visible = true;

                                    if (squad.Type == "Ally")
                                    {
                                        highlight.DrawColor = Color.Blue;
                                        highlight.Texture = AssetManager.Textures["Highlight_Circle"];
                                    }
                                    else if (squad.Type == "Enemy")
                                    {
                                        highlight.DrawColor = Color.Red;
                                        highlight.Texture = AssetManager.Textures["Highlight_Circle"];
                                    }

                                    GameUtil.Examine(this, squad.Name);
                                }
                                else if (Handler.Selected_Token == squad.ID)
                                {
                                    select.Texture = AssetManager.Textures["Highlight_Circle"];
                                    select.Region = squad.Region;
                                    select.Visible = true;
                                    select.DrawColor = new Color(0, 255, 255, 255);

                                    map.GetLayer("Pathing").Visible = false;

                                    GameUtil.Examine(this, squad.Name);
                                }
                                else if (squad.Type == "Enemy")
                                {
                                    select.Texture = AssetManager.Textures["Highlight_Circle"];
                                    select.Region = squad.Region;
                                    select.Visible = true;
                                    select.DrawColor = Color.Red;

                                    GameUtil.Examine(this, squad.Name);
                                }

                                if (InputManager.Mouse_LB_Pressed)
                                {
                                    hovered_squad = null;

                                    if (Handler.StoryStep != 5)
                                    {
                                        if (Handler.Selected_Token == squad.ID)
                                        {
                                            DeselectToken(map);
                                        }
                                        else if (squad.Type == "Ally" &&
                                                 Handler.Selected_Token == -1 &&
                                                 Handler.StoryStep != 5 &&
                                                 Handler.StoryStep != 14 &&
                                                 Handler.StoryStep != 28)
                                        {
                                            SelectToken(army, squad);
                                        }
                                        else if (squad.Type == "Enemy" &&
                                                 Handler.Selected_Token != -1)
                                        {
                                            SelectToken_Enemy(squad);
                                        }
                                    }

                                    break;
                                }
                                else if (InputManager.Mouse_RB_Pressed)
                                {
                                    if (Handler.Selected_Token != -1)
                                    {
                                        DeselectToken(map);
                                    }
                                    else
                                    {
                                        Handler.Selected_Squad = squad.ID;

                                        Handler.ViewOnly_Squad = true;
                                        Handler.ViewOnly_Character = true;
                                        Handler.ViewOnly_Item = true;

                                        DeselectToken(map);
                                        highlight.Visible = false;
                                        GetLabel("Examine").Visible = false;

                                        Layer locations = map.GetLayer("Locations");
                                        Tile location_tile = locations.GetTile(new Vector3(squad.Location.X, squad.Location.Y, 0));
                                        if (location_tile != null &&
                                            squad.Type == "Ally")
                                        {
                                            if (location_tile.Type.Contains("Market") ||
                                                location_tile.Type.Contains("Academy") ||
                                                location_tile.Type.Contains("Ally"))
                                            {
                                                Handler.ViewOnly_Squad = false;
                                                Handler.ViewOnly_Character = false;
                                                Handler.ViewOnly_Item = false;

                                                if (Handler.StoryStep == 5 ||
                                                    Handler.StoryStep == 14 ||
                                                    Handler.StoryStep == 28)
                                                {
                                                    MenuManager.GetMenu("Alerts").Visible = false;
                                                    Handler.StoryStep++;
                                                }
                                            }
                                        }

                                        if (!Handler.ViewOnly_Squad &&
                                            (Handler.StoryStep == 5 ||
                                             Handler.StoryStep == 14 ||
                                             Handler.StoryStep == 28))
                                        {
                                            InputManager.Mouse.Flush();
                                            MenuManager.ChangeMenu("Squad");
                                        }
                                        else if (Handler.StoryStep != 5 &&
                                                 Handler.StoryStep != 14 &&
                                                 Handler.StoryStep != 28)
                                        {
                                            InputManager.Mouse.Flush();
                                            MenuManager.ChangeMenu("Squad");
                                        }
                                    }
                                }
                                else if (Handler.Selected_Token == -1)
                                {
                                    WorldUtil.DisplayPath_Squad(this, map, squad);
                                }
                            }
                            else if (Handler.Selected_Token == squad.ID)
                            {
                                highlight.DrawColor = Color.Blue;
                            }
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

                        GetLabel("Examine").TextColor = Color.White;
                        GameUtil.Examine(this, location.Name);

                        if (Handler.Selected_Token == -1)
                        {
                            Picture highlight = GetPicture("Highlight");
                            highlight.Texture = AssetManager.Textures["Grid_Hover"];
                            highlight.Region = location.Region;
                            highlight.Visible = true;
                            highlight.DrawColor = new Color(255, 255, 255, 255);
                        }
                        else
                        {
                            Picture select = GetPicture("Select");
                            select.Texture = AssetManager.Textures["Grid_Hover"];
                            select.Region = location.Region;
                            select.Visible = true;
                            select.DrawColor = new Color(0, 255, 0, 255);
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

                    if (Handler.Hovering_Squad == null ||
                        Handler.Hovering_Squad.Type != "Enemy")
                    {
                        Picture select = GetPicture("Select");
                        select.Texture = AssetManager.Textures["Grid_Hover"];
                        select.Region = tile.Region;
                        select.DrawColor = new Color(0, 255, 0, 255);
                    }

                    if (InputManager.Mouse_LB_Pressed)
                    {
                        found = false;
                        CheckClick_Selection(world, tile);
                        break;
                    }
                    else if (InputManager.Mouse_RB_Pressed)
                    {
                        DeselectToken(map);
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
            Handler.Level = location_num;

            Army enemies = CharacterManager.GetArmy("Enemy");

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
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Snow };
                }
                else if (!ground_tile.Type.Contains("Desert"))
                {
                    TimeManager.WeatherOptions = new WeatherType[] { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm };
                }

                Handler.LocalMap = true;

                GetButton("PlayPause").Enabled = true;
                GetButton("Speed").Enabled = true;

                //Set "Return to Worldmap" button
                Button worldMap = GetButton("Worldmap");
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

                //Set starting squad at ally base
                Army allies = CharacterManager.GetArmy("Ally");
                Squad ally_squad = allies.Squads[0];
                WorldUtil.AllyToken_Start(ally_squad, localmap);

                //Set enemies at enemy base
                foreach (Squad enemy_squad in enemies.Squads)
                {
                    WorldUtil.EnemyToken_Start(enemy_squad, localmap);
                }

                if (Handler.StoryStep == 0)
                {
                    MenuManager.GetMenu("Alerts").Visible = false;
                    Handler.StoryStep++;
                }

                //Switch to local map
                SceneManager.ChangeScene("Localmap");
                WorldUtil.Resize_OnStart(localmap);

                SoundManager.StopMusic();
                SoundManager.NeedMusic = true;
            }
        }

        private void CheckClick_Selection(World world, Tile tile)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.MoveGridDelay = 0;

            Map map = world.Maps[Handler.Level];

            bool okay = false;
            
            if (Handler.StoryStep == 9 ||
                Handler.StoryStep == 10)
            {
                Tile market = WorldUtil.GetMarket();

                Layer locations = map.GetLayer("Locations");
                Tile location = locations.GetTile(new Vector3(tile.Location.X, tile.Location.Y, 0));
                if (location != null)
                {
                    if (market.ID == location.ID)
                    {
                        if (Handler.StoryStep == 9)
                        {
                            MenuManager.GetMenu("Alerts").Visible = false;
                            Handler.StoryStep++;
                        }

                        okay = true;
                    }
                }
                else
                {
                    okay = true;
                }
            }
            else if (Handler.StoryStep == 20 ||
                     Handler.StoryStep == 21)
            {
                Tile ally_base = WorldUtil.Get_Base(map, "Ally");

                Layer locations = map.GetLayer("Locations");
                Tile location = locations.GetTile(new Vector3(tile.Location.X, tile.Location.Y, 0));
                if (location != null)
                {
                    if (ally_base.ID == location.ID)
                    {
                        if (Handler.StoryStep == 20)
                        {
                            Handler.StoryStep++;
                        }

                        okay = true;
                    }
                }
                else
                {
                    okay = true;
                }
            }
            else if (Handler.StoryStep > 48)
            {
                okay = true;
            }

            if (okay)
            {
                ArmyUtil.SetPath(this, map, tile);

                InputManager.Mouse.Flush();
                GameUtil.Toggle_Pause(false);
            }
        }

        private void SelectToken(Army army, Squad selected_squad)
        {
            int height = (Main.Game.MenuSize.Y / 4) * 3;

            List<Squad> potential_squads = new List<Squad>
            {
                selected_squad
            };

            foreach (Squad squad in army.Squads)
            {
                if (squad.Location.X == selected_squad.Location.X &&
                    squad.Location.Y == selected_squad.Location.Y &&
                    squad.ID != selected_squad.ID)
                {
                    potential_squads.Add(squad);
                }
            }

            if (potential_squads.Count > 1)
            {
                SelectingMultiple = true;

                for (int i = 0; i < potential_squads.Count; i++)
                {
                    Squad squad = potential_squads[i];

                    AddButton(new ButtonOptions
                    {
                        id = Handler.GetID(),
                        font = AssetManager.Fonts["ControlFont"],
                        name = "Squad_" + squad.ID + "_Multiple",
                        text = squad.Name,
                        texture = AssetManager.Textures["ButtonFrame"],
                        texture_highlight = AssetManager.Textures["ButtonFrame"],
                        region = new Region(selected_squad.Region.X + selected_squad.Region.Width, selected_squad.Region.Y + (height * i), Main.Game.MenuSize.X * 3, height),
                        draw_color = Color.White * 0.9f,
                        draw_color_selected = Color.White,
                        text_color = Color.Black,
                        text_selected_color = Color.White,
                        enabled = true,
                        visible = true
                    });

                    Handler.TokenMenu = true;
                }

                GameUtil.Toggle_Pause(false);
            }
            else
            {
                Tile location = WorldUtil.GetLocation(selected_squad);
                if (location != null)
                {
                    SelectingTown = true;

                    if (location.Type == "Base_Ally")
                    {
                        Squad hero_squad = ArmyUtil.Get_Squad(Handler.GetHero());
                        if (selected_squad.ID != hero_squad.ID)
                        {
                            AddButton(new ButtonOptions
                            {
                                id = Handler.GetID(),
                                font = AssetManager.Fonts["ControlFont"],
                                name = "Squad_" + selected_squad.ID + "_Base",
                                text = "Retreat",
                                texture = AssetManager.Textures["ButtonFrame"],
                                texture_highlight = AssetManager.Textures["ButtonFrame"],
                                region = new Region(selected_squad.Region.X + selected_squad.Region.Width, selected_squad.Region.Y, Main.Game.MenuSize.X * 3, height),
                                draw_color = Color.White * 0.9f,
                                draw_color_selected = Color.White,
                                text_color = Color.Black,
                                text_selected_color = Color.White,
                                enabled = true,
                                visible = true
                            });

                            AddButton(new ButtonOptions
                            {
                                id = Handler.GetID(),
                                font = AssetManager.Fonts["ControlFont"],
                                name = "Squad_" + selected_squad.ID + "_Move",
                                text = "Move",
                                texture = AssetManager.Textures["ButtonFrame"],
                                texture_highlight = AssetManager.Textures["ButtonFrame"],
                                region = new Region(selected_squad.Region.X + selected_squad.Region.Width, selected_squad.Region.Y + height, Main.Game.MenuSize.X * 3, height),
                                draw_color = Color.White * 0.9f,
                                draw_color_selected = Color.White,
                                text_color = Color.Black,
                                text_selected_color = Color.White,
                                enabled = true,
                                visible = true
                            });

                            Handler.TokenMenu = true;
                        }
                        else
                        {
                            Handler.Selected_Token = selected_squad.ID;
                            SelectingTown = false;
                        }
                    }
                    else if (location.Type.Contains("Market") ||
                             location.Type.Contains("Academy"))
                    {
                        AddButton(new ButtonOptions
                        {
                            id = Handler.GetID(),
                            font = AssetManager.Fonts["ControlFont"],
                            name = "Squad_" + selected_squad.ID + "_Town",
                            text = "Enter Town",
                            texture = AssetManager.Textures["ButtonFrame"],
                            texture_highlight = AssetManager.Textures["ButtonFrame"],
                            region = new Region(selected_squad.Region.X + selected_squad.Region.Width, selected_squad.Region.Y, Main.Game.MenuSize.X * 3, height),
                            draw_color = Color.White * 0.9f,
                            draw_color_selected = Color.White,
                            text_color = Color.Black,
                            text_selected_color = Color.White,
                            enabled = true,
                            visible = true
                        });

                        AddButton(new ButtonOptions
                        {
                            id = Handler.GetID(),
                            font = AssetManager.Fonts["ControlFont"],
                            name = "Squad_" + selected_squad.ID + "_Move",
                            text = "Move",
                            texture = AssetManager.Textures["ButtonFrame"],
                            texture_highlight = AssetManager.Textures["ButtonFrame"],
                            region = new Region(selected_squad.Region.X + selected_squad.Region.Width, selected_squad.Region.Y + height, Main.Game.MenuSize.X * 3, height),
                            draw_color = Color.White * 0.9f,
                            draw_color_selected = Color.White,
                            text_color = Color.Black,
                            text_selected_color = Color.White,
                            enabled = true,
                            visible = true
                        });

                        Handler.TokenMenu = true;
                    }
                    else
                    {
                        Handler.Selected_Token = selected_squad.ID;
                        SelectingTown = false;
                    }
                }
                else
                {
                    Handler.Selected_Token = selected_squad.ID;
                }

                GameUtil.Toggle_Pause(false);
            }

            InputManager.Mouse.Flush();
        }

        private void SelectToken_FromMultiple(long id)
        {
            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(id);
            if (squad != null)
            {
                Handler.Selected_Token = squad.ID;
            }

            ClearButtons_Squad();
        }

        private void DeselectToken(Map map)
        {
            AssetManager.PlaySound_Random("Click");

            Handler.Selected_Token = -1;

            GetPicture("Select").Visible = false;
            GetPicture("Highlight").Visible = true;
            GetPicture("Highlight").DrawColor = new Color(255, 255, 255, 255);

            if (map != null)
            {
                Layer pathing = map.GetLayer("Pathing");
                pathing.Visible = false;
            }

            GameUtil.Toggle_Pause(false);
            InputManager.Mouse.Flush();
        }

        private void SelectToken_Enemy(Squad enemy_squad)
        {
            if (Handler.StoryStep != 9 &&
                Handler.StoryStep != 10 &&
                Handler.StoryStep != 20 &&
                Handler.StoryStep != 21)
            {
                Squad ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Selected_Token);
                if (ally_squad != null)
                {
                    Character leader = ally_squad.GetLeader();
                    leader.Target_ID = enemy_squad.ID;
                }
            }
        }

        private void UpdateAlerts()
        {
            Label alert = GetLabel("Alert");
            if (alert.Visible)
            {
                alert.Value--;
                alert.Opacity -= 0.01f;
                if (alert.Value <= 0)
                {
                    alert.Visible = false;
                }
            }
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);

            if (button.Name == "PlayPause")
            {
                GameUtil.Toggle_Pause(true);
            }
            else if (button.Name == "Speed")
            {
                SpeedToggle();
            }
            else if (button.Name == "Worldmap")
            {
                if (Handler.Selected_Token != -1)
                {
                    GetPicture("Select").Visible = false;
                    GetPicture("Highlight").Visible = false;

                    Scene scene = WorldUtil.GetScene();
                    DeselectToken(WorldUtil.GetMap(scene.World));
                }

                GameUtil.ReturnToWorldmap();
            }
            else if (button.Name.Contains("Squad"))
            {
                string[] parts = button.Name.Split('_');
                long id = long.Parse(parts[1]);

                if (button.Name.Contains("Multiple"))
                {
                    SelectToken_FromMultiple(id);
                }
                else if (button.Name.Contains("Town"))
                {
                    SelectTown(id);
                }
                else if (button.Name.Contains("Base"))
                {
                    SelectBase(id);
                }
                else if (button.Name.Contains("Move"))
                {
                    SelectMove(id);
                }
            }
            else
            {
                TimeManager.Paused = true;
                SoundManager.AmbientPaused = true;

                Active = false;
                Visible = false;

                MenuManager.ChangeMenu(button.Name);

                InputManager.Mouse.Flush();
                InputManager.Keyboard.Flush();
            }
        }

        private void SelectTown(long id)
        {
            InputManager.Mouse.Flush();
            ClearButtons_Squad();

            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(id);
            if (squad != null)
            {
                SelectingTown = false;

                Tile location = WorldUtil.GetLocation(squad);
                if (location != null)
                {
                    if (location.Type.Contains("Market") ||
                        location.Type.Contains("Academy"))
                    {
                        WorldUtil.EnterTown(location.Type);
                    }
                    else
                    {
                        GameUtil.Toggle_Pause(false);
                    }
                }
            }
        }

        private void SelectBase(long id)
        {
            InputManager.Mouse.Flush();
            ClearButtons_Squad();

            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(id);
            if (squad != null)
            {
                SelectingTown = false;
                squad.Visible = false;
                squad.Active = false;
                GameUtil.Toggle_Pause(false);
            }
        }

        private void SelectMove(long id)
        {
            InputManager.Mouse.Flush();
            ClearButtons_Squad();

            Army army = CharacterManager.GetArmy("Ally");
            Squad squad = army.GetSquad(id);
            if (squad != null)
            {
                Picture highlight = GetPicture("Highlight");
                highlight.Region = squad.Region;
                highlight.Visible = true;
                highlight.DrawColor = new Color(0, 0, 255, 255);
                highlight.Texture = AssetManager.Textures["Highlight_Circle"];

                Handler.Selected_Token = squad.ID;
                SelectingTown = false;
            }
        }

        private void ClearButtons_Squad()
        {
            for (int i = 0; i < Buttons.Count; i++)
            {
                Button button = Buttons[i];
                if (button.Name.Contains("Squad"))
                {
                    Buttons.Remove(button);
                    i--;
                }
            }

            Handler.TokenMenu = false;
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

        private void UpdateGold()
        {
            Label gold = GetLabel("Gold");
            gold.Text = "Gold: " + Handler.Gold;
        }

        private void SpeedToggle()
        {
            Main.TimeSpeed += 2;
            if (Main.TimeSpeed > 10)
            {
                Main.TimeSpeed = 4;
            }

            Button button = GetButton("Speed");
            switch (Main.TimeSpeed)
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

            SaveUtil.ExportINI();
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
                name = "Help",
                hover_text = "Help",
                texture = AssetManager.Textures["Button_Help"],
                texture_highlight = AssetManager.Textures["Button_Help_Hover"],
                texture_disabled = AssetManager.Textures["Button_Help_Disabled"],
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
                texture_disabled = AssetManager.Textures["Button_Exit_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = false
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
                hover_text = "Time x1",
                texture = AssetManager.Textures["Button_Speed1"],
                texture_highlight = AssetManager.Textures["Button_Speed1_Hover"],
                texture_disabled = AssetManager.Textures["Button_Speed1_Disabled"],
                region = new Region(0, 0, 0, 0),
                draw_color = Color.White * 0.8f,
                enabled = true,
                visible = true
            });

            Button speed_button = GetButton("Speed");
            switch (Main.TimeSpeed)
            {
                case 4:
                    speed_button.HoverText = "Time x1";
                    speed_button.Texture = AssetManager.Textures["Button_Speed1"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed1_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed1_Disabled"];
                    break;

                case 6:
                    speed_button.HoverText = "Time x2";
                    speed_button.Texture = AssetManager.Textures["Button_Speed2"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed2_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed2_Disabled"];
                    break;

                case 8:
                    speed_button.HoverText = "Time x3";
                    speed_button.Texture = AssetManager.Textures["Button_Speed3"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed3_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed3_Disabled"];
                    break;

                case 10:
                    speed_button.HoverText = "Time x4";
                    speed_button.Texture = AssetManager.Textures["Button_Speed4"];
                    speed_button.Texture_Highlight = AssetManager.Textures["Button_Speed4_Hover"];
                    speed_button.Texture_Disabled = AssetManager.Textures["Button_Speed4_Disabled"];
                    break;
            }

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Debug", "Debugging", Color.White, new Region(0, 0, 0, 0), false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Date", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Date").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Time", "", Color.White, AssetManager.Textures["Frame_Small"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Time").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Gold", "Gold: " + Handler.Gold, Color.Gold, AssetManager.Textures["Frame_Wide"],
                new Region(0, 0, 0, 0), true);
            GetLabel("Gold").Opacity = 0.8f;

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Alert", "", Color.White, new Region(0, 0, 0, 0), false);

            AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            AddPicture(Handler.GetID(), "Select", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), new Color(0, 255, 0, 255), false);

            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            int width = Main.Game.MenuSize.X;
            int height = Main.Game.MenuSize.X;

            GetButton("PlayPause").Region = new Region(Main.Game.ScreenWidth - (width * 4), 0, width, height);
            GetButton("Speed").Region = new Region(Main.Game.ScreenWidth - (width * 3), 0, width, height);

            GetLabel("Date").Region = new Region(Main.Game.ScreenWidth - (width * 2), 0, width * 2, height / 2);
            GetLabel("Time").Region = new Region(Main.Game.ScreenWidth - (width * 2), height / 2, width * 2, height / 2);
            GetLabel("Debug").Region = new Region(Main.Game.ScreenWidth - (width * 2), height, width * 2, height / 2);
            GetLabel("Gold").Region = new Region((Main.Game.Resolution.X / 2) - (width * 5), 0, width * 10, height);
            GetLabel("Alert").Region = new Region((Main.Game.Resolution.X / 2) - (width * 5), height, width * 10, height);

            GetButton("Main").Region = new Region(0, 0, width, height);
            GetButton("Help").Region = new Region(0, height, width, height);
            GetButton("Army").Region = new Region(width, 0, width, height);
            GetButton("Inventory").Region = new Region(width * 2, 0, width, height);
            GetButton("Worldmap").Region = new Region(width * 3, 0, width, height);

            GetLabel("Examine").Region = new Region(0, 0, 0, 0);
        }

        #endregion
    }
}
