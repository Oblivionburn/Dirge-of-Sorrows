using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Characters;
using OP_Engine.Utility;
using System;

namespace DoS1.Util
{
    public static class CharacterUtil
    {
        public static Character NewCharacter(string name, Vector2 formation, string hairStyle, string hairColor, string headStyle, string eyeColor, string skinColor)
        {
            Character character = new Character();
            character.ID = Handler.GetID();
            character.Name = name;
            character.Level = 1;
            character.Animator.Frames = 4;
            character.Formation = new Vector2(formation.X, formation.Y);
            character.Type = "Ally";
            character.Direction = Direction.Left;
            character.Texture = AssetManager.Textures[character.Direction.ToString() + "_Body_" + skinColor + "_Idle"];
            character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height);

            int xp = 10;
            for (int i = 2; i <= 100; i++)
            {
                character.XP_Needed_ForLevels.Add(i, xp);
                xp += 2;
            }

            character.Stats.Add(new Something { Name = "STR", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "INT", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "DEX", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "AGI", Value = 10, Max_Value = 100 });

            character.Inventory.Name = name;

            //Add head
            Item item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Head";
            item.Type = "Head";
            item.Location = new Location();
            item.Equipped = true;
            item.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + skinColor + "_" + headStyle];
            item.Image = character.Image;
            item.DrawColor = Color.White;
            item.Visible = true;
            character.Inventory.Items.Add(item);

            //Add eyes
            item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Eyes";
            item.Type = "Eyes";
            item.Location = new Location();
            item.Equipped = true;
            item.Texture = AssetManager.Textures[character.Direction.ToString() + "_Eye"];
            item.Image = character.Image;
            item.DrawColor = Handler.EyeColors[eyeColor];
            item.Visible = true;
            character.Inventory.Items.Add(item);

            //Add hair
            if (hairStyle != "Bald")
            {
                item = new Item();
                item.ID = Handler.GetID();
                item.Name = "Hair";
                item.Type = "Hair";
                item.Location = new Location();
                item.Equipped = true;
                item.DrawColor = Handler.HairColors[hairColor];
                item.Texture = AssetManager.Textures[character.Direction.ToString() + "_" + hairStyle];
                item.Image = character.Image;
                item.Visible = true;
                character.Inventory.Items.Add(item);
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

        public static Character NewCharacter_Random(string name, Vector2 formation, bool enemy)
        {
            CryptoRandom random = new CryptoRandom();

            Character character = new Character();
            character.ID = Handler.GetID();
            character.Name = name;
            character.Level = 1;
            character.Animator.Frames = 4;
            character.Formation = new Vector2(formation.X, formation.Y);

            int xp = 10;
            for (int i = 2; i <= 100; i++)
            {
                character.XP_Needed_ForLevels.Add(i, xp);
                xp += 2;
            }

            character.Stats.Add(new Something { Name = "STR", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "INT", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "DEX", Value = 10, Max_Value = 100 });
            character.Stats.Add(new Something { Name = "AGI", Value = 10, Max_Value = 100 });

            if (enemy)
            {
                character.Type = "Enemy";
                character.Direction = Direction.Right;
            }
            else
            {
                character.Type = "Ally";
                character.Direction = Direction.Left;
            }

            string direction = character.Direction.ToString();
            string skin_tone = Handler.SkinTones[random.Next(0, Handler.SkinTones.Length)];
            string head_style = Handler.HeadStyles[random.Next(0, Handler.HeadStyles.Length)];
            Color eye_color = Handler.EyeColors.ElementAt(random.Next(0, Handler.EyeColors.Count)).Value;
            string hairStyle = Handler.HairStyles[random.Next(0, Handler.HairStyles.Length)];
            Color hair_color = Handler.HairColors.ElementAt(random.Next(0, 6)).Value;

            character.Texture = AssetManager.Textures[direction + "_Body_" + skin_tone + "_Idle"];
            character.Image = new Rectangle(0, 0, character.Texture.Width / 4, character.Texture.Height);

            character.Inventory.Name = name;

            //Add head
            Item item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Head";
            item.Type = "Head";
            item.Location = new Location();
            item.Equipped = true;
            item.Texture = AssetManager.Textures[direction + "_" + skin_tone + "_" + head_style];
            item.Image = character.Image;
            item.Visible = true;
            character.Inventory.Items.Add(item);

            //Add eyes
            item = new Item();
            item.ID = Handler.GetID();
            item.Name = "Eyes";
            item.Type = "Eyes";
            item.Location = new Location();
            item.Equipped = true;
            item.Texture = AssetManager.Textures[direction + "_Eye"];
            item.Image = character.Image;
            item.DrawColor = eye_color;
            item.Visible = true;
            character.Inventory.Items.Add(item);

            if (hairStyle != "Bald")
            {
                //Add hair
                item = new Item();
                item.ID = Handler.GetID();
                item.Name = "Hair";
                item.Type = "Hair";
                item.Location = new Location();
                item.Equipped = true;
                item.DrawColor = hair_color;
                item.Texture = AssetManager.Textures[direction + "_" + hairStyle];
                item.Image = character.Image;
                item.Visible = true;
                character.Inventory.Items.Add(item);
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

        public static void DrawCharacter(SpriteBatch spriteBatch, Character character, Color color)
        {
            if (character != null &&
                !character.Dead)
            {
                //Draw shield
                Item item = InventoryUtil.Get_EquippedItem(character, "Shield");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                }

                //Draw body
                spriteBatch.Draw(character.Texture, character.Region.ToRectangle, character.Image, color);

                item = InventoryUtil.Get_EquippedItem(character, "Head");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
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
                Effect effect = AssetManager.Shaders["Grayscale"];

                //Draw shield
                Item item = InventoryUtil.Get_EquippedItem(character, "Shield");
                if (item != null)
                {
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                //Draw body
                effect.Parameters["Texture1"].SetValue(character.Texture);
                spriteBatch.Draw(character.Texture, character.Region.ToRectangle, character.Image, color);
                effect.CurrentTechnique.Passes[0].Apply();

                item = InventoryUtil.Get_EquippedItem(character, "Head");
                if (item != null)
                {
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (item != null)
                {
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (item != null)
                {
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (item != null)
                {
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (item != null)
                {
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (item != null)
                {
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.CurrentTechnique.Passes[0].Apply();
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

        public static void DrawCharacter_Portrait(SpriteBatch spriteBatch, Picture portraitBox, Character character)
        {
            if (character != null &&
                !character.Dead)
            {
                int x = 0;
                int width = 0;
                int height = 0;

                int margin = (int)(portraitBox.Region.Width * 3.125f / 100);
                Rectangle region = new Rectangle((int)portraitBox.Region.X + margin, (int)portraitBox.Region.Y + margin, (int)portraitBox.Region.Width - (margin * 2), (int)portraitBox.Region.Height - (margin * 2));

                Item item = InventoryUtil.Get_EquippedItem(character, "Head");
                if (item != null)
                {
                    x = (item.Texture.Width / 32) * 3;
                    width = item.Texture.Width / 16;
                    height = item.Texture.Height / 4;

                    spriteBatch.Draw(item.Texture, region, new Rectangle(x, 0, width, height), Color.White);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (item != null)
                {
                    spriteBatch.Draw(item.Texture, region, new Rectangle(x, 0, width, height), item.DrawColor);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (item != null)
                {
                    spriteBatch.Draw(item.Texture, region, new Rectangle(x, 0, width, height), item.DrawColor);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (item != null)
                {
                    spriteBatch.Draw(item.Texture, region, new Rectangle(x, 0, width, height), Color.White);
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

        public static void Increase_XP(Character character, int xp)
        {
            int max_level = 100;

            if (character.Level < max_level)
            {
                for (int i = 1; i <= xp; i++)
                {
                    character.XP++;

                    //Do we have enough XP to reach the next level?
                    if (character.XP >= character.XP_Needed_ForLevels[character.Level + 1])
                    {
                        Increase_Level(character);

                        if (character.Level == max_level)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void Increase_Level(Character character)
        {
            CryptoRandom random;

            character.XP = 0;
            character.Level++;

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
                        character.GetStat("STR").IncreaseValue(1);
                    }
                    else if (weapon.Categories[0] == "Grimoire")
                    {
                        character.GetStat("INT").IncreaseValue(1);
                    }
                }
                else
                {
                    character.GetStat("STR").IncreaseValue(1);
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
                        character.GetStat("DEX").IncreaseValue(1);
                    }
                    else if (weapon.Categories[0] == "Grimoire")
                    {
                        //Accuracy doesn't matter for magic, so increase damage
                        character.GetStat("INT").IncreaseValue(1);
                    }
                }
                else
                {
                    character.GetStat("DEX").IncreaseValue(1);
                }
            }
            else if (choice == 2)
            {
                //Increase dodge chance
                character.GetStat("AGI").IncreaseValue(1);
            }
        }
    }
}
