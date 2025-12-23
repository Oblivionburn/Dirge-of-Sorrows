using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class AI_Util
    {
        public static void Set_NextTarget(Map map, Layer ground, Army army, Squad squad)
        {
            Tile target_tile = null;

            if (squad.Assignment == "Attack Base")
            {
                target_tile = WorldUtil.Get_Base(map, "Ally");
            }
            else if (squad.Assignment == "Capture Nearest Town")
            {
                target_tile = WorldUtil.GetNearest_Town(map, ground, army, squad, false);
            }
            else if (squad.Assignment == "Guard Nearest Town")
            {
                target_tile = WorldUtil.GetNearest_Town(map, ground, army, squad, true);
            }
            else if (squad.Assignment == "Attack Nearest Squad")
            {
                Squad target_squad = WorldUtil.GetNearest_Squad(squad);
                if (target_squad != null)
                {
                    squad.GetLeader().Target_ID = target_squad.ID;
                    target_tile = ground.GetTile(new Vector2(target_squad.Location.X, target_squad.Location.Y));
                }
            }
            else if (squad.Assignment == "Attack Hero Squad")
            {
                Squad target_squad = Handler.GetHero().Squad;
                if (target_squad != null)
                {
                    squad.GetLeader().Target_ID = target_squad.ID;
                    target_tile = ground.GetTile(new Vector2(target_squad.Location.X, target_squad.Location.Y));
                }
            }
            else if (squad.Assignment == "Sleeper")
            {
                Squad target_squad = WorldUtil.GetNearest_Squad(squad);
                if (target_squad != null)
                {
                    if (WorldUtil.GetDistance(squad.Location, target_squad.Location) <= 5)
                    {
                        squad.GetLeader().Target_ID = target_squad.ID;
                        target_tile = ground.GetTile(new Vector2(target_squad.Location.X, target_squad.Location.Y));
                    }
                }
            }
            else if (squad.Assignment == "Opportunist")
            {
                Tile ally_base = WorldUtil.Get_Base(map, "Ally");

                bool guarded = false;

                Army ally_army = CharacterManager.GetArmy("Ally");
                foreach (Squad ally_squad in ally_army.Squads)
                {
                    if (ally_squad.Location.X == ally_base.Location.X &&
                        ally_squad.Location.Y == ally_base.Location.Y)
                    {
                        guarded = true;
                        break;
                    }
                }

                if (!guarded)
                {
                    squad.Assignment = "Attack Base";
                    target_tile = ally_base;
                }
            }
            else if (squad.Assignment == "Rest")
            {
                target_tile = WorldUtil.GetNearest_Location_ToRest(map, ground, squad, false);
            }

            if (target_tile == null &&
                squad.Assignment != "Sleeper" &&
                squad.Assignment != "Opportunist")
            {
                squad.Assignment = "Attack Base";
                target_tile = WorldUtil.Get_Base(map, "Ally");
            }

            if (target_tile != null)
            {
                bool already_there = false;
                if (squad.Location.X == target_tile.Location.X &&
                    squad.Location.Y == target_tile.Location.Y)
                {
                    already_there = true;
                }

                if (!already_there)
                {
                    ArmyUtil.SetPath(map, squad, target_tile);
                }
            }
        }

        public static void Get_NewTarget(Map map, Layer ground, Army army, Squad squad)
        {
            bool rest_needed = false;

            foreach (Character character in squad.Characters)
            {
                if (!character.Dead)
                {
                    int epCost = InventoryUtil.Get_EP_Cost(character);
                    if (character.ManaBar.Value < epCost ||
                        character.HealthBar.Value < character.HealthBar.Max_Value)
                    {
                        rest_needed = true;
                    }
                }
            }

            if (rest_needed)
            {
                squad.Assignment = "Rest";
                Set_NextTarget(map, ground, army, squad);
            }
            else
            {
                for (int i = 0; i < army.Squads.Count; i++)
                {
                    Squad existing = army.Squads[i];
                    if (squad.ID == existing.ID)
                    {
                        if (i == 0)
                        {
                            squad.Assignment = "Guard Base";
                            break;
                        }
                    }
                }

                if (squad.Assignment != "Guard Base")
                {
                    CryptoRandom random;

                    if (squad.Assignment != "Opportunist" &&
                        squad.Assignment != "Guard Nearest Town" &&
                        squad.Assignment != "Sleeper")
                    {
                        random = new CryptoRandom();
                        int chance = random.Next(0, 401);
                        if (chance == 0)
                        {
                            RandomAssignment(squad);
                            Set_NextTarget(map, ground, army, squad);
                        }
                    }
                    else
                    {
                        random = new CryptoRandom();
                        int chance = random.Next(0, 401);
                        if (chance == 0)
                        {
                            Set_NextTarget(map, ground, army, squad);
                        }
                    }
                }
            }
        }

        public static void RandomAssignment(Squad squad)
        {
            string assignment = "";

            if (Utility.RandomPercent(5))
            {
                assignment = "Attack Hero Squad";
            }

            if (string.IsNullOrEmpty(assignment) &&
                Utility.RandomPercent(10))
            {
                assignment = "Opportunist";
            }

            if (string.IsNullOrEmpty(assignment) &&
                Utility.RandomPercent(50))
            {
                assignment = "Guard Nearest Town";
            }

            if (string.IsNullOrEmpty(assignment) &&
                Utility.RandomPercent(25))
            {
                assignment = "Capture Nearest Town";
            }

            if (string.IsNullOrEmpty(assignment) &&
                Utility.RandomPercent(10))
            {
                assignment = "Attack Nearest Squad";
            }

            if (string.IsNullOrEmpty(assignment) &&
                Utility.RandomPercent(20))
            {
                assignment = "Sleeper";
            }

            if (string.IsNullOrEmpty(assignment) &&
                Utility.RandomPercent(5))
            {
                assignment = "Attack Base";
            }

            if (!string.IsNullOrEmpty(assignment))
            {
                squad.Assignment = assignment;
            }
        }
    }
}
