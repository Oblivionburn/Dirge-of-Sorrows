using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OP_Engine.Controls;
using OP_Engine.Utility;
using System;

namespace DoS1.Util
{
    public static class ShaderUtil
    {
        public static Vector2[] HorizontalOffsets(int radius, float textureWidth)
        {
            Vector2[] offsets = new Vector2[radius * 2 + 1];

            int index;
            float xOffset = 1.0f / textureWidth;

            for (int i = -radius; i <= radius; ++i)
            {
                index = i + radius;
                offsets[index] = new Vector2(i * xOffset, 0.0f);
            }

            return offsets;
        }

        public static Vector2[] VerticalOffsets(int radius, float textureHeight)
        {
            Vector2[] offsets = new Vector2[radius * 2 + 1];

            int index;
            float yOffset = 1.0f / textureHeight;

            for (int i = -radius; i <= radius; ++i)
            {
                index = i + radius;
                offsets[index] = new Vector2(0.0f, i * yOffset);
            }

            return offsets;
        }

        public static float[] Kernel(int radius, float amount)
        {
            float[] kernel = new float[radius * 2 + 1];
            float sigma = radius / amount;

            float twoSigmaSquare = 2.0f * sigma * sigma;
            float sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
            float total = 0.0f;
            float distance;
            int index;

            for (int i = -radius; i <= radius; ++i)
            {
                distance = i * i;
                index = i + radius;
                kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                total += kernel[index];
            }

            for (int i = 0; i < kernel.Length; ++i)
            {
                kernel[i] /= total;
            }

            return kernel;
        }

        public static void Apply_GaussianBlur(SpriteBatch spriteBatch, int radius, Texture2D texture, Region region, Rectangle image, Color drawColor, float opacity)
        {
            Main.Game.SpriteBatch.End();

            Effect GaussianBlur = AssetManager.Shaders["GaussianBlur"];
            GaussianBlur.Parameters["SampleWeights"].SetValue(Kernel(radius, 2.0f));

            Main.Game.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, GaussianBlur);
            spriteBatch.Draw(texture, region.ToRectangle, image, drawColor * opacity);

            GaussianBlur.Parameters["Texture1"].SetValue(texture);

            GaussianBlur.Parameters["SampleOffsets"].SetValue(HorizontalOffsets(radius, texture.Width));
            GaussianBlur.CurrentTechnique.Passes[0].Apply();

            GaussianBlur.Parameters["SampleOffsets"].SetValue(VerticalOffsets(radius, texture.Height));
            GaussianBlur.CurrentTechnique.Passes[0].Apply();

            Main.Game.SpriteBatch.End();
            Main.Game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
        }
    }
}
