using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class Alpha
    {
        public static Texture2D PremultiplyAlpha(this Texture2D texture)
        {
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                var c = data[i];
                // Color in XNA/FNA stores bytes 0–255
                // Premultiply: rgb = rgb * (a / 255)
                float a = c.A / 255f;
                c.R = (byte)(c.R * a);
                c.G = (byte)(c.G * a);
                c.B = (byte)(c.B * a);
                data[i] = c;
            }

            texture.SetData(data);
            return texture;
        }

        public static Texture2D ScaleAlpha(this Texture2D texture, float factor, byte zeroThreshold = (byte)0, byte oneThreshold = (byte)255)
        {
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                var c = data[i];
                c.A = (byte)(c.A * factor);
                if (c.A < zeroThreshold) c.A = (byte)0;
                if (c.A > oneThreshold) c.A = (byte)255;
                data[i] = c;
            }

            texture.SetData(data);
            return texture;
        }

    }
}
