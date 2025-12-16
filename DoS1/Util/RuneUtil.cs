using Microsoft.Xna.Framework;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Utility;

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
                                    if (item.Type == "Weapon")
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
                            if (ApplyArea(helm, element))
                            {
                                defense += Area_PairedLevel(helm, element);
                            }
                        }

                        Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                        if (armor != null)
                        {
                            if (ApplyArea(armor, element))
                            {
                                defense += Area_PairedLevel(armor, element);
                            }
                        }

                        Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
                        if (shield != null)
                        {
                            if (ApplyArea(shield, element))
                            {
                                defense += Area_PairedLevel(shield, element);
                            }
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

        public static bool CounterWeapon(Menu menu, Character attacker, Character defender)
        {
            Item weapon = InventoryUtil.Get_EquippedItem(attacker, "Weapon");
            int counter_chance = CounterChance(weapon);

            Squad squad = attacker.Squad;
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
                AddCombatLabel(menu, attacker, "Pierced!", new Color(86, 127, 93, 255));

                Label new_damage_label = menu.Labels[menu.Labels.Count - 1];

                attacker.StatusEffects.Add(new Something
                {
                    ID = new_damage_label.ID,
                    Name = "Damage",
                    DrawColor = new_damage_label.TextColor
                });

                StatusEffect(menu, defender, weapon, "Counter", false);
                return true;
            }

            StatusEffect(menu, defender, weapon, "Counter", false);
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

            Squad squad = defender.Squad;
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
                AddCombatLabel(menu, defender, "Countered!", new Color(86, 127, 93, 255));

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

        public static int DeathResistChance(Item item)
        {
            int total = 0;

            if (item != null)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    if (rune.Categories.Contains("Death"))
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

        public static bool Death(Menu menu, Character attacker, Character defender, Item attacker_weapon)
        {
            int chance = InventoryUtil.Get_Item_Element_Level(attacker_weapon, "Death");

            Squad attacker_squad = attacker.Squad;
            foreach (Character character in attacker_squad.Characters)
            {
                if (character.ID != attacker.ID)
                {
                    Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (ApplyArea(weapon, "Death"))
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
                int resist_chance = 0;

                Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
                if (helm != null)
                {
                    resist_chance += DeathResistChance(helm);
                }

                Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
                if (armor != null)
                {
                    resist_chance += DeathResistChance(armor);
                }

                Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
                if (shield != null)
                {
                    resist_chance += DeathResistChance(shield);
                }

                resist_chance += Area_AllArmor_PairedLevel(defender.Squad, defender, "Death");

                if (!Utility.RandomPercent(resist_chance))
                {
                    CombatUtil.AddEffect(menu, defender, attacker_weapon, "Death");
                    CombatUtil.Kill(defender);
                    return true;
                }
                else
                {
                    StatusEffect(menu, defender, attacker_weapon, "Death", false);
                }
            }

            return false;
        }

        public static void DisarmWeapon(Menu menu, Character attacker, Character defender, Item attacker_weapon)
        {
            string element = "Disarm";

            int chance = InventoryUtil.Get_Item_Element_Level(attacker_weapon, element);

            Squad attacker_squad = attacker.Squad;
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
                InventoryUtil.UnequipItem(defender, defender_weapon);

                defender.Inventory.Items.Remove(defender_weapon);

                if (defender.Type == "Ally")
                {
                    Inventory inventory = InventoryManager.GetInventory("Ally");
                    inventory.Items.Add(defender_weapon);
                }

                AddCombatLabel(menu, defender, "Disarmed!", new Color(125, 115, 62, 255));
            }

            StatusEffect(menu, defender, attacker_weapon, element, false);
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

            Squad defender_squad = defender.Squad;
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
                InventoryUtil.UnequipItem(attacker, attacker_weapon);

                attacker.Inventory.Items.Remove(attacker_weapon);

                if (attacker.Type == "Ally")
                {
                    Inventory inventory = InventoryManager.GetInventory("Ally");
                    inventory.Items.Add(attacker_weapon);
                }

                AddCombatLabel(menu, attacker, "Disarmed!", new Color(125, 115, 62, 255));
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

        public static void DrainWeapon(Menu menu, Character attacker, Character defender, Item weapon, string element, int damage)
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

                    StatusEffect(menu, defender, weapon, element, false);
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

            if (armor_drain_level > 0)
            {
                Energy(menu, defender, weapon, armor_drain_level);
            }
        }

        public static bool ApplyStatus(Item weapon, string element)
        {
            int status_chance = 0;

            if (weapon != null)
            {
                for (int i = 0; i < weapon.Attachments.Count; i++)
                {
                    Item rune = weapon.Attachments[i];

                    if (rune.Categories.Contains(element))
                    {
                        Item paired_rune = InventoryUtil.GetPairedRune(weapon, rune);
                        if (paired_rune != null &&
                            paired_rune.Categories[0] == "Effect")
                        {
                            Something level = paired_rune.GetProperty("Level Value");
                            if (level != null)
                            {
                                status_chance += (int)level.Value * 10;
                            }
                        }
                    }
                }
            }

            if (status_chance > 0 &&
                Main.Game.Debugging)
            {
                return true;
            }

            if (Utility.RandomPercent(status_chance))
            {
                return true;
            }

            return false;
        }

        public static bool ResistStatus(Item armor, string element)
        {
            int resist_chance = 0;

            if (armor != null)
            {
                for (int i = 0; i < armor.Attachments.Count; i++)
                {
                    Item rune = armor.Attachments[i];

                    if (rune.Categories.Contains(element))
                    {
                        Item paired_rune = InventoryUtil.GetPairedRune(armor, rune);
                        if (paired_rune != null &&
                            paired_rune.Categories[0] == "Effect")
                        {
                            Something level = paired_rune.GetProperty("Level Value");
                            if (level != null)
                            {
                                resist_chance += (int)level.Value * 10;
                            }
                        }
                    }
                }
            }

            if (Utility.RandomPercent(resist_chance))
            {
                return true;
            }

            return false;
        }

        public static void StatusEffect(Menu menu, Character defender, Item weapon, string element, bool defense_only)
        {
            if (weapon != null)
            {
                for (int i = 0; i < weapon.Attachments.Count; i++)
                {
                    Item rune = weapon.Attachments[i];
                    if (rune.Categories[0] == element)
                    {
                        bool apply = false;
                        if (defense_only)
                        {
                            if (element == "Health" ||
                                element == "Energy")
                            {
                                apply = true;
                            }
                        }
                        else
                        {
                            if (element != "Health" &&
                                element != "Energy")
                            {
                                apply = true;
                            }
                        }

                        if (apply)
                        {
                            if (ApplyStatus(weapon, element))
                            {
                                bool resisted = false;

                                Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
                                if (helm != null)
                                {
                                    resisted = ResistStatus(helm, element);
                                }

                                Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
                                if (armor != null)
                                {
                                    resisted = ResistStatus(armor, element);
                                }

                                Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
                                if (shield != null)
                                {
                                    resisted = ResistStatus(shield, element);
                                }

                                if (!resisted)
                                {
                                    Something statusEffect = null;
                                    bool found = false;

                                    if (rune.Categories.Contains("Counter"))
                                    {
                                        #region Weak

                                        statusEffect = new Something
                                        {
                                            Name = "Weak",
                                            Amount = 1
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 1;
                                                if (existing.Amount > 3)
                                                {
                                                    existing.Amount = 3;
                                                }

                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Death"))
                                    {
                                        #region Cursed

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == "Cursed")
                                            {
                                                found = true;
                                                break;
                                            }
                                        }

                                        if (!found)
                                        {
                                            statusEffect = new Something
                                            {
                                                Name = "Cursed"
                                            };
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Disarm"))
                                    {
                                        #region Melting

                                        statusEffect = new Something
                                        {
                                            Name = "Melting",
                                            Amount = 1
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 1;
                                                if (existing.Amount > 3)
                                                {
                                                    existing.Amount = 3;
                                                }

                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Drain"))
                                    {
                                        #region Poisoned

                                        statusEffect = new Something
                                        {
                                            Name = "Poisoned",
                                            Value = 2
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Value += 2;
                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Earth"))
                                    {
                                        #region Petrified

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == "Petrified")
                                            {
                                                found = true;
                                                break;
                                            }
                                        }

                                        if (!found)
                                        {
                                            statusEffect = new Something
                                            {
                                                Name = "Petrified"
                                            };
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Fire"))
                                    {
                                        #region Burning

                                        statusEffect = new Something
                                        {
                                            Name = "Burning",
                                            Amount = 2,
                                            Value = 4
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 2;
                                                existing.Value += 4;
                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Health"))
                                    {
                                        #region Regenerating

                                        statusEffect = new Something
                                        {
                                            Name = "Regenerating",
                                            Amount = 5,
                                            Value = 20
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 5;
                                                if (existing.Amount > 5)
                                                {
                                                    existing.Amount = 5;
                                                }

                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Energy"))
                                    {
                                        #region Charging

                                        statusEffect = new Something
                                        {
                                            Name = "Charging",
                                            Amount = 5,
                                            Value = 20
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 5;
                                                if (existing.Amount > 5)
                                                {
                                                    existing.Amount = 5;
                                                }

                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Physical"))
                                    {
                                        #region Stunned

                                        statusEffect = new Something
                                        {
                                            Name = "Stunned",
                                            Amount = 1
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 1;
                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Time"))
                                    {
                                        #region Slow

                                        statusEffect = new Something
                                        {
                                            Name = "Slow",
                                            Amount = 1
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 1;
                                                if (existing.Amount > 2)
                                                {
                                                    existing.Amount = 2;
                                                }

                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Ice"))
                                    {
                                        #region Frozen

                                        statusEffect = new Something
                                        {
                                            Name = "Frozen",
                                            Amount = 4,
                                            Value = 5
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 4;
                                                if (existing.Amount > 4)
                                                {
                                                    existing.Amount = 4;
                                                }

                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Lightning"))
                                    {
                                        #region Shocked

                                        statusEffect = new Something
                                        {
                                            Name = "Shocked",
                                            Amount = 2,
                                            Value = 10
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;

                                                existing.Amount += 2;
                                                if (existing.Amount > 2)
                                                {
                                                    existing.Amount = 2;
                                                }

                                                break;
                                            }
                                        }

                                        #endregion
                                    }
                                    else if (rune.Categories.Contains("Diamond"))
                                    {
                                        #region Radiating

                                        statusEffect = new Something
                                        {
                                            Name = "Radiating",
                                            Value = 2
                                        };

                                        foreach (Something existing in defender.StatusEffects)
                                        {
                                            if (existing.Name == statusEffect.Name)
                                            {
                                                found = true;
                                                existing.Value += 2;
                                                break;
                                            }
                                        }

                                        #endregion
                                    }

                                    if (statusEffect != null)
                                    {
                                        if (!found)
                                        {
                                            defender.StatusEffects.Add(statusEffect);
                                        }

                                        Color damage_color = GameUtil.Get_EffectColor(statusEffect.Name);
                                        AddCombatLabel(menu, defender, "+" + statusEffect.Name, damage_color);
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
            heal += Area_AllArmor_PairedLevel(character.Squad, character, element);

            //Check for stat extra healing
            if (weapon.Categories[0] == "Grimoire")
            {
                heal += (int)character.GetStat("INT").Value;
            }

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
            energy += Area_AllArmor_PairedLevel(character.Squad, character, element);

            //Check for stat extra energy
            if (weapon.Categories[0] == "Grimoire")
            {
                energy += (int)character.GetStat("INT").Value;
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

                menu.AddLabel(AssetManager.Fonts["ControlFont"], character.ID, "Damage", "+" + energy.ToString(), new Color(255, 255, 0, 255),
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
            AddCombatLabel(menu, defender, "Dodged!", new Color(255, 140, 20, 255));

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
                    AddCombatLabel(menu, attacker, "Again!", new Color(255, 140, 20, 255));
                    return true;
                }
            }

            return false;
        }

        public static int DiamondChance(Item item)
        {
            int total = 0;

            if (item != null)
            {
                for (int i = 0; i < item.Attachments.Count; i++)
                {
                    Item rune = item.Attachments[i];

                    if (rune.Categories.Contains("Diamond"))
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

        public static bool DiamondChance_AllArmor(Menu menu, Character defender)
        {
            int chance = 0;

            Item helm = InventoryUtil.Get_EquippedItem(defender, "Helm");
            if (helm != null)
            {
                chance += DiamondChance(helm);
            }

            Item armor = InventoryUtil.Get_EquippedItem(defender, "Armor");
            if (armor != null)
            {
                chance += DiamondChance(armor);
            }

            Item shield = InventoryUtil.Get_EquippedItem(defender, "Shield");
            if (shield != null)
            {
                chance += DiamondChance(shield);
            }

            if (Utility.RandomPercent(chance))
            {
                AddCombatLabel(menu, defender, "Diamond Resist!", new Color(200, 191, 231, 255));
                return true;
            }

            return false;
        }

        public static bool DiamondChance_Attack(Menu menu, Character attacker)
        {
            Item weapon = InventoryUtil.Get_EquippedItem(attacker, "Weapon");
            if (InventoryUtil.Item_HasElement(weapon, "Diamond"))
            {
                int chance = InventoryUtil.Get_Item_Element_Level(weapon, "Diamond") * 10;

                if (chance > 0 &&
                    Main.Game.Debugging)
                {
                    chance = 100;
                }

                if (Utility.RandomPercent(chance))
                {
                    AddCombatLabel(menu, attacker, "Diamond Boost!", new Color(200, 191, 231, 255));
                    return true;
                }
            }

            return false;
        }

        public static void AddCombatLabel(Menu menu, Character character, string text, Color text_color)
        {
            menu.AddLabel(AssetManager.Fonts["ControlFont"], character.ID, "Damage", text, text_color,
                new Region(character.HealthBar.Base_Region.X - (character.HealthBar.Base_Region.Width / 4),
                           character.Region.Y - ((character.HealthBar.Base_Region.Width / 8) * 3),
                           character.HealthBar.Base_Region.Width + (character.HealthBar.Base_Region.Width / 2),
                           Main.Game.MenuSize_Y), false);
        }

        public static void Increase_RP(Item rune, int amount)
        {
            Something rp = rune.GetProperty("RP Value");
            Something level = rune.GetProperty("Level Value");

            if (level != null &&
                rp != null)
            {
                if (level.Value < level.Max_Value)
                {
                    for (int i = 1; i <= amount; i++)
                    {
                        rp.Value++;

                        //Do we have enough XP to reach the next level?
                        if (rp.Value >= rp.Max_Value)
                        {
                            Increase_Level(rune);

                            if (level.Value == level.Max_Value)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void Increase_Level(Item rune)
        {
            Something rp = rune.GetProperty("RP Value");
            if (rp != null)
            {
                rp.Value = 0;
            }

            Something level = rune.GetProperty("Level Value");
            if (level != null)
            {
                level.Value++;

                if (level.Value == level.Max_Value)
                {
                    rp.Value = rp.Max_Value;
                }

                UpdateRune_Description(rune);
            }
        }

        public static void UpdateRune_Description(Item rune)
        {
            if (rune.Categories.Count > 0)
            {
                string type = rune.Categories[0];

                Something level = rune.GetProperty("Level Value");
                if (level != null)
                {
                    switch (type)
                    {
                        case "Area":
                            rune.Description = "On Weapon: " + (level.Value * 10) + "% chance for paired On Weapon on whole squad\nOn Armor: " + (level.Value * 10) + "% chance for paired On Armor on whole squad\nStatus: None";
                            break;

                        case "Counter":
                            rune.Description = "On Weapon: " + (level.Value * 10) + "% chance to ignore defenses\nOn Armor: " + (level.Value * 10) + "% chance to counter attack\nStatus: Weak";
                            break;

                        case "Death":
                            rune.Description = "On Weapon: " + level.Value + "% chance to instant kill target\nOn Armor: " + (level.Value * 10) + "% chance to resist instant kill\nStatus: Cursed";
                            break;

                        case "Disarm":
                            rune.Description = "On Weapon: " + level.Value + "% chance to disarm target's weapon\nOn Armor: " + level.Value + "% chance to disarm attacker's weapon when hit\nStatus: Melting";
                            break;

                        case "Drain":
                            rune.Description = "On Weapon: " + (level.Value * 10) + "% chance to restore HP by paired damage\nOn Armor: " + (level.Value * 10) + "% chance to restore EP by paired resistance\nStatus: Poisoned";
                            break;

                        case "Earth":
                            rune.Description = "On Weapon: inflict " + (level.Value * Handler.Element_Multiplier) + " Earth damage\nOn Armor: resist " + (level.Value * Handler.Element_Multiplier) + " Earth damage\nStatus: Petrified";
                            break;

                        case "Effect":
                            rune.Description = "On Weapon: " + (level.Value * 10) + "% chance to apply paired Status\nOn Armor: " + (level.Value * 10) + "% chance to resist paired Status\nStatus: None";
                            break;

                        case "Fire":
                            rune.Description = "On Weapon: inflict " + (level.Value * Handler.Element_Multiplier) + " Fire damage\nOn Armor: resist " + (level.Value * Handler.Element_Multiplier) + " Fire damage\nStatus: Burning";
                            break;

                        case "Health":
                            rune.Description = "On Weapon: restore " + (level.Value * Handler.Element_Multiplier) + " HP\nOn Armor: restore extra " + (level.Value * Handler.Element_Multiplier) + " HP\nStatus: Regenerating";
                            break;

                        case "Energy":
                            rune.Description = "On Weapon: restore " + (level.Value * Handler.Element_Multiplier) + " EP\nOn Armor: restore extra " + (level.Value * Handler.Element_Multiplier) + " EP\nStatus: Charging";
                            break;

                        case "Physical":
                            rune.Description = "On Weapon: inflict " + (level.Value * Handler.Element_Multiplier) + " Physical damage\nOn Armor: resist " + (level.Value * Handler.Element_Multiplier) + " Physical damage\nStatus: Stunned";
                            break;

                        case "Time":
                            rune.Description = "On Weapon: " + level.Value + "% chance to attack again\nOn Armor: " + level.Value + "% chance to dodge attack\nStatus: Slow";
                            break;

                        case "Ice":
                            rune.Description = "On Weapon: inflict " + (level.Value * Handler.Element_Multiplier) + " Ice damage\nOn Armor: resist " + (level.Value * Handler.Element_Multiplier) + " Ice damage\nStatus: Frozen";
                            break;

                        case "Lightning":
                            rune.Description = "On Weapon: inflict " + (level.Value * Handler.Element_Multiplier) + " Lightning damage\nOn Armor: resist " + (level.Value * Handler.Element_Multiplier) + " Lightning damage\nStatus: Shocked";
                            break;

                        case "Diamond":
                            rune.Description = "On Weapon: " + (level.Value * 10) + "% chance to inflict extra half all damage\nOn Armor: " + (level.Value * 10) + "% chance to resist half all damage\nStatus: Radiating";
                            break;
                    }
                }
            }
        }
    }
}
