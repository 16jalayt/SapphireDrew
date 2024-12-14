using Sapphire_Extract_Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;

namespace CIFExtract
{
    //CIF stands for: Compressed Information Format
    internal static class CIF
    {
        public static bool dontdec = false;
        public static bool keepcif = false;

        //Variables should be unchanged for rest of cif. Will be set on first run. Program should only run with one cif file.
        private static Boolean Venice = false;

        private static int nameLength = -1;
        private static Boolean Untested = true;
        private static bool older = false;

        private static short verMajor = -1;
        private static short verMinor = -1;

        public static void Extract(BetterBinaryReader InStream)
        {
            string header = Helpers.String(InStream.ReadBytes(24)).Trim('\0');
            //Console.WriteLine($"header: {header}");

            //If the file has wrong id, say we can't extract
            //CIF files and trees are same structure, just one or many files.
            if (header != "CIF FILE HerInteractive" && header != "CIF TREE HerInteractive" && header != "CIF TREE WayneSikes" && header != "CIF FILE WayneSikes")
            {
                Console.WriteLine($"The file: '{InStream.FileName}' has an invalid header.");
                return;
            }

            verMajor = InStream.ReadShort();
            verMinor = InStream.ReadShort();
            Console.WriteLine($"CIFF version: {verMajor}.{verMinor}\n");

            //Seek past header and version. Already validated.
            InStream.Seek(28);

            ////////////////////////////
            if (verMajor == 3)
            {
                if (!OperatingSystem.IsWindows())
                {
                    Console.WriteLine("V3 (#19 Venice and later) only supported on Windows due to dependency on luac.exe. Exiting...");
                    return;
                }

                //Only way to tell if tree or chunk is to check for a first chunk header.
                string header2 = Helpers.String(InStream.ReadBytes(24)).Trim('\0');
                if (header2 != "CIF FILE HerInteractive")
                {
                    //Must be a loose cif file. Have to get name from filename and is 0 offset in file.
                    CIFObject cif = new CIFObject();
                    cif.fileName = Path.GetFileNameWithoutExtension(InStream.FileName);

                    ParseChunk(InStream, verMajor, verMinor, cif);
                    return;
                }
            }
            else if (header == "CIF FILE WayneSikes" | header == "CIF FILE HerInteractive")
            {
                CIFObject cif = new CIFObject();
                cif.fileName = InStream.FileName;
                cif.filePointer = InStream.Position();
                ciffile(InStream, cif);
                //ParseChunk(InStream, verMajor, verMinor, cif);
                return;
            }
            //else must be a tree

            //////////////////////////////

            //seek to start of name table
            if (verMajor == 3)
            {
                //Name table is at end of file.
                //Skip to last byte in file.
                InStream.Seek(InStream.Length() - 4);
                //Last int in file is the pointer to the start of the name table.
                int start = InStream.ReadInt();
                //seek backwards by previous int, adding in the pointer integer length
                InStream.Seek(InStream.Length() - (4 + start));
            }

            //Number of name table entries
            int count = InStream.ReadShort();
            //only nonzero in cal
            _ = InStream.ReadShort();

            Console.WriteLine($"Extracting '{count}' files.");

            //ver 2 - 2.3
            if (verMajor != 3)
            {
                //2k see CIFPack hash for explaination.
                //table to speed up array access
                InStream.Skip(2048);

                //debug print table
                /*for (int i = 0; i < 1024; i++)
                {
                    Console.WriteLine($"{i}:{InStream.ReadShort()}");
                }*/
            }

            //TODO: possible efficiency increase if parse to list and read files sequentially

            //Technically saving pos here is unnessesary, just needs to be defined out of loop
            long placeholder = InStream.Position();

            //Not sure why need
            if (verMajor == 2 && verMinor == 0)
                placeholder -= 2;

            //LONG section doing tests for name length, as can change randomly
            nameLength = -1;
            getNameLength(InStream);

            if (nameLength == -1)
            {
                Console.WriteLine($"Unknown game version. Aborting...");
                return;
            }

            //Done testing name length. Now parse name table.
            for (int i = 0; i < count; i++)
            {
                CIFObject cif = new CIFObject();

                InStream.Seek(placeholder);
                cif.fileName = Helpers.String(InStream.ReadBytes(nameLength)).TrimEnd('\0');
                if (verMajor == 3)
                {
                    cif.filePointer = InStream.ReadInt();
                    placeholder = InStream.Position();
                    ParseChunk(InStream, verMajor, verMinor, cif);
                }
                //3 only has name and pointer
                else if (verMajor != 3)
                {
                    cif.FileIndex = InStream.ReadShort();
                    Console.WriteLine("idx" + InStream.Position());

                    if (verMajor == 2 && verMinor == 0)
                    {
                        cif.width = InStream.ReadShort();
                        //Unknown
                        int unknownw = InStream.ReadShort();
                        cif.height = InStream.ReadShort();
                        //Unknown
                        int unknownh = InStream.ReadShort();
                        Console.WriteLine("h" + InStream.Position());
                    }
                    else
                    {
                        //attempt to read offset. 0 man, off new
                        cif.filePointer = InStream.ReadInt();

                        //set to variable to debug
                        int unknown1;
                        //bool older = false;
                        //if early 2.1
                        //TODO: Older is wrong?
                        if (cif.filePointer != 0)
                        {
                            //Unknown FF
                            unknown1 = InStream.ReadShort();

                            InStream.Skip(8);
                        }
                        else
                        {
                            unknown1 = InStream.ReadInt();
                            older = true;
                        }

                        //For TGA header
                        cif.XOrigin = InStream.ReadInt();
                        cif.YOrigin = InStream.ReadInt();

                        cif.XStart = InStream.ReadInt();
                        cif.YStart = InStream.ReadInt();
                        cif.XEnd = InStream.ReadInt();
                        cif.YEnd = InStream.ReadInt();

                        cif.width = InStream.ReadShort();
                        //Unknown
                        int unknownw = InStream.ReadShort();
                        cif.height = InStream.ReadShort();
                        //Unknown
                        int unknownh = InStream.ReadShort();
                    }

                    //If early 2.1
                    if (cif.filePointer == 0)
                        cif.filePointer = InStream.ReadInt();

                    //Length of final file
                    cif.DecompressedLength = InStream.ReadInt();
                    //Unknown
                    int unknownlengths = InStream.ReadInt();
                    //Length of file in tree
                    cif.CompressedLength = InStream.ReadInt();
                    Console.WriteLine("type" + InStream.Position());
                    //Is file data or picture
                    cif.FileType = InStream.ReadByte();

                    //Byte align?
                    if (older || (verMajor == 2 && verMinor == 0))
                        _ = InStream.ReadShort();

                    placeholder = InStream.Position();
                    //For !v3 return data and add tga header. Pain to pass, or make object
                    //in v2 and 2.1 there are a few dummy files
                    if (cif.CompressedLength != 0)
                        ParseChunk(InStream, verMajor, verMinor, cif);
                }
            }
        }

        private static void DecompressChunk(CIFObject cif)
        {
            //broken
            //cif.contents = Encoding.Default.GetBytes(Decompress_LZSS(cif.compressed.ToString(), 4, 4));

            //sort of
            //cif.contents = Decompress(cif.compressed);

            //working
            cif.contents = Compression.decompressLZSS(cif.compressed);
        }

        private static void ParseChunk(BetterBinaryReader InStream, short verMajor, short verMinor, CIFObject cif)
        {
            InStream.Seek(cif.filePointer);

            if (keepcif)
            {
                cif.fileExtension = GetExtension(InStream, cif);
                string cifName = cif.fileName + cif.fileExtension;
                Console.WriteLine($"Extracting: {cifName}");

                long begining = InStream.Position();
                InStream.Skip(0x2c);
                cif.DecompressedLength = InStream.ReadInt();
                InStream.Seek(begining);

                cif.contents = InStream.ReadBytes(cif.DecompressedLength);

                //TODO: put raw if v2
                Helpers.Write(InStream.FilePath, cifName, cif.contents);
                return;
            }

            if (verMajor == 3)
            {
                //CIF FILE HerInteractive
                //InStream.Skip(24);
                Helpers.AssertString(InStream, "CIF FILE HerInteractive");
                //skip null term
                InStream.Skip(1);

                //Should be version 3 if ciftree is also 3.
                Helpers.AssertInt(InStream, 3);

                cif.FileType = (byte)InStream.ReadInt();
                cif.fileExtension = GetExtension(InStream, cif);

                //null padded
                //InStream.Skip(12);
                cif.width = InStream.ReadInt();
                cif.height = InStream.ReadInt();

                //1 for ovl and 0 for data?
                //Helpers.AssertInt(InStream, 1);
                int type = InStream.ReadInt();

                cif.DecompressedLength = InStream.ReadInt();
                //cif.CompressedLength = cif.DecompressedLength;

                cif.contents = InStream.ReadBytes(cif.DecompressedLength);

                //Convert the green to alpha
                //TODO: find faster way
                //TODO: command line option to enable/ make seperate func
                if (cif.fileExtension == ".png")
                {
                    cif.contents = ConvertImage(cif.contents, true);
                }
            }
            else
            {
                //cif.contents = InStream.ReadBytes(cif.CompressedLength);
                cif.compressed = InStream.ReadBytes(cif.CompressedLength);
                for (int j = 0; j < cif.CompressedLength; j++)
                    cif.compressed[j] = (byte)(cif.compressed[j] - j);
                DecompressChunk(cif);
                //cif.contents = cif.compressed;

                //IMPORTANT:for v2 must be called after chunk is red and decompressed
                cif.fileExtension = GetExtension(InStream, cif);

                if (cif.fileExtension == ".tga")
                {
                    if (verMajor == 2 && verMinor < 3)
                    {
                        //Uncomment to test broken tga
                        /*if (verMajor == 2 && verMinor == 2)
                        {
                            var image = SixLabors.ImageSharp.Image.LoadPixelData<Bgr24>(cif.contents, cif.width, cif.height);
                            image.Save($"{InStream.FileNameWithoutExtension}\\{cif.fileName}.bmp");
                        }
                        else
                        {
                            var image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra5551>(cif.contents, cif.width, cif.height);
                            image.Save($"{InStream.FileNameWithoutExtension}\\{cif.fileName}.bmp");
                        }*/

                        MemoryStream stream = new MemoryStream();
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            //only type of "Uncompressed, RGB image"
                            writer.Write(new byte[] { 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00 });
                            writer.Write((short)cif.XOrigin);
                            writer.Write((short)cif.YOrigin);
                            writer.Write((short)cif.width);
                            writer.Write((short)cif.height);

                            if (verMajor == 2 && verMinor == 2)
                                //24bpp
                                writer.Write((byte)0x18);
                            else
                                //16bpp
                                writer.Write((byte)0x10);

                            //one attribute bit. does? without it flips upsidedown
                            writer.Write((byte)0x20);
                            //writer.Write((byte)0b00100001);
                            if (cif.contents != null)
                                writer.Write(cif.contents);
                            else
                                Console.WriteLine("The contents of this CIF are empty.");
                        }
                        cif.contents = stream.ToArray();
                    }

                    //cif.contents are still tga, but imagesharp doesn't care and we still need to do alpha conversion anyway
                    cif.fileExtension = ".png";
                    cif.contents = ConvertImage(cif.contents, true);
                }
            }

            Console.WriteLine($"Extracting: {cif.fileName}{cif.fileExtension}");

            string outName = cif.fileName + cif.fileExtension;
            string outRaw = "";
            if (cif.contents != null)
            {
                //If cif file outside of tree
                if (cif.filePointer == 0)
                    outRaw = Helpers.Write(InStream.FilePath, outName, cif.contents, false);
                else
                    outRaw = Helpers.Write(InStream.FilePath, outName, cif.contents);
            }
            else
                Console.WriteLine("The contents of this CIF are empty.");

            //LUAC
            if (cif.fileExtension == ".luac" && !dontdec)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = "unluac.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //put quotes around path in argument
                startInfo.Arguments = $"\"{outRaw}\"";

                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using Process? process = Process.Start(startInfo);
                    if (process != null)
                    {
                        var outputStream = new StreamWriter(Path.Combine(Path.GetDirectoryName(outRaw)!, cif.fileName + ".lua"));
                        //File.WriteLine("test.txt", exeProcess.StandardOutput.ReadToEnd());
                        process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            if (!String.IsNullOrEmpty(e.Data))
                            {
                                outputStream.WriteLine(e.Data);
                            }
                        });

                        process.Start();

                        process.BeginOutputReadLine();

                        process.WaitForExit();
                        process.Close();

                        outputStream.Close();

                        File.Delete(outRaw);
                    }
                    else
                        Console.WriteLine("Unable to start unluac process.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to launch unluac:\n" + e);
                }
            }
        }

        private static byte[]? ConvertImage(byte[]? imageData, bool keyAlpha = true)
        {
            using Image<Rgba32> image = Image.Load<Rgba32>(imageData);
            using var stream = new MemoryStream();

            if (keyAlpha)
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                        foreach (ref Rgba32 pixel in pixelRow)
                        {
                            if (pixel.Rgb.Equals(Color.FromRgb(0, 255, 0)))
                                pixel = Color.Transparent;
                        }
                    }
                });
            }

            image.SaveAsPng(stream);
            return stream.ToArray();
        }

        private static string GetExtension(BetterBinaryReader InStream, CIFObject cif)
        {
            /*
		 * Chunk types are officially documented as:
		 * PLAIN --  image that IS NOT used as a transparent overlay
	     * DECAL --  image that IS used as a transparent overlay
	     * DATA  --  non-image data such as the Cif Listing file.
		 */

            if (keepcif)
                return ".cif";

            //2 is an OVL file or overlay. A sprite that goes on top of a scene. Usually a pickup or interactable.
            if (cif.FileType == 2)
            {
                if (verMajor == 3)
                    //For png there are x and y size. everything else 0. This is presumably an engine convenience
                    return ".png";
                else
                    return ".tga";
            }
            else if (cif.FileType == 3)
            {
                if (verMajor == 3)
                {
                    if (Untested)
                    {
                        long Pos = InStream.Position();
                        InStream.Skip(16);

                        if (Helpers.String(InStream.ReadBytes(4)) == "DATA")
                            Venice = true;
                        Untested = false;
                        InStream.Seek(Pos);
                    }

                    if (Venice)
                        return ".hiff";
                    else
                        return ".luac";
                }
                else
                {
                    if (cif.contents != null)
                    {
                        if (cif.contents[0] == 88)
                            return ".xs1";
                        //dat script file
                        else if (cif.contents[0] == 68)
                            return ".hiff";
                    }
                    return ".unk";
                }
            }
            //ERRATA: extension is officially .xs1. I have modified this to differentiate compiled vs uncompiled.
            //The XSheetDecompile will name the resultant file .xsheet
            //type 6 is only in v3
            else if (cif.FileType == 6)
                return ".xs1";
            else if (cif.FileType == 4)
            {
                if (cif.fileName == ".")
                    return "";
                else
                    return ".unk";
            }
            else
            {
                Console.WriteLine($"Unknown file type {cif.FileType}. Please report game this occured in.");
                return ".unk";
            }
        }

        private static void getNameLength(BetterBinaryReader InStream)
        {
            if (verMajor == 2 && verMinor == 0)
                nameLength = 9;
            else if (verMajor == 2 && verMinor == 1)
            {
                //Name length was different in game 2
                //Seek to name field about 3 files in to see if empty or a number
                InStream.Skip(149);
                byte test = InStream.ReadByte();
                //if any game but dan
                if (test == 0)
                    nameLength = 33;
                //else dan
                else
                    nameLength = 9;
                //Redundant. set in loop. Here for clarity
                //InStream.Seek(placeholder);
            }
            else if (verMajor == 2 && verMinor == 2)
                nameLength = 33;
            else if (verMajor == 2 && verMinor == 3)
                nameLength = 33;
            else if (verMajor == 3)
            {
                //Check for venice. Seek to end of normal name to see if 0, if not then longer name.
                //The most notable change between cif revisions is name field length. The other being graphics format.
                InStream.Skip(69);//nice
                                  //val should either ber first char of next name or last char of next name (venice. last char should be blank).
                byte test = InStream.ReadByte();
                if (test == 0)
                    nameLength = 33;
                else
                    nameLength = 64;

                //Redundant. set in loop. Here for clarity
                //InStream.Seek(placeholder);
            }
        }

        private static void ciffile(BetterBinaryReader InStream, CIFObject cif)
        {
            //TODO: sck
            //cif.FileIndex = InStream.ReadShort();

            //attempt to read offset. 0 man, off new
            //cif.filePointer = InStream.ReadInt();

            //set to variable to debug
            int unknown1;
            //bool older = false;
            //if early 2.1
            //TODO: Older is wrong?
            if (cif.filePointer != 0)
            {
                //Unknown FF
                //_ = InStream.ReadShort();

                InStream.Skip(8);
                //older = true;
            }
            else
                unknown1 = InStream.ReadInt();

            //For TGA header
            cif.XOrigin = InStream.ReadInt();
            cif.YOrigin = InStream.ReadInt();

            cif.XStart = InStream.ReadInt();
            cif.YStart = InStream.ReadInt();
            cif.XEnd = InStream.ReadInt();
            cif.YEnd = InStream.ReadInt();

            cif.width = InStream.ReadShort();
            //Unknown
            int unknownw = InStream.ReadShort();
            cif.height = InStream.ReadShort();
            //Unknown
            int unknownh = InStream.ReadShort();

            //If early 2.1 or cif
            if (cif.filePointer == 0)
                cif.filePointer = InStream.ReadInt();

            //Length of final file
            cif.DecompressedLength = InStream.ReadInt();
            //Unknown
            int unknownlengths = InStream.ReadInt();
            //Length of file in tree
            cif.CompressedLength = InStream.ReadInt();
            //Is file data or picture
            cif.FileType = InStream.ReadByte();

            //Unknown
            //if (older)
            //_ = InStream.ReadShort();
            cif.filePointer = InStream.Position();
            //placeholder = InStream.Position();
            //For !v3 return data and add tga header. Pain to pass, or make object
            //in v2 and 2.1 there are a few dummy files
            if (cif.CompressedLength != 0)
                ParseChunk(InStream, verMajor, verMinor, cif);
        }
    }
}