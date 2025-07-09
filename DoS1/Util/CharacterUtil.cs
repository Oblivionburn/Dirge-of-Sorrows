using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Controls;
using OP_Engine.Inventories;
using OP_Engine.Characters;
using OP_Engine.Utility;

namespace DoS1.Util
{
    public static class CharacterUtil
    {
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
    }
}
