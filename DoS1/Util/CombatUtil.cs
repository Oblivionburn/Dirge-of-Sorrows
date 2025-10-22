using System;
using System.Collections.Generic;

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

            Handler.Combat_Terrain = WorldUtil.Get_Terrain_Tile(map, other_squad_location);

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

            Menu ui = MenuManager.GetMenu("UI");
            Label examine = ui.GetLabel("Examine");
            examine.Visible = false;

            Layer pathing = map.GetLayer("Pathing");
            pathing.Visible = false;
            pathing.Tiles.Clear();

            squad.Path.Clear();
            other_squad.Path.Clear();

            Main.Timer.Stop();
            WorldUtil.CameraToTile(map, ground, destination);
            GameUtil.Alert_Combat(squad, other_squad);
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
                    for (int i = 0; i < character.Tags.Count; i++)
                    {
                        if (character.Tags[i].Contains("Animation"))
                        {
                            character.Tags.Remove(character.Tags[i]);
                            i--;
                        }
                    }

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
                    character.Tags.Add("Animation_" + type);
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
                    if (weapon.Categories.Contains("Axe"))
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
                    else if (weapon.Categories.Contains("Mace") ||
                             weapon.Categories.Contains("Sword"))
                    {
                        Character target = GetTarget_Melee(character, target_squad);
                        if (target != null)
                        {
                            targets.Add(target);
                        }
                    }
                }
                else
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

        public static int GetArmor_Resistance(Character character, string element)
        {
            int total = 0;

            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                total += InventoryUtil.Get_TotalDefense(helm, element);
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                total += InventoryUtil.Get_TotalDefense(armor, element);
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                total += InventoryUtil.Get_TotalDefense(shield, element);
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

                CharacterUtil.UpdateGear(character);
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

                CharacterUtil.UpdateGear(character);
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

        public static void CureStatusEffects(Menu menu, Character character)
        {
            for (int i = 0; i < character.StatusEffects.Count; i++)
            {
                Something statusEffect = character.StatusEffects[i];

                if (statusEffect.Name == "Weak" ||
                    statusEffect.Name == "Cursed" ||
                    statusEffect.Name == "Melting" ||
                    statusEffect.Name == "Poisoned" ||
                    statusEffect.Name == "Petrified" ||
                    statusEffect.Name == "Burning" ||
                    statusEffect.Name == "Stunned" ||
                    statusEffect.Name == "Slow" ||
                    statusEffect.Name == "Frozen" ||
                    statusEffect.Name == "Shocked")
                {
                    character.StatusEffects.Remove(statusEffect);

                    menu.AddLabel(AssetManager.Fonts["ControlFont"], character.ID, "Damage", "-" + statusEffect.Name, Color.White,
                        new Region(character.Region.X + (character.Region.Width / 4), character.Region.Y - ((character.Region.Width / 8) * 3),
                            character.Region.Width / 2, character.Region.Width / 2), false);

                    i--;
                }
            }
        }

        public static bool Dodge(Menu menu, Character attacker, Character defender)
        {
            int attacker_dex = (int)attacker.GetStat("DEX").Value;
            int defender_agi = (int)defender.GetStat("AGI").Value;

            int chance = defender_agi - attacker_dex;
            if (chance > 0)
            {
                bool dodge = Utility.RandomPercent(chance);
                if (dodge)
                {
                    RuneUtil.Time_Dodge(menu, defender);
                    return true;
                }
            }

            return false;
        }

        public static void DoDamage_ForElement(Menu menu, Character attacker, Character defender, Item weapon, string element, ref int ally_total_damage, ref int enemy_total_damage)
        {
            int damage = InventoryUtil.Get_TotalDamage(weapon, element);

            if (InventoryUtil.Weapon_IsMelee(weapon) ||
                weapon.Categories[0] == "Bow")
            {
                damage += (int)attacker.GetStat("STR").Value;
            }
            else if (weapon.Categories[0] == "Grimoire")
            {
                damage += (int)attacker.GetStat("INT").Value;
            }

            Something status_weak = attacker.GetStatusEffect("Weak");
            if (status_weak != null)
            {
                damage /= 2;
            }

            Something status_slow = attacker.GetStatusEffect("Slow");
            if (status_slow != null)
            {
                damage = 0;
            }

            if (!RuneUtil.CounterWeapon(menu, attacker, defender))
            {
                //Reduce by defender's resistance
                int resistance = GetArmor_Resistance(defender, element);
                damage -= resistance;

                //Reduce by squad's Area resistances
                Squad defender_squad = ArmyUtil.Get_Squad(defender.ID);
                int area_resistance = RuneUtil.Area_AllArmor_PairedLevel(defender_squad, defender, element);
                damage -= area_resistance;
            }

            if (element == "Physical")
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
                switch (element)
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

                AddEffect(menu, defender, weapon, element);

                if (defender.Type == "Ally")
                {
                    enemy_total_damage += damage;
                }
                else if (defender.Type == "Enemy")
                {
                    ally_total_damage += damage;
                }

                menu.AddLabel(AssetManager.Fonts["ControlFont"], defender.ID, "Damage", "-" + damage.ToString(), damage_color,
                    new Region(defender.Region.X + (defender.Region.Width / 4), defender.Region.Y - ((defender.Region.Width / 8) * 3),
                        defender.Region.Width / 2, defender.Region.Width / 2), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                defender.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = damage_color
                });

                if (defender.HealthBar.Value <= 0)
                {
                    defender.Dead = true;
                }

                RuneUtil.DrainWeapon(menu, attacker, defender, weapon, element, damage);
            }
            else
            {
                AssetManager.PlaySound_Random("Swing");

                menu.AddLabel(AssetManager.Fonts["ControlFont"], defender.ID, "Damage", "0", Color.White,
                    new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                        defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                defender.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = Color.Black
                });
            }

            RuneUtil.DrainArmor(menu, defender, weapon, element, damage);
            RuneUtil.DisarmArmor(menu, attacker, defender);
            RuneUtil.StatusEffect(menu, defender, weapon, element, false);
        }

        public static void DoDamage_ForStatus(Menu menu, Character character, Something statusEffect, ref int ally_total_damage, ref int enemy_total_damage)
        {
            int damage = (int)statusEffect.Value;

            if (statusEffect.Name == "Burning" &&
                statusEffect.Amount > 0)
            {
                damage *= statusEffect.Amount;
            }

            Color damage_color = GameUtil.Get_EffectColor(statusEffect.Name);
            statusEffect.DrawColor = damage_color;

            if (statusEffect.Name == "Cursed")
            {
                bool death_chance = Utility.RandomPercent(10);
                if (death_chance)
                {
                    character.Dead = true;
                    AddEffect_Status(menu, character, statusEffect.Name);
                }
            }
            else if (statusEffect.Name == "Melting")
            {
                bool acid_chance = Utility.RandomPercent(10);
                if (acid_chance)
                {
                    CryptoRandom random;
                    
                    for (int i = 0; i < character.Inventory.Items.Count; i++)
                    {
                        random = new CryptoRandom();

                        int acid_choice = random.Next(0, character.Inventory.Items.Count);
                        Item item = character.Inventory.Items[acid_choice];

                        if (InventoryUtil.IsArmor(item) ||
                            InventoryUtil.IsWeapon(item))
                        {
                            character.Inventory.Items.Remove(item);
                            break;
                        }
                    }

                    AddEffect_Status(menu, character, statusEffect.Name);
                }
            }
            else
            {
                AddEffect_Status(menu, character, statusEffect.Name);
            }

            if (damage > 0)
            {
                if (statusEffect.Name == "Regenerating" ||
                    statusEffect.Name == "Charging")
                {
                    if (statusEffect.Name == "Regenerating")
                    {
                        character.HealthBar.Value += damage;
                        if (character.HealthBar.Value > character.HealthBar.Max_Value)
                        {
                            character.HealthBar.Value = character.HealthBar.Max_Value;
                        }
                        character.HealthBar.Update();
                    }
                    else if (statusEffect.Name == "Charging")
                    {
                        character.ManaBar.Value += damage;
                        if (character.ManaBar.Value > character.ManaBar.Max_Value)
                        {
                            character.ManaBar.Value = character.ManaBar.Max_Value;
                        }
                        character.ManaBar.Update();
                    }

                    RuneUtil.AddCombatLabel(menu, character, statusEffect.Name + "! (+" + damage.ToString() + ")", damage_color);
                }
                else
                {
                    character.HealthBar.Value -= damage;
                    if (character.HealthBar.Value < 0)
                    {
                        character.HealthBar.Value = 0;
                    }
                    character.HealthBar.Update();

                    if (character.Type == "Ally")
                    {
                        enemy_total_damage += damage;
                    }
                    else if (character.Type == "Enemy")
                    {
                        ally_total_damage += damage;
                    }

                    RuneUtil.AddCombatLabel(menu, character, statusEffect.Name + "! (-" + damage.ToString() + ")", damage_color);
                }
            }
            else
            {
                RuneUtil.AddCombatLabel(menu, character, statusEffect.Name + "!", damage_color);
            }

            Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

            character.StatusEffects.Add(new Something
            {
                ID = new_damage_label.ID,
                Name = "Damage",
                DrawColor = damage_color
            });

            if (character.HealthBar.Value <= 0)
            {
                character.Dead = true;
            }
        }

        public static void DoDamage_Unarmed(Menu menu, Character defender, ref int ally_total_damage, ref int enemy_total_damage)
        {
            string type = "Physical";

            //Unarmed attack
            int damage = 10;

            RuneUtil.DrainArmor(menu, defender, null, type, damage);

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

                AddEffect(menu, defender, null, "Physical");

                if (defender.Type == "Ally")
                {
                    enemy_total_damage += damage;
                }
                else if (defender.Type == "Enemy")
                {
                    ally_total_damage += damage;
                }

                menu.AddLabel(AssetManager.Fonts["ControlFont"], defender.ID, "Damage", "-" + damage.ToString(), Color.White,
                    new Region(defender.Region.X + (defender.Region.Width / 4), defender.Region.Y - ((defender.Region.Width / 8) * 3), 
                        defender.Region.Width / 2, defender.Region.Width / 2), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                defender.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = Color.White
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

                menu.AddLabel(AssetManager.Fonts["ControlFont"], defender.ID, "Damage", "0", Color.White,
                    new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                        defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                defender.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = Color.White
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

                            if (character.Type == "Ally")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump_Right"],
                                    new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                        Color.White, true);
                            }
                            else if (character.Type == "Enemy")
                            {
                                menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump_Left"],
                                    new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                        Color.White, true);
                            }
                        }
                    }
                    else
                    {
                        AssetManager.PlaySound_Random("Thump");

                        if (character.Type == "Ally")
                        {
                            menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump_Right"],
                                new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                    Color.White, true);
                        }
                        else if (character.Type == "Enemy")
                        {
                            menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Thump_Left"],
                                new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                                    Color.White, true);
                        }
                    }
                    break;

                case "Fire":
                    AssetManager.PlaySound_Random("Fire");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Fire"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.8f, true);
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
                        new Region(character.Region.X, character.Region.Y - (character.Region.Height / 3), character.Region.Width, 
                            character.Region.Height), Color.White, true);
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
            effect.Image = new Rectangle(0, 0, effect.Texture.Height, effect.Texture.Height);
        }

        public static void AddEffect_Status(Menu menu, Character character, string status)
        {
            switch (status)
            {
                case "Cursed":
                    AssetManager.PlaySound_Random("Death");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Death"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Melting":
                    AssetManager.PlaySound_Random("Acid");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Acid"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Poisoned":
                    AssetManager.PlaySound_Random("Poison");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Poison"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Burning":
                    AssetManager.PlaySound_Random("Fire");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Fire"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Regenerating":
                case "Charging":
                    AssetManager.PlaySound_Random("Heal");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Heal"],
                        new Region(character.Region.X, character.Region.Y - (character.Region.Height / 3), character.Region.Width,
                            character.Region.Height), Color.White, true);
                    break;

                case "Frozen":
                    AssetManager.PlaySound_Random("Ice");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Ice"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;

                case "Shocked":
                    AssetManager.PlaySound_Random("Shock");

                    menu.AddPicture(Handler.GetID(), "Damage", AssetManager.Textures["Lightning"],
                        new Region(character.Region.X, character.Region.Y, character.Region.Width, character.Region.Height),
                            Color.White * 0.9f, true);
                    break;
            }

            Picture effect = Get_LastDamagePicture(menu);
            if (effect != null)
            {
                effect.Image = new Rectangle(0, 0, effect.Texture.Width / 4, effect.Texture.Height);
            }
        }

        public static void Kill(Character character)
        {
            character.HealthBar.Value = 0;
            character.HealthBar.Update();

            character.Dead = true;

            Squad squad = ArmyUtil.Get_Squad(character.ID);
            if (squad != null)
            {
                squad.Characters.Remove(character);
            }
        }

        public static int GainExp(Character character, int amount)
        {
            int levels_gained = 0;

            //Increase rune XP on equipment
            Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
            if (weapon != null)
            {
                GainExp_Item(weapon, amount);
            }

            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                GainExp_Item(helm, amount);
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                GainExp_Item(armor, amount);
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                GainExp_Item(shield, amount);
            }

            levels_gained = CharacterUtil.Increase_XP(character, amount);

            return levels_gained;
        }

        public static void GainExp_Item(Item item, int amount)
        {
            if (item != null &&
                amount > 0)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];
                    RuneUtil.Increase_XP(rune, amount);
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
