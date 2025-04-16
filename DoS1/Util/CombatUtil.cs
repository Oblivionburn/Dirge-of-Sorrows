using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Menus;
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
            if (character != null &&
                character.Texture != null)
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
                    if (InventoryUtil.Weapon_IsAoE_Offense(weapon))
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
                        Character target = GetTarget_Melee(character, target_squad);
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
                        Character target = GetTarget_Ranged(character, target_squad);
                        if (target != null &&
                            !target.Dead)
                        {
                            targets.Add(target);
                        }
                    }
                }

                if (!targets.Any())
                {
                    Character target = GetTarget_Melee(character, target_squad);
                    if (target != null)
                    {
                        targets.Add(target);
                    }
                }
            }

            return targets;
        }

        public static Character GetTarget_Melee(Character character, Squad target_squad)
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

        public static Character GetTarget_Ranged(Character character, Squad target_squad)
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

        public static Character GetTarget_LeastHP(Squad target_squad)
        {
            Character target = null;

            foreach (Character character in target_squad.Characters)
            {
                if (!character.Dead)
                {
                    target = character;
                    break;
                }
            }

            foreach (Character character in target_squad.Characters)
            {
                if (!character.Dead)
                {
                    if (character.HealthBar.Value < target.HealthBar.Value)
                    {
                        target = character;
                    }
                }
            }

            return target;
        }

        public static Character GetTarget_LeastEP(Squad target_squad)
        {
            Character target = null;

            foreach (Character character in target_squad.Characters)
            {
                if (!character.Dead)
                {
                    target = character;
                    break;
                }
            }

            foreach (Character character in target_squad.Characters)
            {
                if (!character.Dead)
                {
                    if (character.ManaBar.Value < target.ManaBar.Value)
                    {
                        target = character;
                    }
                }
            }

            return target;
        }

        public static int GetArmor_Resistance(Character character, string type)
        {
            int total = 0;

            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                total += InventoryUtil.Get_Item_Element_Level(helm, type);
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                total += InventoryUtil.Get_Item_Element_Level(armor, type);
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                total += InventoryUtil.Get_Item_Element_Level(shield, type);
            }

            if (type == "Death" ||
                type == "Effect")
            {
                total *= 10;
            }
            else if (type != "Time")
            {
                total *= Handler.Element_Multiplier;
            }

            return total;
        }

        public static int GetArmor_Drain_Chance(Character character, string type)
        {
            int total = 0;

            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                total += InventoryUtil.Get_Item_Drain_Chance(helm, type);
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                total += InventoryUtil.Get_Item_Drain_Chance(armor, type);
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                total += InventoryUtil.Get_Item_Drain_Chance(shield, type);
            }

            return total;
        }

        public static int GetArmor_Drain_Amount(Character character, string type)
        {
            int total = 0;

            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                total += InventoryUtil.Get_Item_Drain_Level(helm, type);
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                total += InventoryUtil.Get_Item_Drain_Level(armor, type);
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                total += InventoryUtil.Get_Item_Drain_Level(shield, type);
            }

            return total;
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

        public static void DoDamage(Menu menu, Character attacker, Character defender, Item weapon, ref int ally_total_damage, ref int enemy_total_damage)
        {
            if (weapon != null)
            {
                string[] damage_types = { "Physical", "Fire", "Lightning", "Earth", "Ice" };

                for (int i = 0; i < damage_types.Length; i++)
                {
                    string type = damage_types[i];

                    if (InventoryUtil.Item_HasElement(weapon, type))
                    {
                        int damage = InventoryUtil.Get_TotalDamage(weapon, type);
                        damage -= GetArmor_Resistance(defender, type);

                        Squad defender_squad = ArmyUtil.Get_Squad(defender.ID);
                        damage -= ArmyUtil.Get_AoE_Defense(defender_squad, defender, type);

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

                        if (damage >= 0)
                        {
                            defender.HealthBar.Value -= damage;
                            if (defender.HealthBar.Value < 0)
                            {
                                defender.HealthBar.Value = 0;
                            }
                            defender.HealthBar.Update();

                            Color damage_color = Color.White;
                            switch (type)
                            {
                                case "Fire":
                                    damage_color = Color.Red;
                                    break;

                                case "Lightning":
                                    damage_color = Color.Cyan;
                                    break;

                                case "Earth":
                                    damage_color = Color.Lime;
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

                            menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "-" + damage.ToString(), damage_color,
                                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);

                            Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                            defender.StatusEffects.Add(new Something
                            {
                                ID = new_damage_label.ID,
                                Name = "Damage",
                                DrawColor = damage_color
                            });

                            if (defender.HealthBar.Value < 0)
                            {
                                defender.HealthBar.Value = 0;
                            }

                            if (defender.HealthBar.Value <= 0)
                            {
                                defender.Dead = true;
                            }

                            if (InventoryUtil.Item_HasDrain(weapon, type))
                            {
                                int weapon_drain_chance = InventoryUtil.Get_Item_Drain_Chance(weapon, type);
                                if (Utility.RandomPercent(weapon_drain_chance))
                                {
                                    DoHeal_HP(menu, attacker, weapon, damage);
                                }
                            }
                        }
                        else
                        {
                            AssetManager.PlaySound_Random("Swing");

                            menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "0", Color.Black,
                                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);

                            Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                            defender.StatusEffects.Add(new Something
                            {
                                ID = new_damage_label.ID,
                                Name = "Damage",
                                DrawColor = Color.Black
                            });
                        }

                        int armor_drain_chance = GetArmor_Drain_Chance(defender, type);
                        if (armor_drain_chance > 0)
                        {
                            if (Utility.RandomPercent(armor_drain_chance))
                            {
                                int armor_drain_amount = GetArmor_Drain_Chance(defender, type);
                                DoHeal_EP(menu, defender, weapon, armor_drain_amount);
                            }
                        }
                    }
                }
            }
            else
            {
                string type = "Physical";

                //Unarmed attack
                int damage = 10;
                damage -= GetArmor_Resistance(defender, type);

                if (damage < 0)
                {
                    damage = 0;
                }

                if (damage > 0)
                {
                    defender.HealthBar.Value -= damage;
                    if (defender.HealthBar.Value < 0)
                    {
                        defender.HealthBar.Value = 0;
                    }
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

                    Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                    defender.StatusEffects.Add(new Something
                    {
                        ID = new_damage_label.ID,
                        Name = "Damage",
                        DrawColor = damage_color
                    });

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

                    Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                    defender.StatusEffects.Add(new Something
                    {
                        ID = new_damage_label.ID,
                        Name = "Damage",
                        DrawColor = Color.Black
                    });
                }

                int armor_drain_chance = GetArmor_Drain_Chance(defender, type);
                if (armor_drain_chance > 0)
                {
                    if (Utility.RandomPercent(armor_drain_chance))
                    {
                        int armor_drain_amount = GetArmor_Drain_Chance(defender, type);
                        DoHeal_EP(menu, defender, weapon, armor_drain_amount);
                    }
                }
            }
        }

        public static void DoDodge(Menu menu, Character defender)
        {
            menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "Dodged!", Color.Black,
                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), true);

            Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

            defender.StatusEffects.Add(new Something
            {
                ID = new_damage_label.ID,
                Name = "Damage",
                DrawColor = new_damage_label.TextColor
            });
        }

        public static void DoHeal_HP(Menu menu, Character character, Item weapon, int heal)
        {
            string element = "Health";
            string effect = "Restore";

            //Check for armor extra healing
            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                Something property = helm.GetProperty(element + " " + effect);
                if (property != null)
                {
                    heal += (int)property.Value;
                }
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                Something property = armor.GetProperty(element + " " + effect);
                if (property != null)
                {
                    heal += (int)property.Value;
                }
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                Something property = shield.GetProperty(element + " " + effect);
                if (property != null)
                {
                    heal += (int)property.Value;
                }
            }

            Squad squad = ArmyUtil.Get_Squad(character.ID);
            heal += ArmyUtil.Get_AoE_Defense(squad, character, element);

            if (heal > 0)
            {
                character.HealthBar.Value += heal;
                if (character.HealthBar.Value > character.HealthBar.Max_Value)
                {
                    character.HealthBar.Value = character.HealthBar.Max_Value;
                }
                character.HealthBar.Update();

                AddEffect(menu, character, weapon, element);

                menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "+" + heal.ToString(), Color.White,
                    new Region(character.HealthBar.Base_Region.X, character.Region.Y - ((character.HealthBar.Base_Region.Width / 4) * 3),
                        character.HealthBar.Base_Region.Width, character.HealthBar.Base_Region.Width), true);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                character.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = new_damage_label.TextColor
                });
            }
        }

        public static void DoHeal_EP(Menu menu, Character character, Item weapon, int heal)
        {
            string element = "Energy";
            string effect = "Restore";

            //Check for armor extra healing
            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                Something property = helm.GetProperty(element + " " + effect);
                if (property != null)
                {
                    heal += (int)property.Value;
                }
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                Something property = armor.GetProperty(element + " " + effect);
                if (property != null)
                {
                    heal += (int)property.Value;
                }
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                Something property = shield.GetProperty(element + " " + effect);
                if (property != null)
                {
                    heal += (int)property.Value;
                }
            }

            Squad squad = ArmyUtil.Get_Squad(character.ID);
            heal += ArmyUtil.Get_AoE_Defense(squad, character, element);

            if (heal > 0)
            {
                character.ManaBar.Value += heal;
                if (character.ManaBar.Value > character.ManaBar.Max_Value)
                {
                    character.ManaBar.Value = character.ManaBar.Max_Value;
                }
                character.ManaBar.Update();

                AddEffect(menu, character, weapon, element);

                menu.AddLabel(AssetManager.Fonts["ControlFont"], Handler.GetID(), "Damage", "+" + heal.ToString(), new Color(255, 174, 201, 255),
                    new Region(character.ManaBar.Base_Region.X, character.Region.Y - ((character.ManaBar.Base_Region.Width / 4) * 3),
                        character.ManaBar.Base_Region.Width, character.ManaBar.Base_Region.Width), true);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                character.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = new_damage_label.TextColor
                });
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
                                    new Region(x, y, width, height), Color.White, true);
                            }
                            else if (character.Type == "Enemy")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Slash_Left"],
                                    new Region(x, y, width, height), Color.White, true);
                            }
                        }
                        else if (weapon.Categories.Contains("Bow"))
                        {
                            AssetManager.PlaySound_Random("Bow");

                            if (character.Type == "Ally")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Arrow_Right"],
                                    new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                        Color.White, true);
                            }
                            else if (character.Type == "Enemy")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Arrow_Left"],
                                    new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                        Color.White, true);
                            }
                        }
                        else
                        {
                            AssetManager.PlaySound_Random("Thump");

                            menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump"],
                                new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                    Color.White, true);
                        }
                    }
                    else
                    {
                        AssetManager.PlaySound_Random("Thump");

                        menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump"],
                            new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                Color.White, true);
                    }
                    break;

                case "Fire":
                    AssetManager.PlaySound_Random("Fire");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Fire"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Lightning":
                    AssetManager.PlaySound_Random("Shock");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Lightning"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Earth":
                    AssetManager.PlaySound_Random("Earth");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Earth"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White, true);
                    break;

                case "Ice":
                    AssetManager.PlaySound_Random("Ice");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Ice"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Health":
                case "Energy":
                    AssetManager.PlaySound_Random("Heal");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Heal"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White, true);
                    break;

                case "Death":
                    AssetManager.PlaySound_Random("Death");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Death"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Poison":
                    AssetManager.PlaySound_Random("Poison");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Poison"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "HP Drain":
                    AssetManager.PlaySound_Random("Leech");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["HP Drain"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "EP Drain":
                    AssetManager.PlaySound_Random("Siphon");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["EP Drain"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;
            }

            Picture effect = Get_LastDamagePicture(menu);
            effect.Image = new Rectangle(0, 0, effect.Texture.Width / 4, effect.Texture.Height);
        }

        public static void GainExp(Character character, int amount)
        {
            //Increase rune XP on equipment
            Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
            if (weapon != null)
            {
                GainExp_Item(weapon, 1);
            }

            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                GainExp_Item(helm, 1);
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                GainExp_Item(armor, 1);
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                GainExp_Item(shield, 1);
            }
        }

        public static void GainExp_Item(Item item, int amount)
        {
            if (item != null &&
                amount > 0)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    Something xp = rune.GetProperty("XP Value");
                    if (xp != null)
                    {
                        xp.Value += amount;
                        InventoryUtil.UpdateRune_Level(rune);
                    }
                }

                InventoryUtil.UpdateItem(item);
            }
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
