using System;

namespace VrSharp.GvrTexture
{
    public abstract class GvrPixelCodec : VrPixelCodec
    {
        #region Intensity 8-bit with Alpha
        // Intensity 8-bit with Alpha
        public class IntensityA8 : GvrPixelCodec
        {
            public override bool CanEncode
            {
                get { return true; }
            }

            public override int Bpp
            {
                get { return 16; }
            }

            public override void DecodePixel(byte[] source, int sourceIndex, byte[] destination, int destinationIndex)
            {
                destination[destinationIndex + 3] = source[sourceIndex];
                destination[destinationIndex + 2] = source[sourceIndex + 1];
                destination[destinationIndex + 1] = source[sourceIndex + 1];
                destination[destinationIndex + 0] = source[sourceIndex + 1];
            }

            public override void EncodePixel(byte[] source, int sourceIndex, byte[] destination, int destinationIndex)
            {
                destination[destinationIndex + 0] = source[sourceIndex + 3];
                destination[destinationIndex + 1] = (byte)((0.30 * source[sourceIndex + 2]) + (0.59 * source[sourceIndex + 1]) + (0.11 * source[sourceIndex + 0]));
            }
        }
        #endregion

        #region Rgb565
        // Rgb565
        public class Rgb565 : GvrPixelCodec
        {
            public override bool CanEncode
            {
                get { return true; }
            }

            public override int Bpp
            {
                get { return 16; }
            }

            public override void DecodePixel(byte[] source, int sourceIndex, byte[] destination, int destinationIndex)
            {
                ushort pixel = PTMethods.ToUInt16BE(source, sourceIndex);
                byte r = (byte)((pixel >> 11) & 0x1F);
                byte g = (byte)((pixel >>  5) & 0x3F);
                byte b = (byte)((pixel >>  0) & 0x1F);

                destination[destinationIndex + 3] = 0xFF;
                destination[destinationIndex + 2] = (byte)((r << 3) | (r >> 2));
                destination[destinationIndex + 1] = (byte)((g << 2) | (g >> 4));
                destination[destinationIndex + 0] = (byte)((b << 3) | (b >> 2));
            }

            public override void EncodePixel(byte[] source, int sourceIndex, byte[] destination, int destinationIndex)
            {
                ushort pixel = 0x0000;
                pixel |= (ushort)((source[sourceIndex + 2] >> 3) << 11);
                pixel |= (ushort)((source[sourceIndex + 1] >> 2) << 5);
                pixel |= (ushort)((source[sourceIndex + 0] >> 3) << 0);

                destination[destinationIndex + 1] = (byte)(pixel & 0xFF);
                destination[destinationIndex + 0] = (byte)((pixel >> 8) & 0xFF);
            }
        }
        #endregion

        #region Rgb5a3
        // Rgb5a3
        public class Rgb5a3 : GvrPixelCodec
        {
            public override bool CanEncode
            {
                get { return true; }
            }

            public override int Bpp
            {
                get { return 16; }
            }

            public override void DecodePixel(byte[] source, int sourceIndex, byte[] destination, int destinationIndex)
            {
                ushort pixel = PTMethods.ToUInt16BE(source, sourceIndex);

                if ((pixel & 0x8000) != 0) // Rgb555
                {
                    byte r = (byte)((pixel >> 10) & 0x1F);
                    byte g = (byte)((pixel >> 5) & 0x1F);
                    byte b = (byte)((pixel >> 0) & 0x1F);
                    destination[destinationIndex + 3] = 0xFF;
                    destination[destinationIndex + 2] = (byte)((r << 3) | (r >> 2));
                    destination[destinationIndex + 1] = (byte)((g << 3) | (g >> 2));
                    destination[destinationIndex + 0] = (byte)((b << 3) | (b >> 2));
                }
                else // Argb3444
                {
                    byte a = (byte)((pixel >> 12) & 0x07);
                    byte r = (byte)((pixel >> 8) & 0x0F);
                    byte g = (byte)((pixel >> 4) & 0x0F);
                    byte b = (byte)((pixel >> 0) & 0x0F);
                    destination[destinationIndex + 3] = (byte)((a << 5) | (a << 2) | (a >> 1));
                    destination[destinationIndex + 2] = (byte)((r << 4) | r);
                    destination[destinationIndex + 1] = (byte)((g << 4) | g);
                    destination[destinationIndex + 0] = (byte)((b << 4) | b);
                }
            }

            public override void EncodePixel(byte[] source, int sourceIndex, byte[] destination, int destinationIndex)
            {
                ushort pixel = 0x0000;

                if (source[sourceIndex + 3] <= 0xDA) // Argb3444
                {
                    pixel |= (ushort)((source[sourceIndex + 3] >> 5) << 12);
                    pixel |= (ushort)((source[sourceIndex + 2] >> 4) << 8);
                    pixel |= (ushort)((source[sourceIndex + 1] >> 4) << 4);
                    pixel |= (ushort)((source[sourceIndex + 0] >> 4) << 0);
                }
                else // Rgb555
                {
                    pixel |= 0x8000;
                    pixel |= (ushort)((source[sourceIndex + 2] >> 3) << 10);
                    pixel |= (ushort)((source[sourceIndex + 1] >> 3) << 5);
                    pixel |= (ushort)((source[sourceIndex + 0] >> 3) << 0);
                }

                destination[destinationIndex + 1] = (byte)(pixel & 0xFF);
                destination[destinationIndex + 0] = (byte)((pixel >> 8) & 0xFF);
            }
        }
        #endregion

        #region Get Codec
        public static GvrPixelCodec GetPixelCodec(GvrPixelFormat format)
        {
            switch (format)
            {
                case GvrPixelFormat.IntensityA8:
                    return new IntensityA8();
                case GvrPixelFormat.Rgb565:
                    return new Rgb565();
                case GvrPixelFormat.Rgb5a3:
                    return new Rgb5a3();
            }

            return null;
        }
        #endregion
    }
}