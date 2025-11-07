using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Scenes;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class WorldGen
    {
        #region Variables

        private static CryptoRandom random;
        private static readonly int scale = 2;

        #endregion

        #region Constructors



        #endregion

        #region Methods

        public static void GenWorldmap()
        {
            World world = new World
            {
                ID = Handler.GetID(),
                Visible = true,
                DrawColor = Color.White
            };

            Map world_map = NewMap(world, "World Map", true);
            world.Maps.Add(world_map);

            Layer world_ground = NewLayer(world_map, "Ground", 0);
            GenGround(world_ground, null, false);

            ShapeMap(world, world_ground, null, false);
            world_map.Layers.Add(world_ground);

            Layer world_locations = NewLayer(world_map, "Locations", 0);
            GenLocations(world, world_ground, world_locations, 0, false);
            world_map.Layers.Add(world_locations);

            Layer world_pathing = NewLayer(world_map, "Roads", 0);
            world_map.Layers.Add(world_pathing);

            AlignRegions(world_map);

            //Hide all locations after the first
            for (int i = 0; i < world_locations.Tiles.Count; i++)
            {
                Tile tile = world_locations.Tiles[i];

                random = new CryptoRandom();
                tile.Name = world.Names[random.Next(0, world.Names.Count)];

                if (i > 0)
                {
                    tile.Visible = false;
                }
            }

            SceneManager.GetScene("Worldmap").World = world;
        }

        public static Map GenLocalmap(World world, Tile tile, int depth)
        {
            Map local_map = NewMap(world, tile.Name, true);
            local_map.ID = tile.ID;
            local_map.Type = tile.Type;

            Layer local_ground = NewLayer(local_map, "Ground", depth);
            GenGround(local_ground, tile.Type, true);

            ShapeMap(world, local_ground, tile.Type, true);
            local_map.Layers.Add(local_ground);

            Layer local_locations = NewLayer(local_map, "Locations", depth);
            GenLocations(world, local_ground, local_locations, depth, true);
            local_map.Layers.Add(local_locations);

            Layer local_roads = NewLayer(local_map, "Roads", depth);
            GenRoads(local_ground, local_locations, local_roads);
            local_map.Layers.Add(local_roads);

            Layer local_pathing = NewLayer(local_map, "Pathing", depth);
            local_map.Layers.Add(local_pathing);

            AlignRegions(local_map);

            return local_map;
        }

        public static void GenCombatMap()
        {
            World world = new World
            {
                ID = Handler.GetID(),
                Visible = true,
                DrawColor = Color.White
            };

            Map combat_map = NewMap(world, Handler.Combat_Terrain, true);
            combat_map.Type = Handler.Combat_Terrain;
            world.Maps.Add(combat_map);

            Layer combat_ground = NewCombatLayer(combat_map, "Ground");
            GenCombatGround(combat_ground, Handler.Combat_Terrain);
            combat_map.Layers.Add(combat_ground);

            SceneManager.GetScene("Combat").World = world;
        }

        public static Map NewMap(World world, string name, bool visible)
        {
            return new Map
            {
                ID = Handler.GetID(),
                WorldID = world.ID,
                Visible = visible,
                DrawColor = Color.White,
                Name = name
            };
        }

        public static Layer NewLayer(Map map, string name, int depth)
        {
            return new Layer
            {
                ID = Handler.GetID(),
                WorldID = map.WorldID,
                MapID = map.ID,
                Visible = true,
                DrawColor = Color.White,
                Name = name,
                Rows = 18 + (depth * scale),
                Columns = 30 + (depth * scale)
            };
        }

        public static Layer NewCombatLayer(Map map, string name)
        {
            return new Layer
            {
                ID = Handler.GetID(),
                WorldID = map.WorldID,
                MapID = map.ID,
                Visible = true,
                DrawColor = Color.White,
                Name = name,
                Rows = 5,
                Columns = 11
            };
        }

        public static void GenGround(Layer layer, string type, bool local)
        {
            int width = Main.Game.TileSize.X;
            int height = Main.Game.TileSize.Y;

            for (int y = 0; y < layer.Rows; y++)
            {
                for (int x = 0; x < layer.Columns; x++)
                {
                    Region region = new Region(x * width, y * height, width, height);

                    if (!local)
                    {
                        if (y < 3 || y > layer.Rows - 4)
                        {
                            layer.Tiles.Add(NewTile(layer, "Snow", new Vector2(x, y), region));
                        }
                        else
                        {
                            layer.Tiles.Add(NewTile(layer, "Grass", new Vector2(x, y), region));
                        }
                    }
                    else
                    {
                        layer.Tiles.Add(NewTile(layer, type, new Vector2(x, y), region));
                    }
                }
            }
        }

        public static void GenCombatGround(Layer layer, string type)
        {
            float starting_y = ((float)Main.Game.Resolution.Y / 8) * 5;
            float width = (float)Main.Game.Resolution.X / layer.Columns;
            float height = (Main.Game.Resolution.Y - starting_y) / layer.Rows;

            for (int y = 0; y < layer.Rows; y++)
            {
                for (int x = 0; x < layer.Columns; x++)
                {
                    layer.Tiles.Add(new Tile
                    {
                        ID = Handler.GetID(),
                        WorldID = layer.WorldID,
                        MapID = layer.MapID,
                        LayerID = layer.ID,
                        Visible = false,
                        Location = new Location(x, y, 0),
                        Region = new Region(x * width, y * height, width, height)
                    });
                }
            }
        }

        public static void GenLocations(World world, Layer ground, Layer layer, int count, bool local)
        {
            if (!local)
            {
                ScatterLocations(ground, layer, "Base_Enemy", 20);
            }
            else
            {
                ScatterLocations(ground, layer, "Base_Ally", 1);
                Add_EnemyBase(ground, layer);
                ScatterLocations(ground, layer, "Town_Neutral", count);

                ScatterLocations(ground, layer, "Market_Neutral", 1);
                ScatterLocations(ground, layer, "Academy_Neutral", 1);

                foreach (Tile location in layer.Tiles)
                {
                    random = new CryptoRandom();
                    location.Name = world.Names[random.Next(0, world.Names.Count)];
                }
            }
        }

        public static void GenRoads(Layer ground, Layer locations, Layer roads)
        {
            for (int i = 0; i < locations.Tiles.Count - 1; i++)
            {
                GenRoad(ground, roads, locations.Tiles[i], locations.Tiles[i + 1], 0);
            }

            foreach (Tile road in roads.Tiles)
            {
                Tile location = WorldUtil.Get_Tile(locations, new Vector2(road.Location.X, road.Location.Y));
                if (location != null)
                {
                    road.Visible = false;
                }
            }
        }

        public static Tile NewTile(Layer layer, string type, Vector2 location, Region region)
        {
            Texture2D texture = AssetManager.Textures["Tile_" + type];

            Tile tile = new Tile
            {
                ID = Handler.GetID(),
                WorldID = layer.WorldID,
                MapID = layer.MapID,
                LayerID = layer.ID,
                Visible = true,
                Name = type,
                Type = type,
                Texture = texture,
                Location = new Location(location.X, location.Y, 0),
                Region = new Region(region.X, region.Y, region.Width, region.Height)
            };

            if (type == "Water")
            {
                tile.Image = new Rectangle(0, 0, texture.Width / 4, texture.Height);
            }
            else
            {
                tile.Image = new Rectangle(0, 0, texture.Width, texture.Height);
            }

            return tile;
        }

        public static void ShapeMap(World world, Layer layer, string type, bool local)
        {
            if (!local)
            {
                int deserts = 2;
                int rivers = 4;
                int lakes = 1;
                int forests = 8;
                int mountains = 6;

                GenDeserts(layer, deserts);
                GenRivers(layer, rivers);
                GenLakes(layer, lakes);
                GenForests(layer, forests);
                GenMountains(layer, mountains);
            }
            else
            {
                if (type == "Grass")
                {
                    random = new CryptoRandom();
                    GenRivers(layer, random.Next(2, 5));

                    random = new CryptoRandom();
                    GenLakes(layer, random.Next(0, 2));

                    random = new CryptoRandom();
                    GenForests(layer, random.Next(2, 7));
                }
                else if (type == "Desert")
                {
                    random = new CryptoRandom();
                    GenMountains(layer, random.Next(5, 13));
                }
                else if (type == "Water" ||
                         type == "Ice")
                {
                    GenIslands(layer, world.Maps.Count + 2);

                    random = new CryptoRandom();
                    GenForests(layer, random.Next(1, world.Maps.Count + 1));
                }
                else if (type == "Snow")
                {
                    random = new CryptoRandom();
                    GenRivers(layer, random.Next(2, 5));

                    random = new CryptoRandom();
                    GenLakes(layer, random.Next(0, 2));

                    random = new CryptoRandom();
                    GenForests(layer, random.Next(2, 7));
                }
                else if (type.Contains("Forest"))
                {
                    random = new CryptoRandom();
                    GenIslands(layer, random.Next(world.Maps.Count + 2, world.Maps.Count + 7));

                    random = new CryptoRandom();
                    GenRivers(layer, random.Next(0, 5));
                }
                else if (type.Contains("Mountains"))
                {
                    GenIslands(layer, world.Maps.Count + 2);

                    random = new CryptoRandom();
                    GenForests(layer, random.Next(1, world.Maps.Count + 1));

                    random = new CryptoRandom();
                    GenRivers(layer, random.Next(2, 5));

                    random = new CryptoRandom();
                    GenLakes(layer, random.Next(0, 2));
                }
            }
        }

        public static void GenIslands(Layer layer, int count)
        {
            CryptoRandom random;

            for (int i = 0; i < layer.Columns * layer.Rows; i++)
            {
                random = new CryptoRandom();
                int x = random.Next(0, layer.Columns);

                random = new CryptoRandom();
                int y = random.Next(0, layer.Rows);

                Vector2 location = new Vector2(x, y);
                Tile tile = layer.GetTile(location);
                if (tile != null)
                {
                    bool found = false;
                    if (tile.Type != "Ice" &&
                        !tile.Type.Contains("Snow"))
                    {
                        found = true;
                        random = new CryptoRandom();
                        Spread(layer, location, "Grass", random.Next(2, 7));
                    }
                    else if (tile.Type == "Ice" ||
                             tile.Type.Contains("Snow"))
                    {
                        found = true;
                        random = new CryptoRandom();
                        Spread(layer, location, "Snow", random.Next(2, 7));
                    }

                    if (found)
                    {
                        count--;
                        if (count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void GenDeserts(Layer layer, int count)
        {
            CryptoRandom random;

            for (int i = 0; i < layer.Columns * layer.Rows; i++)
            {
                random = new CryptoRandom();
                int x = random.Next(0, layer.Columns);

                random = new CryptoRandom();
                int y = random.Next(0, layer.Rows);

                Vector2 location = new Vector2(x, y);
                Tile tile = layer.GetTile(location);
                if (tile != null)
                {
                    if (tile.Type == "Grass")
                    {
                        random = new CryptoRandom();
                        Spread(layer, location, "Desert", random.Next(2, 7));

                        count--;
                        if (count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void GenRivers(Layer layer, int count)
        {
            CryptoRandom random;

            for (int i = 0; i < layer.Columns * layer.Rows; i++)
            {
                random = new CryptoRandom();

                int x = random.Next(0, layer.Columns);
                int y = random.Next(0, layer.Rows);

                Vector2 location = new Vector2(x, y);
                Tile tile = layer.GetTile(location);
                if (tile != null)
                {
                    if (tile.Type == "Grass" ||
                        tile.Type == "Snow")
                    {
                        Snake(layer, location, "Water", 20);

                        count--;
                        if (count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void GenLakes(Layer layer, int count)
        {
            CryptoRandom random;

            for (int i = 0; i < layer.Columns * layer.Rows; i++)
            {
                random = new CryptoRandom();
                int x = random.Next(0, layer.Columns);

                random = new CryptoRandom();
                int y = random.Next(0, layer.Rows);

                Vector2 location = new Vector2(x, y);
                Tile tile = layer.GetTile(location);
                if (tile != null)
                {
                    random = new CryptoRandom();
                    Spread(layer, location, "Water", random.Next(2, 7));

                    count--;
                    if (count == 0)
                    {
                        break;
                    }
                }
            }
        }

        public static void GenForests(Layer layer, int count)
        {
            CryptoRandom random;

            for (int i = 0; i < layer.Columns * layer.Rows; i++)
            {
                random = new CryptoRandom();
                int x = random.Next(0, layer.Columns);

                random = new CryptoRandom();
                int y = random.Next(0, layer.Rows);

                Vector2 location = new Vector2(x, y);
                Tile tile = layer.GetTile(location);
                if (tile != null)
                {
                    if (tile.Type == "Grass" ||
                        tile.Type == "Snow")
                    {
                        random = new CryptoRandom();
                        Spread(layer, location, "Forest", random.Next(2, 6));

                        count--;
                        if (count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void GenMountains(Layer layer, int count)
        {
            CryptoRandom random;

            for (int i = 0; i < layer.Columns * layer.Rows; i++)
            {
                random = new CryptoRandom();

                int x = random.Next(0, layer.Columns);
                int y = random.Next(0, layer.Rows);

                Vector2 location = new Vector2(x, y);
                Tile tile = layer.GetTile(location);
                if (tile != null)
                {
                    if (tile.Type == "Grass" ||
                        tile.Type == "Snow" ||
                        tile.Type == "Desert")
                    {
                        Snake(layer, location, "Mountains", 20);

                        count--;
                        if (count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void Snake(Layer layer, Vector2 location, string type, int distance)
        {
            Tile tile = layer.GetTile(location);
            if (tile != null && 
                distance > 0)
            {
                bool okay = false;
                if (type.Contains("Mountains"))
                {
                    tile.Name = "Mountains";

                    if (tile.Type == "Grass")
                    {
                        okay = true;
                        type = "Mountains";
                    }
                    else if (tile.Type == "Snow")
                    {
                        okay = true;
                        type = "Mountains_Snow";
                    }
                    else if (tile.Type == "Desert")
                    {
                        okay = true;
                        type = "Mountains_Desert";
                    }
                }
                else if (type.Contains("Forest"))
                {
                    tile.Name = "Forest";

                    if (tile.Type == "Grass")
                    {
                        okay = true;
                        type = "Forest";
                    }
                    else if (tile.Type == "Snow")
                    {
                        okay = true;
                        type = "Forest_Snow";
                    }
                }
                else if (type == "Water" ||
                         type == "Ice")
                {
                    if (tile.Type == "Grass")
                    {
                        tile.Name = "Water";
                        okay = true;
                        type = "Water";
                    }
                    else if (tile.Type == "Snow")
                    {
                        tile.Name = "Ice";
                        okay = true;
                        type = "Ice";
                    }
                }
                else
                {
                    tile.Name = type;
                    okay = true;
                }

                if (okay)
                {
                    tile.Type = type;
                    tile.Texture = AssetManager.Textures["Tile_" + type];

                    if (type == "Water")
                    {
                        tile.Image = new Rectangle(0, 0, tile.Texture.Width / 4, tile.Texture.Height);
                    }
                    else
                    {
                        tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                    }

                    distance--;

                    for (int i = 0; i < 100; i++)
                    {
                        Tile next;
                        Vector2 next_location = default(Vector2);

                        random = new CryptoRandom();
                        int direction = random.Next(1, 5);
                        if (direction == 1)
                        {
                            //North
                            next_location = new Vector2(location.X, location.Y - 1);
                        }
                        else if (direction == 2)
                        {
                            //East
                            next_location = new Vector2(location.X + 1, location.Y);
                        }
                        else if (direction == 3)
                        {
                            //South
                            next_location = new Vector2(location.X, location.Y + 1);
                        }
                        else if (direction == 4)
                        {
                            //West
                            next_location = new Vector2(location.X - 1, location.Y);
                        }

                        next = layer.GetTile(next_location);
                        if (next != null)
                        {
                            Snake(layer, next_location, type, distance);
                            break;
                        }
                    }
                }
            }
        }

        public static void Spread(Layer layer, Vector2 location, string type, int distance)
        {
            Tile tile = layer.GetTile(location);
            if (tile != null && 
                distance > 0)
            {
                bool okay = false;
                if (type == "Desert")
                {
                    if (tile.Type == "Grass")
                    {
                        tile.Name = type;
                        okay = true;
                    }
                }
                else if (type == "Water" ||
                         type == "Ice")
                {
                    if (tile.Type == "Grass")
                    {
                        tile.Name = "Water";
                        okay = true;
                        type = "Water";
                    }
                    else if (tile.Type == "Snow")
                    {
                        tile.Name = "Ice";
                        okay = true;
                        type = "Ice";
                    }
                }
                else if (type == "Forest" ||
                         type == "Forest_Snow")
                {
                    tile.Name = "Forest";

                    if (tile.Type == "Grass")
                    {
                        okay = true;
                        type = "Forest";
                    }
                    else if (tile.Type == "Snow")
                    {
                        okay = true;
                        type = "Forest_Snow";
                    }
                }
                else if (type == "Mountains" ||
                         type == "Mountains_Snow" ||
                         type == "Mountains_Desert")
                {
                    tile.Name = "Mountains";

                    if (tile.Type == "Grass")
                    {
                        okay = true;
                        type = "Mountains";
                    }
                    else if (tile.Type == "Snow")
                    {
                        okay = true;
                        type = "Mountains_Snow";
                    }
                    else if (tile.Type == "Desert")
                    {
                        okay = true;
                        type = "Mountains_Desert";
                    }
                }
                else if (type == "Oasis")
                {
                    tile.Name = "Water";
                    okay = true;
                }
                else
                {
                    tile.Name = type;
                    okay = true;
                }

                if (okay)
                {
                    if (type == "Oasis")
                    {
                        tile.Type = "Water";
                        tile.Texture = AssetManager.Textures["Tile_Water"];
                        tile.Image = new Rectangle(0, 0, tile.Texture.Width / 4, tile.Texture.Height);
                        type = "Grass";
                    }
                    else
                    {
                        tile.Type = type;
                        tile.Texture = AssetManager.Textures["Tile_" + type];

                        if (type == "Water")
                        {
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width / 4, tile.Texture.Height);
                        }
                        else
                        {
                            tile.Image = new Rectangle(0, 0, tile.Texture.Width, tile.Texture.Height);
                        }
                    }

                    distance--;

                    Spread(layer, new Vector2(location.X, location.Y - 1), type, distance);
                    Spread(layer, new Vector2(location.X + 1, location.Y), type, distance);
                    Spread(layer, new Vector2(location.X, location.Y + 1), type, distance);
                    Spread(layer, new Vector2(location.X - 1, location.Y), type, distance);
                }
            }
        }

        public static void ScatterLocations(Layer ground, Layer layer, string type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < ground.Columns * ground.Rows; j++)
                {
                    random = new CryptoRandom();
                    int x = random.Next(0, ground.Columns);

                    random = new CryptoRandom();
                    int y = random.Next(0, ground.Rows);

                    Vector2 location = new Vector2(x, y);

                    Tile ground_tile = ground.GetTile(location);
                    if (ground_tile != null)
                    {
                        bool okay = true;
                        foreach (Tile existing in layer.Tiles)
                        {
                            int x_diff = (int)existing.Location.X - x;
                            if (x_diff < 0)
                            {
                                x_diff *= -1;
                            }

                            int y_diff = (int)existing.Location.Y - y;
                            if (y_diff < 0)
                            {
                                y_diff *= -1;
                            }

                            int distance = x_diff + y_diff;
                            if (distance <= 2)
                            {
                                okay = false;
                            }
                        }

                        if (okay)
                        {
                            layer.Tiles.Add(NewTile(layer, type, location, ground_tile.Region));
                            break;
                        }
                    }
                }
            }
        }

        public static void Add_EnemyBase(Layer ground, Layer layer)
        {
            for (int j = 0; j < ground.Columns * ground.Rows; j++)
            {
                random = new CryptoRandom();
                int x = random.Next(0, ground.Columns);

                random = new CryptoRandom();
                int y = random.Next(0, ground.Rows);

                Vector2 location = new Vector2(x, y);

                Tile ground_tile = ground.GetTile(location);
                if (ground_tile != null)
                {
                    Tile base_ally = layer.GetTile("Base_Ally");

                    int x_diff = (int)base_ally.Location.X - x;
                    if (x_diff < 0)
                    {
                        x_diff *= -1;
                    }

                    int y_diff = (int)base_ally.Location.Y - y;
                    if (y_diff < 0)
                    {
                        y_diff *= -1;
                    }

                    int distance = x_diff + y_diff;
                    if (distance >= 10)
                    {
                        if (WorldUtil.Get_Tile(layer, location) == null)
                        {
                            layer.Tiles.Add(NewTile(layer, "Base_Enemy", location, ground_tile.Region));
                            break;
                        }
                    }
                }
            }
        }

        public static void GenRoad(Layer ground, Layer roads, Tile current, Tile destination, Direction previous_direction)
        {
            random = new CryptoRandom();

            Vector2 next_coord = default;
            string road = null;

            if (current != null &&
                destination != null)
            {
                if (current.Location.X < destination.Location.X)
                {
                    if (current.Location.Y > destination.Location.Y)
                    {
                        #region NorthEast

                        int pavingTo = random.Next(1, 3);
                        if (pavingTo == 1)
                        {
                            next_coord = new Vector2(current.Location.X, current.Location.Y - 1);

                            if (previous_direction == Direction.Up)
                            {
                                road = "Road_NS";
                            }
                            else if (previous_direction == Direction.Right)
                            {
                                road = "Road_NW";
                            }
                            else if (previous_direction == Direction.Left)
                            {
                                road = "Road_NE";
                            }
                            else
                            {
                                road = "Road_NS";
                            }

                            previous_direction = Direction.Up;
                        }
                        else if (pavingTo == 2)
                        {
                            next_coord = new Vector2(current.Location.X + 1, current.Location.Y);

                            if (previous_direction == Direction.Up)
                            {
                                road = "Road_SE";
                            }
                            else if (previous_direction == Direction.Right)
                            {
                                road = "Road_WE";
                            }
                            else if (previous_direction == Direction.Down)
                            {
                                road = "Road_NE";
                            }
                            else
                            {
                                road = "Road_WE";
                            }

                            previous_direction = Direction.Right;
                        }

                        #endregion
                    }
                    else if (current.Location.Y < destination.Location.Y)
                    {
                        #region SouthEast

                        int pavingTo = random.Next(1, 3);
                        if (pavingTo == 1)
                        {
                            next_coord = new Vector2(current.Location.X, current.Location.Y + 1);

                            if (previous_direction == Direction.Right)
                            {
                                road = "Road_SW";
                            }
                            else if (previous_direction == Direction.Down)
                            {
                                road = "Road_NS";
                            }
                            else if (previous_direction == Direction.Left)
                            {
                                road = "Road_SE";
                            }
                            else
                            {
                                road = "Road_NS";
                            }

                            previous_direction = Direction.Down;
                        }
                        else if (pavingTo == 2)
                        {
                            next_coord = new Vector2(current.Location.X + 1, current.Location.Y);

                            if (previous_direction == Direction.Up)
                            {
                                road = "Road_SE";
                            }
                            else if (previous_direction == Direction.Right)
                            {
                                road = "Road_WE";
                            }
                            else if (previous_direction == Direction.Down)
                            {
                                road = "Road_NE";
                            }
                            else
                            {
                                road = "Road_WE";
                            }

                            previous_direction = Direction.Right;
                        }

                        #endregion
                    }
                    else if (current.Location.Y == destination.Location.Y)
                    {
                        #region East

                        next_coord = new Vector2(current.Location.X + 1, current.Location.Y);

                        if (previous_direction == Direction.Up)
                        {
                            road = "Road_SE";
                        }
                        else if (previous_direction == Direction.Right)
                        {
                            road = "Road_WE";
                        }
                        else if (previous_direction == Direction.Down)
                        {
                            road = "Road_NE";
                        }
                        else
                        {
                            road = "Road_WE";
                        }

                        previous_direction = Direction.Right;

                        #endregion
                    }
                }
                else if (current.Location.X > destination.Location.X)
                {
                    if (current.Location.Y > destination.Location.Y)
                    {
                        #region NorthWest

                        int pavingTo = random.Next(1, 3);
                        if (pavingTo == 1)
                        {
                            next_coord = new Vector2(current.Location.X, current.Location.Y - 1);

                            if (previous_direction == Direction.Up)
                            {
                                road = "Road_NS";
                            }
                            else if (previous_direction == Direction.Right)
                            {
                                road = "Road_NW";
                            }
                            else if (previous_direction == Direction.Left)
                            {
                                road = "Road_NE";
                            }
                            else
                            {
                                road = "Road_NS";
                            }

                            previous_direction = Direction.Up;
                        }
                        else if (pavingTo == 2)
                        {
                            next_coord = new Vector2(current.Location.X - 1, current.Location.Y);

                            if (previous_direction == Direction.Up)
                            {
                                road = "Road_SW";
                            }
                            else if (previous_direction == Direction.Down)
                            {
                                road = "Road_NW";
                            }
                            else if (previous_direction == Direction.Left)
                            {
                                road = "Road_WE";
                            }
                            else
                            {
                                road = "Road_WE";
                            }

                            previous_direction = Direction.Left;
                        }

                        #endregion
                    }
                    else if (current.Location.Y < destination.Location.Y)
                    {
                        #region SouthWest

                        int pavingTo = random.Next(1, 3);
                        if (pavingTo == 1)
                        {
                            next_coord = new Vector2(current.Location.X, current.Location.Y + 1);

                            if (previous_direction == Direction.Right)
                            {
                                road = "Road_SW";
                            }
                            else if (previous_direction == Direction.Down)
                            {
                                road = "Road_NS";
                            }
                            else if (previous_direction == Direction.Left)
                            {
                                road = "Road_SE";
                            }
                            else
                            {
                                road = "Road_NS";
                            }

                            previous_direction = Direction.Down;
                        }
                        else if (pavingTo == 2)
                        {
                            next_coord = new Vector2(current.Location.X - 1, current.Location.Y);

                            if (previous_direction == Direction.Up)
                            {
                                road = "Road_SW";
                            }
                            else if (previous_direction == Direction.Down)
                            {
                                road = "Road_NW";
                            }
                            else if (previous_direction == Direction.Left)
                            {
                                road = "Road_WE";
                            }
                            else
                            {
                                road = "Road_WE";
                            }

                            previous_direction = Direction.Left;
                        }

                        #endregion
                    }
                    else if (current.Location.Y == destination.Location.Y)
                    {
                        #region West

                        next_coord = new Vector2(current.Location.X - 1, current.Location.Y);

                        if (previous_direction == Direction.Up)
                        {
                            road = "Road_SW";
                        }
                        else if (previous_direction == Direction.Down)
                        {
                            road = "Road_NW";
                        }
                        else if (previous_direction == Direction.Left)
                        {
                            road = "Road_WE";
                        }
                        else
                        {
                            road = "Road_WE";
                        }

                        previous_direction = Direction.Left;

                        #endregion
                    }
                }
                else if (current.Location.X == destination.Location.X)
                {
                    if (current.Location.Y > destination.Location.Y)
                    {
                        #region North

                        next_coord = new Vector2(current.Location.X, current.Location.Y - 1);

                        if (previous_direction == Direction.Up)
                        {
                            road = "Road_NS";
                        }
                        else if (previous_direction == Direction.Right)
                        {
                            road = "Road_NW";
                        }
                        else if (previous_direction == Direction.Left)
                        {
                            road = "Road_NE";
                        }
                        else
                        {
                            road = "Road_NS";
                        }

                        previous_direction = Direction.Up;

                        #endregion
                    }
                    else if (current.Location.Y < destination.Location.Y)
                    {
                        #region South

                        next_coord = new Vector2(current.Location.X, current.Location.Y + 1);

                        if (previous_direction == Direction.Right)
                        {
                            road = "Road_SW";
                        }
                        else if (previous_direction == Direction.Down)
                        {
                            road = "Road_NS";
                        }
                        else if (previous_direction == Direction.Left)
                        {
                            road = "Road_SE";
                        }
                        else
                        {
                            road = "Road_NS";
                        }

                        previous_direction = Direction.Down;

                        #endregion
                    }
                    else if (current.Location.Y == destination.Location.Y)
                    {
                        #region Destination

                        if (previous_direction == Direction.Up ||
                            previous_direction == Direction.Down)
                        {
                            road = "Road_NS";
                        }
                        else if (previous_direction == Direction.Right ||
                                 previous_direction == Direction.Left)
                        {
                            road = "Road_WE";
                        }

                        #endregion
                    }
                }

                if (!string.IsNullOrEmpty(road))
                {
                    roads.Tiles.Add(NewTile(roads, road, new Vector2(current.Location.X, current.Location.Y), current.Region));
                }

                if (next_coord != default)
                {
                    Tile next_tile = ground.GetTile(next_coord);
                    if (next_tile != null)
                    {
                        GenRoad(ground, roads, next_tile, destination, previous_direction);
                    }
                }
            }
        }

        public static void AlignRegions(Map map)
        {
            Layer ground = map.GetLayer("Ground");

            foreach (Layer layer in map.Layers)
            {
                if (layer.Name != "Ground")
                {
                    foreach (Tile tile in layer.Tiles)
                    {
                        Tile ground_tile = ground.GetTile(new Vector2(tile.Location.X, tile.Location.Y));
                        if (ground_tile != null)
                        {
                            tile.Region = ground_tile.Region;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
