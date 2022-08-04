using Sapphire_Extract_Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace CIFPack
{
    internal static class V3
    {
        public static void generateCIFTree(string Directory, int gamenum)
        {
        }

        public static void generateCIFChunk(string FileName, int gamenum)
        {
            BetterBinaryReader InStream = new BetterBinaryReader(FileName);
            BinaryWriter OutStream = new BinaryWriter(new FileStream(@Path.GetFileNameWithoutExtension(FileName) + ".cif", FileMode.Create));

            //magic
            OutStream.Write(Encoding.UTF8.GetBytes("CIF FILE HerInteractive\0"));
            //version
            OutStream.Write((short)3);
            OutStream.Write((short)0);

            //TODO: get file extension
            if (InStream.FileExtension == ".png")
            {
                //Type of file
                OutStream.Write((int)2);

                Image img = Image.FromStream(InStream.GetStream());
                //Reading the image leaves stream at end of file
                InStream.Seek(0);
                OutStream.Write((int)img.Width);
                OutStream.Write((int)img.Height);

                //1?
                OutStream.Write((int)1);
            }
            else if (InStream.FileExtension == ".hiff")
            {
                //Note: ntdl.cif seems nonstandrd in ven
                if (gamenum != 18)
                {
                    Console.WriteLine("VEN is the only CIF V3 game that accepts .hiff files.");
                    return;
                }
                //Type of file
                OutStream.Write((int)3);

                //Values used only for OVL
                OutStream.Write(new byte[12]);
            }
            else if (InStream.FileExtension == ".lua")
            {
                //Game will runtime error if plain lua is encapsulated.
                //Needs to be compiled first or run loose
                //Console.WriteLine(".lua files are currently not supported. The game will run them fine loose.");

                if (gamenum == 18)
                {
                    Console.WriteLine("VEN only accepts .hiff files not .lua.");
                    return;
                }

                string luaName = InStream.FileName;
                string luacName = $"{InStream.FileNameWithoutExtension}.luac";

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
                    using (Process process = Process.Start(startInfo))
                    {
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

                        OutStream.Close();
                        InStream.Dispose();
                        File.Delete($"{InStream.FileNameWithoutExtension}.luac");
                        //bypass other file writing
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to launch unluac:\n" + e);
                    return;
                }
            }

            OutStream.Write((int)InStream.Length());
            //int len = (int)InStream.Length();
            byte[] contents = InStream.ReadBytes((int)InStream.Length());
            OutStream.Write(contents);

            OutStream.Close();
        }
    }
}