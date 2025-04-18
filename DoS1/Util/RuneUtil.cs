﻿using FMOD;
using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Utility;
using static System.Net.Mime.MediaTypeNames;

namespace DoS1.Util
{
    public static class RuneUtil
    {
        public static bool ApplyArea(Item item, string element)
        {
            int area_chance = 0;

            if (item != null)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    if (rune.Categories.Contains("Area"))
                    {
                        Item paired_rune = InventoryUtil.GetPairedRune(item, rune);
                        if (paired_rune != null &&
                            paired_rune.Categories[0] == element)
                        {
                            Something level = rune.GetProperty("Level Value");
                            if (level != null)
                            {
                                area_chance += (int)level.Value * 10;
                            }
                        }
                    }
                }
            }

            if (area_chance > 0 &&
                Main.Game.Debugging)
            {
                return true;
            }

            if (Utility.RandomPercent(area_chance))
            {
                return true;
            }

            return false;
        }

        public static int Area_PairedLevel(Item item, string element)
        {
            int total = 0;

            if (item != null)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    if (rune.Categories.Contains("Area"))
                    {
                        Item paired_rune = InventoryUtil.GetPairedRune(item, rune);
                        if (paired_rune != null &&
                            paired_rune.Categories[0] == element)
                        {
                            Something level = paired_rune.GetProperty("Level Value");
                            if (level != null)
                            {
                                if (element == "Death")
                                {
                                    if (InventoryUtil.IsWeapon(item))
                                    {
                                        total += (int)level.Value;
                                    }
                                    else if (InventoryUtil.IsArmor(item))
                                    {
                                        total += (int)level.Value * 10;
                                    }
                                }
                                else if (element == "Time")
                                {
                                    total += (int)level.Value;
                                }
                                else
                                {
                                    total += (int)level.Value * Handler.Element_Multiplier;
                                }
                            }
                        }
                    }
                }
            }

            return total;
        }

        public static int Area_AllArmor_PairedLevel(Squad squad, Character defender, string element)
        {
            int defense = 0;

            if (squad != null)
            {
                foreach (Character character in squad.Characters)
                {
                    if (character.ID != defender.ID)
                    {
                        Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
                        if (helm != null)
                        {
                            defense += Area_PairedLevel(helm, element);
                        }

                        Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                        if (armor != null)
                        {
                            defense += Area_PairedLevel(armor, element);
                        }

                        Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
                        if (shield != null)
                        {
                            defense += Area_PairedLevel(shield, element);
                        }
                    }
                }
            }

            return defense;
        }

        public static int CounterChance(Item item)
        {
            int total = 0;

            if (item != null)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    if (rune.Categories.Contains("Counter"))
                    {
                        Something level = rune.GetProperty("Level Value");
                        if (level != null)
                        {
                            total += (int)level.Value * 10;
                        }
                    }
                }
            }

            if (total > 0 &&
                Main.Game.Debugging)
            {
                return 100;
            }

            return total;
        }

        public static bool CounterWeapon(Menu menu, Character attacker)
        {
            Item weapon = InventoryUtil.Get_EquippedItem(attacker, "Weapon");
            int counter_chance = CounterChance(weapon);

            Squad squad = ArmyUtil.Get_Squad(attacker.ID);
            foreach (Character character in squad.Characters)
            {
                if (character.ID != attacker.ID)
                {
                    weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (InventoryUtil.Item_HasArea_ForElement(weapon, "Counter"))
                    {
                        counter_chance += CounterChance(weapon);
                    }
                }
            }

            if (counter_chance > 0 &&
                Main.Game.Debugging)
            {
                counter_chance = 100;
            }

            if (Utility.RandomPercent(counter_chance))
            {
                menu.AddLabel(AssetManager.Fonts["ControlFont"], attacker.ID, "Damage", "Pierced!", new Color(86, 127, 93, 255),
                    new Region(attacker.Region.X - (attacker.Region.Width / 8), attacker.Region.Y - ((attacker.Region.Width / 8) * 6),
                        attacker.Region.Width + ((attacker.Region.Width / 8) * 2), attacker.Region.Width + ((attacker.Region.Width / 8) * 2)), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                attacker.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = new_damage_label.TextColor
                });

                return true;
            }

            return false;
        }

        public static int CounterChance_AllArmor(Character defender)
        {
            int counter_chance = 0;

            Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
            if (helm != null)
            {
                counter_chance += CounterChance(helm);
            }

            Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
            if (armor != null)
            {
                counter_chance += CounterChance(armor);
            }

            Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
            if (shield != null)
            {
                counter_chance += CounterChance(shield);
            }

            return counter_chance;
        }

        public static bool CounterArmor(Menu menu, Character defender)
        {
            int counter_chance = CounterChance_AllArmor(defender);

            Squad squad = ArmyUtil.Get_Squad(defender.ID);
            foreach (Character character in squad.Characters)
            {
                if (character.ID != defender.ID)
                {
                    Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
                    if (helm != null)
                    {
                        if (InventoryUtil.Item_HasArea_ForElement(helm, "Counter"))
                        {
                            counter_chance += CounterChance(helm);
                        }
                    }

                    Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
                    if (armor != null)
                    {
                        if (InventoryUtil.Item_HasArea_ForElement(armor, "Counter"))
                        {
                            counter_chance += CounterChance(armor);
                        }
                    }

                    Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
                    if (shield != null)
                    {
                        if (InventoryUtil.Item_HasArea_ForElement(shield, "Counter"))
                        {
                            counter_chance += CounterChance(shield);
                        }
                    }
                }
            }

            if (counter_chance > 0 &&
                Main.Game.Debugging)
            {
                counter_chance = 100;
            }

            if (Utility.RandomPercent(counter_chance))
            {
                menu.AddLabel(AssetManager.Fonts["ControlFont"], defender.ID, "Damage", "Countered!", new Color(86, 127, 93, 255),
                    new Region(defender.Region.X - (defender.Region.Width / 8), defender.Region.Y - ((defender.Region.Width / 8) * 6),
                            defender.Region.Width + ((defender.Region.Width / 8) * 2), defender.Region.Width + ((defender.Region.Width / 8) * 2)), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                defender.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = new_damage_label.TextColor
                });

                return true;
            }

            return false;
        }

        public static void Death(Menu menu, Character attacker, Character defender, Item attacker_weapon)
        {
            int chance = InventoryUtil.Get_Item_Element_Level(attacker_weapon, "Death");

            Squad attacker_squad = ArmyUtil.Get_Squad(attacker.ID);
            foreach (Character character in attacker_squad.Characters)
            {
                if (character.ID != attacker.ID)
                {
                    Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (InventoryUtil.Item_HasArea_ForElement(weapon, "Death") &&
                        ApplyArea(weapon, "Death"))
                    {
                        chance += Area_PairedLevel(weapon, "Death");
                    }
                }
            }

            if (chance > 0 &&
                Main.Game.Debugging)
            {
                chance = 100;
            }

            if (Utility.RandomPercent(chance))
            {
                int resist_chance = CombatUtil.GetArmor_Resistance(defender, "Death");

                Squad defender_squad = ArmyUtil.Get_Squad(defender.ID);
                resist_chance += Area_AllArmor_PairedLevel(defender_squad, defender, "Death");

                if (!Utility.RandomPercent(resist_chance))
                {
                    CombatUtil.AddEffect(menu, defender, attacker_weapon, "Death");
                    CombatUtil.Kill(defender);
                }
            }
        }

        public static void DisarmWeapon(Menu menu, Character attacker, Character defender, Item attacker_weapon)
        {
            string element = "Disarm";

            int chance = InventoryUtil.Get_Item_Element_Level(attacker_weapon, element);

            Squad attacker_squad = ArmyUtil.Get_Squad(attacker.ID);
            foreach (Character character in attacker_squad.Characters)
            {
                if (character.ID != attacker.ID)
                {
                    Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (InventoryUtil.Item_HasArea_ForElement(weapon, element) &&
                        ApplyArea(weapon, element))
                    {
                        chance += Area_PairedLevel(weapon, element);
                    }
                }
            }

            if (chance > 0 &&
                Main.Game.Debugging)
            {
                chance = 100;
            }

            if (Utility.RandomPercent(chance))
            {
                Item defender_weapon = InventoryUtil.Get_EquippedItem(defender, "Weapon");
                defender.Inventory.Items.Remove(defender_weapon);

                menu.AddLabel(AssetManager.Fonts["ControlFont"], defender.ID, "Damage", "Disarmed!", new Color(125, 115, 62, 255),
                    new Region(defender.Region.X - (defender.Region.Width / 8), defender.Region.Y - ((defender.Region.Width / 8) * 6),
                            defender.Region.Width + ((defender.Region.Width / 8) * 2), defender.Region.Width + ((defender.Region.Width / 8) * 2)), false);
            }
        }

        public static void DisarmArmor(Menu menu, Character attacker, Character defender)
        {
            int chance = 0;
            string element = "Disarm";

            Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
            if (helm != null)
            {
                chance += InventoryUtil.Get_Item_Element_Level(helm, element);
            }

            Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
            if (armor != null)
            {
                chance += InventoryUtil.Get_Item_Element_Level(armor, element);
            }

            Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
            if (shield != null)
            {
                chance += InventoryUtil.Get_Item_Element_Level(shield, element);
            }

            Squad defender_squad = ArmyUtil.Get_Squad(defender.ID);
            if (defender_squad != null)
            {
                foreach (Character character in defender_squad.Characters)
                {
                    if (character.ID != defender.ID)
                    {
                        helm = InventoryUtil.Get_EquippedItem(character, "Helm");
                        if (helm != null)
                        {
                            if (InventoryUtil.Item_HasArea_ForElement(helm, element) &&
                                ApplyArea(helm, element))
                            {
                                chance += Area_PairedLevel(helm, element);
                            }
                        }

                        armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                        if (armor != null)
                        {
                            if (InventoryUtil.Item_HasArea_ForElement(armor, element) &&
                                ApplyArea(armor, element))
                            {
                                chance += Area_PairedLevel(armor, element);
                            }
                        }

                        shield = InventoryUtil.Get_EquippedItem(character, "Shield");
                        if (shield != null)
                        {
                            if (InventoryUtil.Item_HasArea_ForElement(shield, element) &&
                                ApplyArea(shield, element))
                            {
                                chance += Area_PairedLevel(shield, element);
                            }
                        }
                    }
                }
            }

            if (chance > 0 &&
                Main.Game.Debugging)
            {
                chance = 100;
            }

            if (Utility.RandomPercent(chance))
            {
                Item attacker_weapon = InventoryUtil.Get_EquippedItem(attacker, "Weapon");
                attacker.Inventory.Items.Remove(attacker_weapon);

                menu.AddLabel(AssetManager.Fonts["ControlFont"], attacker.ID, "Damage", "Disarmed!", new Color(125, 115, 62, 255),
                    new Region(attacker.Region.X - (attacker.Region.Width / 8), attacker.Region.Y - ((attacker.Region.Width / 8) * 6),
                        attacker.Region.Width + ((attacker.Region.Width / 8) * 2), attacker.Region.Width + ((attacker.Region.Width / 8) * 2)), false);
            }
        }

        public static int DrainChance(Item item, string element)
        {
            int total = 0;

            if (item != null)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    if (rune.Categories.Contains("Drain"))
                    {
                        Item paired_rune = InventoryUtil.GetPairedRune(item, rune);
                        if (paired_rune != null &&
                            paired_rune.Categories[0] == element)
                        {
                            Something level = rune.GetProperty("Level Value");
                            if (level != null)
                            {
                                total += (int)level.Value * 10;
                            }
                        }
                    }
                }
            }

            if (total > 0 &&
                Main.Game.Debugging)
            {
                return 100;
            }

            return total;
        }

        public static int Drain_PairedLevel(Item item, string element)
        {
            int total = 0;

            if (item != null)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    if (rune.Categories.Contains("Drain"))
                    {
                        Item paired_rune = InventoryUtil.GetPairedRune(item, rune);
                        if (paired_rune != null &&
                            paired_rune.Categories[0] == element)
                        {
                            Something level = paired_rune.GetProperty("Level Value");
                            if (level != null)
                            {
                                total += (int)level.Value * Handler.Element_Multiplier;
                            }
                        }
                    }
                }
            }

            return total;
        }

        public static void DrainWeapon(Menu menu, Character attacker, Item weapon, string element, int damage)
        {
            if (weapon != null)
            {
                if (InventoryUtil.Item_HasDrain_ForElement(weapon, element))
                {
                    int weapon_drain_chance = DrainChance(weapon, element);
                    if (Utility.RandomPercent(weapon_drain_chance))
                    {
                        Health(menu, attacker, weapon, damage);
                    }
                }
            }
        }

        public static void DrainArmor(Menu menu, Character defender, Item weapon, string element, int damage)
        {
            int armor_drain_level = 0;

            Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
            if (helm != null)
            {
                int drain_chance = DrainChance(helm, element);
                if (Utility.RandomPercent(drain_chance))
                {
                    int drain_level = Drain_PairedLevel(helm, element);
                    if (drain_level >= damage)
                    {
                        armor_drain_level += damage;
                    }
                    else
                    {
                        armor_drain_level += drain_level;
                    }
                }
            }

            Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
            if (armor != null)
            {
                int drain_chance = DrainChance(armor, element);
                if (Utility.RandomPercent(drain_chance))
                {
                    int drain_level = Drain_PairedLevel(armor, element);
                    if (drain_level >= damage)
                    {
                        armor_drain_level += damage;
                    }
                    else
                    {
                        armor_drain_level += drain_level;
                    }
                }
            }

            Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
            if (shield != null)
            {
                int drain_chance = DrainChance(shield, element);
                if (Utility.RandomPercent(drain_chance))
                {
                    int drain_level = Drain_PairedLevel(shield, element);
                    if (drain_level >= damage)
                    {
                        armor_drain_level += damage;
                    }
                    else
                    {
                        armor_drain_level += drain_level;
                    }
                }
            }

            Energy(menu, defender, weapon, armor_drain_level);
        }

        public static void Health(Menu menu, Character character, Item weapon, int heal)
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

            //Check for AoE extra healing
            Squad squad = ArmyUtil.Get_Squad(character.ID);
            heal += Area_AllArmor_PairedLevel(squad, character, element);

            if (heal > 0)
            {
                //Apply heal
                character.HealthBar.Value += heal;
                if (character.HealthBar.Value > character.HealthBar.Max_Value)
                {
                    character.HealthBar.Value = character.HealthBar.Max_Value;
                }
                character.HealthBar.Update();

                CombatUtil.AddEffect(menu, character, weapon, element);

                menu.AddLabel(AssetManager.Fonts["ControlFont"], character.ID, "Damage", "+" + heal.ToString(), Color.White,
                    new Region(character.Region.X + (character.Region.Width / 4), character.Region.Y - ((character.Region.Width / 8) * 3),
                        character.Region.Width / 2, character.Region.Width / 2), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                //Add Damage status effect to trigger color flash on character
                character.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = new_damage_label.TextColor
                });
            }
        }

        public static void Energy(Menu menu, Character character, Item weapon, int energy)
        {
            string element = "Energy";
            string effect = "Restore";

            //Check for armor extra energy
            Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
            if (helm != null)
            {
                Something property = helm.GetProperty(element + " " + effect);
                if (property != null)
                {
                    energy += (int)property.Value;
                }
            }

            Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
            if (armor != null)
            {
                Something property = armor.GetProperty(element + " " + effect);
                if (property != null)
                {
                    energy += (int)property.Value;
                }
            }

            Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
            if (shield != null)
            {
                Something property = shield.GetProperty(element + " " + effect);
                if (property != null)
                {
                    energy += (int)property.Value;
                }
            }

            //Check for AoE extra energy
            Squad squad = ArmyUtil.Get_Squad(character.ID);
            if (squad != null)
            {
                energy += Area_AllArmor_PairedLevel(squad, character, element);
            }

            if (energy > 0)
            {
                //Apply energy
                character.ManaBar.Value += energy;
                if (character.ManaBar.Value > character.ManaBar.Max_Value)
                {
                    character.ManaBar.Value = character.ManaBar.Max_Value;
                }
                character.ManaBar.Update();

                CombatUtil.AddEffect(menu, character, weapon, element);

                menu.AddLabel(AssetManager.Fonts["ControlFont"], character.ID, "Damage", "+" + energy.ToString(), new Color(255, 174, 201, 255),
                    new Region(character.Region.X + (character.Region.Width / 4), character.Region.Y - ((character.Region.Width / 8) * 3),
                        character.Region.Width / 2, character.Region.Width / 2), false);

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                //Add Damage status effect to trigger color flash on character
                character.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = new_damage_label.TextColor
                });
            }
        }

        public static bool Time_DodgeChance(Character target, Squad defender_squad)
        {
            int dodge_chance = CombatUtil.GetArmor_Resistance(target, "Time");
            dodge_chance += Area_AllArmor_PairedLevel(defender_squad, target, "Time");

            if (dodge_chance > 0 &&
                Main.Game.Debugging)
            {
                return true;
            }

            if (Utility.RandomPercent(dodge_chance))
            {
                return true;
            }

            return false;
        }

        public static void Time_Dodge(Menu menu, Character defender)
        {
            menu.AddLabel(AssetManager.Fonts["ControlFont"], defender.ID, "Damage", "Dodged!", new Color(255, 140, 20, 255),
                new Region(defender.HealthBar.Base_Region.X, defender.Region.Y - ((defender.HealthBar.Base_Region.Width / 4) * 3),
                    defender.HealthBar.Base_Region.Width, defender.HealthBar.Base_Region.Width), false);

            Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

            defender.StatusEffects.Add(new Something
            {
                ID = new_damage_label.ID,
                Name = "Damage",
                DrawColor = new_damage_label.TextColor
            });
        }

        public static bool Time_AttackChance(Menu menu, Character attacker)
        {
            Item weapon = InventoryUtil.Get_EquippedItem(attacker, "Weapon");
            if (InventoryUtil.Item_HasElement(weapon, "Time"))
            {
                int chance = InventoryUtil.Get_Item_Element_Level(weapon, "Time");

                if (chance > 0 &&
                    Main.Game.Debugging)
                {
                    chance = 100;
                }

                if (Utility.RandomPercent(chance))
                {
                    menu.AddLabel(AssetManager.Fonts["ControlFont"], attacker.ID, "Damage", "Again!", new Color(255, 140, 20, 255),
                        new Region(attacker.Region.X, attacker.Region.Y - ((attacker.Region.Width / 8) * 5),
                            attacker.Region.Width, attacker.Region.Width), true);

                    return true;
                }
            }

            return false;
        }
    }
}
