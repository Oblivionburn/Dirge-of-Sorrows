using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Characters;
using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Utility;

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
                Texture = AssetManager.Textures[direction.ToString() + "_Body_" + skinColor + "_Idle"]
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
                Texture = AssetManager.Textures[character.Direction.ToString() + "_" + skinColor + "_" + gender + "_" + headStyle],
                Image = character.Image,
                DrawColor = Color.White,
                Visible = true
            });

            Texture2D eyeTexture = AssetManager.Textures[character.Direction.ToString() + "_Eye"];
            Texture2D newEyeTexture = GameUtil.CopyTexture_NewColor(eyeTexture, Handler.EyeColors[eyeColor]);

            character.Inventory.Items.Add(new Item
            {
                ID = Handler.GetID(),
                Name = "Eyes",
                Type = "Eyes",
                Location = new Location(),
                Equipped = true,
                Texture = newEyeTexture,
                Image = character.Image,
                DrawColor = Color.White,
                Visible = true
            });

            if (hairStyle != "Bald")
            {
                Texture2D hairTexture = AssetManager.Textures[character.Direction.ToString() + "_" + gender + "_" + hairStyle];
                Texture2D newHairTexture = GameUtil.CopyTexture_NewColor(hairTexture, Handler.HairColors[hairColor]);

                character.Inventory.Items.Add(new Item
                {
                    ID = Handler.GetID(),
                    Name = "Hair",
                    Type = "Hair",
                    Location = new Location(),
                    Equipped = true,
                    DrawColor = Color.White,
                    Texture = newHairTexture,
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

            string hairColor = Handler.HairColors.ElementAt(random.Next(0, Handler.HairColors.Count)).Key;
            string eyeColor = Handler.EyeColors.ElementAt(random.Next(0, Handler.EyeColors.Count)).Key;
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

                        spriteBatch.Draw(AssetManager.Textures[closed_eye], eyes.Region.ToRectangle, eyes.Image, color);
                    }
                }

                Item hair = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (hair != null)
                {
                    spriteBatch.Draw(hair.Texture, hair.Region.ToRectangle, hair.Image, color);
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

                        spriteBatch.Draw(AssetManager.Textures[closed_eye], eyes.Region.ToRectangle, eyes.Image, color);
                    }
                }

                Item hair = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (hair != null)
                {
                    spriteBatch.Draw(hair.Texture, hair.Region.ToRectangle, hair.Image, color);
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
                Effect effect = AssetManager.Shaders["Grayscale"];

                //Draw shield
                Item item = InventoryUtil.Get_EquippedItem(character, "Shield");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                //Draw body
                effect.Parameters["Texture1"].SetValue(character.Texture);
                spriteBatch.Draw(character.Texture, character.Region.ToRectangle, character.Image, color);
                effect.CurrentTechnique.Passes[0].Apply();

                item = InventoryUtil.Get_EquippedItem(character, "Head");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Eyes");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Helm");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Armor");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.Parameters["Texture1"].SetValue(item.Texture);
                    effect.CurrentTechnique.Passes[0].Apply();
                }

                item = InventoryUtil.Get_EquippedItem(character, "Weapon");
                if (item != null)
                {
                    item.Draw(spriteBatch, Main.Game.Resolution, color);
                    effect.Parameters["Texture1"].SetValue(item.Texture);
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
                    spriteBatch.Draw(item.Texture, region, new Rectangle(x, 0, width, height), Color.White);
                }

                item = InventoryUtil.Get_EquippedItem(character, "Hair");
                if (item != null)
                {
                    spriteBatch.Draw(item.Texture, region, new Rectangle(x, 0, width, height), Color.White);
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

        public static Color Get_EyeColor(Character character)
        {
            Texture2D texture = character.Inventory.GetItem("Eyes").Texture;

            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);

            int count = colors.Length;
            for (int i = 0; i < count; i++)
            {
                Color color = colors[i];
                if (color.R == 0 &&
                    color.G == 0 &&
                    color.B == 0 &&
                    color.A == 0)
                {
                    
                }
                else
                {
                    return color;
                }
            }

            return new Color(0, 0, 0, 0);
        }

        public static Color Get_EyeColor(Item eyes)
        {
            if (eyes != null)
            {
                Texture2D texture = eyes.Texture;

                Color[] colors = new Color[texture.Width * texture.Height];
                texture.GetData(colors);

                int count = colors.Length;
                for (int i = 0; i < count; i++)
                {
                    Color color = colors[i];
                    if (color.R == 0 &&
                        color.G == 0 &&
                        color.B == 0 &&
                        color.A == 0)
                    {

                    }
                    else
                    {
                        return color;
                    }
                }
            }

            return new Color(0, 0, 0, 0);
        }

        public static Color Get_HairColor(Character character)
        {
            Item hair = character.Inventory.GetItem("Hair");
            if (hair != null)
            {
                Texture2D texture = hair.Texture;

                Color[] colors = new Color[texture.Width * texture.Height];
                texture.GetData(colors);

                int count = colors.Length;
                for (int i = 0; i < count; i++)
                {
                    Color color = colors[i];
                    if (color.R == 0 &&
                        color.G == 0 &&
                        color.B == 0 &&
                        color.A == 0)
                    {

                    }
                    else
                    {
                        return color;
                    }
                }
            }

            return new Color(0, 0, 0, 0);
        }

        public static Color Get_HairColor(Item hair)
        {
            if (hair != null)
            {
                Texture2D texture = hair.Texture;

                Color[] colors = new Color[texture.Width * texture.Height];
                texture.GetData(colors);

                int count = colors.Length;
                for (int i = 0; i < count; i++)
                {
                    Color color = colors[i];
                    if (color.R == 0 &&
                        color.G == 0 &&
                        color.B == 0 &&
                        color.A == 0)
                    {

                    }
                    else
                    {
                        return color;
                    }
                }
            }

            return new Color(0, 0, 0, 0);
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
            else if (Utility.RandomPercent(5))
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

                character.Texture = AssetManager.Textures[body_texture];
                if (character.Texture != null)
                {
                    Item armor = InventoryUtil.Get_EquippedItem(character, "Armor");
                    if (armor != null)
                    {
                        string armor_texture = direction + "_Armor_" + armor.Categories[0] + "_" + armor.Materials[0] + "_" + type;
                        armor.Texture = AssetManager.Textures[armor_texture];
                    }

                    Item weapon = InventoryUtil.Get_EquippedItem(character, "Weapon");
                    if (weapon != null)
                    {
                        string weapon_texture = direction + "_Weapon_" + weapon.Categories[0] + "_" + weapon.Materials[0] + "_" + type;
                        weapon.Texture = AssetManager.Textures[weapon_texture];
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
