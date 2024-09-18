using Sapphire_Extract_Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CIFPack
{
    internal static class V3
    {
        private class NameTableEntry
        {
            public string name;
            public long pos;
        }

        public static void generateCIFTree(string directory, int gamenum)
        {
            //Abort if subfolder
            if (Directory.GetDirectories(directory).Length > 0)
            {
                Console.WriteLine($"Fatal Error: The directory '{directory}' contians subfolders.\nCiftrees do not support subfolders.");
                System.Environment.Exit(-4);
            }

            string[] fileEntries = Directory.GetFiles(directory);
            if (fileEntries.Length == 0)
            {
                Console.WriteLine($"Fatal Error: The directory '{directory}' is empty.");
                System.Environment.Exit(-5);
            }

            ///// CIF Tree header

            BinaryWriter OutStream = new BinaryWriter(new FileStream("Patch.dat", FileMode.Create));

            //magic
            OutStream.Write(Encoding.UTF8.GetBytes("CIF FILE HerInteractive\0"));
            //version
            OutStream.Write((short)3);
            OutStream.Write((short)0);

            ////

            List<NameTableEntry> TreeMeta = new List<NameTableEntry>();

            int namelength;
            if (gamenum == 18 || gamenum == 19)
                namelength = 0x21;
            else
                namelength = 0x40;

            foreach (string pathName in fileEntries)
            {
                string fileName = Path.GetFileNameWithoutExtension(pathName);
                if (fileName.Length > namelength)
                {
                    Console.WriteLine($"Fatal Error: The file name '{fileName}' is too long. This cannot happen.");
                    System.Environment.Exit(-6);
                }

                long offset = OutStream.BaseStream.Position;

                Stream? MemStream = generateCIFChunk(pathName, gamenum);
                if (MemStream != null)
                {
                    MemStream.Seek(0, SeekOrigin.Begin);
                    MemStream.CopyTo(OutStream.BaseStream);
                    //OutStream.Write(MemStream);

                    TreeMeta.Add(new NameTableEntry { name = fileName, pos = offset });
                    MemStream.Close();
                }
                else
                    Console.WriteLine($"GenerateCIFChunk returned null.");
            }

            OutStream.Write((int)TreeMeta.Count);
            long tableStart = OutStream.BaseStream.Position;

            //write name table
            foreach (NameTableEntry entry in TreeMeta)
            {
                OutStream.Write(Encoding.UTF8.GetBytes(entry.name.PadRight(namelength, '\0')));
                OutStream.Write((int)entry.pos);
            }

            OutStream.Write((int)(OutStream.BaseStream.Position - tableStart + 4));

            OutStream.Close();
        }

        public static void generateCIFFile(string fileName, int gamenum)
        {
            Stream? MemStream = generateCIFChunk(fileName, gamenum);
            FileStream FileStream = new FileStream(@Path.GetFileNameWithoutExtension(fileName) + ".cif", FileMode.Create);
            if (MemStream != null)
            {
                MemStream.Seek(0, SeekOrigin.Begin);
                MemStream.CopyTo(FileStream);

                MemStream.Close();
                FileStream.Close();
            }
            else
                Console.WriteLine($"GenerateCIFChunk returned null.");
        }

        public static Stream? generateCIFChunk(string fileName, int gamenum)
        {
            BetterBinaryReader InStream = new BetterBinaryReader(fileName);
            //BinaryWriter OutStream = new BinaryWriter(new FileStream(@Path.GetFileNameWithoutExtension(FileName) + ".cif", FileMode.Create));
            BinaryWriter OutStream = new BinaryWriter(new MemoryStream((int)InStream.Length() + 38));

            if (@Path.GetExtension(fileName) == ".cif")
            {
                byte[] cifContents = InStream.ReadBytes((int)InStream.Length());
                OutStream.Write(cifContents);

                return OutStream.BaseStream;
            }

            //magic
            OutStream.Write(Encoding.UTF8.GetBytes("CIF FILE HerInteractive\0"));
            //version
            OutStream.Write((short)3);
            OutStream.Write((short)0);

            //TODO: Either convert to png or tell user only png
            if (InStream.FileExtension == ".png")
            {
                //Type of file
                OutStream.Write((int)2);

                //Need image width and height
                //https://stackoverflow.com/questions/60857830/finding-png-image-width-height-via-file-metadata-net-core-3-1-c-sharp
                //Getting directly, Could also use: https://github.com/CodeRanger-com/Coderanger.ImageInfo/
                InStream.Seek(16);
                byte[] widthbytes = new byte[sizeof(int)];
                for (int i = 0; i < sizeof(int); i++) widthbytes[sizeof(int) - 1 - i] = InStream.ReadByte();
                int width = BitConverter.ToInt32(widthbytes, 0);
                byte[] heightbytes = new byte[sizeof(int)];
                for (int i = 0; i < sizeof(int); i++) heightbytes[sizeof(int) - 1 - i] = InStream.ReadByte();
                int height = BitConverter.ToInt32(heightbytes, 0);

                //Reading the image leaves stream at end of file
                InStream.Seek(0);
                OutStream.Write((int)width);
                OutStream.Write((int)height);

                /*Image img = Image.FromStream(InStream.GetStream());
                //Reading the image leaves stream at end of file
                InStream.Seek(0);
                OutStream.Write((int)img.Width);
                OutStream.Write((int)img.Height);*/

                //1?
                OutStream.Write((int)1);
            }
            else if (InStream.FileExtension == ".hiff")
            {
                //Note: ntdl.cif seems nonstandrd in ven
                if (gamenum != 18)
                {
                    Console.WriteLine("VEN is the only CIF V3 game that accepts .hiff files.");
                    return null;
                }
                //Type of file
                OutStream.Write((int)3);

                //Values used only for OVL
                OutStream.Write(new byte[12]);
                //Contents written below
            }
            else if (InStream.FileExtension == ".luac")
            {
                //Already compiled lua. Just insert

                //Type of file
                OutStream.Write((int)3);

                //Values used only for OVL
                OutStream.Write(new byte[12]);
                //Contents written below
            }
            else if (InStream.FileExtension == ".lua")
            {
                //Game will runtime error if plain lua is encapsulated.
                //Needs to be compiled first or run loose
                //Console.WriteLine(".lua files are currently not supported. The game will run them fine loose.");

                if (gamenum == 18)
                {
                    //todo:ven
                    Console.WriteLine("VEN only accepts .hiff files not .lua.");
                    return null;
                }

                string luaName = InStream.FilePath;
                string luacName = $"{InStream.FilePath}c";

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = "luac5.1.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //put quotes around path in argument
                startInfo.Arguments = $"-o  {luacName} {luaName}";

                try
                {
                    // Start the process with the info we specified.
                    // Call WaitForExit and then the using statement will close.
                    using Process? process = Process.Start(startInfo);
                    if (process == null)
                    {
                        Console.WriteLine("Unable to start unluac process.");
                        return null;
                    }
                    process.Start();
                    process.WaitForExit();
                    process.Close();

                    //Delete the lua file and reopen instraeam with luac file
                    InStream.Dispose();
                    //File.Delete(luaName);
                    InStream = new BetterBinaryReader(luacName);

                    //Type of file
                    OutStream.Write((int)3);

                    //Values used only for OVL
                    OutStream.Write(new byte[12]);

                    OutStream.Write((int)InStream.Length());
                    //int len = (int)InStream.Length();
                    byte[] luacContents = InStream.ReadBytes((int)InStream.Length());
                    OutStream.Write(luacContents);

                    //OutStream.Close();
                    InStream.Dispose();
                    //Delete temp compiled file
                    File.Delete(luacName);
                    //bypass other file writing
                    return OutStream.BaseStream;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to launch unluac:\n" + e);
                    return null;
                }
            }

            OutStream.Write((int)InStream.Length());
            //int len = (int)InStream.Length();
            byte[] contents = InStream.ReadBytes((int)InStream.Length());
            OutStream.Write(contents);

            //OutStream.Close();
            return OutStream.BaseStream;
        }
    }
}