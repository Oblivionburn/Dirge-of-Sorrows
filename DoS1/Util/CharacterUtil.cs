using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inputs;
using OP_Engine.Inventories;
using OP_Engine.Menus;
using OP_Engine.Utility;
using OP_Engine.Enums;

namespace DoS1.Util
{
    public static class CharacterUtil
    {
        public static Character NewCharacter(string name, Vector2 formation, string type, Direction direction, string hairStyle, string hairColor, string headStyle, string eyeColor, string skinColor, string gender)
        {
            Character character = new Character
            {
                ID = Handler.GetID(),
                Name = name,
                Gender = gender,
                Level = 1,
                Formation = new Vector2(formation.X, formation.Y),
                Type = type,
                Direction = direction,
                Region = new Region(),
                Texture = Handler.GetTexture(direction.ToString() + "_Body_" + skinColor + "_Idle")
            };
            character.Animator.Frames = 4;
            character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height);
            character.Inventory.Name = name;

            character.Stats.Add(new Something { Name = "STR", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "INT", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "DEX", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "AGI", Value = 10, Max_Value = 100 });

            character.Inventory.Items.Add(new Item
            {
                ID = Handler.GetID(),
                Name = "Head",
                Type = "Head",
                Location = new Location(),
                Equipped = true,
                Texture = Handler.GetTexture(character.Direction.ToString() + "_" + skinColor + "_" + gender + "_" + headStyle),
                Image = character.Image,
                DrawColor = Color.White,
                Visible = true
            });

            character.Inventory.Items.Add(new Item
            {
                ID = Handler.GetID(),
                Name = "Eyes",
                Type = "Eyes",
                Location = new Location(),
                Equipped = true,
                Texture = Handler.GetTexture(character.Direction.ToString() + "_Eye_" + eyeColor),
                Image = character.Image,
                DrawColor = Color.White,
                Visible = true
            });

            if (hairStyle != "Bald")
            {
                character.Inventory.Items.Add(new Item
                {
                    ID = Handler.GetID(),
                    Name = "Hair",
                    Type = "Hair",
                    Location = new Location(),
                    Equipped = true,
                    DrawColor = Color.White,
                    Texture = Handler.GetTexture(character.Direction.ToString() + "_" + gender + "_" + hairStyle + "_" + hairColor),
                    Image = character.Image,
                    Visible = true
                });
            }

            character.HealthBar.Base_Texture = AssetManager.Textures["ProgressBase"];
            character.HealthBar.Bar_Texture = AssetManager.Textures["ProgressBar"];
            character.HealthBar.Bar_Image = new Rectangle(0, 0, 0, character.HealthBar.Base_Texture.Height);
            character.HealthBar.DrawColor = Color.Red;
            character.HealthBar.Max_Value = 100;
            character.HealthBar.Value = 100;
            character.HealthBar.Update();

            character.ManaBar.Base_Texture = AssetManager.Textures["ProgressBase"];
            character.ManaBar.Bar_Texture = AssetManager.Textures["ProgressBar"];
            character.ManaBar.Bar_Image = new Rectangle(0, 0, 0, character.HealthBar.Base_Texture.Height);
            character.ManaBar.DrawColor = Color.Blue;
            character.ManaBar.Max_Value = 100;
            character.ManaBar.Value = 100;
            character.ManaBar.Update();

            return character;
        }

        public static Character NewCharacter_Random(Vector2 formation, bool enemy, int gender)
        {
            CryptoRandom random = new CryptoRandom();

            string name;
            if (gender == 0)
            {
                name = CharacterManager.FirstNames_Male[random.Next(0, CharacterManager.FirstNames_Male.Count)];
            }
            else
            {
                name = CharacterManager.FirstNames_Female[random.Next(0, CharacterManager.FirstNames_Female.Count)];
            }

            string headStyle;
            if (gender == 0)
            {
                headStyle = Handler.HeadStyles_Male[random.Next(0, Handler.HeadStyles_Male.Length)];
            }
            else
            {
                headStyle = Handler.HeadStyles_Female[random.Next(0, Handler.HeadStyles_Female.Length)];
            }

            string hairStyle;
            if (gender == 0)
            {
                hairStyle = Handler.HairStyles_Male[random.Next(0, Handler.HairStyles_Male.Length)];
            }
            else
            {
                hairStyle = Handler.HairStyles_Female[random.Next(0, Handler.HairStyles_Female.Length)];
            }

            string hairColor = Handler.HairColors[random.Next(0, Handler.HairColors.Length)];
            string eyeColor = Handler.EyeColors[random.Next(0, Handler.EyeColors.Length)];
            string skinColor = Handler.SkinTones[random.Next(0, Handler.SkinTones.Length)];

            return NewCharacter(name, formation, enemy ? "Enemy" : "Ally", enemy ? Direction.Right : Direction.Left, hairStyle, 
                hairColor, headStyle, eyeColor, skinColor, gender == 0 ? "Male" : "Female");
        }

        public static void DrawCharacter(SpriteBatch spriteBatch, Character character, Color color)
        {
            if (character != null &&
                !character.Dead)
            {
                Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
                if (shield != null)
                {
                    spriteBatch.Draw(shield.Texture, shield.Region.ToRectangle, shield.Image, color);
                    if (shield.Icon_Visible &&
                        shield.Icon != null &&
                        shield.Icon_Region != null)
                    {
                        spriteBatch.Draw(shield.Icon, shield.Icon_Region.ToRectangle, shield.Icon_Image, Color.White);
                    }
                }

                //Draw body
                spriteBatch.Draw(character.Texture, character.Region.ToRectangle, character.Image, color);

                Item head = InventoryUtil.Get_EquippedItem(character, "Head");
                if (head != null)
                {
                    spriteBatch.Draw(head.Texture, head.Region.ToRectangle, head.Image, color);
                }

                Item eyes = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (eyes != null)
                {
                    if (eyes.Visible)
                    {
                        spriteBatch.Draw(eyes.Texture, eyes.Region.ToRectangle, eyes.Image, color);
                    }
                    else
                    {
                        string[] parts = character.Texture.Name.Split('_');
                        string direction = parts[0];
                        string skin_tone = parts[2];
                        string closed_eye = direction + "_Eye_Closed_" + skin_tone;

                        spriteBatch.Draw(Handler.GetTexture(closed_eye), eyes.Region.ToRectangle, eyes.Image, color);
                    }
                }

                Item hair = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (hair != null)
                {
                    spriteBatch.Draw(hair.Texture, hair.Region.ToRectangle, hair.Image, color);
                }

                Item beard = InventoryUtil.Get_EquippedItem(character, "Beard");
                if (beard != null)
                {
                    spriteBatch.Draw(beard.Texture, beard.Region.ToRectangle, beard.Image, color);
                }

                Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (helm != null)
                {
                    spriteBatch.Draw(helm.Texture, helm.Region.ToRectangle, helm.Image, color);
                    if (helm.Icon_Visible &&
                        helm.Icon != null &&
                        helm.Icon_Region != null)
                    {
                        spriteBatch.Draw(helm.Icon, helm.Icon_Region.ToRectangle, helm.Icon_Image, Color.White);
                    }
                }

                Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (armor != null)
                {
                    spriteBatch.Draw(armor.Texture, armor.Region.ToRectangle, armor.Image, color);
                    if (armor.Icon_Visible &&
                        armor.Icon != null &&
                        armor.Icon_Region != null)
                    {
                        spriteBatch.Draw(armor.Icon, armor.Icon_Region.ToRectangle, armor.Icon_Image, Color.White);
                    }
                }

                Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (weapon != null)
                {
                    spriteBatch.Draw(weapon.Texture, weapon.Region.ToRectangle, weapon.Image, color);
                    if (weapon.Icon_Visible &&
                        weapon.Icon != null &&
                        weapon.Icon_Region != null)
                    {
                        spriteBatch.Draw(weapon.Icon, weapon.Icon_Region.ToRectangle, weapon.Icon_Image, Color.White);
                    }
                }

                if (character.HealthBar.Visible)
                {
                    character.HealthBar.Draw(spriteBatch);
                }

                if (character.ManaBar.Visible)
                {
                    character.ManaBar.Draw(spriteBatch);
                }
            }
        }

        public static void DrawCharacter_Combat(SpriteBatch spriteBatch, Character character, Color color)
        {
            if (character != null &&
                !character.Dead)
            {
                Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
                if (shield != null)
                {
                    spriteBatch.Draw(shield.Texture, shield.Region.ToRectangle, shield.Image, color);
                }

                //Draw body
                spriteBatch.Draw(character.Texture, character.Region.ToRectangle, character.Image, color);

                Item head = InventoryUtil.Get_EquippedItem(character, "Head");
                if (head != null)
                {
                    spriteBatch.Draw(head.Texture, head.Region.ToRectangle, head.Image, color);
                }

                Item eyes = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (eyes != null)
                {
                    if (eyes.Visible)
                    {
                        spriteBatch.Draw(eyes.Texture, eyes.Region.ToRectangle, eyes.Image, color);
                    }
                    else
                    {
                        string[] parts = character.Texture.Name.Split('_');
                        string direction = parts[0];
                        string skin_tone = parts[2];
                        string closed_eye = direction + "_Eye_Closed_" + skin_tone;

                        spriteBatch.Draw(Handler.GetTexture(closed_eye), eyes.Region.ToRectangle, eyes.Image, color);
                    }
                }

                Item hair = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (hair != null)
                {
                    spriteBatch.Draw(hair.Texture, hair.Region.ToRectangle, hair.Image, color);
                }

                Item beard = InventoryUtil.Get_EquippedItem(character, "Beard");
                if (beard != null)
                {
                    spriteBatch.Draw(beard.Texture, beard.Region.ToRectangle, beard.Image, color);
                }

                Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (helm != null)
                {
                    spriteBatch.Draw(helm.Texture, helm.Region.ToRectangle, helm.Image, color);
                }

                Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (armor != null)
                {
                    spriteBatch.Draw(armor.Texture, armor.Region.ToRectangle, armor.Image, color);
                }

                Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (weapon != null)
                {
                    spriteBatch.Draw(weapon.Texture, weapon.Region.ToRectangle, weapon.Image, color);
                }

                if (character.HealthBar.Visible)
                {
                    character.HealthBar.Draw(spriteBatch);
                }

                if (character.ManaBar.Visible)
                {
                    character.ManaBar.Draw(spriteBatch);
                }
            }
        }

        public static void DrawCharacter_Grayscale(SpriteBatch spriteBatch, Character character, Color color)
        {
            if (character != null &&
                !character.Dead)
            {
                spriteBatch.End();

                Effect effect = AssetManager.Shaders["Grayscale"];
                effect.Parameters["percent"].SetValue(0f);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, effect, null);

                //Draw shield
                Item shield = InventoryUtil.Get_EquippedItem(character, "Shield");
                if (shield != null)
                {
                    spriteBatch.Draw(shield.Texture, shield.Region.ToRectangle, shield.Image, color);
                }

                //Draw body
                spriteBatch.Draw(character.Texture, character.Region.ToRectangle, character.Image, color);

                Item head = InventoryUtil.Get_EquippedItem(character, "Head");
                if (head != null)
                {
                    spriteBatch.Draw(head.Texture, head.Region.ToRectangle, head.Image, color);
                }

                Item eyes = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (eyes != null)
                {
                    spriteBatch.Draw(eyes.Texture, eyes.Region.ToRectangle, eyes.Image, color);
                }

                Item hair = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (hair != null)
                {
                    spriteBatch.Draw(hair.Texture, hair.Region.ToRectangle, hair.Image, color);
                }

                Item beard = InventoryUtil.Get_EquippedItem(character, "Beard");
                if (beard != null)
                {
                    spriteBatch.Draw(beard.Texture, beard.Region.ToRectangle, beard.Image, color);
                }

                Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (helm != null)
                {
                    spriteBatch.Draw(helm.Texture, helm.Region.ToRectangle, helm.Image, color);
                }

                Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (armor != null)
                {
                    spriteBatch.Draw(armor.Texture, armor.Region.ToRectangle, armor.Image, color);
                }

                Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (weapon != null)
                {
                    spriteBatch.Draw(weapon.Texture, weapon.Region.ToRectangle, weapon.Image, color);
                }

                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

                if (character.HealthBar.Visible)
                {
                    character.HealthBar.Draw(spriteBatch);
                }

                if (character.ManaBar.Visible)
                {
                    character.ManaBar.Draw(spriteBatch);
                }
            }
        }

        public static void DrawCharacter_Portrait(SpriteBatch spriteBatch, Picture portraitBox, Character character)
        {
            if (character != null)
            {
                int x = 0;
                int width = 0;
                int height = 0;

                int margin = (int)(portraitBox.Region.Width * 3.125f / 100);
                Rectangle region = new Rectangle((int)portraitBox.Region.X + margin, (int)portraitBox.Region.Y + margin, (int)portraitBox.Region.Width - (margin * 2), (int)portraitBox.Region.Height - (margin * 2));

                Item head = InventoryUtil.Get_EquippedItem(character, "Head");
                if (head != null)
                {
                    x = (head.Texture.Width / 32) * 3;
                    width = head.Texture.Width / 16;
                    height = head.Texture.Height / 4;

                    spriteBatch.Draw(head.Texture, region, new Rectangle(x, 0, width, height), Color.White);
                }

                Item eyes = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (eyes != null)
                {
                    spriteBatch.Draw(eyes.Texture, region, new Rectangle(x, 0, width, height), Color.White);
                }

                Item hair = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (hair != null)
                {
                    spriteBatch.Draw(hair.Texture, region, new Rectangle(x, 0, width, height), Color.White);
                }

                Item beard = InventoryUtil.Get_EquippedItem(character, "Beard");
                if (beard != null)
                {
                    spriteBatch.Draw(beard.Texture, region, new Rectangle(x, 0, width, height), Color.White);
                }

                Item helm = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (helm != null)
                {
                    spriteBatch.Draw(helm.Texture, region, new Rectangle(x, 0, width, height), Color.White);
                }
            }
        }

        public static void DrawSquad(SpriteBatch spriteBatch, Squad squad, Color color)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Character character = squad.GetCharacter(new Vector2(x, y));
                    if (character != null)
                    {
                        DrawCharacter(spriteBatch, character, color);
                    }
                }
            }
        }

        public static void ResizeBars(Character character)
        {
            float bar_x = character.Region.X + (character.Region.Width / 8);
            float bar_width = (character.Region.Width / 8) * 6;
            float bar_height = character.Region.Width / 16;

            character.HealthBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height, bar_width, bar_height);
            character.HealthBar.Visible = true;
            character.HealthBar.Update();

            character.ManaBar.Base_Region = new Region(bar_x, character.Region.Y + character.Region.Height + bar_height, bar_width, bar_height);
            character.ManaBar.Visible = true;
            character.ManaBar.Update();
        }

        public static void Animate(Character character)
        {
            character.Animator.Animate(character);
            UpdateGear(character);
        }

        public static void AnimateIdle(Character character)
        {
            if (Utility.RandomPercent(2))
            {
                Animate(character);
            }

            Item eyes = InventoryUtil.Get_EquippedItem(character, "Eyes");
            if (eyes.Visible)
            {
                CryptoRandom random = new CryptoRandom();
                int num = random.Next(0, 151);
                if (num <= 0)
                {
                    eyes.Visible = false;
                }
            }
            else if (Utility.RandomPercent(10))
            {
                eyes.Visible = true;
            }
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

                string body_texture = direction + "_" + category + "_" + skin_tone + "_" + type;

                character.Texture = Handler.GetTexture(body_texture);
                if (character.Texture != null)
                {
                    Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                    if (armor != null &&
                        armor.Categories != null &&
                        armor.Materials != null)
                    {
                        string armor_texture = direction + "_Armor_" + armor.Categories[0] + "_" + armor.Materials[0] + "_" + type;
                        armor.Texture = Handler.GetTexture(armor_texture);
                    }

                    Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (weapon != null &&
                        weapon.Categories != null &&
                        weapon.Materials != null)
                    {
                        string weapon_texture = direction + "_Weapon_" + weapon.Categories[0] + "_" + weapon.Materials[0] + "_" + type;
                        weapon.Texture = Handler.GetTexture(weapon_texture);
                    }

                    ResetAnimation(character);
                }
            }
        }

        public static void ResetAnimation(Character character)
        {
            character.Animator.Reset(character);
            UpdateGear(character);
        }

        public static void UpdateGear(Character character)
        {
            foreach (Item item in character.Inventory.Items)
            {
                if (item.Texture != null)
                {
                    item.Image = character.Image;
                    item.Region = character.Region;
                }
            }
        }

        public static string AttackType(Character character)
        {
            Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
            if (weapon != null)
            {
                if (weapon.Categories.Contains("Grimoire"))
                {
                    return "Cast";
                }
                else if (weapon.Categories.Contains("Bow"))
                {
                    return "Ranged";
                }
            }

            return "Attack";
        }

        public static void ExamineCharacter(Menu menu, Character character)
        {
            int width = Main.Game.MenuSize.X * 4;
            int height = Main.Game.MenuSize.X;

            Label examine = menu.GetLabel("Examine");
            examine.Text = "";

            List<string> lines = new List<string>
            {
                character.Name,
                "",
                "HP: " + character.HealthBar.Value + "/" + character.HealthBar.Max_Value,
                "EP: " + character.ManaBar.Value + "/" + character.ManaBar.Max_Value
            };

            List<Something> statusEffects = new List<Something>();
            for (int i = 0; i < character.StatusEffects.Count; i++)
            {
                Something statusEffect = character.StatusEffects[i];
                if (statusEffect.Name != "Damage")
                {
                    statusEffects.Add(statusEffect);
                }
            }

            if (statusEffects.Count > 0)
            {
                lines.Add("");
                lines.Add("Status Effects:");

                for (int i = 0; i < statusEffects.Count; i++)
                {
                    lines.Add("- " + statusEffects[i].Name);
                }
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                examine.Text += line;

                if (i < lines.Count - 1)
                {
                    examine.Text += "\n";
                    height += (Main.Game.MenuSize.Y / 2);
                }
            }

            int X = InputManager.Mouse.X - (width / 2);
            if (X < 0)
            {
                X = 0;
            }
            else if (X > Main.Game.Resolution.X - width)
            {
                X = Main.Game.Resolution.X - width;
            }

            int Y = InputManager.Mouse.Y + 20;
            if (Y < 0)
            {
                Y = 0;
            }
            else if (Y > Main.Game.Resolution.Y - height)
            {
                Y = Main.Game.Resolution.Y - height;
            }

            examine.Region = new Region(X, Y, width, height);
            examine.Visible = true;
        }

        public static bool IsImmobilized(Character character)
        {
            if (character.Dead)
            {
                return true;
            }

            foreach (Something statusEffect in character.StatusEffects)
            {
                if (statusEffect.Name == "Petrified" ||
                    statusEffect.Name == "Stunned" ||
                    statusEffect.Name == "Frozen" ||
                    statusEffect.Name == "Shocked")
                {
                    return true;
                }
            }

            return false;
        }

        public static int Increase_XP(Character character, int xp)
        {
            int levels_gained = 0;

            int max_level = 100;

            if (character.Level < max_level)
            {
                for (int i = 1; i <= xp; i++)
                {
                    character.XP++;

                    //Do we have enough XP to reach the next level?
                    if (character.XP >= Handler.XP_Needed_ForLevels[character.Level + 1])
                    {
                        levels_gained++;
                        Increase_Level(character);

                        if (character.Level == max_level)
                        {
                            break;
                        }
                    }
                }
            }

            return levels_gained;
        }

        public static void Increase_Level(Character character)
        {
            CryptoRandom random;

            character.XP = 0;
            character.Level++;

            character.HealthBar.Max_Value += 20;

            character.HealthBar.Value += 20;
            if (character.HealthBar.Value > character.HealthBar.Max_Value)
            {
                character.HealthBar.Value = character.HealthBar.Max_Value;
            }

            character.HealthBar.Update();

            Something STR = character.GetStat("STR");
            Something INT = character.GetStat("INT");
            Something DEX = character.GetStat("DEX");
            Something AGI = character.GetStat("AGI");

            //Increase stat
            random = new CryptoRandom();
            int choice = random.Next(0, 3);
            if (choice == 0)
            {
                //Increase damage
                Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (weapon != null)
                {
                    if (InventoryUtil.Weapon_IsMelee(weapon) ||
                        weapon.Categories[0] == "Bow")
                    {
                        STR.Value++;
                        if (STR.Value > STR.Max_Value)
                        {
                            STR.Value = STR.Max_Value;
                        }
                    }
                    else if (weapon.Categories[0] == "Grimoire")
                    {
                        INT.Value++;
                        if (INT.Value > INT.Max_Value)
                        {
                            INT.Value = INT.Max_Value;
                        }
                    }
                }
                else
                {
                    STR.Value++;
                    if (STR.Value > STR.Max_Value)
                    {
                        STR.Value = STR.Max_Value;
                    }
                }
            }
            else if (choice == 1)
            {
                //Increase accuracy
                Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (weapon != null)
                {
                    if (InventoryUtil.Weapon_IsMelee(weapon) ||
                        weapon.Categories[0] == "Bow")
                    {
                        DEX.Value++;
                        if (DEX.Value > DEX.Max_Value)
                        {
                            DEX.Value = DEX.Max_Value;
                        }
                    }
                    else if (weapon.Categories[0] == "Grimoire")
                    {
                        //Accuracy doesn't matter for magic, so increase damage
                        INT.Value++;
                        if (INT.Value > INT.Max_Value)
                        {
                            INT.Value = INT.Max_Value;
                        }
                    }
                }
                else
                {
                    DEX.Value++;
                    if (DEX.Value > DEX.Max_Value)
                    {
                        DEX.Value = DEX.Max_Value;
                    }
                }
            }
            else if (choice == 2)
            {
                //Increase dodge chance
                AGI.Value++;
                if (AGI.Value > AGI.Max_Value)
                {
                    AGI.Value = AGI.Max_Value;
                }
            }
        }
    }
}
