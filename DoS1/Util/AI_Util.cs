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
                target_tile = WorldUtil.GetNearest_Location_ToRest(map, ground, army, squad, false);
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
                        else if (i == 1)
                        {
                            squad.Assignment = "Attack Base";
                            break;
                        }
                    }
                }

                CryptoRandom random;

                if (squad.Assignment != "Guard Base" &&
                    squad.Assignment != "Attack Base" &&
                    squad.Assignment != "Sleeper" &&
                    squad.Assignment != "Guard Nearest Town")
                {
                    random = new CryptoRandom();
                    int chance = random.Next(0, 401);
                    if (chance == 0)
                    {
                        random = new CryptoRandom();
                        int choice = random.Next(0, 7);
                        switch (choice)
                        {
                            case 0:
                                squad.Assignment = "Attack Base";
                                break;

                            case 1:
                                squad.Assignment = "Capture Nearest Town";
                                break;

                            case 2:
                                squad.Assignment = "Guard Nearest Town";
                                break;

                            case 3:
                                squad.Assignment = "Attack Nearest Squad";
                                break;

                            case 4:
                                squad.Assignment = "Attack Hero Squad";
                                break;

                            case 5:
                                squad.Assignment = "Sleeper";
                                break;

                            case 6:
                                squad.Assignment = "Opportunist";
                                break;
                        }

                        Set_NextTarget(map, ground, army, squad);
                    }
                }
                else if (squad.Assignment == "Attack Base" ||
                         squad.Assignment == "Sleeper" ||
                         squad.Assignment == "Guard Nearest Town")
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
}
