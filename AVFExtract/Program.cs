using Sapphire_Extract_Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace AVFExtract
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Tool status
            //Console.WriteLine("EXPEREMENTAL\n");
            //Console.WriteLine("UNVALIDATED\n");
            Console.WriteLine("CURRENTLY BROKEN\n");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage is AVFExtract.exe filename\n");
                return;
            }

            //TODO: add batch folder support
            string FileName = args[0];

            if (!File.Exists(FileName))
            {
                Console.WriteLine($"The file: '{FileName}' does not exist.\n");
                return;
            }
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);
            /////
            InStream.debugprint = true;

            //If the file has wrong id, say we can't extract
            if (!Helpers.AssertString(InStream, "AVF WayneSikes\0"))
            {
                Console.WriteLine($"The file: '{FileName}' has an invalid header.\n");
                return;
            }

            //Unknown. probably wrong sizing. 6 bytes
            Helpers.AssertInt(InStream, 512);
            Helpers.AssertShort(InStream, 0);

            short numEntries = InStream.ReadShort("numEntries: ");
            short width = InStream.ReadShort("width: ");
            short height = InStream.ReadShort("height: ");

            //Unknown. probably wrong sizing. 6 bytes
            Helpers.AssertInt(InStream, 16912);
            Helpers.AssertShort(InStream, 512);

            for (int i = 0; i < numEntries; i++)
            {
                short frameNo = InStream.ReadShort("\r\rframeNo: ");
                int offset = InStream.ReadInt("offset: ");
                int CompressedLength = InStream.ReadInt("length: ");

                //Unknown
                InStream.Skip(9);

                long placeholder = InStream.Position();
                InStream.Seek(offset);

                byte[] compressed = InStream.ReadBytes(CompressedLength);
                for (int j = 0; j < CompressedLength; j++)
                    compressed[j] = (byte)(compressed[j] - j);

                byte[] outData = Compression.decompressLZSS(compressed);

                //possibly wrong colorspace but simple and same final as old extractor?
                //Thought was rgb565 works as bgra5551
                var image = Image.LoadPixelData<Bgra5551>(outData, width, height);
                //png doesn't work. due to pixelformat? bmp and jpg work
                //Now lower in function
                //image.Save("barbasic.bmp");

                //System.drawing attempt
                /*
                Bitmap pic = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                //fonvert from 555 (565?) to rgb colorspace manually
                int colR;
                int colG;
                int colB;
                int[] r = new int[width * height * 3];
                int[] g = new int[width * height * 3];
                int[] b = new int[width * height * 3];

                for (int j = 0; j < outData.Length; j += 2)
                {
                    colR = (outData[j + 1] & 0x7C) << 1;
                    colG = (outData[j + 1] & 0x03) << 6 | (outData[j] & 0xE0) >> 2;
                    colB = (outData[j] & 0x1F) << 3;

                    colR |= colR >> 5;
                    colG |= colG >> 5;
                    colB |= colB >> 5;

                    r[j] = colR;
                    g[j] = colG;
                    b[j] = colB;
                }

                //manually map pixels from the rgb array to a buffered image
                for (int j = 0; j < width * height; j += 3)
                {
                    pic.SetPixel(j / 3 % width, j / 3 / width, Color.FromArgb(r[j], g[j], b[j]));
                }

                pic.Save("test.png");*/

                //Imagesharp attempt
                /*
                                //fonvert from 555 to rgb colorspace manually
                                int colR;
                                int colG;
                                int colB;
                                //Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                                // List<Int16> outColors = new List<Int16>();
                                Color[] outColors = new Color[outData.Length / 2];

                                for (int j = 0; j < outData.Length; j += 2)
                                {
                                    colR = (outData[j + 1] & 0x7C) << 1;
                                    colG = (outData[j + 1] & 0x03) << 6 | (outData[j] & 0xE0) >> 2;
                                    colB = (outData[j] & 0x1F) << 3;

                                    colR |= colR >> 5;
                                    colG |= colG >> 5;
                                    colB |= colB >> 5;

                                    outColors[j / 2] = Color.FromArgb((byte)colR, (byte)colG, (byte)colB);
                                }

                                var image = Image.LoadPixelData<Rgba32>(outColors, width, height);
                                image.Save("bar.jpg");*/

                //if multiple frames create a subfolder
                if (numEntries > 1)
                {
                    //TODO issue with pwd. should specify path everywhere
                    Directory.CreateDirectory(InStream.FileNameWithoutExtension);
                    image.Save($"{InStream.FileNameWithoutExtension}\\{frameNo}.bmp");
                }
                else
                {
                    image.Save($"{InStream.FileNameWithoutExtension}.bmp");
                }

                InStream.Seek(placeholder);
            }
        }
    }
}