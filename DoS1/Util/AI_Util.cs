using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Tiles;

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
    }
}
