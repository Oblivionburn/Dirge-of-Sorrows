using System.Collections.Generic;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public class oPathing : Pathing
    {
        public List<ALocation> Get_Path(Layer ground, Layer roads, Squad squad, Tile tile, int max_distance, bool stop_next_to_tile)
        {
            List<ALocation> result = new List<ALocation>();

            List<ALocation> path = new List<ALocation>();
            ALocation target = new ALocation((int)tile.Location.X, (int)tile.Location.Y);

            ALocation start = new ALocation((int)squad.Location.X, (int)squad.Location.Y);
            List<ALocation> open = new List<ALocation>();
            path.Add(start);
            ALocation last_min = start;

            bool reached = false;
            for (int i = 0; i < max_distance; i++)
            {
                if (last_min != null)
                {
                    List<ALocation> locations = GetLocations(ground, roads, last_min, tile);
                    foreach (ALocation location in locations)
                    {
                        if (!HasLocation(path, location))
                        {
                            location.Distance_ToStart = GetDistance(new Vector2(location.X, location.Y), new Vector2(start.X, start.Y));
                            location.Distance_ToDestination = GetDistance(new Vector2(location.X, location.Y), new Vector2(target.X, target.Y));
                            location.Parent = last_min;
                            open.Add(location);
                        }
                    }

                    if (open.Count > 0)
                    {
                        ALocation min = Get_MinLocation_Target(open, target, last_min);
                        open.Clear();
                        path.Add(min);
                        last_min = min;

                        if (DestinationReached(min, tile, stop_next_to_tile))
                        {
                            reached = true;
                            break;
                        }
                    }
                    else
                    {
                        last_min = last_min.Parent;
                    }
                }
                else
                {
                    break;
                }
            }

            if (reached)
            {
                result = Optimize_Path(path, start);
            }

            return result;
        }

        public override List<ALocation> Optimize_Path(List<ALocation> possible, ALocation start)
        {
            List<ALocation> path = new List<ALocation>();

            ALocation min = possible[possible.Count - 1];
            List<ALocation> open = new List<ALocation>();
            path.Add(min);
            ALocation last_min = min;

            bool reached = false;
            int path_max = possible.Count;
            for (int i = 0; i < path_max; i++)
            {
                List<ALocation> locations = Get_ClosedLocations(possible, last_min);
                foreach (ALocation location in locations)
                {
                    if (!HasLocation(path, location))
                    {
                        open.Add(location);
                    }
                }

                if (open.Count > 0)
                {
                    min = Get_MinLocation_Start(open);
                    open.Clear();
                    possible = Path_RemoveTile(possible, min);
                    path.Add(min);
                    last_min = min;

                    if (min.X == start.X &&
                        min.Y == start.Y)
                    {
                        reached = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            if (reached)
            {
                return path;
            }

            return null;
        }

        public List<ALocation> GetLocations(Layer ground, Layer roads, ALocation location, Tile tile)
        {
            List<ALocation> locations = new List<ALocation>();

            ALocation North = new ALocation(location.X, location.Y - 1);
            ALocation East = new ALocation(location.X + 1, location.Y);
            ALocation South = new ALocation(location.X, location.Y + 1);
            ALocation West = new ALocation(location.X - 1, location.Y);

            int x_diff = (int)tile.Location.X - location.X;
            if (x_diff < 0)
            {
                x_diff *= -1;
            }

            int y_diff = (int)tile.Location.Y - location.Y;
            if (y_diff < 0)
            {
                y_diff *= -1;
            }

            if (tile.Location.X < location.X)
            {
                if (tile.Location.Y <= location.Y)
                {
                    if (x_diff >= y_diff)
                    {
                        if (Walkable(ground, West))
                        {
                            SetPriority(ground, roads, West);
                            locations.Add(West);
                        }

                        if (Walkable(ground, North))
                        {
                            SetPriority(ground, roads, North);
                            locations.Add(North);
                        }
                    }
                    else
                    {
                        if (Walkable(ground, North))
                        {
                            SetPriority(ground, roads, North);
                            locations.Add(North);
                        }

                        if (Walkable(ground, West))
                        {
                            SetPriority(ground, roads, West);
                            locations.Add(West);
                        }
                    }

                    if (Walkable(ground, South))
                    {
                        SetPriority(ground, roads, South);
                        locations.Add(South);
                    }
                }
                else if (tile.Location.Y > location.Y)
                {
                    if (x_diff >= y_diff)
                    {
                        if (Walkable(ground, West))
                        {
                            SetPriority(ground, roads, West);
                            locations.Add(West);
                        }

                        if (Walkable(ground, South))
                        {
                            SetPriority(ground, roads, South);
                            locations.Add(South);
                        }
                    }
                    else
                    {
                        if (Walkable(ground, South))
                        {
                            SetPriority(ground, roads, South);
                            locations.Add(South);
                        }

                        if (Walkable(ground, West))
                        {
                            SetPriority(ground, roads, West);
                            locations.Add(West);
                        }
                    }

                    if (Walkable(ground, North))
                    {
                        SetPriority(ground, roads, North);
                        locations.Add(North);
                    }
                }

                if (Walkable(ground, East))
                {
                    SetPriority(ground, roads, East);
                    locations.Add(East);
                }
            }
            else if (tile.Location.X > location.X)
            {
                if (tile.Location.Y <= location.Y)
                {
                    if (x_diff >= y_diff)
                    {
                        if (Walkable(ground, East))
                        {
                            SetPriority(ground, roads, East);
                            locations.Add(East);
                        }

                        if (Walkable(ground, North))
                        {
                            SetPriority(ground, roads, North);
                            locations.Add(North);
                        }
                    }
                    else
                    {
                        if (Walkable(ground, North))
                        {
                            SetPriority(ground, roads, North);
                            locations.Add(North);
                        }

                        if (Walkable(ground, East))
                        {
                            SetPriority(ground, roads, East);
                            locations.Add(East);
                        }
                    }

                    if (Walkable(ground, South))
                    {
                        SetPriority(ground, roads, South);
                        locations.Add(South);
                    }
                }
                else if (tile.Location.Y > location.Y)
                {
                    if (x_diff >= y_diff)
                    {
                        if (Walkable(ground, East))
                        {
                            SetPriority(ground, roads, East);
                            locations.Add(East);
                        }

                        if (Walkable(ground, South))
                        {
                            SetPriority(ground, roads, South);
                            locations.Add(South);
                        }
                    }
                    else
                    {
                        if (Walkable(ground, South))
                        {
                            SetPriority(ground, roads, South);
                            locations.Add(South);
                        }

                        if (Walkable(ground, East))
                        {
                            SetPriority(ground, roads, East);
                            locations.Add(East);
                        }
                    }

                    if (Walkable(ground, North))
                    {
                        SetPriority(ground, roads, North);
                        locations.Add(North);
                    }
                }

                if (Walkable(ground, West))
                {
                    SetPriority(ground, roads, West);
                    locations.Add(West);
                }
            }
            else if (tile.Location.X == location.X)
            {
                if (tile.Location.Y < location.Y)
                {
                    if (Walkable(ground, North))
                    {
                        SetPriority(ground, roads, North);
                        locations.Add(North);
                    }

                    if (Walkable(ground, West))
                    {
                        SetPriority(ground, roads, West);
                        locations.Add(West);
                    }

                    if (Walkable(ground, East))
                    {
                        SetPriority(ground, roads, East);
                        locations.Add(East);
                    }

                    if (Walkable(ground, South))
                    {
                        SetPriority(ground, roads, South);
                        locations.Add(South);
                    }
                }
                else if (tile.Location.Y > location.Y)
                {
                    if (Walkable(ground, South))
                    {
                        SetPriority(ground, roads, South);
                        locations.Add(South);
                    }

                    if (Walkable(ground, West))
                    {
                        SetPriority(ground, roads, West);
                        locations.Add(West);
                    }

                    if (Walkable(ground, East))
                    {
                        SetPriority(ground, roads, East);
                        locations.Add(East);
                    }

                    if (Walkable(ground, North))
                    {
                        SetPriority(ground, roads, North);
                        locations.Add(North);
                    }
                }
            }

            return locations;
        }

        public override ALocation Get_MinLocation_Target(List<ALocation> locations, ALocation target, ALocation previous)
        {
            ALocation current = locations[0];
            if (current.Distance_ToDestination == 0)
            {
                return current;
            }

            float current_near = current.Distance_ToDestination + current.Priority;
            float current_far = current.Distance_ToStart + current.Priority;

            foreach (ALocation location in locations)
            {
                if (location.Distance_ToDestination == 0)
                {
                    return location;
                }

                float pref_near = location.Distance_ToDestination + location.Priority;
                float pref_far = location.Distance_ToStart + location.Priority;

                if ((pref_near <= current_near && pref_far > current_far) ||
                    pref_near < current_near)
                {
                    current = location;
                    current_near = pref_near;
                    current_far = pref_far;
                }
            }

            return current;
        }

        public override ALocation Get_MinLocation_Start(List<ALocation> locations)
        {
            ALocation current = locations[0];
            if (current.Distance_ToStart == 0)
            {
                return current;
            }

            float current_far = current.Distance_ToStart + current.Priority;

            foreach (ALocation location in locations)
            {
                if (location.Distance_ToStart == 0)
                {
                    return location;
                }

                float pref_far = location.Distance_ToStart + location.Priority;

                if (pref_far < current_far)
                {
                    current = location;
                    current_far = pref_far;
                }
            }

            return current;
        }

        public override bool Walkable(Layer ground, ALocation location)
        {
            Tile tile = ground.GetTile(new Vector3(location.X, location.Y, 0));
            if (tile != null)
            {
                if (tile.BlocksMovement)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public void SetPriority(Layer ground, Layer roads, ALocation location)
        {
            Tile road = roads.GetTile(new Vector3(location.X, location.Y, 0));
            if (road != null)
            {
                location.Priority = -0.75f;
            }
            else
            {
                Tile ground_tile = ground.GetTile(new Vector2(location.X, location.Y));
                if (ground_tile != null)
                {
                    location.Priority = GetPriority(ground_tile.Type);
                }
            }
        }

        public float GetPriority(string type)
        {
            float priority = 0;

            switch (type)
            {
                case "Grass":
                case "Desert":
                case "Snow":
                case "Ice":
                    priority = 1f;
                    break;

                case "Forest":
                case "Forest_Snow":
                    priority = 1.1f;
                    break;

                case "Water":
                    priority = 1.2f;
                    break;

                case "Mountains":
                case "Mountains_Desert":
                case "Mountains_Snow":
                    priority = 1.25f;
                    break;
            }

            return priority;
        }
    }
}
