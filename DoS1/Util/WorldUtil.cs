using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Menus;
using OP_Engine.Particles;
using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Time;
using OP_Engine.Utility;
using OP_Engine.Weathers;
using System.Collections.Generic;
using System.Linq;

namespace DoS1.Util
{
    public static class WorldUtil
    {
        #region Variables



        #endregion

        #region Constructors



        #endregion

        #region Methods

        public static Scene GetScene()
        {
            Scene scene = SceneManager.GetScene("Worldmap");
            if (Handler.LocalMap)
            {
                scene = SceneManager.GetScene("Localmap");
            }

            return scene;
        }

        public static Map GetMap(World world)
        {
            Map map = null;

            if (world.Maps.Any())
            {
                if (Handler.LocalMap)
                {
                    map = world.Maps[Handler.Level];
                }
                else
                {
                    map = world.Maps[0];
                }
            }

            return map;
        }

        public static void Resize_OnZoom(Menu menu, Map map, bool world_map)
        {
            if (map != null)
            {
                Layer ground = map.GetLayer("Ground");
                if (ground != null)
                {
                    Tile current = null;

                    int count = ground.Tiles.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Tile tile = ground.Tiles[i];
                        if (InputManager.MouseWithin(tile.Region.ToRectangle))
                        {
                            current = tile;
                            break;
                        }
                    }

                    if (current == null)
                    {
                        int x = Main.Game.ScreenWidth / 2;
                        int y = Main.Game.ScreenHeight / 2;

                        for (int i = 0; i < count; i++)
                        {
                            Tile tile = ground.Tiles[i];
                            if (x >= tile.Region.X && x < tile.Region.X + tile.Region.Width &&
                                y >= tile.Region.Y && y < tile.Region.Y + tile.Region.Height)
                            {
                                current = tile;
                                break;
                            }
                        }
                    }

                    if (current != null)
                    {
                        ResizeMap(menu, map, ground, current, world_map);
                    }
                }
            }
        }

        public static void Resize_OnStart(Menu menu, Map map)
        {
            if (map != null)
            {
                Layer ground = map.GetLayer("Ground");
                if (ground != null)
                {
                    Tile target = ground.GetTile(new Vector2(ground.Columns / 2, ground.Rows / 2));

                    if (Handler.LocalMap)
                    {
                        Army army = CharacterManager.GetArmy("Ally");
                        Squad squad = army.Squads[0];
                        if (squad.Location != null)
                        {
                            target = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));
                        }
                    }

                    CameraToTile(menu, map, ground, target);
                }
            }
        }

        public static void Resize_OnCombat(World world)
        {
            if (world.Maps.Any())
            {
                Map map = world.Maps[0];
                if (map != null)
                {
                    Layer ground = map.GetLayer("Ground");
                    if (ground != null)
                    {
                        int starting_y = (Main.Game.Resolution.Y / 8) * 5;
                        int width = Main.Game.Resolution.X / ground.Columns;
                        int height = (Main.Game.Resolution.Y - starting_y) / ground.Rows;

                        for (int y = 0; y < ground.Rows; y++)
                        {
                            for (int x = 0; x < ground.Columns; x++)
                            {
                                Tile tile = ground.GetTile(new Vector2(x, y));
                                if (tile != null)
                                {
                                    tile.Region = new Region(x * width, starting_y + (y * height), width, height);
                                }
                            }
                        }

                        Squad ally_squad = CharacterManager.GetArmy("Ally").GetSquad(Handler.Combat_Ally_Squad);

                        Squad enemy_squad = CharacterManager.GetArmy("Enemy").GetSquad(Handler.Combat_Enemy_Squad);
                        if (enemy_squad == null)
                        {
                            enemy_squad = CharacterManager.GetArmy("Special").GetSquad(Handler.Combat_Enemy_Squad);
                        }

                        if (ally_squad != null &&
                            enemy_squad != null)
                        {
                            for (int y = 0; y < 3; y++)
                            {
                                for (int x = 0; x < 3; x++)
                                {
                                    Character ally = ally_squad.GetCharacter(new Vector2(x, y));
                                    if (ally != null)
                                    {
                                        Tile tile = CombatUtil.OriginTile(world, ally);
                                        if (tile != null)
                                        {
                                            Resize_CharacterCombat(tile, ally);
                                        }
                                    }

                                    Character enemy = enemy_squad.GetCharacter(new Vector2(x, y));
                                    if (enemy != null)
                                    {
                                        Tile tile = CombatUtil.OriginTile(world, enemy);
                                        if (tile != null)
                                        {
                                            Resize_CharacterCombat(tile, enemy);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Resize_CharacterCombat(Tile tile, Character character)
        {
            character.Region = new Region(tile.Region.X, tile.Region.Y - (tile.Region.Height * 2), tile.Region.Width, tile.Region.Height * 2.5f);

            float bar_x = character.Region.X + (character.Region.Width / 8);
            float bar_width = (character.Region.Width / 8) * 6;
            float bar_height = character.Region.Width / 16;

            character.HealthBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height, bar_width, bar_height);
            character.HealthBar.Visible = true;
            character.HealthBar.Update();

            character.ManaBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height + bar_height, bar_width, bar_height);
            character.ManaBar.Visible = true;
            character.ManaBar.Update();

            CharacterUtil.UpdateGear(character);
        }

        public static void CameraToTile(Menu menu, Map map, Layer ground, Tile target)
        {
            if (target != null)
            {
                target.Region.X = Main.Game.Resolution.X / 2 - (Main.Game.TileSize.X / 2);
                target.Region.Y = Main.Game.Resolution.Y / 2 - (Main.Game.TileSize.Y / 2);

                ResizeMap(menu, map, ground, target, false);
            }
        }

        public static void ResizeMap(Menu menu, Map map, Layer ground, Tile current, bool world_map)
        {
            current.Region.Width = Main.Game.TileSize.X;
            current.Region.Height = Main.Game.TileSize.Y;

            int count = ground.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = ground.Tiles[i];
                if (tile.ID != current.ID)
                {
                    int x_diff = (int)tile.Location.X - (int)current.Location.X;
                    if (x_diff < 0)
                    {
                        x_diff *= -1;
                    }

                    int y_diff = (int)tile.Location.Y - (int)current.Location.Y;
                    if (y_diff < 0)
                    {
                        y_diff *= -1;
                    }

                    tile.Region.Width = Main.Game.TileSize.X;
                    tile.Region.Height = Main.Game.TileSize.Y;

                    if (tile.Location.X < current.Location.X)
                    {
                        tile.Region.X = current.Region.X - (x_diff * Main.Game.TileSize.X);
                    }
                    else if (tile.Location.X > current.Location.X)
                    {
                        tile.Region.X = current.Region.X + (x_diff * Main.Game.TileSize.X);
                    }
                    else if (tile.Location.X == current.Location.X)
                    {
                        tile.Region.X = current.Region.X;
                    }

                    if (tile.Location.Y < current.Location.Y)
                    {
                        tile.Region.Y = current.Region.Y - (y_diff * Main.Game.TileSize.Y);
                    }
                    else if (tile.Location.Y > current.Location.Y)
                    {
                        tile.Region.Y = current.Region.Y + (y_diff * Main.Game.TileSize.Y);
                    }
                    else if (tile.Location.Y == current.Location.Y)
                    {
                        tile.Region.Y = current.Region.Y;
                    }
                }
            }

            if (!world_map)
            {
                int armyCount = CharacterManager.Armies.Count;
                for (int a = 0; a < armyCount; a++)
                {
                    Army army = CharacterManager.Armies[a];

                    int squadCount = army.Squads.Count;
                    for (int s = 0; s < squadCount; s++)
                    {
                        Squad squad = army.Squads[s];
                        if (squad.Region != null)
                        {
                            int x_diff = (int)squad.Location.X - (int)current.Location.X;
                            if (x_diff < 0)
                            {
                                x_diff *= -1;
                            }

                            int y_diff = (int)squad.Location.Y - (int)current.Location.Y;
                            if (y_diff < 0)
                            {
                                y_diff *= -1;
                            }

                            squad.Region.Width = Main.Game.TileSize.X;
                            squad.Region.Height = Main.Game.TileSize.Y;

                            if (squad.Location.X < current.Location.X)
                            {
                                squad.Region.X = current.Region.X - (x_diff * Main.Game.TileSize.X);
                            }
                            else if (squad.Location.X > current.Location.X)
                            {
                                squad.Region.X = current.Region.X + (x_diff * Main.Game.TileSize.X);
                            }
                            else if (squad.Location.X == current.Location.X)
                            {
                                squad.Region.X = current.Region.X;
                            }

                            if (squad.Location.Y < current.Location.Y)
                            {
                                squad.Region.Y = current.Region.Y - (y_diff * Main.Game.TileSize.Y);
                            }
                            else if (squad.Location.Y > current.Location.Y)
                            {
                                squad.Region.Y = current.Region.Y + (y_diff * Main.Game.TileSize.Y);
                            }
                            else if (squad.Location.Y == current.Location.Y)
                            {
                                squad.Region.Y = current.Region.Y;
                            }

                            Vector2 squad_screen_location = new Vector2(squad.Region.X + squad.Region.Width / 2,
                                squad.Region.Y + squad.Region.Height / 2);

                            float new_speed = Get_TerrainSpeed(map, squad_screen_location);
                            float move_percent = squad.Moved / squad.Move_TotalDistance;

                            squad.Moved = move_percent * Main.Game.TileSize.X;
                            squad.Speed = new_speed;
                            squad.Move_TotalDistance = Main.Game.TileSize.X;

                            if (squad.Moving)
                            {
                                Tile current_tile = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));
                                if (squad.Destination.X < current_tile.Location.X)
                                {
                                    squad.Region.X -= squad.Moved;
                                }
                                else if (squad.Destination.X > current_tile.Location.X)
                                {
                                    squad.Region.X += squad.Moved;
                                }

                                if (squad.Destination.Y < current_tile.Location.Y)
                                {
                                    squad.Region.Y -= squad.Moved;
                                }
                                else if (squad.Destination.Y > current_tile.Location.Y)
                                {
                                    squad.Region.Y += squad.Moved;
                                }
                            }
                        }
                    }
                }

                Picture highlight = menu.GetPicture("Highlight");
                if (highlight != null)
                {
                    highlight.Visible = false;
                }

                Picture select = menu.GetPicture("Select");
                if (select != null)
                {
                    select.Visible = false;
                }
            }
        }

        public static void ResetMap_Combat(Squad ally_squad)
        {
            Scene scene = SceneManager.GetScene("Localmap");
            Map map = GetMap(scene.World);
            Layer ground = map.GetLayer("Ground");

            Tile current = ground.GetTile(new Vector2(ally_squad.Location.X, ally_squad.Location.Y));
            if (current != null)
            {
                current.Region.Width = Main.Game.TileSize.X;
                current.Region.Height = Main.Game.TileSize.Y;

                int count = ground.Tiles.Count;
                for (int i = 0; i < count; i++)
                {
                    Tile tile = ground.Tiles[i];
                    if (tile.ID != current.ID)
                    {
                        int x_diff = (int)tile.Location.X - (int)current.Location.X;
                        if (x_diff < 0)
                        {
                            x_diff *= -1;
                        }

                        int y_diff = (int)tile.Location.Y - (int)current.Location.Y;
                        if (y_diff < 0)
                        {
                            y_diff *= -1;
                        }

                        tile.Region.Width = Main.Game.TileSize.X;
                        tile.Region.Height = Main.Game.TileSize.Y;

                        if (tile.Location.X < current.Location.X)
                        {
                            tile.Region.X = current.Region.X - (x_diff * Main.Game.TileSize.X);
                        }
                        else if (tile.Location.X > current.Location.X)
                        {
                            tile.Region.X = current.Region.X + (x_diff * Main.Game.TileSize.X);
                        }
                        else if (tile.Location.X == current.Location.X)
                        {
                            tile.Region.X = current.Region.X;
                        }

                        if (tile.Location.Y < current.Location.Y)
                        {
                            tile.Region.Y = current.Region.Y - (y_diff * Main.Game.TileSize.Y);
                        }
                        else if (tile.Location.Y > current.Location.Y)
                        {
                            tile.Region.Y = current.Region.Y + (y_diff * Main.Game.TileSize.Y);
                        }
                        else if (tile.Location.Y == current.Location.Y)
                        {
                            tile.Region.Y = current.Region.Y;
                        }
                    }
                }

                int armyCount = CharacterManager.Armies.Count;
                for (int a = 0; a < armyCount; a++)
                {
                    Army army = CharacterManager.Armies[a];
                    if (army.Name != "Special")
                    {
                        int squadCount = army.Squads.Count;
                        for (int s = 0; s < squadCount; s++)
                        {
                            Squad squad = army.Squads[s];
                            if (squad.Region != null)
                            {
                                if (squad.Moving)
                                {
                                    int x_diff = (int)squad.Location.X - (int)current.Location.X;
                                    if (x_diff < 0)
                                    {
                                        x_diff *= -1;
                                    }

                                    int y_diff = (int)squad.Location.Y - (int)current.Location.Y;
                                    if (y_diff < 0)
                                    {
                                        y_diff *= -1;
                                    }

                                    squad.Region.Width = Main.Game.TileSize.X;
                                    squad.Region.Height = Main.Game.TileSize.Y;

                                    if (squad.Location.X < current.Location.X)
                                    {
                                        squad.Region.X = current.Region.X - (x_diff * Main.Game.TileSize.X);
                                    }
                                    else if (squad.Location.X > current.Location.X)
                                    {
                                        squad.Region.X = current.Region.X + (x_diff * Main.Game.TileSize.X);
                                    }
                                    else if (squad.Location.X == current.Location.X)
                                    {
                                        squad.Region.X = current.Region.X;
                                    }

                                    if (squad.Location.Y < current.Location.Y)
                                    {
                                        squad.Region.Y = current.Region.Y - (y_diff * Main.Game.TileSize.Y);
                                    }
                                    else if (squad.Location.Y > current.Location.Y)
                                    {
                                        squad.Region.Y = current.Region.Y + (y_diff * Main.Game.TileSize.Y);
                                    }
                                    else if (squad.Location.Y == current.Location.Y)
                                    {
                                        squad.Region.Y = current.Region.Y;
                                    }

                                    Vector2 squad_screen_location = new Vector2(squad.Region.X + squad.Region.Width / 2,
                                    squad.Region.Y + squad.Region.Height / 2);

                                    float new_speed = Get_TerrainSpeed(map, squad_screen_location);
                                    float move_percent = squad.Moved / squad.Move_TotalDistance;

                                    squad.Moved = move_percent * Main.Game.TileSize.X;
                                    squad.Speed = new_speed;
                                    squad.Move_TotalDistance = Main.Game.TileSize.X;

                                    Tile current_tile = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));
                                    if (squad.Destination.X < current_tile.Location.X)
                                    {
                                        squad.Region.X -= squad.Moved;
                                    }
                                    else if (squad.Destination.X > current_tile.Location.X)
                                    {
                                        squad.Region.X += squad.Moved;
                                    }

                                    if (squad.Destination.Y < current_tile.Location.Y)
                                    {
                                        squad.Region.Y -= squad.Moved;
                                    }
                                    else if (squad.Destination.Y > current_tile.Location.Y)
                                    {
                                        squad.Region.Y += squad.Moved;
                                    }
                                }
                                else
                                {
                                    Tile tile = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));
                                    if (tile != null)
                                    {
                                        squad.Region = new Region(tile.Region.X, tile.Region.Y, tile.Region.Width, tile.Region.Height);
                                    }
                                }
                            }
                        }
                    }
                }

                scene.Menu.GetPicture("Highlight").Visible = false;
                scene.Menu.GetPicture("Select").Visible = false;
            }
        }

        public static void MoveGrid(World world)
        {
            if (world.Maps.Any())
            {
                int x_diff = InputManager.Mouse.X - InputManager.Mouse.lastMouseState.X;
                int y_diff = InputManager.Mouse.Y - InputManager.Mouse.lastMouseState.Y;

                Map map = GetMap(world);
                if (map != null)
                {
                    Layer ground = map.GetLayer("Ground");

                    int count = ground.Tiles.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Tile tile = ground.Tiles[i];
                        tile.Region.X += x_diff;
                        tile.Region.Y += y_diff;
                    }

                    int armyCount = CharacterManager.Armies.Count;
                    for (int a = 0; a < armyCount; a++)
                    {
                        Army army = CharacterManager.Armies[a];

                        int squadCount = army.Squads.Count;
                        for (int s = 0; s < squadCount; s++)
                        {
                            Squad squad = army.Squads[s];
                            if (squad.Visible)
                            {
                                squad.Region.X += x_diff;
                                squad.Region.Y += y_diff;
                            }
                        }
                    }

                    int weatherCount = WeatherManager.Weathers.Count;
                    for (int i = 0; i <  weatherCount; i++)
                    {
                        Weather weather = WeatherManager.Weathers[i];
                        if (weather.Visible)
                        {
                            for (int p = 0; p < weather.ParticleManager.Particles.Count; p++)
                            {
                                Particle particle = weather.ParticleManager.Particles[p];
                                particle.Location.X += x_diff;
                                particle.Location.Y += y_diff;
                            }
                        }
                    }
                }

                Handler.MovingGrid = true;
            }
        }

        public static void ZoomIn()
        {
            if (Main.Game.Zoom < 2)
            {
                Main.Game.Zoom += 0.25f;
            }
            else
            {
                Main.Game.Zoom++;
            }
                
            if (Main.Game.Zoom > 8)
            {
                Main.Game.Zoom = 8;
            }
            else
            {
                Main.Game.ResolutionChange();
            }
        }

        public static void ZoomOut()
        {
            if (Main.Game.Zoom > 2)
            {
                Main.Game.Zoom--;
            }
            else
            {
                Main.Game.Zoom -= 0.25f;
            }
                
            if (Main.Game.Zoom < 0.5)
            {
                Main.Game.Zoom = 0.5f;
            }

            Main.Game.ResolutionChange();
        }

        public static void DeselectToken()
        {
            AssetManager.PlaySound_Random("Click");

            Handler.Selected_Token = -1;

            Scene localmap = SceneManager.GetScene("Localmap");

            Menu menu = localmap.Menu;
            menu.GetPicture("Select").Visible = false;
            menu.GetPicture("Highlight").Visible = true;
            menu.GetPicture("Highlight").DrawColor = new Color(255, 255, 255, 255);

            Map map = GetMap(localmap.World);
            if (map != null)
            {
                Layer pathing = map.GetLayer("Pathing");
                pathing.Visible = false;
            }

            GameUtil.Toggle_Pause(false);
            InputManager.Mouse.Flush();
        }

        public static void AnimateTiles()
        {
            Scene scene = SceneManager.GetScene("Worldmap");
            if (Handler.Combat)
            {
                scene = SceneManager.GetScene("Combat");
            }
            else if (Handler.LocalMap)
            {
                scene = SceneManager.GetScene("Localmap");
            }

            foreach (Map map in scene.World.Maps)
            {
                Layer ground = map.GetLayer("Ground");

                int count = ground.Tiles.Count;
                for (int i = 0; i < count; i++)
                {
                    Tile tile = ground.Tiles[i];
                    if (tile.Type == "Water")
                    {
                        tile.Animate();
                    }
                }
            }
        }

        public static void UpdateTime()
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

            Menu ui = MenuManager.GetMenu("UI");
            Label time = ui.GetLabel("Time");
            Label date = ui.GetLabel("Date");

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

        public static Tile Get_Tile(Layer layer, Vector2 location)
        {
            int count = layer.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = layer.Tiles[i];
                if (tile.Location.X == location.X &&
                    tile.Location.Y == location.Y)
                {
                    return tile;
                }
            }

            return null;
        }

        public static void DisplayPath_Temp(Map map, Tile destination)
        {
            Layer ground = map.GetLayer("Ground");
            Layer roads = map.GetLayer("Roads");

            Layer pathing = map.GetLayer("Pathing");
            if (pathing != null)
            {
                pathing.Tiles.Clear();

                Army army = CharacterManager.GetArmy("Ally");
                Squad squad = army.GetSquad(Handler.Selected_Token);
                if (squad == null)
                {
                    return;
                }

                List<ALocation> path = Handler.Pathing.Get_Path(ground, roads, squad, destination, ground.Columns * ground.Rows, false);
                if (path == null ||
                    path.Count == 0)
                {
                    return;
                }

                path.Reverse();

                ALocation start = path[0];
                Tile start_tile = ground.GetTile(new Vector2(start.X, start.Y));

                if (squad.Region.X == start_tile.Region.X &&
                    squad.Region.Y == start_tile.Region.Y)
                {
                    //Exclude starting location if already there
                    path.RemoveAt(0);
                }

                Vector2 previous_location = new Vector2(-1, -1);
                Vector2 current_location = new Vector2(squad.Location.X, squad.Location.Y);
                Vector2 next_location = new Vector2(path[0].X, path[0].Y);

                AddTileToPathing(ground, pathing, current_location, previous_location, next_location);

                int count = path.Count;
                for (int i = 0; i < count; i++)
                {
                    ALocation location = path[i];

                    previous_location = new Vector2(-1, -1);
                    current_location = new Vector2(location.X, location.Y);
                    next_location = new Vector2(-1, -1);

                    if (i == 0)
                    {
                        previous_location = new Vector2(squad.Location.X, squad.Location.Y);
                    }
                    else if (i > 0)
                    {
                        previous_location = new Vector2(path[i - 1].X, path[i - 1].Y);
                    }

                    if (i < count - 1)
                    {
                        next_location = new Vector2(path[i + 1].X, path[i + 1].Y);
                    }

                    AddTileToPathing(ground, pathing, current_location, previous_location, next_location);
                }

                pathing.Visible = true;
            }
        }

        public static void DisplayPath_Squad(Menu menu, Map map, Squad squad)
        {
            if (squad == null ||
                map == null)
            {
                return;
            }

            Layer ground = map.GetLayer("Ground");

            Layer pathing = map.GetLayer("Pathing");
            pathing.Tiles.Clear();

            List<ALocation> path = squad.Path;
            if (path == null ||
                path.Count == 0)
            {
                return;
            }

            Tile destination = ground.GetTile(new Vector2(squad.Coordinates.X, squad.Coordinates.Y));
            if (destination != null)
            {
                Picture select = menu.GetPicture("Select");
                select.Texture = AssetManager.Textures["Grid_Hover"];
                select.Region = destination.Region;
                select.Visible = true;

                Squad target = ArmyUtil.Get_TargetSquad(squad.GetLeader().Target_ID);
                if (target != null)
                {
                    select.Texture = AssetManager.Textures["Highlight_Circle"];
                    select.Region = target.Region;

                    if (target.Type == "Ally")
                    {
                        select.DrawColor = new Color(0, 0, 255, 255);
                    }
                    else if (target.Type == "Enemy")
                    {
                        select.DrawColor = new Color(255, 0, 0, 255);
                    }
                }
                else
                {
                    select.DrawColor = new Color(0, 255, 0, 255);
                }
            }

            for (int i = 0; i < path.Count; i++)
            {
                ALocation location = path[i];

                Vector2 previous_location = new Vector2(-1, -1);
                Vector2 current_location = new Vector2(location.X, location.Y);
                Vector2 next_location = new Vector2(-1, -1);

                if (i == 0)
                {
                    previous_location = new Vector2(squad.Location.X, squad.Location.Y);
                }
                else if (i > 0)
                {
                    previous_location = new Vector2(path[i - 1].X, path[i - 1].Y);
                }

                if (i < path.Count - 1)
                {
                    next_location = new Vector2(path[i + 1].X, path[i + 1].Y);
                }

                AddTileToPathing(ground, pathing, current_location, previous_location, next_location);
            }

            pathing.Visible = true;
        }

        public static void AddTileToPathing(Layer ground, Layer pathing, Vector2 current_location, Vector2 previous_location, Vector2 next_location)
        {
            Texture2D texture = null;

            Tile tile = ground.GetTile(current_location);
            if (tile != null)
            {
                if (previous_location.X != -1 &&
                    previous_location.Y != -1 &&
                    next_location.X != -1 &&
                    next_location.Y != -1)
                {
                    Direction previous_direction = Get_Direction(previous_location, current_location);
                    Direction direction = Get_Direction(previous_location, next_location);

                    switch (direction)
                    {
                        case Direction.North:
                            if (previous_direction == Direction.Nowhere)
                            {
                                texture = Handler.GetTexture("Path_N");
                            }
                            else
                            {
                                texture = Handler.GetTexture("Path_NS");
                            }
                            break;

                        case Direction.East:
                            if (previous_direction == Direction.Nowhere)
                            {
                                texture = Handler.GetTexture("Path_E");
                            }
                            else
                            {
                                texture = Handler.GetTexture("Path_WE");
                            }
                            break;

                        case Direction.South:
                            if (previous_direction == Direction.Nowhere)
                            {
                                texture = Handler.GetTexture("Path_S");
                            }
                            else
                            {
                                texture = Handler.GetTexture("Path_NS");
                            }
                            break;

                        case Direction.West:
                            if (previous_direction == Direction.Nowhere)
                            {
                                texture = Handler.GetTexture("Path_W");
                            }
                            else
                            {
                                texture = Handler.GetTexture("Path_WE");
                            }
                            break;

                        case Direction.NorthEast:
                            if (previous_direction == Direction.North)
                            {
                                texture = Handler.GetTexture("Path_SE");
                            }
                            else if (previous_direction == Direction.East)
                            {
                                texture = Handler.GetTexture("Path_NW");
                            }
                            break;

                        case Direction.NorthWest:
                            if (previous_direction == Direction.North)
                            {
                                texture = Handler.GetTexture("Path_SW");
                            }
                            else if (previous_direction == Direction.West)
                            {
                                texture = Handler.GetTexture("Path_NE");
                            }
                            break;

                        case Direction.SouthEast:
                            if (previous_direction == Direction.South)
                            {
                                texture = Handler.GetTexture("Path_NE");
                            }
                            else if (previous_direction == Direction.East)
                            {
                                texture = Handler.GetTexture("Path_SW");
                            }
                            break;

                        case Direction.SouthWest:
                            if (previous_direction == Direction.South)
                            {
                                texture = Handler.GetTexture("Path_NW");
                            }
                            else if (previous_direction == Direction.West)
                            {
                                texture = Handler.GetTexture("Path_SE");
                            }
                            break;
                    }
                }
                else if (next_location.X != -1 &&
                         next_location.Y != -1)
                {
                    Direction direction = Get_Direction(current_location, next_location);

                    switch (direction)
                    {
                        case Direction.North:
                            texture = Handler.GetTexture("Path_N");
                            break;

                        case Direction.East:
                            texture = Handler.GetTexture("Path_E");
                            break;

                        case Direction.South:
                            texture = Handler.GetTexture("Path_S");
                            break;

                        case Direction.West:
                            texture = Handler.GetTexture("Path_W");
                            break;
                    }
                }
                else if (previous_location.X != -1 &&
                         previous_location.Y != -1)
                {
                    Direction direction = Get_Direction(previous_location, current_location);

                    switch (direction)
                    {
                        case Direction.North:
                            texture = Handler.GetTexture("Path_S");
                            break;

                        case Direction.East:
                            texture = Handler.GetTexture("Path_W");
                            break;

                        case Direction.South:
                            texture = Handler.GetTexture("Path_N");
                            break;

                        case Direction.West:
                            texture = Handler.GetTexture("Path_E");
                            break;
                    }
                }

                if (texture != null)
                {
                    pathing.Tiles.Add(new Tile
                    {
                        Visible = true,
                        Name = texture.Name,
                        Texture = texture,
                        Image = new Rectangle(0, 0, texture.Width, texture.Height),
                        Location = new Location(current_location.X, current_location.Y, 0),
                        Region = tile.Region
                    });
                }
            }
        }

        public static Direction Get_Direction(Vector2 origin, Vector2 destination)
        {
            Direction direction = Direction.Nowhere;

            if (origin.X < destination.X)
            {
                if (origin.Y > destination.Y)
                {
                    direction = Direction.NorthEast;
                }
                else if (origin.Y < destination.Y)
                {
                    direction = Direction.SouthEast;
                }
                else if (origin.Y == destination.Y)
                {
                    direction = Direction.East;
                }
            }
            else if (origin.X > destination.X)
            {
                if (origin.Y > destination.Y)
                {
                    direction = Direction.NorthWest;
                }
                else if (origin.Y < destination.Y)
                {
                    direction = Direction.SouthWest;
                }
                else if (origin.Y == destination.Y)
                {
                    direction = Direction.West;
                }
            }
            else
            {
                if (origin.Y > destination.Y)
                {
                    direction = Direction.North;
                }
                else if (origin.Y < destination.Y)
                {
                    direction = Direction.South;
                }
            }

            return direction;
        }

        public static int GetDistance(Location origin, Location target)
        {
            int x_diff = (int)origin.X - (int)target.X;
            if (x_diff < 0)
            {
                x_diff *= -1;
            }

            int y_diff = (int)origin.Y - (int)target.Y;
            if (y_diff < 0)
            {
                y_diff *= -1;
            }

            return x_diff + y_diff;
        }

        public static string Get_Terrain_Tile(Map map, Vector2 location)
        {
            Layer ground = map.GetLayer("Ground");

            Tile tile = ground.GetTile(location);
            if (tile != null)
            {
                return tile.Type;
            }

            return null;
        }

        public static string Get_Terrain_Screen(Map map, Vector2 screen_location)
        {
            Layer roads = map.GetLayer("Roads");
            if (roads != null)
            {
                int roadCount = roads.Tiles.Count;
                for (int i = 0; i < roadCount; i++)
                {
                    Tile tile = roads.Tiles[i];
                    if (screen_location.X >= tile.Region.X && screen_location.X < tile.Region.X + tile.Region.Width &&
                        screen_location.Y >= tile.Region.Y && screen_location.Y < tile.Region.Y + tile.Region.Height)
                    {
                        return "Road";
                    }
                }
            }

            Layer ground = map.GetLayer("Ground");

            int count = ground.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = ground.Tiles[i];
                if (screen_location.X >= tile.Region.X && screen_location.X < tile.Region.X + tile.Region.Width &&
                    screen_location.Y >= tile.Region.Y && screen_location.Y < tile.Region.Y + tile.Region.Height)
                {
                    return tile.Type;
                }
            }

            return null;
        }

        public static float Get_TerrainSpeed(Map map, Vector2 screen_location)
        {
            float speed = 0;

            string terrain = Get_Terrain_Screen(map, screen_location);
            switch (terrain)
            {
                case "Grass":
                case "Desert":
                case "Snow":
                case "Ice":
                    speed = (float)Main.Game.TileSize.X / 24; //2
                    break;

                case "Forest":
                case "Forest_Snow":
                    speed = (float)Main.Game.TileSize.X / 32; //1.5
                    break;

                case "Water":
                    speed = (float)Main.Game.TileSize.X / 48; //1
                    break;

                case "Mountains":
                case "Mountains_Desert":
                case "Mountains_Snow":
                    speed = (float)Main.Game.TileSize.X / 96; //0.5
                    break;

                case "Road":
                    speed = (float)Main.Game.TileSize.X / 12; //4
                    break;
            }

            return speed / 20;
        }

        public static Tile Get_Base(Map map, string type)
        {
            Layer locations = map.GetLayer("Locations");

            int count = locations.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = locations.Tiles[i];
                if (tile.Type == "Base_" + type)
                {
                    return tile;
                }
            }

            return null;
        }

        public static Tile GetLocation(Squad squad)
        {
            if (squad != null)
            {
                Scene scene = SceneManager.GetScene("Localmap");
                if (scene.World.Maps.Any())
                {
                    Map map = scene.World.Maps[Handler.Level];
                    if (map != null)
                    {
                        Layer locations = map.GetLayer("Locations");
                        if (locations != null)
                        {
                            int count = locations.Tiles.Count;
                            for (int i = 0; i < count; i++)
                            {
                                Tile tile = locations.Tiles[i];
                                if (squad.Location.X == tile.Location.X &&
                                    squad.Location.Y == tile.Location.Y)
                                {
                                    return tile;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static List<Tile> GetTowns(Map map)
        {
            List<Tile> towns = new List<Tile>();

            Layer locations = map.GetLayer("Locations");
            if (locations != null)
            {
                int count = locations.Tiles.Count;
                for (int i = 0; i < count; i++)
                {
                    Tile tile = locations.Tiles[i];
                    if (!tile.Type.Contains("Base"))
                    {
                        towns.Add(tile);
                    }
                }
            }

            return towns;
        }

        public static Tile GetNearest_Town(Map map, Layer ground, Army army, Squad squad, bool to_guard)
        {
            if (squad != null &&
                map != null)
            {
                List<Tile> Towns = GetTowns(map);
                Tile nearest = null;

                if (to_guard)
                {
                    //Are we already on a captured town?
                    int townCount = Towns.Count;
                    for (int i = 0; i < townCount; i++)
                    {
                        Tile town = Towns[i];

                        if (town.Type.Contains("Enemy") &&
                            !town.Type.Contains("Base") &&
                            town.Location.X == squad.Location.X &&
                            town.Location.Y == squad.Location.Y)
                        {
                            return town;
                        }
                    }
                }

                //Remove towns already owned
                for (int i = 0; i < Towns.Count; i++)
                {
                    Tile town = Towns[i];
                    if (town.Type.Contains("Enemy"))
                    {
                        Towns.Remove(town);
                        i--;
                    }
                }

                //Remove towns already targeted
                for (int i = 0; i < Towns.Count; i++)
                {
                    Tile town = Towns[i];

                    int squadCount = army.Squads.Count;
                    for (int s = 0; s < squadCount; s++)
                    {
                        Squad existing = army.Squads[s];
                        if (existing.ID != squad.ID &&
                            existing.Path.Any())
                        {
                            ALocation target = existing.Path[existing.Path.Count - 1];

                            if (town.Location.X == target.X &&
                                town.Location.Y == target.Y)
                            {
                                Towns.Remove(town);
                                i--;
                            }
                        }
                    }
                }

                //Get first town
                int count = Towns.Count;
                for (int i = 0; i < count; i++)
                {
                    Tile town = Towns[i];
                    if (!town.Type.Contains("Enemy"))
                    {
                        Squad existing = ArmyUtil.Get_Squad(map, army, town.Location);
                        if (existing == null ||
                            existing.Type == "Ally")
                        {
                            nearest = town;
                            break;
                        }
                    }
                }

                if (nearest != null)
                {
                    int distance = GetDistance(squad.Location, nearest.Location);

                    //Check for closer town
                    for (int i = 0; i < count; i++)
                    {
                        Tile town = Towns[i];
                        if (!town.Type.Contains("Enemy"))
                        {
                            Squad existing = ArmyUtil.Get_Squad(map, army, town.Location);
                            int new_distance = GetDistance(squad.Location, town.Location);

                            if (new_distance < distance &&
                                (existing == null || existing.Type == "Ally"))
                            {
                                distance = new_distance;
                                nearest = town;
                            }
                        }
                    }

                    Tile nearest_ground = ground.GetTile(new Vector2(nearest.Location.X, nearest.Location.Y));
                    return nearest_ground;
                }
            }

            return null;
        }

        public static Tile GetNearest_Location_ToRest(Map map, Layer ground, Army army, Squad squad, bool to_guard)
        {
            if (squad != null &&
                map != null)
            {
                List<Tile> locations = new List<Tile>();

                Layer layer = map.GetLayer("Locations");
                if (layer != null)
                {
                    int layerCount = layer.Tiles.Count;
                    for (int i = 0; i < layerCount; i++)
                    {
                        Tile tile = layer.Tiles[i];
                        if (tile.Type != "Base_Ally")
                        {
                            locations.Add(tile);
                        }
                    }
                }

                //Are we already on something?
                int count = locations.Count;
                for (int i = 0; i < count; i++)
                {
                    Tile tile = locations[i];

                    if (tile.Location.X == squad.Location.X &&
                        tile.Location.Y == squad.Location.Y)
                    {
                        return tile;
                    }
                }

                //Get first location
                Tile nearest = locations[0];

                if (nearest != null)
                {
                    int distance = GetDistance(squad.Location, nearest.Location);

                    //Check for closer location
                    for (int i = 0; i < count; i++)
                    {
                        Tile tile = locations[i];

                        int new_distance = GetDistance(squad.Location, tile.Location);

                        if (new_distance < distance)
                        {
                            distance = new_distance;
                            nearest = tile;
                        }
                    }

                    Tile nearest_ground = ground.GetTile(new Vector2(nearest.Location.X, nearest.Location.Y));
                    return nearest_ground;
                }
            }

            return null;
        }

        public static Tile GetMarket()
        {
            World world = GetScene().World;
            Map map = GetMap(world);
            Layer locations = map.GetLayer("Locations");

            int count = locations.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = locations.Tiles[i];
                if (tile.Type.Contains("Market"))
                {
                    return tile;
                }
            }

            return null;
        }

        public static Squad GetNearest_Squad(Squad squad)
        {
            Army ally_army = CharacterManager.GetArmy("Ally");

            Squad nearest = ally_army.Squads[0];
            int distance = GetDistance(squad.Location, nearest.Location);

            int count = ally_army.Squads.Count;
            for (int i = 0; i < count; i++)
            {
                Squad ally_squad = ally_army.Squads[i];

                int new_distance = GetDistance(squad.Location, ally_squad.Location);
                if (new_distance < distance)
                {
                    distance = new_distance;
                    nearest = ally_squad;
                }
            }

            return nearest;
        }

        public static void ChangeLocation(Tile location, Squad squad)
        {
            if (location.Type.Contains("Academy"))
            {
                location.Type = "Academy_" + squad.Type;
            }
            else if (location.Type.Contains("Market"))
            {
                location.Type = "Market_" + squad.Type;
            }
            else if (location.Type.Contains("Base"))
            {
                if (!location.Type.Contains("Ally"))
                {
                    location.Type = "Base_" + squad.Type;
                }
            }
            else
            {
                location.Type = "Town_" + squad.Type;
            }

            location.Texture = Handler.GetTexture("Tile_" + location.Type);
        }

        public static int MaxLevelUnlocked()
        {
            int maxLevel = 0;

            Scene scene = SceneManager.GetScene("Worldmap");
            Map map = scene.World.Maps[0];

            Layer locations = map.GetLayer("Locations");

            int count = locations.Tiles.Count;
            for (int i = 0; i < count; i++)
            {
                Tile tile = locations.Tiles[i];
                if (tile.Visible)
                {
                    maxLevel = i;
                }
            }

            return maxLevel;
        }

        public static void AllyToken_Start(Squad squad, Map map)
        {
            Tile ally_base = Get_Base(map, "Ally");
            if (ally_base != null)
            {
                squad.Location = new Location(ally_base.Location.X, ally_base.Location.Y, ally_base.Location.Z);
                squad.Region = new Region(ally_base.Region.X, ally_base.Region.Y, ally_base.Region.Width, ally_base.Region.Height);
                squad.Visible = true;
                squad.Active = true;
            }
        }

        public static void EnemyToken_Start(Squad squad, Map map)
        {
            Tile enemy_base = Get_Base(map, "Enemy");
            if (enemy_base != null)
            {
                squad.Location = new Location(enemy_base.Location.X, enemy_base.Location.Y, enemy_base.Location.Z);
                squad.Region = new Region(enemy_base.Region.X, enemy_base.Region.Y, enemy_base.Region.Width, enemy_base.Region.Height);
                squad.Visible = true;
                squad.Active = true;
            }
        }

        public static Squad CheckSquadCollision(Squad squad)
        {
            if (squad.Type == "Ally")
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");

                int enemyCount = enemy_army.Squads.Count - 1;
                for (int i = enemyCount; i >= 0; i--)
                {
                    Squad enemy_squad = enemy_army.Squads[i];
                    if (enemy_squad.Region != null)
                    {
                        if (Utility.RegionsOverlapping(squad.Region, enemy_squad.Region))
                        {
                            return enemy_squad;
                        }
                    }
                }
            }
            else if (squad.Type == "Enemy")
            {
                Army ally_army = CharacterManager.GetArmy("Ally");

                int allyCount = ally_army.Squads.Count - 1;
                for (int i = allyCount; i >= 0; i--)
                {
                    Squad ally_squad = ally_army.Squads[i];
                    if (ally_squad.Region != null)
                    {
                        if (Utility.RegionsOverlapping(squad.Region, ally_squad.Region))
                        {
                            return ally_squad;
                        }
                    }
                }
            }

            return null;
        }

        public static void MoveSquads()
        {
            if (!Handler.MovingSquads)
            {
                Handler.MovingSquads = true;

                Scene localmap = SceneManager.GetScene("Localmap");
                if (localmap.World.Maps.Any())
                {
                    if (Handler.Level < localmap.World.Maps.Count)
                    {
                        Map map = localmap.World.Maps[Handler.Level];
                        Layer ground = map.GetLayer("Ground");

                        bool interrupt = false;

                        int armyCount = CharacterManager.Armies.Count;
                        for (int a = 0; a < armyCount; a++)
                        {
                            Army army = CharacterManager.Armies[a];
                            if (army.Name != "Reserves" &&
                                army.Name != "Special")
                            {
                                for (int s = 0; s < army.Squads.Count; s++)
                                {
                                    Squad squad = army.Squads[s];
                                    if (squad.Visible &&
                                        squad.Active)
                                    {
                                        if (squad.Path.Any() &&
                                            squad.Region != null)
                                        {
                                            ALocation path = squad.Path[0];

                                            Vector2 path_location = new Vector2(path.X, path.Y);
                                            Vector2 squad_screen_location = new Vector2(squad.Region.X + squad.Region.Width / 2,
                                                squad.Region.Y + squad.Region.Height / 2);
                                            Vector2 squad_tile_location = new Vector2(squad.Location.X, squad.Location.Y);

                                            if (squad_tile_location.X < path_location.X)
                                            {
                                                squad.Direction = Direction.East;
                                            }
                                            else if (squad_tile_location.X > path_location.X)
                                            {
                                                squad.Direction = Direction.West;
                                            }
                                            else if (squad_tile_location.Y < path_location.Y)
                                            {
                                                squad.Direction = Direction.South;
                                            }
                                            else if (squad_tile_location.Y > path_location.Y)
                                            {
                                                squad.Direction = Direction.North;
                                            }

                                            Tile destination = ground.GetTile(path_location);
                                            Tile location = ground.GetTile(squad_tile_location);

                                            squad.Move_TotalDistance = Main.Game.TileSize.X;
                                            squad.Moving = true;
                                            squad.Speed = Get_TerrainSpeed(map, squad_screen_location);
                                            squad.Destination = new Location(path_location.X, path_location.Y, 0);
                                            squad.Update();

                                            Squad other_squad = CheckSquadCollision(squad);
                                            if (other_squad != null)
                                            {
                                                if (Handler.StoryStep != 9 &&
                                                    Handler.StoryStep != 10 &&
                                                    Handler.StoryStep != 20 &&
                                                    Handler.StoryStep != 21)
                                                {
                                                    squad.Region = new Region(location.Region.X, location.Region.Y, location.Region.Width, location.Region.Height);
                                                    squad.Moving = false;

                                                    Vector2 other_squad_location = new Vector2(other_squad.Location.X, other_squad.Location.Y);
                                                    Tile other_location = ground.GetTile(other_squad_location);

                                                    other_squad.Region = new Region(other_location.Region.X, other_location.Region.Y, other_location.Region.Width, other_location.Region.Height);
                                                    other_squad.Moving = false;

                                                    if (Handler.StoryStep > 48)
                                                    {
                                                        if (Handler.StoryStep == 49)
                                                        {
                                                            Handler.StoryStep++;
                                                        }

                                                        interrupt = true;
                                                        CombatUtil.StartCombat(map, ground, destination, squad, other_squad);
                                                        break;
                                                    }
                                                }
                                            }

                                            if (squad.Location.X == squad.Destination.X &&
                                                squad.Location.Y == squad.Destination.Y)
                                            {
                                                squad.Region = new Region(destination.Region.X, destination.Region.Y, destination.Region.Width, destination.Region.Height);

                                                if (Handler.Hovering_Squad != null &&
                                                    Handler.Hovering_Squad.ID == squad.ID)
                                                {
                                                    //Fix highlight region reference breaking after setting new Region on squad
                                                    Picture highlight = localmap.Menu.GetPicture("Highlight");
                                                    highlight.Region = squad.Region;
                                                }

                                                squad.Path.Remove(path);

                                                Squad target = ArmyUtil.Get_TargetSquad(squad.GetLeader().Target_ID);
                                                if (target != null)
                                                {
                                                    //Chase targeted squad
                                                    Tile target_location = ground.GetTile(new Vector2(target.Location.X, target.Location.Y));
                                                    if (target_location != null)
                                                    {
                                                        ArmyUtil.SetPath(map, squad, target_location);
                                                    }
                                                }
                                                else
                                                {
                                                    //Check if landing at location
                                                    if (!squad.Path.Any())
                                                    {
                                                        Layer locations = map.GetLayer("Locations");
                                                        Tile location_tile = locations.GetTile(new Vector3(squad.Destination.X, squad.Destination.Y, 0));
                                                        location = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));

                                                        if (squad.Type == "Ally")
                                                        {
                                                            if (location_tile != null)
                                                            {
                                                                if (Handler.StoryStep == 10)
                                                                {
                                                                    interrupt = true;
                                                                    GameUtil.Toggle_Pause(false);
                                                                    localmap.Active = false;

                                                                    MenuManager.ChangeMenu("Market");
                                                                    break;
                                                                }
                                                                else if (Handler.StoryStep == 21)
                                                                {
                                                                    CameraToTile(localmap.Menu, map, ground, location);
                                                                    Handler.StoryStep++;
                                                                }
                                                                else if (Handler.StoryStep > 48)
                                                                {
                                                                    interrupt = true;
                                                                    CameraToTile(localmap.Menu, map, ground, location);
                                                                    GameUtil.Alert_Location(map, ground, squad, location_tile);
                                                                    break;
                                                                }
                                                            }
                                                            else if (Handler.StoryStep > 48)
                                                            {
                                                                interrupt = true;
                                                                GameUtil.Alert_MoveFinished(localmap.Menu, map, ground, squad, location);
                                                                break;
                                                            }
                                                        }
                                                        else if (squad.Type == "Enemy")
                                                        {
                                                            CryptoRandom random = new CryptoRandom();
                                                            bool new_choice = false;

                                                            if (location_tile != null)
                                                            {
                                                                if (location_tile.Type == "Base_Ally")
                                                                {
                                                                    interrupt = true;
                                                                    GameUtil.Alert_BaseCaptured(localmap.Menu, map, ground, location_tile);
                                                                }

                                                                ChangeLocation(location_tile, squad);

                                                                if (squad.Assignment == "Guard Nearest Town")
                                                                {
                                                                    int choice = random.Next(0, 2);
                                                                    if (choice == 0)
                                                                    {
                                                                        squad.Assignment = "Sleeper";
                                                                        AI_Util.Set_NextTarget(map, ground, army, squad);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    new_choice = true;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                new_choice = true;
                                                            }

                                                            if (new_choice)
                                                            {
                                                                AI_Util.Get_NewTarget(map, ground, army, squad);
                                                            }

                                                            if (interrupt)
                                                            {
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (squad.Type == "Enemy")
                                        {
                                            AI_Util.Get_NewTarget(map, ground, army, squad);
                                        }
                                    }
                                }

                                if (interrupt)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                Handler.MovingSquads = false;
            }
        }

        public static void RestSquads()
        {
            if (!Handler.RestingSquads)
            {
                Scene localmap = SceneManager.GetScene("Localmap");
                if (localmap.World.Maps.Any())
                {
                    Handler.RestingSquads = true;

                    Map map = localmap.World.Maps[Handler.Level];
                    Layer locations = map.GetLayer("Locations");

                    int count = locations.Tiles.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Tile tile = locations.Tiles[i];

                        int armyCount = CharacterManager.Armies.Count;
                        for (int a = 0; a < armyCount; a++)
                        {
                            Army army = CharacterManager.Armies[a];

                            for (int s = 0; s < army.Squads.Count; s++)
                            {
                                Squad squad = army.Squads[s];
                                if (squad.Location != null)
                                {
                                    if (squad.Location.X == tile.Location.X &&
                                        squad.Location.Y == tile.Location.Y &&
                                        !squad.Moving)
                                    {
                                        for (int c = 0; c < squad.Characters.Count; c++)
                                        {
                                            Character character = squad.Characters[c];
                                            character.HealthBar.IncreaseValue(1);
                                            character.ManaBar.IncreaseValue(1);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Handler.RestingSquads = false;
                }
            }
        }

        public static void Collect_Tax()
        {
            int gold = 0;

            Scene localmap = SceneManager.GetScene("Localmap");

            int mapCount = localmap.World.Maps.Count;
            for (int i = 0; i < mapCount; i++)
            {
                Map map = localmap.World.Maps[i];

                Layer locations = map.GetLayer("Locations");

                int count = locations.Tiles.Count;
                for (int j = 0; j < count; j++)
                {
                    Tile tile = locations.Tiles[j];
                    if (tile.Type.Contains("Ally"))
                    {
                        gold++;
                    }
                }
            }

            if (gold > 0)
            {
                Handler.Gold += gold;
                GameUtil.Alert_Generic("Gold +" + gold, Color.Gold);
            }
        }

        public static void EnterTown(string type)
        {
            Scene localmap = SceneManager.GetScene("Localmap");

            if (type.Contains("Market"))
            {
                localmap.Active = false;
                MenuManager.ChangeMenu("Market");
            }
            else if (type.Contains("Academy"))
            {
                localmap.Active = false;
                MenuManager.ChangeMenu("Academy");
            }
        }

        #endregion
    }
}
