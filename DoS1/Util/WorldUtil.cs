using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Tiles;
using OP_Engine.Utility;
using OP_Engine.Menus;
using OP_Engine.Weathers;
using OP_Engine.Particles;
using OP_Engine.Scenes;

namespace DoS1.Util
{
    public static class WorldUtil
    {
        #region Variables



        #endregion

        #region Constructors



        #endregion

        #region Methods

        public static void Resize_OnZoom(Map map)
        {
            if (map != null)
            {
                Layer ground = map.GetLayer("Ground");
                if (ground != null)
                {
                    Tile current = null;
                    foreach (Tile tile in ground.Tiles)
                    {
                        if (InputManager.MouseWithin(tile.Region.ToRectangle))
                        {
                            current = tile;
                            break;
                        }
                    }

                    if (current == null)
                    {
                        current = ground.GetTile(new Vector2(ground.Columns / 2, ground.Rows / 2));
                    }

                    if (current != null)
                    {
                        ResizeMap(map, ground, current);
                    }
                }
            }
        }

        public static void Resize_OnStart(Map map)
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
                        target = ground.GetTile(new Vector2(squad.Location.X, squad.Location.Y));
                    }

                    CameraToTile(map, ground, target);
                }
            }
        }

        public static void Resize_OnCombat(Map map)
        {
            if (map != null)
            {
                Layer ground = map.GetLayer("Ground");
                if (ground != null)
                {
                    int width = Main.Game.Resolution.X / ground.Columns;
                    int height = width;
                    int starting_y = Main.Game.Resolution.Y - (height * ground.Rows);

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
                }
            }
        }

        public static void CameraToTile(Map map, Layer ground, Tile target)
        {
            if (target != null)
            {
                target.Region.X = Main.Game.Resolution.X / 2 - (Main.Game.TileSize.X / 2);
                target.Region.Y = Main.Game.Resolution.Y / 2 - (Main.Game.TileSize.Y / 2);

                ResizeMap(map, ground, target);
            }
        }

        public static void ResizeMap(Map map, Layer ground, Tile current)
        {
            current.Region.Width = Main.Game.TileSize.X;
            current.Region.Height = Main.Game.TileSize.Y;

            foreach (Tile tile in ground.Tiles)
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

            if (Handler.LocalMap)
            {
                foreach (Army army in CharacterManager.Armies)
                {
                    foreach (Squad squad in army.Squads)
                    {
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

                            float new_speed = Get_TerrainSpeed(map, new Vector2(squad.Location.X, squad.Location.Y));
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
            }
        }

        public static void MoveGrid(World world)
        {
            if (world.Maps.Any())
            {
                int x_diff = InputManager.Mouse.X - InputManager.Mouse.lastMouseState.X;
                int y_diff = InputManager.Mouse.Y - InputManager.Mouse.lastMouseState.Y;

                Map map = world.Maps[0];
                Layer ground = map.GetLayer("Ground");
                foreach (Tile tile in ground.Tiles)
                {
                    tile.Region.X += x_diff;
                    tile.Region.Y += y_diff;
                }

                foreach (Army army in CharacterManager.Armies)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        if (squad.Visible)
                        {
                            squad.Region.X += x_diff;
                            squad.Region.Y += y_diff;
                        }
                    }
                }

                Weather weather = WeatherManager.GetWeather(WeatherManager.CurrentWeather);
                if (weather == null)
                {
                    weather = WeatherManager.GetWeather_TransitioningTo();
                }

                if (weather != null)
                {
                    if (weather.ParticleManager.Particles.Any())
                    {
                        foreach (Particle particle in weather.ParticleManager.Particles)
                        {
                            particle.Location.X += x_diff;
                            particle.Location.Y += y_diff;
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

        public static void AnimateTiles()
        {
            Scene scene = SceneManager.GetScene("Worldmap");
            if (Handler.LocalMap)
            {
                scene = SceneManager.GetScene("Localmap");
            }
            else if (Handler.Combat)
            {
                scene = SceneManager.GetScene("Combat");
            }

            Map map = null;
            if (scene.World.Maps.Any())
            {
                map = scene.World.Maps[0];
            }
            
            if (map != null)
            {
                Layer ground = map.GetLayer("Ground");

                foreach (Tile existing in ground.Tiles)
                {
                    if (existing.Type == "Water")
                    {
                        existing.Animate();
                    }
                }
            }
        }

        public static Tile Get_Tile(Layer layer, Vector2 location)
        {
            foreach (Tile tile in layer.Tiles)
            {
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
                select.Region = destination.Region;
                select.Visible = true;

                Squad target = ArmyUtil.Get_TargetSquad(squad.GetLeader().Target_ID);
                if (target != null)
                {
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
                            texture = AssetManager.Textures["Path_NS"];
                            break;

                        case Direction.East:
                            texture = AssetManager.Textures["Path_WE"];
                            break;

                        case Direction.South:
                            texture = AssetManager.Textures["Path_NS"];
                            break;

                        case Direction.West:
                            texture = AssetManager.Textures["Path_WE"];
                            break;

                        case Direction.NorthEast:
                            if (previous_direction == Direction.North)
                            {
                                texture = AssetManager.Textures["Path_SE"];
                            }
                            else if (previous_direction == Direction.East)
                            {
                                texture = AssetManager.Textures["Path_NW"];
                            }
                            break;

                        case Direction.NorthWest:
                            if (previous_direction == Direction.North)
                            {
                                texture = AssetManager.Textures["Path_SW"];
                            }
                            else if (previous_direction == Direction.West)
                            {
                                texture = AssetManager.Textures["Path_NE"];
                            }
                            break;

                        case Direction.SouthEast:
                            if (previous_direction == Direction.South)
                            {
                                texture = AssetManager.Textures["Path_NE"];
                            }
                            else if (previous_direction == Direction.East)
                            {
                                texture = AssetManager.Textures["Path_SW"];
                            }
                            break;

                        case Direction.SouthWest:
                            if (previous_direction == Direction.South)
                            {
                                texture = AssetManager.Textures["Path_NW"];
                            }
                            else if (previous_direction == Direction.West)
                            {
                                texture = AssetManager.Textures["Path_SE"];
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
                            texture = AssetManager.Textures["Path_N"];
                            break;

                        case Direction.East:
                            texture = AssetManager.Textures["Path_E"];
                            break;

                        case Direction.South:
                            texture = AssetManager.Textures["Path_S"];
                            break;

                        case Direction.West:
                            texture = AssetManager.Textures["Path_W"];
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
                            texture = AssetManager.Textures["Path_S"];
                            break;

                        case Direction.East:
                            texture = AssetManager.Textures["Path_W"];
                            break;

                        case Direction.South:
                            texture = AssetManager.Textures["Path_N"];
                            break;

                        case Direction.West:
                            texture = AssetManager.Textures["Path_E"];
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

        public static void DeselectToken(Map map, Menu menu)
        {
            Handler.Selected_Token = -1;

            menu.GetPicture("Select").Visible = false;
            menu.GetPicture("Highlight").Visible = true;
            menu.GetPicture("Highlight").DrawColor = new Color(255, 255, 255, 255);

            if (map != null)
            {
                Layer pathing = map.GetLayer("Pathing");
                pathing.Visible = false;
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

        public static string Get_Terrain(Map map, Vector2 location)
        {
            Layer ground = map.GetLayer("Ground");

            Tile tile = ground.GetTile(location);
            if (tile != null)
            {
                return tile.Type;
            }

            return null;
        }

        public static float Get_TerrainSpeed(Map map, Vector2 location)
        {
            float speed = 0;

            string terrain = Get_Terrain(map, location);
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
            }

            Layer roads = map.GetLayer("Roads");
            if (roads != null)
            {
                Tile road = roads.GetTile(new Vector3(location.X, location.Y, 0));
                if (road != null)
                {
                    speed = (float)Main.Game.TileSize.X / 12; //4
                }
            }

            return speed / 20;
        }

        public static Tile Get_Base(Map map, string type)
        {
            Layer locations = map.GetLayer("Locations");
            Tile base_location = locations.GetTile(type + " Base");
            if (base_location != null)
            {
                return base_location;
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
                    Map map = scene.World.Maps[0];
                    if (map != null)
                    {
                        Layer locations = map.GetLayer("Locations");
                        if (locations != null)
                        {
                            foreach (Tile location in locations.Tiles)
                            {
                                if (squad.Location.X == location.Location.X &&
                                    squad.Location.Y == location.Location.Y)
                                {
                                    return location;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static void AllyToken_Start(Squad squad, Map map)
        {
            Tile ally_base = Get_Base(map, "Ally");
            if (ally_base != null)
            {
                squad.Location = new Location(ally_base.Location.X, ally_base.Location.Y, ally_base.Location.Z);
                squad.Region = new Region(ally_base.Region.X, ally_base.Region.Y, ally_base.Region.Width, ally_base.Region.Height);
                squad.Visible = true;
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
            }
        }

        public static Squad CheckSquadCollision(Squad squad)
        {
            if (squad.Type == "Ally")
            {
                Army enemy_army = CharacterManager.GetArmy("Enemy");
                foreach (Squad enemy_squad in enemy_army.Squads)
                {
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
                foreach (Squad ally_squad in ally_army.Squads)
                {
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
            Scene localmap = SceneManager.GetScene("Localmap");
            if (localmap.World.Maps.Any())
            {
                Map map = localmap.World.Maps[0];
                Layer ground = map.GetLayer("Ground");

                foreach (Army army in CharacterManager.Armies)
                {
                    foreach (Squad squad in army.Squads)
                    {
                        if (squad.Path.Any())
                        {
                            ALocation path = squad.Path[0];

                            Vector2 path_location = new Vector2(path.X, path.Y);
                            Vector2 squad_location = new Vector2(squad.Location.X, squad.Location.Y);

                            if (squad_location.X < path_location.X)
                            {
                                squad.Direction = Direction.East;
                            }
                            else if (squad_location.X > path_location.X)
                            {
                                squad.Direction = Direction.West;
                            }
                            else if (squad_location.Y < path_location.Y)
                            {
                                squad.Direction = Direction.South;
                            }
                            else if (squad_location.Y > path_location.Y)
                            {
                                squad.Direction = Direction.North;
                            }

                            Tile destination = ground.GetTile(path_location);
                            Tile location = ground.GetTile(squad_location);

                            squad.Move_TotalDistance = Main.Game.TileSize.X;
                            squad.Moving = true;
                            squad.Speed = Get_TerrainSpeed(map, squad_location);
                            squad.Destination = new Vector3(path_location.X, path_location.Y, 0);
                            squad.Update();

                            Squad other_squad = CheckSquadCollision(squad);
                            if (other_squad != null)
                            {
                                squad.Region = new Region(location.Region.X, location.Region.Y, location.Region.Width, location.Region.Height);
                                GameUtil.StartCombat(map, ground, destination, squad, other_squad);
                            }

                            if (squad.Location.X == squad.Destination.X &&
                                squad.Location.Y == squad.Destination.Y)
                            {
                                squad.Region = new Region(destination.Region.X, destination.Region.Y, destination.Region.Width, destination.Region.Height);
                                squad.Path.Remove(path);

                                //Chase targeted squad
                                Squad target = ArmyUtil.Get_TargetSquad(squad.GetLeader().Target_ID);
                                if (target != null)
                                {
                                    Tile target_location = ground.GetTile(new Vector2(target.Location.X, target.Location.Y));
                                    if (target_location != null)
                                    {
                                        ArmyUtil.SetPath(MenuManager.GetMenu("UI"), map, target_location);
                                    }
                                }

                                //Check if landing at location
                                if (!squad.Path.Any())
                                {
                                    Layer locations = map.GetLayer("Locations");
                                    Tile location_tile = locations.GetTile(squad.Destination);
                                    if (location_tile != null)
                                    {
                                        GameUtil.Alert_Location(squad, location_tile);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
