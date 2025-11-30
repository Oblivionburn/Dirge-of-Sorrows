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
using System.Collections.Generic;
using System.Linq;

namespace DoS1.Scenes
{
    public class Localmap : Scene
    {
        #region Variables

        private bool SelectingMultiple = false;
        private bool SelectingTown = false;

        #endregion

        #region Constructors

        public Localmap(ContentManager content)
        {
            ID = Handler.GetID();
            Name = "Localmap";
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

                    if (World.Maps.Any())
                    {
                        Map map = World.Maps[Handler.Level];

                        if (map.Type.Contains("Snow") ||
                            map.Type.Contains("Ice"))
                        {
                            AssetManager.PlayMusic_Random("Snowy", false);
                        }
                        else if (map.Type.Contains("Desert"))
                        {
                            AssetManager.PlayMusic_Random("Desert", false);
                        }
                        else
                        {
                            AssetManager.PlayMusic_Random("Plains", false);
                        }
                    }
                }

                if (Handler.StoryStep <= 5 ||
                    (Handler.StoryStep >= 7 && Handler.StoryStep <= 9) ||
                    Handler.StoryStep == 14 ||
                    Handler.StoryStep == 19 ||
                    (Handler.StoryStep >= 22 && Handler.StoryStep <= 28) ||
                    Handler.StoryStep == 36 ||
                    Handler.StoryStep == 37 ||
                    (Handler.StoryStep >= 45 && Handler.StoryStep <= 48) ||
                    Handler.StoryStep == 55)
                {
                    StoryUtil.Alert_Story(Menu);
                }

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
                        if (Handler.MovingGrid)
                        {
                            WorldUtil.MoveGrid(World);

                            if (InputManager.Mouse_LB_Pressed)
                            {
                                Handler.MoveGridDelay = 0;
                                Handler.MovingGrid = false;
                            }
                        }
                        else
                        {
                            UpdateControls(World);
                        }
                    }
                }

                base.Update(gameRef, content);
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution, Color color)
        {
            if (Visible)
            {
                for (int i = 0; i < World.Maps.Count; i++)
                {
                    Map map = World.Maps[i];
                    if (map.Visible)
                    {
                        foreach (Layer layer in map.Layers)
                        {
                            if (layer.Name != "Pathing")
                            {
                                layer.Draw(spriteBatch, resolution, color);
                            }
                        }
                    }
                }
            }
        }

        public override void DrawWorld(SpriteBatch spriteBatch, Point resolution)
        {
            if (Visible)
            {
                foreach (Map map in World.Maps)
                {
                    if (map.Visible)
                    {
                        foreach (Layer layer in map.Layers)
                        {
                            if (layer.Name == "Pathing")
                            {
                                layer.Draw(spriteBatch, resolution, Color.White);
                                break;
                            }
                        }
                    }
                }

                foreach (Army army in CharacterManager.Armies)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        if (squad.Characters.Any() &&
                            squad.Visible)
                        {
                            squad.Draw(spriteBatch, resolution, Color.White);
                        }
                    }
                }
            }
        }

        public override void DrawMenu(SpriteBatch spriteBatch)
        {
            if (Visible)
            {
                foreach (Picture picture in Menu.Pictures)
                {
                    if (picture.Name != "Highlight" &&
                        picture.Name != "Select")
                    {
                        picture.Draw(spriteBatch);
                    }
                }

                foreach (Picture picture in Menu.Pictures)
                {
                    if (picture.Name == "Highlight")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Picture picture in Menu.Pictures)
                {
                    if (picture.Name == "Select")
                    {
                        picture.Draw(spriteBatch);
                        break;
                    }
                }

                foreach (Button button in Menu.Buttons)
                {
                    button.Draw(spriteBatch);
                }

                foreach (Label label in Menu.Labels)
                {
                    label.Draw(spriteBatch);
                }
            }
        }

        private void UpdateControls(World world)
        {
            if (!SelectingMultiple &&
                !SelectingTown)
            {
                bool hovering_button = HoveringButton();

                Handler.Hovering_Squad = null;
                bool hovering_location = false;
                bool hovering_selection = false;

                if (!hovering_button)
                {
                    Map map = WorldUtil.GetMap(world);
                    Layer pathing = map.GetLayer("Pathing");

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
                            Menu.GetPicture("Select").Visible = true;
                        }
                    }

                    if (!hovering_selection &&
                        !hovering_location &&
                        Handler.Hovering_Squad == null)
                    {
                        Menu.GetPicture("Select").Visible = false;
                    }
                }

                if (!hovering_button &&
                    !hovering_location &&
                    Handler.Hovering_Squad == null)
                {
                    Menu.GetLabel("Examine").Visible = false;

                    if (Handler.Selected_Token == -1)
                    {
                        Menu.GetPicture("Highlight").Visible = false;
                    }
                }
            }
            else
            {
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
            }

            if (!Handler.TokenMenu)
            {
                if (InputManager.Mouse_LB_Held &&
                    InputManager.Mouse.Moved)
                {
                    Handler.MoveGridDelay++;

                    if (Handler.MoveGridDelay >= 4)
                    {
                        Menu.GetLabel("Examine").Visible = false;
                        Handler.MovingGrid = true;
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
        }

        private bool HoveringButton()
        {
            if (Handler.StoryStep == 28)
            {
                return false;
            }

            bool found = false;

            foreach (Button button in Menu.Buttons)
            {
                if (button.Visible &&
                    button.Enabled)
                {
                    if (InputManager.MouseWithin(button.Region.ToRectangle))
                    {
                        found = true;

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
                            Picture highlight = Menu.GetPicture("Highlight");
                            
                            if (InputManager.MouseWithin(squad.Region.ToRectangle))
                            {
                                hovered_squad = squad;
                                ExamineSquad(squad);

                                Map map = WorldUtil.GetMap(world);

                                if (InputManager.Mouse_LB_Pressed)
                                {
                                    //hovered_squad = null;

                                    if (Handler.StoryStep != 5)
                                    {
                                        if (Handler.Selected_Token == squad.ID)
                                        {
                                            WorldUtil.DeselectToken();
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
                                        WorldUtil.DeselectToken();
                                    }
                                    else
                                    {
                                        Handler.Selected_Squad = squad.ID;

                                        Handler.ViewOnly_Squad = true;
                                        Handler.ViewOnly_Character = true;
                                        Handler.ViewOnly_Item = true;

                                        WorldUtil.DeselectToken();
                                        highlight.Visible = false;
                                        Menu.GetLabel("Examine").Visible = false;

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
                                            Active = false;
                                            MenuManager.ChangeMenu("Squad");
                                        }
                                        else if (Handler.StoryStep != 5 &&
                                                 Handler.StoryStep != 14 &&
                                                 Handler.StoryStep != 28)
                                        {
                                            InputManager.Mouse.Flush();
                                            Active = false;
                                            MenuManager.ChangeMenu("Squad");
                                        }
                                    }
                                }
                                else if (Handler.Selected_Token == -1)
                                {
                                    WorldUtil.DisplayPath_Squad(Menu, map, squad);
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

            Layer locations = map.GetLayer("Locations");
            for (int i = 0; i < locations.Tiles.Count; i++)
            {
                Tile location = locations.Tiles[i];

                if (location.Visible)
                {
                    if (InputManager.MouseWithin(location.Region.ToRectangle))
                    {
                        found = true;

                        Menu.GetLabel("Examine").TextColor = Color.White;
                        ExamineLocation(location);

                        if (Handler.Selected_Token == -1)
                        {
                            Picture highlight = Menu.GetPicture("Highlight");
                            highlight.Texture = AssetManager.Textures["Grid_Hover"];
                            highlight.Region = location.Region;
                            highlight.Visible = true;
                            highlight.DrawColor = new Color(255, 255, 255, 255);
                        }
                        else
                        {
                            Picture select = Menu.GetPicture("Select");
                            select.Texture = AssetManager.Textures["Grid_Hover"];
                            select.Region = location.Region;
                            select.Visible = true;
                            select.DrawColor = new Color(0, 255, 0, 255);
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
                        Picture select = Menu.GetPicture("Select");
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
                        WorldUtil.DeselectToken();
                    }
                    else
                    {
                        WorldUtil.DisplayPath_Temp(map, tile);
                    }
                }
            }

            return found;
        }

        private void CheckClick(Button button)
        {
            AssetManager.PlaySound_Random("Click");

            Menu.GetLabel("Examine").Visible = false;

            if (button.Name.Contains("Squad"))
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
                ArmyUtil.SetPath(Menu, map, tile);

                InputManager.Mouse.Flush();
                GameUtil.Toggle_Pause(false);
            }
        }

        private void SelectToken(Army army, Squad selected_squad)
        {
            Menu.GetLabel("Examine").Visible = false;

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

                    Menu.AddButton(new ButtonOptions
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

                if (Handler.Retreating)
                {
                    Handler.Retreating = false;
                }
                else
                {
                    GameUtil.Toggle_Pause(false);
                }
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
                            Menu.AddButton(new ButtonOptions
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

                            Menu.AddButton(new ButtonOptions
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
                        Menu.AddButton(new ButtonOptions
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

                        Menu.AddButton(new ButtonOptions
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

                if (Handler.Retreating)
                {
                    Handler.Retreating = false;
                }
                else
                {
                    GameUtil.Toggle_Pause(false);
                }
            }

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
                Picture highlight = Menu.GetPicture("Highlight");
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
            for (int i = 0; i < Menu.Buttons.Count; i++)
            {
                Button button = Menu.Buttons[i];
                if (button.Name.Contains("Squad"))
                {
                    Menu.Buttons.Remove(button);
                    i--;
                }
            }

            Handler.TokenMenu = false;
        }

        private void ExamineLocation(Tile location)
        {
            Label examine = Menu.GetLabel("Examine");
            examine.Text = location.Name;

            int width = Main.Game.MenuSize.X * 4;
            int height = Main.Game.MenuSize.X;

            if (location.Type.Contains("Market"))
            {
                examine.Text += "\n(Market)";
                height += Main.Game.MenuSize.X / 2;
            }
            else if (location.Type.Contains("Academy"))
            {
                examine.Text += "\n(Academy)";
                height += Main.Game.MenuSize.X / 2;
            }
            else if (location.Type.Contains("Base"))
            {
                examine.Text += "\n(Base)";
                height += Main.Game.MenuSize.X / 2;
            }

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

        private void ExamineSquad(Squad squad)
        {
            Picture highlight = Menu.GetPicture("Highlight");
            Picture select = Menu.GetPicture("Select");

            Label examine = Menu.GetLabel("Examine");
            examine.Text = squad.Name;

            if (squad.Type == "Enemy")
            {
                examine.TextColor = Color.Red;
            }
            else
            {
                examine.TextColor = Color.Blue;
            }

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

                GameUtil.Examine(Menu, squad.Name);
            }
            else if (Handler.Selected_Token == squad.ID)
            {
                select.Texture = AssetManager.Textures["Highlight_Circle"];
                select.Region = squad.Region;
                select.Visible = true;
                select.DrawColor = new Color(0, 255, 255, 255);

                Map map = WorldUtil.GetMap(World);
                map.GetLayer("Pathing").Visible = false;

                GameUtil.Examine(Menu, squad.Name);
            }
            else if (squad.Type == "Enemy")
            {
                select.Texture = AssetManager.Textures["Highlight_Circle"];
                select.Region = squad.Region;
                select.Visible = true;
                select.DrawColor = Color.Red;

                GameUtil.Examine(Menu, squad.Name);
            }

            int width = Main.Game.MenuSize.X * 4;
            int height = Main.Game.MenuSize.X;

            bool wounded = false;
            for (int i = 0; i < squad.Characters.Count; i++)
            {
                Character character = squad.Characters[i];
                if (character.HealthBar.Value < character.HealthBar.Max_Value)
                {
                    wounded = true;
                    break;
                }
            }

            if (wounded)
            {
                examine.Text += "\n(Wounded)";
                height += (Main.Game.MenuSize.X / 2);
            }

            bool tired = false;
            for (int i = 0; i < squad.Characters.Count; i++)
            {
                Character character = squad.Characters[i];
                if (character.ManaBar.Value < character.ManaBar.Max_Value)
                {
                    tired = true;
                    break;
                }
            }

            if (tired)
            {
                examine.Text += "\n(Tired)";
                height += (Main.Game.MenuSize.X / 2);
            }

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
            Menu.AddPicture(Handler.GetID(), "Highlight", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), Color.White, false);
            Menu.AddPicture(Handler.GetID(), "Select", AssetManager.Textures["Grid_Hover"], new Region(0, 0, 0, 0), new Color(0, 255, 0, 255), false);
            Menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Examine", "", Color.White, AssetManager.Textures["ButtonFrame_Large"],
                new Region(0, 0, 0, 0), false);

            Menu.Visible = true;
            Resize(Main.Game.Resolution);
        }

        public override void Resize(Point point)
        {
            if (World.Maps.Any())
            {
                WorldUtil.Resize_OnZoom(Menu, World.Maps[Handler.Level], false);
            }
        }

        #endregion
    }
}
