using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OP_Engine.Inventories;
using OP_Engine.Characters;

namespace DoS1.Util
{
    public static class CharacterUtil
    {
        public static void Draw(SpriteBatch spriteBatch, Character character, Color color)
        {
            if (character != null)
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
            }
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
    }
}
