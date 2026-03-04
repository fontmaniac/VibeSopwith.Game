using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class MipMap
    {
        public static void Generate(Texture2D texture, Color[] baseData, int width, int height)
        {
            int level = 1;
            while (width > 1 && height > 1)
            {
                int newWidth = Math.Max(1, width / 2);
                int newHeight = Math.Max(1, height / 2);
                Color[] mipData = new Color[newWidth * newHeight];

                // Simple box filter (average 4 pixels)
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        int i = (y * 2 * width) + (x * 2);
                        Color c1 = baseData[i];
                        Color c2 = baseData[i + 1];
                        Color c3 = baseData[i + width];
                        Color c4 = baseData[i + width + 1];
                        mipData[y * newWidth + x] = AverageColors(c1, c2, c3, c4);
                    }
                }

                texture.SetData(level, null, mipData, 0, mipData.Length);
                baseData = mipData;
                width = newWidth;
                height = newHeight;
                level++;
            }
        }

        public static Color AverageColors(Color c1, Color c2, Color c3, Color c4)
        {
            return new Color(
                (c1.R + c2.R + c3.R + c4.R) / 4,
                (c1.G + c2.G + c3.G + c4.G) / 4,
                (c1.B + c2.B + c3.B + c4.B) / 4,
                (c1.A + c2.A + c3.A + c4.A) / 4
            );
        }

        public static Texture2D Cast(GraphicsDevice gd, SpriteBatch spriteBatch, Texture2D master, Vector2 origin, float rotation, SpriteEffects effects)
        {
            using var renderTarget = new RenderTarget2D(gd, master.Width, master.Height, mipMap: true, SurfaceFormat.Color, DepthFormat.None);
            gd.SetRenderTarget(renderTarget);
            gd.Clear(Color.Transparent);

            spriteBatch.Begin();
            spriteBatch.Draw(master, origin, null, Color.White, rotation, origin, 1.0f, effects, 0f);
            spriteBatch.End();

            gd.SetRenderTarget(null); // Reset to backbuffer

            var baseData = new Color[master.Width * master.Height];
            renderTarget.GetData(baseData);

            var result = new Texture2D(gd, master.Width, master.Height, true, SurfaceFormat.Color);
            result.SetData(0, null, baseData, 0, baseData.Length);

            MipMap.Generate(result, baseData, master.Width, master.Height);
            return result;
        }

        public static Texture2D CastWithMipMaps(GraphicsDevice gd, SpriteBatch spriteBatch, Texture2D master) =>
            Cast(gd, spriteBatch, master, new Vector2(master.Width / 2f, master.Height / 2f), 0f, SpriteEffects.None);
    }
}
