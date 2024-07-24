using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Rendering;
using OP_Engine.Tiles;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class CombatUtil
    {
        public static void StartCombat(Map map, Layer ground, Tile destination, Squad squad, Squad other_squad)
        {
            Vector2 other_squad_location = new Vector2(other_squad.Location.X, other_squad.Location.Y);

            Handler.Combat_Terrain = WorldUtil.Get_Terrain(map, other_squad_location);

            Handler.Combat_Ally_Base = false;
            Handler.Combat_Enemy_Base = false;

            if (squad.Type == "Ally")
            {
                Handler.Combat_Ally_Squad = squad.ID;
                Handler.Combat_Enemy_Squad = other_squad.ID;

                Tile enemy_base = WorldUtil.Get_Base(map, "Enemy");
                if (enemy_base != null)
                {
                    if (other_squad_location.X == enemy_base.Location.X &&
                        other_squad_location.Y == enemy_base.Location.Y)
                    {
                        Handler.Combat_Enemy_Base = true;
                    }
                }
            }
            else if (squad.Type == "Enemy")
            {
                Handler.Combat_Ally_Squad = other_squad.ID;
                Handler.Combat_Enemy_Squad = squad.ID;

                Tile ally_base = WorldUtil.Get_Base(map, "Ally");
                if (ally_base != null)
                {
                    if (other_squad_location.X == ally_base.Location.X &&
                        other_squad_location.Y == ally_base.Location.Y)
                    {
                        Handler.Combat_Ally_Base = true;
                    }
                }
            }

            squad.Path.Clear();
            Main.Timer.Stop();
            WorldUtil.CameraToTile(map, ground, destination);
            GameUtil.Alert_Combat(squad.Name, other_squad.Name);
        }

        public static void SwitchAnimation(Character character, string type)
        {
            if (character.Texture != null)
            {
                string[] parts = character.Texture.Name.Split('_');
                string direction = parts[0];
                string category = parts[1];
                string skin_tone = parts[2];

                string texture = direction + "_" + category + "_" + skin_tone + "_" + type;

                character.Texture = AssetManager.Textures[texture];
                if (character.Texture != null)
                {
                    Item item = InventoryUtil.Get_EquippedItem(character, "Armor");
                    if (item != null)
                    {
                        texture = direction + "_Armor_" + item.Categories[0] + "_" + item.Materials[0] + "_" + type;
                        item.Texture = AssetManager.Textures[texture];
                    }

                    item = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (item != null)
                    {
                        texture = direction + "_Weapon_" + item.Categories[0] + "_" + item.Materials[0] + "_" + type;
                        item.Texture = AssetManager.Textures[texture];
                    }

                    CharacterUtil.ResetAnimation(character);
                }
            }
        }

        public static List<Character> GetTargets(Character character, Squad ally_squad, Squad enemy_squad)
        {
            List<Character> targets = new List<Character>();

            Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
            Squad target_squad = null;

            if (character.Type == "Ally")
            {
                target_squad = enemy_squad;
            }
            else if (character.Type == "Enemy")
            {
                target_squad = ally_squad;
            }

            if (target_squad != null)
            {
                if (weapon != null)
                {
                    if (InventoryUtil.Weapon_IsAoE(weapon))
                    {
                        foreach (Character target in target_squad.Characters)
                        {
                            if (!target.Dead)
                            {
                                targets.Add(target);
                            }
                        }
                    }
                    else if (weapon.Categories.Contains("Axe"))
                    {
                        Character target = GetMeleeTarget(character, target_squad);
                        if (target != null)
                        {
                            targets.Add(target);

                            Character side_target = null;

                            if (character.Formation.Y == 0 &&
                                target.Formation.Y == 0)
                            {
                                side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y + 1));
                            }
                            else if (character.Formation.Y == 1 &&
                                     target.Formation.Y == 1)
                            {
                                side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y - 1));
                                if (side_target != null &&
                                    side_target.Dead)
                                {
                                    side_target = null;
                                }

                                if (side_target == null)
                                {
                                    side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y + 1));
                                }
                            }
                            else if (character.Formation.Y == 2 &&
                                     target.Formation.Y == 2)
                            {
                                side_target = target_squad.GetCharacter(new Vector2(target.Formation.X, character.Formation.Y - 1));
                            }

                            if (side_target != null &&
                                side_target.Dead)
                            {
                                side_target = null;
                            }

                            if (side_target != null)
                            {
                                targets.Add(side_target);
                            }
                        }
                    }
                    else if (weapon.Categories.Contains("Bow") ||
                             weapon.Categories.Contains("Grimoire"))
                    {
                        Character target = GetRangedTarget(character, target_squad);
                        if (target != null &&
                            !target.Dead)
                        {
                            targets.Add(target);
                        }
                    }
                }

                if (!targets.Any())
                {
                    Character target = GetMeleeTarget(character, target_squad);
                    if (target != null)
                    {
                        targets.Add(target);
                    }
                }
            }

            return targets;
        }

        public static Character GetMeleeTarget(Character character, Squad target_squad)
        {
            Character target = null;

            if (character.Type == "Ally")
            {
                for (int x = 2; x >= 0; x--)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }
            else if (character.Type == "Enemy")
            {
                for (int x = 0; x < 3; x++)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }

            if (target != null &&
                target.Dead)
            {
                target = null;
            }

            return target;
        }

        public static Character GetRangedTarget(Character character, Squad target_squad)
        {
            Character target = null;

            if (character.Type == "Ally")
            {
                for (int x = 0; x < 3; x++)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 0; x < 3; x++)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }
            else if (character.Type == "Enemy")
            {
                for (int x = 2; x >= 0; x--)
                {
                    target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y));
                    if (target != null &&
                        !target.Dead)
                    {
                        return target;
                    }
                }

                if (target == null)
                {
                    if (character.Formation.Y == 0)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 1)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y + 1));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                    else if (character.Formation.Y == 2)
                    {
                        for (int x = 2; x >= 0; x--)
                        {
                            target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 1));
                            if (target != null &&
                                !target.Dead)
                            {
                                return target;
                            }
                        }

                        if (target == null)
                        {
                            for (int x = 2; x >= 0; x--)
                            {
                                target = target_squad.GetCharacter(new Vector2(x, (int)character.Formation.Y - 2));
                                if (target != null &&
                                    !target.Dead)
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }
            }

            if (target != null &&
                target.Dead)
            {
                target = null;
            }

            return target;
        }

        public static Tile TargetTile(World world, Character character)
        {
            Map map = world.Maps[0];
            Layer ground = map.GetLayer("Ground");

            if (character.Type == "Ally")
            {
                Tile tile = ground.GetTile(new Vector2(6, character.Formation.Y + 1));
                if (tile != null)
                {
                    return tile;
                }
            }
            else if (character.Type == "Enemy")
            {
                Tile tile = ground.GetTile(new Vector2(4, character.Formation.Y + 1));
                if (tile != null)
                {
                    return tile;
                }
            }

            return null;
        }

        public static Tile OriginTile(World world, Character character)
        {
            Map map = world.Maps[0];

            Layer ground = map.GetLayer("Ground");
            if (ground != null)
            {
                if (character.Type == "Ally")
                {
                    Tile tile = ground.GetTile(new Vector2(character.Formation.X + 7, character.Formation.Y + 1));
                    if (tile != null)
                    {
                        return tile;
                    }
                }
                else if (character.Type == "Enemy")
                {
                    Tile tile = ground.GetTile(new Vector2(character.Formation.X + 1, character.Formation.Y + 1));
                    if (tile != null)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }

        public static bool AtTile(Character character, Tile tile, float move_speed)
        {
            if (character.Region != null &&
                tile.Region != null)
            {
                if (character.Region.X >= tile.Region.X - (move_speed / 2) &&
                    character.Region.X <= tile.Region.X + (move_speed / 2))
                {
                    character.Region = new Region(tile.Region.X, character.Region.Y, character.Region.Width, character.Region.Height);
                    return true;
                }
            }

            return false;
        }

        public static void MoveForward(Character character, float move_speed)
        {
            if (character != null &&
                character.Region != null)
            {
                if (character.Type == "Ally")
                {
                    character.Region.X -= move_speed;
                }
                else if (character.Type == "Enemy")
                {
                    character.Region.X += move_speed;
                }
            }
        }

        public static void MoveBack(Character character, float move_speed)
        {
            if (character != null &&
                character.Region != null)
            {
                if (character.Type == "Ally")
                {
                    character.Region.X += move_speed;
                }
                else if (character.Type == "Enemy")
                {
                    character.Region.X -= move_speed;
                }
            }
        }

        public static bool FrontRow(Character character)
        {
            if (character.Type == "Ally" &&
                character.Formation.X == 0)
            {
                return true;
            }
            else if (character.Type == "Enemy" &&
                     character.Formation.X == 2)
            {
                return true;
            }

            return false;
        }

        public static bool MiddleRow(Character character)
        {
            if (character.Formation.X == 1)
            {
                return true;
            }

            return false;
        }

        public static bool BackRow(Character character)
        {
            if (character.Type == "Ally" &&
                character.Formation.X == 2)
            {
                return true;
            }
            else if (character.Type == "Enemy" &&
                     character.Formation.X == 0)
            {
                return true;
            }

            return false;
        }

        public static void DoDamage(Menu menu, Character defender, Item weapon, ref int ally_total_damage, ref int enemy_total_damage)
        {
            if (weapon != null)
            {
                string[] damage_types = { "Physical", "Fire", "Lightning", "Earth", "Ice" };

                for (int i = 0; i < damage_types.Length; i++)
                {
                    string type = damage_types[i];

                    if (InventoryUtil.Weapon_HasElement(weapon, type))
                    {
                        int damage = InventoryUtil.Get_TotalDamage(weapon, type);

                        Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
                        if (helm != null)
                        {
                            int defense = InventoryUtil.Get_TotalDefense(helm, type);
                            damage -= defense;
                        }

                        Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
                        if (armor != null)
                        {
                            int defense = InventoryUtil.Get_TotalDefense(armor, type);
                            damage -= defense;
                        }

                        Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
                        if (shield != null)
                        {
                            int defense = InventoryUtil.Get_TotalDefense(shield, type);
                            damage -= defense;
                        }

                        if (type == "Physical")
                        {
                            if ((InventoryUtil.Weapon_IsMelee(weapon) && BackRow(defender)) ||
                                (weapon.Categories.Contains("Bow") && FrontRow(defender)))
                            {
                                damage = (int)Math.Floor(damage * 0.5);
                            }
                            else if (MiddleRow(defender) &&
                                     (InventoryUtil.Weapon_IsMelee(weapon) || weapon.Categories.Contains("Bow")))
                            {
                                damage = (int)Math.Floor(damage * 0.75);
                            }
                        }

                        if (damage < 0)
                        {
                            damage = 0;
                        }

                        if (damage > 0)
                        {
                            defender.HealthBar.Value -= damage;
                            defender.HealthBar.Update();

                            Color damage_color = Color.White;
                            switch (type)
                            {
                                case "Fire":
                                    damage_color = Color.Red;
                                    break;

                                case "Lightning":
                                    damage_color = Color.Yellow;
                                    break;

                                case "Earth":
                                    damage_color = Color.Brown;
                                    break;

                                case "Ice":
                                    damage_color = Color.Blue;
                                    break;
                            }

                            AddEffect(menu, defender, weapon, type);

                            if (defender.Type == "Ally")
                            {
                                enemy_total_damage += damage;
                            }
                            else if (defender.Type == "Enemy")
                            {
                                ally_total_damage += damage;
                            }

                            menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", damage.ToString(), damage_color,
                                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);

                            if (defender.HealthBar.Value < 0)
                            {
                                defender.HealthBar.Value = 0;
                            }

                            if (defender.HealthBar.Value <= 0)
                            {
                                defender.Dead = true;
                            }
                        }
                        else
                        {
                            AssetManager.PlaySound_Random("Swing");

                            menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "0", Color.Black,
                                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);
                        }
                    }
                }
            }
            else
            {
                //Unarmed attack
                int damage = 10;

                Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
                if (helm != null)
                {
                    int defense = InventoryUtil.Get_TotalDefense(helm, "Physical");
                    damage -= defense;
                }

                Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
                if (armor != null)
                {
                    int defense = InventoryUtil.Get_TotalDefense(armor, "Physical");
                    damage -= defense;
                }

                Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
                if (shield != null)
                {
                    int defense = InventoryUtil.Get_TotalDefense(shield, "Physical");
                    damage -= defense;
                }

                if (damage < 0)
                {
                    damage = 0;
                }

                if (damage > 0)
                {
                    defender.HealthBar.Value -= damage;
                    defender.HealthBar.Update();

                    Color damage_color = Color.White;

                    AddEffect(menu, defender, weapon, "Physical");

                    if (defender.Type == "Ally")
                    {
                        enemy_total_damage += damage;
                    }
                    else if (defender.Type == "Enemy")
                    {
                        ally_total_damage += damage;
                    }

                    menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", damage.ToString(), damage_color,
                        new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                            defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);

                    if (defender.HealthBar.Value < 0)
                    {
                        defender.HealthBar.Value = 0;
                    }

                    if (defender.HealthBar.Value <= 0)
                    {
                        defender.Dead = true;
                    }
                }
                else
                {
                    AssetManager.PlaySound_Random("Swing");

                    menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "0", Color.Black,
                        new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                            defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);
                }
            }
        }

        public static void AddEffect(Menu menu, Character character, Item weapon, string type)
        {
            switch (type)
            {
                case "Physical":
                    if (weapon != null)
                    {
                        if (weapon.Categories.Contains("Sword") ||
                            weapon.Categories.Contains("Axe"))
                        {
                            AssetManager.PlaySound_Random("IronSword");

                            float x = character.Region.X - (character.Region.Width / 4);
                            float y = character.Region.Y - (character.Region.Height / 8);
                            float width = character.Region.Width + ((character.Region.Width / 4) * 2);
                            float height = character.Region.Height + ((character.Region.Height / 8) * 2);

                            if (character.Type == "Ally")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Slash_Right"],
                                    new Region(x, y, width, height),
                                        RenderingManager.Lighting.DrawColor, true);
                            }
                            else if (character.Type == "Enemy")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Slash_Left"],
                                    new Region(x, y, width, height),
                                        RenderingManager.Lighting.DrawColor, true);
                            }
                        }
                        else if (weapon.Categories.Contains("Bow"))
                        {
                            AssetManager.PlaySound_Random("Bow");

                            if (character.Type == "Ally")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Arrow_Right"],
                                    new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                        RenderingManager.Lighting.DrawColor, true);
                            }
                            else if (character.Type == "Enemy")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Arrow_Left"],
                                    new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                        RenderingManager.Lighting.DrawColor, true);
                            }
                        }
                        else
                        {
                            AssetManager.PlaySound_Random("Thump");

                            menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump"],
                                new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                    RenderingManager.Lighting.DrawColor, true);
                        }
                    }
                    else
                    {
                        AssetManager.PlaySound_Random("Thump");

                        menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump"],
                            new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                RenderingManager.Lighting.DrawColor, true);
                    }
                    break;

                case "Fire":
                    AssetManager.PlaySound_Random("Fire");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Fire"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor * 0.9f, true);
                    break;

                case "Lightning":
                    AssetManager.PlaySound_Random("Shock");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Lightning"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor * 0.9f, true);
                    break;

                case "Earth":
                    AssetManager.PlaySound_Random("Earth");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Earth"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor, true);
                    break;

                case "Ice":
                    AssetManager.PlaySound_Random("Ice");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Ice"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            RenderingManager.Lighting.DrawColor, true);
                    break;
            }

            Picture effect = Get_LastDamagePicture(menu);
            effect.Image = new Rectangle(0, 0, effect.Texture.Width / 4, effect.Texture.Height);
        }

        public static Picture Get_LastDamagePicture(Menu menu)
        {
            for (int i = menu.Pictures.Count - 1; i >= 0; i--)
            {
                Picture picture = menu.Pictures[i];
                if (picture.Name == "Damage")
                {
                    return picture;
                }
            }

            return null;
        }
    }
}
